using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
using FEA_ITS_Site.Models;
using FEA_ITS_Site.Models.Helper;
using System.IO;
using DevExpress.Web.Mvc;
using DevExpress.XtraBars.Navigation;
using DevExpress.Web;
using DevExpress.XtraPivotGrid;
using DevExpress.Utils;
using System.Data.Objects;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrintingLinks;
namespace FEA_ITS_Site.Controllers
{
    public class MaintenanceController : BaseController
    {
        //
        // GET: /Maintenance/
        public string SessionTmpName = "ItemInfoListMN";

        #region Approve settings page
        public ActionResult ApproveMainPage()
        {
            return View();
        }

        public ActionResult ApprovePage(string strType, int? iStatus=-1)
        {
            ViewBag.EditStatus = iStatus;
            ViewBag.Type = strType; //Added by Tony (2017-05-17)
            return View();
        }

        public ActionResult ApprovedManagement(string Type)
        {
            ViewBag.Type = Type;
            return View();
        }

        public ActionResult GetConfirmedDoc(string strType)
        {
            List<MNRequestMain> lst = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetConfirmedRequest(Helper.UserLoginInfo.UserId);
            return GetGridViewV2(lst, Models.Helper.PartialParameter.MNConfirmedGrid, strType.ToUpper());
        }

        public ActionResult GetConfirmedDocDetails(string ID, string strType)
        {
            ViewBag.ID = ID;
            List<MNRequestMainDetail> lst = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetConfirmedRequestDetails(ID);
            return GetGridViewV2(lst, Models.Helper.PartialParameter.MaintenanceItemDetail, strType.ToUpper());
        }

        public ActionResult GetApproveDoc(string strType, int? iStatus = -1) //Lấy các yêu cầu cần kí duyệt
        {
            ViewBag.EditStatus = iStatus;
            ViewBag.Type = strType.ToUpper();//Added by Tony (2017-05-17)
            List<sp_GetMaintenanceApproveDocument_Result> lst = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetDocumentForApprove(3, Helper.UserLoginInfo.UserId);
            return GetGridViewV2(lst, Models.Helper.PartialParameter.MNApproveDoc, strType.ToUpper());
        }

        public ActionResult ApproveForm(string ID)
        {
            ViewBag.ID = ID;
            return View();
        }

        public ActionResult ApproveDetailList(string ID, string strType) //Lấy thông tin chi tiết số lượng của yêu cầu cần kí duyệt
        {
            ViewBag.ID = ID;
            ViewBag.Type = strType;
            Object obj = new Object();
            obj = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetDetailListQuantity(ID);
            return GetGridViewV2(obj, Models.Helper.PartialParameter.MNApproveQuantity, strType.ToUpper());
        }

        [ValidateInput(false)]
        public ActionResult GetUpdateQuantity(MVCxGridViewBatchUpdateValues<sp_GetMaintenanceDetailsQuantity_Result, int> upDatevalue, string ID, string strType)
        {
            List<sp_GetMaintenanceDetailsQuantity_Result> lstInventory = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetDetailListQuantity(ID); //Lấy số lượng tồn hiện tại
            List<sp_GetMaintenanceDetailsQuantity_Result> lstFinalQuantity = new List<sp_GetMaintenanceDetailsQuantity_Result>();
            sp_GetMaintenanceDetailsQuantity_Result lstItem;
            foreach (var lstUpdate in upDatevalue.Update)
            {
                foreach (sp_GetMaintenanceDetailsQuantity_Result lst in lstInventory)
                {
                    if (lstUpdate.ItemDetailID == lst.ItemDetailID)
                    {
                        lst.ITSInventory = lstUpdate.ITSInventory;
                        lst.ERPInventory = lstUpdate.ERPInventory;
                        lst.PurchaseQuantity = lstUpdate.PurchaseQuantity;
                    }
                    else
                    {
                        decimal PurQuantity, ITSQuantity, ERPQuantity, DiffQuantity;
                        ITSQuantity = Convert.ToDecimal(lst.ITSInventory);
                        ERPQuantity = Convert.ToDecimal(lst.ERPInventory);
                        PurQuantity = Convert.ToDecimal(lst.PurchaseQuantity);
                        DiffQuantity = Convert.ToDecimal(lst.RequestQuantity) - Convert.ToDecimal(lst.PurchaseQuantity);
                        if (DiffQuantity == 0)
                        {
                            ITSQuantity = 0;
                            ERPQuantity = 0;
                        }
                        else if (DiffQuantity <= ITSQuantity)
                        {
                            ITSQuantity = DiffQuantity;
                            ERPQuantity = 0;
                        }
                        else if (ERPQuantity >= (DiffQuantity - ITSQuantity))
                        {
                            ERPQuantity = DiffQuantity - ITSQuantity;
                        }
                        else
                        {
                            PurQuantity = PurQuantity + (DiffQuantity - ITSQuantity);
                        }
                        lst.ITSInventory = ITSQuantity;
                        lst.ERPInventory = ERPQuantity;
                        lst.PurchaseQuantity = PurQuantity;
                    }
                }
            }

            if (!CheckInventory(lstInventory, ID))
            {
                ViewData["EditError"] = Resources.Resource.msgDiffQuantity;
                return GetGridViewV2(lstInventory, Models.Helper.PartialParameter.MNApproveQuantity, strType.ToUpper());
            }
            else
            {
                bool isUpdate = new FEA_BusinessLogic.Maintenance.MNInventoryManager().UpdateFinalQuantity(lstInventory, ID);
                if (isUpdate)
                {
                    ObjectParameter outParam;
                    outParam = new FEA_BusinessLogic.Maintenance.MNInventoryManager().SendQuantityToEland(ID,Helper.UserLoginInfo.UserId);
                    if (outParam.Value.ToString() != "FAILED")
                    {
                        bool isUpdateStatus = new FEA_BusinessLogic.Maintenance.MNInventoryManager().UpdateRequestStatus(ID, 5, Helper.UserLoginInfo.UserId);
                        if (isUpdateStatus)
                        {
                            string url = Url.Action("ApprovePage", "Maintenance", new { iStatus = 1, strType = strType });
                            Response.Redirect(url);
                            return null;
                        }
                        else
                        {
                            string url = Url.Action("ApprovePage", "Maintenance", new { iStatus = 0, strType = strType });
                            Response.Redirect(url);
                            return null;
                        }
                    }
                    else
                    {
                        string url = Url.Action("ApprovePage", "Maintenance", new { iStatus = 0, strType = strType });
                        Response.Redirect(url);
                        return null;
                    }   
                }
                else
                {
                    ViewData["EditError"] = Resources.Resource.msgErrorInputQuantity;
                    return GetGridViewV2(lstInventory, Models.Helper.PartialParameter.MNApproveQuantity, strType.ToUpper());
                }
            }
            return View();
        }

