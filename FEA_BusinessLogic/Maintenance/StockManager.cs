using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Linq.Expressions;

namespace FEA_BusinessLogic.Maintenance
{
    public class StockManager: Base.Connection 
    {
        public enum OrderStatus
        {
            DRAFT=1,
            FINISHED=5,
            DELETE=0,
            NEW=-1
        }

        public MNStockEquipment GetItem(string sItemID)
        {
            return db.MNStockEquipments.Where(i => i.ID == sItemID || i.OrderCode == sItemID).SingleOrDefault();
        }

        public List<MNStockEquipment> GetItemByUser(int iOrderType, string docType, int? iCreatorID=null)
        {
            return db.MNStockEquipments.Where(i => (iCreatorID == null ? true : i.CreatorID == iCreatorID)
                && (i.Status != (int)OrderStatus.DELETE)
                && (i.OrderType==iOrderType)
                && (i.DocType==docType)).OrderByDescending(i => i.CreateDate).ToList();
        }
        
        public string InsertItem(MNStockEquipment o, bool isSaveDraft)
        {
            using (TransactionScope transaction =new TransactionScope())
            {
                try
                {
                    o.ID = Guid.NewGuid().ToString();
                    
                    if(o.MNStockEquipmentDetails!=null)
                    {
                        MNInventoryManager mnManager = new MNInventoryManager();
                        foreach(MNStockEquipmentDetail i in o.MNStockEquipmentDetails)
                        {
                            i.DetailID = Guid.NewGuid().ToString();
                            i.StockInEquipmentID = o.ID;
                            if (o.OrderType == 2)
                                i.ReceiveUserID = null;
                            if (!isSaveDraft)
                                if(o.OrderType==2)
                                    mnManager.AddItemToWarehouse(i.ItemDetailID, i.UnitID, Convert.ToInt16( - i.Quantity.Value), this.db);
                                else
                                    mnManager.AddItemToWarehouse(i.ItemDetailID, i.UnitID, Convert.ToInt16(i.Quantity.Value), this.db);
                        }
                    }
                    if (isSaveDraft)
                        o.Status = (int)OrderStatus.DRAFT;
                    else o.Status = (int)OrderStatus.FINISHED;
                    db.MNStockEquipments.Add(o);

                    db.SaveChanges();
                    transaction.Complete();
                    return o.ID;
                }
                catch(Exception ex)
                {
                    string strErr = ex.Message;
                    transaction.Dispose();
                }
            }
            return "";
        }
        
        public bool UpdateItem (MNStockEquipment o, Boolean isSaveDraft, params System.Linq.Expressions.Expression<Func<MNStockEquipment, object>>[] properties)
        {
            using (TransactionScope transaction =new TransactionScope())
            {
                try
                {
                    MNStockEquipment item = db.MNStockEquipments.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
                    if(item !=null)
                    {
                        foreach(var propertie in properties)
                        {
                            var lamba = (LambdaExpression)propertie;
                            MemberExpression memberExpression;
                            if (lamba.Body is UnaryExpression)
                                memberExpression = (MemberExpression)((UnaryExpression)lamba.Body).Operand;
                            else
                                memberExpression = (MemberExpression)lamba.Body;
                            string propertyName = memberExpression.Member.Name;
                            item.GetType().GetProperty(propertyName).SetValue(item, o.GetType().GetProperty(propertyName).GetValue(o));
                        }
                    }

                    List<MNStockEquipmentDetail> lstDetail = db.MNStockEquipmentDetails.Where(i => i.StockInEquipmentID == item.ID).ToList();
                    if(lstDetail.Count>0)
                    {
                        foreach (MNStockEquipmentDetail i in lstDetail)
                            db.MNStockEquipmentDetails.Remove(i);
                    }

                    if(o.MNStockEquipmentDetails !=null)
                    {
                        MNInventoryManager mnInManager = new MNInventoryManager();
                        foreach(MNStockEquipmentDetail i in o.MNStockEquipmentDetails)
                        {
                            i.DetailID = Guid.NewGuid().ToString();
                            i.StockInEquipmentID = o.ID;
                            if (o.OrderType == 2)
                                i.ReceiveUserID = null;
                            db.MNStockEquipmentDetails.Add(i);

                            if (!isSaveDraft)
                                if(o.OrderType==2)
                                    mnInManager.AddItemToWarehouse(i.ItemDetailID, i.UnitID, Convert.ToInt16(- i.Quantity.Value), this.db);
                                else
                                    mnInManager.AddItemToWarehouse(i.ItemDetailID, i.UnitID, Convert.ToInt16(i.Quantity.Value), this.db);
                        }
                    }

                    if (isSaveDraft)
                        item.Status = (int)OrderStatus.DRAFT;
                    else
                    {
                        item.Status = (int)OrderStatus.FINISHED;
                        item.LastUpdateDate = DateTime.Now;
                    }
                    db.SaveChanges();
                    transaction.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    string msgERR = ex.Message;
                    transaction.Dispose();
                    return false;
                }
            }
        }

        public bool DeleteItem(string sItemID, int iUserID)
        {
           MNStockEquipment item = db.MNStockEquipments.Where(i => i.ID == sItemID).SingleOrDefault();
            if(item !=null)
            {
                if (iUserID != item.CreatorID.Value)
                    return false;
                if (item.Status == (int)OrderStatus.FINISHED)
                    return false;
                item.Status = (int)OrderStatus.DELETE;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool CancelStockConfirmItem(MNStockEquipment o, Boolean isSaveDraft,params System.Linq.Expressions.Expression<Func<MNStockEquipment, object>>[] properties)
        {
            using (TransactionScope transaction =new TransactionScope())
            {
                try 
                {
                    MNStockEquipment item =db.MNStockEquipments.Where(i=>i.ID==o.ID||i.OrderCode==o.OrderCode).SingleOrDefault();
                    if(o.MNStockEquipmentDetails !=null)
                    {
                        MNInventoryManager mnManager=new MNInventoryManager();
                        foreach(MNStockEquipmentDetail i in o.MNStockEquipmentDetails)
                        {
                            if(!isSaveDraft)
                                mnManager.AddItemToWarehouse(i.ItemDetailID,i.UnitID, Convert.ToInt16(i.Quantity.Value), this.db);
                        }
                    }
                    item.Status=(int)OrderStatus.DRAFT;
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

    }
}
