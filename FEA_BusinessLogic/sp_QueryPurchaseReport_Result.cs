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
    
    public partial class sp_QueryPurchaseReport_Result
    {
        public string Company { get; set; }
        public string PR_Code { get; set; }
        public string PH_Code { get; set; }
        public string Level { get; set; }
        public string Apply_Dept { get; set; }
        public string Type { get; set; }
        public string Supplier_code { get; set; }
        public string Supplier_name { get; set; }
        public string Description { get; set; }
        public string Purchase_Items { get; set; }
        public Nullable<decimal> Quantity { get; set; }
        public string Unit { get; set; }
        public string Currency { get; set; }
        public Nullable<decimal> Price_Px__Forecast_ { get; set; }
        public Nullable<decimal> Amount__Forecast_ { get; set; }
        public Nullable<decimal> Price__Purchased_ { get; set; }
        public Nullable<decimal> Price__Discount_ { get; set; }
        public Nullable<decimal> Amount__Discount_ { get; set; }
        public Nullable<decimal> Amount__Purchased_ { get; set; }
        public string Apply_Apporve_Date { get; set; }
        public string Assignment { get; set; }
        public string Purchaser { get; set; }
        public string Last_approver { get; set; }
        public string Last_approval_date { get; set; }
        public string GM_Approve { get; set; }
        public string Current_approver { get; set; }
        public string Pending_time { get; set; }
    }
}
