#region [ Using ]
using InovoCIM.Data.Entities;
using InovoCIM.Data.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
#endregion

namespace InovoCIM.Business
{
    public class DataFilePriority
    {
        public readonly LogRepository LogRepo = new LogRepository();
        public readonly SQLRepository SqlRepo = new SQLRepository();
        public readonly EmailRepository MailRepo = new EmailRepository();

        #region [ Master Controller ]
        public async Task<string> MasterController()
        {
            string PriorityRun = "Not Completed";
            try
            {
                DateTime start = DateTime.Now;
                StatsPriorityModel.Start = start;
                LogRepo.Log("\nData File Priority Start");

                List<PriorityServices> services = await SqlRepo.GetPriorityServices();
                foreach (var service in services)
                {
                    await ManagePriority(service);
                    PriorityRun = "Yes - Priority Completed";
                }

                DateTime end = DateTime.Now;
                TimeSpan runtime = (end - start);
                LogRepo.Log("\nData File Priority End : " + runtime.ToString());

                int result = await ClosePriority();
                return PriorityRun;
            }
            catch (Exception ex)
            {
                LogRepo.Log(ex.Message.ToString());
                await MailRepo.SendError("Priority Processing Failed", ex.Message.ToString() + " => Please view log for more information");
                return PriorityRun;
            }
        }
        #endregion

        //---------------------------------------------------------------------------//

        #region [Private] - [ Manage Priority ]
        private async Task<int> ManagePriority(PriorityServices service)
        {
            DateTime start = DateTime.Now;
            try
            {
                LogRepo.Log("Manage Priority => (Service: " + service.ServiceID.ToString() + " , Load: " + service.LoadID.ToString() + ")");

                bool IsDone = await GetPrioritySourceID(service.ServiceID, service.LoadID);
                if (IsDone)
                {
                    List<PriorityVariables> variables = await GetPriorityVariables(service.ServiceID, service.LoadID);
                    StatsPriorityModel.Services.Add(new ServicesModel { ServiceID = service.ServiceID, LoadID = service.LoadID, Total = variables.Count });

                    string Columns = ColumnFieldNames(variables);
                    string OrderBy = ColumnOrderBy(variables);

                    LogRepo.Log("Manage Priority => Columns (" + Columns + ")");
                    LogRepo.Log("Manage Priority => OrderBy (" + OrderBy + ")");

                    await GetPriorityRank(Columns, OrderBy);
                    await UpdatePriority();

                    DateTime end = DateTime.Now;
                    TimeSpan runtime = (end - start);
                    LogRepo.Log("Manage Priority => (Service: " + service.ServiceID.ToString() + " , Load: " + service.LoadID.ToString() + ") End = " + runtime.ToString());

                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                await MailRepo.SendError("Unable to Manage the Priorities", ex.Message.ToString() + " => Please view log for more information");
                return 0;
            }
        }
        #endregion

        #region [Private] - [ Get Priority SourceID ]
        private async Task<bool> GetPrioritySourceID(int ServiceID, int LoadID)
        {
            DataTable TempData = new DataTable();
            TempData.Columns.Add("ID", typeof(int));
            TempData.Columns.Add("SOURCEID", typeof(int));

            string query = @"SELECT [ID],[SOURCEID] FROM [PREP].[PCO_OUTBOUNDQUEUE] WHERE [STATUS] <> 99 AND [SERVICEID] = " + ServiceID + " AND [LOADID] = " + LoadID;

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

                using (var conn = new SqlConnection(ViewModel.dbInovoCIM))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = "TRUNCATE TABLE [DataSourceList]";
                        await cmd.ExecuteNonQueryAsync();
                    }
                    conn.Close();
                }

                SqlRepo.SqlBulkSourceData(TempData);
                StatsPriorityModel.Total += TempData.Rows.Count;

