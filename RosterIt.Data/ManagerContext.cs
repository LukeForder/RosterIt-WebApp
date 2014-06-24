using MongoDB.Driver;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using System.Diagnostics.Contracts;

namespace RosterIt.Data
{
    public class ManagerContext : IManagerContext
    {
        private readonly MongoCollection<Site> _sitesCollection;
        private readonly MongoCollection<Roster> _rosterCollection;
        private readonly MongoCollection<Guard> _guardsCollection;
        private readonly MongoCollection<Shift> _shiftCollection;

        public ManagerContext(
            MongoCollection<Site> sitesCollection, 
            MongoCollection<Roster> rosterCollection,
            MongoCollection<Guard> guardsCollection,
            MongoCollection<Shift> shiftCollection)
        {
            _sitesCollection = sitesCollection;
            _rosterCollection = rosterCollection;
            _guardsCollection = guardsCollection;
            _shiftCollection = shiftCollection;
        }

        public async Task<IEnumerable<Models.Site>> Sites()
        {
            return await 
                Task.Run(() =>
                    _sitesCollection
                        .AsQueryable()
                        .ToList());
        }

        public async Task<Models.Roster> GetRoster(DateTime date, Models.ShiftTime shiftTime, Guid siteId)
        {
            Contract.Requires(siteId != null);

            // check if the roster exists 
                // yes -> return the existing roster

            // otherwise
                // fetch the site
                // fetch the guards on the site 
                // fetch the available shifts
                // compose the roster
            // return the roster

            var persistedRoster =
                await
                    Task.Run(() => _rosterCollection
                        .AsQueryable()
                        .FirstOrDefault(roster => roster.Date.Date == date.Date && roster.ShiftTime == shiftTime && roster.Site.Id == siteId));

            if (persistedRoster != null)
                return persistedRoster;

            var savedSite = await Task.Run(() => 
                _sitesCollection.FindOneById(siteId));

            if (savedSite == null)
                throw new ArgumentException("The site doesn't exist", "siteId");

            var savedGuards = await Task.Run(() => 
                _guardsCollection
                    .AsQueryable()
                    .Where(guard => guard.SiteId == savedSite.Id)
                    .ToList());

            var savedShifts = await Task.Run(() =>
                _shiftCollection
                    .AsQueryable()
                    .ToList());

            return new Roster
            {
                Date = date.Date,
                GuardRecords =
                    savedGuards
                        .Select(x => new GuardRecord {  Guard = x })
                        .ToList(),
                ShiftTime = shiftTime,
                Site = savedSite,
                AvailableShifts = savedShifts
            };

        }

        public Task SubmitRoster(Models.Roster roster)
        {
            // check if roster exists
                // yes -> is user admin
                    // yes -> overwrite
                    // no -> invalid operation
                // no -> save the roster

            throw new NotImplementedException();
        }
    }
}
