//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FEA_BusinessLogic
{
    using System;
    using System.Collections.Generic;
    
    public partial class Currency
    {
        public Currency()
        {
            this.GAItems = new HashSet<GAItem>();
            this.MNRequestMains = new HashSet<MNRequestMain>();
        }
    
        public string ID { get; set; }
        public string NAME { get; set; }
        public Nullable<int> Status { get; set; }
    
        public virtual ICollection<GAItem> GAItems { get; set; }
        public virtual ICollection<MNRequestMain> MNRequestMains { get; set; }
    }
}
