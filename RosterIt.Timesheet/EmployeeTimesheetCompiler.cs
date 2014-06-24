using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class EmployeeTimesheetCompiler
    {
        public IEnumerable<EmployeeTimesheet> Compile(IEnumerable<SiteTimesheet> siteTimesheets)
        {
            var attendanceByEmployeeListing =
                siteTimesheets
                    .SelectMany(
                        x => x.AttendanceRecords, 
                        (timesheet, record) => new
                            {
                                Employee = record.Guard,
                                Attendance = record.Attendance,
                                Site = timesheet.Site
                            })
                    .GroupBy(x => x.Employee.CompanyNumber)
                    .ToList();


            var employeeTimesheets =
                attendanceByEmployeeListing
                    .Select(x => new EmployeeTimesheet
                    {
                        Employee = x.First().Employee,
                        GeneratedDate = DateTime.Now,
                        SiteRecords = 
                            x.Select(
                                y => new SiteRecord
                                    {
                                        Site = y.Site,
                                        ShiftAttendances = y.Attendance
                                    })
                            .ToList()
                    })
                    .ToList();

            return employeeTimesheets;

        }
    }
}
