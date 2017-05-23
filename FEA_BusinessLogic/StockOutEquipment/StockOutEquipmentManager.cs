using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic
{
    public class StockOutEquipmentManager : Base.Connection
    {
        public enum OrderStatus
        {
            DRAFT = 1,
            FINSHED = 5,
            DELETED = 0,
            New = -1
        }
        /// <summary>
        /// Get item in database
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public StockOutEquipment GetItem(string sItemID)
        {
            return db.StockOutEquipments.Where(i => i.ID == sItemID || i.OrderCode == sItemID).SingleOrDefault();
        }

        /// <summary>
        /// Get List by  Creator, this function using for user
        /// </summary>
        /// <param name="iCreatorID"></param>
        /// <returns></returns>
        public List<StockOutEquipment> GetItems(int? iCreatorID = null)
        {
            return db.StockOutEquipments.Where(i =>(iCreatorID == null?true: i.CreatorID == iCreatorID) 
                                                && (i.Status != (int)OrderStatus.DELETED))
                                                .OrderByDescending(i=>i.CreateDate).ToList();
        }

        /// <summary>
        /// Insert Item: 
        /// DeviceRegistration
        /// if OrderStatus was Save end Send then Insert WFMain to DB
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string InsertItem(StockOutEquipment o, bool isSaveDraff)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    o.ID = Guid.NewGuid().ToString();
                    if (o.StockOutEquipmentDetails != null)
                    {
                        ITInventoryManager itInManager = new ITInventoryManager();
                        foreach (StockOutEquipmentDetail i in o.StockOutEquipmentDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                            i.StockOutEquipmentID = o.ID;

                            // Subtract Number in stock
                            if (!isSaveDraff)
                                itInManager.AddItemToWarehouse(i.ItemDetailID, i.UnitID, i.Quantity.Value - (i.Quantity.Value *2), this.db);
                        }
                    }

                    if (isSaveDraff)
                        o.Status = (int)OrderStatus.DRAFT;
                    else o.Status = (int)OrderStatus.FINSHED;

                    db.StockOutEquipments.Add(o);

                    db.SaveChanges();
                    transaction.Complete();
                    return o.ID;
                }
                catch (Exception)
                {
                    transaction.Dispose();
                }
            }
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public bool DeleteItem(string sItemID, int iUserID)
        {
            StockOutEquipment item = db.StockOutEquipments.Where(i => i.ID == sItemID).SingleOrDefault();
            if (item != null)
            {
                if (iUserID != item.CreatorID.Value)
                    return false;
                if (item.Status == (int)OrderStatus.FINSHED)
                    return false;

                item.Status = (int)OrderStatus.DELETED;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update Item
        /// </summary>
        /// <param name="o">DeviceRegistration to Update</param>
        /// <param name="status">OrderStatus</param>
        /// <param name="w">WFMainDetail</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateItem(StockOutEquipment o, Boolean isSaveDraff, params System.Linq.Expressions.Expression<Func<StockOutEquipment, object>>[] properties)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    StockOutEquipment item = db.StockOutEquipments.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
                    if (item != null)
                    {
                        foreach (var propertie in properties)
                        {
                            var lambda = (LambdaExpression)propertie;
                            MemberExpression memberExpression;
                            if (lambda.Body is UnaryExpression)
                                memberExpression = (MemberExpression)((UnaryExpression)lambda.Body).Operand;
                            else
                                memberExpression = (MemberExpression)lambda.Body;

                            string propertyName = memberExpression.Member.Name;
                            item.GetType().GetProperty(propertyName).SetValue(item, o.GetType().GetProperty(propertyName).GetValue(o));
                        }
                    }

                    // Xóa  Detail
                    List<StockOutEquipmentDetail> lstDeviceDetail = db.StockOutEquipmentDetails.Where(i => i.StockOutEquipmentID == item.ID).ToList();
                    if (lstDeviceDetail.Count > 0)
                    {
                        foreach (StockOutEquipmentDetail i in lstDeviceDetail)
                            db.StockOutEquipmentDetails.Remove(i);
                    }
                    //Add detail
                    if (o.StockOutEquipmentDetails != null)
                    {
                        ITInventoryManager itInManager = new ITInventoryManager();
                        foreach (StockOutEquipmentDetail i in o.StockOutEquipmentDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                            i.StockOutEquipmentID = o.ID;
                            db.StockOutEquipmentDetails.Add(i);

                            // Add Number in stock
                            if (!isSaveDraff)
                                itInManager.AddItemToWarehouse(i.ItemDetailID, i.UnitID, i.Quantity.Value- (i.Quantity.Value *2), this.db);
                        }
                    }

                    if (isSaveDraff)
                        item.Status = (int)OrderStatus.DRAFT;
                    else
                    {
                        item.Status = (int)OrderStatus.FINSHED;
                        item.CofirmDate = o.CofirmDate == null ? DateTime.Now : o.CofirmDate;    // Dont update confirm date if it is "Cancel confirm"
                    }

                    db.SaveChanges();
                    transaction.Complete();

                    return true;
                }
                catch
                {
                    transaction.Dispose();
                    return false;
                }
            }
        }

        public bool UpdateCancelItem(StockOutEquipment o, Boolean isSaveDraff, params System.Linq.Expressions.Expression<Func<StockOutEquipment, object>>[] properties)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    StockOutEquipment item = db.StockOutEquipments.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
                   
                    //Add detail
                    if (o.StockOutEquipmentDetails != null)
                    {
                        ITInventoryManager itInManager = new ITInventoryManager();
                        foreach (StockOutEquipmentDetail i in o.StockOutEquipmentDetails)
                        {
                            // Add Number in stock
                            if (!isSaveDraff)
                                itInManager.AddItemToWarehouse(i.ItemDetailID, i.UnitID, i.Quantity.Value, this.db);
                        }
                    }
                    item.Status = (int)OrderStatus.DRAFT;
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
