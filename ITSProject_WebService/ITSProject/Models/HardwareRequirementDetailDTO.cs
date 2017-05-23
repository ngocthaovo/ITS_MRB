using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSProject.Models
{
    public class HardwareRequirementDetailDTO
    {
        public string ID { get; set; }
        public string HardwareRequirementID { get; set; }
        public string ItemNo { get; set; }
        public string ItemID { get; set; }
        public string ItemDetailID { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string UnitID { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public Nullable<decimal> EstimatedPrice { get; set; }
        public Nullable<decimal> EstimatedAmount { get; set; }
        public string Description { get; set; }
        public Nullable<int> Temp1 { get; set; }
        public Nullable<decimal> Temp2 { get; set; }

        public string ItemDetailName { get; set; }
        public string ItemName  { get; set; }
        public string UnitName  { get; set; }
    }
}