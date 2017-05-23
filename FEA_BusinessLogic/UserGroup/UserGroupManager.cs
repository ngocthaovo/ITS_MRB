using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
   public class UserGroupManager:Base.Connection
    {
       /// <summary>
        /// Get item from database
       /// </summary>
       /// <param name="iUserGroupID"></param>
       /// <returns></returns>
       public UserGroup GetItem(int iUserGroupID)
       {
           return db.UserGroups.Where(i => i.UserGroupID == iUserGroupID).SingleOrDefault();
       }

       /// <summary>
       /// Get items from database
       /// </summary>
       /// <param name="iEnabled"></param>
       /// <returns></returns>
       public List<UserGroup> GetItems(int? iEnabled=-1)
       {
           return db.UserGroups.Where(i => iEnabled >= 0 ? i.Enabled == iEnabled : true).ToList();
       }

       public int InsertItem(UserGroup o)
       {
           db.UserGroups.Add(o);
           db.SaveChanges();
           return o.UserGroupID;
       }

       public bool DeleteItem(int iUserGroupID)
       {

           //Delete SiteFunction_UserGroup
           int result = new SiteFunction_UserGroupManager().DeleteByUserGroup(iUserGroupID);

           //Delete UserGroup
            if(result>=0)
            {
                UserGroup item = db.UserGroups.Where(i => i.UserGroupID == iUserGroupID).SingleOrDefault();
                if (item != null)
                {
                    if (item.Users.Count > 0)
                        return false;

                    db.UserGroups.Remove(item);
                    db.SaveChanges();
                    return true;
                }
            }
           return false;
       }

       /// <summary>
       /// Update Node's Infomation
       /// </summary>
       /// <param name="o"></param>
       /// <returns></returns>
       public bool UpdateItem(UserGroup o, params System.Linq.Expressions.Expression<Func<UserGroup, object>>[] properties)
       {
           var item = db.UserGroups.Where(i => i.UserGroupID == o.UserGroupID).SingleOrDefault();
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
