#region [ Using ]
using InovoCIM.Data.Entities;
using InovoCIM.Data.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
#endregion

namespace InovoCIM.Business
{
    public class DataFileTOne
    {
        public readonly LogRepository LogRepo = new LogRepository();
        public readonly EmailRepository MailRepo = new EmailRepository();

        public DataTable AddCalls = new DataTable();
        public DataTable RmvCalls = new DataTable();

        public DataTable PresenceData = new DataTable();
        public DataTable InvalidPhoneData = new DataTable();

        #region [ Master Controller ]
        public async Task<string> MasterController()
        {
            string FileReceived = "No File";
            try
            {
                DateTime start = DateTime.Now;
                LogRepo.Log("\nDataFile T1 Start");

                List<FileInfo> files = GetFileList();
                foreach (FileInfo file in files.OrderBy(x => x.LastWriteTime))
                {
                    await ManageTextFile(file);
                    FileReceived = "Yes - File Received";
                }

                DateTime end = DateTime.Now;
                TimeSpan runtime = (end - start);

                LogRepo.Log("DataFile T1 End : " + runtime.ToString());
                return FileReceived;
            }
            catch (Exception ex)
            {
                LogRepo.Log(ex.Message.ToString());
                await MailRepo.SendError("File Processing Failed", ex.Message.ToString() + " => Please view log for more information");
                return FileReceived;
            }
        }
        #endregion

        //---------------------------------------------------------------------------//

        #region [ Get File List ]
        private List<FileInfo> GetFileList()
        {
            List<FileInfo> files = new List<FileInfo>();
            try
            {
                LogRepo.Log("GetFileList() - Checking FTP Directory => " + ViewModel.FTP);

                DirectoryInfo inputFTP = new DirectoryInfo(ViewModel.FTP);
                FileInfo[] FilesInFTP = inputFTP.GetFiles("*.txt");

                foreach (FileInfo file in FilesInFTP.Where(x => x.Name.Contains("Presence_App_Upload_T1")))
                {
                    if (!file.IsReadOnly)
                    {
                        string source = Path.Combine(ViewModel.FTP, file.Name);
                        string destination = Path.Combine(ViewModel.Input, file.Name);
                        File.Move(source, destination);
                        LogRepo.Log("GetFileList() - File.Move(source, destination) => " + file.Name);
                    }
                    else
                    {
                        LogRepo.Log("GetFileList() [FTP] - File.IsReadOnly => " + file.Name);
                    }
                }

                LogRepo.Log("GetFileList() - Checking Input Directory => " + ViewModel.Input);

                DirectoryInfo input = new DirectoryInfo(ViewModel.Input);
                FileInfo[] FilesInInput = input.GetFiles("*.txt");

                foreach (FileInfo file in FilesInInput.Where(x => x.Name.Contains("Presence_App_Upload_T1")))
                {
                    if (!file.IsReadOnly)
                    {
                        files.Add(file);
                        LogRepo.Log("GetFileList() - files.Add(file) => " + file.Name);
                    }
                    else
                    {
                        LogRepo.Log("GetFileList() [Input] - File.IsReadOnly => " + file.Name);
                    }
                }

                return files;
            }
            catch (Exception ex)
            {
                LogRepo.Log("GetFileList() - Error => " + ex.Message.ToString());
                Task.Run(async () => await MailRepo.SendError("Unable to get the Files", ex.Message.ToString() + " => Please view log for more information")).GetAwaiter().GetResult();
                return files;
            }
        }
        #endregion

        #region [ Manage Text File ]
        private async Task<int> ManageTextFile(FileInfo file)
        {
            Dictionary<int, string> StatusList = new Dictionary<int, string>();
            Dictionary<int, int> QueueList = new Dictionary<int, int>();

            try
            {
                LogRepo.Log("\nManage Text File => " + file.Name);

                AddCalls = ColumnsDataRequest(AddCalls);
                RmvCalls = ColumnsDataRequest(RmvCalls);
                PresenceData = ColumnsDataPresence(PresenceData);
                InvalidPhoneData = ColumnsDataInvalidPhone(InvalidPhoneData);

                var SqlRepo = new SQLRepository();
                Task.Run(async () => StatusList = await SqlRepo.GetServiceLoadStatus()).GetAwaiter().GetResult();
                Task.Run(async () => QueueList = await SqlRepo.GetPresenceSourceIDs()).GetAwaiter().GetResult();

                StatsFileModel.FileName = file.Name;
                StatsFileModel.Start = DateTime.Now;

                string result = ReadTextFile(file);
                if (result == "Data In DataTables")
                {
                    string removeCallsResult = "";
                    string addCallsResult = "";

                    List<Task> ProcessTasks = new List<Task>
                    {
                        Task.Run(async () => removeCallsResult = await ProcessRemoveCalls()),
                        Task.Run(async () => addCallsResult = await ProcessAddCalls(StatusList,QueueList))
                    };
                    Task.WaitAll(ProcessTasks.ToArray());

                    if (removeCallsResult == "Completed" && addCallsResult == "Completed")
                    {
                        int close = await CloseTextFile(file);
                    }
                    else
                    {
                        LogRepo.Log("\nFile did not complete the process (Rerun the file) => " + result + " = " + file.Name);
                        int closeToRerun = await CloseTextFileRerun(file);
                    }
                }
                else
                {
                    LogRepo.Log("\nError => " + result + " = " + file.Name);
                }

                return 1;
            }
            catch (Exception ex)
            {
                await MailRepo.SendError("Unable to Manage the File", ex.Message.ToString() + " => Please view log for more information");
                return 0;
            }
        }
        #endregion

