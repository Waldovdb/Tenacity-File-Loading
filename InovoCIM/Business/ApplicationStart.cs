#region [ Using ]
using InovoCIM.Data.Entities;
using InovoCIM.Data.Repository;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
#endregion

namespace InovoCIM.Business
{
    public class ApplicationStart
    {
        public readonly LogRepository LogRepo = new LogRepository();

        #region [ Master Controller ]
        public bool MasterController()
        {
            var Settings = new SettingsRepository();
            try
            {
                bool IsActive = Settings.GetVariables();
                if (IsActive)
                {
                    LogRepo.Log("\nApplication Started : " + DateTime.Now.ToString("hh:mm:ss"));
                    bool IsSorted = ManageTables();
                    return IsSorted;
                }
                else
                {
                    LogRepo.Log("Could not read/update the settings.xml");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogRepo.Log(ex.Message.ToString());
                return false;
            }
        }
        #endregion

        //---------------------------------------------------------------------------//

        #region [Private] - [ Manage Tables ]
        private bool ManageTables()
        {
            try
            {
                #region [ db InovoCIM ] - [dbo].[DataSourceList]
                using (var conn = new SqlConnection(ViewModel.dbInovoCIM))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSourceList]') AND type in (N'U'))
                                            BEGIN
                                                CREATE TABLE [DataSourceList] ([ID] INT,[SOURCEID] INT) 
                                            END";
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                #endregion

                #region [ db InovoCIM ] - [dbo].[DataRequestInvalidPhone]
                using (var conn = new SqlConnection(ViewModel.dbInovoCIM))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataRequestInvalidPhone]') AND type in (N'U'))
                                            BEGIN
                                                CREATE TABLE [DataRequestInvalidPhone]
                                                (
	                                                [ID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),[InputType] VARCHAR(20),[File] VARCHAR(350),[Received] DATETIME,[Command] VARCHAR(80),[SourceID] INT,[ServiceID] INT,[LoadID] INT,[CallerName] VARCHAR(40),
	                                                [PH1] VARCHAR(30),[PH2] VARCHAR(30),[PH3] VARCHAR(30),[PH4] VARCHAR(30),[PH5] VARCHAR(30),[PH6] VARCHAR(30),[PH7] VARCHAR(30),[PH8] VARCHAR(30),[PH9] VARCHAR(30),[PH10] VARCHAR(30)
                                                )
                                            END";
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                #endregion

                #region [ db Presence ] - [PREP].[xPriorityRank]
                using (var conn = new SqlConnection(ViewModel.dbPresence))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = @"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[PREP].[xPriorityRank]') AND type in (N'U'))
                                            BEGIN
                                                CREATE TABLE [PREP].[xPriorityRank] ([SourceID] INT,[Priority] INT) 
                                            END";
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                LogRepo.Log(ex.Message.ToString());
                return false;
            }
        }
        #endregion
    }
}