       [ValidateInput(false)]
        public ActionResult GetAllQuantity(string ID,string strType)
        {
            List<sp_GetMaintenanceDetailsQuantity_Result> lstInventory = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetDetailListQuantity(ID);
            List<sp_GetMaintenanceDetailsQuantity_Result> lstFinal = GetFinalItem(lstInventory);
            if (!CheckInventory(lstFinal, ID))
            {
                ViewData["EditError"] = Resources.Resource.msgDiffQuantity;
                return GetGridViewV2(lstFinal, Models.Helper.PartialParameter.MNApproveQuantity, strType.ToUpper());
            }
            else
            {
                bool isUpdate = new FEA_BusinessLogic.Maintenance.MNInventoryManager().UpdateFinalQuantity(lstFinal, ID);
                if (isUpdate)
                {
                    ObjectParameter outParam;
                    outParam = new FEA_BusinessLogic.Maintenance.MNInventoryManager().SendQuantityToEland(ID, Helper.UserLoginInfo.UserId);
                    if (outParam.Value.ToString() != "FAILED")
                    {
                        bool isUpdateStatus = new FEA_BusinessLogic.Maintenance.MNInventoryManager().UpdateRequestStatus(ID, 5, Helper.UserLoginInfo.UserId);
                        if (isUpdateStatus)
                        {
                            string url = Url.Action("ApprovePage", "Maintenance", new { iStatus = 1, strType=strType });
                            Response.Redirect(url);
                            return null;
                        }
                        else
                        {
                            string url = Url.Action("ApprovePage", "Maintenance", new { iStatus = 0, strType = strType });
                            Response.Redirect(url);
                            return null;
                        }
                    }
                   else
                   {
                       string url = Url.Action("ApprovePage", "Maintenance", new { iStatus = 0, strType = strType });
                       Response.Redirect(url);
                       return null;
                    }
                }
                else
                {
                    ViewData["EditError"] = Resources.Resource.msgErrorInputQuantity;
                    return GetGridViewV2(lstFinal, Models.Helper.PartialParameter.MNApproveQuantity, strType.ToUpper());
                }
            }
            return View();
        }

        public List<sp_GetMaintenanceDetailsQuantity_Result> GetFinalItem(List<sp_GetMaintenanceDetailsQuantity_Result> listInventoryQuantity)
        {
            sp_GetMaintenanceDetailsQuantity_Result lstItem;
            foreach (sp_GetMaintenanceDetailsQuantity_Result lst in listInventoryQuantity)
            {
                decimal PurQuantity, ITSQuantity, ERPQuantity, DiffQuantity;
                ITSQuantity = Convert.ToDecimal(lst.ITSInventory);
                ERPQuantity = Convert.ToDecimal(lst.ERPInventory);
                PurQuantity = Convert.ToDecimal(lst.PurchaseQuantity);
                DiffQuantity = Convert.ToDecimal(lst.RequestQuantity) - Convert.ToDecimal(lst.PurchaseQuantity);
                if (DiffQuantity == 0)
                {
                    ITSQuantity = 0;
                    ERPQuantity = 0;
                }
                else if (DiffQuantity <= ITSQuantity)
                {
                    ITSQuantity = DiffQuantity;
                    ERPQuantity = 0;
                }
                else if (ERPQuantity >= (DiffQuantity - ITSQuantity))
                {
                    ERPQuantity = DiffQuantity - ITSQuantity;
                }
                else
                {
                    PurQuantity = PurQuantity + (DiffQuantity - ITSQuantity);
                }
                lst.ITSInventory = ITSQuantity;
                lst.ERPInventory = ERPQuantity;
                lst.PurchaseQuantity = PurQuantity;
            }
            return listInventoryQuantity;
        }

