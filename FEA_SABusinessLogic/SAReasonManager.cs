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
    public  class SAReasonManager:FEA_BusinessLogic.Base.Connection
    {
        //Temp1:
            //	1 – Normal
            //	2 – Lend
            //	3 – Borrow
            //	4 – Donative
            //	5 – Adjust

        public List<SAReason> GetItems(int? status=-1)
        {
            return db.SAReasons.Where(i => (status > -1 ? i.Status == status : true)).ToList();
        }

        public List<SAReason> GetItemsByType(int type)
        {
            string temp1;
            temp1 = Convert.ToString(type);
            return db.SAReasons.Where(i => i.Temp1 == temp1 && i.Status==1).ToList();
        }

        public List<SAReason> GetItemsByID(string ReasonID)
        {
            return db.SAReasons.Where(i => i.ID == ReasonID && i.Status==1).ToList();
        }

        public int InsertItem(SAReason o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    o.ID = Guid.NewGuid().ToString();
                    if (o.Status == null) o.Status = 0;
                    if (o.isConstraint == null) o.isConstraint = 0;
                    if (o.Temp2 == null) o.Temp2 = "";
                    if (o.Temp1 == null) o.Temp1 = "";

                    db.SAReasons.Add(o);
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
        /// Update Item's Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool UpdateItem(SAReason o, params System.Linq.Expressions.Expression<Func<SAReason, object>>[] properties)
        {
            var item = db.SAReasons.Where(i => i.ID == o.ID).SingleOrDefault();
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
