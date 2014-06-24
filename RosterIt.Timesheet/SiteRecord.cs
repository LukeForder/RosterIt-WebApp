using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class SiteRecord
    {
        public SiteRecord()
        {
            ShiftAttendances = new List<ShiftAttendance>();
        }

        public Site Site
        {
            get;
            set;
        }

        public ICollection<ShiftAttendance> ShiftAttendances
        {
            get;
            set;
        }
    }
}
