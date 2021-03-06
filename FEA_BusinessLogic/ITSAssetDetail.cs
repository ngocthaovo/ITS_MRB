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
    
    public partial class ITSAssetDetail
    {
        public string ID { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string RecID { get; set; }
        public string RecCode { get; set; }
        public string RecName { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string AssetID { get; set; }
        public string Brand { get; set; }
        public string FactoryCode { get; set; }
        public string DivisionCode { get; set; }
        public string DepartmentCode { get; set; }
        public Nullable<System.DateTime> EquipDate { get; set; }
        public string JobPosition { get; set; }
        public string Group { get; set; }
        public string Model { get; set; }
        public string Configuration { get; set; }
        public string EmailCoding { get; set; }
        public string EmailAddress { get; set; }
        public string FJ_B_W { get; set; }
        public string FJ_Color { get; set; }
        public string ExtNo { get; set; }
        public string Code { get; set; }
        public string PassCode { get; set; }
        public string DirectLine { get; set; }
        public string JobTitle { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }
        public string Temp3 { get; set; }
        public string Temp4 { get; set; }
        public string Temp5 { get; set; }
        public System.DateTime UploadDate { get; set; }
        public int UploadBy { get; set; }
        public int Status { get; set; }
        public int CreatedByHand { get; set; }
    
        public virtual ITSAssetFTY ITSAssetFTY { get; set; }
        public virtual ITSAssetFTY ITSAssetFTY1 { get; set; }
        public virtual ITSAssetFTY ITSAssetFTY2 { get; set; }
        public virtual ITSAssetRecData ITSAssetRecData { get; set; }
    }
}
