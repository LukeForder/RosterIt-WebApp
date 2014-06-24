using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Timesheet
{
    public class AttendenceRecord
    {
        public virtual Employee Guard
        {
            get;
            set;
        }

        public virtual ICollection<ShiftAttendance> Attendance
        {
            get;
            set;
        }
    }
}
