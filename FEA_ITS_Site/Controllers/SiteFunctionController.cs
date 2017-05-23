using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;

namespace FEA_ITS_Site.Controllers
{
    public class SiteFunctionController : BaseController
    {
        //
        // GET: /SiteFunction/
        public SiteFunctionManager sitefcManager = new SiteFunctionManager();
        public ActionResult Index()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/SiteFunction/Index") });
            //

            return View();
        }
        public ActionResult DataBindingPartial()
        {
            // if (DevExpressHelper.IsCallback)
            // Intentionally pauses server-side processing,
            // to demonstrate the Loading Panel functionality.
            //Thread.Sleep(500);
            return GetGridView(sitefcManager.GetItems(), Models.Helper.PartialParameter.SITE_FUNCTION_TREELIST);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult InlineEditingAddNewPostPartial([ModelBinder(typeof(DevExpressEditorsBinder))]SiteFunction o)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    if (!ValidateObject(o))
                    {
                        ViewData["EditNodeError"] = Resources.Resource.msgInputError;
                    }
                    else
                    {
                        o = RepairObject(o);
                        int result = sitefcManager.InsertItem(o);
                        if (result < 0)
                            ViewData["EditNodeError"] = Resources.Resource.msgInsertFail;
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditNodeError"] = e.Message;
                }
            }
            else
                ViewData["EditNodeError"] = "Please, correct all errors.";
            return GetGridView(sitefcManager.GetItems(), Models.Helper.PartialParameter.SITE_FUNCTION_TREELIST);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult InlineEditingUpdatePostPartial([ModelBinder(typeof(DevExpressEditorsBinder))] SiteFunction o)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // NewsGroupsProvider.UpdatePost(post);

                    if(!ValidateObject(o))
                    {
                        ViewData["EditNodeError"] = Resources.Resource.msgInputError;
                    }
                    else
                    {
                        o = RepairObject(o);
                        bool flag =  sitefcManager.UpdateItem(o, i => i.SiteFunctionName, i => i.SiteFunctionNameEN, i => i.Enabled
                                                  , i => i.IconCssClass, i => i.Order, i => i.ParentID, i => i.URL);
                        if(!flag)
                            ViewData["EditNodeError"] = Resources.Resource.msgInsertFail;
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditNodeError"] = e.Message;
                }
            }
            else
                ViewData["EditNodeError"] = "Please, correct all errors.";
            return GetGridView(sitefcManager.GetItems(), Models.Helper.PartialParameter.SITE_FUNCTION_TREELIST);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult InlineEditingMovePostPartial(int SiteFunctionID, int? ParentID)
        {
            // NewsGroupsProvider.MovePost(postID, parentID);
            bool flag = sitefcManager.MoveItem(SiteFunctionID, ParentID==null?0:ParentID.Value);
            if (!flag)
                ViewData["EditNodeError"] = Resources.Resource.msgInsertFail;

            return GetGridView(sitefcManager.GetItems(), Models.Helper.PartialParameter.SITE_FUNCTION_TREELIST);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult InlineEditingDeletePostPartial(int SiteFunctionID)
        {
            try
            {
                // NewsGroupsProvider.DeletePost(postID);
                bool flag = sitefcManager.DeleteItem(SiteFunctionID);
                if (!flag)
                    ViewData["DeleteNodeError"] = Resources.Resource.msgDeleteFail;
            }
            catch (Exception e)
            {
                ViewData["DeleteNodeError"] = e.Message;
            }
            return GetGridView(sitefcManager.GetItems(), Models.Helper.PartialParameter.SITE_FUNCTION_TREELIST);
        }

        public ActionResult IconList()
        {
            return View();
        }


        private SiteFunction RepairObject(SiteFunction o)
        {
            if (o.ParentID == null) o.ParentID = 0;
            if (o.Enabled == null) o.Enabled = (int)Models.Helper.Status.enabled;
            if (o.IconCssClass == null) o.IconCssClass = "";
            if (o.URL == null) o.URL = "";

            return o;
        }

        private bool ValidateObject(SiteFunction o)
        {
            if (o.SiteFunctionName == null || o.SiteFunctionName.Length == 0)
                return false;
            else if (o.SiteFunctionNameEN == null || o.SiteFunctionNameEN.Length == 0)
                return false;
            else if (o.Order == null)
                return false;
            return true; 
        }


    }
}
