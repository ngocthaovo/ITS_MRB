using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Models.ERPModels
{
    public class GetCostComparingSpanEXCELModel
    {
        public string FactoryName { get; set; }
        public string OrderID { get; set; }
        public string FEPOCode { get; set; }
        // Modify by Tony (24-08-2016)
        public string EnglishName { get; set; }
        //
        public string StyleName { get; set; }
        public string AccountType { get; set; }

        public decimal? OrderQuantity { get; set; }
        public decimal? SalesQty { get; set; }
        public decimal? SalesPrice { get; set; }
        public decimal? SalesAmt { get; set; }
        public decimal? StockInQty { get; set; }
        public decimal? StockInAmt { get; set; }
        public decimal? FabricAmt { get; set; }

        public decimal? FabricPrice { get; set; }
        public decimal? AccessoryAmt { get; set; }
        public decimal? AccessoryPrice { get; set; }
        public decimal? PayAmt { get; set; }
        public decimal? PayPrice { get; set; }
        public decimal? ProduceAmt { get; set; }
        public decimal? ProducePrice { get; set; }
        public decimal? ProcessAmt { get; set; }
        public decimal? ProcessPrice { get; set; }

        public decimal? OutwardAmt { get; set; }
        public decimal? OutwardPrice { get; set; }
        public decimal? DiscountAmt { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? SumPrice { get; set; }
        public decimal? SumAmt { get; set; }

        public decimal? ProfitPrice { get; set; }
        public decimal? ProfitAmt { get; set; }
        public decimal? ProfitRate { get; set; }
    }
}