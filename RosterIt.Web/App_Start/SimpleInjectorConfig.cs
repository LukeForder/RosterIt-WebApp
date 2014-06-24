using MongoDB.Driver;
using RosterIt.Audit;
using RosterIt.Holidays;
using RosterIt.Models;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace RosterIt.Web.App_Start
{
    public static class SimpleInjectorConfig
    {
      

        public static void RegisterTypes(Container container)
        {
            // register Web API controllers (important!)
            var controllerTypes =
                from type in Assembly.GetExecutingAssembly().GetExportedTypes()
                where typeof(IHttpController).IsAssignableFrom(type)
                where !type.IsAbstract
                where !type.IsGenericTypeDefinition
                where type.Name.EndsWith("Controller", StringComparison.Ordinal)
                select type;

            foreach (var controllerType in controllerTypes)
            {
                container.Register(controllerType);
            }

            // register MVC controllers (This is an extension method from the integration package).
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            container.Register(() =>
                {
                        string serverUri = ConfigurationManager.AppSettings["mongoUri"];
                        if (string.IsNullOrWhiteSpace(serverUri))
                            throw new ConfigurationErrorsException("'mongoUri' application setting not found.");

                        var client = new MongoClient(serverUri);
            
                        var server = client.GetServer();

                    return server;
                });

            container.Register(() => 
                {
                    var server = container.GetInstance<MongoServer>();

                    string databaseUri = ConfigurationManager.AppSettings["databaseName"];
                    if (string.IsNullOrWhiteSpace(databaseUri))
                        throw new ConfigurationErrorsException("'databaseName' application setting not found.");

                    var database = server.GetDatabase(databaseUri);

                    return database;
                });

            container.Register(() =>
                {
                    var database = container.GetInstance<MongoDatabase>();

                    var shiftCollection = database.GetCollection<Shift>("shifts");

                    return shiftCollection;
                });

            container.Register(() =>
            {
                var database = container.GetInstance<MongoDatabase>();

                var guardCollection = database.GetCollection<Guard>("guards");

                return guardCollection;
            });

            container.Register(() =>
            {
                var database = container.GetInstance<MongoDatabase>();

                var managerCollection = database.GetCollection<Manager>("manager");

                return managerCollection;
            });

            container.Register(() =>
            {
                var database = container.GetInstance<MongoDatabase>();

                var sitesCollection = database.GetCollection<Site>("sites");

                return sitesCollection;
            });

            container.Register(() =>
            {
                var database = container.GetInstance<MongoDatabase>();

                var auditCollection = database.GetCollection<AuditEntry>("audit");

                return auditCollection;
            });

            container.Register(() =>
            {
                var database = container.GetInstance<MongoDatabase>();

                var rosterCollection = database.GetCollection<Roster>("rosters");

                return rosterCollection;
            });

            container.Register<IHolidayProvider, EnricoHolidayProvider>();

            container.Verify();
            
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }

    }
}