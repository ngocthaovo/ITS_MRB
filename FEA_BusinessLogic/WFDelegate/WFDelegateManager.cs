using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
   public class WFDelegateManager:Base.Connection
   {
       /// <summary>
       /// Get items from database
       /// </summary>
       /// <param name="iEnabled"></param>
       /// <returns></returns>
       public List<WFDelegate> GetItems(int iUserID)
       {
           return db.WFDelegates.Where(i =>i.MainUserID == iUserID && i.Status != 0).ToList();
       }

       /// <summary>
       /// Get Delegator
       /// </summary>
       /// <param name="DelegatorID"></param>
       /// <returns></returns>
       public WFDelegate GetDelegateByDelegator(int DelegatorID)
       {

           DateTime dt = DateTime.Now.Date; 
           return db.WFDelegates.Where(i => 
               
               
               (i.MainUserID == DelegatorID)
               && (dt >= System.Data.Objects.EntityFunctions.TruncateTime(i.From))
               && (dt <= System.Data.Objects.EntityFunctions.TruncateTime(i.To))
               &&(i.Status == 1)
               ).SingleOrDefault();
       }



       public string InsertItem(WFDelegate o, int iUserID)
       {
           o.MainUserID = iUserID;
           o.DelegateID = Guid.NewGuid().ToString();
           o.Temp1 = o.Temp1 == "null" ? null : o.Temp1;
           o.Temp3 = null;
           o.Temp2 = o.Temp2== null ? null : o.Temp2;
           o.Status = 1;
           db.WFDelegates.Add(o);
           db.SaveChanges();
           return o.DelegateID;
       }


       public bool DeleteItem(string sID)
       {


           WFDelegate item = db.WFDelegates.Where(i => i.DelegateID == sID).SingleOrDefault();
               if (item != null)
               {
                   //db.WFDelegates.Remove(item);
                   item.Status = 0;
                   db.SaveChanges();
                   return true;
               }
           return false;
       }

       /// <summary>
       /// Update Node's Infomation
       /// </summary>
       /// <param name="o"></param>
       /// <returns></returns>
       public bool UpdateItem(WFDelegate o, params System.Linq.Expressions.Expression<Func<WFDelegate, object>>[] properties)
       {
           var item = db.WFDelegates.Where(i => i.DelegateID == o.DelegateID).SingleOrDefault();
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
