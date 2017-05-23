using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
using DevExpress.Web.Mvc;
namespace FEA_ITS_Site.Controllers
{
    public class ItemDetailController : BaseController
    {
        //
        // GET: /ItemDetail/

        #region Item Detail Manage
        public ActionResult ItemDetailManage(string ItemID)
        {
            return View();
        }


        [ValidateInput(false)]
        public ActionResult EditModesPartial(string ItemID)
        {
            Item item = new ItemManager().GetItem(ItemID);
            if (item != null)
            {
                ViewData["ShowAddColumn"] = true;

                if (item.ItemType == Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION)
                {
                    ViewData["ShowUnitColumn"] = false;
                }
                else if (item.ItemType == Models.Helper.TagPrefixParameter.HARD_REGISTRATION || item.ItemType == Models.Helper.TagPrefixParameter.SECURITYAREA)
                {
                    ViewData["ShowUnitColumn"] = true;
                    if (item.ID == Models.Helper.ItemIDDefault.NormalMaterial)
                        ViewData["ShowAddColumn"] = false;

                }
                else
                {
                }
            }


            return GetGridView(new ItemDetailManager().GetItems(ItemID), Models.Helper.PartialParameter.ITEM_DETAIL_MANAGE_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] ItemDetail obj, string ItemID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    obj.ItemID = ItemID;
                    Item item = new ItemManager().GetItem(ItemID);
                    if (item != null && obj.ItemDetailName != null && obj.ItemDetailName != "")
                    {
                        if (item.ID == Models.Helper.ItemIDDefault.NormalMaterial)
                            ViewData["EditError"] = "Không thể thêm với loại: " + item.ItemName;
                        else
                        {
                            if (item.ID == Models.Helper.ItemIDDefault.Assets) obj.Temp1 = "1"; // Only with Asset Item

                            if (item.ItemType == Models.Helper.TagPrefixParameter.HARD_REGISTRATION)
                            {
                                if (obj.Temp2 == null || obj.Temp2 == "") // Check For Unit Really input
                                {
                                    ViewData["EditError"] = Resources.Resource.msgInputError;
                                    return EditModesPartial(ItemID);
                                }
                            }

                            bool _result = new FEA_BusinessLogic.ItemDetailManager().InsertItem(obj);
                            if (!_result)
                                ViewData["EditError"] = Resources.Resource.msgInsertFail;
                        }
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
            return EditModesPartial(ItemID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] ItemDetail obj, string ItemID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    obj.ItemID = ItemID;
                    Item item = new ItemManager().GetItem(ItemID);
                    if (item != null && obj.ItemDetailName != null && obj.ItemDetailName != "")
                    {
                        if (item.ID == Models.Helper.ItemIDDefault.NormalMaterial) obj.Temp1 = "";
                        else if (item.ID == Models.Helper.ItemIDDefault.Assets) obj.Temp1 = "1"; // Only with Asset Item

                        if (item.ItemType == Models.Helper.TagPrefixParameter.HARD_REGISTRATION)
                        {
                            if (obj.Temp2 == null || obj.Temp2 == "") // Check For Unit Really input
                            {
                                ViewData["EditError"] = Resources.Resource.msgInputError;
                                return EditModesPartial(ItemID);
                            }
                        }

                        bool _result = new FEA_BusinessLogic.ItemDetailManager().UpdateItem(obj, o => o.ItemDetailName, o => o.Temp1, o => o.Temp2);
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
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return EditModesPartial(ItemID);


            
        }
        #endregion

        /// <summary>
        /// Used at Device manage and hardmanage
        /// </summary>
        /// <returns></returns>
        public ActionResult ItemDetailPartial()
        {
            string ItemID = (Request.Params["ItemID"] != null) ? Request.Params["ItemID"].ToString() : "-1";
            List<ItemDetail> lst = new ItemDetailManager().GetItems(ItemID,FEA_ITS_Site.Helper.UserLoginInfo.UserPosition.ToUpper());
            return PartialView("ItemDetailPartial", lst);
        }

        public ActionResult ReasonPartial()
        {
            List<ItemDetail> lst = new List<ItemDetail>();
            string ItemID = (Request.Params["ItemID"] != null) ? Request.Params["ItemID"].ToString() : "-1";
            if (ItemID == "fc07bf47-ebd0-4a90-869c-b01c8175abc7") // Open SaleOrder Item
            {
                lst = new ItemDetailManager().GetReason();
            }       
            return PartialView("ReasonPartial", lst);
        }

    }
}
