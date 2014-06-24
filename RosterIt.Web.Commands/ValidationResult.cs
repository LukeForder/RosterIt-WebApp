using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Web.Commands
{
    public class ValidationResult
    {
        public static ValidationResult Valid()
        {
            return new ValidationResult();
        }

        public static ValidationResult NotValid(string message)
        {
            return new ValidationResult(message);
        }

        public ValidationResult()
            : this(new string[0])
        {
        }

        public ValidationResult(string errorMessage)
            : this(new string[] {  errorMessage })
        {
        }

        public ValidationResult(IEnumerable<string> errorMessages)
        {
           Errors = errorMessages != null ?  
                errorMessages.ToList() : 
                new List<string>();
        }

        public bool IsValid
        {
            get
            {
                return Errors == null || !Errors.Any();
            }
        }

        public ICollection<string> Errors
        {
            get;
            set;
        }
    }
}
