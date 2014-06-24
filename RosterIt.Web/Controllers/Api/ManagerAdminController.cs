using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RosterIt.Models;
using MongoDB.Driver;
using Common.Logging;
using RosterIt.Web.Models;
using RosterIt.Web.Commands;
using MongoDB.Driver.Builders;
using System.Threading.Tasks;
using RosterIt.Audit;
using RosterIt.Web.Models.Audit;

namespace RosterIt.Web.Controllers
{
    [Authorize]
    [RoutePrefix("api/manager")]
    public class ManagerAdminController : ApiController
    {
        private readonly MongoCollection<AuditEntry> _auditCollection;
        private readonly MongoCollection<Manager> _managerCollection;
        private readonly MongoRoleProvider _roleProvider;

        private readonly ILog _log;

        public ManagerAdminController(
            MongoCollection<Manager> managerCollection,
            MongoCollection<AuditEntry> auditCollection,
            MongoRoleProvider roleProvider)
        {
            _managerCollection = managerCollection;
            _auditCollection = auditCollection;
            _roleProvider = roleProvider;
            _log = LogManager.GetCurrentClassLogger();
        }

        [Route("new", Name="SubmitNewManager")]
        public async Task<HttpResponseMessage> Post([FromBody] CreateManagerCommand manager)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                return
                    Request.CreateResponse(
                        HttpStatusCode.Forbidden,
                        new ErrorResponse
                        {
                            Reason = "You do not have permission to perform this action"
                        });

