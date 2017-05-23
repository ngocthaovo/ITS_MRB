using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic
{
    public class HardwareRequirementManager : Base.Connection
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
        /// <summary>
        /// Get item in database
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public HardwareRequirement GetItem(string sItemID)
        {
            return db.HardwareRequirements.Where(i => i.ID == sItemID || i.OrderCode == sItemID).SingleOrDefault();
        }

        /// <summary>
        /// Get List by  Creator, this function using for user
        /// </summary>
        /// <param name="iCreatorID"></param>
        /// <returns></returns>
        public List<HardwareRequirement> GetItems(int iCreatorID)
        {
            return db.HardwareRequirements.Where(i => i.CreatorID == iCreatorID && i.Status != (int)OrderStatus.DELETED).OrderBy(i=>i.CreateDate).ToList();
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
        /// Insert Item: 
        /// DeviceRegistration
        /// if OrderStatus was Save end Send then Insert WFMain to DB
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string InsertItem(HardwareRequirement o, OrderStatus status, WFMain w)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    if (o.ID.Length == 0 || o.ID == null) { o.ID = Guid.NewGuid().ToString(); }
                    if (o.Description == null) o.Description = "";
                    if (o.Reason == null) o.Reason = "";

                    if (o.HardwareRequirementDetails != null)
                    {
                        int sequence = 0;
                        foreach (HardwareRequirementDetail i in o.HardwareRequirementDetails)
                        {
                            sequence += 1;
                            i.ID = Guid.NewGuid().ToString();
                            i.HardwareRequirementID = o.ID;
                            i.ItemNo = GetItemNo(sequence);
                            i.Temp1 = 0;
                        }
                    }
                    db.HardwareRequirements.Add(o);

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
        public bool UpdateItem(HardwareRequirement o, Boolean isSaveDraff, Boolean isReturned, WFMainDetail w, params System.Linq.Expressions.Expression<Func<HardwareRequirement, object>>[] properties)
        {
            if (o.Description == null) o.Description = "";
            if (o.Reason == null) o.Reason = "";
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    HardwareRequirement item = db.HardwareRequirements.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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
                    List<HardwareRequirementDetail> lstDeviceDetail = db.HardwareRequirementDetails.Where(i => i.HardwareRequirementID == item.ID).ToList();
                    if (lstDeviceDetail.Count > 0)
                    {
                        foreach (HardwareRequirementDetail i in lstDeviceDetail)
                            db.HardwareRequirementDetails.Remove(i);
                    }
                    //Add DeviceRegistrationDetails
                    if (o.HardwareRequirementDetails != null)
                    {
                        int sequence =0;
                        foreach (HardwareRequirementDetail i in o.HardwareRequirementDetails)
                        {
                            sequence+=1;
                            i.ID = Guid.NewGuid().ToString();
                            i.HardwareRequirementID = o.ID;
                            i.ItemNo = GetItemNo(sequence);
                            db.HardwareRequirementDetails.Add(i);
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
        /// this function to return the string ItemNo in Eland-system
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private  string GetItemNo(int sequence)
        {
            string sResult = "00000";
            string _sSequence = string.Format("{0}{1}", sequence, "0");
            if (_sSequence.Length > 5)
                return _sSequence;

            sResult = sResult.Substring(_sSequence.Length);
            sResult += _sSequence;
            
            return sResult;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sItemID"></param>
        /// <returns></returns>
        public bool DeleteItem(string sItemID)
        {
            HardwareRequirement item = db.HardwareRequirements.Where(i => i.ID == sItemID).SingleOrDefault();
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


        public int AdminProcessOrder(List<string> IDs, string sProcessor, DateTime dtCompleteEstimateDate)
        {
            List<HardwareRequirement> lstItem = db.HardwareRequirements.Where(i => IDs.Contains(i.ID) || IDs.Contains(i.OrderCode)).ToList();
            foreach (HardwareRequirement item in lstItem)
            {
                item.ProcessedBy = sProcessor;
                item.CompleteEstimateDate = dtCompleteEstimateDate;
            }
            db.SaveChanges();
            return lstItem.Count();
        }



        /// <summary>
        /// User approve Order
        /// </summary>
        /// <param name="IDs"></param>
        /// <param name="iApproverID"></param>
        /// <returns>-1: failed to save Order, -2: failed to calculate stock, -3: failed to send data to eland</returns>
        public int AdminApproveOrder(List<string> IDs, int iApproverID)
        {

            List<HardwareRequirement> lstItem = db.HardwareRequirements.Where(i => IDs.Contains(i.ID) || IDs.Contains(i.OrderCode)).ToList();
            int result = 0;
            foreach (HardwareRequirement item in lstItem)
            {
                bool success = false;
                using (TransactionScope transaction = new TransactionScope())
                {
                    try
                    {
                        
                        item.EstimatedAmount = 0;
                        item.TechnicianID = iApproverID;
                        item.ConfirmDate = DateTime.Now;
                        // Calculate Quantity in stock of (Hardware Requitement)
                        List<HardwareRequirementDetail> lstDetailItems = db.HardwareRequirementDetails.Where(i => i.HardwareRequirementID == item.ID).ToList();
                        if (lstItem.Count > 0)
                        {
                            foreach (HardwareRequirementDetail detail in lstDetailItems)
                            {
                                var iInStock = db.sp_GetEquipmentInventory(detail.ItemDetailID, detail.UnitID).SingleOrDefault();
                                int _iQuanInStock = 0;// data fo demo
                                if (iInStock != null)
                                    _iQuanInStock = iInStock.InventoryQuantity.Value;

                                int _iQuanNeeded = (detail.Quantity.Value > _iQuanInStock) ? (detail.Quantity.Value - _iQuanInStock) : 0;
                                detail.Temp1 = _iQuanNeeded;
                                detail.Temp2 = _iQuanNeeded * detail.EstimatedPrice.Value;

                                item.EstimatedAmount += detail.Temp2;// Calculate total amount
                            }
                        }
                        
                        db.SaveChanges();
                        transaction.Complete();
                        success = true;
                        
                    }
                    catch
                    {
                        transaction.Dispose();
                        success = false;
                        return -1;
                    }
                }

                if(success)
                {
                    using (TransactionScope transaction = new TransactionScope())
                    {
                        try
                        {
                            db.sp_GenerateStockOutEquipment(item.OrderCode, "STOCKOUT", iApproverID);
                            success = true;
                            transaction.Complete();
                        }
                        catch
                        {
                            success = false;
                            transaction.Dispose();

                            return -2;
                        }
                    }

                }

                if(success)
                {
                   // using (TransactionScope transaction = new TransactionScope())
                   // {
                        try
                        {
                            // Sent Order to Eland and calculate quantity in stock
                            db.sp_GetPurchaseOrderFromITS(item.OrderCode, iApproverID);
                            //transaction.Complete();
                            result += 1;
                        }
                        catch
                        {
                            //transaction.Dispose();
                            return -3;
                        }
                   // }
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateStatus(HardwareRequirement o, params System.Linq.Expressions.Expression<Func<HardwareRequirement, object>>[] properties)
        {
            var item = db.HardwareRequirements.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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
        /// This funciton throw when Admin approve the order and put it to Eland
        /// only for the order have items with inventory great than the number in stock
        /// </summary>
        private void ImportDataToELAND()
        {

        }

        /// <summary>
        /// Create By Jason
        /// </summary>
        /// <param name="sMaterialID"></param>
        /// <param name="iCodeCenterCode"></param>
        /// <returns></returns>
        public int CountMaterialByDept(string sMaterialID, int iCodeCenterCode)
        {
            int _result;

            _result = db.StockOutEquipmentDetails.Where(i =>
                                                            (i.StockOutEquipment.Status == (int)OrderStatus.FINSHED)
                                                            && (i.StockOutEquipment.User1.CostCenterCode == iCodeCenterCode) // User1; Deliveryfor
                                                            && (i.ItemDetailID == sMaterialID)
                                                            ).Sum(u => (int?)u.Quantity) ?? 0;

            return _result;
        }
    }
}
