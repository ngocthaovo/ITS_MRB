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
    
    public partial class MRBRoomEquipment
    {
        public int RoomId { get; set; }
        public string ItemDetailID { get; set; }
        public string MRBRoomEquipmentName { get; set; }
    
        public virtual ItemDetail ItemDetail { get; set; }
        public virtual MRBRoom MRBRoom { get; set; }
    }
}