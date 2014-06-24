using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Models
{
    public class Shift
    {
        public Shift()
        {
            IsFixedDuration = true;
            AvailableDurations = new int[] { 12 };
        }

        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual string Symbol
        {
            get;
            set;
        }

        public virtual string Description
        {
            get;
            set;
        }

        public virtual bool IsFixedDuration
        {
            get;
            set;
        }

        public virtual IEnumerable<int> AvailableDurations
        {
            get;
            set;
        }

        public int HoursWorked
        {
            get;
            set;
        }

    }
}