                TempData.Clear();
                return true;
            }
            catch (Exception ex)
            {
                await MailRepo.SendError("Unable to get the Data Source List for Priorities", ex.Message.ToString() + " => Please view log for more information");
                return false;
            }
        }
        #endregion

        #region [Private] - [ Get Priority Variables ]
        private async Task<List<PriorityVariables>> GetPriorityVariables(int ServiceID, int LoadID)
        {
            List<PriorityVariables> model = new List<PriorityVariables>();
            DataTable TempData = new DataTable();
            string query = @"SELECT [T].[ServiceID],[T].[LoadID],[V].[Field],[V].[Sort],[V].[Rank]
                             FROM [dbo].[TemplatePriority] [T]
                             LEFT JOIN [dbo].[TemplatePriorityVariable] [V] ON [T].[ID] = [V].[TemplatePriorityID]
                             WHERE [T].[Status] = 1 AND [T].[ServiceID] = " + ServiceID + " AND [T].[LoadID] = " + LoadID;

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

                var RuleRepo = new RuleRepository();

                for (int i = 0; i < TempData.Rows.Count; i++)
                {
                    string fieldName = RuleRepo.RuleFieldName(int.Parse(TempData.Rows[i]["Field"].ToString(), CultureInfo.InvariantCulture));
                    string sortName = RuleRepo.RuleSortName(int.Parse(TempData.Rows[i]["Sort"].ToString(), CultureInfo.InvariantCulture));

                    model.Add(new PriorityVariables
                    {
                        ServiceID = int.Parse(TempData.Rows[i]["ServiceID"].ToString(), CultureInfo.InvariantCulture),
                        LoadID = int.Parse(TempData.Rows[i]["LoadID"].ToString(), CultureInfo.InvariantCulture),
                        Field = int.Parse(TempData.Rows[i]["Field"].ToString(), CultureInfo.InvariantCulture),
                        FieldName = fieldName,
                        Sort = int.Parse(TempData.Rows[i]["Sort"].ToString(), CultureInfo.InvariantCulture),
                        SortName = sortName,
                        Rank = int.Parse(TempData.Rows[i]["Rank"].ToString(), CultureInfo.InvariantCulture)
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

        #region [Private] - [ Get Priority Rank ]
        private async Task GetPriorityRank(string Columns, string OrderBy)
        {
            try
            {
                DataTable TempData = new DataTable();
                TempData.Columns.Add("SourceID", typeof(int));
                TempData.Columns.Add("NewPriority", typeof(int));

                string query = @"SELECT [SourceID],ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [NewPriority] FROM (SELECT ROW_NUMBER() OVER (PARTITION BY [SourceID] ORDER BY [ACTIONED] DESC) AS [ROW],[SourceID]," + Columns + " FROM [dbo].[DataRequestDone] " +
                               @"WHERE [SOURCEID] IN (SELECT [SOURCEID] FROM [DataSourceList])) AS X WHERE [ROW] = 1 ORDER BY " + OrderBy;

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

                TempData.Columns["NewPriority"].ColumnName = "Priority";

                using (var conn = new SqlConnection(ViewModel.dbPresence))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = "TRUNCATE TABLE [PREP].[xPriorityRank]";
                        await cmd.ExecuteNonQueryAsync();
                    }
                    conn.Close();
                }

                SqlRepo.SqlBulkPriorityRankData(TempData);
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region [Private] - [ Update Priority ]
        private async Task UpdatePriority()
        {
            try
            {
                string query = @"UPDATE [PREP].[PCO_OUTBOUNDQUEUE]
                                 SET [PRIORITY] = ([X].[PRIORITY] + 1),[RDATE] = GETDATE()
                                 FROM [PREP].[PCO_OUTBOUNDQUEUE][A]
                                 INNER JOIN [PREP].[xPriorityRank] [X] ON [A].[SOURCEID] = [X].[SourceID]
                                 WHERE [A].[SERVICEID] NOT IN " + ViewModel.ExcludeServices;

                using (var conn = new SqlConnection(ViewModel.dbPresence))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        int Records = cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region [Private] - [ Close Priority ]
        private async Task<int> ClosePriority()
        {
            var EmailRepo = new EmailRepository();
            try
            {
                StatsPriorityModel.End = DateTime.Now;

                await EmailRepo.SendReportPriority();

                StatsPriorityModel.Start = DateTime.Now;
                StatsPriorityModel.End = DateTime.Now;

                StatsPriorityModel.Total = 0;
                StatsPriorityModel.Services.Clear();

                return 1;
            }
            catch (Exception ex)
            {
                LogRepo.Log("ClosePriority() - Error => " + ex.Message.ToString());
                return 0;
            }
        }
        #endregion

        //---------------------------------------------------------------------------//

        #region [ Column Field Names ]
        private string ColumnFieldNames(List<PriorityVariables> variables)
        {
            string Columns = "";
            for (int x = 0; x < variables.Count; x++)
            {
                Columns += variables[x].FieldName + ",";
            }
            return Columns = Columns.Remove(Columns.Length - 1);
        }
        #endregion

        #region [ Column Order By ]
        private string ColumnOrderBy(List<PriorityVariables> variables)
        {
            string OrderBy = "";
            for (int x = 0; x < variables.Count; x++)
            {
                OrderBy += variables[x].FieldName + " " + variables[x].SortName + ",";
            }
            return OrderBy = OrderBy.Remove(OrderBy.Length - 1);
        }
        #endregion
    }
}