            if (manager == null)
                return 
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "No details were provided for the manager"
                        });

            if (string.IsNullOrWhiteSpace(manager.UserName))
                return 
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "Manager must be assigned a user name."
                        });

            if (string.IsNullOrWhiteSpace(manager.Password) || string.IsNullOrWhiteSpace(manager.PasswordConfirmation))
                return 
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "Manager's account must be password protected, password is empty"
                        });

            if (manager.IsSiteManager == false && manager.IsAdministrator == false)
                return 
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "Manager has no roles assigned, at least one role must be assigned."
                        });

            try
            {
                if (manager.Password != manager.PasswordConfirmation)
                    return Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse {
                            Reason = "Password and the confirmation did not match."
                        });
                
                var salt = PasswordManager.GenerateSalt(10);

                Manager persistedManager = new Manager
                {
                    Id = Guid.NewGuid(),
                    UserName = manager.UserName.ToLower(),
                    FullName = manager.FullName,
                    ManagedSites = (manager.ManagedSiteIds ?? new Guid[0]).ToArray(),
                    PasswordHash = PasswordManager.Encode(PasswordManager.ComputeHash(manager.Password, salt)),
                    EncodedSalt = PasswordManager.Encode(salt),
                    IsAdministrator = manager.IsAdministrator,
                    IsSiteManager = manager.IsSiteManager
                };

                var outcome = _managerCollection.Save(persistedManager);

                _auditCollection.Save(new ManagerAudit
                {
                    ChangeType = ChangeType.Add,
                    Date = DateTime.Now,
                    User = new User
                    {
                        UserName = User.Identity.Name
                    },
                    New = CopyWithoutCredentials(persistedManager)
                });

                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new ManagerResult
                    {
                        Id = persistedManager.Id,
                        FullName = persistedManager.FullName,
                        UserName = persistedManager.UserName
                    });
            }
            catch (WriteConcernException exception)
            {
                if (exception.CommandResult.Code == 11000) // unique index voilated
                {
                    return
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = "A manager with that username already exists."
                            });
                }
                else
                {
                    _log.Error(exception.ExceptionDescription());

                    return Request.CreateResponse(
                        HttpStatusCode.InternalServerError,
                        new ErrorResponse
                        {
                            Reason = "Something went worng when creating a manager."
                        });
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception.ExceptionDescription());

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went worng when creating a manager."
                    });
            }
        }

        private static Manager CopyWithoutCredentials(Manager persistedManager)
        {
            return new Manager
            {
                Id = persistedManager.Id,
                FullName = persistedManager.FullName,
                IsAdministrator = persistedManager.IsAdministrator,
                IsSiteManager = persistedManager.IsSiteManager,
                ManagedSites = persistedManager.ManagedSites.ToArray(),
                UserName = persistedManager.UserName
            };
        }

        [Route("password/{id:guid}", Name="ApiChangePassword")]
        public async Task<HttpResponseMessage> Put(Guid id, [FromBody] PasswordChangeCommand command)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                return
                    Request.CreateResponse(
                        HttpStatusCode.Forbidden,
                        new ErrorResponse
                        {
                            Reason = "You do not have permission to perform this action"
                        });

            if (command == null)
                return
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "No details were provided for the password change"
                        });

            try
            {
                // validate this command
                ValidationResult validationResult = PasswordChangeCommand.Valid(command);                
                if (!validationResult.IsValid)
                {
                    string message = 
                        string.Format(
                            "Unable to change the password because of the following errors; {0}.",
                            string.Join(", ", validationResult.Errors.Select(x => x.ToLower())));

                    return
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = message
                            });
                }

                var salt = PasswordManager.GenerateSalt(10);
                var rawHash = PasswordManager.ComputeHash(command.Password, salt);
                var passwordHash = PasswordManager.Encode(rawHash);
                var encodedSalt = PasswordManager.Encode(salt);

                var outcome =
                    _managerCollection.Update(
                        Query<Manager>.EQ(x => x.Id, id),
                        Update<Manager>
                            .Set(x => x.PasswordHash, passwordHash)
                            .Set(x => x.EncodedSalt, encodedSalt));

                _auditCollection.Save(new ManagerPasswordChangeAudit
                {
                    ChangeType = ChangeType.Edit,
                    Old = new Manager {  Id = id },
                    New = new Manager { Id = id },
                    User = new User
                    {
                        UserName = User.Identity.Name
                    }
                });

                return (outcome.DocumentsAffected == 0) ?
                    Request.CreateResponse(HttpStatusCode.NotFound) :
                    Request.CreateResponse(HttpStatusCode.OK);
                    

            }
            catch (Exception exception)
            {
                _log.Error(exception.ExceptionDescription());

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong updating the managers password."
                    });
            }

        }

        [Route("{id:guid}", Name="ApiUpdateDetails")]
        public async Task<HttpResponseMessage> Put(Guid id, [FromBody] UpdateManagerCommand command)
        {
            if (!(await _roleProvider.InRoleAsync(User.Identity.Name, MongoRoleProvider.AdministratorRole)))
                return
                    Request.CreateResponse(
                        HttpStatusCode.Forbidden,
                        new ErrorResponse
                        {
                            Reason = "You do not have permission to perform this action"
                        });


            if (command == null)
                return 
                    Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new ErrorResponse
                        {
                            Reason = "No details to update the manager details."
                        });

            try
            {
                var validationResult = UpdateManagerCommand.Validate(command);
                if (!validationResult.IsValid)
                    return
                        Request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new ErrorResponse
                            {
                                Reason = validationResult.Errors.Count > 0 ? validationResult.Errors.ElementAt(0) : "Update didn't contain the necessary information"
                            });


                var outcome =
                    _managerCollection.Update(
                        Query<Manager>.EQ(x => x.Id, id),
                        Update<Manager>
                            .Set(x => x.IsAdministrator, command.IsAdministrator)
                            .Set(x => x.IsSiteManager, command.IsSiteManager)
                            .Set(x => x.UserName, command.UserName)
                            .Set(x => x.FullName, command.FullName));

                _auditCollection.Save(new ManagerAudit
                {
                    ChangeType = ChangeType.Edit,
                    Old = new Manager { Id = id },
                    New = new Manager { Id = id, FullName = command.FullName, UserName = command.UserName },
                    User = new User
                    {
                        UserName = User.Identity.Name
                    }
                });

                return (outcome.DocumentsAffected == 0) ?
                    Request.CreateResponse(HttpStatusCode.NotFound) :
                    Request.CreateResponse(HttpStatusCode.OK);


            }
            catch (Exception exception)
            {
                _log.Error(exception.ExceptionDescription());

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong updating the managers password."
                    });
            }
        }

        [Route("{id:guid}")]
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

                var outcome =
                    _managerCollection.Remove(
                        Query<Manager>.EQ(x => x.Id, id));

                _auditCollection.Save(new ManagerAudit
                {
                    ChangeType = ChangeType.Delete,
                    Old = new Manager { Id = id },
                    User = new User
                    {
                        UserName = User.Identity.Name
                    }
                });

                return (outcome.DocumentsAffected == 0) ?
                    Request.CreateResponse(HttpStatusCode.NotFound, new ErrorResponse { Reason = "Manager was not found." }) :
                    Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception exception)
            {
                _log.Error(exception.ExceptionDescription());

                return Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new ErrorResponse
                    {
                        Reason = "Something went wrong deleting the managers."
                    });
            }
        }
    }
}
