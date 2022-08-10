#region [ Using ]
using InovoCIM.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace InovoCIM.Business
{
    public class DataFileValidate
    {
        public readonly LogRepository LogRepo = new LogRepository();
        public readonly SQLRepository SqlRepo = new SQLRepository();
        public readonly EmailRepository MailRepo = new EmailRepository();

        #region [ Master Controller ]
        public async Task<string> MasterController()
        {
            string ValidateRun = "Not Completed";
            try
            {
                DateTime start = DateTime.Now;



                return ValidateRun;
            }
            catch (Exception ex)
            {
                LogRepo.Log(ex.Message.ToString());
                await MailRepo.SendError("Validation Processing Failed", ex.Message.ToString() + " => Please view log for more information");
                return ValidateRun;
            }
        }
        #endregion

        //---------------------------------------------------------------------------//


    }
}
