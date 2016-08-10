using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loreal_print.Models.Rates
{
    public class RatesModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int TieredID { get; set; }
        public int BookID { get; set; }
        public int VersionID { get; set; }
        public string Year { get; set; }
        public int RateTypeID { get; set; }
        public int EditionTypeID { get; set; }
        public int AdTypeID { get; set; }        
    }
}