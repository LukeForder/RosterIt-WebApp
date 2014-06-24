using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Models
{
    public class Site
    {
        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual string Name
        {
            get;
            set;
        }

        public virtual ManagerDetails Manager
        {
            get;
            set;
        }

        
    }
}
