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
    
    public partial class DeviceRegistrationDetail
    {
        public string ID { get; set; }
        public string DeviceRegistrationID { get; set; }
        public string ItemID { get; set; }
        public string ItemDetailID { get; set; }
        public string Description { get; set; }
        public string Temp1 { get; set; }
        public string ReasonItemDetailID { get; set; }
    
        public virtual Item Item { get; set; }
        public virtual DeviceRegistration DeviceRegistration { get; set; }
        public virtual DeviceRegistration DeviceRegistration1 { get; set; }
        public virtual ItemDetail ItemDetail { get; set; }
        public virtual ItemDetail ItemDetail1 { get; set; }
        public virtual ItemDetail ItemDetail2 { get; set; }
    }
}
