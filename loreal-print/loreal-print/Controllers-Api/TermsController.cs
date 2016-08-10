using loreal_print.MEC_Common.Security;
using loreal_print.MEC_Common.Security.Principal;
using loreal_print.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace loreal_print.Controllers_Api
{
    public class TermsController : ApiController
    {
        // GET api/<controller>
        public IHttpActionResult Get()
        {
            // Update Cookie wth bookID
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            SaveBookInfoToCookie(Convert.ToInt32(lCookieData.BookID));

            IHttpActionResult lResult = null;
            QuestionModel vm = new QuestionModel();
            vm.Get(System.Web.HttpContext.Current);
            if (vm.SectionsListModel.SectionList.Any())
            {
                lResult = Ok(vm.SectionsListModel.SectionList);
            }
            else
            {
                lResult = NotFound();
            }
            return lResult;
        }


        [HttpGet()]
        [Route("api/Terms/GetAnswers")]
        [Authorize]
        public IHttpActionResult GetAnswers()
        {
            IHttpActionResult lResult = null;
            var lstAnswers = new List<Answer>();
            var pContext = System.Web.HttpContext.Current;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            using (Loreal_DEVEntities3 db = new Loreal_DEVEntities3())
            {
                lstAnswers = db.Answers
                    .Where(a => a.BookID == lCookieData.BookID && a.Year == lCookieData.Year && a.VersionID == lCookieData.VersionID)
                    .ToList();
            }

            if (lstAnswers.Any())
            {
                lResult = Ok(lstAnswers);
            }
            else
            {
                lResult = NotFound();
            }

            return lResult;
        }

        //// GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<controller>
        [Route("api/Terms/saveAnswers/answers")]
        [Authorize]
        public IHttpActionResult saveAnswers(List<Answer> answers)
        {
            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            IHttpActionResult lResult = null;

            var lAnswerIDs = new List<string>();

            foreach (var answer in answers)
            {
                answer.BookID = Convert.ToInt32(lCookieData.BookID);
                answer.VersionID = lCookieData.VersionID < 1 ? 1 : lCookieData.VersionID;
                answer.Year = lCookieData.Year;
                answer.Load_Date = DateTime.Now;
                string lID = Save(answer);
                lAnswerIDs.Add(lID);
            }

            var lAnswerIDsArray = lAnswerIDs.ToArray();

            if (lAnswerIDsArray.Any())
            {
                lResult = Ok(lAnswerIDsArray);
            }
            else if (lAnswerIDs.Contains("-1"))
            {
                lResult = BadRequest();
            }
            else
            {
                lResult = NotFound();
            }

            return lResult;
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

        private string Save(Answer pAnswer)
        {
            int lAnswerID = -1;
            var messages = new ModelStateDictionary();

            using (Loreal_DEVEntities3 db = new Loreal_DEVEntities3())
            {
                // Ensure the correct Answer is either updated or created.
                var isUpdate = db.Answers.Find(pAnswer.AnswerID);

                if (isUpdate == null)
                {
                    // Create
                    db.Answers.Add(pAnswer);
                    lAnswerID = db.SaveChanges();
                }
                else
                {
                    // Update
                    db.Entry(pAnswer).State = EntityState.Modified;
                    lAnswerID = db.SaveChanges();
                }
            }
            return lAnswerID.ToString();
        }

        private ModelStateDictionary ConvertToModelState(System.Web.Mvc.ModelStateDictionary state)
        {
            ModelStateDictionary lResult = new ModelStateDictionary();

            foreach (var list in state.ToList())
            {
                for (int i = 0; i < list.Value.Errors.Count; i++)
                {
                    lResult.AddModelError(list.Key, list.Value.Errors[i].ErrorMessage);
                }
            }

            return lResult;
        }

        private VersionInfoModel SaveBookInfoToCookie(int pBookID)
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

        private List<BookToVersion> GetVersionInfo(int pBookID, string pYear)
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