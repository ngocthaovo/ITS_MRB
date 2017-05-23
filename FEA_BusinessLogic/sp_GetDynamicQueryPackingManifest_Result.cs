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
    
    public partial class sp_GetDynamicQueryPackingManifest_Result
    {
        public string CustomerPO { get; set; }
        public string PONo { get; set; }
        public string InvoiceNo { get; set; }
        public string PackingManifestType { get; set; }
        public string MainLine_ { get; set; }
        public string ColorName { get; set; }
        public string ShortDescription { get; set; }
        public string Size { get; set; }
        public Nullable<decimal> ContainerCount { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> TotalContainerNotConfirmed { get; set; }
        public Nullable<int> TotalQuantityNotConfirmed { get; set; }
        public Nullable<decimal> TotalContainerConfirmed { get; set; }
        public Nullable<int> TotalQuantityConfirmed { get; set; }
        public Nullable<decimal> TotalReturnContainer { get; set; }
        public Nullable<int> TotalReturnQuantity { get; set; }
        public Nullable<decimal> TotalContainerImported { get; set; }
        public Nullable<int> TotaQuantityImported { get; set; }
        public Nullable<int> TotalStockedInContainerFromERP { get; set; }
        public int TotalStockedInQuantityFromERP { get; set; }
        public Nullable<decimal> EndTotalContainer { get; set; }
        public Nullable<int> EndTotalQuantity { get; set; }
        public Nullable<decimal> ContainerDifferences { get; set; }
        public Nullable<int> QuantityDifferences { get; set; }
        public Nullable<decimal> ContainerDifferencesBetweenWarehouseAndPacking { get; set; }
        public Nullable<int> QuantityDifferencesBetweenWarehouseAndPacking { get; set; }
        public int OrderState { get; set; }
        public Nullable<int> TotalExportItem { get; set; }
        public Nullable<int> TotalExportItemFromERP { get; set; }
    }
}
