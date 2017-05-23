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
    public class SAConfigureManager : FEA_BusinessLogic.Base.Connection
    {
        public List<SAConfigure> GetItems(string ReasonID)
        {
            return db.SAConfigures.Where(i => i.ReasonID.Contains(ReasonID.Replace("\\",""))).ToList();
        }

          public int InsertItem(SAConfigure o)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                try
                {
                    o.Temp1 = Guid.NewGuid().ToString();
                    db.SAConfigures.Add(o);
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


          public bool UpdateItem(SAConfigure o, params System.Linq.Expressions.Expression<Func<SAConfigure, object>>[] properties)
          {
              var item = db.SAConfigures.Where(i => i.Temp1 == o.Temp1).SingleOrDefault();
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

          public bool DeleteItem(string ID)
          {
              SAConfigure item = db.SAConfigures.Where(i => i.Temp1 == ID).SingleOrDefault();
              if (item != null)
              {
                  db.SAConfigures.Remove(item);
                  db.SaveChanges();
                  return true;
              }
              return false;
          }
    }
}
