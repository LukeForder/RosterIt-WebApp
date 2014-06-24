using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Holidays
{
    public interface IHolidayProvider
    {
        Task<IEnumerable<Holiday>> GetHolidays(DateTime from, TimeSpan duration);
    }
}
