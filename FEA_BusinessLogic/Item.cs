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
    
    public partial class Item
    {
        public Item()
        {
            this.DeviceRegistrationDetails = new HashSet<DeviceRegistrationDetail>();
            this.ExportItemApproverItems = new HashSet<ExportItemApproverItem>();
            this.GAItemDetails = new HashSet<GAItemDetail>();
            this.ShelfInformations = new HashSet<ShelfInformation>();
            this.ItemDetails = new HashSet<ItemDetail>();
            this.HardwareRequirementDetails = new HashSet<HardwareRequirementDetail>();
            this.HardwareRequirementDetails1 = new HashSet<HardwareRequirementDetail>();
            this.HardwareRequirementDetails2 = new HashSet<HardwareRequirementDetail>();
            this.StockInEquipmentDetails = new HashSet<StockInEquipmentDetail>();
            this.StockInEquipmentDetails1 = new HashSet<StockInEquipmentDetail>();
            this.StockInEquipmentDetails2 = new HashSet<StockInEquipmentDetail>();
            this.StockOutEquipmentDetails = new HashSet<StockOutEquipmentDetail>();
            this.MNRequestMainDetails = new HashSet<MNRequestMainDetail>();
            this.MNStockEquipmentDetails = new HashSet<MNStockEquipmentDetail>();
        }
    
        public string ID { get; set; }
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public Nullable<int> Status { get; set; }
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }
    
        public virtual ICollection<DeviceRegistrationDetail> DeviceRegistrationDetails { get; set; }
        public virtual ICollection<ExportItemApproverItem> ExportItemApproverItems { get; set; }
        public virtual ICollection<GAItemDetail> GAItemDetails { get; set; }
        public virtual ICollection<ShelfInformation> ShelfInformations { get; set; }
        public virtual ICollection<ItemDetail> ItemDetails { get; set; }
        public virtual ICollection<HardwareRequirementDetail> HardwareRequirementDetails { get; set; }
        public virtual ICollection<HardwareRequirementDetail> HardwareRequirementDetails1 { get; set; }
        public virtual ICollection<HardwareRequirementDetail> HardwareRequirementDetails2 { get; set; }
        public virtual ICollection<StockInEquipmentDetail> StockInEquipmentDetails { get; set; }
        public virtual ICollection<StockInEquipmentDetail> StockInEquipmentDetails1 { get; set; }
        public virtual ICollection<StockInEquipmentDetail> StockInEquipmentDetails2 { get; set; }
        public virtual ICollection<StockOutEquipmentDetail> StockOutEquipmentDetails { get; set; }
        public virtual ICollection<MNRequestMainDetail> MNRequestMainDetails { get; set; }
        public virtual ICollection<MNStockEquipmentDetail> MNStockEquipmentDetails { get; set; }
    }
}
