#region [ Using ]
using InovoCIM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
#endregion

namespace InovoCIM.Data.Repository
{
    public class RuleRepository
    {
        #region [ Rule Load ID ]
        public int RuleLoadID(int LoadID)
        {
            if (LoadID == 1)
                return 1;

            if (LoadID == 2)
                return 3;

            if (LoadID == 3)
                return 3;

            return 0;
        }
        #endregion

        #region [ Rule Create Load ID ]
        public int RuleCreateLoadID(int LoadID,  string CampaignNo, string CampaignDescription, int ServiceID = 71)
        {
            try
            {
                string Description = CampaignNo + "_" + CampaignDescription + " " + DateTime.Now.ToString("MMMM yyyy");

                string query = @"  
                        BEGIN TRY
	                        INSERT INTO [PREP].[PCO_LOAD]
	                        ([SERVICEID],[LOADID],[STATUS],[DESCRIPTION],[RDATE],[RECORDCOUNT],[PRIORITYTYPE],[PRIORITYVALUE])
	                        VALUES
	                        (@SERVICEID,@LOADID,@STATUS,@DESCRIPTION,GETDATE(),0,0,0)
                        END TRY
                        BEGIN CATCH
                            -- DO NOTHING
                        END CATCH";

                using (var conn = new SqlConnection(ViewModel.dbPresence))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@SERVICEID", ServiceID);
                    cmd.Parameters.AddWithValue("@LOADID", LoadID);
                    cmd.Parameters.AddWithValue("@STATUS", "D");
                    cmd.Parameters.AddWithValue("@DESCRIPTION", Description);
                    cmd.ExecuteNonQuery();
                }

                return LoadID;
            }
            catch (Exception ex)
            {
                return LoadID;
            }
        }
        #endregion


        #region [ Rule Format Phone ]
        public PhoneModel RuleFormatPhone(PhoneModel model)
        {
            DataTable TempData = new DataTable();

            StringBuilder variables = new StringBuilder();
            #region [ Create all Variables to be passed ]
            variables.Append(string.Format("DECLARE @PH01 VARCHAR(30) = '{0}' ", model.Phone1));
            variables.Append(string.Format("DECLARE @PH02 VARCHAR(30) = '{0}' ", model.Phone2));
            variables.Append(string.Format("DECLARE @PH03 VARCHAR(30) = '{0}' ", model.Phone3));
            variables.Append(string.Format("DECLARE @PH04 VARCHAR(30) = '{0}' ", model.Phone4));
            variables.Append(string.Format("DECLARE @PH05 VARCHAR(30) = '{0}' ", model.Phone5));
            variables.Append(string.Format("DECLARE @PH06 VARCHAR(30) = '{0}' ", model.Phone6));
            variables.Append(string.Format("DECLARE @PH07 VARCHAR(30) = '{0}' ", model.Phone7));
            variables.Append(string.Format("DECLARE @PH08 VARCHAR(30) = '{0}' ", model.Phone8));
            variables.Append(string.Format("DECLARE @PH09 VARCHAR(30) = '{0}' ", model.Phone9));
            variables.Append(string.Format("DECLARE @PH10 VARCHAR(30) = '{0}' ", model.Phone10));
            #endregion

            string query = variables.ToString() + " " +
                            @"SELECT ISNULL([dbo].[CIMNumber](@PH01),'') AS 'PH01',ISNULL([dbo].[CIMNumber](@PH02),'') AS 'PH02',ISNULL([dbo].[CIMNumber](@PH03),'') AS 'PH03',
	                               ISNULL([dbo].[CIMNumber](@PH04),'') AS 'PH04',ISNULL([dbo].[CIMNumber](@PH05),'') AS 'PH05',ISNULL([dbo].[CIMNumber](@PH06),'') AS 'PH06',
	                               ISNULL([dbo].[CIMNumber](@PH07),'') AS 'PH07',ISNULL([dbo].[CIMNumber](@PH08),'') AS 'PH08',ISNULL([dbo].[CIMNumber](@PH09),'') AS 'PH09',
	                               ISNULL([dbo].[CIMNumber](@PH10),'') AS 'PH10'";

            try
            {
                using (var conn = new SqlConnection(ViewModel.dbInovoCIM))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = query;
                        SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        TempData.Load(reader);
                    }
                    conn.Close();
                }

                foreach (DataRow row in TempData.Rows)
                {
                    model.Phone1 = row["PH01"].ToString();
                    model.Phone2 = row["PH02"].ToString();
                    model.Phone3 = row["PH03"].ToString();
                    model.Phone4 = row["PH04"].ToString();
                    model.Phone5 = row["PH05"].ToString();
                    model.Phone6 = row["PH06"].ToString();
                    model.Phone7 = row["PH07"].ToString();
                    model.Phone8 = row["PH08"].ToString();
                    model.Phone9 = row["PH09"].ToString();
                    model.Phone10 = row["PH10"].ToString();
                }

