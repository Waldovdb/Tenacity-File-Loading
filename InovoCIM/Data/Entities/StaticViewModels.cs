#region [ Using ]
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace InovoCIM.Data.Entities
{
    #region [ ViewModel ]
    public static class ViewModel
    {
        public static string FTP { get; set; }
        public static string Input { get; set; }
        public static string Output { get; set; }
        public static string Complete { get; set; }
        public static string Report { get; set; }

        public static string FileColumns => "ID,FK,InputType,File,Status,Retry,LastRetry,Received,Actioned,Command,ServiceID,LoadID,CampaignNo,SourceID,CallerName,Phone1,Phone2,Phone3,Phone4,Phone5,Phone6,Phone7,Phone8,Phone9,Phone10,Comments,CustomData1,CustomData2,CustomData3,CallerID,AgentLogin,ScheduleDate,Priority,ClientTitle,ClientName,ClientSurname,ClientIDNumber,ClientGender,ClientEmail,CampaignTypeCode,CDStatusNo,RCStatusNo,Product,BalanceAmount,CurrentDueAmount,OTBAmount,PastDueAmount,TotalDueAmount,CollectionStatus,CampaignDescription,AccountStatus,OriginCode,Block1,Block2,CardNo,CampaignReason,CreditUsage,DebitOrderDay,DebitOrderFlag,CompanyName,HomeCity,HomeCode,HomeProvince,HomeSuburb,CD1,CD2,CD3,CD4,CD5,CD6,RC1,RC2,RC3,RC4,RC5,RC6,LastPaymentAmt,LastPaymentDate,LastPurchaseDate,MonthsOpen,Occupation,ProductNo,RetailerGroup,BccRiskBand,FieldsToModify,IPAddress,CurrencySymbol,PayAtCode,PayAtUrl";
        public static string FileHeader => "SERVICE ID~LOAD ID~CAMPAIGN NO~CAMPAIGN ACCOUNT NO~REQUEST TYPE~ACCOUNT NO~PRIORITY~CAMPAIGN TYPE CODE~CD STATUS NO~RC STATUS NO~PRODUCT~TITLE~FIRST NAME~LAST NAME~INITIALS~GENDER~CELL PHONE~HOME PHONE~WORK PHONE~BALANCE AMT~CURRENT DUE AMT~OTB AMT~PAST DUE AMT~TOTAL DUE AMT~COLLECTION STATUS~CAMPAIGN DESC~ADDITIONAL CELL~TEMP TEL~REFRENCE1 CELL PHONE~REFRENCE1 HOME PHONE~REFRENCE1 WORK PHONE~REFRENCE2 CELL PHONE~REFRENCE2 HOME PHONE~ACCOUNT STATUS~APPL DATE~ORIGIN CODE~BANK HOLDER NAME~BANK ACCOUNT NO~BANK ACCOUNT TYPE~BANK BRANCH NO~BANK CODE~BLOCK1~BLOCK2~CARD NO~CAMPAIGN STATUS DATE~CAMPAIGN REASON~CREDIT USAGE~DEBIT ORDER DAY~DEBIT ORDER FLAG~EMAIL~COMPANY NAME~HOME CITY~HOME LINE1~HOME LINE2~HOME CODE~HOME PROVINCE~HOME SUBURB~ID NO~CD1~CD2~CD3~CD4~CD5~CD6~RC1~RC2~RC3~RC4~RC5~RC6~LAST PAYMENT AMT~LAST PAYMENT DATE~LAST PURCHASE DATE~MONTHS OPEN~OCCUPATION~POSTAL CITY~POSTAL LINE1~POSTAL LINE2~POSTAL CODE~POSTAL PROVINCE~POSTAL SUBURB~COUNTRY~PRODUCT NO~CAMPAIGN ACTION DATE~AGENT USERNAME~ACTION BRINGUP DATE~DEPT MANAGER~GEN MANAGER~PTP RECEIVED AMT~PTP QUALIFY AMT~TEAM MANAGER~PTP AMT~RETAILER GROUP~BCC RISK BAND~PAY DAY~SILO NO~SILO AGENT~CURRENCY SYMBOL~PAYAT SOURCE ID~PAYAT URL";

        public static string FileHeaderTOne => "Action~Type~Call_Action~Call_Type~Appl_No~Source_ID~Origin_Code~Product_Desc~Referral_Indicator~Country_Code~Retailer_Code~Campaign_No~Campaign_Description~Title~Name~Surname~ID_No~Cell_Phone~Work_Phone~Home_Phone~Add_Cell_Phone~Temp_Phone~Ref1_Cell_Phone~Ref1_Work_Phone~Ref1_Home_Phone~Ref2_Cell_Phone~Ref2_Home_Phone~Email";

        public static string dbInovoCIM { get; set; }
        public static string dbPresence { get; set; }

        public static string ExcludeServices { get; set; }

        public static string InternalDisplay { get; set; }
        public static string InternalFrom { get; set; }
        public static string InternalServer { get; set; }
        public static string InternalPort { get; set; }
        public static string InternalUsername { get; set; }
        public static string InternalPassword { get; set; }

        public static string ExternalDisplay { get; set; }
        public static string ExternalFrom { get; set; }
        public static string ExternalServer { get; set; }
        public static string ExternalPort { get; set; }
        public static string ExternalUsername { get; set; }
        public static string ExternalPassword { get; set; }

        public static List<EmailModel> EmailReports = new List<EmailModel>();
        public static List<EmailModel> EmailErrors = new List<EmailModel>();
        public static List<EmailModel> EmailMonitors = new List<EmailModel>();

        public static string URL { get; set; }
        public static string JSON { get; set; }
        public static string APIKey { get; set; }
    }
    #endregion

    #region [ EmailModel ]
    public class EmailModel
    {
        public string Email { get; set; }
    }
    #endregion

    #region [ StatsFileModel ]
    public static class StatsFileModel
    {
        public static string FileName { get; set; }

        public static DateTime Start { get; set; }
        public static DateTime End { get; set; }

        public static int Total { get; set; }

        public static int FormatPass { get; set; }
        public static int FormatFail { get; set; }

        public static int QueryRmvPass { get; set; }
        public static int QueryRmvFail { get; set; }

        public static int QueryAddPass { get; set; }
        public static int QueryAddFail { get; set; }

        public static int QueryAddPresenceFail { get; set; }
        public static int QueryAddInvalidPhone { get; set; }
        public static int QueryAddNotDefined { get; set; }
    }
    #endregion

    #region [ StatsPriorityModel ]
    public static class StatsPriorityModel
    {
        public static DateTime Start { get; set; }
        public static DateTime End { get; set; }

        public static int Total { get; set; }

        public static List<ServicesModel> Services = new List<ServicesModel>();
    }
    #endregion

    #region [ ServicesModel ]
    public class ServicesModel
    {
        public int ServiceID { get; set; }
        public int LoadID { get; set; }
        public int Total { get; set; }
    }
    #endregion

    #region [ PhoneModel ]
    public class PhoneModel
    {
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Phone3 { get; set; }
        public string Phone4 { get; set; }
        public string Phone5 { get; set; }
        public string Phone6 { get; set; }
        public string Phone7 { get; set; }
        public string Phone8 { get; set; }
        public string Phone9 { get; set; }
        public string Phone10 { get; set; }
    }
    #endregion

    #region [ PriorityServices ]
    public class PriorityServices
    {
        public int ServiceID { get; set; }
        public int LoadID { get; set; }
    }
    #endregion

    #region [ PriorityVariables ]
    public class PriorityVariables
    {
        public int ServiceID { get; set; }
        public int LoadID { get; set; }
        public int Field { get; set; }
        public string FieldName { get; set; }
        public int Sort { get; set; }
        public string SortName { get; set; }
        public int Rank { get; set; }
    }
    #endregion

    #region [ PriorityRank ]
    public class PriorityRank
    {
        public int SourceID { get; set; }
        public int Priority { get; set; }
    }
    #endregion

    #region [ Data Sync ]
    public class DataSync
    {
        public int SourceID { get; set; }
        public string Command { get; set; }
        
    }
    #endregion
}