using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FEA_BusinessLogic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.Data.Objects;
using System.Data;
namespace FEA_GABusinessLogic
{
    public class GAItemManager : FEA_BusinessLogic.Base.Connection
    {       
       public enum OrderStatus
        {
            DRAFT = 1,
            SENDING = 2,
            CHECKED = 3,
            RETURNED = 4,
            FINSHED = 5,
            DELETED = 0,
            NEW = -1
        }

       public enum OrderType
       {
           Stationery = 1,
           OtherSupplies =2,
           Asset = 3
       }

               /// <summary>
        /// Get item in database
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public GAItem GetItem(string sItemID)
        {
            return db.GAItems.Where(i => i.ID == sItemID || i.OrderCode == sItemID).SingleOrDefault();
        }

        /// <summary>
        /// Get List by  Creator, this function using for user
        /// </summary>
        /// <param name="iCreatorID"></param>
        /// <returns></returns>
        public List<GAItem> GetItems(int iCreatorID)
        {
            return db.GAItems.Where(i => i.CreatorID == iCreatorID && i.Status != (int)OrderStatus.DELETED).OrderBy(i => i.CreateDate).ToList();
        }

        /// <summary>
        /// Insert Item: 
        /// DeviceRegistration
        /// if OrderStatus was Save end Send then Insert WFMain to DB
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string InsertItem(GAItem o, OrderStatus status, WFMain w)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    if (o.ID.Length == 0 || o.ID == null) { o.ID = Guid.NewGuid().ToString(); }
                    if (o.Description == null) o.Description = "";
                    if (o.Reason == null) o.Reason = "";

                    //if (o.CreateDate != null) o.DeliveryDate = o.CreateDate.AddMonths(1); else o.DeliveryDate = DateTime.Now.AddMonths(1);
                    if (o.GAItemDetails != null)
                    {
                        int sequence = 0;
                        foreach (GAItemDetail i in o.GAItemDetails)
                        {
                            sequence += 1;
                            i.ID = Guid.NewGuid().ToString();
                            i.GAItemID = o.ID;
                            i.ItemNO = GetItemNo(sequence);
                            i.Temp1 = "";
                        }
                    }
                    db.GAItems.Add(o);

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
        /// Update Item
        /// </summary>
        /// <param name="o">to Update</param>
        /// <param name="status">OrderStatus</param>
        /// <param name="w">WFMainDetail</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateItem(GAItem o, Boolean isSaveDraff, Boolean isReturned, WFMainDetail w, params System.Linq.Expressions.Expression<Func<GAItem, object>>[] properties)
        {
            if (o.Description == null) o.Description = "";
            if (o.Reason == null) o.Reason = "";
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    GAItem item = db.GAItems.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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
                            item.Status = (int)GAItemManager.OrderStatus.SENDING;

                        // Update  DeviceRegistrationDetail
                    }

