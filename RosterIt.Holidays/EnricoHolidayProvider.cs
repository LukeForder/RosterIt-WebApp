using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RosterIt.Holidays
{
    public class EnricoHolidayProvider : IHolidayProvider
    {
        private readonly ILog _log;

        private static readonly ConcurrentDictionary<string, IEnumerable<Holiday>> _cache;

        static EnricoHolidayProvider()
        {
            _cache = new ConcurrentDictionary<string, IEnumerable<Holiday>>();
        }

        public EnricoHolidayProvider()
        {
            _log = LogManager.GetCurrentClassLogger();
          
        }

        private static string GetCacheKey(DateTime date, TimeSpan duration)
        {
            return string.Format("{0}-{1}", date.ToString("dd-MM-yyyy"), duration.Ticks);
        }

        public async Task<IEnumerable<Holiday>> GetHolidays(DateTime from, TimeSpan duration)
        {
            IEnumerable<Holiday> holidays = null;
            if (_cache.TryGetValue(GetCacheKey(from, duration), out holidays))
                return holidays;

            string mozillaServiceAddress = ConfigurationManager.AppSettings["calendar-service"];

            UriBuilder uriBuilder = new UriBuilder(mozillaServiceAddress);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query.Add("action", "getPublicHolidaysForDateRange");
            query.Add("fromDate", from.ToString("dd-MM-yyyy"));
            query.Add("toDate", from.Add(duration).ToString("dd-MM-yyyy"));
            query.Add("country", "zaf");

            uriBuilder.Query = query.ToString();

            HttpClient client = new HttpClient();

            _log.Trace(m => m("Calling holidays webservice @'{0}'", uriBuilder.ToString()));

            var response = await client.GetAsync(uriBuilder.Uri);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _log.Error(m => m("Unexpected response from the holiday web service. Status Code: {0}, Body: {1}", response.StatusCode, body));
                throw new HolidayServiceException();
            }
            try
            {
                var json = await Task.Run(() => JsonConvert.DeserializeObject<EnricoHoliday[]>(body));

                holidays =
                    json
                        .Select(x => new Holiday { Date = x.date.Date, Name = x.englishName, Country = "ZAF" })
                        .ToList();

                _cache.AddOrUpdate(GetCacheKey(from, duration), holidays, (key, @new) => holidays);

                return holidays;
            }
            catch (Exception ex)
            {
                _log.Error(m => m("Unexpected error, Type: {0}, Message: {1}", ex.GetType().FullName, ex.Message));

                throw;
            }
        }
    }
}
