using loreal_print.MEC_Common.Security;
using loreal_print.Models;
using loreal_print.Models.Rates;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace loreal_print.Controllers_Api
{
    public class RatesController : ApiController
    {

        //public IHttpActionResult GetRates(string pYear)
        //{
        //    var lRates = new List<RatesModel>();

        //    using 


        //    var lResult = lRates;

        //    return lResult;
        //}

        [Route("api/Rates/Model")]
        [HttpGet]
        public IHttpActionResult GetRates()
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

        [Route("api/Rates/getGuaranteedRateBase")]
        [Authorize]
        [HttpGet()]
        public IHttpActionResult getGuaranteedRateBase()
        {
            IHttpActionResult lResult = null;
            //var lRateBaseCirculationGuarantee = int.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lParentAdTypeID = 1;
            var lEditionTypeID = 1;

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                // Enter the RateBaseCirculationGuarantee value.
                var lRateBaseCirculationGuarantee = db.CirculationGuarantees
                    .Where(c => (c.Year == lCookieData.Year
                    && c.BookID == lCookieData.BookID
                    && c.VersionID == lCookieData.VersionID
                    && c.ParentAdTypeID == lParentAdTypeID
                    && c.EditionTypeID == lEditionTypeID))
                    .Select(r => r.RateBaseCirculationGuarantee)
                    .FirstOrDefault();

                if (lRateBaseCirculationGuarantee > -1)
                {
                    lResult = Ok(lRateBaseCirculationGuarantee);
                }
                else
                {
                    lResult = BadRequest();
                }
            }
            return lResult;
        }

        [Route("api/Rates/saveGuaranteedRateBase/rate")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult saveGuaranteedRateBase(int rate)
        {
            // This is for the Page Rate Guarranteed Rate Base
            IHttpActionResult lResult = null;
            var lCirculationGuaranteeID = int.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lParentAdTypeID = 1;
            var lEditionTypeID = 1;

            var lCircGuar = new CirculationGuarantee
            {
                Year = lCookieData.Year,
                BookID = Convert.ToInt32(lCookieData.BookID),
                ParentAdTypeID = lParentAdTypeID,
                EditionTypeID = lEditionTypeID,
                RateBaseCirculationGuarantee = 1000 * rate,
                VersionID = lCookieData.VersionID
            };

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                // Enter the RateBaseCirculationGuarantee value.
                var isUpdate = db.CirculationGuarantees
                    .Where(c => (c.Year == lCircGuar.Year
                    && c.BookID == lCircGuar.BookID
                    && c.VersionID == lCircGuar.VersionID
                    && c.ParentAdTypeID == lCircGuar.ParentAdTypeID
                    && c.EditionTypeID == lCircGuar.EditionTypeID)).Any();

                if (isUpdate == false)
                {
                    // Create
                    db.CirculationGuarantees.Add(lCircGuar);
                    lCirculationGuaranteeID = db.SaveChanges();
                }
                else
                {
                    // Update
                    db.Entry(lCircGuar).State = EntityState.Modified;
                    lCirculationGuaranteeID = db.SaveChanges();
                }
            }

            if (lCirculationGuaranteeID > int.MinValue)
            {
                lResult = Ok(lCirculationGuaranteeID);
            }
            else
            {
                lResult = NotFound();
            }
            return lResult;
        }

        [Route("api/Rates/getGuaranteedRateSubAvgs")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult getGuaranteedRateSubAvgs()
        {
            /* Return the six records associated with the BookID, Year & Version
             * for ParentAdTypes:
             * 1. Insert - 2
             * 2. BRC Card - 3
             * 3. Scent Strip - 4 */
            const string FULLRUN = "Full Run";
            const string SUBSONLY = "Subscriptions Only";
            IHttpActionResult lResult = null;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            int[] lParents = new int[] { 2, 3, 4 };
            var lCircGuars = new List<RateSubAvg>();

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                // Retrieve records
                var lResults = db.CirculationGuarantees
                    .Where(c => (c.Year == lCookieData.Year
                    && c.BookID == lCookieData.BookID
                    && c.VersionID == lCookieData.VersionID
                    && lParents.Contains(c.ParentAdTypeID)))
                    .OrderBy(c => c.ParentAdTypeID)
                    .ThenBy(c => c.EditionTypeID)
                    .Select(c => new { c.EditionTypeID, c.AveragePrintRun, c.RateBaseCirculationGuarantee })
                    .ToList();

                if (lResults.Any())
                {
                    foreach (var record in lResults)
                    {
                        var item = new RateSubAvg
                        {
                            EditionType = record.EditionTypeID == 1 ? FULLRUN : SUBSONLY,
                            AveragePrintRun = Convert.ToInt32(record.AveragePrintRun),
                            RateBaseCirculationGuarantee = Convert.ToInt32(record.RateBaseCirculationGuarantee)
                        };
                        lCircGuars.Add(item);
                    }
                    lResult = Ok(lCircGuars);
                }
                lResult = NotFound();
            }
            return lResult;
        }

        [Route("api/Rates/saveGuaranteedRateSubAvgs/rates")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult saveGuaranteedRateSubAvgs(List<RateSubAvg> rates)
        {
            // TODO: Refactor and merge with saveGuaranteedRateBase.
            /* This is for the Page Rate Guarranteed Rate - for Subscription & Full Run - 
             * for ParentAdTypes:
             * 1. Insert - 2
             * 2. BRC Card - 3
             * 3. Scent Strip - 4 
             * These records/values go in lockstep - the user enters:
             * - RateBaseCirculationGuarantee
             * - AveragePrintRun
             * values for Edition Types (four values total):
             * - Full Run
             * - Subscriptions Only
             * for each of the ParentAdTypes.
             * This results two records for each of the ParentAdTypes:
             * - 6 records total
             * - 12 values (RateBaseCirculationGuarantee & AveragePrintRun) total */
            const string FULLRUN = "Full Run";
            const string SUBSONLY = "Subscriptions Only";

            IHttpActionResult lResult = null;
            var lCirculationGuaranteeID = int.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lParentAdTypeID = 2;
            var lEditionTypeID = 1;
            var lRate = new RateSubAvg();

            // We only need to find one of the six records because they are always entered in lockstep
            var lCircGuar = new CirculationGuarantee
            {
                Year = lCookieData.Year,
                BookID = Convert.ToInt32(lCookieData.BookID),
                ParentAdTypeID = lParentAdTypeID,
                EditionTypeID = lEditionTypeID,
                VersionID = lCookieData.VersionID
            };

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                var isUpdate = db.CirculationGuarantees
                    .Where(c => (c.Year == lCircGuar.Year
                    && c.BookID == lCircGuar.BookID
                    && c.VersionID == lCircGuar.VersionID
                    && c.ParentAdTypeID == lCircGuar.ParentAdTypeID
                    && c.EditionTypeID == lCircGuar.EditionTypeID)).Any();

                // Create Six Records
                for (int p = 2; p < 5; p++)
                {
                    lCircGuar.ParentAdTypeID = p;
                    for (int e = 1; e <= 2; e++)
                    {
                        lCircGuar.EditionTypeID = e;
                        // Insert AveragePrintRun & RateBaseCirculationGuarantee for each EditionType
                        if (e == 1) // EditionType = Full Run
                        {
                            lRate = rates.Where(r => r.EditionType == FULLRUN).FirstOrDefault();
                            lCircGuar.AveragePrintRun = lRate.AveragePrintRun;
                            lCircGuar.RateBaseCirculationGuarantee = lRate.RateBaseCirculationGuarantee;
                        }
                        else // EditionType = Subscriptions Only
                        {
                            lRate = rates.Where(r => r.EditionType == SUBSONLY).FirstOrDefault();
                            lCircGuar.AveragePrintRun = lRate.AveragePrintRun;
                            lCircGuar.RateBaseCirculationGuarantee = lRate.RateBaseCirculationGuarantee;
                        }

                        if (!isUpdate)
                            db.CirculationGuarantees.Add(lCircGuar);
                        else
                            db.Entry(lCircGuar).State = EntityState.Modified;
                    }
                }
                lCirculationGuaranteeID = db.SaveChanges();
            }

            // What do we want to return here?
            if (lCirculationGuaranteeID > int.MinValue)
            {
                lResult = Ok(lCirculationGuaranteeID);
            }
            else
            {
                lResult = NotFound();
            }
            return lResult;
        }

        [Route("api/Rates/getOpenRate/{type}")]
        [Authorize]
        [HttpGet()]
        public IHttpActionResult getOpenRate(string type)
        {
            const string P4C = "P4C";
            var lAdTypeID = int.MinValue;
            var lOpenRate = new RateOpen();

            IHttpActionResult lResult = null;

            // Determine whether P4C or P4CB
            if (type == P4C)
                lAdTypeID = 17;
            else
                lAdTypeID = 18;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lEditionTypeID = 1;

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                var lRate = db.Rates
                    .Where(c => (c.Year == lCookieData.Year
                    && c.BookID == lCookieData.BookID
                    && c.VersionID == lCookieData.VersionID
                    && c.AdTypeID == lAdTypeID
                    && c.EditionTypeID == lEditionTypeID))
                    .FirstOrDefault();

                if (lRate != null)
                {
                    lOpenRate.CPM = Convert.ToDecimal(lRate.CPM);
                    lOpenRate.Rate = lRate.Rate1;
                    lOpenRate.Type = type;
                    lResult = Ok(lOpenRate);
                }
                else
                {
                    lResult = BadRequest();
                }
            }
            return lResult;
        }

        [Route("api/Rates/saveOpenRate/openRate")]
        [Authorize]
        [HttpPost]
        public IHttpActionResult saveOpenRate(RateOpen openRate)
        {
            IHttpActionResult lResult = null;
            var lRateID = int.MinValue;

            const string P4C = "P4C";
            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lAdTypeID = 0;
            var lEditionTypeID = 1;
            var lRateTypeID = 1;
            var lTierID = 0;

            // Determine whether P4C or P4CB
            if (openRate.Type == P4C)
                lAdTypeID = 17;
            else
                lAdTypeID = 18;

            var lRate = new Rate
            {
                Year = lCookieData.Year,
                BookID = Convert.ToInt32(lCookieData.BookID),
                AdTypeID = lAdTypeID,
                EditionTypeID = lEditionTypeID,
                RateTypeID = lRateTypeID,
                TierID = lTierID,
                Rate1 = openRate.Rate,
                CPM = 1000 * openRate.CPM,
                VersionID = lCookieData.VersionID
            };

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                // Enter the RateBaseCirculationGuarantee value.
                var isUpdate = db.Rates
                    .Where(r => (r.Year == lRate.Year
                    && r.BookID == lRate.BookID
                    && r.VersionID == lRate.VersionID
                    && r.AdTypeID == lRate.AdTypeID
                    && r.EditionTypeID == lRate.EditionTypeID)).Any();

                if (isUpdate == false)
                {
                    // Create
                    db.Rates.Add(lRate);
                    lRateID = db.SaveChanges();
                }
                else
                {
                    // Update
                    db.Entry(lRate).State = EntityState.Modified;
                    lRateID = db.SaveChanges();
                }
            }

            if (lRateID > int.MinValue)
            {
                lResult = Ok(lRateID);
            }
            else
            {
                lResult = NotFound();
            }
            return lResult;
        }

        [HttpGet()]
        [Route("api/Rates/getDiscountStructure")]
        [Authorize]
        public IHttpActionResult getDiscountStructure()
        {
            IHttpActionResult lResult = null;
            var lDiscountStructure = new PremiumDiscountRate();
            var pContext = System.Web.HttpContext.Current;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                lDiscountStructure = db.PremiumDiscountRates
                    .Where(p => p.BookID == lCookieData.BookID && p.Year == lCookieData.Year && p.VersionID == lCookieData.VersionID)
                    .FirstOrDefault();
            }

            if (lDiscountStructure != null)
            {
                lResult = Ok(lDiscountStructure);
            }
            else
            {
                lResult = NotFound();
            }
            return lResult;
        }

        [HttpGet()]
        [Route("api/Rates/getScentStripAdTypesOpen")]
        [Authorize]
        public IHttpActionResult getScentStripAdTypesOpen()
        {
            IHttpActionResult lResult = null;
            var pContext = System.Web.HttpContext.Current;
            var lParentAdTypeID = 4; // Scent Strip
            var lAdTypes = new List<RateAdType>();

            // Get Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                // Retrieve records
                var lResults = db.AdTypes
                    .Where(a => a.Year == lCookieData.Year && a.ParentAdTypeID == lParentAdTypeID)
                    .OrderBy(a => a.AdTypeID)
                    .Select(a => new { a.AdTypeID, a.AdType1 })
                    .ToList();

                foreach (var item in lResults)
                {
                    var lAdType = new RateAdType
                    {
                        AdTypeID = item.AdTypeID,
                        AdType = item.AdType1
                    };
                    lAdTypes.Add(lAdType);
                }
            }

            if (lAdTypes.Any())
            {
                lResult = Ok(lAdTypes);
            }
            else
            {
                lResult = NotFound();
            }
            return lResult;
        }

        [HttpGet()]
        [Route("api/Rates/getScentStripAdTypesEarned")]
        [Authorize]
        public IHttpActionResult getScentStripAdTypesEarned()
        {
            IHttpActionResult lResult = null;
            var pContext = System.Web.HttpContext.Current;
            const string SCENTSTRIP = "Scent Strip";
            var lAdTypes = new List<RateAdType>();
            var lRateTiers = new List<RateTier>();
            var lTierCount = 0;

            // Get Year & BookID from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
            {
                // Get number of Tiers for given bookID
                var lTierTotal = db.Tiers.Where(t => (t.Year == lCookieData.Year && t.BookID == lCookieData.BookID && t.TierID != 0)).Count();

                var lVersionNum = db.GetRates(lCookieData.Year, 58, 0);

                // Retrieve records
                var lResults = db.V_TierByParentAdType
                    .Where(a => a.Year == lCookieData.Year && a.BookID == lCookieData.BookID && a.ParentAdType == SCENTSTRIP)
                    .OrderBy(a => a.AdTypeID)
                    .ThenBy(a => a.TierID)
                    .Select(a => new { a.AdTypeID, a.AdType, a.TierID, a.Tier, a.Tier_Range })
                    .ToList();

                foreach (var item in lResults)
                {
                    lTierCount++;
                    var lTier = new RateTier
                    {
                        TierID = item.TierID,
                        Tier = item.Tier,
                        TierRange = item.Tier_Range
                    };
                    lRateTiers.Add(lTier);

                    if (lTierCount == lTierTotal)
                    {
                        var lAdType = new RateAdType
                        {
                            AdTypeID = item.AdTypeID,
                            AdType = item.AdType,
                            RateTiers = lRateTiers,
                        };
                        lAdTypes.Add(lAdType);
                        lTierCount = 0;
                    }
                }
            }

            if (lAdTypes.Any())
            {
                lResult = Ok(lAdTypes);
            }
            else
            {
                lResult = NotFound();
            }
            return lResult;
        }
    }
}