                    // Xóa Device Registration Detail
                    List<GAItemDetail> lstDeviceDetail = db.GAItemDetails.Where(i => i.GAItemID == item.ID).ToList();
                    if (lstDeviceDetail.Count > 0)
                    {
                        foreach (GAItemDetail i in lstDeviceDetail)
                            db.GAItemDetails.Remove(i);
                    }
                    //Add DeviceRegistrationDetails
                    if (o.GAItemDetails != null)
                    {
                        int sequence = 0;
                        foreach (GAItemDetail i in o.GAItemDetails)
                        {
                            sequence += 1;
                            i.ID = Guid.NewGuid().ToString();
                            i.GAItemID = o.ID;
                            i.ItemNO = GetItemNo(sequence);
                            db.GAItemDetails.Add(i);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateStatus(GAItem o, params System.Linq.Expressions.Expression<Func<GAItem, object>>[] properties)
        {
            var item = db.GAItems.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public bool DeleteItem(string sItemID)
        {
            GAItem item = db.GAItems.Where(i => i.ID == sItemID).SingleOrDefault();
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
        /// this function to return the string ItemNo in Eland-system
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private string GetItemNo(int sequence)
        {
            string sResult = "00000";
            string _sSequence = string.Format("{0}{1}", sequence, "0");
            if (_sSequence.Length > 5)
                return _sSequence;

            sResult = sResult.Substring(_sSequence.Length);
            sResult += _sSequence;

            return sResult;
        }

        #region GA CHECKING MANAGEMENT FUNCTION FOR ADMIN

        public int GAApproveOrder(List<string> IDs, int iApproverID)
        {
            List<GAItem> lstItem = db.GAItems.Where(i => IDs.Contains(i.ID) || IDs.Contains(i.OrderCode)).ToList();
            foreach (GAItem item in lstItem)
            {
                item.Status = (int)OrderStatus.FINSHED;
                item.ConfirmDate = DateTime.Now;
                item.TechnicianID = iApproverID;
            }
            db.SaveChanges();
            return lstItem.Count();
        }




        /// <summary>
        /// This function only for GA Checking Approval
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<sp_GetApproveDocumentForGADepartment_Result> GetItems(OrderStatus status, int UserID)
        {
            return db.sp_GetApproveDocumentForGADepartment((int)status, UserID).ToList();
        }

        /// <summary>
        /// This function only for Admin
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public int CountItems(int UserID)
        {

            return db.sp_GetApproveDocumentForGADepartment((int)OrderStatus.CHECKED, UserID).ToList().Count();
        }

        /// <summary>
        /// This function only for GA Finished Checking Approval
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public object GetFinishedDocument(int UserID)
        {
            return (object)db.sp_GetApproveDocumentForGADepartment(5, UserID).ToList();
        }

        public object GetDetailFinishDocument(string ID, string DocumentTypeName)
        {
            object x = new object();
            x = db.GAItemDetails.Where(i => i.GAItemID == ID).Select(i => new { i.GAItem.OrderCode, i.Item.ItemName, i.ItemDetail.ItemDetailName, i.Quantity, i.Temp1, i.EstimatePrice, i.EstimateAmount, i.Description }).ToList();
            return x;
        }
 

        #endregion

        /// <summary>
        /// Check this itemID is damged or not
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public bool IsDamgedChecking(string itemDetailID, int costCenterCode)
        {
            var item = db.GAAdjustments.Where(i => i.ItemDetail == itemDetailID && 
                                                   i.CostCenterCode == costCenterCode && 
                                                   i.Month.Month == DateTime.Now.Month && 
                                                   i.Month.Year == DateTime.Now.Year
                                                   &&i.Damged == 1
                                                   &&i.Status ==1).SingleOrDefault();
            if (item != null)
                return false;
            return true;
        }



        ///Get Ga request list, using for query function
        /// <summary>
        /// Get Ga request list, using for query function
        /// </summary>
        /// <param name="OrderCode"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <returns></returns>
        public List<sp_GetGARequestList_Result> GetGARequestList(string OrderCode, DateTime DateFrom, DateTime DateTo)
        {
            return db.sp_GetGARequestList(OrderCode, DateFrom, DateTo).ToList();
        }
        // Add by Tony (2016-09-17)
        #region GA Project 2
        public ObjectParameter GAPushDataToEland(string OrderCode, string Month, int UserID)
        {
            ObjectParameter outParam=new ObjectParameter("Msg",typeof(string));
            db.sp_PushGADataToEland(OrderCode, Month, UserID, outParam);
            return outParam;
        }
        public List<sp_GetGAPushDataList_Result> GetGaPushDataList(string OrderCode, DateTime DateFrom, DateTime DateTo, int Status)
        {
            return db.sp_GetGAPushDataList(OrderCode, DateFrom, DateTo, Status).ToList();
        }
        public object GetGAPushDataDetail(string ID)
        {
            object x=new object();
            x = db.GAItemDetails.Where(i => i.GAItemID == ID).Select(i => new { i.GAItem.OrderCode, i.Item.ItemName, i.ItemDetail.ItemDetailName, i.Quantity, i.Temp1, i.EstimatePrice, i.EstimateAmount, i.Description }).ToList();
            return x;
        }
        #endregion
        //Tony
    }

}
