using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace RosterIt.Web.Models
{
    public class MongoRoleProvider 
    {
        public static readonly string AdministratorRole = "Administrator";
        public static readonly string SiteManagerRole = "SiteManager";

        private readonly MongoCollection<Manager> _managerCollection;

        public MongoRoleProvider(
            MongoCollection<Manager> managerCollections)
        {
            _managerCollection = managerCollections;
        }

        public IEnumerable<string> GetRoles(string userName)
        {
            var manager =
                    _managerCollection
                        .FindOne(Query<Manager>.EQ(x => x.UserName, userName));

            if (manager == null)
                return new string[0];

            var roles = new List<string>();

            if (manager.IsAdministrator)
                roles.Add(AdministratorRole);

            if (manager.IsSiteManager)
                roles.Add(SiteManagerRole);

            return roles.AsReadOnly();
        }

        public async Task<IEnumerable<string>> GetRolesAsync(string userName)
        {
            var manager =
                await Task.Run(() =>
                    _managerCollection
                        .FindOne(Query<Manager>.EQ(x => x.UserName, userName)));

            if (manager == null)
                return new string[0];

            var roles = new List<string>();

            if (manager.IsAdministrator)
                roles.Add(AdministratorRole);

            if (manager.IsSiteManager)
                roles.Add(SiteManagerRole);

            return roles.AsReadOnly();
        }

        public bool InRole(string userName, string role)
        {
            var roles = GetRoles(userName);

            return roles.Contains(role);
        }

        public async Task<bool> InRoleAsync(string userName, string role)
        {
            var roles = await GetRolesAsync(userName);

            return roles.Contains(role);
        }
    }
}