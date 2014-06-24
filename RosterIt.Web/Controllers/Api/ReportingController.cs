using MongoDB.Driver;
using RosterIt.Models;
using RosterIt.Timesheet;
using RosterIt.Web.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace RosterIt.Web.Controllers.Api
{
    [RoutePrefix("reporting"), Authorize]
    public class ReportingController : ApiController
    {
        private readonly SiteTimesheetCompiler _siteCompiler;
        private readonly EmployeeTimesheetCompiler _employeeCompiler;
        private readonly WorkHoursCompiler _workHoursCompiler;
        private readonly MongoCollection<Site> _siteCollection;
        private readonly ExcelSpreadsheetGenerator _spreasheetGenerator;
        private readonly MongoRoleProvider _roleProvider;
        private readonly SiteHistorySpreadsheetGenerator _siteHistorySpreadsheetGenerator;

        public ReportingController(
            MongoRoleProvider roleProvider,
            MongoCollection<Site> siteCollection,
            SiteTimesheetCompiler siteCompiler,
            EmployeeTimesheetCompiler employeeCompiler,
            WorkHoursCompiler workHoursCompiler,
            ExcelSpreadsheetGenerator spreadsheetGenerator,
            SiteHistorySpreadsheetGenerator siteHistorySpreadsheetGenerator)
        {
            _siteCollection = siteCollection;
            _siteCompiler = siteCompiler;
            _employeeCompiler = employeeCompiler;
            _workHoursCompiler = workHoursCompiler;
            _spreasheetGenerator = spreadsheetGenerator;
            _roleProvider = roleProvider;
            _siteHistorySpreadsheetGenerator = siteHistorySpreadsheetGenerator;
        }

        private Tuple<DateTime, DateTime> OrderDates(DateTime x, DateTime y)
        {
            return (x <= y) ? new Tuple<DateTime, DateTime>(x, y) : new Tuple<DateTime, DateTime>(y, x);
        }

        [Route("rollup-site", Name = "RollupSite")]
        public async Task<HttpResponseMessage> GetSiteRollup(DateTime start, DateTime end, Guid site)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            Site selectedSite = _siteCollection.FindOneById(site);
            if (selectedSite == null)
                return
                     Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "The site requested doesn't exist."
                        });

            Tuple<DateTime, DateTime> orderedDates = OrderDates(start, end);
            start = orderedDates.Item1;
            end = orderedDates.Item2;

            TimeSpan duration = end - start;
            if (duration.Days < 1)
                return
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "The duration of the rollup must be at least one day"
                        });

            SiteTimesheet timesheet =  await Task.Run(() => _siteCompiler.Compile(start, duration, site));


            var spreadsheetData =
                await
                _siteHistorySpreadsheetGenerator
                    .GenerateSpreadsheetAsync(timesheet);

            spreadsheetData.Position = 0;

            var content = new StreamContent(spreadsheetData);

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = string.Format("{0} history from {1} to {2}.xlsx", selectedSite.Name, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"))
            };

            response.Content = content;

            return response;
        }

        [Route("rollup-hours", Name = "RollupHours")]
        public async Task<HttpResponseMessage> GetHoursRollup(DateTime start, DateTime end)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

             List<Guid> sites = await Task.Run(() => _siteCollection.FindAll().Select(x => x.Id).Distinct().ToList());

             Tuple<DateTime, DateTime> orderedDates = OrderDates(start, end);
             start = orderedDates.Item1;
             end = orderedDates.Item2;

            TimeSpan duration = end - start;            
            if (duration.Days < 1) 
                return 
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest, 
                        new ErrorResponse
                        {
                            Reason = "The duration of the rollup must be at least one day"
                        });

            ConcurrentBag<SiteTimesheet> siteTimesheets = new ConcurrentBag<SiteTimesheet>();

            Parallel.ForEach(sites, (site) => {
                SiteTimesheet timesheet =  _siteCompiler.Compile(start, duration, site);
                siteTimesheets.Add(timesheet);
            });

            IEnumerable<EmployeeTimesheet> employeeTimesheets = _employeeCompiler.Compile(siteTimesheets.ToList());
            ConcurrentBag<Task<EmployeeWorkhoursBySiteSheet>> calcalutionTasks = new ConcurrentBag<Task<EmployeeWorkhoursBySiteSheet>>();
            Parallel.ForEach(employeeTimesheets,  (timesheet) =>
                {
                    calcalutionTasks.Add(_workHoursCompiler.CompileBySiteAsync(timesheet));
                });

            await Task.WhenAll(calcalutionTasks);

            var spreadsheetData = 
                await 
                _spreasheetGenerator
                    .GenerateSpreadsheetAsync(
                        calcalutionTasks
                            .Select(x => x.Result)
                            .OrderBy(x => x.Employee.CompanyNumber, new PseudoNumericComparer())
                            .ToList());

            spreadsheetData.Position = 0;

            var content = new StreamContent(spreadsheetData);

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = string.Format("Roster {0} to {1}.xlsx", start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"))
            };

            response.Content = content;

            return response;

        }

        [Route("roll-up/employee/month", Name = "employeeMonthlyAttendence")]
        public async Task<HttpResponseMessage> GetEmployeeAttendanceForMonth(string companyNumber, DateTime month)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            if (string.IsNullOrWhiteSpace(companyNumber))
                return
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse { Reason = "Company number was not provided." });

            List<Guid> sites = await Task.Run(() => _siteCollection.FindAll().Select(x => x.Id).Distinct().ToList());

            DateTime start = new DateTime(month.Year, month.Month, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);

            Tuple<DateTime, DateTime> orderedDates = OrderDates(start, end);
            start = orderedDates.Item1;
            end = orderedDates.Item2;

            TimeSpan duration = end - start;
            if (duration.Days < 1)
                return
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "The duration of the rollup must be at least one day"
                        });

            ConcurrentBag<SiteTimesheet> siteTimesheets = new ConcurrentBag<SiteTimesheet>();

            Parallel.ForEach(sites, (site) =>
            {
                SiteTimesheet timesheet = _siteCompiler.Compile(start, duration, site);
                siteTimesheets.Add(timesheet);
            });

            IEnumerable<EmployeeTimesheet> employeeTimesheets = _employeeCompiler.Compile(siteTimesheets.ToList());

            EmployeeTimesheet employeeTimesheet = employeeTimesheets.FirstOrDefault(x => string.Compare(x.Employee.CompanyNumber, companyNumber, true) == 0);

            if (employeeTimesheet != null)
            {
                var attendences = employeeTimesheet.SiteRecords.SelectMany(x => x.ShiftAttendances);

                return
                    Request
                        .CreateResponse(
                            HttpStatusCode.OK,
                            attendences
                                .Select(
                                    x => new
                                    {
                                        id = "",
                                        title = x.Shift.IsFixedDuration == false ? string.Format("{0} ({1} hr{2})", x.Shift.Symbol, x.Shift.HoursWorked, x.Shift.HoursWorked != 1 ? "s" : "") : x.Shift.Symbol,
                                        description = x.Shift.Description,
                                        start = (x.Date - new DateTime(1970, 1, 1)).TotalMilliseconds + (x.ShiftTime == ShiftTime.Night ? TimeSpan.FromHours(12).Milliseconds : 0),
                                        end = (x.Date - new DateTime(1970, 1, 1)).TotalMilliseconds + (x.ShiftTime == ShiftTime.Night ? TimeSpan.FromHours(12).Milliseconds : 0) + TimeSpan.FromHours(12).Milliseconds - 1,
                                        shiftClass = x.ShiftTime == ShiftTime.Day ? "fa-sun-o" : "fa-moon-o"
                                    })
                                    .OrderBy(x => x.start)
                                .ToList()
                        );
            }
            else
            {
                return
                    Request
                        .CreateResponse(
                            HttpStatusCode.OK,
                            new object[0]);
            }
        }

    }
}