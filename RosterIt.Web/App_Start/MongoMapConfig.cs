using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace RosterIt.Web.App_Start
{
    public static class MongoMapConfig
    {
        public static void Configure()
        {
            string serverUri = ConfigurationManager.AppSettings["mongoUri"];
            if (string.IsNullOrWhiteSpace(serverUri))
                throw new ConfigurationErrorsException("'mongoUri' application setting not found.");

            var client = new MongoClient(serverUri);

            var server = client.GetServer();
            

            string databaseUri = ConfigurationManager.AppSettings["databaseName"];
            if (string.IsNullOrWhiteSpace(databaseUri))
                throw new ConfigurationErrorsException("'databaseName' application setting not found.");

            var database = server.GetDatabase(databaseUri);
        
            var shiftCollection = database.GetCollection<Shift>("shifts");
            shiftCollection.EnsureIndex(
                new IndexKeysBuilder<Shift>()
                    .Ascending(x => x.Symbol),
                    IndexOptions.SetUnique(true));

            var guardCollection = database.GetCollection<Guard>("guards");
            guardCollection.EnsureIndex(
                new IndexKeysBuilder<Guard>()
                    .Ascending(x => x.CompanyNumber),
                    IndexOptions.SetUnique(true));

            var managerCollection = database.GetCollection<Manager>("manager");
            managerCollection.EnsureIndex(
                new IndexKeysBuilder<Manager>()
                    .Ascending(x => x.UserName),                    
                    IndexOptions.SetUnique(true));

            
        }

    }
}