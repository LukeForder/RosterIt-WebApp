using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
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
    [Authorize, RoutePrefix("admin/guard")]
    public class GuardController : ApiController
    {
        private readonly MongoCollection<Guard> _guardCollection;
        private readonly ILog _log;
        private readonly MongoRoleProvider _roleProvider;


        public GuardController(
            MongoCollection<Guard> guardCollection,
            MongoRoleProvider roleProvider)
        {
            _guardCollection = guardCollection;
            _roleProvider = roleProvider;
            _log = LogManager.GetCurrentClassLogger();
        }

        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                    return
                        Request.CreateResponse(
                            HttpStatusCode.Forbidden,
                            new ErrorResponse
                            {
                                Reason = "You do not have permission to perform this action"
                            });

                

                var guards =
                    _guardCollection
                        .FindAll();

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    guards
                        .Select(x => new
                        {
                            id = x.Id,
                            companyNumber = x.CompanyNumber,
                            name = x.FullName
                        })
                        .OrderBy(x => x.companyNumber)
                        .ToList()
                );
            }
            catch (Exception e)
            {
                _log.Error(e.ExceptionDescription());

               return Request.CreateResponse(
                    HttpStatusCode.InternalServerError, 
                    new ErrorResponse
                    {
                        Reason = "Something went wrong loading the guards."
                    });
            }
        }

        [Route("", Name = "SubmitNewGuard")]
        public async Task<HttpResponseMessage> Post([FromBody] Guard guard)
        {
            try
            {
                if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                    return
                        Request.CreateResponse(
                            HttpStatusCode.Forbidden,
                            new ErrorResponse
                            {
                                Reason = "You do not have permission to perform this action"
                            });

                if (guard == null)
                    return
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = "The guard details were missing."
                            });

                if (string.IsNullOrWhiteSpace(guard.CompanyNumber))
                    return
                           Request.CreateResponse(
                               HttpStatusCode.BadRequest,
                               new ErrorResponse
                               {
                                   Reason = "The guard company number must be assigned."
                               });

                guard.CompanyNumber = guard.CompanyNumber.ToUpper();

                // Assign a new id
                guard.Id = Guid.NewGuid();

                _guardCollection.Save(guard);

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    guard);
            }
            catch (WriteConcernException exception)
            {
                if (exception.CommandResult.Code == 11000)
                {
                    return Request.CreateResponse<ErrorResponse>(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = string.Format("A Guard already exists with the company number, '{0}'.", guard.CompanyNumber)
                        });
                }

                _log.Error(exception.ExceptionDescription());

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong saving the guard."
                    });
            }
            catch (Exception exception)
            {
                _log.Error(f => f("Saving guard {{ Id: {0} }}\n{1}", guard.Id, exception.ExceptionDescription()));

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong saving the guard."
                    });
            }
        }

        [Route("api/{id:guid}", Name="ApiUpdateGuard")]
        public async Task<HttpResponseMessage> Put(Guid id, [FromBody] Guard guard)
        {
            try
            {

                if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                    return
                        Request.CreateResponse(
                            HttpStatusCode.Forbidden,
                            new ErrorResponse
                            {
                                Reason = "You do not have permission to perform this action"
                            });

                if (guard == null)
                    return
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = "The guard details were missing."
                            });

                if (string.IsNullOrWhiteSpace(guard.CompanyNumber))
                    return
                           Request.CreateResponse(
                               HttpStatusCode.BadRequest,
                               new ErrorResponse
                               {
                                   Reason = "The guard company number must be assigned."
                               });

                guard.CompanyNumber = guard.CompanyNumber.ToUpper();


                guard.Id = id;

                var outcome = _guardCollection.Save(guard);

                if (outcome.DocumentsAffected == 0)
                    return Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        new ErrorResponse
                        {
                            Reason   = "Guard was not found"
                        });

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    guard);
            }
            catch (WriteConcernException exception)
            {
                if (exception.CommandResult.Code == 11000)
                {
                    return Request.CreateResponse<ErrorResponse>(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = string.Format("A Guard already exists with the company number, {0}.", guard.CompanyNumber)
                        });
                }

                _log.Error(exception.ExceptionDescription());

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong saving the guard."
                    });
            }
            catch (Exception exception)
            {
                _log.Error(f => f("Updating guard {{ Id: {0} }}\n{1}", id, exception.ExceptionDescription()));

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong saving the guard."
                    });
            }
        }

        
        public async Task<HttpResponseMessage> Delete(Guid id)
        {
            try
            {
                if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                    return
                        Request.CreateResponse(
                            HttpStatusCode.Forbidden,
                            new ErrorResponse
                            {
                                Reason = "You do not have permission to perform this action"
                            });

                var outcome = _guardCollection.Remove(
                    Query<Guard>.EQ(guard => guard.Id, id));

                if (outcome.DocumentsAffected == 0)
                    return Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        new ErrorResponse
                        {
                            Reason = "Guard was not found"
                        });

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception exception)
            {
                _log.Error(f => f("Deleting guard {{Id: {0}}}\n{1}", id, exception.ExceptionDescription()));

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong deleting the guard."
                    });
            }
        }
    }
}