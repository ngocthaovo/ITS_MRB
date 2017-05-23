using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class WFRefferenceManager : Base.Connection
    {
        public List<WFRefference> GetItems(string sOrderCode)
        {
            return db.WFRefferences.Where(i => i.OrderCode == sOrderCode).OrderByDescending(o=>o.CreateDate).ToList();
        }

        public WFRefference GetItem(string sID)
        {
            return db.WFRefferences.Where(i => i.ID == sID).SingleOrDefault();
        }

        public WFRefference GetItem(string OrderCode, string WFMainDetailID, int SenderID)
        {
            return db.WFRefferences.Where(i => (i.OrderCode == OrderCode) && (i.SenderID == SenderID) && (i.MainDetailID == WFMainDetailID)).SingleOrDefault();
        }

        public string GetReferencePersonList(string OrderCode)
        {
            return db.sp_GetReferencePersonList(OrderCode).FirstOrDefault();
        }

        public bool InsertItem(WFRefference o)
        {
            o.ID = Guid.NewGuid().ToString();
            o.CreateDate = DateTime.Now;

            if (o.Temp1 == null) o.Temp1 = "";
            if (o.Temp2 == null) o.Temp2 = "";
            if (o.Temp3 == null) o.Temp3 = "";

            db.WFRefferences.Add(o);
            db.SaveChanges();
            return true;
        }

        public bool UpdateItem(WFRefference o, params System.Linq.Expressions.Expression<Func<WFRefference, object>>[] properties)
        {
            var item = db.WFRefferences.Where(i => i.ID == o.ID).SingleOrDefault();
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
