using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Models
{
    public class GuardRecord
    {
        public virtual Guard Guard
        {
            get;
            set;
        }

        public virtual Shift Shift
        {
            get;
            set;
        }

        public int? OvertimeHours
        {
            get;
            set;
        }

    }
}
