using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class EmployeeWorkhoursBySiteSheet
    {
        public EmployeeWorkhoursBySiteSheet()
        {
            WorkHoursList = new List<EmployeeSiteWorkRecord>();
        }

        public DateTime Start
        {
            get;
            set;
        }

        public DateTime End
        {
            get;
            set;
        }

        public Employee Employee
        {
            get;
            set;
        }

        public ICollection<EmployeeSiteWorkRecord> WorkHoursList
        {
            get;
            set;
        }
    }
}
