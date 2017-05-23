using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSProject.Models
{
    public class WaitingAreDTO
    {
        public string DocumentID { get; set; }
        public string DocumentCode { get; set; }
        public string DocumentType { get; set; }
        public string StatusString { get; set; }
        public string EmployeeName { get; set; }
        public string ApproverName { get; set; }
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }

        public string NodeID { get; set; }
        public string TypeUser { get; set; }
        public string MainID { get; set; }
        public string MainDetailID { get; set; }
        public int? CheckUserID { get; set; }
        public string DelegateID { get; set; }
        public int? DelegateUserID { get; set; }
        public bool IsUrgent { get; set; }



        //public string Approver { get; set; }
        
        //public DateTime? CreateDate { get; set; }
        //public string Creator { get; set; }
        //public string DelegateID { get; set; }
        
        //public string DocumentTypeName { get; set; }
        //public string isFinished { get; set; }
        //public string OrderCode { get; set; }
        //public int? PostUserID { get; set; }
        //public string Sender { get; set; }
        //public string SignType { get; set; }
    }
}