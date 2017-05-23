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
            var aa = db.GAAdjustments.Where(i => i.Status != -1).Select(i => new { ID = i.CostCenterCode, Name = i.CostCenter.Remark }).Distinct().ToDictionary(a => a.ID, a => a.Name);
            return aa;
        }

        /// <summary>
        /// GetItemDetail List using to filter
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetItemDetail(int costCenterCode)
        {
            var bbb = db.GAAdjustments.Where(i => i.CostCenterCode == costCenterCode && i.Status != -1).Select
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
            return db.GAAdjustments.Where(i => i.Status!=Status).ToList();
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
              
                    int month = o.Month.Month;
                    //Check duplicate
                    var item = db.GAAdjustments.Where(i => (i.CostCenterCode == o.CostCenterCode) && (i.ItemDetail == o.ItemDetail) && (i.Month.Month==month)).SingleOrDefault();
                    if (item != null)
                        return -1;
                    

                    o.ID = Guid.NewGuid().ToString();
                    o.CreateTime = DateTime.Now;
                    if (o.Status == null) o.Status = 0;
                    if (o.Temp1 == null) o.Temp1 = "";
                    if (o.Temp2 == null) o.Temp2 = "";
                    if (o.Damged == null) o.Damged = 0;
                    if (o.Reason == null) o.Reason = "";
                    if (o.AdjustAmount == null) o.AdjustAmount = 0;
                    
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
        /// Copy GAAdjustments to database
        /// </summary>
        /// <param name="o"></param>
        /// <returns>1: true; 0: false, -1: dubplicate Type</returns>
        public int CopyGAAdjustments( DateTime EndDate, DateTime CopyMonth)
        {

            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    db.sp_CopyGAAdjustment(CopyMonth, DateTime.Now, EndDate);
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
                int month = item.Month.Month;
                int year = item.Month.Year;
                //var gaItemDetail = db.GAItemDetails.Where(i => i.ItemDetailID == item.ItemDetail).Select(i => new { ID = db.GAItems.Where(a => a.ID == i.GAItemID && a.CreateDate.Month == month).FirstOrDefault() });
                GAItemDetail itemDetail = db.GAItemDetails.Where(i => i.ItemDetailID == item.ItemDetail
                                                    && i.GAItem.CreateDate.Month == month
                                                    && i.GAItem.CreateDate.Year == year
                                                    && i.GAItem.Status != (int)GAItemManager.OrderStatus.DELETED).FirstOrDefault();
                if (itemDetail != null)
                {
                        return false;
                }
                else
                {
                    db.GAAdjustments.Remove(item);
                    db.SaveChanges();
                    return true;
                }
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
            int year = o.Month.Year;

            //var gaItemDetail = db.GAItemDetails.Where(i => i.ItemDetailID == o.ItemDetail).Select(i => 
            //    new { ID = db.GAItems.Where(a => a.ID == i.GAItemID && a.CreateDate.Month==month && a.CreateDate.Year==year && a.User.CostCenterCode==item.CostCenterCode).FirstOrDefault()});

            var gaItemDetail = db.GAItemDetails.Where(i => i.ItemDetailID == item.ItemDetail 
                                                        && i.GAItem.DeliveryDate.Value.Month == month
                                                        && i.GAItem.DeliveryDate.Value.Month == year
                                                        && i.GAItem.User.CostCenterCode == item.CostCenterCode).FirstOrDefault();
            if (item != null)
            {

                if (gaItemDetail== null)
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


                // Lay danh sach tat ca item co trong bien thuoc thang hien tai
                List<GAAdjustment> lst = db.GAAdjustments.Where(i => i.ItemDetail == item.ItemDetail
                                                                    && i.Month.Month == item.Month.Month
                                                                    && i.Month.Year == item.Month.Year).ToList();
                // Cap nhat lai gia
                lst.ForEach(i => i.AdjustAmount = o.AdjustAmount);

                db.SaveChanges();

                return true;
            }

            else
            {
                return false;
            }
            return true;
        }


        /// <summary>
        ///  Get All GAAdjustment by month  in System(except:deleted)
        /// </summary>
        /// <returns></returns>
        public List<GAAdjustment> GetGAAdjustment(DateTime EndDate)
        {
            return db.GAAdjustments.Where(i => (i.Status != -1 && i.Month.Month == EndDate.Month && i.Month.Year==EndDate.Year)).ToList();
        }



        public GAAdjustment GetItem(string ItemDetailID, int CostCenterCost)
        {
            int month = DateTime.Now.AddMonths(1).Month;
            int year = DateTime.Now.AddMonths(1).Year;
            return db.GAAdjustments.Where(i => i.Status != -1
                                          && i.Month.Month == month
                                          && i.Month.Year == year
                                          && i.CostCenterCode == CostCenterCost
                                          && i.ItemDetail == ItemDetailID).FirstOrDefault();

        }
    }

}
