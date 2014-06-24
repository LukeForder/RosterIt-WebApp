using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Holidays
{
    public class HolidayServiceException : Exception
    {
        public HolidayServiceException()
        {
        }

        public HolidayServiceException(string message)
            : base(message)
        {
        }
    }
}
