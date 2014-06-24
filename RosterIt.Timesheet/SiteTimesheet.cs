using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Timesheet
{
    public class SiteTimesheet
    {
        public SiteTimesheet()
        {
            AttendanceRecords = new HashSet<AttendenceRecord>();
        }

        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual DateTime StartDate
        {
            get;
            set;
        }

        public virtual TimeSpan Period
        {
            get;
            set;
        }

        public virtual Site Site 
        {
            get;
            set; 
        }

        public virtual ICollection<AttendenceRecord> AttendanceRecords
        {
            get;
            set;
        }

        public virtual DateTime GeneratedDate
        {
            get;
            set;
        }
    }
}
