using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Models
{
    public class Guard
    {
        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual string CompanyNumber
        {
            get;
            set;
        }

        public virtual string FullName
        {
            get;
            set;
        }

        public virtual Guid SiteId
        {
            get;
            set;
        }
    }
}
