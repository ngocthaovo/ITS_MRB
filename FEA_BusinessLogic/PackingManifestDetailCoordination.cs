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
    
    public partial class PackingManifestDetailCoordination
    {
        public string ID { get; set; }
        public string PackingManifestDetailID { get; set; }
        public string RANGE { get; set; }
        public long SerialNo { get; set; }
        public string PackCode { get; set; }
        public string Line_ { get; set; }
        public string PO_ { get; set; }
        public string BuyerItem_ { get; set; }
        public string SKU_ { get; set; }
        public string MainLine_ { get; set; }
        public string ColorName { get; set; }
        public string Size { get; set; }
        public string ShortDescription { get; set; }
        public string ShipmentMethod { get; set; }
        public Nullable<int> ItemQuantity { get; set; }
        public Nullable<int> ItemQtyPerCtnPack { get; set; }
        public Nullable<int> InnetPkgCount { get; set; }
        public string R { get; set; }
        public string CtnCode { get; set; }
        public string NETNET { get; set; }
        public string NET { get; set; }
        public string GROSS { get; set; }
        public string UNIT { get; set; }
        public string L { get; set; }
        public string W { get; set; }
        public string H { get; set; }
        public string UNIT2 { get; set; }
        public string MaterialStyle { get; set; }
        public string CustomerCrossReferenceNumber { get; set; }
        public string UPC_EAN { get; set; }
        public Nullable<System.DateTime> EstShipDate { get; set; }
    
        public virtual PackingManifestDetail PackingManifestDetail { get; set; }
    }
}
