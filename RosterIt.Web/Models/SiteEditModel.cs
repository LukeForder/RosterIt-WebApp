using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RosterIt.Web.Models
{
    public class SiteEditModel
    {
        public Site Site
        {
            get;
            set;
        }

        public IEnumerable<ManagerDetails> Managers
        {
            get;
            set;
        }

        public Guid ManagerId
        {
            get;
            set;
        }
    }
}