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
    
    public partial class ITSAssetFTY1
    {
        public ITSAssetFTY1()
        {
            this.ITSAssetDetails = new HashSet<ITSAssetDetail1>();
            this.ITSAssetDetails1 = new HashSet<ITSAssetDetail1>();
            this.ITSAssetDetails2 = new HashSet<ITSAssetDetail1>();
        }
    
        public string ID { get; set; }
        public string Name { get; set; }
        public Nullable<int> Type { get; set; }
        public Nullable<int> Status { get; set; }
        public string ParentID { get; set; }
    
        public virtual ICollection<ITSAssetDetail1> ITSAssetDetails { get; set; }
        public virtual ICollection<ITSAssetDetail1> ITSAssetDetails1 { get; set; }
        public virtual ICollection<ITSAssetDetail1> ITSAssetDetails2 { get; set; }
    }
}
