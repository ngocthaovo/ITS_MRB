using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Models
{
    public class ItemInfo
    {
        public string ID { get; set; }
        public string ItemID { get; set; }
        public string ItemType { get; set; } // added by Jason: DeviceRegistrationManager.OrderType
        public string ItemName { get; set; }
        public string ItemDetailID { get; set; }
        public string ItemDetailName { get; set; }
        public string Des { get; set; }
        public string ReasonItemDetailID { get; set; }
        public string ReasonItemDetailName { get; set; }
        //Added by Tony (2017-04-26) dùng cho phiếu xuất kho bảo trì
        public int ReceivedUserID { get; set; }
        public string ReceivedUserName { get; set; }


        // Region for Hardware Requiment
        public DateTime? DeliveryDate { get; set; }
        public string UnitID { get; set; }
        public string UnitName { get; set; }
        public int Quantity { get; set; }
        public decimal SAQuantity { get; set; } //Added by Tony (2017-05-29)
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public int QuanInStock { get; set; }
        public int QuanNeeded { get; set; }
        public int ToTalReceiverByDept { get; set; }

        // Region for Security area
        public string Gender { get; set; }
        public int? CostCenterCode { get; set; }
        public string CostCenterName { get; set; }
        public string Spec { get; set; }

        // For Sa Detail
        public string Operate { get; set; }

        // for Ga detail
        public bool IsBroken { get; set; } // the item have ben broken so thay want to return and receive new equip
    }
}