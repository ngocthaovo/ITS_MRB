using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

using FEA_BusinessLogic;
using System.Data.Objects;

namespace FEA_SABusinessLogic
{
    public class ExportItemManager : FEA_BusinessLogic.Base.Connection
    {
        public enum OrderType
        {
            Normal = 1,
            Lend = 2,
            Borrow = 3,
            Donative = 4,
            Adjust = 5
        }

        public enum OrderStatus
        {
            DRAFT = 1,
            SENDING = 2,
            CHECKED = 3,
            RETURNED = 4,
            FINSHED = 5,
            SPENDING = 6,
            DELETED = 0,
            New = -1
        }


        public List<ExportItem> GetItems(int MemberID, int Type)
        {
            return db.ExportItems.Where(i => 
                
                (i.CreatorID == MemberID)
                && (i.OrderType == Type || (i.OrderType == (int)OrderType.Adjust && i.ExportItem2.OrderType == Type))
                && (i.Status != (int)OrderStatus.DELETED)
                
                
                ).ToList();
        }

        public List<ExportItem> GetItemByStatus(int Status)
        {
            return db.ExportItems.Where(i =>
                (i.Status != (int)OrderStatus.DELETED)
                && ((Status>-1)?i.Status == Status:true)

                ).ToList();
        }


        public bool DeleteItem(string ID)
        {
            var item = db.ExportItems.Where(i => i.ID == ID).SingleOrDefault();
            if (item != null)
            {
                if (item.Status == (int)OrderStatus.CHECKED
                   || item.Status == (int)OrderStatus.SENDING
                   || item.Status == (int)OrderStatus.FINSHED
                   || item.Status == (int)OrderStatus.SPENDING
                    )
                    return false;

                // Hidden List of Sign
                if (item.Status == (int)OrderStatus.RETURNED)
                {
                    WFMainDetail wfDtail = db.WFMainDetails.Where(i => i.WFMain.OrderCode == item.OrderCode && i.isFinished == 0 && i.NodeID == "").SingleOrDefault();
                    if (wfDtail != null)
                    {
                        wfDtail.isFinished = 1;
                        wfDtail.CheckDate = DateTime.Now;
                    }
                }

                item.Status = (int)OrderStatus.DELETED;
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// This function only for Admin
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<sp_GetApproveDocumentForSADepartment_Result> GetItemsForApprove()
        {
            return db.sp_GetApproveDocumentForSADepartment((int)OrderStatus.CHECKED).ToList(); //db.DeviceRegistrations.Where(i => i.Status == (int)status).ToList();
        }

        /// <summary>
        /// This function only for Admin
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<sp_GetApproveDocumentForSADepartment_Result> GetItemsForHistory()
        {
            return db.sp_GetApproveDocumentForSADepartment((int)OrderStatus.FINSHED).ToList(); //db.DeviceRegistrations.Where(i => i.Status == (int)status).ToList();
        }
        /// <summary>
        /// This function only for Admin
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public int CountItems()
        {

            return db.sp_GetApproveDocumentForSADepartment((int)OrderStatus.CHECKED).ToList().Count(); //db.DeviceRegistrations.Where(i => i.Status == (int)status).ToList();
        }

        public ExportItem GetItem(string sItemID)
        {
            return db.ExportItems.Where(i => i.ID == sItemID || i.OrderCode == sItemID).SingleOrDefault();
        }

        public string InsertItem(ExportItem o, OrderStatus status, WFMain w)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                   // o.ID = Guid.NewGuid().ToString();
                    if (o.ID.Length == 0 || o.ID == null) { o.ID = Guid.NewGuid().ToString(); }
                    if (o.Description == null) o.Description = "";
                    if (o.ReasonID == null) o.ReasonID = "";

                    if (o.ExportItemDetails != null)
                    {
                        int sequence = 0;
                        foreach (ExportItemDetail i in o.ExportItemDetails)
                        {
                            sequence += 1;
                            i.ID = Guid.NewGuid().ToString();
                            i.ExportItemID = o.ID;
                        }
                    }

                    db.ExportItems.Add(o);

                    // Check for sending?
                    if (status == OrderStatus.SENDING)
                    {
                        db.WFMains.Add(w);
                    }

                    db.SaveChanges();
                    transaction.Complete();
                    return o.ID;
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                }
            }
            return "";
        }

