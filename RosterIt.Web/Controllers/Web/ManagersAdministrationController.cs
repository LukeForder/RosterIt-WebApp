using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using RosterIt.Models;
using RosterIt.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RosterIt.Web.Controllers.Web
{
    [Authorize, RoutePrefix("admin/manager")]
    public class ManagersAdministrationController : Controller
    {
        private readonly MongoCollection<Manager> _managersCollection;
        private readonly MongoRoleProvider _roleProvider;
        private readonly ILog _log;

        public ManagersAdministrationController(
            MongoCollection<Manager> managersCollection,
            MongoRoleProvider roleProvider)
        {
            _managersCollection = managersCollection;
            _roleProvider = roleProvider;
            _log = LogManager.GetCurrentClassLogger();
        }

        [HttpGet, Route("~/admin/managers", Name = "AllManagers")]
        public async Task<ActionResult> Index()
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            var managers =
                _managersCollection
                    .AsQueryable()
                    .ToList();

            var model =
                managers
                    .Select(x => new
                    {
                        isAdministrator = x.IsAdministrator,
                        isSiteManager = x.IsSiteManager,
                        userName = x.UserName,
                        fullName = x.FullName,
                        id = x.Id
                    })
                    .ToList();

           var json = await JsonConvert.SerializeObjectAsync(model);

            return View(new JsonModel { Json = json });
        }

        [HttpGet, Route("new", Name = "NewManager")]
        public async Task<ActionResult> New()
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            return View();
        }

        [HttpGet, Route("{id:guid}", Name="EditManager")]
        public async Task<ActionResult> Edit(Guid id)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");


            var manager = _managersCollection.FindOneById(id);
            if (manager == null)
                throw new HttpException((int)HttpStatusCode.Forbidden, "Manager was not found.");

            return View(new 
               Manager {
                    Id = manager.Id,
                    FullName = manager.FullName,
                    UserName = manager.UserName,
                    IsAdministrator = manager.IsAdministrator,
                    IsSiteManager = manager.IsSiteManager
                });
        }

        [/*RequireHttps,*/ HttpGet, Route("{id:guid}/password", Name = "ChangeManagerPassword")]
        public async Task<ActionResult> EditPassword(Guid id)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                throw new HttpException((int)HttpStatusCode.Forbidden, "You are not permitted to perform this action");

            var manager = _managersCollection.FindOneById(id);
            if (manager == null)
                throw new HttpException(404, "Manager was not found.");

            return View(new
            Manager {
                Id = manager.Id,
                FullName = manager.FullName,
                UserName = manager.UserName
            });
        }


	}
}