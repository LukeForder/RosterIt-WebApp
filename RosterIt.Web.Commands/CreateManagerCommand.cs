using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Web.Commands
{
    [DataContract]
    public class CreateManagerCommand
    {
        public CreateManagerCommand()
        {
            ManagedSiteIds = new Guid[0];
        }

        [DataMember]
        public string FullName
        {
            get;
            set;
        }

        [DataMember]
        public string UserName
        {
            get;
            set;
        }

        [DataMember]
        public string Password
        {
            get;
            set;
        }

        [DataMember]
        public string PasswordConfirmation
        {
            get;
            set;
        }

        [DataMember]
        public IEnumerable<Guid> ManagedSiteIds
        {
            get;
            set;
        }

        [DataMember]
        public bool IsAdministrator
        {
            get;
            set;
        }

        [DataMember]
        public bool IsSiteManager
        {
            get;
            set;
        }
    }
}