        public bool CheckInventory(ICollection<sp_GetMaintenanceDetailsQuantity_Result> lstFinalQuantity, string ID)
        {
            List<sp_GetMaintenanceDetailsQuantity_Result> lstInventoryQuantity = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetDetailListQuantity(ID);
            foreach (sp_GetMaintenanceDetailsQuantity_Result lstFinal in lstFinalQuantity)
            {
                foreach (sp_GetMaintenanceDetailsQuantity_Result lstInven in lstInventoryQuantity)
                {
                    if ((lstFinal.ItemDetailID == lstInven.ItemDetailID) && ((lstFinal.ITSInventory > lstInven.ITSInventory) || (lstFinal.ERPInventory > lstInven.ERPInventory) || (lstFinal.RequestQuantity != (lstFinal.ITSInventory + lstFinal.ERPInventory + lstFinal.PurchaseQuantity))))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public ActionResult RejectMNRequest(string ID, string Comment, string strType)
        {
            MNRequestMain itemRequest = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetRequest(ID);
            string strComment = "MN Department:-->" + Comment;
            if (itemRequest != null)
            {
                int DelegateUserID, CheckUserID;
                string NodeID, MainID, MainDetailID, DelegateID;
                WFMain main = new WFMainManager().GetItem(itemRequest.OrderCode);
                MainID = main.MainID;
                MainDetailID = main.WFMainDetails.OrderByDescending(i => i.CheckDate).FirstOrDefault().MainDetailID;

                NodeID = "";
                CheckUserID = Helper.UserLoginInfo.UserId;
                DelegateID = "";
                DelegateUserID = 0;
                int status = WaitingAreaController.SignRejectDocument("Reject", "", strComment, itemRequest.DocType, main.User.CostCenterCode, NodeID, main.OrderCode, "", MainID, MainDetailID, "", main.CreateUserID.Value, CheckUserID, DelegateID, DelegateUserID, true);
            }
            List<sp_GetMaintenanceApproveDocument_Result> lst = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetDocumentForApprove(3, Helper.UserLoginInfo.UserId);
            return GetGridViewV2(lst, Models.Helper.PartialParameter.MNApproveDoc, strType.ToUpper());
        }
        #endregion

        #region Request management settings page
        public ActionResult RequestMainPage( string Type)
        {
            ViewBag.Type = Type.ToUpper();
            return View();
        }

        public ActionResult Index(string ID, string NodeID, string TypeUser, string MainDetailID,string MainID, int? CheckUserID, string DelegateID,int?DelegateUserID, int? editStatus, string strDocType)
        {
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/Maintenance/Index") });  
            if ( strDocType==null || strDocType.Trim().Length==0)
            {
                strDocType = Request.QueryString["Type"];
            }
            MNRequestMain mnRequestMainItem;
            User uInfo = new User();
            Boolean bIsCreateNew = false;
            Models.ItemUpload ItemUpload = new Models.ItemUpload();
            if (ID == null || ID.Trim().Length == 0)
            {
                uInfo = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
                mnRequestMainItem = new MNRequestMain()
                {
                    CreateDate = DateTime.Now,
                    DeliveryDate = DateTime.Now.AddMonths(1).AddDays(1 - DateTime.Now.Day),
                    CreatorID = Helper.UserLoginInfo.UserId,
                    Status = (int)FEA_BusinessLogic.Maintenance.MaintenanceManager.OrderStatus.NEW,
                    CurrencyID = "",
                    OrderCode = Controllers.HelperController.GenerateELandCode(Models.Helper.TagPrefixParameter.MAINTENANCE),
                    IsUrgent = 0,
                    ID = Guid.NewGuid().ToString()
                };
                ItemUpload.Guid = mnRequestMainItem.ID;
                Session[SessionTmpName] = new List<ItemInfo>();
                bIsCreateNew = true;
            }
            else
            {
                mnRequestMainItem = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetRequest(ID);
                ItemUpload.Guid = mnRequestMainItem.ID;
                if(System.IO.Directory.Exists(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root+mnRequestMainItem.AttachmentLink))&& mnRequestMainItem.AttachmentLink.Trim().Length!=0)
                {
                    string[] ListItem = null;
                    ListItem = mnRequestMainItem.AttachmentLink != null ? Directory.GetFiles(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + mnRequestMainItem.AttachmentLink)) : null;
                    ItemUpload.ListAddress = ListItem;
                }
                if(mnRequestMainItem!=null)
                {
                    uInfo = new UserManager().GetItem(mnRequestMainItem.CreatorID.Value);
                    Session[SessionTmpName] = ConvertToItemInfos(mnRequestMainItem.MNRequestMainDetails);
                    bIsCreateNew = false;
                }
            }
            if (editStatus != null)
                ViewBag.EditStatus = editStatus;
            ViewBag.Type = strDocType.ToUpper();
            ViewBag.User = uInfo;
            ViewBag.IsCreateNew = bIsCreateNew;
            ViewBag.NodeID = NodeID == null ? "" : NodeID;
            ViewBag.TypeUser = TypeUser == null ? "" : TypeUser;
            ViewBag.MainDetailID = MainDetailID == null ? "" : MainDetailID;
            ViewBag.MainID = MainID == null ? "" : MainID;
            ViewBag.CheckUserID = CheckUserID;
            ViewBag.DelegateID = DelegateID;
            ViewBag.DelegateUserID = DelegateUserID;
            ViewBag.ItemUploadGuiid = ItemUpload.Guid;
            Session["ItemUpload"] = ItemUpload;
            
            return View(mnRequestMainItem);
        }

        public ActionResult Management(string Type)
        {
            ViewBag.Type = Type.ToUpper();
            return View();
        }

        [ValidateInput(false)]
        public ActionResult MNRequestList( string strDocType)
        {
            ViewBag.Type = strDocType.ToUpper();
            List<MNRequestMain> lstItem = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetRequestByUser(Helper.UserLoginInfo.UserId);
            return GetGridViewV2(lstItem, Models.Helper.PartialParameter.MNRequestList,strDocType.ToUpper());
        }

        [HttpPost]
        public ActionResult AddItemToOrder(string ItemID, string ItemDetail, string Description, DateTime? DeliveryDate, string Price, string DocType, int? Quantity = 0)
        {
            if (ItemID.Trim().Length == 0 || ItemDetail.Trim().Length == 0
                || Price.Trim().Length == 0 || Convert.ToDecimal(Quantity) <= 0 || Convert.ToDecimal(Price) <= 0 || DeliveryDate == null || DeliveryDate.Value.Date < DateTime.Now.Date)
            {
                string _sErr = Resources.Resource.msgInputError;
                if (ItemID.Trim().Length == 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.ItemType);
                if (ItemDetail.Trim().Length == 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Specs);
                if (Price.Trim().Length == 0 || Convert.ToDecimal(Price) <= 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Price);
                if (Convert.ToDecimal(Quantity) <= 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Quantity);
                if (DeliveryDate == null || DeliveryDate.Value.Date < DateTime.Now.Date)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.DeliveryDate);

                ViewData["EditError"] = _sErr;
            }
            else
            {
                //var flag = CheckItemList(ItemID);
                var flag = true;
                if (!flag)
                    ViewData["EditError"] = Resources.Resource.msgInvalidItemInList;
                else
                {
                    flag = false;
                    if (FindItemDetailInList(ItemDetail)) //Kiểm tra Item đã tồn tại trong list yêu cầu
                    {
                        ViewData["EditError"] += "</br>" + Resources.Resource.msgDataduplicate;
                        flag = true;
                    }
                    if (!flag)
                    {
                        try
                        {
                            Item i = new ItemManager().GetItem(ItemID);
                            ItemDetail item_detail = new ItemDetailManager().GetItem(ItemDetail);
                            Unit unit = new UnitManager().GetItem(item_detail == null ? "" : item_detail.Temp2);
                            Models.ItemInfo item = new ItemInfo()
                            {
                                ID = Guid.NewGuid().ToString(),
                                ItemID = i.ID,
                                ItemName = i.ItemName,
                                ItemDetailID = item_detail.ID,
                                ItemDetailName = item_detail.ItemDetailName,
                                ReasonItemDetailName = item_detail.ItemDetailName,
                                Des = Description,

                                DeliveryDate = DeliveryDate,
                                UnitID = unit.ID,
                                UnitName = unit.NAME,
                                Quantity = Quantity.Value,
                                Price = decimal.Parse(Price, System.Globalization.CultureInfo.InvariantCulture),
                                Amount = Convert.ToDecimal(Quantity.Value * decimal.Parse(Price, System.Globalization.CultureInfo.InvariantCulture)),
                                ToTalReceiverByDept = 0,
                            };
                            item.QuanInStock = 0;
                            item.QuanNeeded = 0;
                            List<ItemInfo> lst = new List<ItemInfo>();
                            if (Session[SessionTmpName] != null)
                            {
                                lst = (List<ItemInfo>)Session[SessionTmpName];
                                lst.Add(item);
                                Session[SessionTmpName] = lst;
                            }
                            else
                            {
                                lst.Add(item);
                                Session[SessionTmpName] = lst;
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewData["EditError"] = ex.Message;
                        }
                    }
                }
            }
            ViewData["TotalAmount"] = GetTotalAmount();
            ViewData["ShowCommand"] = true;
            return GetGridViewV2(Session[SessionTmpName], Models.Helper.PartialParameter.MaintanceItemListGrid, DocType.ToUpper());
        }

        private bool CheckItemList(string ItemID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
            {
                lst = (List<ItemInfo>)Session[SessionTmpName];
                if (lst.Count == 0)
                    return true;
                else return lst.Where(i => i.ItemID == ItemID).Count() > 0 ? true : false;
            }
            else
                return true;
        }

        private bool FindItemDetailInList(string ItemDetailID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
                lst = (List<ItemInfo>)Session[SessionTmpName];
            return lst.Where(i => i.ItemDetailID == ItemDetailID).Count() > 0 ? true : false;
        }

        private bool FindItemDetailInListStock(string ItemDetailID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];
            return lst.Where(i => i.ItemDetailID == ItemDetailID).Count() > 0 ? true : false;
        }
        #endregion

        #region Stock management settings page
        public ActionResult StockMainPage(string Type, string OrderType)
        {
            ViewBag.Type = Type;
            ViewBag.OrderType = OrderType;
            return View();
        }

