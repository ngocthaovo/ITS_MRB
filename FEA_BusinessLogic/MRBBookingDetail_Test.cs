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
    
    public partial class MRBBookingDetail_Test
    {
        public int ID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<bool> AllDay { get; set; }
        public Nullable<int> Status { get; set; }
        public string Label { get; set; }
        public Nullable<int> RoomId { get; set; }
        public string UserList { get; set; }
        public Nullable<int> UserID { get; set; }
        public string ItemID { get; set; }
        public string ItemDetailID { get; set; }
        public string ItemList { get; set; }
        public string Location { get; set; }
    
        public virtual MRBRoom MRBRoom { get; set; }
        public virtual User User { get; set; }
    }
}