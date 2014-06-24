using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using RosterIt.Web.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RosterIt.Web.Controllers.Web
{
    [Authorize, RoutePrefix("admin/guard")]
    public class GuardController : Controller
    {
        private readonly MongoCollection<Site> _sitesCollection;
        private readonly MongoCollection<Guard> _guardsCollection;
        private readonly MongoRoleProvider _roleProvider;
        private readonly ILog _log;

        public GuardController(
            MongoCollection<Site> sitesCollection,
            MongoCollection<Guard> guardsCollection,
            MongoRoleProvider roleProvider)
        {
            _sitesCollection = sitesCollection;
            _guardsCollection = guardsCollection;
            _roleProvider = roleProvider;
            _log = LogManager.GetCurrentClassLogger();
        }
        
        [Route("~/admin/guards", Name="AllGuards")]
        public async Task<ActionResult> Index()
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            return View();
        }

        [Route("new", Name="NewGuard")]
        public async Task<ActionResult> New()
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            var sites =
                _sitesCollection
                    .FindAll()
                    .Select(x => new { id = x.Id, name = x.Name })
                    .ToList();

            return View(sites);
        }

        [Route("{id:guid}", Name = "EditGuard")]
        public async Task<ActionResult> Edit(Guid id)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            var sites =
               _sitesCollection
                   .FindAll()
                   .ToList();

            var guard =
                _guardsCollection
                    .FindOne(Query<Guard>.EQ(x => x.Id, id));

            if (guard == null)
                throw new HttpException(404, "The guard was not found.");

            return View(new GuardEditModel
            {
                Guard = guard,
                Sites = sites
            });
        }
	}
}