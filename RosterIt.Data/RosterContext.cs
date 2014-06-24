using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Data
{
    public class RosterContext : IRosterContext
    {
        private readonly MongoCollection<Roster> _rosterCollection;

        public RosterContext(
            MongoCollection<Roster> rosterCollection)
        {
            _rosterCollection = rosterCollection;
        }

        public async Task<IEnumerable<Models.RosterDetails>> GetByDate(DateTime date)
        {
            var query = Query.And(
                Query<Roster>.GTE(x => x.Date, date.Date),
                Query<Roster>.LTE(x => x.Date, date.AddDays(1).Date));

            var rostersOnDate = await Task.Run(() => _rosterCollection.Find(query));

            var details =
                rostersOnDate
                    .Select(x => 
                        new RosterDetails
                        {
                            Date = x.Date,
                            Site = x.Site,
                            ShiftTime = x.ShiftTime 
                        })
                    .ToList();

            return details;
        }
    }
}
 