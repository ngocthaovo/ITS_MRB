using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using FEA_BusinessLogic;
using System.Transactions;
namespace FEA_SABusinessLogic
{
    public class SADestinationManager : FEA_BusinessLogic.Base.Connection
    {

        public List<SADestination> GetItems(string Code, string Name, int Status)
        {
            return db.SADestinations.Where(i=>
                    (Status > -1? i.Status == Status:true)
                    &&(i.CompanyCode.Contains(Code))
                    &&(i.CompanyName.Contains(Name))
                
                ).ToList();
        }

        public List<SADestination> GetItems()
        {
            return db.SADestinations.OrderBy(x=>x.CompanyCode).ToList();
        }

        public int InsertItem(SADestination o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    o.ID = Guid.NewGuid().ToString();
                    if (o.Status == null) o.Status = 0;
                    if (o.Temp2 == null) o.Temp2 = "";
                    if (o.Temp1 == null) o.Temp1 = "";

                    db.SADestinations.Add(o);
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
        public bool UpdateItem(SADestination o, params System.Linq.Expressions.Expression<Func<SADestination, object>>[] properties)
        {
            var item = db.SADestinations.Where(i => i.ID == o.ID).SingleOrDefault();
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
