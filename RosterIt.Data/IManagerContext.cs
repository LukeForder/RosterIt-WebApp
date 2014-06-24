using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Data
{
    public interface IManagerContext
    {
        Task<IEnumerable<Site>> Sites();

        Task<Roster> GetRoster(DateTime date, ShiftTime shiftTime, Guid siteId);

        Task SubmitRoster(Roster roster);
    }
}
