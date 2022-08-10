#region [ Using ]
using InovoCIM.Business;
using InovoCIM.Data.Repository;
using System.Threading.Tasks;
#endregion

namespace InovoCIM
{
   public class Program
   {
      static void Main(string[] args)
      {
         var AppStart = new ApplicationStart();
            bool IsActive = AppStart.MasterController();
         if (IsActive)
         {
            string FileReceived = "No File";
            var dataFile = new DataFile();
            Task.Run(async () => FileReceived = await dataFile.MasterController()).GetAwaiter().GetResult();

                if (FileReceived == "Yes - File Received")
            {
               string PriorityRun = "Not Completed";
               var dataFilePriority = new DataFilePriority();
               Task.Run(async () => PriorityRun = await dataFilePriority.MasterController()).GetAwaiter().GetResult();

               if (PriorityRun == "Yes - Priority Completed")
               {
                  var dataFileValidation = new DataFileValidate();
                  Task.Run(async () => await dataFileValidation.MasterController()).GetAwaiter().GetResult();
               }
            }

            string FileReceivedT1 = "No File";
            var dataFileT1 = new DataFileTOne();
            Task.Run(async () => FileReceivedT1 = await dataFileT1.MasterController()).GetAwaiter().GetResult();

                //var ReportRepo = new Reporting();
                //Task.Run(async () => await ReportRepo.GetStatusBreakdown()).GetAwaiter().GetResult();
            }
         else
         {
            var MailRepo = new EmailRepository();
            bool IsSent = false;
            Task.Run(async () => IsSent = await MailRepo.SendError("Application is not Active", "Could not read the settings for the application to function")).GetAwaiter().GetResult();
         }
      }
   }
}