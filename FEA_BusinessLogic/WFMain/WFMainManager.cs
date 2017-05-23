using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions; 

namespace FEA_BusinessLogic
{
    public class WFMainManager : Base.Connection
    {
        /// <summary>
        /// sID can be: MainID or OrderCode
        /// </summary>
        /// <param name="sID"></param>
        /// <returns></returns>
        public WFMain GetItem(string sID)
        {
            return db.WFMains.Where(i => i.MainID == sID || i.OrderCode == sID).SingleOrDefault();
        }


        public bool InsertItem(WFMain o)
        {
            db.WFMains.Add(o);
            db.SaveChanges();
            return true;
        }

        public bool UpdateItem(WFMain o, params System.Linq.Expressions.Expression<Func<WFMain, object>>[] properties)
        {
            var item = db.WFMains.Where(i => i.OrderCode == o.OrderCode).SingleOrDefault();
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
