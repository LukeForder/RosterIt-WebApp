using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Web.Commands
{
    public class ManagerResult
    {
        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual string FullName
        {
            get;
            set;
        }

        public virtual string UserName
        {
            get;
            set;
        }

        public bool IsAdministrator
        {
            get;
            set;
        }

        public bool IsSiteManager
        {
            get;
            set;
        }
    }
}
