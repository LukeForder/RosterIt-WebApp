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
using System.Web.Http;

namespace RosterIt.Web.Controllers
{
    [Authorize]
    public class SiteController : ApiController
    {
        private readonly MongoCollection<Manager> _managerCollection;
        private readonly MongoCollection<Site> _siteCollection;
        private readonly ILog _log;

        public SiteController(
            MongoCollection<Site> siteCollection,
            MongoCollection<Manager> managerCollection)
        {
            _siteCollection = siteCollection;
            _managerCollection = managerCollection; 
            _log = LogManager.GetCurrentClassLogger();
        }

        public HttpResponseMessage Post([FromBody] Site site)
        {
            try
            {
                if (site == null)
                    return Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "The site details were missing."
                        });



                site.Id = Guid.NewGuid();

                _siteCollection.Save(site);

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    site);

            }
            catch (WriteConcernException exception)
            {
                if (exception.CommandResult.Code == 11000)
                    return 
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = "A site with this name already exists."
                            });

                _log.Error(string.Format("Error creating site: {0}", exception.ExceptionDescription()));

                return Request.CreateResponse(
                      HttpStatusCode.BadRequest,
                      new ErrorResponse
                      {
                          Reason = "Something went wrong creating the site."
                      });
            }
            catch (Exception exception)
            {
                _log.Error(string.Format("Error creating site: {0}", exception.ExceptionDescription()));

                return Request.CreateResponse(
                      HttpStatusCode.BadRequest,
                      new ErrorResponse
                      {
                          Reason = "Something went wrong creating the site."
                      });
            }
        }

        public HttpResponseMessage PutNewManager(Guid id,  Guid managerId)
        {
            try
            {

                var manager =
                    _managerCollection
                        .FindOne(
                            Query<Manager>.EQ(x => x.Id, managerId));

                if (manager == null)
                    return Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = "Manager doesn't exist"
                            });

                var outcome =
                    _siteCollection.Update(
                        Query<Site>.EQ(x => x.Id, id),
                        Update<Site>.Set(x => x.Manager, new ManagerDetails { Id = managerId, FullName = manager.FullName }));
                
                return (outcome.DocumentsAffected == 0) ?
                    Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        new ErrorResponse 
                        {
                            Reason = "The site was not found"
                        }) :
                    Request.CreateResponse(
                        HttpStatusCode.OK);

            }
            catch (WriteConcernException exception)
            {
                if (exception.CommandResult.Code == 11000)
                    return
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = "A site with this name already exists."
                            });


                _log.Error(string.Format("Error setting site manager: {0}", exception.ExceptionDescription()));

                return Request.CreateResponse(
                      HttpStatusCode.BadRequest,
                      new ErrorResponse
                      {
                          Reason = "Something went wrong setting the site manager."
                      });
            }
            catch (Exception exception)
            {
                _log.Error(string.Format("Error creating site: {0}", exception.ExceptionDescription()));

                return Request.CreateResponse(
                      HttpStatusCode.BadRequest,
                      new ErrorResponse
                      {
                          Reason = "Something went wrong setting the site manager."
                      });
            }
        }

        public HttpResponseMessage PutRename(Guid id, string name)
        {
            try
            {

                var outcome =
                    _siteCollection.Update(
                        Query<Site>.EQ(x => x.Id, id),
                        Update<Site>.Set(x => x.Name, name));
                
                return (outcome.DocumentsAffected == 0) ?
                    Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        new ErrorResponse 
                        {
                            Reason = "The site was not found"
                        }) :
                    Request.CreateResponse(
                        HttpStatusCode.OK);

            }
            catch (WriteConcernException exception)
            {
                 if (exception.CommandResult.Code == 11000)
                    return 
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = "A site with this name already exists."
                            });

                _log.Error(string.Format("Error rename site: {0}", exception.ExceptionDescription()));

                return Request.CreateResponse(
                      HttpStatusCode.BadRequest,
                      new ErrorResponse
                      {
                          Reason = "Something went wrong setting the site name."
                      });
            }
            catch (Exception exception)
            {
                _log.Error(string.Format("Error creating site: {0}", exception.ExceptionDescription()));

                return Request.CreateResponse(
                      HttpStatusCode.BadRequest,
                      new ErrorResponse
                      {
                          Reason = "Something went wrong setting the site name."
                      });
            }
        }

        public HttpResponseMessage Delete(Guid id)
        {
            try
            {
                var outcome =
                    _siteCollection.Remove(
                        Query<Site>.EQ(x => x.Id, id));
                
                return (outcome.DocumentsAffected == 0) ?
                    Request.CreateResponse(
                        HttpStatusCode.NotFound,
                        new ErrorResponse 
                        {
                            Reason = "The site was not found"
                        }) :
                    Request.CreateResponse(
                        HttpStatusCode.OK);

            }
            catch (Exception exception)
            {
                _log.Error(string.Format("Error creating site: {0}", exception.ExceptionDescription()));

                return Request.CreateResponse(
                      HttpStatusCode.BadRequest,
                      new ErrorResponse
                      {
                          Reason = "Something went wrong setting the site name."
                      });
            }
        }
    }
}
