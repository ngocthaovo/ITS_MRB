using FEA_BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FEA_ITS_Site.Controllers
{
    public class StockInEquipmentController : BaseController
    {
        //
        // GET: /StockInEquipment/
        #region "User Manage Order"
        public ActionResult StockInManagement()
        {            
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/StockInEquipment/StockInManagement") });
            //
            return View();
        }

        

        [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {

            List<StockInEquipment> lstItem = new StockInEquipmentManager().GetItems();
            
            return GetGridView(lstItem, Models.Helper.PartialParameter.STOCK_IN_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string ID)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.StockInEquipmentManager().DeleteItem(ID,Helper.UserLoginInfo.UserId);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteStockInFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(new StockInEquipmentManager().GetItems(), Models.Helper.PartialParameter.STOCK_IN_LIST_GRID);
        }
        #endregion

    }
}