        public ActionResult StockRequest(string ID, int? editStatus, int OrderType, string Type, string strError)
        {
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/Maintenance/StockRequest") });

            MNStockEquipment mnStockItem;
            User uInfo = new User();
            Boolean bIsCreateNew = false;
            if(ID==null ||ID.Trim().Length==0)
            {
                uInfo = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
                mnStockItem = new MNStockEquipment()
                {
                    CreateDate = DateTime.Now,
                    Description = "",
                    CreatorID = Helper.UserLoginInfo.UserId,
                    Status = (int)FEA_BusinessLogic.Maintenance.StockManager.OrderStatus.NEW,
                    OrderCode = (OrderType == 1) ? Controllers.HelperController.GenerateCode(TagPrefixParameter.MAINTENANCESTOCKIN) : Controllers.HelperController.GenerateCode(TagPrefixParameter.MAINTENANCESTOCKOUT)
                };
                Session["ItemInfoList"] = new List<ItemInfo>();
                bIsCreateNew = true;
            }
           else
            {
                mnStockItem = new FEA_BusinessLogic.Maintenance.StockManager().GetItem(ID);
                if(mnStockItem!=null)
                {
                    uInfo = new UserManager().GetItem(mnStockItem.CreatorID.Value);
                    Session["ItemInfoList"] = StockConvertToItemInfos(mnStockItem.MNStockEquipmentDetails);
                    bIsCreateNew = false;
                }
            }
            if (editStatus != null)
                ViewBag.EditStatus = editStatus;
            ViewBag.User = uInfo;
            ViewBag.IsCreateNew = bIsCreateNew;
            ViewBag.OrderType = OrderType;
            if(!string.IsNullOrEmpty(strError))
            {
                ViewBag.ErrInfo= strError.Replace("\\n", "<br />");// Đổi kí tự \\n thành kí tự <br/>
            }
            ViewBag.Type = Type;
            return View(mnStockItem);
        }

        public ActionResult StockManagement(int OrderType, string Type)
        {
            ViewBag.Type = Type;
            ViewBag.OrderType = OrderType;
            return View();
        }

