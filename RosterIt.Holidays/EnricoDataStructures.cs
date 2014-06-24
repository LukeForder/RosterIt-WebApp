using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Holidays
{
    internal class EnricoDate
    {
        public int day { get; set; }
        public int month { get; set; }
        public int year { get; set; }

        [JsonIgnore]
        public DateTime Date
        {
            get
            {
                return new DateTime(year, month, day);
            }
        }
    }
    
    internal class EnricoHoliday
    {
        public string localName { get; set; }
        public string englishName { get; set; }
        public EnricoDate date { get; set; }
    }
}
