using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Models
{
    public class Manager
    {
        public Manager()
        {
            ManagedSites = new HashSet<Guid>();
        }

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

        public virtual string PasswordHash
        {
            get;
            set;
        }

        public virtual string EncodedSalt
        {
            get;
            set;
        }

        public virtual ICollection<Guid> ManagedSites
        {
            get;
            set;
        }

        public virtual bool IsAdministrator
        {
            get;
            set;
        }

        public virtual bool IsSiteManager
        {
            get;
            set;
        }
    }
}