        #region [Private] - [ Read Text File ]
        private string ReadTextFile(FileInfo file)
        {
            Dictionary<int, int> SourceList = new Dictionary<int, int>();
            Dictionary<string, int> RetailerServiceMap = MapServiceID();
            try
            {
                string fileLine;
                using (var fileStream = File.OpenRead(Path.Combine(ViewModel.Input, file.Name)))
                {
                    using (var reader = new StreamReader(fileStream, Encoding.UTF8, true, 1024))
                    {
                        while ((fileLine = reader.ReadLine()) != null)
                        {
                            if (fileLine != ViewModel.FileHeaderTOne)
                            {
                                StatsFileModel.Total++;
                                string[] DataStructure = fileLine.Split('~').ToArray();
                                int _SourceID = int.Parse(DataStructure[5].ToString(), CultureInfo.InvariantCulture);

                                bool HasSourceID = SourceList.ContainsValue(_SourceID);
                                if (HasSourceID == false)
                                {
                                    SourceList.Add(StatsFileModel.Total, _SourceID);
                                    string result = FormatTextFile(DataStructure, file.Name, RetailerServiceMap);
                                    if (result == "Format Pass")
                                    {
                                        StatsFileModel.FormatPass++;
                                    }
                                    else
                                    {
                                        StatsFileModel.FormatFail++;
                                        LogRepo.LogErrorLine(fileLine);
                                    }
                                }
                                else
                                {
                                    StatsFileModel.FormatFail++;
                                    LogRepo.LogDuplicateLine(fileLine);
                                }
                            }
                        }
                    }
                }

                SourceList.Clear();
                return "Data In DataTables";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        #endregion

        #region [ Handle Routing Of Call based on Retailer_Code ]
        private Dictionary<string,int> MapServiceID()
        {
            try
            {
                Dictionary<string, int> outMap = new Dictionary<string, int>();
                outMap.Add("PEP", 78);
                outMap.Add("ACKS", 71);
                outMap.Add("DUNNS", 71);
                outMap.Add("REFINE", 71);
                outMap.Add("SHOEC", 71);
                outMap.Add("TT", 71);
                return outMap;
            }
            catch(Exception)
            {
                throw;
            }
        }
        #endregion

        #region [Private] - [ Format Text File ]
        private string FormatTextFile(string[] strRow, string fileName, Dictionary<string, int> inServiceMap)
        {
            try
            {
                #region [ Add Call Command ]
                if (strRow[2].ToString().ToLower().Trim() == "add")
                {
                    DataRow NewRow = AddCalls.NewRow();
                    NewRow["ID"] = DBNull.Value;
                    NewRow["FK"] = "0";
                    NewRow["InputType"] = "File";
                    NewRow["File"] = fileName;
                    NewRow["Status"] = "Received";
                    NewRow["Retry"] = "0";
                    NewRow["LastRetry"] = DBNull.Value;
                    NewRow["Received"] = DateTime.Now;
                    NewRow["Actioned"] = DateTime.Now;
                    NewRow["Command"] = "addcall";
                    // WVDB - handle setting of ServiceID according to new field in index 10
                    NewRow["ServiceID"] = inServiceMap[strRow[10]];
                    NewRow["LoadID"] = int.Parse(strRow[11], CultureInfo.InvariantCulture); // WVDB - handle setting of ServiceID according to new field in index 10
                    NewRow["CampaignNo"] = strRow[11]; // WVDB - handle setting of ServiceID according to new field in index 10
                    NewRow["SourceID"] = int.Parse(strRow[5], CultureInfo.InvariantCulture);
                    NewRow["CallerName"] = strRow[4];
                    NewRow["Phone1"] = strRow[17]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone2"] = strRow[18]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone3"] = strRow[19]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone4"] = strRow[20]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone5"] = strRow[21]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone6"] = strRow[22]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone7"] = strRow[23]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone8"] = strRow[24]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone9"] = strRow[25]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Phone10"] = strRow[26]; // WVDB - increment string array index by 1 to accomodate for new field in index 10
                    NewRow["Comments"] = strRow[11]; // WVDB - handle setting of ServiceID according to new field in index 10
                    NewRow["CustomData1"] = DBNull.Value;
                    NewRow["CustomData2"] = DBNull.Value;
                    NewRow["CustomData3"] = DBNull.Value;
                    NewRow["CallerID"] = DBNull.Value;
                    NewRow["AgentLogin"] = "0";
                    NewRow["ScheduleDate"] = DBNull.Value;
                    NewRow["Priority"] = "1";
                    NewRow["ClientTitle"] = strRow[13]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientName"] = strRow[14]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientSurname"] = strRow[15]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientIDNumber"] = strRow[16]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientGender"] = DBNull.Value;
                    NewRow["ClientEmail"] = strRow[27]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["CampaignTypeCode"] = strRow[3];
                    NewRow["CDStatusNo"] = 1;
                    NewRow["RCStatusNo"] = 1;
                    NewRow["Product"] = strRow[7];
                    NewRow["BalanceAmount"] = 0;
                    NewRow["CurrentDueAmount"] = 0;
                    NewRow["OTBAmount"] = 0;
                    NewRow["PastDueAmount"] = 0;
                    NewRow["TotalDueAmount"] = 0;
                    NewRow["CollectionStatus"] = DBNull.Value;
                    NewRow["CampaignDescription"] = strRow[12]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["AccountStatus"] = DBNull.Value;
                    NewRow["OriginCode"] = strRow[6];
                    NewRow["Block1"] = DBNull.Value;
                    NewRow["Block2"] = DBNull.Value;
                    NewRow["CardNo"] = DBNull.Value;
                    NewRow["CampaignReason"] = DBNull.Value;
                    NewRow["CreditUsage"] = DBNull.Value;
                    NewRow["DebitOrderDay"] = DBNull.Value;
                    NewRow["DebitOrderFlag"] = DBNull.Value;
                    NewRow["CompanyName"] = DBNull.Value;
                    NewRow["HomeCity"] = DBNull.Value;
                    NewRow["HomeCode"] = DBNull.Value;
                    NewRow["HomeProvince"] = DBNull.Value;
                    NewRow["HomeSuburb"] = DBNull.Value;
                    NewRow["CD1"] = DBNull.Value;
                    NewRow["CD2"] = DBNull.Value;
                    NewRow["CD3"] = DBNull.Value;
                    NewRow["CD4"] = DBNull.Value;
                    NewRow["CD5"] = DBNull.Value;
                    NewRow["CD6"] = DBNull.Value;
                    NewRow["RC1"] = DBNull.Value;
                    NewRow["RC2"] = DBNull.Value;
                    NewRow["RC3"] = DBNull.Value;
                    NewRow["RC4"] = DBNull.Value;
                    NewRow["RC5"] = DBNull.Value;
                    NewRow["RC6"] = DBNull.Value;
                    NewRow["LastPaymentAmt"] = 0;
                    NewRow["LastPaymentDate"] = DBNull.Value;
                    NewRow["LastPurchaseDate"] = DBNull.Value;
                    NewRow["MonthsOpen"] = "0";
                    NewRow["Occupation"] = DBNull.Value;
                    NewRow["ProductNo"] = "0";
                    NewRow["RetailerGroup"] = strRow[7];
                    NewRow["BccRiskBand"] = DBNull.Value;
                    NewRow["FieldsToModify"] = DBNull.Value;
                    NewRow["IPAddress"] = "127.0.0.1";
                    NewRow["CurrencySymbol"] = DBNull.Value;

