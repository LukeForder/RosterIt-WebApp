using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RosterIt.Web.Models
{
    public class RosterInfo
    {
        public DateTime Date
        {
            get;
            set;
        }

        public ShiftTime Shift
        {
            get;
            set;
        }

        public Guid SiteId
        {
            get;
            set;
        }
    }
}