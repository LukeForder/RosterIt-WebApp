using MongoDB.Driver;
using RosterIt.Data;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Notifications
{
    class Program
    {
        static void Main(string[] args)
        {
            Notifications.NotifyOfYesterdaysSubmissions().Wait();

        }
    }
}
