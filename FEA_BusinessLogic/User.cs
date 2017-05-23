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
    
    public partial class User
    {
        public User()
        {
            this.LogChangePasses = new HashSet<LogChangePass>();
            this.WFMains = new HashSet<WFMain>();
            this.WFMainDetails = new HashSet<WFMainDetail>();
            this.WFMainDetails1 = new HashSet<WFMainDetail>();
            this.WFNodeDetails = new HashSet<WFNodeDetail>();
            this.ExportItemCheckings = new HashSet<ExportItemChecking>();
            this.StockInEquipments = new HashSet<StockInEquipment>();
            this.StockOutEquipments = new HashSet<StockOutEquipment>();
            this.StockOutEquipments1 = new HashSet<StockOutEquipment>();
            this.WHExportOrders = new HashSet<WHExportOrder>();
            this.WFRefferences = new HashSet<WFRefference>();
            this.PackingManifests = new HashSet<PackingManifest>();
            this.WFRefferenceDetail = new HashSet<WFRefferenceDetail>();
            this.WHImportOrders = new HashSet<WHImportOrder>();
            this.DeviceRegistrations = new HashSet<DeviceRegistration>();
            this.DeviceRegistrations1 = new HashSet<DeviceRegistration>();
            this.HardwareRequirements = new HashSet<HardwareRequirement>();
            this.HardwareRequirements1 = new HashSet<HardwareRequirement>();
            this.ExportItemApproverItems = new HashSet<ExportItemApproverItem>();
            this.ExportItems = new HashSet<ExportItem>();
            this.ExportItems1 = new HashSet<ExportItem>();
            this.HistoryScans = new HashSet<HistoryScan>();
            this.CostCenters = new HashSet<CostCenter>();
            this.GAItems = new HashSet<GAItem>();
            this.ERPDocuments = new HashSet<ERPDocument>();
            this.WFDelegates = new HashSet<WFDelegate>();
            this.WFDelegates1 = new HashSet<WFDelegate>();
            this.MNRequestMains = new HashSet<MNRequestMain>();
            this.MNRequestMains1 = new HashSet<MNRequestMain>();
            this.MNStockEquipments = new HashSet<MNStockEquipment>();
            this.MNStockEquipments1 = new HashSet<MNStockEquipment>();
            this.MNStockEquipmentDetails = new HashSet<MNStockEquipmentDetail>();
        }
    
        public int UserID { get; set; }
        public string UserCodeID { get; set; }
        public string UserPass { get; set; }
        public string UserName { get; set; }
        public string UserNameEN { get; set; }
        public int CostCenterCode { get; set; }
        public string UserAddress { get; set; }
        public string UserSex { get; set; }
        public Nullable<System.Guid> UserPosstion { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public Nullable<System.DateTime> UserStartDate { get; set; }
        public Nullable<System.DateTime> UserLastLogin { get; set; }
        public Nullable<System.DateTime> UserExpired { get; set; }
        public Nullable<int> UserGroupID { get; set; }
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }
        public string Temp3 { get; set; }
        public string Temp4 { get; set; }
        public Nullable<short> Enabled { get; set; }
        public Nullable<int> FactoryID { get; set; }
    
        public virtual CostCenter CostCenter { get; set; }
        public virtual ICollection<LogChangePass> LogChangePasses { get; set; }
        public virtual ICollection<WFMain> WFMains { get; set; }
        public virtual ICollection<WFMainDetail> WFMainDetails { get; set; }
        public virtual ICollection<WFMainDetail> WFMainDetails1 { get; set; }
        public virtual ICollection<WFNodeDetail> WFNodeDetails { get; set; }
        public virtual UserPosition UserPosition { get; set; }
        public virtual UserGroup UserGroup { get; set; }
        public virtual ICollection<ExportItemChecking> ExportItemCheckings { get; set; }
        public virtual ICollection<StockInEquipment> StockInEquipments { get; set; }
        public virtual ICollection<StockOutEquipment> StockOutEquipments { get; set; }
        public virtual ICollection<StockOutEquipment> StockOutEquipments1 { get; set; }
        public virtual ICollection<WHExportOrder> WHExportOrders { get; set; }
        public virtual ICollection<WFRefference> WFRefferences { get; set; }
        public virtual ICollection<PackingManifest> PackingManifests { get; set; }
        public virtual ICollection<WFRefferenceDetail> WFRefferenceDetail { get; set; }
        public virtual ICollection<WHImportOrder> WHImportOrders { get; set; }
        public virtual ICollection<DeviceRegistration> DeviceRegistrations { get; set; }
        public virtual ICollection<DeviceRegistration> DeviceRegistrations1 { get; set; }
        public virtual ICollection<HardwareRequirement> HardwareRequirements { get; set; }
        public virtual ICollection<HardwareRequirement> HardwareRequirements1 { get; set; }
        public virtual ICollection<ExportItemApproverItem> ExportItemApproverItems { get; set; }
        public virtual ICollection<ExportItem> ExportItems { get; set; }
        public virtual ICollection<ExportItem> ExportItems1 { get; set; }
        public virtual ICollection<HistoryScan> HistoryScans { get; set; }
        public virtual ICollection<CostCenter> CostCenters { get; set; }
        public virtual ICollection<GAItem> GAItems { get; set; }
        public virtual ICollection<ERPDocument> ERPDocuments { get; set; }
        public virtual ICollection<WFDelegate> WFDelegates { get; set; }
        public virtual ICollection<WFDelegate> WFDelegates1 { get; set; }
        public virtual ICollection<MNRequestMain> MNRequestMains { get; set; }
        public virtual ICollection<MNRequestMain> MNRequestMains1 { get; set; }
        public virtual ICollection<MNStockEquipment> MNStockEquipments { get; set; }
        public virtual ICollection<MNStockEquipment> MNStockEquipments1 { get; set; }
        public virtual ICollection<MNStockEquipmentDetail> MNStockEquipmentDetails { get; set; }
    }
}
