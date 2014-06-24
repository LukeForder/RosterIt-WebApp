using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RosterIt.Web.Models
{
    public class ServerRoster : Roster
    {
        public bool Persisted
        {
            get;
            set;
        }

        public bool IsAdministator
        {
            get;
            set;
        }
    }
}