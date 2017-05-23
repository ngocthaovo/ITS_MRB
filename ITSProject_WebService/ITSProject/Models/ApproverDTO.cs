using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSProject.Models
{
    public class ApproverDTO
    {
        public Nullable<int> ApproverID { get; set; }
        public string NodeID { get; set; } // next node ID khi sign
        public Nullable<int> Sequence { get; set; }
        public string CheckUserID { get; set; }
        public string CheckUserName { get; set; }
        public string DocumentTypeID { get; set; }
    }
}