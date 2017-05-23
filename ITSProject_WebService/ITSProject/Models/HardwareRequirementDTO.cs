using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ITSProject.Models
{
    public class HardwareRequirementDTO
    {
        public string ID { get; set; }
        public string OrderCode { get; set; }
        public Nullable<int> IsUrgent { get; set; }
        public Nullable<decimal> EstimatedAmount { get; set; }
        public string CurrencyID { get; set; }
        public string Description { get; set; }
        public string Reason { get; set; }
        public Nullable<int> CreatorID { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<int> Status { get; set; }
        public string Temp1 { get; set; }
        public string Temp2 { get; set; }
        public Nullable<int> TechnicianID { get; set; }
        public Nullable<System.DateTime> ConfirmDate { get; set; }
        public string AttachmentLink { get; set; }
        public string ProcessedBy { get; set; }
        

        public List<HardwareRequirementDetailDTO> HardwareRequirementDetailDTOs { get; set; }
        public List<ApproverDTO> ApproverDTOs { get; set; }
        public List<WFHistoryDTO> WFHistoryDTOs { get; set; }
        public List<FileInfoDTO> FileInfoDTOs { get; set; }
    }
}