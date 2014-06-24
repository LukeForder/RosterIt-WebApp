using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Models
{
    public class RosterDetails
    {
        public DateTime Date
        {
            get;
            set;
        }

        public ShiftTime ShiftTime
        {
            get;
            set;
        }

        public Site Site
        {
            get;
            set;
        }
    }
}
