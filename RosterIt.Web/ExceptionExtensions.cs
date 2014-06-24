using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace RosterIt.Web
{
    public static class ExceptionExtensions
    {
        public static string ExceptionDescription(this Exception e)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("Type: '{0}'\n", e.GetType().FullName);
            builder.AppendFormat("Message: '{0}'\n", e.Message);
            builder.AppendFormat(":Stack Trace: '{0}'\n", e.StackTrace);

            if (e.InnerException != null)
            {
                builder.AppendLine("--- Inner Exception ---");
                builder.Append(e.InnerException.ExceptionDescription());
            }

            return builder.ToString();
        }
    }
}