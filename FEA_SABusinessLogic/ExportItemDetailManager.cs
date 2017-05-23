using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using FEA_BusinessLogic;
namespace FEA_SABusinessLogic
{
   public class ExportItemDetailManager : FEA_BusinessLogic.Base.Connection
    {
       public decimal CheckForItemDetail(string ExportItemID, string ItemDetailID, decimal InputQuantity)
       {
           decimal? TotalCheckingIN = 0, TotalCheckingOUT = 0;
           decimal? TotalInput = 0;
           decimal? TotalAdjust = 0;

           TotalCheckingIN = db.ExportItemCheckings.Where(i => (i.ExportItemDetail.ExportItemID == ExportItemID) && (ItemDetailID ==i.ExportItemDetail.ItemDetailID) && (i.OperationType =="IN")).Sum(a => a.Quantity);

           TotalCheckingOUT = db.ExportItemCheckings.Where(i => (i.ExportItemDetail.ExportItemID == ExportItemID) && (ItemDetailID == i.ExportItemDetail.ItemDetailID) && (i.OperationType == "OUT")).Sum(a => a.Quantity);


           TotalInput = db.ExportItemDetails.Where(i => (i.ExportItemID == ExportItemID) && (i.ItemDetailID == ItemDetailID)).Sum(i => i.Quantity);
           TotalAdjust = db.ExportItemDetails.Where(i => (i.ExportItem.ExportItemAdjustID == ExportItemID) 
                         && (i.ExportItem.Status == (int)FEA_SABusinessLogic.ExportItemManager.OrderStatus.FINSHED) 
                         && (i.ItemDetailID == ItemDetailID)).Sum(i => i.Quantity);


           TotalCheckingIN = (TotalCheckingIN == null) ? 0 : TotalCheckingIN.Value;
           TotalCheckingOUT = (TotalCheckingOUT == null) ? 0 : TotalCheckingOUT.Value;

           TotalInput = (TotalInput == null) ? 0 : TotalInput.Value;
           TotalAdjust = (TotalAdjust == null) ? 0 : TotalAdjust.Value;

           //return ((TotalInput.Value + TotalCheckingIN.Value) - (TotalCheckingOUT.Value + InputQuantity + TotalAdjust.Value));
           return (TotalCheckingOUT.Value) - (TotalCheckingIN.Value + TotalAdjust.Value + InputQuantity);
       }

       public ExportItemDetail GetItem(string ID)
       {
           return db.ExportItemDetails.Where(i => i.ID == ID).SingleOrDefault();
       }


       /// <summary>
       /// Get List by ExportItem for Adjust Applicaiton
       /// </summary>
       /// <param name="ExportItemID"></param>
       /// <returns></returns>
       public List<ExportItemDetail> GetItemByExportItem(string ExportItemID)
       {
           return db.ExportItemDetails.Where(i => i.ExportItemID == ExportItemID).ToList();
       }
    }
}
