using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Data
{
    public interface IRosterContext
    {
        Task<IEnumerable<RosterDetails>> GetByDate(DateTime date);
    }
}
