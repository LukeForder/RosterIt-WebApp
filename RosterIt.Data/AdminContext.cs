using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver.Linq;

namespace RosterIt.Data
{
    public class AdminContext : IAdminContext
    {
        private readonly MongoCollection<Guard> _guardCollection;
        private readonly MongoCollection<Site> _siteCollection;
        private readonly MongoCollection<Shift> _shiftCollection;

        public AdminContext(
            MongoCollection<Guard> guardCollection,
            MongoCollection<Site> siteCollection,
            MongoCollection<Shift> shiftCollection)
        {
            _guardCollection = guardCollection;
            _siteCollection = siteCollection;
            _shiftCollection = shiftCollection;
        }

        public async Task<Models.Guard> AddOrUpdateGuard(Models.Guard guard)
        {
            Contract.Requires(guard != null);

            if (guard.Id == default(Guid))
                guard.Id = Guid.NewGuid();

            await Task.Run(() => _guardCollection.Save(guard));

            return guard;
        }

        public async Task RemoveGuard(Guid guardId)
        {
            var removeGuardCommand = Query<Guard>.EQ(x => x.Id, guardId);

            await Task.Run(() => _guardCollection.Remove(removeGuardCommand));
        }

        public async Task<IEnumerable<Models.Guard>> GetAllGuards()
        {
            return await Task.Run(() => _guardCollection.AsQueryable().ToList());
        }

        public async Task<IEnumerable<Models.Guard>> GetBySite(Guid siteId)
        {
            return await Task.Run(() => 
                _guardCollection
                    .AsQueryable()
                    .Where(guard => guard.SiteId == siteId)
                    .ToList());
        }

        public async Task<Models.Guard> GetById(Guid guardId)
        {
            return await Task.Run(() =>
                {
                    return _guardCollection.FindOneById(guardId);
                });
        }

        public Task<Models.Site> AddOrUpdateSite(Models.Site site)
        {
            throw new NotImplementedException();
        }

        public Task RemoveSite(Guid siteId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Models.Site>> GetAllSites()
        {
            throw new NotImplementedException();
        }

        public Task<Models.Site> GotById(Guid siteId)
        {
            throw new NotImplementedException();
        }

        public Task<Models.Shift> AddOrUpdateShift(Models.Shift shift)
        {
            throw new NotImplementedException();
        }

        public Task RemoveShift(Guid shiftId)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Models.Shift>> IAdminContext.GetAllShifts()
        {
            throw new NotImplementedException();
        }
    }
}
