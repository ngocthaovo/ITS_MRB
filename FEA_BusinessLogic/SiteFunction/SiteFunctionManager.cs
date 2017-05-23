using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class SiteFunctionManager:Base.Connection
    {
        public List<SiteFunction> GetItems(int? iEnabled = -1)
        {
            return db.SiteFunctions.OrderBy(i=>i.Order).Where(i=> iEnabled <0?true:i.Enabled == iEnabled).ToList();
        }

        public List<SiteFunction> GetItemsPermission(int iUserGroupID)
        {
            return db.SiteFunctions.OrderBy(i => i.Order)
                                                        .Join(db.SiteFunction_UserGroup, s => s.SiteFunctionID, su => su.SiteFunctionID, (s, su) => new { SiteFunction = s, SiteFunction_UserGroup = su })
                                                        .Where(x=>x.SiteFunction_UserGroup.UserGroup == iUserGroupID)
                                                        .Select(x=>x.SiteFunction).ToList();
        }

        /// <summary>
        /// Insert an item to database
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public int InsertItem(SiteFunction o)
        {
            db.SiteFunctions.Add(o);
            db.SaveChanges();
            return o.SiteFunctionID;
        }


        /// <summary>
        /// Update item
        /// </summary>
        /// <param name="o"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public bool UpdateItem(SiteFunction o, params System.Linq.Expressions.Expression<Func<SiteFunction, object>>[] properties)
        {
            var item = db.SiteFunctions.Where(i => i.SiteFunctionID == o.SiteFunctionID).SingleOrDefault();
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

        public bool MoveItem(int iSiteFunctionID, int iParentID )
        {

            var item = db.SiteFunctions.Where(i => i.SiteFunctionID == iSiteFunctionID).SingleOrDefault();
            if(item != null)
            {
                item.ParentID = iParentID;

                db.SaveChanges();
                return true;
            }
            return false;

        }

        /// <summary>
        /// Delete a item
        /// </summary>
        /// <param name="iItemID"></param>
        /// <returns></returns>
        public bool DeleteItem(int iItemID)
        {
            SiteFunction item = db.SiteFunctions.Where(i => i.SiteFunctionID == iItemID).SingleOrDefault();
            if (item != null)
            {
                if (item.SiteFunction_UserGroup.Count > 0) // Chekc for permission
                    return false;

                int childCount = db.SiteFunctions.Where(i => i.ParentID == item.SiteFunctionID).Count(); // Count for child menus
                if (childCount > 0)
                    return false;   


                db.SiteFunctions.Remove(item);
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
