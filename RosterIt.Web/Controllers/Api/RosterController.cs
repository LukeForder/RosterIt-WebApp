using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using RosterIt.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RosterIt.Web;
using MongoDB.Driver.Linq;
using System.Web;
using System.Threading.Tasks;

namespace RosterIt.Controllers
{
    [Authorize, RoutePrefix("rosters")]
    public class RosterController : ApiController
    {
        private readonly MongoCollection<Manager> _managerCollection;
        private readonly MongoCollection<Roster> _rosterCollection;
        private readonly MongoCollection<Site> _siteCollection;
        private readonly MongoCollection<Guard> _guardCollection;

        private readonly MongoRoleProvider _roleProvider;
        private readonly ILog _log;

        public RosterController(
            MongoCollection<Roster> rosterCollection,
            MongoCollection<Site> siteCollection,
            MongoCollection<Manager> managerCollection,
            MongoCollection<Guard> guardCollection,
            MongoRoleProvider roleProvider)
        {
            _managerCollection = managerCollection;
            _rosterCollection = rosterCollection;
            _siteCollection = siteCollection;
            _guardCollection = guardCollection;

            _roleProvider = roleProvider;
            _log = LogManager.GetCurrentClassLogger();
        }

        [Route("", Name="RostersByDate")]
        public HttpResponseMessage Get(DateTime date)
        {
            try
            {

                // force UTC
                // Otherwise we get inconsistent behaviour depending on whatever Kind the system decides date is
                var utcDate = new DateTime(date.Ticks, DateTimeKind.Utc);

                string userName = User.Identity.Name;
                
                var manager =
                    _managerCollection
                        .FindOne(
                            Query<Manager>.EQ(x => x.UserName, userName.ToLower())
                        );


                List<Site> managedSites = 
                    (manager.IsAdministrator) ? // admins have access to all sites
                        _siteCollection
                            .AsQueryable()
                            .ToList() :
                        _siteCollection
                            .AsQueryable()
                            .Where(x => x.Manager.Id == manager.Id)
                            .ToList();

                var siteIds = managedSites.Select(x => x.Id).ToList();

                var existingRosters =
                    _rosterCollection
                        .AsQueryable()
                        .Where(x => x.Site.Id.In(siteIds))
                        .Where(x => x.Date == utcDate)
                        .ToList()
                        .GroupBy(x => x.Site.Id);

                var rosters = new List<dynamic>();
                foreach (var site in managedSites)
                {
                    var rostersForSiteGroup = existingRosters.FirstOrDefault(x => x.Key == site.Id);
                    var rostersForSite = rostersForSiteGroup != null ? rostersForSiteGroup.ToList() : new List<Roster>();


                    rosters.Add(new
                    {
                        name = site.Name,
                        id = site.Id,
                        shifts = new dynamic[] {
                            new {
                                name = "Day",
                                submitted = rostersForSite.Any(r => r.ShiftTime == ShiftTime.Day)
                            },
                            new {
                                name = "Night",
                                submitted = rostersForSite.Any(r => r.ShiftTime == ShiftTime.Night)
                            }
                        }
                    });

                }

                //string k = Url.Link("Roster", new { date = DateTime.Now.Date, Shift = ShiftTime.Day, siteId = Guid.NewGuid() });

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    rosters);

            }
            catch (Exception e)
            {
                _log.Error(e.ExceptionDescription());

                return
                    Request.CreateResponse(
                        HttpStatusCode.InternalServerError,
                        new ErrorResponse
                        {
                            Reason = string.Format("Something went wrong retrieving the rosters for the {0}.", date.ToShortDateString())
                        });
            }

        }

        [Route("", Name="Roster")]
        public HttpResponseMessage Get(DateTime date, ShiftTime shift, Guid siteId)
        {
            try
            {
                // force UTC
                // Otherwise we get inconsistent behaviour depending on whatever Kind the system decides date is
                var utcDate = new DateTime(date.Ticks, DateTimeKind.Utc);

                var roster =
                  _rosterCollection
                      .FindOne(
                          Query.And(
                            Query<Roster>.EQ(x => x.Date, utcDate),
                            Query<Roster>.EQ(x => x.Site.Id, siteId),
                            Query<Roster>.EQ(x => x.ShiftTime, shift)));



                if (roster != null)
                {
                    roster = new ServerRoster
                    {
                        AvailableShifts = roster.AvailableShifts.Where(x => x.Id != Shifts.Overtime.Id).ToArray(),
                        Date = roster.Date,
                        GuardRecords = roster.GuardRecords,
                        Persisted = true,
                        ShiftTime = roster.ShiftTime,
                        Site = roster.Site,
                        IsAdministator = _roleProvider.InRole(User.Identity.Name, MongoRoleProvider.AdministratorRole) || IsUserSiteManager(roster.Site)
                    };

                    return Request.CreateResponse(
                            HttpStatusCode.OK,
                            roster);
                }

                roster = GenerateRoster(utcDate, shift, siteId);

                return 
                    Request.CreateResponse(
                        HttpStatusCode.OK,
                        roster);
            }
            catch(Exception e)
            {
                _log.Error(e.ExceptionDescription());

                return
                    Request.CreateResponse(
                        HttpStatusCode.InternalServerError,
                        new ErrorResponse
                        {
                            Reason = "Something went wrong generating the roster."
                        });
            }

        }


