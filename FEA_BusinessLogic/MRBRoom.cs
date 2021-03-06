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
    
    public partial class MRBRoom
    {
        public MRBRoom()
        {
            this.MRBBookingDetail_Test = new HashSet<MRBBookingDetail_Test>();
            this.MRBRoomEquipments = new HashSet<MRBRoomEquipment>();
        }
    
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomName { get; set; }
        public string Location { get; set; }
        public Nullable<int> NumberOfPeople { get; set; }
        public string ImageUrl { get; set; }
        public string Detail { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual ICollection<MRBBookingDetail_Test> MRBBookingDetail_Test { get; set; }
        public virtual MRBRoomType MRBRoomType { get; set; }
        public virtual ICollection<MRBRoomEquipment> MRBRoomEquipments { get; set; }
    }
}
