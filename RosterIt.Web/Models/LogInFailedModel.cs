using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RosterIt.Web.Models
{
    public class LogInFailedModel
    {
        public string Message
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }
    }
}