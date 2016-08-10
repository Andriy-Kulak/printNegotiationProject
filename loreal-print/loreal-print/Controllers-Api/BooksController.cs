using loreal_print.MEC_Common.Security.Principal;
using loreal_print.Models;
using loreal_print.ViewModels;
using loreal_print.MEC_Common.Security;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class BooksController : ApiController
    {
        [HttpGet()]
        [Route("api/Books/GetBookYears")]
        public IHttpActionResult GetBookYears()
        {
            IHttpActionResult lResult = null;
            var lstYears = new BooksViewModel().Years;

            using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
            {
                var years = db.Books
                    .Select(b => b.Year)
                    .Distinct()
                    .ToList();

                if (years.Any())
                {
                    foreach (var item in years)
                    {
                        var bkYear = new BookYear();
                        bkYear.Year = item;
                        bkYear.YearId = Convert.ToInt16(item);
                        lstYears.Add(bkYear);
                    }

                    lResult = Ok(lstYears);
                }
                else
                {
                    lResult = NotFound();
                }
            }
            return lResult;
        }

        // PUT api/<controller>/5
        [HttpPut()]
        public void Put(int id, [FromBody]string value)
        {

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lName = String.Empty;

            // Get Book Name
            using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
            {
                lName = db.Books
                    .Where(b => b.BookID == id)
                    .Select(B => B.Book1).FirstOrDefault();
            }


            // Update Cookie
            var cookieData = new CustomPrincipalSerializerModel();
            cookieData.Book = lName;
            cookieData.BookID = id;

            var lbool = CookieManagement.UpdateCookie(System.Web.HttpContext.Current, cookieData);
        }

        [HttpGet()]
        [Route("api/Books/GetBooks/{year}")]
        [Authorize]
        public IHttpActionResult GetBooks(string year)
        {
            IHttpActionResult lResult = null;
            var lstBooks = new List<BookViewModel>();

            var userId = User.Identity.GetUserId();
            int? publisherID = 0;

            using (Loreal_DEVEntities5 db = new Loreal_DEVEntities5())
            {
                publisherID = db.AspNetUsers
                    .Where(a => a.Id == userId)
                    .Select(a => a.PublisherID)
                    .SingleOrDefault();
            }

            // Update Cookie
            var cookieData = new CustomPrincipalSerializerModel();
            cookieData.Id = userId;
            cookieData.Year = year;
            cookieData.PublisherID = publisherID;
            cookieData.Publisher = CookieManagement.GetPublisher(publisherID);
            var lbool = CookieManagement.UpdateCookie(System.Web.HttpContext.Current, cookieData);

            using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
            {
                var books = db.Books
                    .Where(b => b.Year == year && b.PublisherID == publisherID)
                    .OrderBy(b => b.Book1)
                    .ToList();

                if (books.Any())
                {
                    foreach (var book in books)
                    {
                        var lbook = new BookViewModel();
                        lbook.ID = book.BookID;
                        lbook.Name = book.Book1;

                        lstBooks.Add(lbook);
                    }
                    lResult = Ok(lstBooks);
                }
                else
                {
                    lResult = NotFound();
                }
            }
            return lResult;
        }
    }
}
