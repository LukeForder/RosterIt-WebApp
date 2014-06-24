using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Timesheet
{
    public class ShiftAttendance
    {
        public virtual Shift Shift
        {
            get;
            set;
        }

        public virtual TimeSpan WorkedDuration
        {
            get;
            set;
        }

        public virtual DateTime Date
        {
            get;
            set;
        }

        public virtual ShiftTime ShiftTime
        {
            get;
            set;
        }
    }
}
