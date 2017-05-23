using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSProject.Models
{
    public class WFHistoryDTO
    {
        public string OrderCode { get; set; }
        public string Sender { get; set; }
        public string Approver { get; set; }
        public string Comment { get; set; }
        public System.DateTime CheckDate { get; set; }
        public System.DateTime Sequence { get; set; }
        public string Status { get; set; }
    }
}