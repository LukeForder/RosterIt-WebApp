using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using RosterIt.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RosterIt.Web.Controllers.Web
{
    [Authorize]
    [RoutePrefix("admin/sites")]
    public class SitesController : Controller
    {
        private readonly MongoCollection<Site> _sitesCollection;
        private readonly ILog _log;
        private readonly MongoCollection<Manager> _managerCollection;

        public SitesController(
            MongoCollection<Site> sitesCollection,
            MongoCollection<Manager> managersCollection)
        {
            _sitesCollection = sitesCollection;
            _managerCollection = managersCollection;
            _log = LogManager.GetCurrentClassLogger();
        }

        [HttpGet, Route("~/admin/sites", Name="AllSites")]
        public async Task<ActionResult> Index()
        {

            var sites = await Task.Run(() =>
                _sitesCollection.FindAll()
                    .ToList());

            return View(sites);
        }

        [HttpGet, Route("new", Name = "NewSite")]
        public ActionResult New()
        {
            var siteManagers = 
                _managerCollection
                    .Find(Query<Manager>.EQ(x => x.IsSiteManager, true))
                    .ToList();

            return View(siteManagers);
        }

        [HttpGet, Route("{id:guid}/edit", Name ="EditSite")]
        public ActionResult Edit(Guid id)
        {
            var site =
                _sitesCollection
                    .FindOne(
                        Query<Site>.EQ(x => x.Id, id));

            if (site == null)
                throw new HttpException(404, "Site was not found");

            var siteManagers = 
                _managerCollection
                    .Find(Query<Manager>.EQ(x => x.IsSiteManager, true))
                    .ToList();


            return View(new SiteEditModel
            {
                ManagerId = (site.Manager != null) ? site.Manager.Id : Guid.Empty,
                Managers = siteManagers.Select(x => new ManagerDetails { Id = x.Id, FullName = x.FullName }).ToArray(),
                Site = site
            });
        }
	}
}