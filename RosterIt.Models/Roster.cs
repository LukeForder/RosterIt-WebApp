using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Models
{
    public class Roster
    {
        public Roster()
        {
            GuardRecords = new HashSet<GuardRecord>();
            AvailableShifts = new HashSet<Shift>();
            Id = Guid.NewGuid();
        }

        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual ShiftTime ShiftTime
        {
            get;
            set;
        }

        public virtual DateTime Date
        {
            get;
            set;
        }

        public virtual Site Site
        {
            get;
            set;
        }

        public virtual ICollection<GuardRecord> GuardRecords
        {
            get;
            set;
        }

        public virtual ICollection<Shift> AvailableShifts
        {
            get;
            set;
        }
    }
}
