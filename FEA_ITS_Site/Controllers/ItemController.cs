using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
namespace FEA_ITS_Site.Controllers
{
    public class ItemController : BaseController
    {

        #region "Item management"
        public ActionResult ItemManage()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/Item/ItemManage") });
            //
            return View();
        }


        [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {
            return GetGridView(new ItemManager().GetAllItems(), Models.Helper.PartialParameter.ITEM_MANAGE_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] Item obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.ItemType != null && obj.ItemType !="" && obj.ItemName != null && obj.ItemName != null)
                    {
                        int _result = new FEA_BusinessLogic.ItemManager().InsertItem(obj);
                        if(_result<1)
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


        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] Item obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.ItemType != null && obj.ItemType != "" && obj.ItemName != null && obj.ItemName != null)
                    {
                        bool _result = new FEA_BusinessLogic.ItemManager().UpdateItem(obj,o=>o.ItemType,o=>o.ItemName,o=>o.Status);
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
        #endregion

        //
        // GET: /Item/
        /// <summary>
        /// using on devicemanage and hardware manage
        /// </summary>
        /// <param name="sItemType"></param>
        /// <returns></returns>
        public ActionResult ItemPartial(string sItemType,string sOrderType)
        {
            List<Item> lstItem = new FEA_BusinessLogic.ItemManager().GetItems(sItemType,sOrderType, 1);

            return PartialView("ItemPartial", lstItem);
        }
        
    }
}
