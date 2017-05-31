using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic.Maintenance
{
    public class MNInventoryManager:Base.Connection
    {
        
        public void AddItemToWarehouse(string ItemDetailID, string UnitID, int Quantity, string strDocType, FEA_BusinessLogic.FEA_ITSEntities dbRef)
        {
            string strType = strDocType;
            switch (strDocType)
            {
                case "MAINTENANCESTOCK":
                    strType = "MAINTENANCE";
                    break;
                case "PRODUCTIONSTOCK":
                    strType = "PRODUCTION";
                    break;
            }
            var item = dbRef.MNInventories.Where(i => (i.ItemDetailID == ItemDetailID) && (i.UnitID == UnitID)).SingleOrDefault();
            if (item != null)
                item.Quantity += Quantity;
            else
            {
                item = new MNInventory()
                {
                    ID = Guid.NewGuid().ToString(),
                    DocType =strType,
                    ItemDetailID=ItemDetailID,
                    Quantity=Quantity,
                    UnitID=UnitID
              };
                dbRef.MNInventories.Add(item);
            }
        }

        public int CountInStock (string ItemDetailID, string UnitID)
        {
            var item = db.MNInventories.Where(i => (i.ItemDetailID == ItemDetailID) && (i.UnitID == UnitID)).SingleOrDefault();
            if (item != null)
                return Convert.ToInt16(item.Quantity.Value);
            return 0;
        }

        public List<sp_CheckCancelConfirmMNStockIn_Result> CheckCancelConfirm(string OrderCode)
        {
            return db.sp_CheckCancelConfirmMNStockIn(OrderCode).ToList();
        }

        public List<sp_CheckMaintenanceInventory_Result> GetInventory(string strType)
        {
            return db.sp_CheckMaintenanceInventory(strType).ToList();//Added by Tony (2017-05-29) add document type
        }
        
        public bool UpdateFinalQuantity(List<sp_GetMaintenanceDetailsQuantity_Result> o, string ID)
        {
            using (TransactionScope transaction =new TransactionScope())
            {
                try
                {
                        foreach (sp_GetMaintenanceDetailsQuantity_Result lst in o)
                        {
                            MNRequestMainDetail item = db.MNRequestMainDetails.Where(i => (i.RequestMainID == ID) && (i.ItemDetailID==lst.ItemDetailID)).SingleOrDefault();
                            if (item != null)
                            {
                            if (item.ItemDetailID == lst.ItemDetailID)
                            {
                                item.ITSQuantity = lst.ITSInventory;
                                item.ERPQuantity = lst.ERPInventory;
                                item.PurchaseQuantity = lst.PurchaseQuantity;
                            }
                        }
                    }
                    db.SaveChanges();
                    transaction.Complete();
                    return true;
                }
                catch(Exception ex)
                {
                    transaction.Dispose();
                    return false;
                }
            }
        }

        public ObjectParameter SendQuantityToEland(string ID, int confirmID)
        {
            ObjectParameter outParam = new ObjectParameter("ErrorMsg", typeof(string));
            db.sp_PushMNOrderAndGenerateStockOut(ID,confirmID, outParam);
            return outParam;
        }

        public bool UpdateRequestStatus(string ID, int iStatus, int confirmUserID)
        {
            using (TransactionScope transaction =new TransactionScope())
            {
                try
                {
                    MNRequestMain item = db.MNRequestMains.Where(i => i.ID == ID).SingleOrDefault();
                    if(item !=null)
                    {
                        item.Status = iStatus;
                        item.ConfirmID = confirmUserID;
                        item.ConfirmDate = DateTime.Now;
                    }
                    db.SaveChanges();
                    transaction.Complete();
                    return true;
                }
                catch(Exception ex)
                {
                    transaction.Dispose();
                    return false;
                }
            }
        }
        /// <summary>
        /// Added by Steven 2017-05-03
        /// </summary>
        /// <param name="DateTo"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DocumentType"></param>
        /// <returns></returns>
        public List<sp_GetMNDynamicInventory_Result> GetMnDynamicInventory(string DateTo, string DateFrom, string DocumentType)
        {
            return db.sp_GetMNDynamicInventory(DateFrom,DateTo,DocumentType).ToList();
        }
    }
}
