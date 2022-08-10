#region [ Using ]
using InovoCIM.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
#endregion

namespace InovoCIM.Data.Repository
{
    public class SettingsRepository
    {
        private readonly string SettingsFile = AppDomain.CurrentDomain.BaseDirectory + "\\InovoCIM.Settings.Main.xml";
        private readonly string SettingsFileProd = AppDomain.CurrentDomain.BaseDirectory + "\\InovoCIM.Settings.Prod.xml";
        private readonly string SettingsFileQA = AppDomain.CurrentDomain.BaseDirectory + "\\InovoCIM.Settings.QA.xml";

        #region [ Get Variables ]
        public bool GetVariables()
        {
            var LogRepo = new LogRepository();
            try
            {
                if (!File.Exists(SettingsFile))
                {
                    CreateXML();
                    CreateXMLProd();
                    CreateXMLQA();
                    return false;
                }
                else
                {
                    ReadXML();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        //--------------------------------------------//

        #region [ Create XML ]
        private void CreateXML()
        {
            try
            {
                XmlWriterSettings xml = new XmlWriterSettings { Async = false, Indent = true, IndentChars = "\t" };

                using (XmlWriter writer = XmlWriter.Create(SettingsFile, xml))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Settings");

                    writer.WriteStartElement("File");
                    writer.WriteElementString("FTP", @"C:\inovo\OmniCIM_Input\");
                    writer.WriteElementString("Input", @"C:\0.InovoFileProd\1.Working\");
                    writer.WriteElementString("Output", @"C:\0.InovoFileProd\2.FileLogs\");
                    writer.WriteElementString("Complete", @"C:\0.InovoFileProd\3.Completed\");
                    writer.WriteElementString("Report", @"C:\0.InovoFileProd\4.Reports\");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Database");
                    writer.WriteElementString("dbInovoCIM", @"Data Source=localhost\SQLEXPRESS;Initial Catalog=TenacityCIM_Prod_P4; Integrated Security=True;");
                    writer.WriteElementString("dbPresence", @"Data Source=localhost\SQLEXPRESS;Initial Catalog=SQLPR1; Integrated Security=True;");
                    writer.WriteEndElement();

                    writer.WriteStartElement("EmailInternal");
                    writer.WriteElementString("Display", "InovoCIM Tenacity");
                    writer.WriteElementString("From", "inovocim@tenacity.co.za");
                    writer.WriteElementString("Server", "172.18.101.44");
                    writer.WriteElementString("Port", "25");
                    writer.WriteElementString("Username", "omni@ten.local");
                    writer.WriteElementString("Password", "g8rF1eld");
                    writer.WriteEndElement();

                    writer.WriteStartElement("EmailExternal");
                    writer.WriteElementString("Display", "Tenacity Accounts");
                    writer.WriteElementString("From", "accounts2@tenacityinc.co.za");
                    writer.WriteElementString("Server", "172.18.101.44");
                    writer.WriteElementString("Port", "25");
                    writer.WriteElementString("Username", "accounts2@ten.local");
                    writer.WriteElementString("Password", "g8rF1eld");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Reports");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Errors");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Monitor");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Integration");
                    writer.WriteElementString("URL", @"https://ten2.pdws.co.za/");
                    writer.WriteElementString("JSON", @"https://ten2.pdws.co.za/JSON_Req");
                    writer.WriteElementString("APIKey", "33E5F1F1-6EC5-4791-817E-8B97A7601F2C");
                    writer.WriteEndElement();

                    writer.WriteStartElement("ExcludeService");
                    writer.WriteElementString("Services", "(50,51,52,53,54,55,990)");
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Create XML Prod ]
        private void CreateXMLProd()
        {
            try
            {
                XmlWriterSettings xml = new XmlWriterSettings { Async = false, Indent = true, IndentChars = "\t" };

                using (XmlWriter writer = XmlWriter.Create(SettingsFileProd, xml))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Settings");

                    writer.WriteStartElement("File");
                    writer.WriteElementString("FTP", @"C:\inovo\OmniCIM_Input\");
                    writer.WriteElementString("Input", @"C:\0.InovoFileProd\1.Working\");
                    writer.WriteElementString("Output", @"C:\0.InovoFileProd\2.FileLogs\");
                    writer.WriteElementString("Complete", @"C:\0.InovoFileProd\3.Completed\");
                    writer.WriteElementString("Report", @"C:\0.InovoFileProd\4.Reports\");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Database");
                    writer.WriteElementString("dbInovoCIM", @"Data Source=localhost\SQLEXPRESS;Initial Catalog=TenacityCIM_Prod_P4; Integrated Security=True");
                    writer.WriteElementString("dbPresence", @"Data Source=localhost\SQLEXPRESS;Initial Catalog=SQLPR1; Integrated Security=True");
                    writer.WriteEndElement();

                    writer.WriteStartElement("EmailInternal");
                    writer.WriteElementString("Display", "Tenacity Production");
                    writer.WriteElementString("From", "inovocim@tenacity.co.za");
                    writer.WriteElementString("Server", "172.18.101.44");
                    writer.WriteElementString("Port", "25");
                    writer.WriteElementString("Username", "omni@ten.local");
                    writer.WriteElementString("Password", "g8rF1eld");
                    writer.WriteEndElement();

                    writer.WriteStartElement("EmailExternal");
                    writer.WriteElementString("Display", "Tenacity Accounts");
                    writer.WriteElementString("From", "accounts2@tenacityinc.co.za");
                    writer.WriteElementString("Server", "172.18.101.44");
                    writer.WriteElementString("Port", "25");
                    writer.WriteElementString("Username", "accounts2@ten.local");
                    writer.WriteElementString("Password", "g8rF1eld");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Reports");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteElementString("Email", "MARCELIJ@tenacityinc.co.za");
                    writer.WriteElementString("Email", "JONATHAG@tenacityinc.co.za");
                    writer.WriteElementString("Email", "WILLIAMA@tenacityinc.co.za");
                    writer.WriteElementString("Email", "GERHARDL@tenacityinc.co.za");
                    writer.WriteElementString("Email", "kobusk@pepkorit.com");
                    writer.WriteElementString("Email", "diallersupport@tenacityinc.co.za");
                    writer.WriteElementString("Email", "TENIT@tenacityinc.co.za");
                    writer.WriteElementString("Email", "ServiceDesk@tenacityinc.co.za");
                    writer.WriteElementString("Email", "AUDREYR@tenacityinc.co.za");
                    writer.WriteElementString("Email", "ZANEM@tenacityinc.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Errors");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Monitor");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Integration");
                    writer.WriteElementString("URL", @"https://ten2.pdws.co.za/");
                    writer.WriteElementString("JSON", @"https://ten2.pdws.co.za/JSON_Req");
                    writer.WriteElementString("APIKey", "33E5F1F1-6EC5-4791-817E-8B97A7601F2C");
                    writer.WriteEndElement();

                    writer.WriteStartElement("ExcludeService");
                    writer.WriteElementString("Services", "(50,51,52,53,54,55,990)");
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Create XML QA ]
        private void CreateXMLQA()
        {
            try
            {
                XmlWriterSettings xml = new XmlWriterSettings { Async = false, Indent = true, IndentChars = "\t" };

                using (XmlWriter writer = XmlWriter.Create(SettingsFileQA, xml))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Settings");

                    writer.WriteStartElement("File");
                    writer.WriteElementString("FTP", @"C:\inovo\InovoCIM_UAT_Input\");
                    writer.WriteElementString("Input", @"C:\0.InovoFileQA\1.Working\");
                    writer.WriteElementString("Output", @"C:\0.InovoFileQA\2.FileLogs\");
                    writer.WriteElementString("Complete", @"C:\0.InovoFileQA\3.Completed\");
                    writer.WriteElementString("Report", @"C:\0.InovoFileQA\4.Reports\");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Database");
                    writer.WriteElementString("dbInovoCIM", @"Data Source=172.18.101.84\INST02;Initial Catalog=TenacityCIM_QA_P4;User Id=INOVOCIM;Password=S0lv1tNT;");
                    writer.WriteElementString("dbPresence", @"Data Source=172.18.101.36\DEV;Initial Catalog=SQLPR1_DEV;User Id=PREP;Password=PREP;");
                    writer.WriteEndElement();

                    writer.WriteStartElement("EmailInternal");
                    writer.WriteElementString("Display", "Tenacity QA");
                    writer.WriteElementString("From", "inovocim@tenacity.co.za");
                    writer.WriteElementString("Server", "172.18.101.44");
                    writer.WriteElementString("Port", "25");
                    writer.WriteElementString("Username", "omni@ten.local");
                    writer.WriteElementString("Password", "g8rF1eld");
                    writer.WriteEndElement();

                    writer.WriteStartElement("EmailExternal");
                    writer.WriteElementString("Display", "Tenacity Accounts");
                    writer.WriteElementString("From", "accounts2@tenacityinc.co.za");
                    writer.WriteElementString("Server", "172.18.101.44");
                    writer.WriteElementString("Port", "25");
                    writer.WriteElementString("Username", "accounts2@ten.local");
                    writer.WriteElementString("Password", "g8rF1eld");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Reports");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Errors");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Monitor");
                    writer.WriteElementString("Email", "jbrimble@inovo.co.za");
                    writer.WriteElementString("Email", "omiles@inovo.co.za");
                    writer.WriteElementString("Email", "jkuhn@inovo.co.za");
                    writer.WriteElementString("Email", "cbouwer@inovo.co.za");
                    writer.WriteEndElement();

                    writer.WriteStartElement("Integration");
                    writer.WriteElementString("URL", @"https://ten2.pdws.co.za/");
                    writer.WriteElementString("JSON", @"https://ten2.pdws.co.za/JSON_Req");
                    writer.WriteElementString("APIKey", "741F4EE0-B586-41E5-B31D-9AEE3722BEC2");
                    writer.WriteEndElement();

                    writer.WriteStartElement("ExcludeService");
                    writer.WriteElementString("Services", "(50,51,52,53,54,55,990)");
                    writer.WriteEndElement();

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Read XML ]
        private void ReadXML()
        {
            try
            {
                var xdoc = XDocument.Load(SettingsFileProd);
                var xmlFile = from x in xdoc.Descendants("File")
                              select new
                              {
                                  FTP = (string)x.Element("FTP").Value,
                                  Input = (string)x.Element("Input").Value,
                                  Output = (string)x.Element("Output").Value,
                                  Complete = (string)x.Element("Complete").Value
                              };

                foreach (var xmlItem in xmlFile)
                {
                    ViewModel.FTP = xmlItem.FTP;
                    ViewModel.Input = xmlItem.Input;
                    ViewModel.Output = xmlItem.Output;
                    ViewModel.Complete = xmlItem.Complete;
                }

                #region [ xml Database ]
                var xmlDatabase = from item in xdoc.Descendants("Database")
                                  select new
                                  {
                                      dbInovoCIM = (string)item.Element("dbInovoCIM").Value,
                                      dbPresence = (string)item.Element("dbPresence").Value
                                  };

                foreach (var xmlItem in xmlDatabase)
                {
                    ViewModel.dbInovoCIM = xmlItem.dbInovoCIM;
                    ViewModel.dbPresence = xmlItem.dbPresence;
                }
                #endregion

                #region [ xml Internal ]
                var xmlInternal = from item in xdoc.Descendants("EmailInternal")
                                  select new
                                  {
                                      Display = (string)item.Element("Display").Value,
                                      From = (string)item.Element("From").Value,
                                      Server = (string)item.Element("Server").Value,
                                      Port = (string)item.Element("Port").Value,
                                      Username = (string)item.Element("Username").Value,
                                      Password = (string)item.Element("Password").Value
                                  };

                foreach (var xmlItem in xmlInternal)
                {
                    ViewModel.InternalDisplay = xmlItem.Display;
                    ViewModel.InternalFrom = xmlItem.From;
                    ViewModel.InternalServer = xmlItem.Server;
                    ViewModel.InternalPort = xmlItem.Port;
                    ViewModel.InternalUsername = xmlItem.Username;
                    ViewModel.InternalPassword = xmlItem.Password;
                }
                #endregion  

                #region [ xml External ]
                var xmlExternal = from item in xdoc.Descendants("EmailExternal")
                                  select new
                                  {
                                      Display = (string)item.Element("Display").Value,
                                      From = (string)item.Element("From").Value,
                                      Server = (string)item.Element("Server").Value,
                                      Port = (string)item.Element("Port").Value,
                                      Username = (string)item.Element("Username").Value,
                                      Password = (string)item.Element("Password").Value
                                  };

                foreach (var xmlItem in xmlExternal)
                {
                    ViewModel.ExternalDisplay = xmlItem.Display;
                    ViewModel.ExternalFrom = xmlItem.From;
                    ViewModel.ExternalServer = xmlItem.Server;
                    ViewModel.ExternalPort = xmlItem.Port;
                    ViewModel.ExternalUsername = xmlItem.Username;
                    ViewModel.ExternalPassword = xmlItem.Password;
                }
                #endregion 

                #region [ xml Reports ]
                List<XElement> xmlReports = (from item in xdoc.Descendants("Reports").Elements("Email") select item).ToList();
                foreach (XElement xmlItem in xmlReports)
                {
                    ViewModel.EmailReports.Add(new EmailModel { Email = xmlItem.ToString().Replace("<Email>", "").Replace("</Email>", "").Replace(" ", "") });
                }
                #endregion

                #region [ xml Errors ]
                List<XElement> xmlErrors = (from item in xdoc.Descendants("Errors").Elements("Email") select item).ToList();
                foreach (XElement xmlItem in xmlErrors)
                {
                    ViewModel.EmailErrors.Add(new EmailModel { Email = xmlItem.ToString().Replace("<Email>", "").Replace("</Email>", "").Replace(" ", "") });
                }
                #endregion

                #region [ xml Monitor ]
                List<XElement> xmlMonitor = (from item in xdoc.Descendants("Monitor").Elements("Email") select item).ToList();
                foreach (XElement xmlItem in xmlMonitor)
                {
                    ViewModel.EmailMonitors.Add(new EmailModel { Email = xmlItem.ToString().Replace("<Email>", "").Replace("</Email>", "").Replace(" ", "") });
                }
                #endregion

                #region [ xml Integration ]
                var xmlIntegration = from item in xdoc.Descendants("Integration")
                                     select new
                                     {
                                         URL = (string)item.Element("RestURL").Value,
                                         APIKey = (string)item.Element("APIKey").Value
                                     };

                foreach (var xmlItem in xmlIntegration)
                {
                    ViewModel.URL = xmlItem.URL;
                    ViewModel.APIKey = xmlItem.APIKey;
                }
                #endregion

                #region [ xml Exclude Services ]
                var xmlExclude = from item in xdoc.Descendants("ExcludeService")
                                 select new
                                 {
                                     Services = (string)item.Element("Services").Value
                                 };

                foreach (var xmlItem in xmlExclude)
                {
                    ViewModel.ExcludeServices = xmlItem.Services;
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion  
    }
}