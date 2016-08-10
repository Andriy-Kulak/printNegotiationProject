using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using loreal_print.Models;
using loreal_print.MEC_Common.Security.Principal;
using System.Web.Script.Serialization;
using System.Security.Principal;
using System.Collections.Generic;

namespace loreal_print.MEC_Common.Security
{
    internal static class CookieManagement
    {
        internal static void CreateCookie(System.Web.HttpContext pContext, IPrincipal pUser, string pEmail)
        {
            var userId = pUser.Identity.GetUserId();

            //Create Model for Cookie Data
            var serializeModel = new CustomPrincipalSerializerModel
            {
                Id = userId,
                Email = pEmail,
                UserName = pEmail
            };

            //Serialize the Model
            var serializer = new JavaScriptSerializer();
            var userData = serializer.Serialize(serializeModel);

            //Create an Auth Ticket
            var authTicket = new FormsAuthenticationTicket(
                            1,
                            pEmail, //User
                            DateTime.Now, //When
                            DateTime.MaxValue, //Expires in
                            false,
                            // model.RememberMe, //Persistant? - we can add later
                            userData);

            //Encrypt the auth ticket to prevent Cookie Attacks
            var encTicket = FormsAuthentication.Encrypt(authTicket);

            //Add Cookie to the Context Reponse
            var faCookie = new HttpCookie(
                 "lorealPrint", encTicket);

            pContext.Response.Cookies.Add(faCookie);
        }

        internal static CustomPrincipalSerializerModel GetCookie(System.Web.HttpContext pContext)
        {
            var lresult = new CustomPrincipalSerializerModel();
            var authCookie = pContext.Request.Cookies["lorealPrint"];

            // BUG FIX:  Handle empty cookie
            if (String.IsNullOrEmpty(authCookie.Value))
            {
                var pUser = HttpContext.Current.User;
                var lEmailAddress = pUser.Identity.Name;

                // Create Cookie
                CreateCookie(System.Web.HttpContext.Current, pUser, lEmailAddress);
                authCookie = pContext.Request.Cookies["lorealPrint"];
            }

            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            var serializer = new JavaScriptSerializer();
            var lCookieData = serializer.Deserialize<CustomPrincipalSerializerModel>(authTicket.UserData);

            lresult = lCookieData;

            return lresult;
        }

        internal static bool UpdateCookie(System.Web.HttpContext pContext, CustomPrincipalSerializerModel pCookieData)
        {
            var lResult = false;
            var authCookie = pContext.Request.Cookies["lorealPrint"];

            // BUG FIX:  Handle empty cookie
            if (String.IsNullOrEmpty(authCookie.Value))
            {
                var pUser = HttpContext.Current.User;
                var lEmailAddress = pUser.Identity.Name;

                // Create Cookie
                CreateCookie(System.Web.HttpContext.Current, pUser, lEmailAddress);
                authCookie = pContext.Request.Cookies["lorealPrint"];
            }

            var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            var serializer = new JavaScriptSerializer();
            var lCookieData = serializer.Deserialize<CustomPrincipalSerializerModel>(authTicket.UserData);
            var lRememberMe = authTicket.IsPersistent;

            // Handle empty Version
            var lVersionID = pCookieData.VersionID >= lCookieData.VersionID ? pCookieData.VersionID : lCookieData.VersionID;
            if (lVersionID < 1)
                lVersionID = 1;

            try
            {
                // Create Model for Cookie Data
                var serializeModel = new CustomPrincipalSerializerModel
                {
                    Id = pCookieData.Id,
                    Email = pCookieData.Email != null ? pCookieData.Email : lCookieData.Email,
                    UserName = pCookieData.UserName != null ? pCookieData.UserName : lCookieData.UserName,
                    VersionID = lVersionID,
                    Book = pCookieData.Book != null ? pCookieData.Book : lCookieData.Book,
                    BookID = pCookieData.BookID != null ? pCookieData.BookID : lCookieData.BookID,
                    Year = pCookieData.Year != null ? pCookieData.Year: lCookieData.Year,
                    Publisher = pCookieData.Publisher != null ? pCookieData.Publisher: lCookieData.Publisher,
                    PublisherID = pCookieData.PublisherID != null ? pCookieData.PublisherID : lCookieData.PublisherID
                };

                // Serialize the Model
                serializer = new JavaScriptSerializer();
                var userData = serializer.Serialize(serializeModel);

                // Create an Auth Ticket
                authTicket = new FormsAuthenticationTicket(
                                1,
                                serializeModel.UserName, // User
                                DateTime.Now, // When
                                DateTime.MaxValue, // Expires in
                                lRememberMe, // Persistant?
                                userData);

                // Encrypt the auth ticket to prevent Cookie Attacks
                var encTicket = FormsAuthentication.Encrypt(authTicket);

                // Add Cookie to the Context Reponse
                var faCookie = new HttpCookie(
                     "lorealPrint", encTicket);

                // TODO
                //If "Remember Me" has been selected, once user clicks the 'Log in' button, system shall store email/password. Selection shall be stored till user logs out
                if (authTicket.IsPersistent)
                {
                    faCookie.Expires = DateTime.MaxValue;
                }


                pContext.Response.Cookies.Add(faCookie);
                return true;
            }
            catch (Exception ex)
            {
                //lLogMsg = String.Format("Error updating cookie for {0}: {1}", pCookieData.UserName, ex.Message);
                //logger.Error(lLogMsg);
                return lResult;
            }
        }

