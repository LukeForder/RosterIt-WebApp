using MongoDB.Driver;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Test.Data
{
    public class Program
    {
        static void Main(params string[] args)
        {
            string mongoConnectionString = ConfigurationManager.AppSettings["mongoConnectionString"];
            if (mongoConnectionString == null)
                throw new ConfigurationErrorsException("missing app setting 'mongoConnectionString'");

            MongoUrl url = MongoUrl.Create(mongoConnectionString);

            MongoClient client = new MongoClient(url);

            MongoServer server = client.GetServer();

            MongoDatabase database = server.GetDatabase(url.DatabaseName);

            var managers = CreateManagers(database);

            var sites = CreateSites(database, managers);

            CreateGuards(database, sites);
        }

        private static IEnumerable<Guard> CreateGuards(MongoDatabase database, IEnumerable<Site> sites)
        {
            var guardsCollection = database.GetCollection<Guard>("guards");

            guardsCollection.RemoveAll();

            var sitesArray = sites.ToArray();

            var names = new string[] { "Bob", "James", "Lucy", "Kevin", "Jim", "Clive", "Betty", "Gracie", "William", "Eve", "Dorian", "Beth" };

            var nameRandomiser = new Random(214125215);

            var guards =
                Enumerable.
                    Range(0, sitesArray.Length * 3).
                    Select(x  => 
                    {
                        string name = names[nameRandomiser.Next()%names.Length];

                        return
                            new Guard
                            {
                                Id = Guid.NewGuid(),
                                FullName = name,
                                CompanyNumber = string.Format("{0}{1}", name, x),
                                SiteId = sitesArray[x % sitesArray.Length].Id
                            };
                    }).
                    ToList();

            guards.ForEach(x => guardsCollection.Save(x));

            return guards;
        }

        private static IEnumerable<Site> CreateSites(MongoDatabase database, IEnumerable<Manager> managers)
        {
            
            var sitesCollection = database.GetCollection<Site>("sites");

            sitesCollection.RemoveAll();
            
            var sites =
                managers.
                    Where(x => x.IsSiteManager).
                    SelectMany((manager, idx) =>
                        Enumerable.
                            Range(0, idx + 1).
                            Select(i => 
                                new Site 
                                {
                                    Id = Guid.NewGuid(),
                                    Manager = new ManagerDetails 
                                    {
                                        FullName = manager.FullName,
                                        Id = manager.Id 
                                    },
                                    Name = string.Format("Site {0}", idx + i)
                                }).
                            ToList()
                    ).
                    ToList();

            sites.ForEach(x => sitesCollection.Save(x));

            return sites;
        }

        private static Tuple<string, string> GenerateEncodedPasswordHashAndSalt(string password)
        {            
            byte[] salt =  PasswordManager.GenerateSalt(16);
            string saltEncoded = PasswordManager.Encode(salt);
            
            byte[] passwordHash = PasswordManager.ComputeHash(password, salt);
            string passwordHashEncoded = PasswordManager.Encode(passwordHash);

            return new Tuple<string, string>(passwordHashEncoded, saltEncoded);
        }

        private static IEnumerable<Manager> CreateManagers(MongoDatabase database)
        {
            var adminCredentials = GenerateEncodedPasswordHashAndSalt("secret1");

            Manager admin = new Manager
            {
                Id = Guid.NewGuid(),
                IsAdministrator = true,
                IsSiteManager = false,
                UserName = "admin",
                FullName = "The Adminstrator",
                EncodedSalt = adminCredentials.Item2,
                PasswordHash = adminCredentials.Item1
            };

            var bobsCredentials = GenerateEncodedPasswordHashAndSalt("secret2");

            Manager bob = new Manager
            {
                Id = Guid.NewGuid(),
                IsAdministrator = false,
                IsSiteManager = true,
                UserName = "bsmith",
                FullName = "Bob Smith",
                EncodedSalt = bobsCredentials.Item2,
                PasswordHash = bobsCredentials.Item1
            };

            var alicesCredentials = GenerateEncodedPasswordHashAndSalt("secret3");

            Manager alice = new Manager
            {
                Id = Guid.NewGuid(),
                IsAdministrator = true,
                IsSiteManager = true,
                UserName = "anewbie",
                FullName = "Alice Newbie",
                EncodedSalt = alicesCredentials.Item2,
                PasswordHash = alicesCredentials.Item1
            };

            var managersCollection = database.GetCollection<Manager>("manager");

            var managers = new Manager[] { admin, bob, alice};

            managersCollection.RemoveAll();
            
            foreach (var manager in managers)
            {
                managersCollection.Save(manager);
            }

            return managers;
        }
    }
}
