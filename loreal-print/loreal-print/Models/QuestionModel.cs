using loreal_print.MEC_Common.Security;
using loreal_print.MEC_Common.Security.Principal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models
{
    public class QuestionModel
    {
        const string DYNSTATEMENT = "DynamicStatement";
        const string DYNYESNO = "DynamicYesNo";
        const string AVGVERIFIEDCIRCPERCENT = "{AvgVerifiedCircPercent}";
        const string GENRE = "{Genre}";
        public QuestionModel()
        {
            Init();
        }

        private void Init()
        {
            Name = string.Empty;
            QuestionType = string.Empty;
            Year = "2017";
            AnswerYesNo = string.Empty;
            AnswerFreeForm = string.Empty;

            QuestionChildrenList = new List<QuestionChildModel>();
            SectionsListModel = new QuestionSectionsListModel();
        }

        #region Public Properties
        public List<QuestionModel> QuestionsList { get; set; }
        public List<QuestionChildModel> QuestionChildrenList { get; set; }
        public QuestionSectionsListModel SectionsListModel { get; set; }
        public int ID { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
        public string Year { get; set; }
        public string QuestionType { get; set; }
        public string AnswerYesNo { get; set; }
        public string AnswerFreeForm { get; set; }
        #endregion

        #region Load Questions
        public void Get(System.Web.HttpContext pContext)
        {
            const string YES = "Yes";
            var lCookieData = CookieManagement.GetCookie(pContext);

            using (Loreal_DEVEntities3 db = new Loreal_DEVEntities3())
            {
                /*ar listCount = db.V_QuestionDetails.Count();*/
                var questionsList = new List<QuestionModel>();
                //var lQuestion = string.Empty;
                var IsDynamic = false;

                // Get the Questions from the database.
                //var lQuestions = db.V_QuestionDetails
                //    .GroupJoin(db.Answers, q => q.QuestionID, a => a.QuestionID,
                //        (q, a) => new { V_QuestionDetails = q, Answers = a })
                //    .SelectMany(
                //        a => a.Answers.DefaultIfEmpty(),
                //        (q, a) => new { V_QuestionDetails = q.V_QuestionDetails, Answers = a })
                //    .Where(a => (a.Answers.Year == lCookieData.Year
                //        && a.Answers.BookID == lCookieData.BookID
                //        && a.Answers.VersionID == lCookieData.VersionID))
                //    .OrderBy(q => q.V_QuestionDetails.SectionOrder)
                //    .ThenBy(q => q.V_QuestionDetails.SubSectionOrder)
                //    .ThenBy(q => q.V_QuestionDetails.SubSectionToQuestionOrder)
                //    .ToList();

                //var lQuestions = (from q in db.V_QuestionDetails
                //                 join a in db.Answers
                //                 on q.QuestionID equals a.QuestionID
                //                 into ag
                //                 from agi in ag.Where(a => (a.Year == lCookieData.Year && a.BookID == lCookieData.BookID && a.VersionID == lCookieData.VersionID)).DefaultIfEmpty()
                //                 select q).ToList();

                var lQuestions = db.GetAnswers(lCookieData.Year, lCookieData.BookID, lCookieData.VersionID)
                                    .OrderBy(q => q.SectionOrder)
                                    .ThenBy(q => q.SubSectionOrder)
                                    .ThenBy(q => q.SubSectionToQuestionOrder)
                                    .ToList();

                //.Select(q => new { q., c.AveragePrintRun, c.RateBaseCirculationGuarantee })
                //.ToList();


                //.ToList();
                //.OrderBy(q => q.)
                //        .ThenBy(q => q.V_QuestionDetails.SubSectionOrder)
                //        .ThenBy(q => q.V_QuestionDetails.SubSectionToQuestionOrder)
                //        .ToList();


                //                from p in context.Periods
                //join f in context.Facts on p.id equals f.periodid into fg
                //from fgi in fg.Where(f => f.otherid == 17).DefaultIfEmpty()
                //where p.companyid == 100
                //select f.value

                //.Where(a => (a.Answers. == lCookieData.Year
                //    && o.Answers.BookID == lCookieData.BookID
                //    && o.Answers.VersionID == lCookieData.VersionID))

                var IsParent = false;
                var newSectionList = new QuestionSectionsListModel();
                var newSection = new QuestionSectionModel();
                var newSubSection = new QuestionSubSectionModel();
                var newQuestion = new QuestionModel();
                var newChildQuestion = new QuestionChildModel();
                var QuestName = string.Empty;
                var SubSecName = string.Empty;
                var SecName = string.Empty;
                var recordCount = 0;
                var recordTotal = lQuestions.Count;

                /* Data is ordered so that the first item in the collection should be a Question (as opposed to a child question.
                 * This means the first record will contain:
                 * 1.  A new Section
                 * 2.  A new SubSection
                 * 3.  A new Question
                 * All subsequent records may either be contained within an existing Question, SubSection and/or Section.
                 * Thus the way this is structured below. */

                foreach (var question in lQuestions)
                {
                    recordCount++;
                    // We work our way out from Child Question to Section.
                    IsParent = question.IsTopLevel == YES ? true : false;

                    /* If the current Question is a Parent and there is a previous Question we now add it to the Question List.
                     * (Otherwise, the current question is a Child question and we will continue to add Children to the question
                     * until the next Parent question. */
                    /* UPDATE: We not only have to ask the question of whether the current question IsParent - but check to ensure
                     * the SubSections match. Otherwise, the next question could be a parent - but within a new SubSection. */
                    if ((IsParent && newQuestion.ID > 0) || (!IsParent && SubSecName != question.SubSection))
                    {
                        newSubSection.QuestionList.Add(newQuestion);
                    }

                    // If the Section is new the SubSection and Question are, by default, new as well.
                    if (String.IsNullOrEmpty(SecName) || (SecName != question.Section))
                    {
                        if (!String.IsNullOrEmpty(SecName))
                        {
                            // First add the existing SubSection to the existing Section
                            newSection.SubSectionList.Add(newSubSection);
                            // Add the existing Section to the SectionList prior to creating the new Section.
                            newSectionList.SectionList.Add(newSection);
                        }

                        newSection = new QuestionSectionModel
                        {
                            ID = question.SectionID,
                            Name = question.Section,
                            Order = question.SectionOrder
                        };
                        SecName = question.Section;

                        newSubSection = new QuestionSubSectionModel
                        {
                            ID = question.SubSectionID,
                            Name = question.SubSection,
                            Order = question.SubSectionOrder
                        };
                        SubSecName = question.SubSection;
                    }
                    else if (SecName == question.Section && (SubSecName != question.SubSection))
                    {
                        // First, add the existing SubSection to the Section.
                        newSection.SubSectionList.Add(newSubSection);

                        newSubSection = new QuestionSubSectionModel
                        {
                            ID = question.SubSectionID,
                            Name = question.SubSection,
                            Order = question.SubSectionOrder
                        };
                        SubSecName = question.SubSection;
                    }

                    // Question/Child question
                    if (!IsParent)
                    {
                        IsDynamic = (question.QuestionType == DYNSTATEMENT || question.QuestionType == DYNYESNO);

                        // Resolve Dynamic Questions
                        if (IsDynamic)
                        {
                            // Temp for testing
                            var lBookID = lCookieData.BookID == null ? 8 : lCookieData.BookID;

                            if (String.IsNullOrEmpty(Dynamic.AvgVerifiedCircPercent))
                            {
                                GetDynamicValues(lBookID, lCookieData.Year);
                            }
                            QuestName = DynamicQuestion(question.Question);
                        }
                        else
                            QuestName = question.Question;

                        newChildQuestion = new QuestionChildModel
                        {
                            ID = question.QuestionID,
                            ParentID = question.ParentQuestionID,
                            Name = QuestName,
                            QuestionType = question.QuestionType,
                            AnswerType = question.AnswerType,
                            AnswerYesNo = question.AnswerYesNo,
                            AnswerFreeForm = question.AnswerFreeForm
                        };
                        newQuestion.QuestionChildrenList.Add(newChildQuestion);
                    }
                    else
                    {
                        IsDynamic = (question.QuestionType == DYNSTATEMENT || question.QuestionType == DYNYESNO);

                        // Resolve Dynamic Questions
                        if (IsDynamic)
                        {
                            // Temp for testing
                            var lBookID = lCookieData.BookID == null ? 8 : lCookieData.BookID;

                            if (String.IsNullOrEmpty(Dynamic.AvgVerifiedCircPercent))
                            {
                                GetDynamicValues(lBookID, lCookieData.Year);
                            }
                            QuestName = DynamicQuestion(question.Question);
                        }
                        else
                            QuestName = question.Question;

                        //QuestName = question.Question;
                        newQuestion = new QuestionModel
                        {
                            ID = question.QuestionID,
                            Order = question.SubSectionToQuestionOrder,
                            Name = QuestName,
                            QuestionType = question.QuestionType,
                            AnswerYesNo = question.AnswerYesNo,
                            AnswerFreeForm = question.AnswerFreeForm
                        };
                    }

                    // Insert final question.
                    if (recordCount == recordTotal)
                    {
                        // Is this a child question?
                        if (IsParent && newQuestion.ID > 0)
                        {
                            newSubSection.QuestionList.Add(newQuestion);
                        }
                        else
                        {
                            newSubSection.QuestionList.Add(newQuestion);
                        }
                        // Add final question to SubSection and Section Lists.
                        newSection.SubSectionList.Add(newSubSection);
                        newSectionList.SectionList.Add(newSection);

                        SectionsListModel = newSectionList;
                    }
                }
            }
        }

        private string DynamicQuestion(string question)
        {
            question = question.Replace(AVGVERIFIEDCIRCPERCENT, Dynamic.AvgVerifiedCircPercent);
            question = question.Replace(GENRE, Dynamic.Genre);

            return question;
        }

        private void GetDynamicValues(int? pBookID, string pYear)
        {
            using (Loreal_DEVEntities4 db = new Loreal_DEVEntities4())
            {
                if (String.IsNullOrEmpty(pYear))
                    pYear = "2017";

                //var lDynamicValues = db.Books
                //    .Where(b => b.BookID == pBookID)
                //    .Include(b => b.Genres)
                //    .FirstOrDefault();

                // For now - come back and streamline with a single query.
                var lGenreID = db.Books
                    .Where(b => b.BookID == pBookID)
                    .Select(b => b.GenreID)
                    .FirstOrDefault();

                var lDynamicValues = db.Genres
                    .Where(g => (g.GenreID == lGenreID) &&
                    (g.Year == pYear))
                    .FirstOrDefault();


                Dynamic.AvgVerifiedCircPercent = lDynamicValues.AvgVerifiedCircPercent.ToString();
                Dynamic.Genre = lDynamicValues.Genre1;
            }
        }
        #endregion
    }
}