        internal static bool DeleteCookie(System.Web.HttpContext pContext)
        {
            var lResult = true;
            try
            {
                var authCookie = pContext.Request.Cookies["lorealPrint"];

                // Remove cookie when logging out.
                if (authCookie != null)
                {
                    HttpCookie myCookie = new HttpCookie("lorealPrint");
                    myCookie.Expires = DateTime.Now.AddDays(-1d);
                    HttpContext.Current.Response.Cookies.Add(myCookie);
                }
            }
            catch (Exception ex)
            {
                lResult = false;
            }
            return lResult;
        }

        internal static string GetPublisher(int? pPublisherID)
        {
            var lPublisher = string.Empty;
            using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
            {
                lPublisher = db.Publishers
                    .Where(p => p.PublisherID == pPublisherID)
                    .Select(p => p.Publisher1)
                    .FirstOrDefault();
            }
            return lPublisher;
        }

        internal static VersionInfoModel SaveBookInfoToCookie(int pBookID)
        {
            var lBook = new Book();

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            var lVersionInfo = new VersionInfoModel();
            var lYear = lCookieData.Year;

            using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
            {
                lBook = db.Books
                    .Where(b => b.BookID == pBookID && b.Year == lYear).SingleOrDefault();
            }

            // Get Version and Status information; Update cookie with this and pass back to client.
            var lVersions = GetVersionInfo(pBookID, lYear);
            var lLatestVersion = lVersions.OrderBy(v => v.VersionID).FirstOrDefault();

            // Update Cookie
            var cookieData = new CustomPrincipalSerializerModel();
            cookieData.BookID = pBookID;
            cookieData.Book = lBook.Book1;
            cookieData.VersionID = lLatestVersion.VersionID;
            cookieData.Status = lLatestVersion.Status;

            var lbool = CookieManagement.UpdateCookie(System.Web.HttpContext.Current, cookieData);

            // Populate VersionInfoModel
            lVersionInfo.Status = lLatestVersion.Status;
            lVersionInfo.VersionID = lLatestVersion.VersionID;
            lVersionInfo.VersionList = lVersions;

            return lVersionInfo;
        }

        internal static List<BookToVersion> GetVersionInfo(int pBookID, string pYear)
        {
            var lBookVersion = new List<BookToVersion>();
            const string STATUS = "In Progress";

            using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
            {
                lBookVersion = db.BookToVersions
                    .Where(a => a.BookID == pBookID && a.Year == pYear)
                    .ToList();

                if (!lBookVersion.Any())
                {
                    // Call CreateNewVersion SP
                    var lVersionNum = db.CreateNewVersion(pYear, pBookID);
                    var lBook = new BookToVersion
                    {
                        BookID = pBookID,
                        VersionID = lVersionNum,
                        Year = pYear,
                        Status = STATUS
                    };
                    lBookVersion.Add(lBook);

                }
                return lBookVersion;
            }
        }
    }
}