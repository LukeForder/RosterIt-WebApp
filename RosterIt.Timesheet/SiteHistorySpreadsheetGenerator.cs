using ClosedXML.Excel;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class SiteHistorySpreadsheetGenerator
    {
        private void SetBackground(IXLWorksheet ws, int startRow, int endRow, int columns, XLThemeColor theme)
        {
            var minorColor = XLColor.FromTheme(theme, 0.8);
            var majorColor = XLColor.FromTheme(theme, 0.6);

            ws.Range(startRow, 1, endRow, columns).Style.Fill.SetBackgroundColor(majorColor);
            ws.Range(endRow, 3, endRow, columns).Style.Fill.SetBackgroundColor(minorColor);
        }

        private void SetBorder(IXLWorksheet ws, int startRow, int endRow, int columns)
        {
            ws.Range(startRow, 1, startRow, columns).Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
            ws.Range(endRow, 1, endRow, columns).Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
        }

        
        public Task<Stream> GenerateSpreadsheetAsync(SiteTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var wb = new XLWorkbook();

                var ws = wb.Worksheets.Add(timesheet.Site.Name);

                ws.SheetView.FreezeRows(2);
                ws.SheetView.FreezeColumns(2);
                
                var symbolCountsByDate =
                    timesheet.AttendanceRecords
                        .SelectMany(x => x.Attendance)
                        .GroupBy(x => x.Date )
                        .Select(x => new {
                                Date = x.Key,
                                Night = new {
                                    Present = x.Count(y => y.ShiftTime == ShiftTime.Night && y.Shift.Id == Shifts.Present.Id),
                                    Firearm = x.Count(y => y.ShiftTime == ShiftTime.Night && y.Shift.Id == Shifts.Firearm.Id),
                                    Leave = x.Count(y => y.ShiftTime == ShiftTime.Night && y.Shift.Id == Shifts.Leave.Id),
                                    Sick = x.Count(y => y.ShiftTime == ShiftTime.Night && y.Shift.Id == Shifts.Sick.Id),
                                    Absent = x.Count(y => y.ShiftTime == ShiftTime.Night && y.Shift.Id == Shifts.Absent.Id),
                                    Overtime = x.Count(y => y.ShiftTime == ShiftTime.Night && y.Shift.Id == Shifts.Overtime.Id),
                                    Extra = x.Count(y => y.ShiftTime == ShiftTime.Night && y.Shift.Id == Shifts.ExtraShift.Id)
                                },
                                Day = new
                                {
                                    Present = x.Count(y => y.ShiftTime == ShiftTime.Day && y.Shift.Id == Shifts.Present.Id),
                                    Firearm = x.Count(y => y.ShiftTime == ShiftTime.Day && y.Shift.Id == Shifts.Firearm.Id),
                                    Leave = x.Count(y => y.ShiftTime == ShiftTime.Day && y.Shift.Id == Shifts.Leave.Id),
                                    Sick = x.Count(y => y.ShiftTime == ShiftTime.Day && y.Shift.Id == Shifts.Sick.Id),
                                    Absent = x.Count(y => y.ShiftTime == ShiftTime.Day && y.Shift.Id == Shifts.Absent.Id),
                                    Overtime = x.Count(y => y.ShiftTime == ShiftTime.Day && y.Shift.Id == Shifts.Overtime.Id),
                                    Extra = x.Count(y => y.ShiftTime == ShiftTime.Day && y.Shift.Id == Shifts.ExtraShift.Id)
                                }
                            }
                        )
                        .OrderBy(x => x.Date.Date)
                        .ToList();

                int columnCount = symbolCountsByDate.Count + 2;

                ws.Range(1, 1, 20, columnCount + 2)
                    .Style
                    .Alignment
                    .Horizontal = (XLAlignmentHorizontalValues.Center);


                ws.Range(1, 1, 1, columnCount - 2)
                  .Merge()
                  .FirstCell()
                    .SetValue(timesheet.Site.Name)
                  .Style
                    .Font.SetFontSize(18)
                    .Font.SetBold();

                ws.Columns(3, columnCount).Width = 18;
                ws.Range(3, 3, 16, columnCount)
                    .Style
                        .Border.SetRightBorder(XLBorderStyleValues.Thin);

                ws.Range(18, 3, 20, columnCount)
                   .Style
                       .Border.SetRightBorder(XLBorderStyleValues.Thin);

                ws.Cell(1, columnCount - 1 )
                    .SetValue(timesheet.StartDate.ToString("yyyy/MM/dd"))
                  .Style
                    .Font.SetFontSize(18);

                ws.Cell(1, columnCount)
                  .SetValue(timesheet.StartDate.Add(timesheet.Period).ToString("yyyy/MM/dd"))
                      .Style
                        .Font.SetFontSize(18);


                ws.Column(1).Width = 15;

                ws.Cell(3, 1).Value = "Present";
                ws.Cell(3, 2).Value = "Day";
                ws.Cell(4, 2).Value = "Night";
                SetBackground(ws, 3, 4, columnCount, XLThemeColor.Accent1);
              //  SetBorder(ws, 3, 4, columnCount);

                ws.Cell(5, 1).Value = "Firearm";
                ws.Cell(5, 2).Value = "Day";
                ws.Cell(6, 2).Value = "Night";
                SetBackground(ws, 5, 6, columnCount, XLThemeColor.Accent2);
               // SetBorder(ws, 5, 6, columnCount);

                ws.Cell(7, 1).Value = "Leave";
                ws.Cell(7, 2).Value = "Day";
                ws.Cell(8, 2).Value = "Night";
                SetBackground(ws, 7, 8, columnCount, XLThemeColor.Accent3);
                //SetBorder(ws, 7, 8, columnCount);

                ws.Cell(9, 1).Value = "Sick";
                ws.Cell(9, 2).Value = "Day";
                ws.Cell(10, 2).Value = "Night";
                SetBackground(ws, 9, 10, columnCount, XLThemeColor.Accent4);
               // SetBorder(ws, 9, 10, columnCount);

                ws.Cell(11, 1).Value = "Absent";
                ws.Cell(11, 2).Value = "Day";
                ws.Cell(12, 2).Value = "Night";
                SetBackground(ws, 11, 12, columnCount, XLThemeColor.Accent5);
                //SetBorder(ws, 11, 12, columnCount);

                ws.Cell(13, 1).Value = "Overtime";
                ws.Cell(13, 2).Value = "Day";
                ws.Cell(14, 2).Value = "Night";
                SetBackground(ws, 13, 14, columnCount, XLThemeColor.Accent6);
               // SetBorder(ws, 13, 14, columnCount);
                
                ws.Cell(15, 1).Value = "Extra";
                ws.Cell(15, 2).Value = "Day";
                ws.Cell(16, 2).Value = "Night";
                SetBackground(ws, 15, 16, columnCount, XLThemeColor.Accent1);

                ws.Cell(18, 1).Value = "Sub Total";
                ws.Cell(18, 2).Value = "Day";
                ws.Cell(19, 2).Value = "Night";
                ws.Cell(20, 1).Value = "Total";
                SetBackground(ws, 18, 20, columnCount, XLThemeColor.Accent2);
               // SetBorder(ws, 16, 17, columnCount);
               // SetBorder(ws, 18, 18, columnCount);

                for (int i = 0; i < symbolCountsByDate.Count; i++)
                {
                    var record = symbolCountsByDate[i];

                    ws.Cell(2, 3 + i).Value = record.Date.Date.ToString("yyyy/MM/dd");

                    ws.Cell(3, 3 + i).Value = record.Day.Present;
                    ws.Cell(5, 3 + i).Value = record.Day.Firearm;
                    ws.Cell(7, 3 + i).Value = record.Day.Leave;
                    ws.Cell(9, 3 + i).Value = record.Day.Sick;
                    ws.Cell(11, 3 + i).Value = record.Day.Absent;
                    ws.Cell(13, 3 + i).Value = record.Day.Overtime;
                    ws.Cell(15, 3 + i).Value = record.Day.Extra;

                    ws.Cell(4, 3 + i).Value = record.Night.Present;
                    ws.Cell(6, 3 + i).Value = record.Night.Firearm;
                    ws.Cell(8, 3 + i).Value = record.Night.Leave;
                    ws.Cell(10, 3 + i).Value = record.Night.Sick;
                    ws.Cell(12, 3 + i).Value = record.Night.Absent;
                    ws.Cell(14, 3 + i).Value = record.Night.Overtime;
                    ws.Cell(16, 3 + i).Value = record.Night.Extra;

                    var totalDay = record.Day.Present + record.Day.Firearm + record.Day.Leave + record.Day.Sick + record.Day.Absent + record.Day.Overtime + record.Day.Extra;
                    var totalNight = record.Night.Present + record.Night.Firearm + record.Night.Leave + record.Night.Sick + record.Night.Absent + record.Night.Overtime + record.Night.Extra;

                    ws.Cell(18, 3 + i).Value = totalDay;
                    ws.Cell(19, 3 + i).Value = totalNight;

                    ws.Cell(20, 3 + i).Value = totalNight + totalDay;
                }

                Stream stream = new MemoryStream();

                wb.SaveAs(stream);

                return stream;
            });
        }
    }
}
