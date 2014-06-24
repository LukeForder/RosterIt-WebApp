using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class EmployeeSiteWorkRecord
    {
        public EmployeeSiteWorkRecord()
        {
            WorkHoursList = new List<WorkHoursList>();
        }

        public Site Site
        {
            get;
            set;
        }

        public ICollection<WorkHoursList> WorkHoursList
        {
            get;
            set;
        }
    }
}
