using FEA_BusinessLogic;
using FEA_ITS_Site.Models;
using FEA_ITS_Site.Models.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FEA_ITS_Site.Controllers
{
    public class StockOutEquipmentController : BaseController
    {
        //
        // GET: /StockOutEquipment/

        public ActionResult MainPage()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/StockOutEquipment/MainPage") });
            //


            return View();
        }

        #region "Load Page"
        /// <summary>
        /// Create Stockin Order
        /// </summary>
        /// <returns></returns>
        public ActionResult StockOut(string ID, int? editStatus)
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/StockOutEquipment/StockOut") });
            //

            StockOutEquipment deRegItem;
            User uInfo = new User();
            Boolean bIsCreateNew = false;

            if (ID == null || ID.Trim().Length == 0) // Create New
            {
                // Get User Logged info
                uInfo = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
                // get DeviceReg Info
                deRegItem = new StockOutEquipment()
                {
                    CreateDate = DateTime.Now,
                    Description = "",
                    CreatorID = Helper.UserLoginInfo.UserId,
                    Status = (int)DeviceRegistrationManager.OrderStatus.New,
                    OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.STOCK_OUT),
                };

                // Item Detail
                Session["ItemInfoList"] = new List<ItemInfo>();
                bIsCreateNew = true;
            }
            else // Load Info and show to View Page
            {
                //deRegItem = new StockInEquipment();
                deRegItem = new StockOutEquipmentManager().GetItem(ID);

                if (deRegItem != null)
                {
                    // Get User Logged info
                    uInfo = new UserManager().GetItem(deRegItem.CreatorID.Value);
                    Session["ItemInfoList"] = ConvertToItemInfos(deRegItem.StockOutEquipmentDetails);
                    //GetApprover - this is partial

                    bIsCreateNew = false;
                }
            }

            if (editStatus != null)
                ViewBag.EditStatus = editStatus;
            ViewBag.User = uInfo;
            ViewBag.IsCreateNew = bIsCreateNew;
            return View(deRegItem);
        }

        /// <summary>
        /// Convert List of HardwareRequirementDetail to List of ItemInfo
        /// </summary>
        /// <param name="HardRgDetails"></param>
        /// <returns></returns>
        private List<ItemInfo> ConvertToItemInfos(ICollection<StockOutEquipmentDetail> HardRgDetails)
        {
            List<ItemInfo> lstResult = new List<ItemInfo>();
            ItemInfo _itemInfo;
            foreach (StockOutEquipmentDetail dvResDetailItem in HardRgDetails)
            {
                _itemInfo = new ItemInfo()
                {
                    ID = dvResDetailItem.ID,
                    ItemID = dvResDetailItem.ItemID,
                    ItemName = dvResDetailItem.Item.ItemName,
                    ItemDetailID = dvResDetailItem.ItemDetailID,
                    ItemDetailName = dvResDetailItem.ItemDetail.ItemDetailName,
                    Des = dvResDetailItem.Description,
                    Quantity = dvResDetailItem.Quantity.Value,
                    UnitID = dvResDetailItem.UnitID,
                    DeliveryDate = dvResDetailItem.DeliveryDate,
                    UnitName = dvResDetailItem.Unit.NAME,

                };
                lstResult.Add(_itemInfo);
            }
            return lstResult;
        }


        public ActionResult UserListPartial(StockOutEquipment model)
        {

            return PartialView("_UserListPartial", model);
        }

        #endregion

        #region Item Detail in list Manager
        /// <summary>
        /// Add Item and ItemDetail to Grid
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="ItemDetail"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddItemToOrder(string ItemID, string ItemDetail, string Description, DateTime? DeliveryDate, int? Quantity = 0)
        {

            if (ItemID.Trim().Length == 0 || ItemDetail.Trim().Length == 0
                || Convert.ToDecimal(Quantity) <= 0 || DeliveryDate == null || DeliveryDate.Value.Date < DateTime.Now.Date)
            {
                string _sErr = Resources.Resource.msgInputError;
                if (ItemID.Trim().Length == 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.ItemType);
                if (ItemDetail.Trim().Length == 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Specs);
                if (Convert.ToDecimal(Quantity) <= 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Quantity);
                if (DeliveryDate == null || DeliveryDate.Value.Date < DateTime.Now.Date)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.DeliveryDate);

                ViewData["EditError"] = _sErr;
            }
            else
            {
                var flag = FindItemDetailInList(ItemDetail);
                if (flag) // Đã tồn tại trong list
                    ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                else
                {
                    try
                    {
                        Unit unit;
                        Item i = new ItemManager().GetItem(ItemID);

                        ItemDetail item_detail = new ItemDetailManager().GetItem(ItemDetail);
                        unit = new UnitManager().GetItem(item_detail == null ? "" : item_detail.Temp2); // temp2 was unit id

                        string sError = "";
                        var checkInventory = CheckInventoryInStock(item_detail.ID, item_detail.ItemDetailName, unit.ID, unit.NAME, Quantity.Value, ref sError);
                        if (!checkInventory)
                        {
                            ViewData["EditError"] = sError;
                        }
                        else
                        {
                            Models.ItemInfo item = new ItemInfo()
                            {
                                ID = Guid.NewGuid().ToString(),
                                ItemID = i.ID,
                                ItemName = i.ItemName,
                                ItemDetailID = item_detail.ID,
                                ItemDetailName = item_detail.ItemDetailName,
                                Des = Description,

                                DeliveryDate = DeliveryDate,
                                UnitID = unit == null ? "" : unit.ID,
                                UnitName = unit == null ? "" : unit.NAME,
                                Quantity = Quantity.Value,
                            };
                            var iInStock = new FEA_BusinessLogic.Base.Connection().db.sp_GetEquipmentInventory(item.ItemDetailID, item.UnitID).SingleOrDefault();
                            item.QuanInStock = (iInStock == null) ? 0 : iInStock.InventoryQuantity.Value;
                            item.QuanNeeded = (item.Quantity > item.QuanInStock) ? (item.Quantity - item.QuanInStock) : 0;



                            List<ItemInfo> lst = new List<ItemInfo>();
                            if (Session["ItemInfoList"] != null)
                            {
                                lst = (List<ItemInfo>)Session["ItemInfoList"];
                                lst.Add(item);
                                Session["ItemInfoList"] = lst;
                            }
                            else
                            {
                                lst.Add(item);
                                Session["ItemInfoList"] = lst;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["EditError"] = ex.Message;
                    }

                }
            }
            ViewData["ShowCommand"] = true;
            return GetGridView(Session["ItemInfoList"], Models.Helper.PartialParameter.ITEM_DETAIL_STOCKITEM_LIST_GRID);
        }

        private bool FindItemDetailInList(string ItemDetailID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];

            return lst.Where(i => i.ItemDetailID == ItemDetailID).Count() > 0 ? true : false;
        }


        public ActionResult GetListItemDetailPartial(bool? ShowCommand = false)
        {
            ViewData["ShowCommand"] = ShowCommand;
            return GetGridView((List<ItemInfo>)Session["ItemInfoList"], Models.Helper.PartialParameter.ITEM_DETAIL_STOCKITEM_LIST_GRID);
        }

        /// <summary>
        /// Delete an Item in Grid 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeleteItemDetail(string ID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];

            ItemInfo i = lst.Where(o => o.ID == ID).SingleOrDefault();
            lst.Remove(i);

            Session["ItemInfoList"] = lst;
            ViewData["ShowCommand"] = true;
            return GetGridView((List<ItemInfo>)Session["ItemInfoList"], Models.Helper.PartialParameter.ITEM_DETAIL_STOCKITEM_LIST_GRID);
        }

        #endregion

        #region  SaveToDB


        /// <summary>
        /// Save to DB for Inserting and Updateing
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        /// 

        [HttpPost]
        public ActionResult StockOut([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] StockOutEquipment o, bool CreateNew)
        {

            string sResult = "";

            bool ErrInventoty = false;
            o.DeliveryFor = Convert.ToInt32(DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboUserList"));

            int editStatus = (int)Models.Helper.EditItemStatus.failed;

            if (Request.Form["btnSaveDraff"] != null) //Save Draft
            {
                sResult = CreateStockOutOrder(o, true);
            }
            else if (Request.Form["btnSaveAndSend"] != null) // Save And Send
            {
                BaseJsonResult basejson = CheckListInventoryInStock();
                if (basejson.ErrorCode == 1)
                {
                    ViewBag.ErrInfo = basejson.Message;
                    sResult = o.ID == null ? "" : o.ID;
                    ErrInventoty = true;
                }
                else
                {
                    if (CreateNew)
                        sResult = CreateStockOutOrder(o, false);//isSaveDraff = false - Create Order;
                    else
                        sResult = UpdateStockOutOrder(o, false);//isSaveDraff = false - Update Order;
                }
            }
            else if (Request.Form["btnUpdate"] != null) //Update - 
            {
                sResult = UpdateStockOutOrder(o, true);
            }
            else if (Request.Form["btnCancelConfirm"] != null)
            {
                sResult = UpdateCancelConfirm(o, false);
            }


            CreateNew = sResult.Length > 0 ? false : CreateNew;

            editStatus = sResult.Length > 0 ? (int)Models.Helper.EditItemStatus.success : editStatus = (int)Models.Helper.EditItemStatus.failed;
            if (ErrInventoty) editStatus = (int)Models.Helper.EditItemStatus.failed;


            if (ErrInventoty)
            {
                o.ID = sResult;
                ViewBag.EditStatus = editStatus;
                ViewBag.User = new UserManager().GetItem(Helper.UserLoginInfo.UserId); ;
                ViewBag.IsCreateNew = CreateNew;
                return View(o);

            }
            else
            {
                if (sResult.Length > 0)
                    return RedirectToAction("StockOut", new { ID = sResult, editStatus = (int)Models.Helper.EditItemStatus.success });// Index(sResult,"","", (int)Models.Helper.EditItemStatus.success);
                else
                    return RedirectToAction("StockOut", new { ID = sResult, editStatus = (int)Models.Helper.EditItemStatus.failed });//Index(sResult, "", "", (int)Models.Helper.EditItemStatus.failed);
            }

        }

        /// <summary>
        /// Create StockOut Information
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <param name="wfMain"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string CreateStockOutOrder(StockOutEquipment o, bool isSaveDraff)
        {
            //Master
            o.DeliveryFor = Convert.ToInt32(DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboUserList"));
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.StockOutEquipmentDetails = StockOutEquipmentDetailList();
            o.OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.STOCK_OUT);
            o.Temp2 = "";

            if (isSaveDraff)
                o.Status = (int)DeviceRegistrationManager.OrderStatus.DRAFT;
            else
            {
                o.Status = (int)DeviceRegistrationManager.OrderStatus.FINSHED;
                o.CofirmDate = DateTime.Now;
            }
            string sID = new StockOutEquipmentManager().InsertItem(o, isSaveDraff);
            return sID;
        }

        /// <summary>
        /// this function using for Update (update, save and send) when document Returned
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string UpdateStockOutOrder(StockOutEquipment o, bool isSaveDraff)
        {

            //Master
            o.DeliveryFor = Convert.ToInt32(DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboUserList"));
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.StockOutEquipmentDetails = StockOutEquipmentDetailList();
            o.Temp2 = "";
            bool _result = new StockOutEquipmentManager().UpdateItem(o, isSaveDraff,
                                                                         i => i.Description, i => i.DeliveryFor);
            return _result == true ? o.ID : "";
        }
          /// <summary>
          /// added by Steven 2014-07-11
          /// Update status  = 1 and IT inventory
          /// </summary>
          /// <param name="o"></param>
          /// <param name="isSaveDraff"></param>
          /// <returns></returns>
        private string UpdateCancelConfirm(StockOutEquipment o, bool isSaveDraff)
        {
            o.StockOutEquipmentDetails = StockOutEquipmentDetailList();
            bool _result = new StockOutEquipmentManager().UpdateCancelItem(o, isSaveDraff,
                                                                         i => i.StockOutEquipmentDetails);
            return _result == true ? o.ID : "";
        }

        /// <summary>
        /// Get DeviceRegistrationDetail to save to DB
        /// </summary>
        /// <returns></returns>
        private List<StockOutEquipmentDetail> StockOutEquipmentDetailList()
        {
            List<StockOutEquipmentDetail> lst = new List<StockOutEquipmentDetail>();
            if (Session["ItemInfoList"] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session["ItemInfoList"];
                StockOutEquipmentDetail harddetail;
                foreach (ItemInfo i in lstItemInfo)
                {
                    harddetail = new StockOutEquipmentDetail()
                    {
                        Description = i.Des,
                        ItemDetailID = i.ItemDetailID,
                        ItemID = i.ItemID,
                        Quantity = i.Quantity,
                        UnitID = i.UnitID,
                        DeliveryDate = i.DeliveryDate,
                        Temp1 = "",
                        Temp2 = ""
                    };
                    lst.Add(harddetail);
                }
            }
            return lst;
        }


        // [HttpPost]
        public BaseJsonResult CheckListInventoryInStock()
        {
            BaseJsonResult result = new BaseJsonResult() { ErrorCode = 0, Message = "" };
            bool hasError = false;
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];

            string message = "";

            if (lst.Count == 0)
            {
                hasError = true;
                message = Resources.Resource.msgSelectProductItem;
            }
            else
            {
                foreach (ItemInfo item in lst)
                {
                    string _sError = "";
                    var flag = CheckInventoryInStock(item.ItemDetailID, item.ItemDetailName, item.UnitID, item.UnitName, item.Quantity, ref _sError);
                    if (!flag)
                    {
                        hasError = true;
                        message += _sError + "</br>";
                    }
                }

            }


            if (hasError)
            {
                result.ErrorCode = 1;
                result.Message = message;
            }

            return result;
        }

        private bool CheckInventoryInStock(string ItemDetailID, string ItemDetailName, string UnitID, string UnitName, int Quantity, ref string sError)
        {
            int itemInStock = new FEA_BusinessLogic.ITInventoryManager().CountInStock(ItemDetailID, UnitID);
            var flag = (Quantity > itemInStock) ? false : true;
            if (!flag)
            {
                sError = string.Format("[{0}] không đủ số lượng, số lượng hiện tại chỉ còn: {1}({2})", ItemDetailName, itemInStock, UnitName);
                return false;
            }

            return true;
        }
        #endregion


        //
        // GET: /StockInEquipment/
        #region "User Manage Order"
        public ActionResult StockOutManagement()
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

            List<StockOutEquipment> lstItem = new StockOutEquipmentManager().GetItems();
            return GetGridView(lstItem, Models.Helper.PartialParameter.STOCK_OUT_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string ID)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.StockOutEquipmentManager().DeleteItem(ID, Helper.UserLoginInfo.UserId);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteStockInFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(new StockOutEquipmentManager().GetItems(), Models.Helper.PartialParameter.STOCK_OUT_LIST_GRID);
        }
        #endregion

    }
}
