using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
using DevExpress.Web.Mvc;
namespace FEA_ITS_Site.Controllers
{
    public class SiteFunctionUserGroupController : BaseController
    {
        //
        // GET: /SiteFunctionUserGroup/

        public ActionResult Index(int? UserGroupID =-1)
        {
            ViewBag.UserGroupID = UserGroupID;
            UserGroup group = new UserGroupManager().GetItem(UserGroupID.Value);
            if(group != null)
                ViewBag.UserGroupName = group.UserGroupName;
            else
                ViewBag.UserGroupName = "";
            return View();
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Index(FormCollection form, int UserGroupID)
        {
            ViewBag.UserGroupID = UserGroupID;
            UserGroup group = new UserGroupManager().GetItem(UserGroupID);
            if (group != null)
                ViewBag.UserGroupName = group.UserGroupName;
            else
                ViewBag.UserGroupName = "";
            try
            {
                string checkedNodes = form["hidden_1"];
                string[] lstNodes;
                if(checkedNodes.Trim().Length == 0 || checkedNodes == "")
                    lstNodes = new string[0];
                else
                    lstNodes = checkedNodes.Split(';');
                
                int result = new FEA_BusinessLogic.SiteFunction_UserGroupManager().UpdatePremission(UserGroupID, lstNodes);
                if (result > 0)
                {
                    ViewBag.UpdateStatus = true;
                    ViewBag.UpdateMessage = Resources.Resource.msgUpdateSuccess;
                }
                else
                {
                    ViewBag.UpdateStatus = true;
                    ViewBag.UpdateMessage = Resources.Resource.msgUpdateFailed;
                }
            }
            catch(Exception ex)
            {
                ViewBag.UpdateStatus = false;
                ViewBag.UpdateMessage = ex.Message;
            }

            return View();
        }
        //[ValidateInput(false)]
        //public ActionResult EditModesPartial(int UserGroupID)
        //{
        //    return GetGridView(new SiteFunctionManager().GetItems(), Models.Helper.PartialParameter.SITEFUNCTION_USERGROUP_TREEVIEW);
        //}

        public static void CreateTreeViewNodesRecursive(List<SiteFunction> model, List<SiteFunction_UserGroup> lstPermission, MVCxTreeViewNodeCollection nodesCollection, Int32 parentID)
        {
            //var rows = model.Select("[ParentID] = " + parentID);
            var rows = model.Where(o=>o.ParentID == parentID).ToList();
            foreach (SiteFunction row in rows)
            {
                MVCxTreeViewNode node = nodesCollection.Add(FEA_ITS_Site.Helper.SessionManager.CurrentLang == FEA_Ultil.FEALanguage.LangCode_VN?row.SiteFunctionName:row.SiteFunctionNameEN, row.SiteFunctionID.ToString());
                node.Checked = lstPermission.Where(o=>o.SiteFunctionID == row.SiteFunctionID).Count()>0?true:false;
                
                CreateTreeViewNodesRecursive(model,lstPermission, node.Nodes, row.SiteFunctionID);
            }
        }
    }
}
