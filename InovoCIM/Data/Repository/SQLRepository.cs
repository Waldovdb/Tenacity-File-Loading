#region [ Using ]
using InovoCIM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace InovoCIM.Data.Repository
{
    public class SQLRepository
    {
        public readonly LogRepository LogRepo = new LogRepository();

        #region [ Get Service Load Status ]
        public async Task<Dictionary<int, string>> GetServiceLoadStatus()
        {
            Dictionary<int, string> model = new Dictionary<int, string>();
            DataTable TempData = new DataTable();
            string query = @"SELECT [SERVICEID],[LOADID],[STATUS] FROM [PREP].[PCO_LOAD] WHERE [SERVICEID] NOT IN " + ViewModel.ExcludeServices;

            try
            {
                using (var conn = new SqlConnection(ViewModel.dbPresence))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = query;
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        TempData.Load(reader);
                    }
                    conn.Close();
                }

                for (int i = 0; i < TempData.Rows.Count; i++)
                {
                    string ServiceID = TempData.Rows[i]["SERVICEID"].ToString();
                    string LoadID = TempData.Rows[i]["LOADID"].ToString();
                    string Status = TempData.Rows[i]["STATUS"].ToString();
                    model.Add(i, ServiceID + "," + LoadID + "," + Status);
                }
                return model;
            }
            catch (Exception)
            {
                return model;
            }
        }
        #endregion

        #region [ Get Presence Source IDs ]
        public async Task<Dictionary<int, int>> GetPresenceSourceIDs()
        {
            Dictionary<int, int> model = new Dictionary<int, int>();
            DataTable TempData = new DataTable();
            string query = @"SELECT [ID],[SOURCEID] FROM [PREP].[PCO_OUTBOUNDQUEUE] WHERE [SERVICEID] NOT IN " + ViewModel.ExcludeServices;

            try
            {
                using (var conn = new SqlConnection(ViewModel.dbPresence))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = query;
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        TempData.Load(reader);
                    }
                    conn.Close();
                }

                for (int i = 0; i < TempData.Rows.Count; i++)
                {
                    int ID = int.Parse(TempData.Rows[i]["ID"].ToString(), CultureInfo.InvariantCulture);
                    int SourceID = int.Parse(TempData.Rows[i]["SOURCEID"].ToString(), CultureInfo.InvariantCulture);

                    model.Add(ID, SourceID);
                }
                return model;
            }
            catch (Exception)
            {
                return model;
            }
        }
        #endregion

        #region [ SqlBulk Data Request Done ]
        public void SqlBulkDataRequestDone(DataTable model)
        {
            try
            {
                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(ViewModel.dbInovoCIM))
                {
                    SqlBulk.DestinationTableName = "[dbo].[DataRequestDone]";
                    SqlBulk.BatchSize = 5000;
                    SqlBulk.WriteToServer(model);
                }
            }
            catch (Exception ex)
            {
                LogRepo.Log("SqlBulkDataRequestDone() => " + ex.Message.ToString());

                StringBuilder output = new StringBuilder();
                foreach (DataRow row in model.Rows)
                {
                    foreach (DataColumn col in model.Columns) { output.AppendFormat("{0}~", row[col].ToString()); }
                    output.AppendLine();
                }
                LogRepo.LogBulkDataRequestDoneFailed(output.ToString());
            }
        }
        #endregion

        #region [ SqlBulk Data Request Invalid Phone ]
        public void SqlBulkDataRequestInvalidPhone(DataTable model)
        {
            try
            {
                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(ViewModel.dbInovoCIM))
                {
                    SqlBulk.DestinationTableName = "[dbo].[DataRequestInvalidPhone]";
                    SqlBulk.BatchSize = 5000;
                    SqlBulk.WriteToServer(model);
                }
            }
            catch (Exception ex)
            {
                LogRepo.Log("SqlBulkDataRequestInvalidPhone() => " + ex.Message.ToString());

                StringBuilder output = new StringBuilder();
                foreach (DataRow row in model.Rows)
                {
                    foreach (DataColumn col in model.Columns) { output.AppendFormat("{0}~", row[col].ToString()); }
                    output.AppendLine();
                }
                LogRepo.LogBulkDataRequestInvalidPhoneFailed(output.ToString());
            }
        }
        #endregion

        #region [ SqlBulk Presence Data ]
        public void SqlBulkPresenceData(DataTable model)
        {
            try
            {
                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(ViewModel.dbPresence))
                {
                    SqlBulk.DestinationTableName = "[PREP].[PCO_OUTBOUNDQUEUE]";
                    SqlBulk.BatchSize = 5000;
                    SqlBulk.WriteToServer(model);
                }
            }
            catch (Exception ex)
            {
                LogRepo.Log("SqlBulkPresenceData() => " + ex.Message.ToString());

                StringBuilder output = new StringBuilder();
                foreach (DataRow row in model.Rows)
                {
                    foreach (DataColumn col in model.Columns) { output.AppendFormat("{0}~", row[col].ToString()); }
                    output.AppendLine();
                    StatsFileModel.QueryAddPresenceFail++;
                }
                LogRepo.LogBulkPresenceDataFailed(output.ToString());
            }
        }
        #endregion

        #region [ Get Priority Services ]
        public async Task<List<PriorityServices>> GetPriorityServices()
        {
            List<PriorityServices> model = new List<PriorityServices>();
            DataTable TempData = new DataTable();
            string query = @"SELECT DISTINCT [ServiceID],[LoadID] FROM [dbo].[TemplatePriority] WHERE [Status] = 1";

            try
            {
                using (var conn = new SqlConnection(ViewModel.dbInovoCIM))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = query;
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        TempData.Load(reader);
                    }
                    conn.Close();
                }

                for (int i = 0; i < TempData.Rows.Count; i++)
                {
                    model.Add(new PriorityServices
                    {
                        ServiceID = int.Parse(TempData.Rows[i]["ServiceID"].ToString()),
                        LoadID = int.Parse(TempData.Rows[i]["LoadID"].ToString())
                    });
                }
                return model;
            }
            catch (Exception)
            {
                return model;
            }
        }
        #endregion

        #region [ SqlBulk Source Data ]
        public void SqlBulkSourceData(DataTable model)
        {
            try
            {
                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(ViewModel.dbInovoCIM))
                {
                    SqlBulk.DestinationTableName = "[DataSourceList]";
                    SqlBulk.BatchSize = 5000;
                    SqlBulk.WriteToServer(model);
                }
            }
            catch (Exception ex)
            {
                LogRepo.Log("SqlBulkSourceData() => " + ex.Message.ToString());
            }
        }
        #endregion

        #region [ SqlBulk Priority Rank Data ]
        public void SqlBulkPriorityRankData(DataTable model)
        {
            try
            {
                using (SqlBulkCopy SqlBulk = new SqlBulkCopy(ViewModel.dbPresence))
                {
                    SqlBulk.DestinationTableName = "[PREP].[xPriorityRank]";
                    SqlBulk.BatchSize = 5000;
                    SqlBulk.WriteToServer(model);
                }
            }
            catch (Exception ex)
            {
                LogRepo.Log("SqlBulkPriorityRankData() => " + ex.Message.ToString());
            }
        }
        #endregion

        #region [ Get Priority Services ]
        //public async Task<List<PriorityServices>> GetPriorityServices()
        //{
        //    List<PriorityServices> model = new List<PriorityServices>();
        //    DataTable TempData = new DataTable();
        //    string query = @"SELECT DISTINCT [ServiceID],[LoadID] FROM [dbo].[TemplatePriority] WHERE [Status] = 1";

        //    try
        //    {
        //        using (var conn = new SqlConnection(ViewModel.dbInovoCIM))
        //        {
        //            await conn.OpenAsync();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandTimeout = 0;
        //                cmd.CommandText = query;
        //                SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        //                TempData.Load(reader);
        //            }
        //            conn.Close();
        //        }

        //        for (int i = 0; i < TempData.Rows.Count; i++)
        //        {
        //            model.Add(new PriorityServices
        //            {
        //                ServiceID = int.Parse(TempData.Rows[i]["ServiceID"].ToString()),
        //                LoadID = int.Parse(TempData.Rows[i]["LoadID"].ToString())
        //            });
        //        }
        //        return model;
        //    }
        //    catch (Exception)
        //    {
        //        return model;
        //    }
        //}
        #endregion
    }
}