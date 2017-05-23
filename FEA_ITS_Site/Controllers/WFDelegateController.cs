using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
namespace FEA_ITS_Site.Controllers
{
    public class WFDelegateController : BaseController
    {
        //
        // GET: /WFDelegate/

        public ActionResult Index()
        {           
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WFDelegate/DelegateManage") });
            //
            return View();
        }

        public ActionResult DelegateManage()
        {
            return View();
        }
        [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {
            int iCurrentUserID = Helper.UserLoginInfo.UserId;
            return GetGridView(new WFDelegateManager().GetItems(iCurrentUserID), Models.Helper.PartialParameter.DELEGATE_MANAGE_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFDelegate obj)
        {
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                try
                {
                    if (CheckValidate(obj))
                    {
                        int iCurrentUserID = Helper.UserLoginInfo.UserId;
                        string _result = new FEA_BusinessLogic.WFDelegateManager().InsertItem(obj, iCurrentUserID);
                        if (_result.Length ==0)
                            ViewData["EditError"] = Resources.Resource.msgInsertFail;
                    }
                    else
                    {
                        ViewData["EditError"] = Resources.Resource.msgInputError;
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return EditModesPartial();
        }


        [HttpPost]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFDelegate obj)
        {
            ModelState.Clear();
            if (ModelState.IsValid)
            {
                try
                {
                    if (CheckValidate(obj))
                    {
                        obj.Temp1 = obj.Temp1 == "null" ? null : obj.Temp1;
                        obj.Temp2 = obj.Temp2 == null ? null : obj.Temp2;
                        bool _result = new FEA_BusinessLogic.WFDelegateManager().UpdateItem(obj, o => o.DelegateUserID, o => o.From, o => o.Temp1, o => o.Temp2);
                        if (!_result)
                            ViewData["EditError"] = Resources.Resource.msgInsertFail;
                    }
                    else
                    {
                        ViewData["EditError"] = Resources.Resource.msgInputError;
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return EditModesPartial();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string DelegateID)
        {
            if (DelegateID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.WFDelegateManager().DeleteItem(DelegateID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return EditModesPartial();
        }


        private bool CheckValidate(WFDelegate obj)
        {
            if (obj.DelegateUserID == null || obj.From == null || obj.To == null)
                return false;
            if (obj.To < obj.From)
                return false;

            return true;
        }
    }
}