                    AddCalls.Rows.Add(NewRow);
                    return "Format Pass";
                }
                #endregion

                #region [ Remove Call Command ]
                if (strRow[4].ToString().ToLower().Trim() == "delete")
                {
                    DataRow NewRow = RmvCalls.NewRow();
                    NewRow["ID"] = DBNull.Value;
                    NewRow["FK"] = "0";
                    NewRow["InputType"] = "File";
                    NewRow["File"] = fileName;
                    NewRow["Status"] = "Received";
                    NewRow["Retry"] = "0";
                    NewRow["LastRetry"] = DBNull.Value;
                    NewRow["Received"] = DateTime.Now;
                    NewRow["Actioned"] = DateTime.Now;
                    NewRow["Command"] = "removecall";
                    // WVDB - handle setting of ServiceID according to new field in index 11
                    NewRow["ServiceID"] = inServiceMap[strRow[10]];
                    NewRow["LoadID"] = int.Parse(strRow[11], CultureInfo.InvariantCulture); // WVDB - handle setting of ServiceID according to new field in index 10
                    NewRow["CampaignNo"] = strRow[11]; // WVDB - handle setting of ServiceID according to new field in index 10
                    NewRow["SourceID"] = int.Parse(strRow[5], CultureInfo.InvariantCulture);
                    NewRow["CallerName"] = strRow[4];
                    NewRow["Phone1"] = strRow[17]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone2"] = strRow[18]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone3"] = strRow[19]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone4"] = strRow[20]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone5"] = strRow[21]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone6"] = strRow[22]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone7"] = strRow[23]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone8"] = strRow[24]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone9"] = strRow[25]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Phone10"] = strRow[26]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["Comments"] = strRow[11]; // WVDB - handle setting of ServiceID according to new field in index 10
                    NewRow["CustomData1"] = DBNull.Value;
                    NewRow["CustomData2"] = DBNull.Value;
                    NewRow["CustomData3"] = DBNull.Value;
                    NewRow["CallerID"] = DBNull.Value;
                    NewRow["AgentLogin"] = "0";
                    NewRow["ScheduleDate"] = DBNull.Value;
                    NewRow["Priority"] = "1";
                    NewRow["ClientTitle"] = strRow[13]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientName"] = strRow[14]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientSurname"] = strRow[15]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientIDNumber"] = strRow[16]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["ClientGender"] = DBNull.Value;
                    NewRow["ClientEmail"] = strRow[27]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["CampaignTypeCode"] = strRow[3];
                    NewRow["CDStatusNo"] = 1;
                    NewRow["RCStatusNo"] = 1;
                    NewRow["Product"] = strRow[7];
                    NewRow["BalanceAmount"] = 0;
                    NewRow["CurrentDueAmount"] = 0;
                    NewRow["OTBAmount"] = 0;
                    NewRow["PastDueAmount"] = 0;
                    NewRow["TotalDueAmount"] = 0;
                    NewRow["CollectionStatus"] = DBNull.Value;
                    NewRow["CampaignDescription"] = strRow[12]; // WVDB - increment string array index by 1 to accomodate for new field in index 11
                    NewRow["AccountStatus"] = DBNull.Value;
                    NewRow["OriginCode"] = strRow[6];
                    NewRow["Block1"] = DBNull.Value;
                    NewRow["Block2"] = DBNull.Value;
                    NewRow["CardNo"] = DBNull.Value;
                    NewRow["CampaignReason"] = DBNull.Value;
                    NewRow["CreditUsage"] = DBNull.Value;
                    NewRow["DebitOrderDay"] = DBNull.Value;
                    NewRow["DebitOrderFlag"] = DBNull.Value;
                    NewRow["CompanyName"] = DBNull.Value;
                    NewRow["HomeCity"] = DBNull.Value;
                    NewRow["HomeCode"] = DBNull.Value;
                    NewRow["HomeProvince"] = DBNull.Value;
                    NewRow["HomeSuburb"] = DBNull.Value;
                    NewRow["CD1"] = DBNull.Value;
                    NewRow["CD2"] = DBNull.Value;
                    NewRow["CD3"] = DBNull.Value;
                    NewRow["CD4"] = DBNull.Value;
                    NewRow["CD5"] = DBNull.Value;
                    NewRow["CD6"] = DBNull.Value;
                    NewRow["RC1"] = DBNull.Value;
                    NewRow["RC2"] = DBNull.Value;
                    NewRow["RC3"] = DBNull.Value;
                    NewRow["RC4"] = DBNull.Value;
                    NewRow["RC5"] = DBNull.Value;
                    NewRow["RC6"] = DBNull.Value;
                    NewRow["LastPaymentAmt"] = 0;
                    NewRow["LastPaymentDate"] = DBNull.Value;
                    NewRow["LastPurchaseDate"] = DBNull.Value;
                    NewRow["MonthsOpen"] = "0";
                    NewRow["Occupation"] = DBNull.Value;
                    NewRow["ProductNo"] = "0";
                    NewRow["RetailerGroup"] = strRow[7];
                    NewRow["BccRiskBand"] = DBNull.Value;
                    NewRow["FieldsToModify"] = DBNull.Value;
                    NewRow["IPAddress"] = "127.0.0.1";
                    NewRow["CurrencySymbol"] = DBNull.Value;

                    RmvCalls.Rows.Add(NewRow);
                    return "Format Pass";
                }
                #endregion

                return "Should Not Get To This Point";
            }
            catch (Exception ex)
            {
                LogRepo.Log("FormatTextFile() - Error => " + ex.Message.ToString());
                return "Format Fail";
            }
        }
        #endregion

        #region [Private] - [ Process Remove Calls ]
        private async Task<string> ProcessRemoveCalls()
        {
            Dictionary<int, string> ResultStatus = new Dictionary<int, string>();
            try
            {
                using (var conn = new SqlConnection(ViewModel.dbPresence))
                {
                    await conn.OpenAsync();
                    for (int i = 0; i < RmvCalls.Rows.Count; i++)
                    {
                        string RecordStatus = "";
                        int SourceID = int.Parse(RmvCalls.Rows[i]["SourceID"].ToString(), CultureInfo.InvariantCulture);

                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"DELETE TOP (10) FROM [PREP].[PCO_OUTBOUNDQUEUE] WHERE [SOURCEID] = " + SourceID + " AND [SERVICEID] NOT IN " + ViewModel.ExcludeServices;
                            int Records = cmd.ExecuteNonQuery();

                            if (Records > 0)
                            {
                                RecordStatus = "Success";
                                StatsFileModel.QueryRmvPass++;
                            }
                            else
                            {
                                RecordStatus = "No Record";
                                StatsFileModel.QueryRmvFail++;
                            }
                            RmvCalls.Rows[i]["Status"] = RecordStatus;
                        }
                    };
                    conn.Close();
                }

                int input = RmvCalls.Rows.Count;
                int processed = StatsFileModel.QueryRmvPass + StatsFileModel.QueryRmvFail;
                if (input != processed)
                {
                    return "Not The Same";
                }

                var SqlRepo = new SQLRepository();
                SqlRepo.SqlBulkDataRequestDone(RmvCalls);

                RmvCalls.Clear();
                RmvCalls.Dispose();
                ResultStatus.Clear();

                return "Completed";
            }
            catch (Exception ex)
            {
                await MailRepo.SendError("File Process Remove Calls", ex.ToString());
                return "Not The Same";
            }
        }
        #endregion

        #region [Private] - [ Process Add Calls ]
        private async Task<string> ProcessAddCalls(Dictionary<int, string> StatusList, Dictionary<int, int> QueueList)
        {
            var SqlRepo = new SQLRepository();
            var RuleRepo = new RuleRepository();
            Dictionary<int, string> ResultStatus = new Dictionary<int, string>();
            Dictionary<int, string[]> OutboundQueue = new Dictionary<int, string[]>();
            Dictionary<int, string[]> InvalidPhone = new Dictionary<int, string[]>();
            try
            {
                for (int i = 0; i < AddCalls.Rows.Count; i++)
                {
                    string RecordStatus = "";
                    int SourceID = int.Parse(AddCalls.Rows[i]["SourceID"].ToString(), CultureInfo.InvariantCulture);
                    bool InPresence = QueueList.ContainsValue(SourceID);
                    if (InPresence == true)
                    {
                        RecordStatus = "Existing Record";
                        StatsFileModel.QueryAddFail++;
                    }
                    else
                    {
                        int inputLoadID = int.Parse(AddCalls.Rows[i]["LoadID"].ToString(), CultureInfo.InvariantCulture);
                        int inputServiceID = int.Parse(AddCalls.Rows[i]["ServiceID"].ToString(), CultureInfo.InvariantCulture);
                        string inputCampaignNo = AddCalls.Rows[i]["CampaignNo"].ToString();
                        string inputCampaignDescription = AddCalls.Rows[i]["CampaignDescription"].ToString();

                        int _LoadID = RuleRepo.RuleCreateLoadID(inputLoadID, inputCampaignNo, inputCampaignDescription, inputServiceID);
                        if (_LoadID != 0)
                        {
                            PhoneModel numbers = new PhoneModel { Phone1 = AddCalls.Rows[i]["Phone1"].ToString(), Phone2 = AddCalls.Rows[i]["Phone2"].ToString(), Phone3 = AddCalls.Rows[i]["Phone3"].ToString(), Phone4 = AddCalls.Rows[i]["Phone4"].ToString(), Phone5 = AddCalls.Rows[i]["Phone5"].ToString(), Phone6 = AddCalls.Rows[i]["Phone6"].ToString(), Phone7 = AddCalls.Rows[i]["Phone7"].ToString(), Phone8 = AddCalls.Rows[i]["Phone8"].ToString(), Phone9 = AddCalls.Rows[i]["Phone9"].ToString(), Phone10 = AddCalls.Rows[i]["Phone10"].ToString() };
                            numbers = RuleRepo.RuleFormatPhone(numbers);
                            string IsPhoneCorrect = RuleRepo.RulePhoneCorrect(numbers);
                            if (IsPhoneCorrect == "Yes - Numbers are Valid")
                            {
                                string _ServiceID = AddCalls.Rows[i]["ServiceID"].ToString();
                                bool IsEnabled = StatusList.ContainsValue(_ServiceID + "," + _LoadID + ",E");
                                int _LoadStatus = (IsEnabled == true) ? 1 : 41;

                                string[] QueueItem = new string[] { _ServiceID, AddCalls.Rows[i]["CallerName"].ToString(), AddCalls.Rows[i]["SourceID"].ToString(), _LoadStatus.ToString(), _LoadID.ToString(), numbers.Phone1.ToString(), numbers.Phone2.ToString(), numbers.Phone3.ToString(), numbers.Phone4.ToString(), numbers.Phone5.ToString(), numbers.Phone6.ToString(), numbers.Phone7.ToString(), numbers.Phone8.ToString(), numbers.Phone9.ToString(), numbers.Phone10.ToString(), AddCalls.Rows[i]["Comments"].ToString(), AddCalls.Rows[i]["CustomData1"].ToString(), AddCalls.Rows[i]["CustomData2"].ToString(), AddCalls.Rows[i]["CustomData3"].ToString() };
                                OutboundQueue.Add(i, QueueItem);
                                RecordStatus = "Success";
                                StatsFileModel.QueryAddPass++;
                            }
                            else
                            {
                                string[] Invalid = new string[] { AddCalls.Rows[i]["SourceID"].ToString(), AddCalls.Rows[i]["ServiceID"].ToString(), AddCalls.Rows[i]["LoadID"].ToString(), AddCalls.Rows[i]["CallerName"].ToString(), AddCalls.Rows[i]["Phone1"].ToString(), AddCalls.Rows[i]["Phone2"].ToString(), AddCalls.Rows[i]["Phone3"].ToString(), AddCalls.Rows[i]["Phone4"].ToString(), AddCalls.Rows[i]["Phone5"].ToString(), AddCalls.Rows[i]["Phone6"].ToString(), AddCalls.Rows[i]["Phone7"].ToString(), AddCalls.Rows[i]["Phone8"].ToString(), AddCalls.Rows[i]["Phone9"].ToString(), AddCalls.Rows[i]["Phone10"].ToString() };
                                InvalidPhone.Add(i, Invalid);
                                RecordStatus = "Invalid Phone";
                                StatsFileModel.QueryAddInvalidPhone++;
                            }
                        }
                        else
                        {
                            RecordStatus = "Not Defined";
                            StatsFileModel.QueryAddNotDefined++;
                        }
                    }
                    AddCalls.Rows[i]["Status"] = RecordStatus;
                };

                foreach (var item in OutboundQueue)
                {
                    #region [ Add Row Presence Data ]
                    DataRow NewRow = PresenceData.NewRow();
                    NewRow["ID"] = DBNull.Value;
                    NewRow["SERVICEID"] = item.Value[0].ToString();
                    NewRow["NAME"] = item.Value[1].ToString();
                    NewRow["PHONE"] = item.Value[5].ToString();
                    NewRow["CALLINGHOURS"] = DBNull.Value;
                    NewRow["SOURCEID"] = item.Value[2].ToString();
                    NewRow["STATUS"] = item.Value[3].ToString();
                    NewRow["SCHEDULETYPE"] = DBNull.Value;
                    NewRow["SCHEDULEDATE"] = DBNull.Value;
                    NewRow["LOADID"] = item.Value[4].ToString();
                    NewRow["LASTAGENT"] = DBNull.Value;
                    NewRow["LASTQCODE"] = DBNull.Value;
                    NewRow["FIRSTHANDLINGDATE"] = DBNull.Value;
                    NewRow["LASTHANDLINGDATE"] = DBNull.Value;
                    NewRow["DAILYCOUNTER"] = DBNull.Value;
                    NewRow["TOTALCOUNTER"] = DBNull.Value;
                    NewRow["BUSYSIGNALCOUNTER"] = DBNull.Value;
                    NewRow["NOANSWERCOUNTER"] = DBNull.Value;
                    NewRow["ANSWERMACHINECOUNTER"] = DBNull.Value;
                    NewRow["FAXCOUNTER"] = DBNull.Value;
                    NewRow["INVGENREASONCOUNTER"] = DBNull.Value;
                    NewRow["PRIORITY"] = "2";
                    NewRow["CAPTURINGAGENT"] = DBNull.Value;
                    NewRow["PHONE1"] = item.Value[5].ToString();
                    NewRow["PHONE2"] = item.Value[6].ToString();
                    NewRow["PHONE3"] = item.Value[7].ToString();
                    NewRow["PHONE4"] = item.Value[8].ToString();
                    NewRow["PHONE5"] = item.Value[9].ToString();
                    NewRow["PHONE6"] = item.Value[10].ToString();
                    NewRow["PHONE7"] = item.Value[11].ToString();
                    NewRow["PHONE8"] = item.Value[12].ToString();
                    NewRow["PHONE9"] = item.Value[13].ToString();
                    NewRow["PHONE10"] = item.Value[14].ToString();
                    NewRow["PHONEDESC1"] = "1";
                    NewRow["PHONEDESC2"] = "2";
                    NewRow["PHONEDESC3"] = "3";
                    NewRow["PHONEDESC4"] = "4";
                    NewRow["PHONEDESC5"] = "5";
                    NewRow["PHONEDESC6"] = "6";
                    NewRow["PHONEDESC7"] = "7";
                    NewRow["PHONEDESC8"] = "8";
                    NewRow["PHONEDESC9"] = "9";
                    NewRow["PHONEDESC10"] = "10";
                    NewRow["PHONESTATUS1"] = DBNull.Value;
                    NewRow["PHONESTATUS2"] = DBNull.Value;
                    NewRow["PHONESTATUS3"] = DBNull.Value;
                    NewRow["PHONESTATUS4"] = DBNull.Value;
                    NewRow["PHONESTATUS5"] = DBNull.Value;
                    NewRow["PHONESTATUS6"] = DBNull.Value;
                    NewRow["PHONESTATUS7"] = DBNull.Value;
                    NewRow["PHONESTATUS8"] = DBNull.Value;
                    NewRow["PHONESTATUS9"] = DBNull.Value;
                    NewRow["PHONESTATUS10"] = DBNull.Value;
                    NewRow["PHONETIMEZONEID"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID1"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID2"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID3"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID4"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID5"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID6"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID7"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID8"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID9"] = "Presence_Server";
                    NewRow["PHONETIMEZONEID10"] = "Presence_Server";
                    NewRow["CURRENTPHONE"] = "0";
                    NewRow["CURRENTPHONECOUNTER"] = "0";
                    NewRow["TIMEZONEID"] = "Presence_Server";
                    NewRow["COMMENTS"] = item.Value[15].ToString();
                    NewRow["RDATE"] = DateTime.Now;
                    NewRow["CUSTOMDATA1"] = item.Value[16].ToString();
                    NewRow["CUSTOMDATA2"] = item.Value[17].ToString();
                    NewRow["CUSTOMDATA3"] = item.Value[18].ToString();
                    NewRow["CALLERID"] = item.Value[2].ToString();
                    NewRow["CALLERNAME"] = item.Value[1].ToString();

                    PresenceData.Rows.Add(NewRow);
                    #endregion
                }

                foreach (var item in InvalidPhone)
                {
                    #region [ Add Row Presence Data ]
                    DataRow NewRow = InvalidPhoneData.NewRow();
                    NewRow["ID"] = DBNull.Value;
                    NewRow["InputType"] = "File";
                    NewRow["File"] = StatsFileModel.FileName;
                    NewRow["Received"] = DateTime.Now;
                    NewRow["Command"] = "addCall";
                    NewRow["SourceID"] = item.Value[0].ToString();
                    NewRow["ServiceID"] = item.Value[1].ToString();
                    NewRow["LoadID"] = item.Value[2].ToString();
                    NewRow["CallerName"] = item.Value[3].ToString();
                    NewRow["PH1"] = item.Value[4].ToString();
                    NewRow["PH2"] = item.Value[5].ToString();
                    NewRow["PH3"] = item.Value[6].ToString();
                    NewRow["PH4"] = item.Value[7].ToString();
                    NewRow["PH5"] = item.Value[8].ToString();
                    NewRow["PH6"] = item.Value[9].ToString();
                    NewRow["PH7"] = item.Value[10].ToString();
                    NewRow["PH8"] = item.Value[11].ToString();
                    NewRow["PH9"] = item.Value[12].ToString();
                    NewRow["PH10"] = item.Value[13].ToString();

                    InvalidPhoneData.Rows.Add(NewRow);
                    #endregion
                }

                SqlRepo.SqlBulkPresenceData(PresenceData);
                SqlRepo.SqlBulkDataRequestDone(AddCalls);
                SqlRepo.SqlBulkDataRequestInvalidPhone(InvalidPhoneData);

                AddCalls.Clear();
                AddCalls.Dispose();

                InvalidPhoneData.Clear();
                InvalidPhoneData.Dispose();

                ResultStatus.Clear();
                OutboundQueue.Clear();
                InvalidPhone.Clear();

                return "Completed";
            }
            catch (Exception ex)
            {
                await MailRepo.SendError("File Process Add Calls", ex.ToString());
                return ex.Message.ToString();
            }
        }
        #endregion

        #region [Private] - [ Close Text File ]
        private async Task<int> CloseTextFile(FileInfo file)
        {
            var EmailRepo = new EmailRepository();
            try
            {
                string source = Path.Combine(ViewModel.Input, file.Name);
                string destination = Path.Combine(ViewModel.Complete, file.Name);

                if (!Directory.Exists(ViewModel.Complete))
                    Directory.CreateDirectory(ViewModel.Complete);

                StatsFileModel.End = DateTime.Now;

                File.Move(source, destination);
                await EmailRepo.SendReportFile(file);

                StatsFileModel.FileName = "";
                StatsFileModel.Total = 0;

                StatsFileModel.FormatPass = 0;
                StatsFileModel.FormatFail = 0;

                StatsFileModel.QueryRmvPass = 0;
                StatsFileModel.QueryRmvFail = 0;

                StatsFileModel.QueryAddPass = 0;
                StatsFileModel.QueryAddFail = 0;

                StatsFileModel.QueryAddPresenceFail = 0;
                StatsFileModel.QueryAddInvalidPhone = 0;
                StatsFileModel.QueryAddNotDefined = 0;

                StatsFileModel.Start = DateTime.Now;
                StatsFileModel.End = DateTime.Now;

                return 1;
            }
            catch (Exception ex)
            {
                LogRepo.Log("CloseTextFile() - Error => " + ex.Message.ToString());
                return 0;
            }
        }
        #endregion

        #region [Private] - [ Close Text File Rerun ]
        private async Task<int> CloseTextFileRerun(FileInfo file)
        {
            var EmailRepo = new EmailRepository();
            try
            {
                StatsFileModel.End = DateTime.Now;
                await EmailRepo.SendReportFileRerun(file);

                StatsFileModel.FileName = "";
                StatsFileModel.Total = 0;

                StatsFileModel.FormatPass = 0;
                StatsFileModel.FormatFail = 0;

                StatsFileModel.QueryRmvPass = 0;
                StatsFileModel.QueryRmvFail = 0;

                StatsFileModel.QueryAddPass = 0;
                StatsFileModel.QueryAddFail = 0;

                StatsFileModel.QueryAddPresenceFail = 0;
                StatsFileModel.QueryAddInvalidPhone = 0;
                StatsFileModel.QueryAddNotDefined = 0;

                StatsFileModel.Start = DateTime.Now;
                StatsFileModel.End = DateTime.Now;

                return 1;
            }
            catch (Exception ex)
            {
                LogRepo.Log("CloseTextFileRerun() - Error => " + ex.Message.ToString());
                return 0;
            }
        }
        #endregion

        //---------------------------------------------------------------------------//

        #region [ Columns Data Request ]
        private DataTable ColumnsDataRequest(DataTable BigDataAdd)
        {
            #region [ Column Mappings => BigDataAdd ]
            BigDataAdd.Columns.Add("ID", typeof(int));
            BigDataAdd.Columns.Add("FK", typeof(int));
            BigDataAdd.Columns.Add("InputType", typeof(string));
            BigDataAdd.Columns.Add("File", typeof(string));
            BigDataAdd.Columns.Add("Status", typeof(string));
            BigDataAdd.Columns.Add("Retry", typeof(int));
            BigDataAdd.Columns.Add("LastRetry", typeof(DateTime));
            BigDataAdd.Columns.Add("Received", typeof(DateTime));
            BigDataAdd.Columns.Add("Actioned", typeof(DateTime));
            BigDataAdd.Columns.Add("Command", typeof(string));
            BigDataAdd.Columns.Add("ServiceID", typeof(int));
            BigDataAdd.Columns.Add("LoadID", typeof(string));
            BigDataAdd.Columns.Add("CampaignNo", typeof(string));
            BigDataAdd.Columns.Add("SourceID", typeof(int));
            BigDataAdd.Columns.Add("CallerName", typeof(string));
            BigDataAdd.Columns.Add("Phone1", typeof(string));
            BigDataAdd.Columns.Add("Phone2", typeof(string));
            BigDataAdd.Columns.Add("Phone3", typeof(string));
            BigDataAdd.Columns.Add("Phone4", typeof(string));
            BigDataAdd.Columns.Add("Phone5", typeof(string));
            BigDataAdd.Columns.Add("Phone6", typeof(string));
            BigDataAdd.Columns.Add("Phone7", typeof(string));
            BigDataAdd.Columns.Add("Phone8", typeof(string));
            BigDataAdd.Columns.Add("Phone9", typeof(string));
            BigDataAdd.Columns.Add("Phone10", typeof(string));
            BigDataAdd.Columns.Add("Comments", typeof(string));
            BigDataAdd.Columns.Add("CustomData1", typeof(string));
            BigDataAdd.Columns.Add("CustomData2", typeof(string));
            BigDataAdd.Columns.Add("CustomData3", typeof(string));
            BigDataAdd.Columns.Add("CallerID", typeof(string));
            BigDataAdd.Columns.Add("AgentLogin", typeof(int));
            BigDataAdd.Columns.Add("ScheduleDate", typeof(DateTime));
            BigDataAdd.Columns.Add("Priority", typeof(int));
            BigDataAdd.Columns.Add("ClientTitle", typeof(string));
            BigDataAdd.Columns.Add("ClientName", typeof(string));
            BigDataAdd.Columns.Add("ClientSurname", typeof(string));
            BigDataAdd.Columns.Add("ClientIDNumber", typeof(string));
            BigDataAdd.Columns.Add("ClientGender", typeof(string));
            BigDataAdd.Columns.Add("ClientEmail", typeof(string));
            BigDataAdd.Columns.Add("CampaignTypeCode", typeof(string));
            BigDataAdd.Columns.Add("CDStatusNo", typeof(int));
            BigDataAdd.Columns.Add("RCStatusNo", typeof(int));
            BigDataAdd.Columns.Add("Product", typeof(string));
            BigDataAdd.Columns.Add("BalanceAmount", typeof(decimal));
            BigDataAdd.Columns.Add("CurrentDueAmount", typeof(decimal));
            BigDataAdd.Columns.Add("OTBAmount", typeof(decimal));
            BigDataAdd.Columns.Add("PastDueAmount", typeof(decimal));
            BigDataAdd.Columns.Add("TotalDueAmount", typeof(decimal));
            BigDataAdd.Columns.Add("CollectionStatus", typeof(string));
            BigDataAdd.Columns.Add("CampaignDescription", typeof(string));
            BigDataAdd.Columns.Add("AccountStatus", typeof(string));
            BigDataAdd.Columns.Add("OriginCode", typeof(string));
            BigDataAdd.Columns.Add("Block1", typeof(string));
            BigDataAdd.Columns.Add("Block2", typeof(string));
            BigDataAdd.Columns.Add("CardNo", typeof(string));
            BigDataAdd.Columns.Add("CampaignReason", typeof(string));
            BigDataAdd.Columns.Add("CreditUsage", typeof(string));
            BigDataAdd.Columns.Add("DebitOrderDay", typeof(string));
            BigDataAdd.Columns.Add("DebitOrderFlag", typeof(string));
            BigDataAdd.Columns.Add("CompanyName", typeof(string));
            BigDataAdd.Columns.Add("HomeCity", typeof(string));
            BigDataAdd.Columns.Add("HomeCode", typeof(string));
            BigDataAdd.Columns.Add("HomeProvince", typeof(string));
            BigDataAdd.Columns.Add("HomeSuburb", typeof(string));
            BigDataAdd.Columns.Add("CD1", typeof(string));
            BigDataAdd.Columns.Add("CD2", typeof(string));
            BigDataAdd.Columns.Add("CD3", typeof(string));
            BigDataAdd.Columns.Add("CD4", typeof(string));
            BigDataAdd.Columns.Add("CD5", typeof(string));
            BigDataAdd.Columns.Add("CD6", typeof(string));
            BigDataAdd.Columns.Add("RC1", typeof(string));
            BigDataAdd.Columns.Add("RC2", typeof(string));
            BigDataAdd.Columns.Add("RC3", typeof(string));
            BigDataAdd.Columns.Add("RC4", typeof(string));
            BigDataAdd.Columns.Add("RC5", typeof(string));
            BigDataAdd.Columns.Add("RC6", typeof(string));
            BigDataAdd.Columns.Add("LastPaymentAmt", typeof(decimal));
            BigDataAdd.Columns.Add("LastPaymentDate", typeof(DateTime));
            BigDataAdd.Columns.Add("LastPurchaseDate", typeof(DateTime));
            BigDataAdd.Columns.Add("MonthsOpen", typeof(int));
            BigDataAdd.Columns.Add("Occupation", typeof(string));
            BigDataAdd.Columns.Add("ProductNo", typeof(int));
            BigDataAdd.Columns.Add("RetailerGroup", typeof(string));
            BigDataAdd.Columns.Add("BccRiskBand", typeof(string));
            BigDataAdd.Columns.Add("FieldsToModify", typeof(string));
            BigDataAdd.Columns.Add("IPAddress", typeof(string));
            BigDataAdd.Columns.Add("CurrencySymbol", typeof(string));
            #endregion

            return BigDataAdd;
        }
        #endregion

        #region [ Columns Data Presence ]
        private DataTable ColumnsDataPresence(DataTable DataPresence)
        {
            #region [ Column Mappings => DataPresence ]
            DataPresence.Columns.Add("ID", typeof(int));
            DataPresence.Columns.Add("SERVICEID", typeof(int));
            DataPresence.Columns.Add("NAME", typeof(string));
            DataPresence.Columns.Add("PHONE", typeof(string));
            DataPresence.Columns.Add("CALLINGHOURS", typeof(string));
            DataPresence.Columns.Add("SOURCEID", typeof(int));
            DataPresence.Columns.Add("STATUS", typeof(int));
            DataPresence.Columns.Add("SCHEDULETYPE", typeof(char));
            DataPresence.Columns.Add("SCHEDULEDATE", typeof(DateTime));
            DataPresence.Columns.Add("LOADID", typeof(int));
            DataPresence.Columns.Add("LASTAGENT", typeof(int));
            DataPresence.Columns.Add("LASTQCODE", typeof(int));
            DataPresence.Columns.Add("FIRSTHANDLINGDATE", typeof(DateTime));
            DataPresence.Columns.Add("LASTHANDLINGDATE", typeof(DateTime));
            DataPresence.Columns.Add("DAILYCOUNTER", typeof(int));
            DataPresence.Columns.Add("TOTALCOUNTER", typeof(int));
            DataPresence.Columns.Add("BUSYSIGNALCOUNTER", typeof(int));
            DataPresence.Columns.Add("NOANSWERCOUNTER", typeof(int));
            DataPresence.Columns.Add("ANSWERMACHINECOUNTER", typeof(int));
            DataPresence.Columns.Add("FAXCOUNTER", typeof(int));
            DataPresence.Columns.Add("INVGENREASONCOUNTER", typeof(int));
            DataPresence.Columns.Add("PRIORITY", typeof(int));
            DataPresence.Columns.Add("CAPTURINGAGENT", typeof(int));
            DataPresence.Columns.Add("PHONE1", typeof(string));
            DataPresence.Columns.Add("PHONE2", typeof(string));
            DataPresence.Columns.Add("PHONE3", typeof(string));
            DataPresence.Columns.Add("PHONE4", typeof(string));
            DataPresence.Columns.Add("PHONE5", typeof(string));
            DataPresence.Columns.Add("PHONE6", typeof(string));
            DataPresence.Columns.Add("PHONE7", typeof(string));
            DataPresence.Columns.Add("PHONE8", typeof(string));
            DataPresence.Columns.Add("PHONE9", typeof(string));
            DataPresence.Columns.Add("PHONE10", typeof(string));
            DataPresence.Columns.Add("PHONEDESC1", typeof(int));
            DataPresence.Columns.Add("PHONEDESC2", typeof(int));
            DataPresence.Columns.Add("PHONEDESC3", typeof(int));
            DataPresence.Columns.Add("PHONEDESC4", typeof(int));
            DataPresence.Columns.Add("PHONEDESC5", typeof(int));
            DataPresence.Columns.Add("PHONEDESC6", typeof(int));
            DataPresence.Columns.Add("PHONEDESC7", typeof(int));
            DataPresence.Columns.Add("PHONEDESC8", typeof(int));
            DataPresence.Columns.Add("PHONEDESC9", typeof(int));
            DataPresence.Columns.Add("PHONEDESC10", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS1", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS2", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS3", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS4", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS5", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS6", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS7", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS8", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS9", typeof(int));
            DataPresence.Columns.Add("PHONESTATUS10", typeof(int));
            DataPresence.Columns.Add("PHONETIMEZONEID", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID1", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID2", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID3", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID4", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID5", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID6", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID7", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID8", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID9", typeof(string));
            DataPresence.Columns.Add("PHONETIMEZONEID10", typeof(string));
            DataPresence.Columns.Add("CURRENTPHONE", typeof(int));
            DataPresence.Columns.Add("CURRENTPHONECOUNTER", typeof(int));
            DataPresence.Columns.Add("TIMEZONEID", typeof(string));
            DataPresence.Columns.Add("COMMENTS", typeof(string));
            DataPresence.Columns.Add("RDATE", typeof(DateTime));
            DataPresence.Columns.Add("CUSTOMDATA1", typeof(string));
            DataPresence.Columns.Add("CUSTOMDATA2", typeof(string));
            DataPresence.Columns.Add("CUSTOMDATA3", typeof(string));
            DataPresence.Columns.Add("CALLERID", typeof(string));
            DataPresence.Columns.Add("CALLERNAME", typeof(string));
            #endregion

            return DataPresence;
        }
        #endregion

        #region [ Columns Data Invalid Phone ]
        private DataTable ColumnsDataInvalidPhone(DataTable InvalidPhoneData)
        {
            #region [ Column Mappings => InvalidPhoneData ]
            InvalidPhoneData.Columns.Add("ID", typeof(int));
            InvalidPhoneData.Columns.Add("InputType", typeof(string));
            InvalidPhoneData.Columns.Add("File", typeof(string));
            InvalidPhoneData.Columns.Add("Received", typeof(DateTime));
            InvalidPhoneData.Columns.Add("Command", typeof(string));
            InvalidPhoneData.Columns.Add("SourceID", typeof(int));
            InvalidPhoneData.Columns.Add("ServiceID", typeof(int));
            InvalidPhoneData.Columns.Add("LoadID", typeof(int));
            InvalidPhoneData.Columns.Add("CallerName", typeof(string));
            InvalidPhoneData.Columns.Add("PH1", typeof(string));
            InvalidPhoneData.Columns.Add("PH2", typeof(string));
            InvalidPhoneData.Columns.Add("PH3", typeof(string));
            InvalidPhoneData.Columns.Add("PH4", typeof(string));
            InvalidPhoneData.Columns.Add("PH5", typeof(string));
            InvalidPhoneData.Columns.Add("PH6", typeof(string));
            InvalidPhoneData.Columns.Add("PH7", typeof(string));
            InvalidPhoneData.Columns.Add("PH8", typeof(string));
            InvalidPhoneData.Columns.Add("PH9", typeof(string));
            InvalidPhoneData.Columns.Add("PH10", typeof(string));
            #endregion

            return InvalidPhoneData;
        }
        #endregion
    }
}
