#region [ Using ]
using InovoCIM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace InovoCIM.Data.Repository
{
    public class EmailRepository
    {
        public readonly LogRepository LogRepo = new LogRepository();

        #region [ Send Report File ]
        public async Task<bool> SendReportFile(FileInfo file)
        {
            bool sendClient = false;
            try
            {
                TimeSpan runtime = (StatsFileModel.End - StatsFileModel.Start);

                double seconds = runtime.TotalSeconds;
                double recordsPerSeconds = Math.Round((StatsFileModel.Total / seconds), 0);

                string TempClient = File.ReadAllText(Directory.GetCurrentDirectory() + @"\EmailTemplates\ReportFile.html");

                TempClient = TempClient.Replace("#NOTIFICATION#", "<span style='color:blue;'>File Notification (End to End Completed)</span>");

                TempClient = TempClient.Replace("#FILENAME#", StatsFileModel.FileName);
                TempClient = TempClient.Replace("#RECEIVED#", StatsFileModel.Start.ToString("dd-MM-yyyy hh:mm:ss"));
                TempClient = TempClient.Replace("#TOTALLINES#", StatsFileModel.Total.ToString());

                TempClient = TempClient.Replace("#FORMAT_PASSED#", StatsFileModel.FormatPass.ToString());
                TempClient = TempClient.Replace("#FORMAT_FAILED#", StatsFileModel.FormatFail.ToString());

                TempClient = TempClient.Replace("#RMV_PASSED#", StatsFileModel.QueryRmvPass.ToString());
                TempClient = TempClient.Replace("#RMV_FAILED#", StatsFileModel.QueryRmvFail.ToString());

                TempClient = TempClient.Replace("#ADD_PASSED#", StatsFileModel.QueryAddPass.ToString());
                TempClient = TempClient.Replace("#ADD_FAILED#", StatsFileModel.QueryAddFail.ToString());
                TempClient = TempClient.Replace("#ADD_QUERY_FAILED#", StatsFileModel.QueryAddPresenceFail.ToString());
                TempClient = TempClient.Replace("#ADD_QUERY_INVALID#", StatsFileModel.QueryAddInvalidPhone.ToString());
                TempClient = TempClient.Replace("#ADD_QUERY_NOTDEFINED#", StatsFileModel.QueryAddNotDefined.ToString());

                TempClient = TempClient.Replace("#LINESPERSECOND#", recordsPerSeconds.ToString());
                TempClient = TempClient.Replace("#TOTALRUNTIME#", runtime.ToString());

                int TotalRemove = (StatsFileModel.QueryRmvPass + StatsFileModel.QueryRmvFail);
                int TotalAdd = (StatsFileModel.QueryAddPass + StatsFileModel.QueryAddFail + StatsFileModel.QueryAddPresenceFail + StatsFileModel.QueryAddInvalidPhone + StatsFileModel.QueryAddNotDefined);
                int TotalProcess = TotalRemove + TotalAdd;
                string healthCheck = (TotalProcess == StatsFileModel.Total) ? "<span style='color:blue;'> - Validation Passed</span>" : "<span style='color:red;'> - Validation Failed</span>";

                TempClient = TempClient.Replace("#TOTALPROCESSEDS#", TotalProcess.ToString() + healthCheck);
                TempClient = TempClient.Replace("#TOTALADD#", TotalAdd.ToString());
                TempClient = TempClient.Replace("#TOTALREMOVE#", TotalRemove.ToString());

                TempClient = TempClient.Replace("#DATE#", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture));

                sendClient = await SendEmailInternalAsync("Report", "InovoCIM Tenacity - File Notification", TempClient.ToString());
                LogRepo.Log("SendReportFile() - Feedback Report => " + sendClient.ToString());

                return sendClient;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendReportFile() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        #region [ Send Report File Rerun ]
        public async Task<bool> SendReportFileRerun(FileInfo file)
        {
            bool sendClient = false;
            try
            {
                TimeSpan runtime = (StatsFileModel.End - StatsFileModel.Start);

                double seconds = runtime.TotalSeconds;
                double recordsPerSeconds = Math.Round((StatsFileModel.Total / seconds), 0);

                string TempClient = File.ReadAllText(Directory.GetCurrentDirectory() + @"\EmailTemplates\ReportFile.html");

                TempClient = TempClient.Replace("#NOTIFICATION#", "<span style='color:red;'>File Notification (File to be processed again, to ensure sync matches)</span>");

                TempClient = TempClient.Replace("#FILENAME#", StatsFileModel.FileName);
                TempClient = TempClient.Replace("#RECEIVED#", StatsFileModel.Start.ToString("dd-MM-yyyy hh:mm:ss"));
                TempClient = TempClient.Replace("#TOTALLINES#", StatsFileModel.Total.ToString());

                TempClient = TempClient.Replace("#FORMAT_PASSED#", StatsFileModel.FormatPass.ToString());
                TempClient = TempClient.Replace("#FORMAT_FAILED#", StatsFileModel.FormatFail.ToString());

                TempClient = TempClient.Replace("#RMV_PASSED#", StatsFileModel.QueryRmvPass.ToString());
                TempClient = TempClient.Replace("#RMV_FAILED#", StatsFileModel.QueryRmvFail.ToString());

                TempClient = TempClient.Replace("#ADD_PASSED#", StatsFileModel.QueryAddPass.ToString());
                TempClient = TempClient.Replace("#ADD_FAILED#", StatsFileModel.QueryAddFail.ToString());
                TempClient = TempClient.Replace("#ADD_QUERY_FAILED#", StatsFileModel.QueryAddPresenceFail.ToString());
                TempClient = TempClient.Replace("#ADD_QUERY_INVALID#", StatsFileModel.QueryAddInvalidPhone.ToString());
                TempClient = TempClient.Replace("#ADD_QUERY_NOTDEFINED#", StatsFileModel.QueryAddNotDefined.ToString());

                TempClient = TempClient.Replace("#LINESPERSECOND#", recordsPerSeconds.ToString());
                TempClient = TempClient.Replace("#TOTALRUNTIME#", runtime.ToString());

                int TotalRemove = (StatsFileModel.QueryRmvPass + StatsFileModel.QueryRmvFail);
                int TotalAdd = (StatsFileModel.QueryAddPass + StatsFileModel.QueryAddFail + StatsFileModel.QueryAddPresenceFail + StatsFileModel.QueryAddInvalidPhone + StatsFileModel.QueryAddNotDefined);
                int TotalProcess = TotalRemove + TotalAdd;
                string healthCheck = (TotalProcess == StatsFileModel.Total) ? "<span style='color:blue;'> - Validation Passed</span>" : "<span style='color:red;'> - Validation Failed</span>";

                TempClient = TempClient.Replace("#TOTALPROCESSEDS#", TotalProcess.ToString() + healthCheck);
                TempClient = TempClient.Replace("#TOTALADD#", TotalAdd.ToString());
                TempClient = TempClient.Replace("#TOTALREMOVE#", TotalRemove.ToString());

                TempClient = TempClient.Replace("#DATE#", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture));

                sendClient = await SendEmailInternalAsync("Report", "InovoCIM Tenacity - File Notification (Not Completed)", TempClient.ToString());
                LogRepo.Log("SendReportFileRerun() - Feedback Report => " + sendClient.ToString());

                return sendClient;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendReportFileRerun() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        #region [ Send Report Priority ]
        public async Task<bool> SendReportPriority()
        {
            bool sendClient = false;
            try
            {
                TimeSpan runtime = (StatsPriorityModel.End - StatsPriorityModel.Start);

                double seconds = runtime.TotalSeconds;
                double recordsPerSeconds = Math.Round((StatsPriorityModel.Total / seconds), 0);

                string TempClient = File.ReadAllText(Directory.GetCurrentDirectory() + @"\EmailTemplates\ReportPriority.html");

                TempClient = TempClient.Replace("#STARTTIME#", StatsPriorityModel.Start.ToString("dd-MM-yyyy hh:mm:ss"));
                TempClient = TempClient.Replace("#ENDTIME#", StatsPriorityModel.End.ToString("dd-MM-yyyy hh:mm:ss"));
                TempClient = TempClient.Replace("#TOTALLINES#", StatsPriorityModel.Total.ToString());

                string ServiceInfo = "";
                foreach (var service in StatsPriorityModel.Services.OrderBy(x => x.ServiceID).ThenBy(x => x.LoadID))
                {
                    ServiceInfo += "<li>Service: " + service.ServiceID + " | Load: " + service.LoadID + " | Rules = " + service.Total + "</li>";
                }
                TempClient = TempClient.Replace("#SERVICES#", ServiceInfo);

                TempClient = TempClient.Replace("#LINESPERSECOND#", recordsPerSeconds.ToString());
                TempClient = TempClient.Replace("#TOTALRUNTIME#", runtime.ToString());
                TempClient = TempClient.Replace("#DATE#", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture));

                sendClient = await SendEmailInternalAsync("Report", "InovoCIM Tenacity - Priority Notification", TempClient.ToString());
                LogRepo.Log("SendReportPriority() - Feedback Report => " + sendClient.ToString());

                return sendClient;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendReportPriority() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        #region [ Send Report Excel ]
        public async Task<bool> SendReportExcel(string heading, string message, List<string> files)
        {
            bool sendClient = false;
            try
            {
                string TempClient = File.ReadAllText(Directory.GetCurrentDirectory() + @"\EmailTemplates\ReportExcel.html");

                TempClient = TempClient.Replace("#HEADING#", heading);
                TempClient = TempClient.Replace("#MESSAGE#", message);
                TempClient = TempClient.Replace("#DATE#", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture));

                sendClient = await SendEmailInternalAttachAsync("Report", "InovoCIM Tenacity - Report Notification", TempClient.ToString(), files);
                LogRepo.Log("SendReportExcel() - Feedback Report => " + sendClient.ToString());

                return sendClient;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendReportExcel() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        #region [ Send Error ]
        public async Task<bool> SendError(string heading, string message)
        {
            bool sendClient = false;
            try
            {
                string TempClient = File.ReadAllText(Directory.GetCurrentDirectory() + @"\EmailTemplates\Error.html");

                TempClient = TempClient.Replace("#HEADING#", heading);
                TempClient = TempClient.Replace("#MESSAGE#", message);
                TempClient = TempClient.Replace("#DATE#", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture));

                sendClient = await SendEmailInternalAsync("Error", "InovoCIM Tenacity - Error Notification", TempClient.ToString());
                LogRepo.Log("SendError() - Feedback Error => " + sendClient.ToString());

                return sendClient;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendError() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        #region [ Send Monitor ]
        public async Task<bool> SendMonitor()
        {
            bool sendClient = false;
            try
            {
                string TempClient = File.ReadAllText(Directory.GetCurrentDirectory() + @"\EmailTemplates\Monitor.html");

                TempClient = TempClient.Replace("#STARTTIME#", StatsPriorityModel.Start.ToString("dd-MM-yyyy hh:mm:ss"));
                TempClient = TempClient.Replace("#ENDTIME#", StatsPriorityModel.End.ToString("dd-MM-yyyy hh:mm:ss"));
                TempClient = TempClient.Replace("#TOTALLINES#", StatsPriorityModel.Total.ToString());


                TempClient = TempClient.Replace("#DATE#", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture));

                sendClient = await SendEmailInternalAsync("Monitor", "InovoCIM Tenacity - Monitor Notification", TempClient.ToString());
                LogRepo.Log("SendMonitor() - Feedback Monitor => " + sendClient.ToString());

                return sendClient;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendMonitor() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        //-------------------------------------------------------------------------------------//

        #region [Private] - [ Send Email Internal Async ]
        private async Task<bool> SendEmailInternalAsync(string group, string subject, string message)
        {
            try
            {
                var emailMsg = new MailMessage { From = new MailAddress(ViewModel.InternalFrom, ViewModel.InternalDisplay) };

                List<string> accounts = new List<string>();
                accounts = GetEmailList(group);
                foreach (string account in accounts) { emailMsg.To.Add(account); }

                emailMsg.Subject = subject;
                emailMsg.Body = message;
                emailMsg.IsBodyHtml = true;

                string path = Directory.GetCurrentDirectory() + @"\Content\inovo.png";
                LinkedResource img = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
                img.ContentId = "CompanyLogo";

                AlternateView av = AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html);
                av.LinkedResources.Add(img);
                emailMsg.AlternateViews.Add(av);

                using (var smtpClient = new SmtpClient(ViewModel.InternalServer))
                {
                    smtpClient.Port = int.Parse(ViewModel.InternalPort, CultureInfo.InvariantCulture);
                    smtpClient.EnableSsl = false;
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(ViewModel.InternalUsername, ViewModel.InternalPassword);
                    await smtpClient.SendMailAsync(emailMsg);
                }

                LogRepo.Log("SendEmailInternalAsync() - Email Sent !");
                return true;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendEmailInternalAsync() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        #region [Private] - [ Send Email Internal Attach Async ]
        private async Task<bool> SendEmailInternalAttachAsync(string group, string subject, string message, List<string> files)
        {
            try
            {
                var emailMsg = new MailMessage { From = new MailAddress(ViewModel.InternalFrom, ViewModel.InternalDisplay) };

                List<string> accounts = new List<string>();
                accounts = GetEmailList(group);
                foreach (string account in accounts) { emailMsg.To.Add(account); }

                emailMsg.Subject = subject;
                emailMsg.Body = message;
                emailMsg.IsBodyHtml = true;

                string path = Directory.GetCurrentDirectory() + @"\Content\inovo.png";
                LinkedResource img = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
                img.ContentId = "CompanyLogo";

                AlternateView av = AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html);
                av.LinkedResources.Add(img);
                emailMsg.AlternateViews.Add(av);

                foreach (string file in files)
                {
                    Attachment attach = new Attachment(file);
                    emailMsg.Attachments.Add(attach);
                }

                using (var smtpClient = new SmtpClient(ViewModel.InternalServer))
                {
                    smtpClient.Port = int.Parse(ViewModel.InternalPort, CultureInfo.InvariantCulture);
                    smtpClient.EnableSsl = false;
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(ViewModel.InternalUsername, ViewModel.InternalPassword);
                    await smtpClient.SendMailAsync(emailMsg);
                }

                LogRepo.Log("SendEmailInternalAttachAsync() - Email Sent !");
                return true;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendEmailInternalAttachAsync() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        #region [Private] - [ Send Email External Async ]
        private async Task<bool> SendEmailExternalAsync(string mailTo, string subject, string message)
        {
            try
            {
                var emailMsg = new MailMessage { From = new MailAddress(ViewModel.ExternalFrom, ViewModel.ExternalDisplay) };

                emailMsg.To.Add(mailTo);
                emailMsg.Subject = subject;
                emailMsg.Body = message;
                emailMsg.IsBodyHtml = true;

                string path = Directory.GetCurrentDirectory() + @"\Content\tenacity.png";
                LinkedResource img = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
                img.ContentId = "CompanyLogo";

                AlternateView av = AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html);
                av.LinkedResources.Add(img);
                emailMsg.AlternateViews.Add(av);

                using (var smtpClient = new SmtpClient(ViewModel.ExternalServer))
                {
                    smtpClient.Port = int.Parse(ViewModel.ExternalPort, CultureInfo.InvariantCulture);
                    smtpClient.EnableSsl = false;
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(ViewModel.ExternalUsername, ViewModel.ExternalPassword);
                    await smtpClient.SendMailAsync(emailMsg);
                }

                LogRepo.Log("SendEmailExternalAsync() - Email Sent !");
                return true;
            }
            catch (Exception ex)
            {
                LogRepo.Log("SendEmailExternalAsync() - Error => " + ex.Message.ToString());
                return false;
            }
        }
        #endregion

        //-------------------------------------------------------------------------------------//

        #region [ Get Email List ]
        private List<string> GetEmailList(string group)
        {
            List<string> accounts = new List<string>();
            accounts.Add("jnel@inovo.co.za");

            if (group == "Report")
            {
                foreach (var account in ViewModel.EmailReports) { accounts.Add(account.Email.ToString()); }
            }

            if (group == "Error")
            {
                foreach (var account in ViewModel.EmailErrors) { accounts.Add(account.Email.ToString()); }
            }

            if (group == "Monitor")
            {
                foreach (var account in ViewModel.EmailMonitors) { accounts.Add(account.Email.ToString()); }
            }

            return accounts;
        }
        #endregion
    }
}