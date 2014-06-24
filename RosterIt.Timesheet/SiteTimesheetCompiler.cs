using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class SiteTimesheetCompiler
    {
        private readonly MongoCollection<Roster> _rosterCollection;
        private readonly MongoCollection<Site> _siteCollection;

        public SiteTimesheetCompiler(
            MongoCollection<Site> siteCollection,
            MongoCollection<Roster> rosterCollection)
        {
            _rosterCollection = rosterCollection;
            _siteCollection = siteCollection;
        }

        public RosterIt.Timesheet.SiteTimesheet Compile(DateTime start, TimeSpan length, Guid siteId)
        {
            DateTime end = start.Add(length);

            var site = 
                _siteCollection
                    .FindOne(
                        Query<Site>.EQ(x => x.Id, siteId));

            if (site == null)
                throw new SiteNotFoundException(siteId);

            var rostersInDuration =
                _rosterCollection
                    .Find(
                        Query.And(
                            Query<Roster>.LTE(x => x.Date, end),
                            Query<Roster>.GTE(x => x.Date, start),
                            Query<Roster>.EQ(x => x.Site.Id, siteId)))
                    .ToList();

            var timesheet = new SiteTimesheet
            {
                Id = Guid.NewGuid(),
                Period = length,
                StartDate = start,
                Site = site                
            };

            var attendanceRecords =
                rostersInDuration
                    .SelectMany(
                        x => x.GuardRecords,
                        (roster, record) => new
                        {
                            Guard = new Employee { CompanyNumber = record.Guard.CompanyNumber, FullName = record.Guard.FullName },
                            Shift = record.Shift,
                            Date = roster.Date.ToLocalTime(),
                            ShiftTime = roster.ShiftTime,
                            OvertimeHours = record.OvertimeHours
                        }).
                        ToList();

            

            var shiftRecords =
                attendanceRecords.
                    Where(x => x.Shift != null).
                    Select(x =>
                            new
                            {
                                Date = x.Date,
                                Guard = x.Guard,
                                Shift = x.Shift,
                                ShiftTime = x.ShiftTime,
                                WorkedDuration = TimeSpan.FromHours(12)
                            }).
                    ToList();

            var overtimeRecords =
                attendanceRecords.
                    Where(x => x.OvertimeHours != null && x.OvertimeHours > 0).
                    Select(x =>
                            new
                            {
                                Date = x.Date,
                                Guard = x.Guard,
                                Shift = Shifts.Overtime,
                                ShiftTime = x.ShiftTime,
                                WorkedDuration = TimeSpan.FromHours((int)x.OvertimeHours)
                            }).
                    ToList();

            
            timesheet.AttendanceRecords =
                Enumerable.Concat(overtimeRecords, shiftRecords)
                            .GroupBy(x => x.Guard.CompanyNumber)
                            .Select(x => new AttendenceRecord
                            {
                                Guard = x.First().Guard,
                                Attendance = 
                                    x.Select(
                                        a => new ShiftAttendance
                                        {
                                            Date = a.Date,
                                            Shift = a.Shift,
                                            WorkedDuration = a.WorkedDuration,
                                            ShiftTime = a.ShiftTime
                                        })
                                    .ToList()
                            })
                            .ToList();

            return timesheet;
                    
        }
    }
}
