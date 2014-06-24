using RosterIt.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RosterIt.Audit
{
    public abstract class AuditEntry<T> : AuditEntry
    {
        public T Old
        {
            get;
            set;
        }

        public T New
        {
            get;
            set;
        }
    }
}