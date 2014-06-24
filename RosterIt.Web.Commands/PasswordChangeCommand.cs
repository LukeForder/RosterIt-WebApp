using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace RosterIt.Web.Commands
{
    [DataContract]
    public class PasswordChangeCommand
    {
        public static int MinPasswordLength
        {
            get
            {
                return 8;
            }
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

        public static ValidationResult Valid(PasswordChangeCommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            if (string.IsNullOrWhiteSpace(command.Password))
                return ValidationResult.NotValid("Can not be empty");

            if (string.Compare(command.Password, command.PasswordConfirmation) != 0)
                return ValidationResult.NotValid("Doesn't match the password confirmation");

            List<string> passwordComplexityMessages = new List<string>();
            if (command.Password.Length < PasswordChangeCommand.MinPasswordLength)
                passwordComplexityMessages.Add(string.Format("Length must be at least {0} characters or more", PasswordChangeCommand.MinPasswordLength));

            var containsLowerCase = command.Password.Any(x => char.IsLower(x));
            if (!containsLowerCase)
                passwordComplexityMessages.Add("Must contain at least one lowercase letter");

            var containsUpperCase = command.Password.Any(x => char.IsUpper(x));
            if (!containsUpperCase)
                passwordComplexityMessages.Add("Must contain at least one upper letter");

            var containsNumbers = command.Password.Any(x => char.IsNumber(x));
            if (!containsNumbers)
                passwordComplexityMessages.Add("Must contain at least one number");

            var containsPunctuation = command.Password.Any(x => char.IsPunctuation(x));
            if (!containsPunctuation)
                passwordComplexityMessages.Add("Must contain at least punctuation character (!, \", #, %, &, ', (, ), *, -, ?, @, ; etc)");

            return (passwordComplexityMessages.Count == 0) ?
                ValidationResult.Valid() :
                new ValidationResult
                {
                    Errors = passwordComplexityMessages
                };
        }
    }
}
