using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using RosterIt.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RosterIt.Controllers
{
    [Authorize]
    public class ManagerController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Roster(Guid siteId, DateTime date, ShiftTime shift)
        {
            return View(new RosterInfo
                {
                    Date = date,
                    Shift = shift,
                    SiteId = siteId
                });
        }

        [HttpPost]
        public JsonResult Roster(Roster roster)
        {

            return Json(new { Success = true });

        }
    }
}