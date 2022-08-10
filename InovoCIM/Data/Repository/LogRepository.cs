#region [ Using ]
using InovoCIM.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion

namespace InovoCIM.Data.Repository
{
    public class LogRepository
    {
        #region [ Log ]
        public void Log(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Transactions - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
                Console.WriteLine(text);
            }
            catch { }
        }
        #endregion

        #region [ Log Error Line ]
        public void LogErrorLine(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Line Error - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
            }
            catch { }
        }
        #endregion

        #region [ Log Duplicate Line ]
        public void LogDuplicateLine(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Line Duplicate Error - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
            }
            catch { }
        }
        #endregion

        #region [ Log Bulk Data Request Done Failed ]
        public void LogBulkDataRequestDoneFailed(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Bulk DataRequests - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
            }
            catch { }
        }
        #endregion

        #region [ Log Bulk Data Request Invalid Phone Failed ]
        public void LogBulkDataRequestInvalidPhoneFailed(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Bulk DataRequests Invalid Phone - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
            }
            catch { }
        }
        #endregion

        #region [ Log Bulk Presence Data Failed ]
        public void LogBulkPresenceDataFailed(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Bulk Presence - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
            }
            catch { }
        }
        #endregion

        #region [ Log Invalid Phone ]
        public void LogInvalidPhone(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Invalid Phone - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
            }
            catch { }
        }
        #endregion

        #region [ Log Error Query ]
        public void LogErrorQuery(string text)
        {
            try
            {
                string fileLocation = ViewModel.Output + "Query Error - " + DateTime.Today.ToString("dd.MM.yyyy") + ".txt";
                using (StreamWriter writer = new StreamWriter(fileLocation, true)) { writer.WriteLine(text); }
            }
            catch { }
        }
        #endregion
    }
}
