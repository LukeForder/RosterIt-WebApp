using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Data
{
    public interface IAdminContext
    {
        Task<Guard> AddOrUpdateGuard(Guard guard);

        Task RemoveGuard(Guid guardId);

        Task<IEnumerable<Guard>> GetAllGuards();

        Task<IEnumerable<Guard>> GetBySite(Guid siteId);

        Task<Guard> GetById(Guid guardId);

        Task<Site> AddOrUpdateSite(Site site);

        Task RemoveSite(Guid siteId);

        Task<IEnumerable<Site>> GetAllSites();

        Task<Site> GotById(Guid siteId);

        Task<Shift> AddOrUpdateShift(Shift shift);

        Task RemoveShift(Guid shiftId);

        Task<IEnumerable<Shift>> GetAllShifts();
    }
}
