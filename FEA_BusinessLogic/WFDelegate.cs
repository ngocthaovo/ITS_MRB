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
    
    public partial class WFDelegate
    {
        public string DelegateID { get; set; }
        public Nullable<int> MainUserID { get; set; }
        public Nullable<int> DelegateUserID { get; set; }
        public Nullable<System.DateTime> From { get; set; }
        public Nullable<System.DateTime> To { get; set; }
        public Nullable<int> Status { get; set; }
        public string Temp1 { get; set; }
        public Nullable<int> Temp2 { get; set; }
        public string Temp3 { get; set; }
    
        public virtual CostCenter CostCenter { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public virtual WFDocumentType WFDocumentType { get; set; }
    }
}
