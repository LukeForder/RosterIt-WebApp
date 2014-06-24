using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Newtonsoft.Json;
using RosterIt.Models;
using RosterIt.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RosterIt.Web.Controllers
{
    [Authorize]
    public class ShiftController : ApiController
    {
        private readonly MongoCollection<Shift> _shiftCollection;
        private readonly ILog _log;

        public ShiftController(
            MongoCollection<Shift> shiftCollection)
        {
            _shiftCollection = shiftCollection;
            _log = LogManager.GetCurrentClassLogger();
        }

        public HttpResponseMessage Post([FromBody] Shift shift)
        {
            try
            {
                if (shift == null)
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);

                shift.Id = Guid.NewGuid();

                var outcome = _shiftCollection.Save(shift);

                if (!outcome.Ok)
                {
                    _log.Warn(async f => f("Unable to save shift: '{0}', reason: '{1}'.", await JsonConvert.SerializeObjectAsync(shift), outcome.ErrorMessage));

                    // TODO: if the symbol unique symbol has been voilated 
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                    
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (WriteConcernException writeConcernException)
            {
                if (writeConcernException.CommandResult.Code == 11000)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new ErrorResponse { Reason = "A shift with that symbol already exists." });

                _log.Error(writeConcernException.Message);

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);

            }
            catch (Exception exception)
            {
                _log.Error(exception.Message);

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public HttpResponseMessage Delete(Guid id)
        {
            try
            {
                var outcome = _shiftCollection.Remove(
                    Query<Shift>.EQ(x => x.Id, id));

                if (!outcome.Ok)
                {
                    _log.Warn(f => f("Unable to delete shift with id: '{0}', reason: '{1}'.", id, outcome.ErrorMessage));

                    // TODO: if the symbol unique symbol has been voilated 
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception.Message);

                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
