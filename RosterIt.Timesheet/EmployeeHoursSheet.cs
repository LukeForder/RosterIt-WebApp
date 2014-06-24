using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Timesheet
{
    public class EmployeeHoursSheet
    {
        public EmployeeHoursSheet()
        {
            WorkHoursList = new List<WorkHoursList>();
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

        public ICollection<WorkHoursList> WorkHoursList
        {
            get;
            set;
        }
    }
}
