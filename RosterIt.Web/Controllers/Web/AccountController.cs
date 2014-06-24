using Common.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using RosterIt.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RosterIt.Web;
using RosterIt.Web.Models;

namespace RosterIt.Controllers
{
    public class AccountController : Controller
    {
        private readonly MongoCollection<Manager> _managerCollection;
        private readonly ILog _log;

        public AccountController(
            MongoCollection<Manager> managerCollection)
        {
            _log = LogManager.GetCurrentClassLogger();
            _managerCollection = managerCollection;
        }

        [/*RequireHttps,*/ HttpGet, Route("login")]
        public ActionResult LogIn()
        {
            return View();
        }

        [/*RequireHttps,*/ HttpPost, Route("login")]
        public ActionResult LogIn(string userName, string password)
        {
            try
            {
                var manager =
                    _managerCollection.FindOne(
                        Query<Manager>.EQ(x => x.UserName, userName.ToLower()));

                if (manager == null)
                    return View(new LogInFailedModel { Message = "Incorrect user name or password.", UserName = userName });

                var salt = PasswordManager.Decode(manager.EncodedSalt);
                var raw = PasswordManager.ComputeHash(password, salt);
                var saltedBytes = PasswordManager.Decode(manager.PasswordHash);

                if (Enumerable.SequenceEqual(saltedBytes, raw))
                {
                    string url = FormsAuthentication.GetRedirectUrl(userName, false);
                }
                else
                {
                    return View(new LogInFailedModel { Message = "Incorrect user name or password.", UserName = userName });
                }

                FormsAuthentication.SetAuthCookie(userName, false);

                string redirectUrl = FormsAuthentication.GetRedirectUrl(userName, false);

                return Redirect(redirectUrl);
            }
            catch (HttpException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                _log.Error(e.ExceptionDescription());

                return View(new LogInFailedModel { Message = "Incorrect user name or password.", UserName = userName });
            }
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post), Route("logoff", Name="logoff")]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("LogIn");
        }
    }
}