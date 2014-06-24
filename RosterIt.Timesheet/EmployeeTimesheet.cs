using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Timesheet
{
    public class EmployeeTimesheet
    {
        public EmployeeTimesheet()
        {
            SiteRecords = new List<SiteRecord>();
        }

        public virtual Employee Employee
        {
            get;
            set;
        }

        public virtual ICollection<SiteRecord> SiteRecords
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
