using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FEA_BusinessLogic;
namespace FEA_SABusinessLogic
{
  public  class SAExportApprovalItemManager:FEA_BusinessLogic.Base.Connection
    {

        public Dictionary<int, string> GetCostCenter()
        {
            var aa = db.ExportItemApproverItems.Where(i =>i.Status == 1).Select(i => new { ID = i.CostCenterCode, Name = i.CostCenter.Remark }).Distinct().ToDictionary(a => a.ID, a => a.Name);
            return aa;
        }

        public Dictionary<string, string> GetItemType(int costCenterCode)
        { 
            var aaa = db.ExportItemApproverItems.Where(i => i.CostCenterCode == costCenterCode && i.Status==1).Select(i => new { ID = i.ItemID, Name = i.Item.ItemName }).
                Distinct().ToDictionary(a => a.ID, a => a.Name);
            return aaa;
        }

        public Dictionary<string, string> GetItemDetail(int costCenterCode, string idType)
        {
            var bbb = db.ExportItemApproverItems.Where(i => i.CostCenterCode == costCenterCode && i.ItemID == idType && i.Status==1).Select
                (i => new { ID = i.ItemDetailID, Name = db.ItemDetails.Where(a=>a.ID == i.ItemDetailID).FirstOrDefault().ItemDetailName}).
                Distinct().ToDictionary(b => b.ID, b => b.Name);
            return bbb;
        }



        /// <summary>
        /// Update ItemID's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItemID(ExportItemApproverItem o, params System.Linq.Expressions.Expression<Func<ExportItemApproverItem, object>>[] properties)
        {
            var item = db.Items.Where(i => i.ID == o.ItemID).SingleOrDefault();
            var exportitemDetail = db.ExportItemDetails.Where(i => i.ItemID == o.ItemID).SingleOrDefault(); // so sánh với bảng ExportItemDetail, nếu tồn tài itemId này thì không được update
            if (item != null && exportitemDetail == null)
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
        /// Update ItemDetailID's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItemDetailID(ExportItemApproverItem o, params System.Linq.Expressions.Expression<Func<ExportItemApproverItem, object>>[] properties)
        {
            var item = db.ItemDetails.Where(i => i.ID == o.ItemDetailID).SingleOrDefault();
            var exportitemDetail = db.ExportItemDetails.Where(i => i.ItemDetailID == o.ItemDetailID).SingleOrDefault();// so sánh với bảng ExportItemDetail, nếu tồn tài itemDetailID này thì không được update
            if (item != null && exportitemDetail==null)
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
        /// Insert ExportItemApproverItem with ExportItemApproverItem to database
        /// </summary>
        /// <param name="o"></param>
        /// <returns>1: true; 0: false, -1: dubplicate Type</returns>
        public int InsertExportItemApproverItem(ExportItemApproverItem o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //Check duplicate
                    var item = db.ExportItemApproverItems.Where(i =>(i.CostCenterCode == o.CostCenterCode) && (i.ItemID == o.ItemID) && (i.ItemDetailID == o.ItemDetailID)).SingleOrDefault();
                    if (item != null)
                        return -1;


                    o.ID = Guid.NewGuid().ToString();
                    if (o.Status == null) o.Status = 0;
                    if (o.Temp1 == null) o.Temp1 = "";
                    if (o.Temp2 == null) o.Temp2 = "";

                    db.ExportItemApproverItems.Add(o);
                    db.SaveChanges();
                    transaction.Complete();
                    return 1;
                }
                catch (Exception ex)
                {
                    transaction.Dispose();
                    return 0;
                }
            }
        }

        public bool DeleteExportItemApproverItem(string sID)
        {
            ExportItemApproverItem item = db.ExportItemApproverItems.Where(i => i.ID == sID).SingleOrDefault();
          
            if (item != null)
            {
                var temp = db.ExportItemDetails.Where(i => i.CostCenterCode == item.CostCenterCode && i.ItemID == item.ItemID && i.ItemDetailID == item.ItemDetailID).FirstOrDefault();
                if (temp!=null)
                    return false;

                db.ExportItemApproverItems.Remove(item);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        public List<ExportItemApproverItem> GetItems(int? Status=-1)
        {
            return db.ExportItemApproverItems.Where(i => (Status > -1 ? i.Status == Status : true)).ToList();
        }


        public bool UpdateItem(ExportItemApproverItem o, params System.Linq.Expressions.Expression<Func<ExportItemApproverItem, object>>[] properties)
        {
            var item = db.ExportItemApproverItems.Where(i => i.ID == o.ID).SingleOrDefault();
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

    }
}
