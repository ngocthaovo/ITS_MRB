using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using FEA_BusinessLogic;

namespace FEA_GABusinessLogic
{
    public class GAAdjustmentItemManager : FEA_BusinessLogic.Base.Connection
    {

        /// <summary>
        /// GetCostCenter List using to filter
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Dictionary<int?, string> GetCostCenter()
        {
            var aa = db.GAAdjustments.Where(i => i.Status == 1).Select(i => new { ID = i.CostCenterCode, Name = i.CostCenter.Remark }).Distinct().ToDictionary(a => a.ID, a => a.Name);
            return aa;
        }

        /// <summary>
        /// GetItemDetail List using to filter
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetItemDetail(int costCenterCode)
        {
            var bbb = db.GAAdjustments.Where(i => i.CostCenterCode == costCenterCode && i.Status == 1).Select
                (i => new { ID = i.ItemDetail, Name = db.ItemDetails.Where(a => a.ID == i.ItemDetail).FirstOrDefault().ItemDetailName }).Distinct().ToDictionary(b => b.ID, b => b.Name);
            return bbb;
        }


        /// <summary>
        /// Get GAAdjustment Item List and bind to datagridview
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public List<GAAdjustment> GetItems(int? Status = -1)
        {
            return db.GAAdjustments.Where(i => (Status > -1 ? i.Status == Status : true)).ToList();
        }


        /// <summary>
        /// Insert GAAdjustments with GAAdjustments to database
        /// </summary>
        /// <param name="o"></param>
        /// <returns>1: true; 0: false, -1: dubplicate Type</returns>
        public int InsertGAAdjustments(GAAdjustment o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    //Check duplicate
                    var item = db.GAAdjustments.Where(i => (i.CostCenterCode == o.CostCenterCode) && (i.ItemDetail == o.ItemDetail)).SingleOrDefault();
                    if (item != null)
                        return -1;


                    o.ID = Guid.NewGuid().ToString();
                    if (o.Status == null) o.Status = 0;
                    if (o.Temp1 == null) o.Temp1 = "";
                    if (o.Temp2 == null) o.Temp2 = "";

                    db.GAAdjustments.Add(o);
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

        /// <summary>
        /// Delete GAAdjustments with  ID
        /// </summary>
        /// <param name="o"></param>
        /// </returns>

        public bool DeleteGAAdjustment(string sID)
        {
            GAAdjustment item = db.GAAdjustments.Where(i => i.ID == sID).SingleOrDefault();

            if (item != null)
            {
                var temp = db.GAAdjustments.Where(i => i.CostCenterCode == item.CostCenterCode && i.ItemDetail==item.ItemDetail).FirstOrDefault();
                if (temp != null)
                    return false;

                db.GAAdjustments.Remove(item);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update GAAdjustments with  ID
        /// </summary>
        /// <param name="o"></param>
        /// </returns>
        public bool UpdateItem(GAAdjustment o, params System.Linq.Expressions.Expression<Func<GAAdjustment, object>>[] properties)
        {
            var item = db.GAAdjustments.Where(i => i.ID == o.ID).SingleOrDefault();
            int month = o.Month.Month;
            var gaItemDetail = db.GAItemDetails.Where(i => i.ItemDetailID == o.ItemDetail).Select(i => new { ID = db.GAItems.Where(a => a.ID == i.GAItemID && a.CreateDate.Month==month).FirstOrDefault()});

            if (item != null && gaItemDetail !=null)
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
