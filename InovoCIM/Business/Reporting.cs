#region [ Using ]
using InovoCIM.Data.Entities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace InovoCIM.Business
{
    public class Reporting
    {
        #region [ Get File Breakdown ]
        public async Task<string> GetFileBreakdown()
        {
            string ReportFile = ViewModel.Output + "File Breakdown - " + DateTime.Today.ToString("dd.MM.yyyy") + ".xlsx";
            try
            {
                using (var doc = new ExcelPackage())
                {
                    var sheet01 = doc.Workbook.Worksheets.Add("DATA");
                    sheet01.Cells[1, 1].Value = "SERVICEID";
                    sheet01.Cells[1, 2].Value = "NAME";
                    sheet01.Cells[1, 3].Value = "LOADID";
                    sheet01.Cells[1, 4].Value = "SOURCEID";
                    sheet01.Cells[1, 5].Value = "PHONE";
                    sheet01.Cells[1, 6].Value = "CTICODE";
                    sheet01.Cells[1, 7].Value = "ATTEMPTS";




                    doc.SaveAs(new FileInfo(ReportFile));
                }

                return ReportFile;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        #endregion

    }
}
