using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace RosterIt.Web.Models
{
    public class MongoPrincipal : IPrincipal
    {
        private readonly MongoRoleProvider _roleProvider;
        private readonly IIdentity _userIdentity;

        public MongoPrincipal(
            IIdentity userIdentity,
            MongoRoleProvider roleProvider)
        {
            _roleProvider = roleProvider;
            _userIdentity = userIdentity;
        }

        public IIdentity Identity
        {
            get
            {
                return _userIdentity;
            }
        }

        public bool IsInRole(string role)
        {
            return _roleProvider.InRole(_userIdentity.Name, role);
        }

        public IEnumerable<string> GetRoles()
        {
            return _roleProvider.GetRoles(_userIdentity.Name);
        }
    }
}