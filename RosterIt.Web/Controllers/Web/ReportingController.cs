using MongoDB.Driver;
using RosterIt.Models;
using RosterIt.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RosterIt.Controllers
{
    [Authorize, RoutePrefix("reports")]
    public class ReportingController : Controller
    {
        private readonly MongoRoleProvider _roleProvider;
        private readonly MongoCollection<Models.Site> _siteCollection;

        public ReportingController(
            MongoCollection<Site> siteCollection,
            MongoRoleProvider roleProvider)
        {
            _siteCollection = siteCollection;
            _roleProvider = roleProvider;
        }

        [Route("by-work-month", Name = "WorkMonthReports")]
        public async Task<ActionResult> Download()
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            return View();
        }

        [Route("by-employee", Name = "EmployeeReports")]
        public async Task<ActionResult> Employees()
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            return View();
        }

        [Route("by-site", Name = "SiteReports")]
        public async Task<ActionResult> Site()
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            IEnumerable<Site> sites = 
                _siteCollection
                    .FindAll()
                    .ToList();

            return View(sites);
        }
	}
}