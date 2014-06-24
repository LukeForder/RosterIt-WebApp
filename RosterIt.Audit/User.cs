using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Audit
{
    public class User
    {
        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual string UserName
        {
            get;
            set;
        }

        public virtual string DisplayName
        {
            get;
            set;
        }
    }
}