        public ActionResult GetStockList(int iOrderType, string strType)
        {
            ViewBag.OrderType = iOrderType;
            ViewBag.Type = strType.ToUpper();
            List<MNStockEquipment> lst = new FEA_BusinessLogic.Maintenance.StockManager().GetItemByUser(iOrderType, strType, Helper.UserLoginInfo.UserId);
            return GetGridViewV2(lst, Models.Helper.PartialParameter.MNStockList, strType.ToUpper());
        }
       
        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteStockRequestPartial(string ID, string strType, int iOrderType)
        {
            List<MNStockEquipment> lstItem=new List<MNStockEquipment>();
            if(ID.Length!=0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.Maintenance.StockManager().DeleteItem(ID, Helper.UserLoginInfo.UserId);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteFail;
                    ViewBag.OrderType = iOrderType;
                    ViewBag.Type = strType.ToUpper();
                    lstItem = new FEA_BusinessLogic.Maintenance.StockManager().GetItemByUser(iOrderType,strType,Helper.UserLoginInfo.UserId);
                }
                catch(Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridViewV2(lstItem, Models.Helper.PartialParameter.MNStockList, strType.ToUpper());
        }

        public ActionResult UserListPartial(MNStockEquipment model)
        {
            return PartialView("_UserListPartial", model);
        }

        private List<ItemInfo> StockConvertToItemInfos(ICollection<MNStockEquipmentDetail> mnStockEquipmentDetail)
        {
            List<ItemInfo> lstResult = new List<ItemInfo>();
            ItemInfo _itemInfo;
            foreach(MNStockEquipmentDetail mnStockDetailItem in mnStockEquipmentDetail)
            {
                _itemInfo = new ItemInfo()
                {
                    ID = mnStockDetailItem.DetailID,
                    ItemID = mnStockDetailItem.ItemID,
                    ItemName = mnStockDetailItem.Item.ItemName,
                    ItemDetailID = mnStockDetailItem.ItemDetailID,
                    ItemDetailName = mnStockDetailItem.ItemDetail.ItemDetailName,
                    Des = mnStockDetailItem.Description,
                    Quantity = Convert.ToInt16( mnStockDetailItem.Quantity.Value),
                    UnitID = mnStockDetailItem.UnitID,
                    DeliveryDate = mnStockDetailItem.DeliveryDate,
                    UnitName = mnStockDetailItem.Unit.NAME,
                    ReceivedUserID=(mnStockDetailItem.ReceiveUserID==null)?0: mnStockDetailItem.ReceiveUserID.Value
                };
                lstResult.Add(_itemInfo);
            }
            return lstResult;
        }

        [HttpPost]
        public ActionResult StockAddItemToOrder(string ItemID, string ItemDetail, string Description, int iOrderType, string DocType,int iReceivedUserID,string strReceivedUserName , DateTime ? DeliveryDate, int ? Quantity=0)
        {
            if(ItemID.Trim().Length==0|| ItemDetail.Trim().Length==0
                || Convert.ToDecimal(Quantity)<=0 || DeliveryDate ==null || DeliveryDate.Value.Date <DateTime.Now.Date)
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
                var flag = FindItemDetailInListStock(ItemDetail);
                if (flag)
                    ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                else
                {
                    try
                    {
                        FEA_BusinessLogic.Unit unit;
                        Item i = new ItemManager().GetItem(ItemID);
                        ItemDetail item_detail = new ItemDetailManager().GetItem(ItemDetail);
                        unit = new UnitManager().GetItem(item_detail == null ? "" : item_detail.Temp2);
                        string sError = "";
                        var checkInventoryStock = CheckInventoryInStock(item_detail.ID, item_detail.ItemDetailName, unit.ID, unit.NAME, Quantity.Value, ref sError);
                        if (iOrderType == 2 && !checkInventoryStock)
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
                                ReceivedUserID = iReceivedUserID,
                                ReceivedUserName = strReceivedUserName
                            };

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
            ViewBag.Type = DocType.ToUpper();
            ViewBag.OrderType = iOrderType;
            return GetGridViewV2(Session["ItemInfoList"], Models.Helper.PartialParameter.MaintanceItemListGrid, DocType.ToUpper());
        }

        [HttpPost]
        public ActionResult StockSaveData([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] MNStockEquipment o, bool CreateNew)
        {
            string sResult = "";
            string sMessage = "";
            bool ErrInventory = false;
            string strErrorInfo="";
            int editStatus = (int)Models.Helper.EditItemStatus.failed;
            if (Request.Form["btnSaveDraft"] != null)
            {
                if (o.OrderType == 2)
                {
                    o.DeliveryFor = Convert.ToInt16(DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboUserList"));
                    o.RequestMainID = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboRequestList");
                }
                sResult = CreateStockOrder(o, true);
            }
            else if (Request.Form["btnConfirm"] != null)
                if (CreateNew)
                    sResult = CreateStockOrder(o, false);
                else
                {      
                    if(o.OrderType==2)
                    {
                        BaseJsonResult baseJson = CheckListInventoryInStock();
                        if(baseJson.ErrorCode==1)
                        {
                            strErrorInfo = baseJson.Message;           
                            sResult = o.ID == null ? "" : o.ID;
                            ErrInventory = true;
                        }
                        else
                        {
                            sResult = UpdateStockOrder(o, false);
                        }
                    }
                    else
                    {
                        sResult = UpdateStockOrder(o, false);
                    }
                }
                    
            else if(Request.Form["btnUpdate"]!=null)
            {
                sResult = UpdateStockOrder(o, true);
            }
            else if(Request.Form["btnCancelConfirm"]!=null)
            {
                if(o.OrderType==1)
                {
                    List<sp_CheckCancelConfirmMNStockIn_Result> checkItem = new FEA_BusinessLogic.Maintenance.MNInventoryManager().CheckCancelConfirm(o.OrderCode);
                    if(checkItem.Count!=0)
                    {
                        foreach(sp_CheckCancelConfirmMNStockIn_Result item in checkItem)
                        {
                            if(item.DifferentQuantity<0)
                            {
                                sMessage += "\\n"+ Resources.Resource.NotEnough +"\\n"+ Resources.Resource.ItemNameDetail + ":" + item.ItemDetailName + "" + Resources.Resource.Quantity + ": " + item.DifferentQuantity + "";
                            }
                        }
                        sResult = "";
                    }
                    else
                    {
                        sResult = UpdateCancelStockItem(o, false);
                    }
                }
                else
                {
                    sResult = UpdateCancelStockItem(o, false);
                }
            }
           
            if (ErrInventory)
            {
                return RedirectToAction("StockRequest", new { ID = sResult, editStatus = (int)Models.Helper.EditItemStatus.failed, OrderType = o.OrderType, Type = o.DocType, strError = strErrorInfo });
            }
            else
            {
                if (sResult.Length > 0)
                    return RedirectToAction("StockRequest", new { ID = sResult, editStatus = (int)Models.Helper.EditItemStatus.success, OrderType = o.OrderType, Type = o.DocType });
                else
                    return RedirectToAction("StockRequest", new { ID = o.ID, editStatus = (int)Models.Helper.EditItemStatus.failed, OrderType = o.OrderType, Type = o.DocType, strError =sMessage});
            }      
        }

        private string CreateStockOrder(MNStockEquipment o, bool isSaveDraft)
        {
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.MNStockEquipmentDetails = StockEquipmentDetailList();
            o.OrderCode = (o.OrderType == 1) ? Controllers.HelperController.GenerateCode(TagPrefixParameter.MAINTENANCESTOCKIN) : Controllers.HelperController.GenerateCode(TagPrefixParameter.MAINTENANCESTOCKOUT);
            if (isSaveDraft) 
                o.Status = (int)FEA_BusinessLogic.Maintenance.MaintenanceManager.OrderStatus.DRAFT;
            else
            {
                o.Status = (int)FEA_BusinessLogic.Maintenance.MaintenanceManager.OrderStatus.FINSHED;
            }
            string sID = new FEA_BusinessLogic.Maintenance.StockManager().InsertItem(o, isSaveDraft);
            return sID;
        }

        private List<MNStockEquipmentDetail> StockEquipmentDetailList()
        {
            List<MNStockEquipmentDetail> lst = new List<MNStockEquipmentDetail>();
            if (Session["ItemInfoList"] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session["ItemInfoList"];
                MNStockEquipmentDetail stockDetail;
                foreach (ItemInfo i in lstItemInfo)
                {
                    stockDetail = new MNStockEquipmentDetail()
                    {
                        Description = i.Des,
                        ItemDetailID = i.ItemDetailID,
                        ItemID = i.ItemID,
                        Quantity = i.Quantity,
                        UnitID = i.UnitID,
                        DeliveryDate = i.DeliveryDate,
                        ReceiveUserID=i.ReceivedUserID
                        
                    };
                    lst.Add(stockDetail);
                }
            }
            return lst;
        }

        private string UpdateCancelStockItem(MNStockEquipment o, bool isSaveDraft)
        {
            o.MNStockEquipmentDetails = CancelStockEquipmentDetailList(Convert.ToInt16(o.OrderType));
            bool _result = new FEA_BusinessLogic.Maintenance.StockManager().CancelStockConfirmItem(o, isSaveDraft, i => i.Status);
            return _result == true ? o.ID : "";
        }

        private List<MNStockEquipmentDetail> CancelStockEquipmentDetailList(int iOrderType)
        {
            List<MNStockEquipmentDetail> lst = new List<MNStockEquipmentDetail>();
            if(Session["ItemInfoList"]!=null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session["ItemInfoList"];
                MNStockEquipmentDetail mnStockDetail;
                foreach(ItemInfo i in lstItemInfo)
                {
                    mnStockDetail = new MNStockEquipmentDetail()
                    {
                        Description = i.Des,
                        ItemDetailID = i.ItemDetailID,
                        ItemID = i.ItemID,
                        Quantity = (iOrderType == 1) ? i.Quantity * -1 : i.Quantity,
                        UnitID = i.UnitID,
                        DeliveryDate = i.DeliveryDate
                    };
                    lst.Add(mnStockDetail);
                }
            }
            return lst;
        }

        private string UpdateStockOrder(MNStockEquipment o, bool isSaveDraft)
        {
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.MNStockEquipmentDetails = StockEquipmentDetailList();
            bool _result = new FEA_BusinessLogic.Maintenance.StockManager().UpdateItem(o, isSaveDraft, i => i.Description);

            return _result == true ? o.ID : "";
        }
        
        public BaseJsonResult CheckListInventoryInStock()
        {
            BaseJsonResult result = new BaseJsonResult() { ErrorCode = 0, Message = "" };
            bool hasError = false;
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];
            string message = "";
            if(lst.Count==0)
            {
                hasError = true;
                message = Resources.Resource.msgSelectProductItem;
            }
            else
            {
                foreach(ItemInfo item in lst)
                {
                    string _sError = "";
                    var flag = CheckInventoryInStock(item.ItemDetailID, item.ItemDetailName, item.UnitID, item.UnitName, item.Quantity, ref _sError);
                    if(!flag)
                    {
                        hasError = true;
                        message += "\\n" + _sError ;
                    }
                }
            }
            if(hasError)
            {
                result.ErrorCode = 1;
                result.Message = message;
            }
            return result;
        }
       
        private bool CheckInventoryInStock (string ItemDetailID, string ItemDetailName, string UnitID,string UnitName, int Quantity, ref string sError)
        {
            int itemInStock = new FEA_BusinessLogic.Maintenance.MNInventoryManager().CountInStock(ItemDetailID, UnitID);
            var flag = (Quantity > itemInStock) ? false : true;
            if(!flag)
            {
                sError = string.Format(@"[{0}] không đủ số lượng, số lượng hiện tại chỉ còn : {1}({2})", ItemDetailName, itemInStock, UnitName);
                return false;            
            }
            return true;
        }
        #endregion

        #region Inventory management settings page
        public ActionResult InventoryMainPage()
        {
            return View();
        }

        public ActionResult MNInventory(string Type)
        {
            return View();
        }

        public ActionResult MNDynamicQuery(string Type)
        {
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/Maintenance/MNDynamicQuery?Type" + Type) });
            ViewBag.DocumentType = Type;
            return View();
        }

        public ActionResult CallbackMNDynamicInventory(FormCollection frm, string DateTo, string DateFrom, string Type)
        {
            DateTo = Convert.ToDateTime(DateTo).ToString("yyyy-MM-dd");
            DateFrom = Convert.ToDateTime(DateFrom).ToString("yyyy-MM-dd");
            ViewBag.DateFrom = DateFrom;
            ViewBag.DateTo = DateTo;
            ViewBag.DocumentType = Type;
            if (String.IsNullOrEmpty(DateFrom) || DateTo == "0001-01-01")
            {
                return GetGridViewV2(null, FEA_ITS_Site.Models.Helper.PartialParameter.QueryMNDynamicInventory, Type);
            }
            else
            {
                return GetGridViewV2(new FEA_BusinessLogic.Maintenance.MNInventoryManager().GetMnDynamicInventory(DateTo, DateFrom, Type), FEA_ITS_Site.Models.Helper.PartialParameter.QueryMNDynamicInventory, Type);
            }
        }

        public ActionResult ExportToExcel(FormCollection frm, string DateTo, string DateFrom, string Type)
        {
            DateFrom = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("date").ToString("yyyy-MM-dd");
            DateTo = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("date2").ToString("yyyy-MM-dd"); 
            ViewBag.DateFrom = DateFrom;
            ViewBag.DateTo = DateTo;
            ViewBag.DocumentType = Type;
            return GridViewExtension.ExportToXlsx(CreateExportGridViewSettings()
                , new FEA_BusinessLogic.Maintenance.MNInventoryManager().GetMnDynamicInventory(DateTo, DateFrom, Type)
                , "Maintenance Inventory Report - " + DateTime.Now.Year + " - " + DateTime.Now.Month
                , true);
        }

        public static DevExpress.Web.Mvc.GridViewSettings CreateExportGridViewSettings()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "QueryDynamicInventory";
            settings.KeyFieldName = "ItemDetailID";
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.CommandColumn.Visible = false;
            settings.Settings.VerticalScrollBarMode = DevExpress.Web.ScrollBarMode.Auto;
            settings.Settings.VerticalScrollableHeight = 450;
            settings.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowFilterRowMenuLikeItem = true;
            settings.Settings.ShowGroupPanel = true;
            settings.SettingsDetail.ShowDetailRow = true;
            settings.SettingsBehavior.AutoExpandAllGroups = true;
            return settings;
        }

        public ActionResult GetMNInventory()
        {
            List<sp_CheckMaintenanceInventory_Result> lst = new FEA_BusinessLogic.Maintenance.MNInventoryManager().GetInventory();
            return GetGridViewV2(lst, Models.Helper.PartialParameter.MNInventory, "");
        }
        #endregion

        #region Grid views settings
        public ActionResult GetItemDetailList(bool? ShowCommand, string DocType)
        {
            ViewData["ShowCommand"] = ShowCommand;
            ViewBag.Type = DocType;
            return GetGridViewV2((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.MaintanceItemListGrid, DocType.ToUpper());
        }
       
        public ActionResult GetStockItemDetailList(bool? ShowCommand, string DocType, int iOrderType)
        {
            ViewData["ShowCommand"] = ShowCommand;
            ViewBag.Type = DocType;
            ViewBag.OrderType = iOrderType;
            return GetGridViewV2((List<ItemInfo>)Session["ItemInfoList"], Models.Helper.PartialParameter.MaintanceItemListGrid, DocType.ToUpper());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteItemDetail(string ID, string strDocType)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
                lst = (List<ItemInfo>)Session[SessionTmpName];
            ItemInfo item = lst.Where(i => i.ID == ID).SingleOrDefault();
            lst.Remove(item);

            Session[SessionTmpName] = lst;
            ViewData["TotalAmount"] = GetTotalAmount();
            ViewData["ShowCommand"] = true;
            return GetGridViewV2((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.MaintanceItemListGrid, strDocType.ToUpper());
        }

        [HttpPost,ValidateInput(false)]
        public ActionResult DeleteStockItemDetail(string ID, string strDocType)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];
            ItemInfo item = lst.Where(i => i.ID == ID).SingleOrDefault();
            lst.Remove(item);

            Session["ItemInfoList"] = lst;
            ViewData["ShowCommand"] = true;
            return GetGridViewV2((List<ItemInfo>)Session["ItemInfoList"], Models.Helper.PartialParameter.MaintanceItemListGrid, strDocType.ToUpper());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteRequestList(string ID, string strDocType)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.Maintenance.MaintenanceManager().DeleteRequest(ID);
                    if (!flag)
                        ViewData["DeleteError"] = @Resources.Resource.msgDeleteDeviceRegFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridViewV2(new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetRequestByUser(Helper.UserLoginInfo.UserId), FEA_ITS_Site.Models.Helper.PartialParameter.MNRequestList, strDocType.ToUpper());
        }

        private decimal GetTotalAmount()
        {
            decimal _result = 0;
            if (Session[SessionTmpName] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session[SessionTmpName];
                foreach (ItemInfo i in lstItemInfo)
                {
                    _result += i.Amount;
                }
            }
            return _result;
        }
        #endregion
    
        #region Handle data
        public ActionResult SaveData([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))]MNRequestMain o,bool CreateNew)
        {
            string sResult = "";
            if (Request.Form["btnSaveDraft"] != null)
                sResult = CreateMaintenanceRequest(o, true);
            else if (Request.Form["btnSaveAndSend"] != null)
                if (CreateNew)
                    sResult = CreateMaintenanceRequest(o, false);
                else
                    sResult = UpdateMNRequest(o, false);
            else if(Request.Form["btnUpdate"]!=null)
            {
                sResult = UpdateMNRequest(o, true);
            }
            if (sResult.Length > 0)
                return RedirectToAction("Index", new { ID = sResult, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.success, strDocType =o.DocType});
            else
                return RedirectToAction("Index", new { ID = sResult, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.failed, strDocType = o.DocType });
            return View();
        }
        