        private Roster GenerateRoster(DateTime date, ShiftTime shift, Guid siteId)
        {
            // force UTC
            // Otherwise we get inconsistent behaviour depending on whatever Kind the system decides date is
            var utcDate = new DateTime(date.Ticks, DateTimeKind.Utc);


            var guards =
              _guardCollection
                  .Find(
                      Query<Guard>.EQ(x => x.SiteId, siteId)
                  );

            var site =
                _siteCollection
                    .FindOne(
                        Query<Site>.EQ(x => x.Id, siteId)
                    );

            if (site == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "Site was not found");

            var shifts = Shifts.All.Where(x => x.Id != Shifts.Overtime.Id).ToList();


            Roster roster = new ServerRoster
            {
                Date = utcDate,
                ShiftTime = shift,
                Site = site,
                AvailableShifts = shifts,
                GuardRecords =
                    guards
                    .Select(x => new GuardRecord
                        {
                            Guard = x,
                            Shift = shifts.First(s => s.Symbol == "O")
                        })
                    .ToList(),
                Persisted = false,
                IsAdministator = _roleProvider.InRole(User.Identity.Name, MongoRoleProvider.AdministratorRole) || IsUserSiteManager(site)
            };

            return roster;
        }


        [Route("", Name = "SubmitRoster")]
        public async Task<HttpResponseMessage> Post(Guid siteId, DateTime date, ShiftTime shift, [FromBody] Roster roster)
        {
            try
            {
                // force UTC
                // Otherwise we get inconsistent behaviour depending on whatever Kind the system decides date is
                var utcDate = new DateTime(date.Ticks, DateTimeKind.Utc);


                var persistedRoster =
                  _rosterCollection
                      .FindOne(
                          Query.And(
                            Query<Roster>.EQ(x => x.Date, utcDate),
                            Query<Roster>.EQ(x => x.Site.Id, siteId),
                            Query<Roster>.EQ(x => x.ShiftTime, shift)));

                var site = 
                    _siteCollection
                        .FindOne(
                            Query<Site>.EQ(x => x.Id, siteId));



                if (site == null)
                    return
                        Request
                            .CreateResponse(
                                HttpStatusCode.NotFound,
                                new ErrorResponse
                                {
                                    Reason = "Site was not found"
                                });

                var manager = this._managerCollection.FindOne(Query<Manager>.EQ(X => X.UserName, User.Identity.Name));

                // only managers can update rosters once they have been submitted
                if (persistedRoster != null && !(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole) || manager.Id == site.Manager.Id))
                {
                    return
                        Request.CreateResponse(
                            HttpStatusCode.Forbidden,
                            new ErrorResponse
                            {
                                Reason = "You can not edit a submitted roster, an administrator or the site manager needs to make the changes."
                            });
                }

                
                // if the roster hasn't been created, check the current user is an administrator or the manager for the site
                if ((!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)) && manager.Id != site.Manager.Id))
                    return
                        Request
                            .CreateResponse(
                                HttpStatusCode.Forbidden,
                                new ErrorResponse
                                {
                                    Reason = "You are not the manager for this site."
                                });

                // set the defining attributes for the roster
                roster.Date = utcDate;
                roster.ShiftTime = shift;
                roster.Site = site;

                if (persistedRoster != null)
                    roster.Id = persistedRoster.Id ;

                _rosterCollection.Save(roster);

                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                _log.Error(e.ExceptionDescription());

                return
                    Request
                        .CreateResponse(
                            HttpStatusCode.InternalServerError,
                            new ErrorResponse
                            {
                                Reason = "Something went wrong while saving the roster."
                            });
            }

        }


        private bool IsUserSiteManager(Site site)
        {

            var manager = this._managerCollection.FindOne(Query<Manager>.EQ(X => X.UserName, User.Identity.Name));

            bool isSiteManager = (manager != null) ? manager.Id == site.Manager.Id : false;

            return isSiteManager;
            
        }

    }
}
