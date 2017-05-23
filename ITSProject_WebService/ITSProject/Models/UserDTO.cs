using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSProject.Models
{
    public class UserDTO
    {
        public int UserID { get; set; }
        public string UserCodeID { get; set; }
        public string UserPass { get; set; }
        public string UserName { get; set; }
        public string UserNameEN { get; set; }
        public int CostCenterCode { get; set; }
        public string UserAddress { get; set; }
        public string UserSex { get; set; }
        public Nullable<System.Guid> UserPosstion { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public Nullable<System.DateTime> UserStartDate { get; set; }
        public Nullable<System.DateTime> UserLastLogin { get; set; }
        public Nullable<System.DateTime> UserExpired { get; set; }
        public Nullable<int> UserGroupID { get; set; }
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }
        public string Temp3 { get; set; }
        public string Temp4 { get; set; }
        public Nullable<short> Enabled { get; set; }
        public Nullable<int> FactoryID { get; set; }
    }
}