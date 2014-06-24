using Common.Logging;
using RosterIt.Holidays;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    /// <summary>
    /// Takes an employee's timesheet and determines the work hours for each payroll category.
    /// </summary>
    public class WorkHoursCompiler
    {
        private readonly IHolidayProvider _holidayProvider;
        private readonly ILog _log;

        public WorkHoursCompiler(
            IHolidayProvider holidayProvider)
        {
            _holidayProvider = holidayProvider;
            _log = LogManager.GetCurrentClassLogger();
        }

        private readonly IEnumerable<Shift> workedShifts = new Shift[] {
            Shifts.Firearm, Shifts.Present
        };


        public async Task<EmployeeWorkhoursBySiteSheet> CompileBySiteAsync(EmployeeTimesheet employeeTimesheet)
        {
            var attendancesBySite =
                employeeTimesheet
                    .SiteRecords
                    .GroupBy(x => x.Site.Id);


            var attendances =
                employeeTimesheet
                .SiteRecords
                .SelectMany(x => x.ShiftAttendances);

            DateTime
                lastDate = attendances.Max(x => x.Date),
                firstDate = attendances.Min(x => x.Date);

            TimeSpan duration = lastDate - firstDate;

            IEnumerable<Holiday> holidays = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    holidays = await _holidayProvider.GetHolidays(firstDate, duration);
                    break;
                }
                catch
                {
                }
            }

            if (holidays == null)
                throw new HolidayServiceException("Unable to fetch holidays from 'Enrico'.");

            var workhoursBySiteHoursSheet = new EmployeeWorkhoursBySiteSheet
            {
                Employee = employeeTimesheet.Employee,
                End = lastDate,
                Start = firstDate
            };

            foreach (var site in attendancesBySite)
            {
                var restrictedTimesheet = new EmployeeTimesheet
                {
                    Employee = employeeTimesheet.Employee,
                    GeneratedDate = DateTime.Now,
                    SiteRecords = site.ToList()
                };

                Task<int>
                    holidayHoursTask = CalculateHolidayHoursAsync(restrictedTimesheet, holidays),
                    sundayHoursTask = CalculateSundayHoursAsync(restrictedTimesheet),
                    normalHoursTask = CalculateNormalHoursAsync(restrictedTimesheet, holidays),
                    offHoursTask = CalculateOffHoursAsync(restrictedTimesheet),
                    leaveHoursTask = CalculateLeaveHoursAsync(restrictedTimesheet),
                    sickHoursTask = CalculateSickHoursAsync(restrictedTimesheet),
                    nightHoursTask = CalculateNightHoursAsync(restrictedTimesheet),
                    firearmHoursTask = CalculateFirearmsHoursAsync(restrictedTimesheet),
                    absentHoursTask = CalculateAbsentHoursAsync(restrictedTimesheet),
                    overtimehoursTask = CalculateOvertimeHoursAsync(restrictedTimesheet),
                    extraHoursTask = CalculateExtraHoursAsync(restrictedTimesheet);

                await Task.WhenAll(
                    holidayHoursTask,
                    sundayHoursTask,
                    normalHoursTask,
                    offHoursTask,
                    leaveHoursTask,
                    sickHoursTask,
                    nightHoursTask,
                    firearmHoursTask,
                    absentHoursTask,
                    extraHoursTask);

                var siteWorkHoursRecord = new EmployeeSiteWorkRecord
                {
                    Site = site.Select(x => x.Site).FirstOrDefault(),
                    WorkHoursList = new List<WorkHoursList> 
                    {
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Holiday,
                            UnitsWorked = holidayHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Sunday,
                            UnitsWorked = sundayHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Normal,
                            UnitsWorked = normalHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Absent,
                            UnitsWorked = absentHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Leave,
                            UnitsWorked = leaveHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Night,
                            UnitsWorked = nightHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Firearm,
                            UnitsWorked = firearmHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Off,
                            UnitsWorked = offHoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Overtime,
                            UnitsWorked = overtimehoursTask.Result
                        },
                        new WorkHoursList 
                        {
                            PayrollCategory = PayrollCategory.Sick,
                            UnitsWorked = sickHoursTask.Result
                        },
                        new WorkHoursList
                        {
                            PayrollCategory = PayrollCategory.Extra,
                            UnitsWorked = extraHoursTask.Result
                        }
                    }
                };

                workhoursBySiteHoursSheet.WorkHoursList.Add(siteWorkHoursRecord);

            }

            return workhoursBySiteHoursSheet;
        }

        public async Task<EmployeeHoursSheet> CompileAsync(EmployeeTimesheet employeeTimesheet)
        {
            var attendances =
                employeeTimesheet
                    .SiteRecords
                    .SelectMany(x => x.ShiftAttendances);

            DateTime
                lastDate = attendances.Max(x => x.Date),
                firstDate = attendances.Min(x => x.Date);

            TimeSpan duration = lastDate - firstDate;

            IEnumerable<Holiday> holidays = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    holidays = await _holidayProvider.GetHolidays(firstDate, duration);
                    break;
                }
                catch
                {
                }
            }

            if (holidays == null)
                throw new HolidayServiceException("Unable to fetch holidays from 'Enrico'.");

            Task<int>
                holidayHoursTask = CalculateHolidayHoursAsync(employeeTimesheet, holidays),
                sundayHoursTask = CalculateSundayHoursAsync(employeeTimesheet),
                normalHoursTask = CalculateNormalHoursAsync(employeeTimesheet, holidays),
                offHoursTask = CalculateOffHoursAsync(employeeTimesheet),
                leaveHoursTask = CalculateLeaveHoursAsync(employeeTimesheet),
                sickHoursTask = CalculateSickHoursAsync(employeeTimesheet),
                nightHoursTask = CalculateNightHoursAsync(employeeTimesheet),
                firearmHoursTask = CalculateFirearmsHoursAsync(employeeTimesheet),
                absentHoursTask = CalculateAbsentHoursAsync(employeeTimesheet),
                overtimehoursTask = CalculateOvertimeHoursAsync(employeeTimesheet),
                extraHoursTask = CalculateExtraHoursAsync(employeeTimesheet);

            await Task.WhenAll(
                holidayHoursTask, 
                sundayHoursTask, 
                normalHoursTask, 
                offHoursTask, 
                leaveHoursTask, 
                sickHoursTask, 
                nightHoursTask, 
                firearmHoursTask, 
                absentHoursTask,
                extraHoursTask);

            var employeesHourSheet = new EmployeeHoursSheet
            {
                Employee = employeeTimesheet.Employee,
                End = lastDate,
                Start = firstDate,
                WorkHoursList = new List<WorkHoursList> 
                {
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Holiday,
                        UnitsWorked = holidayHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Sunday,
                        UnitsWorked = sundayHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Normal,
                        UnitsWorked = normalHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Absent,
                        UnitsWorked = absentHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Leave,
                        UnitsWorked = leaveHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Night,
                        UnitsWorked = nightHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Firearm,
                        UnitsWorked = firearmHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Off,
                        UnitsWorked = offHoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Overtime,
                        UnitsWorked = overtimehoursTask.Result
                    },
                    new WorkHoursList 
                    {
                        PayrollCategory = PayrollCategory.Sick,
                        UnitsWorked = sickHoursTask.Result
                    },
                    new WorkHoursList
                    {
                        PayrollCategory = PayrollCategory.Extra,
                        UnitsWorked = extraHoursTask.Result
                    }
                }
            };

            return employeesHourSheet;
        }

        private Task<int> CalculateExtraHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                    timesheet
                        .SiteRecords
                        .SelectMany(x => x.ShiftAttendances)
                        .Where(x => x.Shift.Id == Shifts.ExtraShift.Id)
                        .Sum(x => x.WorkedDuration.TotalHours);

                return (int)TimeSpan.FromHours(hoursWorked).TotalHours;
            });
        }

        private Task<int> CalculateHolidayHoursAsync(EmployeeTimesheet timesheet, IEnumerable<Holiday> holidays)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                    timesheet
                        .SiteRecords
                        .SelectMany(x => x.ShiftAttendances)
                        .Where(x => holidays.Any(holiday => holiday.Date.Date == x.Date.Date))
                        .Where(x => workedShifts.Any(shift => x.Shift.Id == shift.Id))
                        .Where(x => x.Shift.Id != Shifts.ExtraShift.Id)
                        .Sum(x => x.WorkedDuration.TotalHours);

                return (int)TimeSpan.FromHours(hoursWorked).TotalHours;
            });
        }

        private Task<int> CalculateSundayHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
                {
                    var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => x.Date.DayOfWeek == DayOfWeek.Sunday)
                            .Where(x => workedShifts.Any(shift => x.Shift.Id == shift.Id))
                            .Sum(x => x.WorkedDuration.TotalHours);

                    return (int)hoursWorked;
                });
        }

        private Task<int> CalculateNormalHoursAsync(EmployeeTimesheet timesheet, IEnumerable<Holiday> holidays)
        {
            return Task.Run(() =>
                {
                    var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => !holidays.Any(holiday => holiday.Date.Date == x.Date.Date))
                            .Where(x => x.Date.DayOfWeek != DayOfWeek.Sunday)
                            .Where(x => workedShifts.Any(shift => x.Shift.Id == shift.Id))
                            .Where(x => x.Shift.Id != Shifts.ExtraShift.Id)
                            .Sum(x => x.WorkedDuration.TotalHours);

                    return (int)hoursWorked;
                });
        }

        private Task<int> CalculateOffHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => x.Shift.Id == Shifts.Off.Id)
                            .Count();

                return (int)hoursWorked;
            });
        }

        private Task<int> CalculateLeaveHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => x.Shift.Id == Shifts.Leave.Id)
                            .Count();

                return (int)hoursWorked;
            });
        }

        private Task<int> CalculateSickHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => x.Shift.Id == Shifts.Sick.Id)
                            .Count();

                return (int)hoursWorked;
            });
        }

        private Task<int> CalculateOvertimeHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => x.Shift.Id == Shifts.Overtime.Id)
                            .Sum(x => x.WorkedDuration.TotalHours);

                return (int)hoursWorked;
            });
        }

        private Task<int> CalculateFirearmsHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => x.Shift.Id == Shifts.Firearm.Id)
                            .Count();

                return (int)hoursWorked;
            });
        }

        private Task<int> CalculateNightHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => x.ShiftTime == ShiftTime.Night)
                            .Where(x => workedShifts.Any(shift => shift.Id == x.Shift.Id))
                            .Where(x => x.Shift.Id != Shifts.Overtime.Id)
                            .Where(x => x.Shift.Id != Shifts.ExtraShift.Id)
                            .Count();

                return (int)hoursWorked;
            });
        }

        private Task<int> CalculateAbsentHoursAsync(EmployeeTimesheet timesheet)
        {
            return Task.Run(() =>
            {
                var hoursWorked =
                        timesheet
                            .SiteRecords
                            .SelectMany(x => x.ShiftAttendances)
                            .Where(x => workedShifts.Any(shift => shift.Id == Shifts.Absent.Id))
                            .Count();

                return (int)hoursWorked;
            });
        }
    }
}