                return model;
            }
            catch (Exception ex)
            {
                return model;
            }
        }
        #endregion

        #region [ Rule Phone Correct ]
        public string RulePhoneCorrect(PhoneModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Phone1) && string.IsNullOrEmpty(model.Phone2) && string.IsNullOrEmpty(model.Phone3) && string.IsNullOrEmpty(model.Phone4) && string.IsNullOrEmpty(model.Phone5) &&
                    string.IsNullOrEmpty(model.Phone6) && string.IsNullOrEmpty(model.Phone7) && string.IsNullOrEmpty(model.Phone8) && string.IsNullOrEmpty(model.Phone9) && string.IsNullOrEmpty(model.Phone10))
                {
                    return "No - All Numbers Invalid";
                }
                else
                {
                    return "Yes - Numbers are Valid";
                }
            }
            catch (Exception)
            {
                return "No - All Numbers Invalid";
            }
        }
        #endregion

        #region [ Rule Field Name ]
        public string RuleFieldName(int FieldInt)
        {
            try
            {
                switch (FieldInt)
                {
                    case 0:
                        return "Priority";
                    case 1:
                        return "CallerName";
                    case 2:
                        return "Phone1";
                    case 3:
                        return "Phone2";
                    case 4:
                        return "Phone3";
                    case 5:
                        return "Phone4";
                    case 6:
                        return "Phone5";
                    case 7:
                        return "Phone6";
                    case 8:
                        return "Phone7";
                    case 9:
                        return "Phone8";
                    case 10:
                        return "Phone9";
                    case 11:
                        return "Phone10";
                    case 12:
                        return "Comments";
                    case 13:
                        return "ScheduleDate";
                    case 14:
                        return "ClientTitle";
                    case 15:
                        return "ClientName";
                    case 16:
                        return "ClientSurname";
                    case 17:
                        return "ClientIDNumber";
                    case 18:
                        return "ClientEmail";
                    case 19:
                        return "CampaignTypeCode";
                    case 20:
                        return "CDStatusNo";
                    case 21:
                        return "RCStatusNo";
                    case 22:
                        return "Product";
                    case 23:
                        return "BalanceAmount";
                    case 24:
                        return "CurrentDueAmount";
                    case 25:
                        return "OTBAmount";
                    case 26:
                        return "PastDueAmount";
                    case 27:
                        return "TotalDueAmount";
                    case 28:
                        return "CollectionStatus";
                    case 29:
                        return "CampaignDescription";
                    case 30:
                        return "AccountStatus";
                    case 31:
                        return "OriginCode";
                    case 32:
                        return "Block1";
                    case 33:
                        return "Block2";
                    case 34:
                        return "CardNo";
                    case 35:
                        return "CampaignReason";
                    case 36:
                        return "CreditUsage";
                    case 37:
                        return "DebitOrderDay";
                    case 38:
                        return "DebitOrderFlag";
                    case 39:
                        return "CompanyName";
                    case 40:
                        return "HomeCity";
                    case 41:
                        return "HomeCode";
                    case 42:
                        return "HomeProvince";
                    case 43:
                        return "HomeSuburb";
                    case 44:
                        return "CD1";
                    case 45:
                        return "CD2";
                    case 46:
                        return "CD3";
                    case 47:
                        return "CD4";
                    case 48:
                        return "CD5";
                    case 49:
                        return "CD6";
                    case 50:
                        return "RC1";
                    case 51:
                        return "RC2";
                    case 52:
                        return "RC3";
                    case 53:
                        return "RC4";
                    case 54:
                        return "RC5";
                    case 55:
                        return "RC6";
                    case 56:
                        return "LastPaymentAmt";
                    case 57:
                        return "LastPaymentDate";
                    case 58:
                        return "LastPurchaseDate";
                    case 59:
                        return "MonthsOpen";
                    case 60:
                        return "Occupation";
                    case 61:
                        return "ProductNo";
                    case 62:
                        return "RetailerGroup";
                    case 63:
                        return "BccRiskBand";
                    case 64:
                        return "Currency";
                    default:
                        return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        #endregion

        #region [ Rule Sort Name ]
        public string RuleSortName(int SortInt)
        {
            try
            {
                switch (SortInt)
                {
                    case 1:
                        return "ASC";
                    case 2:
                        return "DESC";
                    default:
                        return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        #endregion
    }
}