        private string CreateMaintenanceRequest(MNRequestMain o,bool isSaveDraft)
        {
            string _sApproverUser = "";
            WFMain wfMain = new WFMain();
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.MNRequestMainDetails = MNRequestMainDetailList();
            o.OrderCode = Controllers.HelperController.GenerateELandCode(TagPrefixParameter.MAINTENANCE);
            o.AttachmentLink = FEA_ITS_Site.Helper.Ultilities.UploadFolder + o.ID;
            if (isSaveDraft)
                o.Status = (int)FEA_BusinessLogic.Maintenance.MaintenanceManager.OrderStatus.DRAFT;
            else
            {
                o.Status = (int)FEA_BusinessLogic.Maintenance.MaintenanceManager.OrderStatus.SENDING;
                _sApproverUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
                wfMain = GetWFMain(o.OrderCode, _sApproverUser);
            }
            if (!isSaveDraft && _sApproverUser.Length > 0)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApproverUser), o.CreatorID.Value, o.Reason, "");
            string sID = new FEA_BusinessLogic.Maintenance.MaintenanceManager().InsertItem(o, (FEA_BusinessLogic.Maintenance.MaintenanceManager.OrderStatus)o.Status, wfMain);
            return sID;
        }
       
        private List<MNRequestMainDetail> MNRequestMainDetailList()
        {
            List<MNRequestMainDetail> lst = new List<MNRequestMainDetail>();
            if(Session[SessionTmpName]!=null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session[SessionTmpName];
                MNRequestMainDetail mnDetail;
                foreach(ItemInfo i in lstItemInfo)
                {
                    mnDetail = new MNRequestMainDetail()
                    {
                        Description = i.Des,
                        ItemNo = 0,
                        ItemDetailID = i.ItemDetailID,
                        ItemID = i.ItemID,
                        Quantity = i.Quantity,
                        UnitID = i.UnitID,
                        DeliveryDate = i.DeliveryDate,
                        EstimatePrice = i.Price,
                        EstimateAmount = i.Amount
                    };
                    lst.Add(mnDetail);
                }
            }
            return lst;
        }
        
        private WFMain GetWFMain(string sOrderCode, string _sApproverUser)
        {
            WFMain wfMain = new WFMain()
            {
                MainID = Guid.NewGuid().ToString(),
                OrderCode = sOrderCode,
                DocumentTypeID = Session["DocumentTypeID"] == null ? "" : Session["DocumentTypeID"].ToString(),
                isFinished = 0,
                CreateUserID = Helper.UserLoginInfo.UserId,
                CreateDate = DateTime.Now,
                Temp1 = "",
                Temp2 = "",
                Temp3 = ""
            };
            wfMain.WFMainDetails.Add(
                new WFMainDetail()
                {
                    MainDetailID = Guid.NewGuid().ToString(),
                    MainID = wfMain.MainID,
                    NodeID = Session["NodeID"] == null ? "" : Session["NodeID"].ToString(),
                    DelegateID = null,
                    PostUserID = Helper.UserLoginInfo.UserId,
                    CheckUserID = int.Parse(_sApproverUser),
                    isFinished = 0,
                    Comment = "",
                    Temp1 = "0",
                    Temp2 = "",
                    Temp3 = ""
                });
            return wfMain;
        }

        private string UpdateMNRequest(MNRequestMain o, bool isSaveDarft)
        {
            string _sApprovedUser = "";
            WFMainDetail wfMainDetail = new WFMainDetail();
            bool isReturned = (o.Status == (int)FEA_BusinessLogic.Maintenance.MaintenanceManager.OrderStatus.RETURNED) ? true : false;

            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.MNRequestMainDetails = MNRequestMainDetailList();
            if(!isSaveDarft)
            {
                _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
                WFMain wfMain = new WFMainManager().GetItem(o.OrderCode);
                if(wfMain ==null)
                {
                    wfMain = new WFMain()
                    {
                        MainID = Guid.NewGuid().ToString(),
                        OrderCode = o.OrderCode,
                        DocumentTypeID = Session["DocumentTypeID"] == null ? "" : Session["DocumentTypeID"].ToString(),
                        isFinished = 0,
                        CreateUserID = Helper.UserLoginInfo.UserId,
                        CreateDate = DateTime.Now,
                        Temp1 = "0",
                        Temp2 = "",
                        Temp3 = ""
                    };
                    bool flag = new WFMainManager().InsertItem(wfMain);
                }
                wfMainDetail = new WFMainDetail()
                {
                    MainDetailID = Guid.NewGuid().ToString(),
                    MainID = wfMain.MainID,
                    NodeID = Session["NodeID"] == null ? "" : Session["NodeID"].ToString(),
                    DelegateID = null,
                    PostUserID = Helper.UserLoginInfo.UserId,
                    CheckUserID = int.Parse(_sApprovedUser),
                    isFinished = 0,
                    Comment = "",
                    Temp1 = "",
                    Temp2 = "",
                    Temp3 = ""
                };
            }
            bool _result = new FEA_BusinessLogic.Maintenance.MaintenanceManager().UpdateItem(o, isSaveDarft, isReturned, wfMainDetail, i => i.Description, i => i.Reason, i => i.CreateDate
                , i => i.IsUrgent, i => i.CurrencyID, i => i.EstimatedAmount);
            if (!isSaveDarft && _result)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Reason, "");
            return _result == true ? o.ID : "";
        }

        private List<ItemInfo> ConvertToItemInfos(ICollection<MNRequestMainDetail> MNRequestDetail)
        {
            List<ItemInfo> lstResult = new List<ItemInfo>();
            ItemInfo _itemInfo;
            foreach (MNRequestMainDetail MNDetailItem in MNRequestDetail)
            {
                _itemInfo = new ItemInfo()
                {
                    ID = MNDetailItem.DetailID,
                    ItemID = MNDetailItem.ItemID,
                    ItemName = MNDetailItem.Item.ItemName,
                    ItemDetailID = MNDetailItem.ItemDetailID,
                    ItemDetailName = MNDetailItem.ItemDetail.ItemDetailName,
                    Des = MNDetailItem.Description,
                    Quantity = Convert.ToInt16(MNDetailItem.Quantity.Value),
                    UnitID = MNDetailItem.UnitID,
                    DeliveryDate = MNDetailItem.DeliveryDate,
                    Price = MNDetailItem.EstimatePrice.Value,
                    Amount = MNDetailItem.EstimateAmount.Value,
                    UnitName = MNDetailItem.Unit.NAME,
                    ToTalReceiverByDept = 0
                };
                _itemInfo.QuanInStock = 0;
                _itemInfo.QuanNeeded = 0;
                lstResult.Add(_itemInfo);
            }
            return lstResult;
        }
        #endregion

        #region Partial
        // Lấy các item từ bản Item dựa vào mã request main ID
        public ActionResult ItemPartialRequest(MNStockEquipment model)
        {
            string requestMainID = (Request.Params["RequestMainID"] != null) ? Request.Params["RequestMainID"].ToString() : (model.RequestMainID!=null ? model.RequestMainID: "-1");
            List<Item> lst = new ItemManager().GetRequestItem(requestMainID);
            return PartialView("_ItemPartial", lst);
        }
        // Lấy tên item từ bảng ItemDetail dựa vào mã yêu cầu và ItemID
        public ActionResult ItemDetailPartialRequest(MNStockEquipment model)
        {
            string requestMainID = (Request.Params["RequestMainID"] != null) ? Request.Params["RequestMainID"].ToString() : (model.RequestMainID!=null ? model.RequestMainID: "-1");
            string itemID = (Request.Params["ItemID"] != null) ? Request.Params["ItemID"].ToString() : "-1";
            List<sp_GetItemDetailName_Result> lst = new ItemManager().GetRequestItemDetail(requestMainID, itemID);
            return PartialView("_ItemDetailPartial", lst);
        }
        //Lấy list yêu cầu với điều kiện yêu cầu đó đã được bộ phận bảo trì duyệt.
        public ActionResult RequestListPartial(MNStockEquipment model)
        {
            return PartialView("_RequestListPartial", model);
        }
        #endregion

        #region Query & Statistic
        public ActionResult MNQuery(string Type)
        {
            ViewBag.DocumentType = Type;
            ViewBag.DateFrom = ViewBag.DateTo = DateTime.Now;
            return View();
        }

        public ActionResult MNQueryClick(string OrderCode, string DateFrom, string DateTo, string DocumentType)
        {
            DateTo = Convert.ToDateTime(DateTo).ToString("yyyy-MM-dd");
            DateFrom = Convert.ToDateTime(DateFrom).ToString("yyyy-MM-dd");
            ViewBag.DateFrom = DateFrom;
            ViewBag.DateTo = DateTo;
            ViewBag.DocumentType = DocumentType;
            ViewBag.OrderCode = OrderCode =="null" ? "" : OrderCode;
            if (String.IsNullOrEmpty(DateFrom) || DateTo == "0001-01-01")
            {
                return GetGridViewV2(null, FEA_ITS_Site.Models.Helper.PartialParameter.MNQueryRequestList, DocumentType);
            }
            else
            {
                return GetGridViewV2(new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetMNRequestList(ViewBag.OrderCode, ViewBag.DateFrom, ViewBag.DateTo, DocumentType)
                    , FEA_ITS_Site.Models.Helper.PartialParameter.MNQueryRequestList
                    , DocumentType);
            }
        }

        public ActionResult MNStatistic(string Type)
        {
            ViewBag.DateFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
            ViewBag.DateTo = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.DocumentType = Type;
            return View();
        }

        public ActionResult PivotExportToExcel(string DateFrom, string DateTo, string DocumentType)
        {
          //  DateTo = Convert.ToDateTime(DateTo).ToString("yyyy-MM-dd");
          //  DateFrom = Convert.ToDateTime(DateFrom).ToString("yyyy-MM-dd");
           DateFrom=  DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtBeginDate").ToString("yyyy-MM-dd");
           DateTo = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").ToString("yyyy-MM-dd"); 
                   
            ViewBag.DateFrom = DateFrom;
            ViewBag.DateTo = DateTo;
            return ExportTo(DateFrom, DateTo, DocumentType);
        }
        
        public ActionResult ExportTo(string DateFrom, string DateTo, string DocumentType)
        {
            List<sp_GetMNSummaryReport_Result> _object = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetMNSummaryReport(DateFrom, DateTo, DocumentType).ToList();

            PrintingSystem ps = new PrintingSystem();
            ps.XlsxDocumentCreated += new XlsxDocumentCreatedEventHandler(PrintingSystem_XlsxDocumentCreated); // most important part

            CompositeLink complink = new CompositeLink(ps);
            PrintableComponentLink link1 = new PrintableComponentLink();
            PivotGridSettings PivotSetting = GetSettings(DateFrom, DateTo, DocumentType);
            link1.Component = PivotGridExtension.CreatePrintableObject(PivotSetting, _object);
            complink.Links.Add(link1);
            complink.CreatePageForEachLink();
            FileStreamResult result = CreateExcelExportResult(complink);
            return result;
        }

        FileStreamResult CreateExcelExportResult(CompositeLink link)
        {
            MemoryStream stream = new MemoryStream();

            XlsxExportOptions opt = new XlsxExportOptions() { ExportMode = XlsxExportMode.SingleFilePageByPage };
            link.PrintingSystem.ExportToXlsx(stream, opt);
            stream.Position = 0;
            FileStreamResult result = new FileStreamResult(stream, "application/xls");
            result.FileDownloadName = "MN Statistic " + string.Format("{0}-{1}", DateTime.Now.Month, DateTime.Now.Year) + "_" + DateTime.Now.Year + "年" + DateTime.Now.Month + "月進入存報告" + ".xlsx";
            return result;
        }
        // and event where you change sheetname
        void PrintingSystem_XlsxDocumentCreated(object sender, XlsxDocumentCreatedEventArgs e)
        {

            if (e.SheetNames != null)
            {
                e.SheetNames[0] = "MN Statistic";

            }
        }

        public ActionResult MNStatisticClick(string DateFrom, string DateTo, string DocumentType)
        {
            DateTo = Convert.ToDateTime(DateTo).ToString("yyyy-MM-dd");
            DateFrom = Convert.ToDateTime(DateFrom).ToString("yyyy-MM-dd");
            ViewBag.DateFrom = DateFrom;
            ViewBag.DateTo = DateTo;
            ViewBag.DocumentType = DocumentType;

            if (String.IsNullOrEmpty(DateFrom) || DateTo == "0001-01-01")
            {
                //Session["MNSummaryReport"] = null;
                return PartialView("_MNPivot", null);
            }
            else
            {
                List<sp_GetMNSummaryReport_Result> model = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetMNSummaryReport(DateFrom, DateTo, DocumentType).ToList();
               // Session["MNSummaryReport"] = model;
                return PartialView("_MNPivot", model);
            }

        }

        public PivotGridSettings GetSettings(string DateFrom, string DateTo, string DocumentType)
        {

            PivotGridSettings settings = new PivotGridSettings();
            settings.Name = "PivotGrid1";
            settings.OptionsView.ShowFilterHeaders = false;
            settings.OptionsView.ShowContextMenus = false;
            settings.OptionsView.ShowFilterSeparatorBar = false;

            //settings.OptionsCustomization.AllowExpandOnDoubleClick = false;
            //settings.OptionsView.ShowFilterHeaders = false;
            //settings.OptionsView.ShowContextMenus = false;
            //settings.OptionsView.ShowFilterSeparatorBar = false;
            //settings.OptionsData.AutoExpandGroups = DefaultBoolean.False;
            settings.CallbackRouteValues = new { Controller = "Maintenance", Action = "MNStatisticClick", DocumentType = DocumentType, DateFrom = DateFrom, DateTo = DateTo };

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = @Resources.Resource.ItemNameDetail;
                field.FieldName = "ItemDetailName";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 1;
                field.Caption = @Resources.Resource.Unit;
                field.FieldName = "UnitName";
                field.SortMode = PivotSortMode.None;
            });


            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.ColumnArea;
                field.AreaIndex = 0;
                field.Caption = @Resources.Resource.Department;
                field.FieldName = "Dept";
                field.SortMode = PivotSortMode.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 0;
                field.Caption = @Resources.Resource.Quantity;
                field.FieldName = "Quantity";
                field.CellFormat.FormatString = "##,#";
                field.CellFormat.FormatType = FormatType.Custom;
            });

            settings.FieldValueDisplayText = (sender, e) =>
            {
                if (e != null)
                {
                    if (e.DisplayText == "Grand Total")
                        e.DisplayText = "Grand Total";
                }
            };

            return settings;
            
        }

        #endregion
              
        #region Sign-reject
        public ActionResult SignDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] MNRequestMain o)
        {
            int Status = 0;
            string _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            MNRequestMain mnitem = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetRequest(o.ID);
            if (new FEA_BusinessLogic.WaitingArea.WaitingArea().CheckAlreadySigned(MainDetailID) == 0)
            {
                Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Sign", _sApprovedUser, comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.MAINTENANCE, mnitem.User1.CostCenterCode, NodeID /*current node*/, o.OrderCode, mnitem.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, mnitem.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);
            }
            else
            {
                Status = 0;
            }
            return RedirectToAction("Index", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID, strDocType =o.DocType});
        }
        
        public ActionResult RejectDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] MNRequestMain o)
        {
            int Status = 0;
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
            MNRequestMain mnitem = new FEA_BusinessLogic.Maintenance.MaintenanceManager().GetRequest(o.ID);
            Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Reject", "", comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.MAINTENANCE, mnitem.User1.CostCenterCode, NodeID  /*current node*/, o.OrderCode, mnitem.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, mnitem.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);
            return RedirectToAction("Index", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID, strDocType=o.DocType });
        }
        #endregion
    }
}
