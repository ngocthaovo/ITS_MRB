using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic.WarehouseArea
{
    public class WHExportOrderManager : Base.Connection
    {

        public enum OrderStatus
        {
            NORMAL = 1,
            DELETED = 0,
            New = -1
        }
        public enum ConfirmStatus
        {
            DRAFF = 0,
            CONFIRMED =1,
        }


        /// <summary>
        /// Get item list
        /// </summary>
        /// <param name="CreatorID"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public List<WHExportOrder> GetItems(int CreatorID, int Status =-1)
        {
            return db.WHExportOrders.Where(i =>
                    (CreatorID > 0 ? i.CreatorID == CreatorID : true)
                    && (Status > -1 ? i.Status == Status : true)
                ).ToList();
        }

        /// <summary>
        /// Get an item
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public WHExportOrder GetItem(string sItemID)
        {
            return db.WHExportOrders.Where(i => i.ID == sItemID || i.OrderCode == sItemID).SingleOrDefault();
        }


        /// <summary>
        /// Get List by  Creator, this function using for user
        /// </summary>
        /// <param name="iCreatorID"> -1 if get all</param>
        /// <returns></returns>
        //public List<WHExportOrder> GetItems(/*int iCreatorID*/ DateTime DateFrom, DateTime DateTo)
        //{                                   
        //    return db.WHExportOrders.Where(i => i.CreateDate <= DateTo && i.CreateDate >= DateFrom
        //                                        //((iCreatorID > -1) ? i.CreatorID == iCreatorID : true)
        //                                        && i.Status != (int)OrderStatus.DELETED).OrderByDescending(i => i.CreateDate).ToList();
        //}

        public List<sp_GetWHExportOrderByMainLine_Result> GetItems(DateTime DateFrom, DateTime DateTo)
        {
            return db.sp_GetWHExportOrderByMainLine(DateFrom, DateTo).ToList();
        }

        /// <summary>
        /// Delete item
        /// </summary>
        /// <param name="ID"></param>
        /// <returns>false: cannot be deleted</returns>
        public bool DeleteItem(string ID,int CreateUser)
        {
            var item = db.WHExportOrders.Where(i => i.ID == ID).SingleOrDefault();
            if (item != null && CreateUser == item.CreatorID)
            {
                if (item.isConfirm == (int)ConfirmStatus.CONFIRMED)
                    return false;

                // Check for Item have been exported
                var count = item.WHExportOrderDetails.Where(u => u.IsExported == 1).Count();
                if (count > 0)
                    return false;


                item.Status = (int)OrderStatus.DELETED;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Insert Item
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string InsertItem(WHExportOrder o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    o.ID = Guid.NewGuid().ToString();
                    o.CreateDate = DateTime.Now;


                    if (o.Note == null) o.Note = "";
                    if (o.Temp1 == null) o.Temp1 = "";
                    if (o.Temp2 == null) o.Temp2 = "";
                    if (o.Temp3 == null) o.Temp3 = "";

                    if (o.WHExportOrderDetails != null)
                    {
                        foreach (WHExportOrderDetail i in o.WHExportOrderDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                        }
                    }

                    db.WHExportOrders.Add(o);
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
        /// Update Item
        /// </summary>
        /// <param name="o"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateItem(WHExportOrder o, params System.Linq.Expressions.Expression<Func<WHExportOrder, object>>[] properties)
        {
            if (o.Note == null) o.Note = "";
            if (o.Temp1 == null) o.Temp1 = "";
            if (o.Temp2 == null) o.Temp2 = "";
            if (o.Temp3 == null) o.Temp3 = "";

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    WHExportOrder item = db.WHExportOrders.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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


                    // Xóa WHExportOrderDetail
                    List<WHExportOrderDetail> lstDetail = db.WHExportOrderDetails.Where(i => i.ExportOrderID == item.ID).ToList();
                    if (lstDetail.Count > 0)
                    {
                        foreach (WHExportOrderDetail i in lstDetail)
                            db.WHExportOrderDetails.Remove(i);
                    }
                    //Add WHExportOrderDetail
                    if (o.WHExportOrderDetails != null)
                    {
                        foreach (WHExportOrderDetail i in o.WHExportOrderDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                            i.ExportOrderID = o.ID;
                            db.WHExportOrderDetails.Add(i);
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
        /// Confirm Order
        /// </summary>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        public int ConfirmOrder(string OrderID)
        {
            var item = db.WHExportOrders.Where(i => (i.ID == OrderID || i.OrderCode == OrderID)
                                                && (i.Status != (int)OrderStatus.DELETED)

                ).SingleOrDefault();

            if (item != null)
            {
                item.isConfirm = (int)ConfirmStatus.CONFIRMED;
                db.SaveChanges();
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        public int CancelOrder(string OrderID)
        {
            var item = db.WHExportOrders.Where(i => (i.ID == OrderID || i.OrderCode == OrderID)
                                    && (i.Status != (int)OrderStatus.DELETED)

                                    ).SingleOrDefault();
            if (item != null)
            {

                item.isConfirm = (int)ConfirmStatus.DRAFF;
                // Check for Item have been exported
                var count = item.WHExportOrderDetails.Where(u => u.IsExported == 1).Count();
                if (count > 0)
                    return -1;


                db.SaveChanges();
                return 1;
            }
            return 0;
        }
    }
}
