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
    
    public partial class ITInventory
    {
        public string ID { get; set; }
        public string ItemDetailID { get; set; }
        public string UnitID { get; set; }
        public Nullable<int> Quantity { get; set; }
    
        public virtual Unit Unit { get; set; }
        public virtual ItemDetail ItemDetail { get; set; }
    }
}
