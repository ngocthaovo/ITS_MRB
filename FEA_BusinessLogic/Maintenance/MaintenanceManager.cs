using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Linq.Expressions;
namespace FEA_BusinessLogic.Maintenance
{
    public class MaintenanceManager: Base.Connection
    {
        public enum OrderStatus
        {
            DRAFT=1,
            SENDING=2,
            CHECKED=3,
            RETURNED=4,
            FINSHED=5,
            DELETED=0,
            NEW=-1
        }

        public enum OrderType
        {
            StockIn=1,
            StockOut=2
        }

        #region Get data
        public List<sp_GetMaintenanceApproveDocument_Result> GetDocumentForApprove(int Status, int UserID, string strDocType)
        {
            return db.sp_GetMaintenanceApproveDocument(Status, UserID,strDocType).ToList();
        }

        public MNRequestMain GetRequest(string sRequestID)
        {
            return db.MNRequestMains.Where(i => i.ID == sRequestID || i.OrderCode == sRequestID).SingleOrDefault();
        }

        public List<MNRequestMain> GetRequestByUser(int inUserID, string strType)
        {
            return db.MNRequestMains.Where(i => (i.CreatorID == inUserID) && (i.DocType==strType)).OrderByDescending(i=>i.OrderCode).ToList();
        }

        public List<MNRequestMain> GetRequestForStockIn(string strDocType)
        {
           return db.MNRequestMains.Where(i => (i.Status == 5) && (i.DocType == strDocType.ToUpper())).OrderBy(x=>x.ConfirmDate).ToList();
        }
        public List<sp_GetMaintenanceDetailsQuantity_Result> GetDetailListQuantity(string ID)
        {
            return db.sp_GetMaintenanceDetailsQuantity(ID).ToList();
        }
        public List<sp_GetMNRequestList_Result> GetMNRequestList(string OrderCode, string DateFrom, string DateTo, string DocumentType)
        {
            return db.sp_GetMNRequestList(OrderCode, DateFrom, DateTo, DocumentType).ToList();
        }

        public List<sp_GetMNSummaryReport_Result> GetMNSummaryReport(string DateFrom, string DateTo, string DocumentType)
        {
            return db.sp_GetMNSummaryReport(DateFrom, DateTo, DocumentType).ToList();
        }

        public List<MNRequestMain> GetConfirmedRequest(int userID,string strType)
        {
            return db.MNRequestMains.Where(i => (i.ConfirmID == userID) && (i.Status == 4 || i.Status == 5) && (i.DocType==strType)).OrderByDescending(i => i.ConfirmDate).ToList();
        }

        public List<MNRequestMainDetail> GetConfirmedRequestDetails(string ID)
        {
            return db.MNRequestMainDetails.Where(i => i.RequestMainID == ID).ToList();
        }
        #endregion

        #region Handle data
        public string InsertItem(MNRequestMain o,OrderStatus status,WFMain w)
        {
            using (TransactionScope transaction=new TransactionScope())
            {
                try
                {
                    if (o.ID.Length == 0 || o.ID == null)
                        o.ID = Guid.NewGuid().ToString();
                    if (o.Description == null) o.Description = "";
                    if (o.Reason == null) o.Reason = "";
                    if(o.MNRequestMainDetails !=null)
                    {
                        int sequence = 0;
                        foreach (MNRequestMainDetail i in o.MNRequestMainDetails)
                        {
                            sequence += 1;
                            i.DetailID = Guid.NewGuid().ToString();
                            i.RequestMainID = o.ID;
                            i.ItemNo = Convert.ToInt16( GetItemNo(sequence));
                            i.PurchaseQuantity = 0;
                            i.PurchaseEstimateAmount = 0;
                        }
                    }
                    db.MNRequestMains.Add(o);
                    if (status == OrderStatus.SENDING)
                        db.WFMains.Add(w);
                    db.SaveChanges();
                    transaction.Complete();
                    return o.ID;
                }
                catch(Exception)
                {
                    transaction.Dispose();
                }
            }
            return "";
        }
       
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
       
        public bool UpdateItem(MNRequestMain o, Boolean isSaveDraft,Boolean isReturned,WFMainDetail w, params System.Linq.Expressions.Expression<Func<MNRequestMain,object>>[] properties)
        {
            if (o.Description == null) o.Description = "";
            if (o.Reason == null) o.Reason = "";
            using (TransactionScope transaction =new TransactionScope())
            {
                try
                {
                    MNRequestMain item = db.MNRequestMains.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
                    if(item !=null)
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
                        if (!isSaveDraft)
                            item.Status = (int)OrderStatus.SENDING;
                    }

                    List<MNRequestMainDetail> lstRequestDetail = db.MNRequestMainDetails.Where(i => i.RequestMainID == item.ID).ToList();
                    if(lstRequestDetail.Count >0)
                    {
                        foreach (MNRequestMainDetail i in lstRequestDetail)
                            db.MNRequestMainDetails.Remove(i);
                    }

                    if(o.MNRequestMainDetails!=null)
                    {
                        int sequence =0;
                        foreach (MNRequestMainDetail i in o.MNRequestMainDetails)
                        {
                            sequence +=1;
                            i.DetailID=Guid.NewGuid().ToString();
                            i.RequestMainID =o.ID;
                            i.ItemNo = Convert.ToInt16( GetItemNo(sequence));
                            db.MNRequestMainDetails.Add(i);
                        }
                    }
                    if(!isSaveDraft)
                    {
                        if(isReturned)
                        {
                            WFMainDetail wfDetail = db.WFMainDetails.Where(i => i.WFMain.OrderCode == o.OrderCode && i.isFinished == 0 && i.NodeID == "").SingleOrDefault();
                            if(wfDetail !=null)
                            {
                                wfDetail.isFinished = 1;
                                wfDetail.CheckDate = DateTime.Now;
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
        
        public bool UpdateStatus(MNRequestMain o, params System.Linq.Expressions.Expression<Func<MNRequestMain, object>>[] properties)
        {
            var item = db.MNRequestMains.Where(i => i.ID == o.ID || i.OrderCode == o.OrderCode).SingleOrDefault();
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
       
        public bool DeleteRequest(string sRequestID)
        {

            MNRequestMain item = db.MNRequestMains.Where(i => i.ID == sRequestID).SingleOrDefault();
            if(item !=null)
            {
                if (item.Status == (int)OrderStatus.CHECKED
                    || item.Status == (int)OrderStatus.SENDING
                    || item.Status == (int)OrderStatus.FINSHED)
                    return false;
                if(item.Status==(int)OrderStatus.RETURNED)
                {
                    WFMainDetail wfDtail = db.WFMainDetails.Where(i => i.WFMain.OrderCode == item.OrderCode && i.isFinished == 0 && i.NodeID == "").SingleOrDefault();
                    if(wfDtail !=null)
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
        #endregion
    }
}
