using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Data.PivotGrid;
using DevExpress.Utils;
using DevExpress.Web.Mvc;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrintingLinks;
using FEA_BusinessLogic;
using FEA_GABusinessLogic;
using FEA_ITS_Site.Models;
using FEA_ITS_Site.Models.Helper;
using System.Data.Objects;
// Add by Tony (2016-09-21)
using System.Threading;
using System.Threading.Tasks;
//
namespace FEA_ITS_Site.Controllers
{
    public class GAController : BaseController
    {
        //
        // GET: /GA/
        public string SessionTmpName = "ItemInfoListGA";
        public ActionResult MainPage()
        {

            return View();
        }

        #region Application
        public ActionResult Index(string ID, string NodeID, string TypeUser, string MainDetailID, string MainID, int? CheckUserID, string DelegateID, int? DelegateUserID, int? editStatus)
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/GA/Index") });
            //

            GAItem gaItem;
            User uInfo = new User();
            Boolean bIsCreateNew = false;
            FEA_ITS_Site.Models.ItemUpload ItemUpload = new ItemUpload();

            if (ID == null || ID.Trim().Length == 0) // Create New
            {
                // Get User Logged info
                uInfo = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
                // get DeviceReg Info
                gaItem = new GAItem()
                {
                    CreateDate = DateTime.Now,
                    DeliveryDate =DateTime.Now.AddMonths(1).AddDays(1-DateTime.Now.Day),
                    CreatorID = Helper.UserLoginInfo.UserId,
                    Status = (int)HardwareRequirementManager.OrderStatus.NEW,
                    CurrencyID = "",
                    OrderCode = Controllers.HelperController.GenerateELandCode(TagPrefixParameter.GENERALAFFAIR),
                    IsUrgent = 0,
                    ID = Guid.NewGuid().ToString()
                };
                ItemUpload.Guid = gaItem.ID;
                // Item Detail
                Session[SessionTmpName] = new List<ItemInfo>();
                bIsCreateNew = true;
            }
            else // Load Info and show to View Page
            {
                gaItem = new GAItemManager().GetItem(ID);
                ItemUpload.Guid = gaItem.ID;
                if (System.IO.Directory.Exists(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + gaItem.AttachmentLink)) && gaItem.AttachmentLink.Trim().Length != 0)
                {
                    string[] ListItem = null;// Directory.GetFiles(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + deRegItem.AttachmentLink));     // Please test again.
                    ListItem = gaItem.AttachmentLink != null ? Directory.GetFiles(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + gaItem.AttachmentLink)) : null;
                    ItemUpload.ListAddress = ListItem;
                }
                if (gaItem != null)
                {
                    // Get User Logged info
                    uInfo = new UserManager().GetItem(gaItem.CreatorID.Value);
                    Session[SessionTmpName] = ConvertToItemInfos(gaItem.GAItemDetails);

                    //GetApprover - this is partial
                    bIsCreateNew = false;
                }
            }

            if (editStatus != null)
                ViewBag.EditStatus = editStatus;
            ViewBag.User = uInfo;
            ViewBag.IsCreateNew = bIsCreateNew;
            ViewBag.NodeID = NodeID == null ? "" : NodeID; // from
            ViewBag.TypeUser = TypeUser == null ? "" : TypeUser;
            ViewBag.MainDetailID = MainDetailID == null ? "" : MainDetailID;
            ViewBag.MainID = MainID == null ? "" : MainID;
            ViewBag.CheckUserID = CheckUserID;
            ViewBag.DelegateID = DelegateID;
            ViewBag.DelegateUserID = DelegateUserID;
            ViewBag.ItemUploadGuid = ItemUpload.Guid;
            Session["ItemUpload"] = ItemUpload;
            return View(gaItem);
        }

        /// <summary>
        /// Convert List of HardwareRequirementDetail to List of ItemInfo
        /// </summary>
        /// <param name="GaItemDetails"></param>
        /// <returns></returns>
        private List<ItemInfo> ConvertToItemInfos(ICollection<GAItemDetail> GaItemDetails)
        {
            List<ItemInfo> lstResult = new List<ItemInfo>();
            ItemInfo _itemInfo;
            foreach (GAItemDetail dvResDetailItem in GaItemDetails)
            {
                _itemInfo = new ItemInfo()
                {
                    ID = dvResDetailItem.ID,
                    ItemID = dvResDetailItem.ItemID,
                    ItemName = dvResDetailItem.Item.ItemName,
                    ItemDetailID = dvResDetailItem.ItemDetailID,
                    ItemDetailName = dvResDetailItem.ItemDetail.ItemDetailName,
                    Des = dvResDetailItem.Description,
                    Quantity = (int)dvResDetailItem.Quantity.Value,
                    UnitID = dvResDetailItem.UnitID,
                    DeliveryDate = dvResDetailItem.DeliveryDate,
                    Price = dvResDetailItem.EstimatePrice.Value,
                    Amount = dvResDetailItem.EstimateAmount.Value,
                    UnitName = dvResDetailItem.Unit.NAME,
                    ToTalReceiverByDept = 0, //new FEA_BusinessLogic.HardwareRequirementManager().CountMaterialByDept(dvResDetailItem.ItemDetailID, dvResDetailItem.HardwareRequirement.User1.CostCenterCode),
                    IsBroken = dvResDetailItem.IsBroken == null ? false : dvResDetailItem.IsBroken.Value
                };

                var iInStock = new FEA_BusinessLogic.Base.Connection().db.sp_GetEquipmentInventory(dvResDetailItem.ItemDetailID, dvResDetailItem.UnitID).SingleOrDefault();
                _itemInfo.QuanInStock = (iInStock == null) ? 0 : iInStock.InventoryQuantity.Value;
                _itemInfo.QuanNeeded = (_itemInfo.Quantity > _itemInfo.QuanInStock) ? (_itemInfo.Quantity - _itemInfo.QuanInStock) : 0;

                lstResult.Add(_itemInfo);
            }
            return lstResult;
        }

        #region Item Detail in list Manager
        /// <summary>
        /// Add Item and ItemDetail to Grid
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="ItemDetail"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddItemToOrder(string ItemID, string ItemDetail, string Description, DateTime? DeliveryDate, string Price, bool IsBroken, int? Quantity = 0)
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
                var flag = CheckItemInList(ItemID);
                if (!flag)
                    ViewData["EditError"] = Resources.Resource.msgInvalidItemInList;
                else
                {
                    flag = false;

                    if (FindItemDetailInList(ItemDetail)) // Đã tồn tại trong list
                    {
                        ViewData["EditError"] += "</br>" + Resources.Resource.msgDataduplicate;
                        flag = true;
                    }
                    if (IsBroken && CheckIsBrokenItem(FEA_ITS_Site.Helper.UserLoginInfo.CurrentUser.CostCenterCode, ItemDetail))
                    {
                        ViewData["EditError"] += "</br>" + Resources.Resource.msgIsNotBroken;
                        flag = true;
                    }

                    string sError = CheckGAAdjustmentQuantity(ItemDetail,Quantity==null?999999:Quantity.Value);
                    if (sError != "")
                    {
                        ViewData["EditError"] += "</br>" + sError;
                        flag = true;
                    }

                    if (!flag)
                        {
                            try
                            {
                                Item i = new ItemManager().GetItem(ItemID);
                                ItemDetail item_detail = new ItemDetailManager().GetItem(ItemDetail);
                                Unit unit = new UnitManager().GetItem(item_detail == null ? "" : item_detail.Temp2); // temp2 was unit id

                                Models.ItemInfo item = new ItemInfo()
                                {
                                    ID = Guid.NewGuid().ToString(),
                                    ItemID = i.ID,
                                    ItemName = i.ItemName,
                                    ItemDetailID = item_detail.ID,
                                    ItemDetailName = item_detail.ItemDetailName,
                                    Des = Description,

                                    DeliveryDate = DeliveryDate,
                                    UnitID = unit.ID,
                                    UnitName = unit.NAME,
                                    Quantity = Quantity.Value,
                                    Price = decimal.Parse(Price, System.Globalization.CultureInfo.InvariantCulture),
                                    Amount = Convert.ToDecimal(Quantity.Value * decimal.Parse(Price, System.Globalization.CultureInfo.InvariantCulture)),
                                    ToTalReceiverByDept = new FEA_BusinessLogic.HardwareRequirementManager().CountMaterialByDept(ItemDetail, new FEA_BusinessLogic.UserManager().GetItem(Helper.UserLoginInfo.UserId).CostCenterCode),
                                    IsBroken = IsBroken == null ? false : IsBroken
                                };
                                var iInStock = new FEA_BusinessLogic.Base.Connection().db.sp_GetEquipmentInventory(item.ItemDetailID, item.UnitID).SingleOrDefault();
                                item.QuanInStock = (iInStock == null) ? 0 : iInStock.InventoryQuantity.Value;
                                item.QuanNeeded = (item.Quantity > item.QuanInStock) ? (item.Quantity - item.QuanInStock) : 0;



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
            return GetGridView(Session[SessionTmpName], Models.Helper.PartialParameter.GAItemListGrid);


        }

        /// <summary>
        /// true:
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        private bool CheckItemInList(string ItemID) // Only one ItemID in list
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
            {
                lst = (List<ItemInfo>)Session[SessionTmpName];
                if (lst.Count == 0)
                    return true;
                else return lst.Where(i => i.ItemID == ItemID).Count() > 0 ? true : false;
            }
            return true;

        }

        private bool FindItemDetailInList(string ItemDetailID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
                lst = (List<ItemInfo>)Session[SessionTmpName];

            return lst.Where(i => i.ItemDetailID == ItemDetailID).Count() > 0 ? true : false;
        }

        private bool CheckIsBrokenItem(int CostCenterCode, string ItemDetailID)
        {
            return new FEA_GABusinessLogic.GAItemManager().IsDamgedChecking(ItemDetailID, CostCenterCode);
            
        }

        public ActionResult GetListItemDetailPartial(bool? ShowCommand = false)
        {
            ViewData["ShowCommand"] = ShowCommand;
            return GetGridView((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.GAItemListGrid);
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
        /// <summary>
        /// Delete an Item in Grid 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeleteItemDetail(string ID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
                lst = (List<ItemInfo>)Session[SessionTmpName];

            ItemInfo i = lst.Where(o => o.ID == ID).SingleOrDefault();
            lst.Remove(i);

            Session[SessionTmpName] = lst;
            ViewData["TotalAmount"] = GetTotalAmount();
            ViewData["ShowCommand"] = true;
            return GetGridView((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.GAItemListGrid);
        }


        private int GetOrdeTypeByItem()
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
            {
                lst = (List<ItemInfo>)Session[SessionTmpName];
                if (lst.Where(i => i.ItemName.Contains("Văn Phòng Phẩm")).Count() > 0) return (int) FEA_GABusinessLogic.GAItemManager.OrderType.Stationery;
                if (lst.Where(i => i.ItemName.Contains("Vật Tư Khác")).Count() > 0) return (int)FEA_GABusinessLogic.GAItemManager.OrderType.OtherSupplies;
                if (lst.Where(i => i.ItemName.Contains("Tài sản")).Count() > 0) return (int)FEA_GABusinessLogic.GAItemManager.OrderType.Asset;
            }

            return -1;
        }

        private string CheckGAAdjustmentQuantity(string ItemDetailID, int quantity)
        {
            sp_GAAdjustmentChecking_Result checking = new FEA_GABusinessLogic.GAItemManager().db.sp_GAAdjustmentChecking(quantity, FEA_ITS_Site.Helper.UserLoginInfo.CurrentUser.CostCenterCode, ItemDetailID).FirstOrDefault();
            if (checking != null)
            {
                if (checking.RemainingQuantity > 0)
                {
                    return string.Format(Resources.Resource.msgErrorNotBroken, checking.ItemDetailName, checking.RemainingQuantity);
                }
                return "";
            }
            return "no data";
        }
        private string CheckGAAdjustmentQuantity(int department)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
            {
                lst = (List<ItemInfo>)Session[SessionTmpName];
                FEA_GABusinessLogic.GAItemManager bus =new GAItemManager();
                string strHTML = string.Empty;
                foreach (ItemInfo item in lst)
                {
                    sp_GAAdjustmentChecking_Result checking = bus.db.sp_GAAdjustmentChecking(item.Quantity, department, item.ItemDetailID).FirstOrDefault();
                    if (checking != null && checking.RemainingQuantity>0)
                    {
                        strHTML += "</br>" + string.Format(Resources.Resource.msgErrorNotBroken, checking.ItemDetailName, checking.RemainingQuantity);
                    }
                }
                return strHTML;
            }

            return "";

        }

        [HttpPost]
        public string CheckLimited(int Department)
        {
            string sResult = string.Empty;

            // Check for permission
            if (!Helper.UserLoginInfo.IsLogin)
            {
                sResult += "Please login into system.";
                return sResult;
            }

            if (Department == -1 || Department == null)
                Department = Helper.UserLoginInfo.CurrentUser.CostCenterCode;

            //Check for item limited
            return CheckGAAdjustmentQuantity(Department);

        }
        #endregion

        #region Save data
        [HttpPost]
        public ActionResult SaveData([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] GAItem o, bool CreateNew)
        {

            string sResult = "";
            if (Request.Form["btnSaveDraff"] != null) //Save Draft
                sResult = CreateGaItem(o, true);
            else if (Request.Form["btnSaveAndSend"] != null) // Save And Send
                if (CreateNew)
                    sResult = CreateGaItem(o, false);//isSaveDraff = false - Create Order;
                else
                    sResult = UpdateGAItemRequiment(o, false);//isSaveDraff = false - Update Order;
            else if (Request.Form["btnUpdate"] != null) //Update - 
            {
                sResult = UpdateGAItemRequiment(o, true);
            }
            if (sResult.Length > 0)
                return RedirectToAction("Index", new { ID = sResult, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.success });// Index(sResult,"","", (int)Models.Helper.EditItemStatus.success);
            else
                return RedirectToAction("Index", new { ID = sResult, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.failed });//Index(sResult, "", "", (int)Models.Helper.EditItemStatus.failed);
        }

        /// <summary>
        /// Create Device Registration Information
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <param name="wfMain"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string CreateGaItem(GAItem o, bool isSaveDraff)
        {
            //Master
            string _sApprovedUser = "";
            WFMain wfMain = new WFMain();
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.GAItemDetails = GAItemDetailList();
            o.OrderCode = Controllers.HelperController.GenerateELandCode(TagPrefixParameter.GENERALAFFAIR);
            o.OrderType = GetOrdeTypeByItem();
            // add by Kane 2016-05-25, Văn Phòng Phẩm OrderType=1, Vật Tư Khác OrderType=2
            if (o.GAItemDetails.Select(i => i.ItemID == "0373810b-4b1b-46fa-a8a4-60f5aa46caff").FirstOrDefault())
                o.OrderType = 1;
            else if (o.GAItemDetails.Select(i => i.ItemID == "7d36a360-3474-41de-aac9-2e749bb99104").FirstOrDefault())
                o.OrderType = 2;
            o.Temp1 = o.Temp2 = "";
            o.AttachmentLink = FEA_ITS_Site.Helper.Ultilities.UploadFolder + o.ID; // -- Added Attachment link
            if (isSaveDraff)
                o.Status = (int)GAItemManager.OrderStatus.DRAFT;
            else
            {
                o.Status = (int)GAItemManager.OrderStatus.SENDING;

                _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");

                wfMain = GetWFmain(o.OrderCode, _sApprovedUser);
            }
            ///Sent mail to next approver
            if (!isSaveDraff && _sApprovedUser.Length > 0)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Reason, "");
            //End sent mail

            string sID = new GAItemManager().InsertItem(o, (GAItemManager.OrderStatus)o.Status, wfMain);
            return sID;
        }

        /// <summary>
        /// this function using for Update (update, save and send) when document Returned
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string UpdateGAItemRequiment(GAItem o, bool isSaveDraff)
        {
            string _sApprovedUser = "";
            WFMainDetail wfMainDetail = new WFMainDetail();
            bool isReturned = (o.Status == (int)GAItemManager.OrderStatus.RETURNED) ? true : false;

            //Master
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.GAItemDetails = GAItemDetailList();
            o.OrderType = GetOrdeTypeByItem();
            if (!isSaveDraff)
            {
                _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
                WFMain wfMain = new WFMainManager().GetItem(o.OrderCode);

                if (wfMain == null) // WFMain not exist in database
                {
                    wfMain = new WFMain()
                    {
                        MainID = Guid.NewGuid().ToString(),
                        OrderCode = o.OrderCode,
                        DocumentTypeID = Session["DocumentTypeID"] == null ? "" : Session["DocumentTypeID"].ToString(),// Get from GetApproverPartial() function
                        isFinished = 0,
                        CreateUserID = Helper.UserLoginInfo.UserId,
                        CreateDate = DateTime.Now,
                        Temp1 = "",
                        Temp2 = "",
                        Temp3 = ""
                    };

                    bool flag = new WFMainManager().InsertItem(wfMain); // Create WFMain
                }

                wfMainDetail = new WFMainDetail()
                {
                    MainDetailID = Guid.NewGuid().ToString(),
                    MainID = wfMain.MainID,
                    NodeID = Session["NodeID"] == null ? "" : Session["NodeID"].ToString(), // Get from GetApproverPartial() function
                    DelegateID = null,
                    PostUserID = Helper.UserLoginInfo.UserId,
                    CheckUserID = int.Parse(_sApprovedUser),
                    isFinished = 0,
                    Comment = "",
                    Temp1 = "0",
                    Temp2 = "",
                    Temp3 = ""
                };
            }
            bool _result = new GAItemManager().UpdateItem(o, isSaveDraff, isReturned, wfMainDetail,
                                                                         i => i.Description, i => i.Reason, i => i.CreateDate,
                                                                         i => i.IsUrgent, i => i.CurrencyID, i => i.EstimateAmount,i=>i.OrderType);

            ///Sent mail to next approver
            if (!isSaveDraff && _result)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Reason, "");
            //End sent mail
            return _result == true ? o.ID : "";
        }


        /// <summary>
        /// Get WF Main and WF MainDetail
        /// </summary>
        /// <param name="sOrderCode"></param>
        /// <param name="_sApprovedUser"></param>
        /// <returns></returns>
        private WFMain GetWFmain(string sOrderCode, string _sApprovedUser)
        {
            WFMain wfMain = new WFMain()
            {
                MainID = Guid.NewGuid().ToString(),
                OrderCode = sOrderCode,
                DocumentTypeID = Session["DocumentTypeID"] == null ? "" : Session["DocumentTypeID"].ToString(),// Get from GetApproverPartial() function
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
                                            NodeID = Session["NodeID"] == null ? "" : Session["NodeID"].ToString(), // Get from GetApproverPartial() function on DeviceRegistrationController
                                            DelegateID = null,
                                            PostUserID = Helper.UserLoginInfo.UserId,
                                            CheckUserID = int.Parse(_sApprovedUser),
                                            isFinished = 0,
                                            Comment = "",
                                            Temp1 = "0",
                                            Temp2 = "",
                                            Temp3 = ""
                                        }
                                      );

            return wfMain;
        }

        /// <summary>
        /// Get HardwareRequirementDetail to save to DB
        /// </summary>
        /// <returns></returns>
        private List<GAItemDetail> GAItemDetailList()
        {
            List<GAItemDetail> lst = new List<GAItemDetail>();
            if (Session[SessionTmpName] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session[SessionTmpName];
                GAItemDetail itemdetail;
                foreach (ItemInfo i in lstItemInfo)
                {
                    itemdetail = new GAItemDetail()
                    {
                        Description = i.Des,
                        ItemNO = "",
                        ItemDetailID = i.ItemDetailID,
                        ItemID = i.ItemID,
                        Quantity = i.Quantity,
                        UnitID = i.UnitID,
                        DeliveryDate = i.DeliveryDate,
                        EstimatePrice = i.Price,
                        EstimateAmount = i.Amount,
                        IsBroken = i.IsBroken,
                        Temp1 = "",
                        Temp2 = ""
                    };
                    lst.Add(itemdetail);
                }
            }
            return lst;
        }
        #endregion

        #region Sign - reject
        public ActionResult SignDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] GAItem o)
        {
            int Status = 0;
            string _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            GAItem gaitem = new GAItemManager().GetItem(o.ID);
            if (new FEA_BusinessLogic.WaitingArea.WaitingArea().CheckAlreadySigned(MainDetailID) == 0)
            {
                Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Sign", _sApprovedUser, comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.GENERALAFFAIR, gaitem.User.CostCenterCode, NodeID /*current node*/, o.OrderCode, gaitem.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, gaitem.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);
            }
            else
            {
                Status = 0;
            }

            return RedirectToAction("Index", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });

        }

        public ActionResult RejectDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] GAItem o)
        {
            int Status = 0;
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
            GAItem gaitem = new GAItemManager().GetItem(o.ID);
            Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Reject", "", comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.GENERALAFFAIR, gaitem.User.CostCenterCode, NodeID  /*current node*/, o.OrderCode, gaitem.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, gaitem.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);
            return RedirectToAction("Index", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });
        }
    
        #endregion


        #endregion

        #region "User Manage Order"
        public ActionResult Management()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult GAItemEditModesPartial()
        {

            List<GAItem> lstItem = new GAItemManager().GetItems(Helper.UserLoginInfo.UserId);
            return GetGridView(lstItem, Models.Helper.PartialParameter.GaItemManagerListGrid);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult GAItemEditModesDeletePartial(string ID)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_GABusinessLogic.GAItemManager().DeleteItem(ID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteDeviceRegFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(new GAItemManager().GetItems(Helper.UserLoginInfo.UserId), Models.Helper.PartialParameter.GaItemManagerListGrid);
        }

        #endregion

        #region GAAdjustment
        // GA Biên chế vật tư
        public ActionResult GAAdjustment()
        {
            ViewBag.EndDate = DateTime.Now;
            ViewBag.CopyMonth = DateTime.Now.AddMonths(1);
            if (Request.Form["btnQuery"] != null)
            {
                ViewBag.EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate");
            }
            if (Request.Form["btnCopy"] != null)
            {
                ViewBag.EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate");
                ViewBag.CopyMonth = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtCopyMonth");
                try
                {
                    int copy = new FEA_GABusinessLogic.GAAdjustmentItemManager().CopyGAAdjustments(ViewBag.EndDate, ViewBag.CopyMonth);
                    if (copy==0)
                    {
                        ViewData["EditError"] = "Can not copy data - pls contact with IT";
                    }
                    else
                    {
                        ViewData["Success"] = "Success";
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return View();
        }

        [ValidateInput(false)]
        public ActionResult EditModesPartial( DateTime EndDate)
        {
            ViewBag.EndDate = EndDate;// used on _getGridView partical
            var frm = new FEA_GABusinessLogic.GAAdjustmentItemManager().GetGAAdjustment( ViewBag.EndDate);
            return GetGridView(frm, Models.Helper.PartialParameter.GAAdjustmentList);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] GAAdjustment obj,  DateTime EndDate)
        {

            ViewBag.EndDate = EndDate;// used on _getGridView partical
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.CostCenterCode != null && obj.ItemDetail != null && obj.Month != null && obj.AdjustQuantity !=null)
                    {
                    
                        obj.CreatorID = FEA_ITS_Site.Helper.UserLoginInfo.UserId;
                        obj.UpdateUserID = FEA_ITS_Site.Helper.UserLoginInfo.UserId;
                        int _result = new GAAdjustmentItemManager().InsertGAAdjustments(obj);
                        if (_result < 1)
                        {
                            if (_result == -1)
                                ViewData["EditError"] = "Dữ liệu đã tồn tại";
                            else
                                ViewData["EditError"] = Resources.Resource.msgInsertFail;
                        }
                        else
                        {
                            ViewData["Success"] = "Success";
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
            return EditModesPartial(ViewBag.EndDate);
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] GAAdjustment obj,DateTime EndDate)
        {
            ViewBag.EndDate = EndDate;// used on _getGridView partical
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.CostCenterCode != null && obj.ItemDetail != null && obj.Month != null && obj.AdjustQuantity != null)
                    {
                        //bool _result = new SAExportApprovalItemManager().UpdateItem(obj, o => o.ApproverID, o => o.Status);
                        obj.UpdateUserID = FEA_ITS_Site.Helper.UserLoginInfo.UserId;
                        bool _result = new GAAdjustmentItemManager().UpdateItem(obj, o=>o.CostCenterCode, o=> o.ItemDetail, o=> o.AdjustQuantity, o=> o.AdjustAmount, o=> o.Damged,o=> o.Month, o=> o.Reason,o => o.Status);
                        if (!_result)
                            ViewData["EditError"] = Resources.Resource.msgInsertFail;
                        else
                            ViewData["Success"] = "Success";
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
            return EditModesPartial(ViewBag.EndDate);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string ID, DateTime EndDate)
        {
            ViewBag.EndDate = EndDate;// used on _getGridView partical
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new GAAdjustmentItemManager().DeleteGAAdjustment(ID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
                    else
                        ViewData["Success"] = "Success";
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return EditModesPartial(ViewBag.EndDate);
        }
        #endregion

        #region GA CHECKING MANAGEMENT FUNCTION FOR ADMIN
        
        //GA Approve managerment
        public ActionResult GACheckingManagerment()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/GA/GACheckingManagerment") });
            //

            if (Request.Form["btnApprove"] != null)
            {
                var lstItem = Request.Form["mydata"];
                try
                {
                    if (lstItem.Trim().Length != 0)
                    {
                        Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
                        if (values != null)
                        {
                            int iResult = 0;

                            // List of GA Registration - IDs
                            List<string> lstTosign = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.GENERALAFFAIR).Select(i => i.Key).ToList<string>();
                            if (lstTosign != null)
                                iResult = new FEA_GABusinessLogic.GAItemManager().GAApproveOrder(lstTosign, Helper.UserLoginInfo.UserId);
                            //if (lstTosign != null && lstTosign.Count > 0)
                            //{
                            //    int status = new FEA_BusinessLogic.HardwareRequirementManager().AdminApproveOrder(lstTosign, Helper.UserLoginInfo.UserId);
                            //    if (status <= 0)
                            //    {
                            //        if (status == -1)
                            //            ViewBag.EditInfo = "Can not update the data";
                            //        else if (status == -2)
                            //            ViewBag.EditInfo = "Can not calculate the quantity in the Stock";
                            //        else if (status == -3)
                            //            ViewBag.EditInfo = "Can not send data to Eland system";

                            //        ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                            //        return View();
                            //    }
                            //    else
                            //        iResult += status;
                            //}


                            if (iResult > 0)
                            {
                                ViewBag.EditStatus = Models.Helper.EditItemStatus.success;
                                ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
                            }
                            else
                            {
                                ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                                ViewBag.EditInfo = Resources.Resource.msgUpdateFailed;
                            }
                        }
                    }
                    else
                    {
                        ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                        ViewBag.EditInfo = Resources.Resource.msgSelectItem;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = ex.Message;
                }
            }
            else if (Request.Form["btnReject"] != null) // GA Dept Reject Document(s)
            {
                var lstItem = Request.Form["mydata"];
                var it_commnet = "GA Department: --> " + Request.Form["GA_comment"];
                if (lstItem.Trim().Length != 0)
                {
                    try
                    {
                        Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
                        if (values != null)
                        {
                            int result = 0;

                            foreach (KeyValuePair<string, string> entry in values)
                            {

                                int DelegateUserID, CheckUserID;
                                string NodeID, MainID, MainDetailID, DelegateID;

                                WFMain main = new WFMainManager().GetItem(entry.Key);
                                MainID = main.MainID;
                                MainDetailID = main.WFMainDetails.OrderByDescending(i => i.CheckDate).FirstOrDefault().MainDetailID;

                                NodeID = "";
                                CheckUserID = FEA_ITS_Site.Helper.UserLoginInfo.UserId;
                                DelegateID = "";
                                DelegateUserID = 0;
                                int status = WaitingAreaController.SignRejectDocument("Reject", "", it_commnet, entry.Value, main.User.CostCenterCode, NodeID, main.OrderCode, "", MainID, MainDetailID, "", main.CreateUserID.Value, CheckUserID, DelegateID, DelegateUserID, true);
                                result += status;
                            }
                            if (result == values.Count)
                            {
                                ViewBag.EditStatus = Models.Helper.EditItemStatus.success;
                                ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
                            }
                            else
                            {
                                ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                                ViewBag.EditInfo = "Một vài đơn chưa thực hiện được, vui lòng kiêm tra lại";
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                        ViewBag.EditInfo = ex.Message;
                    }
                }
                else
                {
                    ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = Resources.Resource.msgSelectItem;
                }

            }

            return View();
        }



        //2

        [HttpGet]
        public ActionResult GADoProcessRequestOrder(string data)
        {
            ViewBag.Data = data;
            return View();
        }


        //3
        [ValidateInput(false)]
        public ActionResult ApprovePartial()
        {
            var frm = new FEA_GABusinessLogic.GAItemManager().GetItems(GAItemManager.OrderStatus.CHECKED, Helper.UserLoginInfo.UserId);
            return GetGridView(frm, Models.Helper.PartialParameter.GA_APPROVE_ORDER_GRID);
        }


        public ActionResult FinishedManagementForAdmin()
        {
            object x = new FEA_GABusinessLogic.GAItemManager().GetFinishedDocument(Helper.UserLoginInfo.UserId);
            return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.GAFinishedForManager);
        }

        // When click butt show detail in gridview history
        public ActionResult CallbackFinishedDocumentGrid()
        {
            object x = new FEA_GABusinessLogic.GAItemManager().GetFinishedDocument(Helper.UserLoginInfo.UserId);
            return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.GAFinishedForManager);
        }

        #endregion

        public ActionResult CallbackDetail(string ID, string DocumentTypeName)
        {
            ViewData["ID"] = ID;
            string GridName = string.Empty;
           // object x = new FEA_BusinessLogic.Statistic.Report().DetailGetFinishedDocument(ID, DocumentTypeName);
            object x = new FEA_GABusinessLogic.GAItemManager().GetDetailFinishDocument(ID, DocumentTypeName);
            return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.GADetailFinishedForManager);
        }



        #region "Ga summary"
        public ActionResult GaSummary()
        {
            ViewBag.BeginDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.EndDate = DateTime.Now;
            ViewBag.OrderCode = "";

            if (Request.Form["btnQuery"] != null)
            {
                ViewBag.BeginDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtBeginDate");
                ViewBag.EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate");
                ViewBag.OrderCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtOrderCode");
            }

            return View();
        }

        [ValidateInput(false)]
        public ActionResult PivotGrid1Partial(int Type,DateTime BeginDate, DateTime EndDate)
        {
            ViewBag.BeginDate = BeginDate;
            ViewBag.EndDate = EndDate;
            ViewBag.Type = Type;

            List<sp_GetGASummaryReport_Result> model = new FEA_BusinessLogic.Base.Connection().db.sp_GetGASummaryReport(BeginDate, EndDate).ToList();
            Session["GAData"] = model;
            return PartialView("_PivotGrid1Partial", model.Where(m=>m.OrderType == Type));
        }

        public ActionResult ExportToExcel()
        {
   
            return ExportTo();
        }
        public ActionResult ExportTo()
        {
            List<sp_GetGASummaryReport_Result> _object = (List < sp_GetGASummaryReport_Result > )Session["GAData"];

            PrintingSystem ps = new PrintingSystem();
            ps.XlsxDocumentCreated += new XlsxDocumentCreatedEventHandler(PrintingSystem_XlsxDocumentCreated); // most important part

            CompositeLink complink = new CompositeLink(ps);
            PrintableComponentLink link1 = new PrintableComponentLink();
            PivotGridSettings PivotSetting = PivotGridSetting();
            link1.Component = PivotGridExtension.CreatePrintableObject(PivotSetting, _object.Where(i=>i.OrderType ==1).ToList() as List<sp_GetGASummaryReport_Result>);
            complink.Links.Add(link1);

            PrintableComponentLink link2 = new PrintableComponentLink();
            PivotGridSettings PivotSetting2 = PivotGridSetting();

            link2.Component = PivotGridExtension.CreatePrintableObject(PivotSetting2, _object.Where(i => i.OrderType == 2).ToList() as List<sp_GetGASummaryReport_Result>);
            complink.Links.Add(link2);

            complink.CreatePageForEachLink();
          //  complink.EnablePageDialog = true;
           // complink.CreateDocument();

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
            result.FileDownloadName = "Xuất nhập tồn kho tháng "+string.Format("{0}-{1}",DateTime.Now.Month, DateTime.Now.Year)+"_"+DateTime.Now.Year+"年"+DateTime.Now.Month+"月進入存報告" + ".xlsx";
            return result;
        }
        // and event where you change sheetname
        void PrintingSystem_XlsxDocumentCreated(object sender, XlsxDocumentCreatedEventArgs e) { 
            
            if (e.SheetNames != null)
            {
                e.SheetNames[0] = "Văn phòng phẩm 文具用品";
                e.SheetNames[1] = "Vật tư khác 其它用品";
            }
        }
        static PivotGridSettings PivotGridSetting()
        {
            PivotGridSettings settings = new PivotGridSettings();
            settings.Name = "Export item pivot -" + DateTime.Today;
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsView.ShowFilterHeaders = false;


            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = "TÊN VPP / VẬT TƯ 貨品名稱";
                field.FieldName = "ItemDetailName";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 2;
                field.Caption = "Đ.V TÍNH 單位";
                field.FieldName = "UnitName";
                field.SortMode = PivotSortMode.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 1;
                field.Caption = "Đơn giá 單價(Max)";
                field.FieldName = "EstimatePrice";
                field.CellFormat.FormatString = "##,#";
                field.CellFormat.FormatType = FormatType.Custom;
            });


            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.ColumnArea;
                field.AreaIndex = 0;
                field.Caption = "Bộ phận 部門";
                field.FieldName = "Dept";
                field.SortMode = PivotSortMode.None;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 0;
                field.Caption = "Số lượng 數量(Sum) ";
                field.FieldName = "Quantity";
                field.CellFormat.FormatString = "##,#";
                field.CellFormat.FormatType = FormatType.Custom;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 1;
                field.Caption = "Thành tiền  金額";
                field.FieldName = "Amount";
                field.CellFormat.FormatString = "##,#";
                field.CellFormat.FormatType = FormatType.Custom;
            });

            //settings.Fields.Add(field =>
            //{
            //    field.Area = PivotArea.ColumnArea;
            //    field.AreaIndex = 4;
            //    field.Caption = "GHI CHÚ 備註";

            //});
            settings.FieldValueDisplayText = (sender, e) =>
            {
                if (e != null)
                {
                    if (e.DisplayText == "Grand Total")
                        e.DisplayText = "TỔNG 合計";
                }
            };

            return settings;
        }



        #endregion
        [HttpPost]
        public decimal GetItemPriceByAdjust(string itemdetailID)
        {
            GAAdjustment item = new GAAdjustmentItemManager().GetItem(itemdetailID, Helper.UserLoginInfo.CurrentUser.CostCenterCode);
            if (item != null)
            {
                return item.AdjustAmount == null ? 0 : item.AdjustAmount.Value;
            }
            else
                return 0;
        }


        #region GA Query funciton
        public ActionResult GetGARequested()
        {
            ViewBag.BeginDate = DateTime.Now.AddMonths(-1);
            ViewBag.EndDate = DateTime.Now;
            ViewBag.OrderCode = "";

            if (Request.Form["btnQuery"] != null)
            {
                ViewBag.BeginDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtBeginDate");
                ViewBag.EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate");
                ViewBag.OrderCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtOrderCode");
                if (ViewBag.OrderCode == null) ViewBag.OrderCode = "";
            }

            return View();
        }

        [ValidateInput(false)]
        public ActionResult DocumentGARequestedPartial(string OrderCode, DateTime BeginDate, DateTime EndDate)
        {
            ViewBag.BeginDate = BeginDate;
            ViewBag.EndDate = EndDate;
            ViewBag.OrderCode = (OrderCode == null) ? "" : OrderCode;

            var frm = new FEA_GABusinessLogic.GAItemManager().GetGARequestList(ViewBag.OrderCode, ViewBag.BeginDate, ViewBag.EndDate);
            return GetGridView(frm, Models.Helper.PartialParameter.GARequestList);
        }
        #endregion
        // Add by Tony (2016-09-15)
        #region GA Push Data
        FileStreamResult CreateExcelExportResultPushData(CompositeLink link)
        {
            MemoryStream stream = new MemoryStream();

            XlsxExportOptions opt = new XlsxExportOptions() { ExportMode = XlsxExportMode.SingleFilePageByPage };



            link.PrintingSystem.ExportToXlsx(stream, opt);
            stream.Position = 0;
            FileStreamResult result = new FileStreamResult(stream, "application/xls");
            result.FileDownloadName = "PushData" + string.Format("{0}-{1}", DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").Month, DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").Year) + "_" + DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").Year + "" + DateTime.Now.Month + "" + ".xlsx";
            return result;
        }
        public ActionResult PushDataExportTo()
        {
            
            List<sp_GetGAPushDataList_Result> _object = (List<sp_GetGAPushDataList_Result>)Session["GADataPush"];

            PrintingSystem ps = new PrintingSystem();
            ps.XlsxDocumentCreated += new XlsxDocumentCreatedEventHandler(PrintingSystem_XlsxDocumentCreated); // most important part

            CompositeLink complink = new CompositeLink(ps);
            PrintableComponentLink link1 = new PrintableComponentLink();
            PivotGridSettings PivotSetting = PivotGridSettingPushData();
            link1.Component = PivotGridExtension.CreatePrintableObject(PivotSetting, _object.ToList() as List<sp_GetGAPushDataList_Result>);
            complink.Links.Add(link1);
            complink.CreatePageForEachLink();

            FileStreamResult result = CreateExcelExportResultPushData(complink);


            return result;
        }
        public ActionResult ExportToExcelPushData()
        {
            return PushDataExportTo();
        }
        static PivotGridSettings PivotGridSettingPushData()
        {
            PivotGridSettings settings = new PivotGridSettings();
            settings.Name = "Export item pivot -" + DateTime.Today;
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsView.ShowFilterHeaders = false;
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.DatePush;
                field.FieldName = "Temp2";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.PushPerson;
                field.FieldName = "PersonPush";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.EstiamtedAmount;
                field.FieldName = "EstimateAmount";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.CompleteDate;
                field.FieldName = "ConfirmDate";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.ProcessPerson;
                field.FieldName = "ProcessPerson";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.CreateDate;
                field.FieldName = "CreateDate";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.Reason;
                field.FieldName = "Reason";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.Department;
                field.FieldName = "Department";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.Creator;
                field.FieldName = "Creator";
                field.SortMode = PivotSortMode.None;
            });
           
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.OrderCode;
                field.FieldName = "OrderCode";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.PRCode;
                field.FieldName = "PRCode";
                field.SortMode = PivotSortMode.None;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.Caption = Resources.Resource.DocType;
                field.FieldName = "OrderType";
                field.SortMode = PivotSortMode.None;
            });
            return settings;
        }

        public ActionResult GAManagementData()
        {  
            return View();
        }
        public ActionResult GAPushData()
        {
            if (!IsPostback)
            {
                string sEndDate = DateTime.Now.ToString("yyyy-MM-22");
                ViewBag.EndDate = DateTime.Parse(sEndDate);
                string sBeginDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-23");
                ViewBag.BeginDate = DateTime.Parse(sBeginDate);
                ViewBag.OrderCode = "";
            }
            else
            {
                string sBeginDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").AddMonths(-1).ToString("yyyy-MM-23");
                string sEndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").ToString("yyyy-MM-22");
                ViewBag.BeginDate = DateTime.Parse(sBeginDate);
                ViewBag.EndDate = DateTime.Parse(sEndDate);
            }
            if(Request.QueryString["Type"]!="7")
            {
                ViewBag.Status = 5;
            }
            else
            {
               
                ViewBag.Status = 7;
            }
            if (Request.Form["btnQuery"] != null)
            {
                string  BeginDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").AddMonths(-1).ToString("yyyy-MM-23");
                string  EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").ToString("yyyy-MM-22");
                ViewBag.BeginDate = DateTime.Parse(BeginDate);
                //Session["BeginDate"] = ViewBag.BeginDate;
                ViewBag.EndDate = DateTime.Parse(EndDate);
                //Session["EndDate"] = ViewBag.EndDate;
                if (Request.QueryString["Type"] != "7")
                {
                    ViewBag.Status = 5;
                }
                else
                {
                    ViewBag.Status = 7;
                }
                ViewBag.OrderCode = ""; 
            }
           
            if(Request.Form["btnPush"]!=null)
            {
                if (!Helper.UserLoginInfo.IsLogin)
                   return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/GA/GAPushData") });
              var lstItem = Request.Form["mydata"];
              
                    if (lstItem.Trim().Length != 0)
                    {
                        Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
                      
                        if (values != null)
                        {
                            List<string> lstTosign = values.Where(i => i.Value == "Push").Select(i => i.Key).ToList<string>();
                            string OrderCode = string.Join(",", lstTosign.ToArray());
                            string EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").ToString("MM");
                            ObjectParameter iResult;
                            Thread.Sleep(1500);
                            iResult = new FEA_GABusinessLogic.GAItemManager().GAPushDataToEland(OrderCode,EndDate,Helper.UserLoginInfo.UserId);
                            ViewBag.EditStatus = iResult.Value;
                        }
                        
                    }
                   
                }
            
            return View();
        }
       
        public ActionResult DocumentGAPushDataPartial(string OrderCode, DateTime BeginDate, DateTime EndDate, int Status)
        {

            ViewBag.showCreatePerson = false;
            ViewBag.showDatePush = false;
            ViewBag.showPRCode = false;
            ViewBag.BeginDate = BeginDate;
            ViewBag.EndDate = EndDate;
            ViewBag.OrderCode = "";
            ViewBag.Status = Status;
            if (ViewBag.Status==5)
            {
                ViewBag.Checkbox = true;
                ViewBag.showCreatePerson = false;
                ViewBag.showDatePush = false;
                ViewBag.showPRCode = false;
            }
            else
            {
                ViewBag.Checkbox = false;
                ViewBag.showCreatePerson = true;
                ViewBag.showDatePush = true;
                ViewBag.showPRCode = true;
            }
           
            var frm = new FEA_GABusinessLogic.GAItemManager().GetGaPushDataList(ViewBag.OrderCode, ViewBag.BeginDate,ViewBag.EndDate, ViewBag.Status);
            Session["GADataPush"] = frm;
            return GetGridView(frm, Models.Helper.PartialParameter.GAPushDataList);
        }
        public ActionResult GAPushDataDetail(string ID)
        {
            ViewData["ID"] = ID;
            string GridName = string.Empty;
            object x = new FEA_GABusinessLogic.GAItemManager().GetGAPushDataDetail(ID);
            return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.GADetailPushData);
        }
        
        #endregion
        //Tony
    }
}
