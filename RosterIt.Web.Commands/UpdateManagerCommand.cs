using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Web.Commands
{
    [DataContract]
    public class UpdateManagerCommand
    {
        [DataMember]
        public string UserName
        {
            get;
            set;
        }

        [DataMember]
        public string FullName
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

        [DataMember]
        public bool IsAdministrator
        {
            get;
            set;
        }

        public static ValidationResult Validate(UpdateManagerCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            if (string.IsNullOrWhiteSpace(command.UserName))
                return ValidationResult.NotValid("Manager must be assigned a user name");

            if (string.IsNullOrWhiteSpace(command.FullName))
                return ValidationResult.NotValid("Manger's name must be given");

            if (!(command.IsAdministrator || command.IsSiteManager))
                return ValidationResult.NotValid("Manager must be assigned at least one role");

            return ValidationResult.Valid();
        }
    }
}
