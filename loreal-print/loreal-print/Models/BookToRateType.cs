//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace loreal_print.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BookToRateType
    {
        public string Year { get; set; }
        public int BookID { get; set; }
        public int RateTypeID { get; set; }
        public System.DateTime Load_Date { get; set; }
    
        public virtual Book Book { get; set; }
        public virtual RateType RateType { get; set; }
    }
}
