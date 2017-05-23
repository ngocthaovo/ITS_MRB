using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic.WarehouseArea
{
    public class PackingManifestManager: Base.Connection
    {
        public struct CustomerCodeType
        {
            public static string Columbia = "060";
            public static string Fila = "078";
            public static string Nike = "C34";
            public static string UnderAmour = "UA7";
        }

        public enum OrderStatus
        {
            SAVED = 1,
            DELETED = 0,
            New = -1
        }

        public enum ConfirmStatus
        {
            CONFIRMED =1,
            UNCONFIRM = 0
        }

        public enum SockinStatus
        {
            STOCKINED =1,
            UNSCTOCKIN = 0
        }

        /// <summary>
        /// Get items from database
        /// </summary>
        /// <param name="iEnabled"></param>
        /// <returns></returns>
        public PackingManifest GetItem(string sID, int iStatus)
        {
            return db.PackingManifests.Where(i => i.ID == sID || i.CustomerPO == sID).SingleOrDefault();
        }

        public PackingManifest GetItem(string sID)
        {
            return db.PackingManifests.Where(i => i.CustomerPO.Trim() == sID.Trim()).First();
        }


        /// <summary>
        /// Get List by  Creator, this function using for user
        /// </summary>
        /// <param name="iCreatorID"> -1 if get all</param>
        /// <returns></returns>
        public List<PackingManifest> GetItems(int iCreatorID, string CustomerCodeType)
        {
            return db.PackingManifests.Where(i => i.CustomerCode == CustomerCodeType &&
                                                ( (iCreatorID>-1)?i.CreatorID == iCreatorID:true)
                                                && i.STATUS != (int)OrderStatus.DELETED ).OrderByDescending(i => i.CreateDate).Take
                                                (1000).ToList();
        }


        /// <summary>
        /// Insert item with detail to database
        /// </summary>
        /// <param name="o"></param>
        /// <param name="iUserID"></param>
        /// <returns></returns>
        public string InsertItem(PackingManifest o, int iUserID)
        {

           

            // Check for duplicate Serial with CustomerPO
            if(o.PackingManifestDetails !=null && o.PackingManifestDetails.Count() > 0)
            {
                List<long> lstSerialNo = o.PackingManifestDetails.Select(i => i.SerialNo).ToList();

                string sError ="";
                if (!CheckExistedSerial(lstSerialNo,o.CustomerPO,"",out sError))
                    throw new Exception(sError);
            }


            int count = db.PackingManifests.Where(i => i.OrderCode == o.OrderCode && (i.STATUS != (int)OrderStatus.DELETED)).Count(); // check for data existed
            if (count == 0)
            {
                using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(0, 15, 0)))// Transaction time out 15 min
                {
                    try
                    {
                        o.CreatorID = iUserID;
                        o.ID = Guid.NewGuid().ToString();
                        o.STATUS = (int)OrderStatus.SAVED;
                        o.CreateDate = DateTime.Now;
                        o.isConfirm = 0;
                        db.PackingManifests.Add(o);

                        db.SaveChanges();

                        transaction.Complete();
                        return o.ID;
                    }
                    catch (Exception ex)
                    {
                        transaction.Dispose();
                        throw new Exception( ex.Message);
                    }

                }
            }
            else {
                throw new Exception(string.Format("Order code: {0} have been existed in system",o.OrderCode));
            }
            return "";
        }

        public bool UpdateItem(PackingManifest o, params System.Linq.Expressions.Expression<Func<PackingManifest, object>>[] properties)
        {
            var item = db.PackingManifests.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();

            if (item != null)
            {
                List<long> lstSerialNo = o.PackingManifestDetails.Select(i => i.SerialNo).ToList();
                string sError = "";

                //Check validate
                if (!CheckExistedSerial(lstSerialNo, o.CustomerPO,item.OrderCode , out sError))
                    throw new Exception(sError);


                PackingManifestDetail[] detailsTMP = new PackingManifestDetail[o.PackingManifestDetails.Count];
                o.PackingManifestDetails.CopyTo(detailsTMP, 0);

                using (TransactionScope transaction = new TransactionScope())
                {
                    try
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

                        

                        // Xóa PackingManifestDetail củ, trừ ra những trường hợp nào đã stocin or confirms

                        List<PackingManifestDetail> lstPackingManifestDetail = db.PackingManifestDetails.Where(i => i.PackingManifestID == item.ID).ToList();
                        if (lstPackingManifestDetail.Count > 0)
                        {
                            PackingManifestDetailCoordinationManager manager = new PackingManifestDetailCoordinationManager();
                            foreach (PackingManifestDetail i in lstPackingManifestDetail)
                            {
                                if (i.STATUS != (int)OrderStatus.DELETED && i.isCOnfirm != (int)ConfirmStatus.CONFIRMED && i.isStockin != (int)SockinStatus.STOCKINED)
                                {

                                    //Xóa PackingManifestDetailCoordination Củ
                                    int result = manager.DeleteItemByParent(i.ID,db);

                                    // Xóa PackingManifestDetail
                                    db.PackingManifestDetails.Remove(i);
                                }
                            }
                                
                        }

                        // Thêm Device PackingManifestDetail
                        if (detailsTMP != null)
                        {
                            foreach (PackingManifestDetail i in detailsTMP)
                            {
                                i.ID = Guid.NewGuid().ToString();
                                i.PackingManifestID = item.ID;
                                db.PackingManifestDetails.Add(i);
                            }
                        }


                        item.LastUpdateDate = DateTime.Now;
                        db.SaveChanges();
                        transaction.Complete();
                        return true;
                    }
                    catch(Exception ex)
                    {
                        transaction.Dispose();
                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public Boolean ChangeConfirmStatus(string id, ConfirmStatus confirm)
        {
            PackingManifest item = db.PackingManifests.Where(i => i.ID == id || i.OrderCode == id).SingleOrDefault();

            if(item != null)
            {


                if (confirm == ConfirmStatus.CONFIRMED)
                    item.isConfirm = (int) ConfirmStatus.CONFIRMED;
                else if (confirm == (int)ConfirmStatus.UNCONFIRM)
                {
                    int count = item.PackingManifestDetails.ToList().Where(i => (i.isCOnfirm.Value == 1) || (i.isStockin.Value == 1)).Count();
                    if (count > 0) return false;

                    item.isConfirm = (int)ConfirmStatus.UNCONFIRM;
                }

                db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteItem(string sItemID, int iUserID)
        {
            PackingManifest item = db.PackingManifests.Where(i => i.ID == sItemID).SingleOrDefault();
            if (item != null)
            {
                if (item.CreatorID != iUserID)
                {
                    throw new Exception(" You can not delete this item, because it's not belong to you...");
                }


                if (item.isConfirm == (int)ConfirmStatus.CONFIRMED) // Check father
                    return false;
                if(item.PackingManifestDetails != null) // check chidren
                {
                    int count = item.PackingManifestDetails.ToList().Where(i => (i.isCOnfirm.Value == 1)|| (i.isStockin.Value ==1)).Count();
                    if(count > 0) return false;
                }

                item.STATUS = (int)OrderStatus.DELETED;
                item.PackingManifestDetails.ToList().ForEach(i => i.STATUS = (int)OrderStatus.DELETED);

                db.SaveChanges();
                return true;
            }
            return false;
        }

        public int RemoveBarcode(List<string> barcodes, int iUserID, string ObjectScan)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    int result =0 ;

                    List<PackingManifestDetail> lstItems = db.PackingManifestDetails.Where(i =>
                                                                                                (i.isCOnfirm == 0)
                                                                                                &&(i.isStockin == 0)
                                                                                                &&(barcodes.Contains(i.ID))
                                                                                            ).ToList();
                    if(lstItems != null && lstItems.Count > 0)
                    {
                        HistoryScan historyscan;
                        foreach(PackingManifestDetail item in lstItems)
                        {
                            // Update Item Status
                            item.STATUS = (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.OrderStatus.DELETED;

                            // Insert Into HistoryScan
                            historyscan = new HistoryScan();
                            historyscan.ID = Guid.NewGuid().ToString();
                            historyscan.PackingManifestDetailID = item.ID;
                            historyscan.ObjectScan = ObjectScan;
                            historyscan.OperationType = FEA_BusinessLogic.WarehouseArea.HistoryScanManager.OperationType.REMOVED.ToString();
                            historyscan.CreatorID = iUserID;
                            historyscan.CreateDate = DateTime.Now;
                            historyscan.PackingManifestID = item.PackingManifestID;
                            historyscan.SerialNo = item.SerialNo;
                            historyscan.TEMP3 = "";

                            db.HistoryScans.Add(historyscan);

                            result += 1;
                        }
                        db.SaveChanges();
                    }

                    transaction.Complete();

                    return result;
                }
                catch(Exception ex)
                {
                    transaction.Dispose();
                    throw new Exception(ex.Message);
                }
            }
        }



        #region "Common funciton"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstSerialNo"></param>
        /// <param name="sCustomerPO"></param>
        /// <param name="sOrderCode"> enter empty string if you dont want compare without these order code</param>
        /// <param name="sError"></param>
        /// <returns></returns>
        private bool CheckExistedSerial(List<long> lstSerialNo,string sCustomerPO, string sOrderCode, out string sError)
        {
            sError = "";
            List<long> SerialNoDuplicateCount = db.PackingManifestDetails.Where(i => (lstSerialNo.Contains(i.SerialNo))
                                                                              && (i.STATUS != (int)OrderStatus.DELETED)
                                                                              && (i.PackingManifest.CustomerPO == sCustomerPO)
                                                                              && (i.PackingManifest.STATUS != (int)OrderStatus.DELETED)
                                                                              && (sOrderCode.Trim() == ""?true:i.PackingManifest.OrderCode != sOrderCode)
                                                                              ).OrderBy(i=>i.SerialNo).Select(i => i.SerialNo).ToList();

            if (SerialNoDuplicateCount.Count > 0) { 
                sError =  (string.Format("Serial Number {0} with Customer PO {1} was existed in system", string.Join(",", SerialNoDuplicateCount), sCustomerPO));
                return false;
            }

            return true;
        }
        #endregion
    }
}
