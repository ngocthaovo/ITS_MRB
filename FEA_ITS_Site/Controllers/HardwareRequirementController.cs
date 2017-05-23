using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
using FEA_ITS_Site.Models.Helper;
using FEA_ITS_Site.Models;
using System.IO;
namespace FEA_ITS_Site.Controllers
{
    public class HardwareRequirementController : BaseController
    {
        //
        // GET: /HardwareRequirement/

        public string SessionTmpName = "ItemInfoListHR";
        public ActionResult MainPage()
        {
            return View();
        }


        public ActionResult Index(string ID, string NodeID, string TypeUser, string MainDetailID, string MainID, int? CheckUserID,string DelegateID,int? DelegateUserID, int? editStatus)
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/HardwareRequirement/Index") });
            //

            HardwareRequirement deRegItem;
            User uInfo = new User();
            Boolean bIsCreateNew = false;
            FEA_ITS_Site.Models.ItemUpload ItemUpload = new ItemUpload();

            if (ID == null || ID.Trim().Length == 0) // Create New
            {
                // Get User Logged info
                uInfo = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
                // get DeviceReg Info
                deRegItem = new HardwareRequirement()
                {
                    CreateDate = DateTime.Now,
                    CreatorID = Helper.UserLoginInfo.UserId,
                    Status = (int)HardwareRequirementManager.OrderStatus.NEW,
                    EstimatedAmount = 0,
                    CurrencyID = "",
                    OrderCode = Controllers.HelperController.GenerateELandCode(TagPrefixParameter.HARD_REGISTRATION),
                    IsUrgent = 0,
                    ID = Guid.NewGuid().ToString()
                };
                ItemUpload.Guid = deRegItem.ID;
                // Item Detail
                Session[SessionTmpName] = new List<ItemInfo>();
                bIsCreateNew = true;
            }
            else // Load Info and show to View Page
            {
                deRegItem = new HardwareRequirementManager().GetItem(ID);
                ItemUpload.Guid = deRegItem.ID;
                if (System.IO.Directory.Exists(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + deRegItem.AttachmentLink)) && deRegItem.AttachmentLink.Trim().Length != 0)
                {
                    string[] ListItem = null;// Directory.GetFiles(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + deRegItem.AttachmentLink));     // Please test again.
                    ListItem = deRegItem.AttachmentLink != null ? Directory.GetFiles(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + deRegItem.AttachmentLink)) : null;
                    ItemUpload.ListAddress = ListItem;
                }
                if (deRegItem != null)
                {
                    // Get User Logged info
                    uInfo = new UserManager().GetItem(deRegItem.CreatorID.Value);
                    Session[SessionTmpName] = ConvertToItemInfos(deRegItem.HardwareRequirementDetails);

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
            return View(deRegItem);
        }
        /// <summary>
        /// Convert List of HardwareRequirementDetail to List of ItemInfo
        /// </summary>
        /// <param name="HardRgDetails"></param>
        /// <returns></returns>
        private List<ItemInfo> ConvertToItemInfos(ICollection<HardwareRequirementDetail> HardRgDetails)
        {
            List<ItemInfo> lstResult = new List<ItemInfo>();
            ItemInfo _itemInfo;
            foreach (HardwareRequirementDetail dvResDetailItem in HardRgDetails)
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
                    Price = dvResDetailItem.EstimatedPrice.Value,
                    Amount = dvResDetailItem.EstimatedAmount.Value,
                    UnitName = dvResDetailItem.Unit.NAME,
                    ToTalReceiverByDept = new FEA_BusinessLogic.HardwareRequirementManager().CountMaterialByDept(dvResDetailItem.ItemDetailID, dvResDetailItem.HardwareRequirement.User1.CostCenterCode),
                    
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
        public ActionResult AddItemToOrder(string ItemID, string ItemDetail, string Description, DateTime? DeliveryDate, string Price, int? Quantity = 0)
        {

            if (ItemID.Trim().Length == 0 || ItemDetail.Trim().Length == 0 
                || Price.Trim().Length == 0 || Convert.ToDecimal(Quantity) <= 0 || Convert.ToDecimal(Price) <= 0 || DeliveryDate == null || DeliveryDate.Value.Date < DateTime.Now.Date){

                string _sErr = Resources.Resource.msgInputError;
                if (ItemID.Trim().Length == 0)
                    _sErr += string.Format("<br/>"+Resources.Resource.msgInputRequite, Resources.Resource.ItemType);
                if(ItemDetail.Trim().Length == 0 )
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Specs);
                if (Price.Trim().Length == 0 || Convert.ToDecimal(Price) <= 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Price);
                if(Convert.ToDecimal(Quantity) <= 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Quantity);
                if( DeliveryDate == null || DeliveryDate.Value.Date < DateTime.Now.Date)
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
                    flag = FindItemDetailInList(ItemDetail);
                    if (flag) // Đã tồn tại trong list
                        ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                    else
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
                                Price = decimal.Parse(Price,System.Globalization.CultureInfo.InvariantCulture),
                                Amount = Convert.ToDecimal(Quantity.Value * decimal.Parse(Price, System.Globalization.CultureInfo.InvariantCulture)),
                                ToTalReceiverByDept = new FEA_BusinessLogic.HardwareRequirementManager().CountMaterialByDept(ItemDetail,new FEA_BusinessLogic.UserManager().GetItem(Helper.UserLoginInfo.UserId).CostCenterCode)
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
            return GetGridView(Session[SessionTmpName], Models.Helper.PartialParameter.ITEM_DETAIL_HARDWARE_LIST_GRID);
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


        public ActionResult GetListItemDetailPartial(bool? ShowCommand = false)
        {
            ViewData["ShowCommand"] = ShowCommand;
            return GetGridView((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.ITEM_DETAIL_HARDWARE_LIST_GRID);
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
            return GetGridView((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.ITEM_DETAIL_HARDWARE_LIST_GRID);
        }

        #endregion



        /// <summary>
        /// Save to DB for Inserting and Updateing
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        /// 


        #region "Save Data"

        [HttpPost]
        public ActionResult SaveData([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] HardwareRequirement o, bool CreateNew)
        {

            string sResult = "";
            if (Request.Form["btnSaveDraff"] != null) //Save Draft
                sResult = CreateHardwareRequiment(o, true);
            else if (Request.Form["btnSaveAndSend"] != null) // Save And Send
                if (CreateNew)
                    sResult = CreateHardwareRequiment(o, false);//isSaveDraff = false - Create Order;
                else
                    sResult = UpdateHardwareRequiment(o, false);//isSaveDraff = false - Update Order;
            else if (Request.Form["btnUpdate"] != null) //Update - 
            {
                sResult = UpdateHardwareRequiment(o, true);
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
        private string CreateHardwareRequiment(HardwareRequirement o, bool isSaveDraff)
        {
            //Master
            string _sApprovedUser = "";
            WFMain wfMain = new WFMain();
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.HardwareRequirementDetails = HardwareRequimentDetailList();
            o.OrderCode = Controllers.HelperController.GenerateELandCode(TagPrefixParameter.HARD_REGISTRATION);
            o.Temp1 = o.Temp2 ="";
            o.AttachmentLink = FEA_ITS_Site.Helper.Ultilities.UploadFolder + o.ID; // -- Added Attachment link
            if (isSaveDraff)
                o.Status = (int)DeviceRegistrationManager.OrderStatus.DRAFT;
            else
            {
                o.Status = (int)DeviceRegistrationManager.OrderStatus.SENDING;

                _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");

                wfMain = GetWFmain(o.OrderCode, _sApprovedUser);
            }
            ///Sent mail to next approver
            if (!isSaveDraff && _sApprovedUser.Length > 0)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Reason,"");
            //End sent mail

            string sID = new HardwareRequirementManager().InsertItem(o, (HardwareRequirementManager.OrderStatus)o.Status, wfMain);
            return sID;
        }

        /// <summary>
        /// this function using for Update (update, save and send) when document Returned
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string UpdateHardwareRequiment(HardwareRequirement o, bool isSaveDraff)
        {
            string _sApprovedUser = "";
            WFMainDetail wfMainDetail = new WFMainDetail();
            bool isReturned = (o.Status == (int)HardwareRequirementManager.OrderStatus.RETURNED) ? true : false;

            //Master
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.HardwareRequirementDetails = HardwareRequimentDetailList();

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
                    Temp1="0",
                    Temp2="",
                    Temp3=""
                };
            }
            bool _result = new HardwareRequirementManager().UpdateItem(o, isSaveDraff, isReturned, wfMainDetail,
                                                                         i => i.Description, i => i.Reason, i => i.CreateDate,
                                                                         i => i.IsUrgent, i => i.CurrencyID, i => i.EstimatedAmount);

            ///Sent mail to next approver
            if (!isSaveDraff && _result)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Reason,"");
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
                                            Temp1="0",
                                            Temp2="",
                                            Temp3=""
                                        }
                                      );

            return wfMain;
        }

