using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic
{
    public class SiteFunction_UserGroupManager : Base.Connection
    {
        public List<SiteFunction_UserGroup> GetItems(int iUserGroupID)
        {
            return db.SiteFunction_UserGroup.Where(o=>o.UserGroup == iUserGroupID).ToList();
        }

        /// <summary>
        /// Remmove the sitefunction_usergroup by UsergroupID
        /// </summary>
        /// <param name="iUserGroupID"></param>
        /// <returns></returns>
        public int DeleteByUserGroup(int iUserGroupID)
        {
            List<SiteFunction_UserGroup> items = db.SiteFunction_UserGroup.Where(i => i.UserGroup == iUserGroupID).ToList();
            if(items.Count> 0)
            {
                foreach (SiteFunction_UserGroup item in items)
                    db.SiteFunction_UserGroup.Remove(item);
                db.SaveChanges();
            }
            return items.Count();
            
        }

        /// <summary>
        /// Update Permission 
        /// 1. Remove old Permission
        /// 2. Add new 
        /// </summary>
        /// <param name="iUserGroup"></param>
        /// <param name="lstPermission"></param>
        /// <returns></returns>
        public int UpdatePremission(int iUserGroup, string[]lstPermission)
        {
            List<SiteFunction_UserGroup> lstItems = db.SiteFunction_UserGroup.Where(o => o.UserGroup == iUserGroup).ToList();
            if(lstItems.Count > 0)
            {
                foreach(SiteFunction_UserGroup item in lstItems)
                {
                    db.SiteFunction_UserGroup.Remove(item);
                }
            }
            foreach (string sSiteFunctionID in lstPermission)
            {
                SiteFunction_UserGroup o = new SiteFunction_UserGroup()
                {
                    Enabled = 1,
                    SiteFunctionID = int.Parse(sSiteFunctionID),
                    UserGroup = iUserGroup
                };

                db.SiteFunction_UserGroup.Add(o);
            }
            db.SaveChanges();
            return 1;
        }
    }
}
