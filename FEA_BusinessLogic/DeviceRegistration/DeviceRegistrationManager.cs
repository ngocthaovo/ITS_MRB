using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic
{
    public class DeviceRegistrationManager : Base.Connection
    {

        public enum OrderStatus
        {
            DRAFT = 1,
            SENDING = 2,
            CHECKED = 3,
            RETURNED = 4,
            FINSHED = 5,
            DELETED = 0,
            // Add by Tony (2016-09-19)
            PUSHDATA = 7,
            //
            New = -1
        }

        public struct OrderType
        {
            public static string SOFTWARE = "software";
            public static string HARDWARE = "hardware";
            
        }

        /// <summary>
        /// Get item in database
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public DeviceRegistration GetItem(string sItemID)
        {
            return db.DeviceRegistrations.Where(i => i.ID == sItemID || i.OrderCode == sItemID).SingleOrDefault();
        }

        public object GetFinishedDocument(int ManagerID)
        {
            return (object)db.sp_GetApproveDocumentForAdmin(ManagerID.ToString(), 5).ToList();
        }


        /// <summary>
        /// Get List by  Creator, this function using for user
        /// </summary>
        /// <param name="iCreatorID"></param>
        /// <returns></returns>
        public List<DeviceRegistration> GetItems(int iCreatorID)
        {
            return db.DeviceRegistrations.Where(i => i.CreatorID == iCreatorID && i.Status != (int)OrderStatus.DELETED).OrderBy(i=>i.CreateDate).ToList();
        }

        /// <summary>
        /// This function only for Admin
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<sp_GetApproveDocumentForAdmin_Result> GetItems(string sUserID, OrderStatus status)
        {
            if (sUserID == "" || sUserID.Trim().Length == 0) sUserID = "%%";
            return db.sp_GetApproveDocumentForAdmin(sUserID, (int)status).ToList(); //db.DeviceRegistrations.Where(i => i.Status == (int)status).ToList();
        }

        /// <summary>
        /// This function only for Admin
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public int CountItems(string sUserID, OrderStatus status)
        {
            if (sUserID == "" || sUserID.Trim().Length == 0) sUserID = "%%";
            return db.sp_GetApproveDocumentForAdmin(sUserID, (int)status).ToList().Count(); //db.DeviceRegistrations.Where(i => i.Status == (int)status).ToList();
        }
        /// <summary>
        ///  GetApprover
        /// </summary>
        /// <param name="sDocTypeName"></param>
        /// <param name="iCodeCenterCode"></param>
        /// <param name="sNodeID"></param>
        /// <returns></returns>
        public List<sp_GetApprover_Result> GetApprover(string sDocTypeName, int iCodeCenterCode, string sNodeID, string sOrderCode, int UserID)
        {
            return db.sp_GetApprover(sDocTypeName, iCodeCenterCode, sNodeID, sOrderCode,UserID).ToList();
        }
        /// <summary>
        /// Insert Item: 
        /// DeviceRegistration
        /// if OrderStatus was Save end Send then Insert WFMain to DB
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string InsertItem(DeviceRegistration o, OrderStatus status, WFMain w)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    if (o.ID.Length == 0 || o.ID == null) { o.ID = Guid.NewGuid().ToString(); }
                    if (o.Description == null) o.Description = "";
                    if (o.Reason == null) o.Reason = "";
                    if (o.DeviceRegistrationDetails != null)
                    {
                        foreach (DeviceRegistrationDetail i in o.DeviceRegistrationDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                            i.DeviceRegistrationID = o.ID;
                            i.Temp1 = "";
                        }
                    }
                    db.DeviceRegistrations.Add(o);

                    // Check for sending?
                    if (status == OrderStatus.SENDING)
                    {
                        db.WFMains.Add(w);
                    }

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
        public bool DeleteItem(string sItemID)
        {
            DeviceRegistration item = db.DeviceRegistrations.Where(i => i.ID == sItemID).SingleOrDefault();
            if (item != null)
            {
                if (item.Status == (int)OrderStatus.CHECKED
                   || item.Status == (int)OrderStatus.SENDING
                   || item.Status == (int)OrderStatus.FINSHED)
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
        /// Update Item
        /// </summary>
        /// <param name="o">DeviceRegistration to Update</param>
        /// <param name="status">OrderStatus</param>
        /// <param name="w">WFMainDetail</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateItem(DeviceRegistration o, Boolean isSaveDraff, Boolean isReturned, WFMainDetail w, params System.Linq.Expressions.Expression<Func<DeviceRegistration, object>>[] properties)
        {
            if (o.Description == null) o.Description = "";
            if (o.Reason == null) o.Reason = "";

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    DeviceRegistration item = db.DeviceRegistrations.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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
                        if (!isSaveDraff)
                            item.Status = (int)DeviceRegistrationManager.OrderStatus.SENDING;

                        // Update  DeviceRegistrationDetail
                    }

                    // Xóa Device Registration Detail
                    List<DeviceRegistrationDetail> lstDeviceDetail = db.DeviceRegistrationDetails.Where(i => i.DeviceRegistrationID == item.ID).ToList();
                    if (lstDeviceDetail.Count > 0)
                    {
                        foreach (DeviceRegistrationDetail i in lstDeviceDetail)
                            db.DeviceRegistrationDetails.Remove(i);
                    }
                    //Add DeviceRegistrationDetails
                    if (o.DeviceRegistrationDetails != null)
                    {
                        foreach (DeviceRegistrationDetail i in o.DeviceRegistrationDetails)
                        {
                            i.ID = Guid.NewGuid().ToString();
                            i.DeviceRegistrationID = o.ID;
                            db.DeviceRegistrationDetails.Add(i);
                        }
                    }


                    if (!isSaveDraff)
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

        public bool UpdateStatus(DeviceRegistration o, params System.Linq.Expressions.Expression<Func<DeviceRegistration, object>>[] properties)
        {
            var item = db.DeviceRegistrations.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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

        public int AdminApproveOrder(List<string> IDs, int iApproverID)
        {
            List<DeviceRegistration> lstItem = db.DeviceRegistrations.Where(i => IDs.Contains(i.ID) || IDs.Contains(i.OrderCode)).ToList();
            foreach (DeviceRegistration item in lstItem)
            {
                item.Status = (int)OrderStatus.FINSHED;
                item.CompleteDate = DateTime.Now;
                item.TechnicianID = iApproverID;
                UpdateERPSalesOrderStatus(item.OrderCode);  //-- Added by Steven 2016-07-14
            }
            db.SaveChanges();
            return lstItem.Count();
        }

        /// <summary>
        /// Set Processor for admin user
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="sProcessor"></param>
        /// <returns></returns>
        public int AdminProcessOrder(List<string> IDs, string sProcessor, DateTime dtCompleteEstimateDate)
        {
            List<DeviceRegistration> lstItem = db.DeviceRegistrations.Where(i => IDs.Contains(i.ID) || IDs.Contains(i.OrderCode)).ToList();
            foreach (DeviceRegistration item in lstItem)
            {
                item.ProcessedBy = sProcessor;
                item.CompleteEstimateDate = dtCompleteEstimateDate;
            }
            db.SaveChanges();
            return lstItem.Count();
        }

        //Added by Tony(2017-06-05) 
        public List<DeviceRegistration> GetListItemToProcess(List<string> IDs)
        {
            List<DeviceRegistration> lstItem = db.DeviceRegistrations.Where(i => IDs.Contains(i.ID) || IDs.Contains(i.OrderCode)).ToList();
            return lstItem;
        }

        /// <summary>
        /// Add by Jason - Get All Order  in System(except:deleted)
        /// </summary>
        /// <returns></returns>
        public List<sp_GetOrderRequest_Result> GetOrderRequested(string sOrderCode, DateTime BeginDate, DateTime EndDate)
        {
            return db.sp_GetOrderRequest(sOrderCode, BeginDate, EndDate).OrderByDescending(i=>i.CreateDate).ToList();
        }
        
        public bool CheckFEPO(string FEPO)
        {
            var t = Task<vw_GetOrderFromERP>.Factory.StartNew(() =>
            {
                return db.vw_GetOrderFromERP.Where(i => i.FEPOCode == FEPO).SingleOrDefault();
            });
            t.Wait();
            if (t.Result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
            return true;
        }

        public void UpdateERPSalesOrderStatus(string OrderCode)
        {
            db.sp_UpdateERPAllowCancel(OrderCode);
        }
    }
}
