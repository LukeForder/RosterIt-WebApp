using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Audit
{
    public abstract class AuditEntry
    {
        public AuditEntry()
        {
            Id = Guid.NewGuid();
            Date = DateTime.Now;
        }

        public virtual ChangeType ChangeType
        {
            get;
            set;
        }

        public virtual Guid Id
        {
            get;
            set;
        }

        public virtual DateTime Date
        {
            get;
            set;
        }

        public virtual User User
        {
            get;
            set;
        }
    }
}
