using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic.WarehouseArea
{
    public class WHImportOrderManager : Base.Connection
    {
        public enum OrderStatus
        {
            NORMAL = 1,
            SHELFED = 2,
            DELETED = 0,
            New = -1
        }
        /// <summary>
        /// Get item list
        /// </summary>
        /// <param name="CreatorID"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public List<WHImportOrder> GetItems(string ordercode,DateTime begindate, DateTime enddate,int CreatorID, int Status = -1)
        {
            begindate = new DateTime(begindate.Year, begindate.Month, begindate.Day, 0, 0, 0);
            enddate = new DateTime(enddate.Year, enddate.Month, enddate.Day, 23, 59, 59);

            return db.WHImportOrders.Where(i =>
                    (CreatorID > 0 ? i.CreatorID == CreatorID : true)
                    && (Status > -1 ? i.Status == Status : true)
                    &&(i.Status != (int)OrderStatus.DELETED)
                    &&(i.OrderCode.Contains(ordercode))
                    &&(i.CreateDate >= begindate)
                    &&(i.CreateDate <=enddate)
                ).OrderByDescending(i=>i.CreateDate).ToList();
        }

        public WHImportOrder GetItem(string ItemID)
        {

            return db.WHImportOrders.Where(
                                        i =>
                                        i.Status != (int) OrderStatus.DELETED
                                        && ( i.ID == ItemID || i.OrderCode == ItemID)
                
                ).SingleOrDefault();

        }

        public string InsertItem(WHImportOrder o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    if (o.WHImportOrderDetails != null)
                    {
                        foreach (WHImportOrderDetail i in o.WHImportOrderDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                        }
                    }

                    if (o.WHImportOrderDetails.Count != 0)
                    {
                        foreach (WHImportOrderDetail ImportOrderDetail in o.WHImportOrderDetails)
                        {
                            List<WHImportOrderDetail> lstDetailItem = db.WHImportOrderDetails.Where(i => i.PackingManifestDetailID == ImportOrderDetail.PackingManifestDetailID).ToList();
                            foreach (WHImportOrderDetail item2 in lstDetailItem)
                            {
                                item2.Status = 3;
                                item2.ShelfID = null;
                            }
                        }
                   }

                    db.WHImportOrders.Add(o);
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

        public bool UpdateItem(WHImportOrder o, params System.Linq.Expressions.Expression<Func<WHImportOrder, object>>[] properties)
        {

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    WHImportOrder item = db.WHImportOrders.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>-1: có barcode dang tren ke, 0: khong tim thay Item</returns>
        public int Deleteitem(string ID)
        {
            var item = db.WHImportOrders.Where(i => i.ID == ID && i.Status != (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderManager.OrderStatus.DELETED).SingleOrDefault();
            if (item != null)
            {
                int count = item.WHImportOrderDetails.Where(i =>
                            (i.ShelfID != null) || (i.Status == (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderDetailManager.ItemStatus.Shelfed) || (i.Status == (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderDetailManager.ItemStatus.Exported)
                            ).Count();

                if (count > 0)
                    return -1;
                else
                {
                    item.Status = (int)OrderStatus.DELETED;
                    db.SaveChanges();
                    return 1;
                }
            }

            return 0;
        }

        public bool UpdateItemForScan(WHImportOrder o, params System.Linq.Expressions.Expression<Func<WHImportOrder, object>>[] properties)
        {

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    WHImportOrder item = db.WHImportOrders.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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

        public List<sp_GetDataForDashBoard_Result> GetDataForDashBoard(string ShelfID)
        {
            
            return db.sp_GetDataForDashBoard(ShelfID).OrderBy(i => i.ShelfCode).ToList();
        }

        public List<sp_GetTotalShelfDataReport_Result> GetTotalShelfDataReport(string CustomerPO,int SerialNo)
        {
            return db.sp_GetTotalShelfDataReport(CustomerPO, SerialNo).OrderBy(i => i.ShelfCode).ToList();
        }

        //add by Kane 2016-08-01 -- Get Shelf information by Coputername
        public ShelfInformation GetShelfID(string computerID)
        {
            return db.ShelfInformations.Where(i => i.ComputerID == computerID).SingleOrDefault();
        }
    }
}
