using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.Statistic
{
    public class Report : Base.Connection
    {
        public List<FEA_BusinessLogic.sp_DeviceRegistrationReport_Result> GetDRReport(string DateTo,string DateFrom )
        {
            return db.sp_DeviceRegistrationReport(DateTo, DateFrom).ToList();
        }

        public List<FEA_BusinessLogic.sp_HardwareRequirementReport_Result> GetHRReport(string DateTo, string DateFrom)
        {
            return db.sp_HardwareRequirementReport(DateTo, DateFrom).ToList();
        }

        public List<FEA_BusinessLogic.sp_HardwareRequirementTotalReport_Result> GetHRTotalReport(string DateTo, string DateFrom)
        {
            return db.sp_HardwareRequirementTotalReport(DateTo, DateFrom).ToList();
        }

        public object DetailGetFinishedDocument(string ID, string DocumentTypeName)
        {
            object X = new object();
            switch (DocumentTypeName)
            {
                case "DEVICEREGISTRATION":
                    X = db.DeviceRegistrationDetails.Where(i => i.DeviceRegistrationID == ID).Select(i => new { i.DeviceRegistration.OrderCode, i.Item.ItemName, i.ItemDetail.ItemDetailName, i.Description }).ToList();
                    break;
                case "HARDWAREREQUIREMENT":
                    X = db.HardwareRequirementDetails.Where(i => i.HardwareRequirementID == ID).Select(i => new { i.HardwareRequirement.OrderCode, i.Item.ItemName, i.ItemDetail.ItemDetailName, i.Quantity,i.Temp1,i.EstimatedPrice,i.EstimatedAmount, i.Description }).ToList();
                    break;
                default:
                    break;
            }
            return X;
        }
    }
}
