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
    
    public partial class MNRequestMain
    {
        public MNRequestMain()
        {
            this.MNRequestMainDetails = new HashSet<MNRequestMainDetail>();
            this.MNStockEquipments = new HashSet<MNStockEquipment>();
        }
    
        public string ID { get; set; }
        public string OrderCode { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }
        public Nullable<int> IsUrgent { get; set; }
        public Nullable<decimal> EstimatedAmount { get; set; }
        public string CurrencyID { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<int> CreatorID { get; set; }
        public Nullable<int> ConfirmID { get; set; }
        public Nullable<System.DateTime> ConfirmDate { get; set; }
        public Nullable<System.DateTime> LastUpdateDate { get; set; }
        public Nullable<int> Status { get; set; }
        public string DocType { get; set; }
        public string AttachmentLink { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
    
        public virtual Currency Currency { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public virtual ICollection<MNRequestMainDetail> MNRequestMainDetails { get; set; }
        public virtual ICollection<MNStockEquipment> MNStockEquipments { get; set; }
    }
}
