using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RosterIt.Timesheet
{
    public class SiteNotFoundException : Exception
    {
        readonly Guid _id;
        private Exception _innerException;

        public SiteNotFoundException(Guid id)
        {
            _id = id;
        }

        public SiteNotFoundException(Guid id, string message)
            : base(message)
        {
            _id = id;
        }

        public SiteNotFoundException(Guid id, string message, Exception innerException)
            : base(message, innerException)
        {
            _id = id;
        }
    }
}
