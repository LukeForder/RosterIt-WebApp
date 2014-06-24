using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class ExcelSpreadsheetGenerator
    {
        public Task<Stream> GenerateSpreadsheetAsync(IEnumerable<EmployeeWorkhoursBySiteSheet> hoursSheets)
        {
            return Task.Run(() =>
            {
                var wb = new XLWorkbook();

                var ws = wb.Worksheets.Add("Hours");

                DateTime start = hoursSheets.Min(x => x.Start);
                DateTime end = hoursSheets.Max(x => x.End);

                string title = string.Format("Roster for {0} to {1}", start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));

                ws.Range("A1", "M1")
                    .Merge()
                    .SetValue(title)
                    .Style
                        .Font.SetFontSize(18);

                ws.Cell("A2").Value = "COY";
                ws.Cell("B2").Value = "NAME";
                ws.Cell("C2").Value = "SITE";
                ws.Cell("D2").Value = "NORMAL";
                ws.Cell("E2").Value = "SUNDAY";
                ws.Cell("F2").Value = "HOLIDAY";
                ws.Cell("G2").Value = "FIREARM";
                ws.Cell("H2").Value = "NIGHT";
                ws.Cell("I2").Value = "OVERTIME";
                ws.Cell("J2").Value = "EXTRA";
                ws.Cell("K2").Value = "LEAVE";
                ws.Cell("L2").Value = "SICK";
                ws.Cell("M2").Value = "ABSENT";
                ws.Cell("N2").Value = "OFF";
                ws.Range("A1", "N2").Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent5, 0.2)).Font.SetBold();

                ws.Rows(1, 2).Height = 20;

                ws.Column(2).Width = 20;
                ws.Column(3).Width = 30;
                ws.Columns(4, 14).Width = 10;
                ws.SheetView.FreezeRows(2);
                ws.Range(1, 1, 1, 14).Style.Alignment.Horizontal = (XLAlignmentHorizontalValues.Center);

                int offset = 0;
                for (int i = 0; i < hoursSheets.Count(); i++)
                {
                    var hoursSheet = hoursSheets.ElementAt(i);

                    ws.Range(offset + 3, 1, offset + 3, 14).Style.Border.SetTopBorder(XLBorderStyleValues.Thin);

                    ws.Cell(offset + 3, 1).Value = hoursSheet.Employee.CompanyNumber;
                    ws.Cell(offset + 3, 2).Value = hoursSheet.Employee.FullName;

                    for (int siteIdx = 0; siteIdx < hoursSheet.WorkHoursList.Count; siteIdx++)
                    {

                        if ((i + siteIdx) % 2 == 1)
                            ws.Range(offset + 3, 1, offset + 3, 14).Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent5, 0.4));
                        else
                            ws.Range(offset + 3, 1, offset + 3, 14).Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent5, 0.8));

                        var siteSheet = hoursSheet.WorkHoursList.ElementAt(siteIdx);

                        ws.Cell(offset + 3, 3).Value = siteSheet.Site.Name;

                        ws.Cell(offset + 3, 4).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Normal).UnitsWorked;
                        ws.Cell(offset + 3, 5).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Sunday).UnitsWorked;
                        ws.Cell(offset + 3, 6).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Holiday).UnitsWorked;
                        ws.Cell(offset + 3, 7).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Firearm).UnitsWorked;
                        ws.Cell(offset + 3, 8).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Night).UnitsWorked;
                        ws.Cell(offset + 3, 9).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Overtime).UnitsWorked;
                        ws.Cell(offset + 3, 10).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Extra).UnitsWorked;
                        ws.Cell(offset + 3, 11).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Leave).UnitsWorked;
                        ws.Cell(offset + 3, 12).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Sick).UnitsWorked;
                        ws.Cell(offset + 3, 13).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Absent).UnitsWorked;
                        ws.Cell(offset + 3, 14).Value = siteSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Off).UnitsWorked;

                        offset++;
                    }
                    
                }


                ws.Range(2, 1, offset + 2, 13).Style.Alignment.Horizontal = (XLAlignmentHorizontalValues.Center);

                Stream stream = new MemoryStream();

                wb.SaveAs(stream);

                return stream;
            });
        }

        public Task<Stream> GenerateSpreadsheetAsync(IEnumerable<EmployeeHoursSheet> hoursSheets)
        {
            return Task.Run(() =>
            {
                var wb = new XLWorkbook();

                var ws = wb.Worksheets.Add("Hours");

                DateTime start = hoursSheets.Min(x => x.Start);
                DateTime end = hoursSheets.Max(x => x.End);

                string title = string.Format("Roster for {0} to {1}", start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));

                ws.Range("A1", "M1")
                    .Merge()
                    .SetValue(title)
                    .Style
                        .Font.SetFontSize(18);

                ws.Cell("A2").Value = "COY";
                ws.Cell("B2").Value = "NAME";
                ws.Cell("C2").Value = "NORMAL";
                ws.Cell("D2").Value = "SUNDAY";
                ws.Cell("E2").Value = "HOLIDAY";
                ws.Cell("F2").Value = "FIREARM";
                ws.Cell("G2").Value = "NIGHT";
                ws.Cell("H2").Value = "OVERTIME";
                ws.Cell("I2").Value = "EXTRA";
                ws.Cell("J2").Value = "LEAVE";
                ws.Cell("K2").Value = "SICK";
                ws.Cell("L2").Value = "ABSENT";
                ws.Cell("M2").Value = "OFF";
                ws.Range("A1", "M2").Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent5, 0.2)).Font.SetBold();

                ws.Column(2).Width = 20;
                ws.Columns(3, 13).Width = 10;
                ws.SheetView.FreezeRows(2);
                ws.Range(1, 1, 1, 13).Style.Alignment.Horizontal = (XLAlignmentHorizontalValues.Center);

                for (int i = 0; i < hoursSheets.Count(); i++)
                {
                    var hoursSheet = hoursSheets.ElementAt(i);

                    ws.Range(i + 3, 1, i + 3, 13).Style.Border.SetTopBorder(XLBorderStyleValues.Thin);

                    if (i % 2 == 1)
                        ws.Range(i + 3, 1, i + 3, 13).Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent5, 0.4));
                    else
                        ws.Range(i + 3, 1, i + 3, 13).Style.Fill.SetBackgroundColor(XLColor.FromTheme(XLThemeColor.Accent5, 0.8));
                    
                    ws.Cell(i + 3, 1).Value = hoursSheet.Employee.CompanyNumber;
                    ws.Cell(i + 3, 2).Value = hoursSheet.Employee.FullName;
                    ws.Cell(i + 3, 3).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Normal).UnitsWorked;
                    ws.Cell(i + 3, 4).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Sunday).UnitsWorked;
                    ws.Cell(i + 3, 5).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Holiday).UnitsWorked;
                    ws.Cell(i + 3, 6).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Firearm).UnitsWorked;
                    ws.Cell(i + 3, 7).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Night).UnitsWorked;
                    ws.Cell(i + 3, 8).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Overtime).UnitsWorked;
                    ws.Cell(i + 3, 9).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Extra).UnitsWorked;
                    ws.Cell(i + 3, 10).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Leave).UnitsWorked;
                    ws.Cell(i + 3, 11).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Sick).UnitsWorked;
                    ws.Cell(i + 3, 12).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Absent).UnitsWorked;
                    ws.Cell(i + 3, 13).Value = hoursSheet.WorkHoursList.First(x => x.PayrollCategory == PayrollCategory.Off).UnitsWorked;
                }


                ws.Range(2, 1, hoursSheets.Count() + 2, 13).Style.Alignment.Horizontal = (XLAlignmentHorizontalValues.Center);

                Stream stream = new MemoryStream();

                wb.SaveAs(stream);

                return stream;
            });
        }
    }
}
