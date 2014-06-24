using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Timesheet
{
    public class Employee
    {
        public virtual string CompanyNumber
        {
            get;
            set;
        }

        public virtual string FullName
        {
            get;
            set;
        }
    }
}
