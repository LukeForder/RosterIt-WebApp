using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RosterIt.Web.Models
{
    [DataContract]
    public class ErrorResponse
    {
        [DataMember]
        public string Reason
        {
            get;
            set;
        }
    }
}