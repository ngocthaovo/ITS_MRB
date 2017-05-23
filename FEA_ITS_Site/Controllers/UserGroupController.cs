using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
using DevExpress.Web.Mvc;
namespace FEA_ITS_Site.Controllers
{
    public class UserGroupController : BaseController
    {
        //
        // GET: /UserGroup/
        UserGroupManager groupMng = new UserGroupManager();
        public ActionResult Index()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/UserGroup/Index") });
            //
          
            List<UserGroup> lst = new UserGroupManager().GetItems();
            return View(lst);
        }

        [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {
            return GetGridView(groupMng.GetItems(), Models.Helper.PartialParameter.USER_GROUP_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] UserGroup obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   if (obj.Enabled == null) obj.Enabled = 0;
                   int result =  groupMng.InsertItem(obj);

                   if (result < 0)
                    {
                        ViewData["EditError"] = Resources.Resource.msgInsertFail;
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return GetGridView(groupMng.GetItems(), Models.Helper.PartialParameter.USER_GROUP_LIST_GRID); ;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] UserGroup obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool result = groupMng.UpdateItem(obj, o => o.UserGroupName, o => o.Enabled);
                    if (!result)
                    {
                        ViewData["EditError"] = Resources.Resource.msgDeleteFail;
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;

            return GetGridView(groupMng.GetItems(), Models.Helper.PartialParameter.USER_GROUP_LIST_GRID);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(int UserGroupID)
        {
            if (UserGroupID > 0)
            {
                try
                {
                    bool flag = groupMng.DeleteItem(UserGroupID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(groupMng.GetItems(), Models.Helper.PartialParameter.USER_GROUP_LIST_GRID);
        }
        //Added by Tony (2016-10-04)
            public string SAAudit(int Status)
        {
            System.Data.Objects.ObjectParameter iResult;
            iResult = new FEA_SABusinessLogic.ExportItemManager().SAAudit(Status);
           return iResult.Value.ToString();
        }
        public int SAAuditStatus()
            {
                object x;
                x = new FEA_SABusinessLogic.ExportItemManager().SAAuditStatus();
                Type a = x.GetType();
                var n= a.GetProperty("ID").GetValue(x,null);
                return int.Parse(n.ToString());
            }
        //
    }
}