        /// <summary>
        /// Get HardwareRequirementDetail to save to DB
        /// </summary>
        /// <returns></returns>
        private List<HardwareRequirementDetail> HardwareRequimentDetailList()
        {
            List<HardwareRequirementDetail> lst = new List<HardwareRequirementDetail>();
            if (Session[SessionTmpName] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session[SessionTmpName];
                HardwareRequirementDetail harddetail;
                foreach (ItemInfo i in lstItemInfo)
                {
                    harddetail = new HardwareRequirementDetail()
                    {
                        Description = i.Des,
                        ItemNo = "",
                        ItemDetailID = i.ItemDetailID,
                        ItemID = i.ItemID,
                        Quantity = i.Quantity,
                        UnitID = i.UnitID,
                        DeliveryDate = i.DeliveryDate,
                        EstimatedPrice = i.Price,
                        EstimatedAmount = i.Amount,
                        Temp1 = 0,
                        Temp2 = 0
                    };
                    lst.Add(harddetail);
                }
            }
            return lst;
        }
        #endregion


        #region "User Manage Order"
        public ActionResult HardwareRequimentManagement()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {

            List<HardwareRequirement> lstItem = new HardwareRequirementManager().GetItems(Helper.UserLoginInfo.UserId);
            return GetGridView(lstItem, Models.Helper.PartialParameter.HARDWARE_REQUIREMENT_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string ID)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.HardwareRequirementManager().DeleteItem(ID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteDeviceRegFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(new HardwareRequirementManager().GetItems(Helper.UserLoginInfo.UserId), Models.Helper.PartialParameter.HARDWARE_REQUIREMENT_LIST_GRID);
        }
        #endregion

        public ActionResult SignDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] HardwareRequirement o)
        {
            int Status = 0;
            string _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            HardwareRequirement hardware = new HardwareRequirementManager().GetItem(o.ID);
            if (new FEA_BusinessLogic.WaitingArea.WaitingArea().CheckAlreadySigned(MainDetailID) == 0)
            {
                Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Sign", _sApprovedUser, comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.HARD_REGISTRATION, hardware.User1.CostCenterCode, NodeID /*current node*/, o.OrderCode, hardware.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, hardware.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);
            }
            else
            {
                Status = 0;
            }
            #region Removed by Steven 2014-09-03
            //FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);

            //List<sp_GetApprover_Result> ApproverList = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetApprover(FEA_ITS_Site.Models.Helper.TagPrefixParameter.HARD_REGISTRATION, hardware.User1.CostCenterCode, NodeID);
            //if (ApproverList.Count == 0)
            //{
            //    _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateEndWorkFlow(o.OrderCode, FEA_ITS_Site.Models.Helper.TagPrefixParameter.HARD_REGISTRATION, MainDetailID, comment);
            //    if(_result)
            //        Controllers.HelperController.SendMailToITServices(hardware.CreatorID.Value, string.Format("{0}-", hardware.OrderCode) + hardware.Reason, comment);
            //     End workflow
            //}
            //else
            //{
            //    _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateNextWorkFlow(_sApprovedUser, MainDetailID, Session["NodeID"].ToString(), MainID, comment, Helper.UserLoginInfo.UserId);
            //     Find next Approver

            //     Send Email
            //    if(_result)
            //        Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), Helper.UserLoginInfo.UserId,hardware.Reason, comment);
            //     Find next Approver
            //}
            //switch (_result)
            //{
            //    case true:
            //        Status = (int)Models.Helper.EditItemStatus.success;
            //        break;
            //    case false:
            //        Status = (int)Models.Helper.EditItemStatus.failed;
            //        break;
            //    default:
            //        Status = (int)Models.Helper.EditItemStatus.nomal;
            //        break;
            //}

            #endregion
            return RedirectToAction("Index", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });

        }

        public ActionResult RejectDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] HardwareRequirement o)
        {
            int Status = 0;
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
            HardwareRequirement hardware = new HardwareRequirementManager().GetItem(o.ID);
            Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Reject", "", comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.HARD_REGISTRATION, hardware.User1.CostCenterCode, NodeID  /*current node*/, o.OrderCode, hardware.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, hardware.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);
            #region Removed by Steven 2014-09-03
            //_result = new FEA_BusinessLogic.WaitingArea.WaitingArea().RejectDocument(device.OrderCode, MainDetailID, FEA_ITS_Site.Models.Helper.TagPrefixParameter.HARD_REGISTRATION, comment, UI.UserID, MainID, Convert.ToInt32(device.CreatorID));
            //switch (_result)
            //{
            //    case true:
            //        Status = (int)Models.Helper.EditItemStatus.success;
            //        Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(device.CreatorID), Helper.UserLoginInfo.UserId,device.Reason, comment);       // Added by Steven 2014-08-21
            //        break;
            //    case false:
            //        Status = (int)Models.Helper.EditItemStatus.failed;
            //        break;
            //    default:
            //        Status = (int)Models.Helper.EditItemStatus.nomal;
            //        break;
            //}
            #endregion
            return RedirectToAction("Index", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });
        }
    
    }
}
