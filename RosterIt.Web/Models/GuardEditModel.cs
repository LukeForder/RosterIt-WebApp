using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RosterIt.Web.Models
{
    public class GuardEditModel
    {
        public Guard Guard
        {
            get;
            set;
        }

        public IEnumerable<Site> Sites
        {
            get;
            set;
        }
    }
}