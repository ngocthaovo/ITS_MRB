using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic
{
    public class WFMainDetailManager : Base.Connection
    {
        public bool InsertItem(WFMainDetail o)
        {
            db.WFMainDetails.Add(o);
            db.SaveChanges();
            return true;
        }

        public bool UpdateItem(WFMainDetail o, params System.Linq.Expressions.Expression<Func<WFMainDetail, object>>[] properties)
        {
            var item = db.WFMainDetails.Where(i => i.MainDetailID == o.MainDetailID).SingleOrDefault();
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
        //Added by Tony (2017-06-08) Cập nhật giá trị của cột temp3 về 1 khi có một đơn bị trả về
        public bool UpdateWFDetail(string mainID)
        {
            List<WFMainDetail> lstItem = db.WFMainDetails.Where(i => i.MainID == mainID).ToList();
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    foreach (WFMainDetail item in lstItem)
                    {
                        item.Temp3 = "1";
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
        /// Check for User was signed on Order
        /// </summary>
        /// <param name="NextApproverID"></param>
        /// <param name="MainDetailID"></param>
        /// <param name="MainID"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public bool IsDuplicateWFMainDetail(int NextApproverID, string MainID, int UserID)
        {

            int wfdt = db.WFMainDetails.Where(i =>
                                                            (i.CheckUserID == NextApproverID)
                                                            &&(i.PostUserID == UserID)
                                                            &&(i.MainID == MainID)
                                                            &&(i.isFinished == 0)).Count();
            if (wfdt > 0)
                return true;
            
            return false;
        }
    }
}