        /// <summary>
        /// Update Item
        /// </summary>
        /// <param name="o">to Update</param>
        /// <param name="status">OrderStatus</param>
        /// <param name="w">WFMainDetail</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateItem(ExportItem o, Boolean isSaveDraff, Boolean isReturned, WFMainDetail w, params System.Linq.Expressions.Expression<Func<ExportItem, object>>[] properties)
        {
            if (o.Description == null) o.Description = "";
            if (o.ReasonName == null) o.ReasonName = "";
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    ExportItem item = db.ExportItems.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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
                        if (!isSaveDraff && item.OrderType != (int)ExportItemManager.OrderType.Donative)
                            item.Status = (int)DeviceRegistrationManager.OrderStatus.SENDING;
                        else if (!isSaveDraff && item.OrderType == (int)ExportItemManager.OrderType.Donative)
                            item.Status = (int)DeviceRegistrationManager.OrderStatus.CHECKED;
                        // Update  DeviceRegistrationDetail
                    }

                    // Xóa Device Registration Detail
                    List<ExportItemDetail> lstDetail = db.ExportItemDetails.Where(i => i.ExportItemID == item.ID).ToList();
                    if (lstDetail.Count > 0)
                    {
                        foreach (ExportItemDetail i in lstDetail)
                            db.ExportItemDetails.Remove(i);
                    }
                    //Add DeviceRegistrationDetails
                    if (o.ExportItemDetails != null)
                    {
                        foreach (ExportItemDetail i in o.ExportItemDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                            i.ExportItemID = o.ID;
                            db.ExportItemDetails.Add(i);
                        }
                    }


                    if (!isSaveDraff)
                    {
                        if (o.OrderType != (int)ExportItemManager.OrderType.Donative) // Nếu là tặng cho hàng thành phẩm thì ko cần phải ký
                        {
                            if (isReturned)
                            {
                                WFMainDetail wfDtail = db.WFMainDetails.Where(i => i.WFMain.OrderCode == o.OrderCode && i.isFinished == 0 && i.NodeID == "").SingleOrDefault();
                                if (wfDtail != null)
                                {
                                    wfDtail.isFinished = 1;
                                    wfDtail.CheckDate = DateTime.Now;
                                }
                            }
                            db.WFMainDetails.Add(w);
                        }
                    }
                    db.SaveChanges();
                    transaction.Complete();

                    return true;
                }
                catch (Exception exception)
                {
                    transaction.Dispose();
                    return false;
                }
            }
        }


        public int AdminApproveOrder(List<string> IDs, int iApproverID)
        {
            try
            {
                List<ExportItem> lstItem = db.ExportItems.Where(i => IDs.Contains(i.ID) || IDs.Contains(i.OrderCode)).ToList();
                foreach (ExportItem item in lstItem)
                {

                    item.UserProcessID = iApproverID;
                    item.ConfirmDate = DateTime.Now;

                    if (item.OrderType == (int)OrderType.Adjust)
                        item.Status = (int)OrderStatus.FINSHED;
                    else
                        item.Status = (int)OrderStatus.SPENDING;
                }

                db.SaveChanges();

                return lstItem.Count();
            }
            catch{ return -1; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateStatus(ExportItem o, params System.Linq.Expressions.Expression<Func<ExportItem, object>>[] properties)
        {
            var item = db.ExportItems.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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
                db.SaveChanges();
            }
            else
            {
                return false;
            }
            return true;
        }

        #region Added by Steven 2015-02-13
        public List<sp_GetSADynamicReport_Result> GetSADynamicReport(string DateFrom, string DateTo)
        {
            return db.sp_GetSADynamicReport(DateFrom, DateTo).ToList();
        }

        public List<sp_GetDetailSADynamicReport_Result> GetDetailSADynamicReport(string DateFrom, string DateTo, int Type,string ItemDetailID)
        {
            return db.sp_GetDetailSADynamicReport(DateFrom, DateTo, Type, ItemDetailID).ToList();
        }

        public List<sp_GetDataForSAPivotGrid_Result> GetDataForSAPivotGrid(string DateFrom, string DateTo)
        {
            return db.sp_GetDataForSAPivotGrid(DateFrom, DateTo).ToList();
        }

        public List<sp_GetDataForSAPivotGridUseForChart_Result> GetDataForSAPivotGridUseForChart(string DateFrom, string DateTo)
        {
            return db.sp_GetDataForSAPivotGridUseForChart(DateFrom, DateTo).ToList();
        }

        public List<sp_GetSARequestList_Result> GetSARequestList(string OrderCode, DateTime DateFrom, DateTime DateTo)
        {
            return db.sp_GetSARequestList(OrderCode,DateFrom, DateTo).ToList();
        }
        #endregion
        //Added by Tony (2016-10-05)
        #region SA Audit 
        public ObjectParameter SAAudit(int Status)
        {
            ObjectParameter outParam = new ObjectParameter("Msg", typeof(string));
            db.sp_SAAudit(Status, outParam);
            return outParam;
        }
       public object SAAuditStatus()
        {
            object x =new object();
            x = db.SAAuditStatus.Select(i=> new {i.ID}).SingleOrDefault();
            return x;
        }
        #endregion
        //
    }
}
