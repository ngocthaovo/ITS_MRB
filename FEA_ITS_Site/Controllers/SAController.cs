using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using FEA_BusinessLogic;
using FEA_SABusinessLogic;
using FEA_ITS_Site.Models;
using FEA_ITS_Site.Models.Helper;
using DevExpress.XtraPivotGrid;
using DevExpress.Utils;
using System.Web.UI;
using DevExpress.Data.PivotGrid;
using DevExpress.Web;
using DevExpress.XtraPrintingLinks;
using System.IO;
using DevExpress.XtraPrinting;

namespace FEA_ITS_Site.Controllers
{
    public class SAController : BaseController
    {
        //
        // GET: /SA/

        #region "Main Page"
        public ActionResult ApplicationMainPage()
        {
            return View();
        }
        #endregion

        #region Master Information
        public ActionResult Application(string ID, int Type, string NodeID, string TypeUser, string MainDetailID, string MainID
                                        , int? CheckUserID, string DelegateID, int? DelegateUserID, int? editStatus
                                        , string ExportItemAdjustID = "", string ExportItemAdjustOrder = "")
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/SA/Application?Type=" + Type) });
            //

            ExportItem item = null;
            User uInfo = new User();
            Boolean bIsCreateNew = false;

            FEA_ITS_Site.Models.ItemUpload ItemUpload = new ItemUpload();
            ItemUpload.Type = FEA_ITS_Site.Models.Helper.TagPrefixParameter.SECURITYAREA;

            if (ID == null || ID.Trim().Length == 0) // tạo mới
            {
                uInfo = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
                item = new ExportItem()
                {
                    OrderCode = Controllers.HelperController.GenerateCode(FEA_ITS_Site.Models.Helper.TagPrefixParameter.SECURITYAREA),
                    Status = (int)ExportItemManager.OrderStatus.New,
                    OrderType = Type,
                    ExportItemAdjustID = ExportItemAdjustID,
                    CreateDate = DateTime.Now,
                    ID = Guid.NewGuid().ToString()
                };
                // Item Detail
                Session["ItemInfoList"] = new List<ItemInfo>();
                bIsCreateNew = true;


                //For ExportItemAdjust
                ViewBag.ExportItemAdjustID = ExportItemAdjustID;
                ViewBag.ExportItemAdjustOrder = ExportItemAdjustOrder;

                //Upload file
                ItemUpload.Guid = item.ID;
            }
            else
            {
                item = new ExportItemManager().GetItem(ID);
                ItemUpload.Guid = item.ID; // Get GuID 
                if (item.AttachmentLink == null)
                {
                    item.AttachmentLink = string.Empty;
                }
                if (System.IO.Directory.Exists(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + item.AttachmentLink)) && item.AttachmentLink.Trim().Length != 0)
                {
                    string[] ListItem = null;// Directory.GetFiles(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + deRegItem.AttachmentLink));     // Please test again.
                    ListItem = item.AttachmentLink != null ? Directory.GetFiles(Request.MapPath(FEA_ITS_Site.Helper.Ultilities.Root + item.AttachmentLink)) : null;
                    ItemUpload.ListAddress = ListItem;
                }


                if (item != null)
                {
                    // Get User Logged info
                    uInfo = new UserManager().GetItem(item.CreatorID.Value);
                    Session["ItemInfoList"] = ConvertToItemInfos(item.ExportItemDetails);

                    //GetApprover - this is partial
                    bIsCreateNew = false;

                    //For ExportItemAdjust
                    ViewBag.ExportItemAdjustID = item.ExportItemAdjustID;
                    ViewBag.ExportItemAdjustOrder = item.ExportItem2 == null ? "" : item.ExportItem2.OrderCode;
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

            ViewBag.DocTitle = GetDocumentName(item.OrderType.Value);
            

            //Attachment link
            ViewBag.ItemUploadGuid = ItemUpload.Guid;
            Session["ItemUpload"] = ItemUpload;

            return View(item);
        }

        public static string GetDocumentName(int Type)
        {
            string _sResult = Resources.Resource.STATUS_UNKNOW;
            if (Type == (int)ExportItemManager.OrderType.Normal)
                _sResult = Resources.Resource.SA_NomalApplication;
            else if (Type == (int)ExportItemManager.OrderType.Lend)
                _sResult = Resources.Resource.SA_LendApplication;
            else if (Type == (int)ExportItemManager.OrderType.Borrow)
                _sResult = Resources.Resource.SA_BorrowApplication;
            else if (Type == (int)ExportItemManager.OrderType.Donative)
                _sResult = Resources.Resource.SA_DonativeApplication;
            else if (Type == (int)ExportItemManager.OrderType.Adjust)
                _sResult = Resources.Resource.SA_AdjustApplication;
            return _sResult;
        }

        public static Dictionary<int,string> GetDocumentList()
        {
            Dictionary<int, string> lst = new Dictionary<int, string>();

            lst.Add((int)ExportItemManager.OrderType.Normal, Resources.Resource.SA_NomalApplication);
            lst.Add((int)ExportItemManager.OrderType.Lend, Resources.Resource.SA_LendApplication);
            lst.Add((int)ExportItemManager.OrderType.Borrow, Resources.Resource.SA_BorrowApplication);
            lst.Add((int)ExportItemManager.OrderType.Donative, Resources.Resource.SA_DonativeApplication);
            lst.Add((int)ExportItemManager.OrderType.Adjust, Resources.Resource.SA_AdjustApplication);
            return lst;
        }


        [HttpPost]
        public ActionResult SaveData([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] FEA_BusinessLogic.ExportItem o, bool CreateNew)
        {
            string sResult = "";

            if (Request.Form["btnSaveDraff"] != null) //Save Draft
                sResult = CreateItem(o, true);
            else if (Request.Form["btnSaveAndSend"] != null) // Save And Send
                if (CreateNew)
                    sResult = CreateItem(o, false);//isSaveDraff = false - Create Order;
                else
                    sResult = UpdateItem(o, false);//isSaveDraff = false - Update Order;
            else if (Request.Form["btnUpdate"] != null) //Update - 
            {
                sResult = UpdateItem(o, true);// UpdateDeviceRegistration(o, true);
            }
            if (sResult.Length > 0)
                return RedirectToAction("Application", new { ID = sResult, Type = o.OrderType, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.success });// Index(sResult,"","", (int)Models.Helper.EditItemStatus.success);
            else
                return RedirectToAction("Application", new { ID = sResult, Type = o.OrderType, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.failed });//Index(sResult, "", "", (int)Models.Helper.EditItemStatus.failed);

        }

        /// <summary>
        /// Create Device Registration Information
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <param name="wfMain"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string CreateItem(ExportItem o, bool isSaveDraff)
        {
            //Master
            string _sApprovedUser = "";
            WFMain wfMain = new WFMain();
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.ExportItemDetails = ExportItemDetailList();
            o.OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.SECURITYAREA);
            o.Temp1 = o.Temp2 = "";
            o.Destination = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboDestinationList");
            o.ReasonID = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("ReasonID");
            o.ReasonName = Request.Form["ReasonID"];

            o.AttachmentLink = FEA_ITS_Site.Helper.Ultilities.SAUploadFolder + o.ID; // -- Added Attachment link
            //For Adjust Document



            if (isSaveDraff)
                o.Status = (int)ExportItemManager.OrderStatus.DRAFT;
            else
            {
                if (o.OrderType == (int)ExportItemManager.OrderType.Donative) // Nếu là tặng cho hàng thành phẩm thì ko cần phải ký
                {
                    o.Status = (int)ExportItemManager.OrderStatus.CHECKED;
                }
                else
                {
                    o.Status = (int)ExportItemManager.OrderStatus.SENDING;

                    _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");

                    wfMain = GetWFmain(o.OrderCode, _sApprovedUser);
                }

            }
            ///Sent mail to next approver
            if (!isSaveDraff && _sApprovedUser.Length > 0)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.ReasonName, "");
            //End sent mail

            string sID = new FEA_SABusinessLogic.ExportItemManager().InsertItem(o, (ExportItemManager.OrderStatus)o.Status, wfMain);
            return sID;
        }

        /// <summary>
        /// this function using for Update (update, save and send) when document Returned
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string UpdateItem(ExportItem o, bool isSaveDraff)
        {
            string _sApprovedUser = "";
            WFMainDetail wfMainDetail = new WFMainDetail();
            bool isReturned = (o.Status == (int)ExportItemManager.OrderStatus.RETURNED) ? true : false;

            //Master
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.ExportItemDetails = ExportItemDetailList();
            o.Destination = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboDestinationList");
            o.ReasonID = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("ReasonID");

            if (!isSaveDraff)
            {

                if (o.OrderType == (int)ExportItemManager.OrderType.Donative) // Nếu là tặng cho hàng thành phẩm thì ko cần phải ký
                {
                    o.Status = (int)ExportItemManager.OrderStatus.CHECKED;
                }
                else
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
            }
            bool _result = new ExportItemManager().UpdateItem(o, isSaveDraff, isReturned, wfMainDetail,
                                                                         i => i.CreatorID, i => i.Destination, i => i.PersonalName, i => i.Description, i => i.ReasonID);

            ///Sent mail to next approver
            if (!isSaveDraff && _result)
                if (o.OrderType != (int)ExportItemManager.OrderType.Donative)
                    Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Description, "",GetDocumentTypeName((int)o.OrderType,true));
            //End sent mail
            return _result == true ? o.ID : "";
        }

        /// <summary>
        /// Get HardwareRequirementDetail to save to DB
        /// </summary>
        /// <returns></returns>
        private List<ExportItemDetail> ExportItemDetailList()
        {
            List<ExportItemDetail> lst = new List<ExportItemDetail>();
            if (Session["ItemInfoList"] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session["ItemInfoList"];
                ExportItemDetail expItemDetail;
                foreach (ItemInfo i in lstItemInfo)
                {

                    expItemDetail = new ExportItemDetail()
                    {
                        ID = Guid.NewGuid().ToString(),
                        ItemDetailID = i.ItemDetailID,
                        
                        Specification = i.Spec,
                        Note = i.Des,
                        Sex = i.Gender,
                        ReturnDate = i.DeliveryDate,
                        Quantity = i.Quantity,
                        UnitID = i.UnitID,
                        Temp1 = "",
                        Temp2 = "",

                        CostCenterCode = i.CostCenterCode,
                        ItemID = i.ItemID

                    };
                    lst.Add(expItemDetail);
                }
            }
            return lst;
        }

        /// <summary>
        /// Get Total quantity 
        /// </summary>
        /// <returns></returns>
        private int GetTotalQuantity()
        {
            List<ExportItemDetail> lst = new List<ExportItemDetail>();
            int TotalQuantity = 0;
            if (Session["ItemInfoList"] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session["ItemInfoList"];
                foreach (ItemInfo i in lstItemInfo)
                {
                    TotalQuantity += i.Quantity;
                }
            }
            return TotalQuantity;
        }


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
                                            NodeID = Session["NodeID"] == null ? "" : Session["NodeID"].ToString(), // Get from GetApproverPartial() function
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
        /// Convert List of HardwareRequirementDetail to List of ItemInfo
        /// </summary>
        /// <param name="HardRgDetails"></param>
        /// <returns></returns>
        private List<ItemInfo> ConvertToItemInfos(ICollection<ExportItemDetail> HardRgDetails)
        {
            List<ItemInfo> lstResult = new List<ItemInfo>();
            ItemInfo _itemInfo;
            foreach (ExportItemDetail dvResDetailItem in HardRgDetails)
            {
                _itemInfo = new ItemInfo()
                {
                    ID = dvResDetailItem.ID,
                    ItemDetailID = dvResDetailItem.ItemDetailID,
                    ItemDetailName = dvResDetailItem.ItemDetail.ItemDetailName,

                   // ItemDetailID = "-1",
                    Spec = dvResDetailItem.Specification,

                    Des = dvResDetailItem.Note,
                    Gender = dvResDetailItem.Sex,
                    Quantity = (int)dvResDetailItem.Quantity.Value,
                    DeliveryDate = dvResDetailItem.ReturnDate,


                    UnitID = dvResDetailItem.ItemDetail.Temp2,
                    UnitName = new FEA_BusinessLogic.UnitManager().GetItem(dvResDetailItem.ItemDetail.Temp2).NAME,

                    CostCenterCode = (dvResDetailItem.CostCenterCode == null) ? null : dvResDetailItem.CostCenterCode,
                    CostCenterName = (dvResDetailItem.CostCenter == null) ? "" : dvResDetailItem.CostCenter.Remark,
                    
                    ItemID = dvResDetailItem.ItemID,
                    ItemName = dvResDetailItem.ItemDetail.Item.ItemName
                };

                lstResult.Add(_itemInfo);
            }
            return lstResult;
        }

        /// <summary>
        /// Lấyd danh sách SADestination
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult DestinationListPartial(ExportItem item)
        {

            return PartialView("_DestinationList", item);
        }
        #endregion

        #region "Member Managerment"
        public ActionResult ExportManagement(int Type)
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/SA/ExportManagement?Type=" + Type) });
            //

            ViewBag.Type = Type;
            return View();
        }

        [ValidateInput(false)]
        public ActionResult EditModesPartial(int OrderType)
        {
            ViewData["Type"] = OrderType;
            List<ExportItem> lstItem = new FEA_SABusinessLogic.ExportItemManager().GetItems(Helper.UserLoginInfo.UserId, OrderType);
            return GetGridView(lstItem, Models.Helper.PartialParameter.SA_ITEM_LIST_GRID);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string ID, int OrderType)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_SABusinessLogic.ExportItemManager().DeleteItem(ID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteDeviceRegFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return EditModesPartial(OrderType);
        }
        #endregion
        #region Item detail Region

        public ActionResult GetListItemDetailPartial(int type, bool? ShowCommand = false)
        {
            ViewData["ShowCommand"] = ShowCommand;
            ViewData["OrderType"] = type;

            return GetGridView(Session["ItemInfoList"], Models.Helper.PartialParameter.ITEM_DETAIL_SA_LIST_GRID);
        }

        [HttpPost]
        public string CheckLimited(string ReasonType)
        {
            string sResult = string.Empty;

            // Check for permission
            if (!Helper.UserLoginInfo.IsLogin)
            {
                sResult += "Please login into system.";
                return sResult;
            }

            // Check for reason
            if (ReasonType == null || ReasonType.Length == 0)
            {
                return "Please select the reason";

            }

            //Check for item limited
            int TotalQuantity = GetTotalQuantity();


            sp_CheckLimitForDonative_Result item = new FEA_BusinessLogic.Base.Connection().db.sp_CheckLimitForDonative(ReasonType, Helper.UserLoginInfo.UserId, DateTime.Now, TotalQuantity).SingleOrDefault();

            if (item != null)
            {
                if (item.Quantity < 0)
                    sResult += Resources.Resource.msgInvalidDonateQuantity + item.Quantity.ToString();
            }
            return sResult;
        }
        /// <summary>
        /// Add Item and ItemDetail to Grid
        /// </summary>
        /// <param name="ItemDetailID"></param>
        /// <param name="ItemDetail"></param>
        /// <param name="Description"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddItemToOrder(int OrderType, string CostCenterCodeID, string ItemID, string ItemDetailID, string Gender, string Spec, string Description, DateTime? ReturnedDate, int? Quantity = 0, string ExportItemAdjustID = "")
        {

            if (ItemDetailID.Trim().Length == 0 || ItemDetailID == null
                || Quantity <=0
                || (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative ? (Gender == null || Gender.Trim().Length == 0 ? true : false) : false)
                || (OrderType != (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust ? (Spec == null || Spec.Trim().Length == 0 ? true : false) : false)
                || (OrderType != (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust && OrderType != (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative ? (Convert.ToDecimal(Quantity) <= 0 || ReturnedDate == null || ReturnedDate.Value.Date < DateTime.Now.Date ? true : false) : false))
            {

                string _sErr = Resources.Resource.msgInputError;
                if (ItemDetailID.Trim().Length == 0 || ItemDetailID == null)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.MaterialName);
                if ((OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative ? (Gender == null || Gender.Trim().Length == 0 ? true : false) : false))
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Gender);
                if (Convert.ToDecimal(Quantity) <= 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Quantity);

                if (OrderType != (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust)
                {
                    if (Spec == null || Spec.Trim().Length == 0)
                        _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Specs);

                }

                if (OrderType != (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative && OrderType != (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust)
                {
                    if (ReturnedDate == null || ReturnedDate.Value.Date < DateTime.Now.Date)
                        _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.ReturnedDate);
                }

                ViewData["EditError"] = _sErr;
            }
            else
            {

                bool flag1 = false;
                bool flag2 = false;

                if (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust)
                {

                    flag1 = FindItemDetailInList(ItemDetailID);
                }
                else
                {
                    flag1 = FindItemDetailInList(ItemDetailID, int.Parse(CostCenterCodeID), ItemID);
                    flag2 = FindItemDetailInListCostCenterConditions(int.Parse(CostCenterCodeID));
                }


                int? iCostCenterCodeID = null;
                if (int.Parse(CostCenterCodeID) > 0) iCostCenterCodeID = int.Parse(CostCenterCodeID);


                if (flag1) // Đã tồn tại trong list
                    ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                if(flag2) // khác phòng phát hàng
                    ViewData["EditError"] = Resources.Resource.msgDataduplicateCostCenterCode;
                //if(flag3)
                //    ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                else if(flag1==false && flag2==false)
                {
                    try
                    {
                        //If Order Type was: Adjust, must be check quantity in ExportItemChecking and ExportitemDetail, 
                        flag1 = true;
                        flag2 = true;

                        if (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust)
                        {
                            var reQuantity = new FEA_SABusinessLogic.ExportItemDetailManager().CheckForItemDetail(ExportItemAdjustID, ItemDetailID, Quantity.Value);
                            if (reQuantity < 0)
                            {
                                ViewData["EditError"] = Resources.Resource.msgInvalidQuantityAdjust + " " + reQuantity.ToString();
                                flag1 = false;
                                flag2 = false;
                            }
                        }

                        if (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative)
                        {
                            //Check for limit

                        }

                        if (flag1 || flag2)
                        {
                            ItemDetail item_detail = new ItemDetailManager().GetItem(ItemDetailID);
                            Unit unit = new UnitManager().GetItem(item_detail == null ? "" : item_detail.Temp2); // temp2 was unit id
                            CostCenter ce = new FEA_BusinessLogic.CodeCenterManager().GetItem(int.Parse(CostCenterCodeID));
                            Models.ItemInfo item = new ItemInfo()
                            {
                                ID = Guid.NewGuid().ToString(),

                                ItemDetailID = item_detail.ID,
                                ItemDetailName = item_detail.ItemDetailName,

                                //ItemDetailID = "-1",
                                Spec = Spec,

                                Des = Description,
                                Gender = Gender,
                                Quantity = Quantity.Value,
                                DeliveryDate = ReturnedDate,


                                UnitID = unit.ID,
                                UnitName = unit.NAME,

                                CostCenterCode = iCostCenterCodeID,
                                CostCenterName = (ce==null)?"":ce.Remark,

                                ItemID = ItemID,
                                ItemName = item_detail.Item.ItemName
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
            ViewData["OrderType"] = OrderType;
            return GetGridView(Session["ItemInfoList"], Models.Helper.PartialParameter.ITEM_DETAIL_SA_LIST_GRID);
        }

        /// <summary>
        /// true:
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        //private bool CheckItemInList(string ItemID) // Only one ItemID in list
        //{
        //    List<ItemInfo> lst = new List<ItemInfo>();
        //    if (Session["ItemInfoList"] != null)
        //    {
        //        lst = (List<ItemInfo>)Session["ItemInfoList"];
        //        if (lst.Count == 0)
        //            return true;
        //        else return lst.Where(i => i.ItemID == ItemID).Count() > 0 ? true : false;
        //    }
        //    return true;

        //}
        //private bool FindItemDetailInList(string ItemDetailID/*, int costCenterCode, string itemID*/)
        //{
        //    List<ItemInfo> lst = new List<ItemInfo>();
        //    if (Session["ItemInfoList"] != null)
        //        lst = (List<ItemInfo>)Session["ItemInfoList"];

        //    return lst.Where(i => i.ItemDetailID == ItemDetailID
        //                      //|| (i.CostCenterCode!=costCenterCode)
        //                      //|| ((i.CostCenterCode==costCenterCode) && (i.ItemID==itemID) && (i.ItemDetailID==ItemDetailID))
        //                      ).Count() > 0 ? true : false;

        //}
        private bool FindItemDetailInList(string ItemDetailID, int costCenterCode, string itemID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];

            return lst.Where(i => ((i.CostCenterCode == costCenterCode) && (i.ItemID == itemID) && (i.ItemDetailID == ItemDetailID))).Count() > 0 ? true : false;
        }

        private bool FindItemDetailInListCostCenterConditions(int costCenterCode)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];

            return lst.Where(i =>i.CostCenterCode!=costCenterCode).Count() > 0 ? true : false;
        }

        private bool FindItemDetailInList(string ItemDetailID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];

            return lst.Where(i => i.ItemDetailID == ItemDetailID).Count() > 0 ? true : false;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeleteItemDetail(string ID, int type)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoList"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoList"];

            ItemInfo i = lst.Where(o => o.ID == ID).SingleOrDefault();
            lst.Remove(i);

            Session["ItemInfoList"] = lst;
            ViewData["ShowCommand"] = true;
            ViewData["OrderType"] = type;
            return GetGridView((List<ItemInfo>)Session["ItemInfoList"], Models.Helper.PartialParameter.ITEM_DETAIL_SA_LIST_GRID);
        }


        public ActionResult ExportItemCheckingListPartial(string ExportItemDetailID)
        {

            List<ExportItemChecking> lst = new FEA_SABusinessLogic.ExportItemCheckingManager().GetItems(ExportItemDetailID);
            ViewData["ExportItemDetailID"] = ExportItemDetailID;
            return GetGridView(lst, "ExportItemChecking_List_Grid");
        }
        #endregion

        #region Sign Document
        public ActionResult SignDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] ExportItem o)
        {
            int Status = 0;
            string _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            ExportItem item = new ExportItemManager().GetItem(o.ID);
            if (new FEA_BusinessLogic.WaitingArea.WaitingArea().CheckAlreadySigned(MainDetailID) == 0)
            {
                Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Sign", _sApprovedUser, comment
                                                                        , FEA_ITS_Site.Models.Helper.TagPrefixParameter.SECURITYAREA, item.User.CostCenterCode
                                                                        , NodeID /*current node*/, o.OrderCode, item.Description, MainID
                                                                        , MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/
                                                                        , item.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID
                                                                        , false, GetDocumentTypeName(item.OrderType.Value,true));
            }
            return RedirectToAction("Application", new { ID = o.ID, Type = o.OrderType, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });

        }
        public ActionResult RejectDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] ExportItem o)
        {
            int Status = 0;
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
            ExportItem item = new ExportItemManager().GetItem(o.ID);
            Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Reject", "", comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.SECURITYAREA
                , item.User.CostCenterCode, NodeID  /*current node*/, o.OrderCode
                , item.Description, MainID, MainDetailID
                , Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/
                , item.CreatorID.Value, CheckUserID, DelegateID
                , DelegateUserID,false, GetDocumentTypeName(item.OrderType.Value,true));

            return RedirectToAction("Application", new { ID = o.ID, Type = o.OrderType, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });
        }
        #endregion

        #region Securiry Area

        /// <summary>
        /// Get document need be approve for sa
        /// </summary>
        /// <returns></returns>
        public ActionResult SecurityArea()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/SA/SecurityArea") });
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
                            List<string> lstTosign = values.Select(i => i.Key).ToList<string>();

                            string sError = CheckSubmitValidate(values);
                            if (sError.Length == 0)
                            {
                                if (lstTosign != null && lstTosign.Count > 0)
                                {
                                    int status = new FEA_SABusinessLogic.ExportItemManager().AdminApproveOrder(lstTosign, Helper.UserLoginInfo.UserId);
                                    if (status <= 0)
                                    {
                                        if (status == -1)
                                            ViewBag.EditInfo = "Can not update the data, please check your operate.";

                                        ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                                        return View();
                                    }
                                    else
                                        iResult += status;
                                }


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
                            else
                            {
                                ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                                ViewBag.EditInfo = sError;
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

            return View();
        }

        private string CheckSubmitValidate(Dictionary<string, string> values)
        {
            string sResult = string.Empty;

            foreach (var item in values)
            {

                //Check validation for Adjust Application
                if (int.Parse(item.Value) == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust)
                {
                    // Check for Quantity
                    var exportItem = new FEA_BusinessLogic.Base.Connection().db.ExportItems.Where(i => i.OrderCode == item.Key && (i.Status == (int)FEA_SABusinessLogic.ExportItemManager.OrderStatus.CHECKED)).SingleOrDefault();
                    if (exportItem != null)
                    {
                        foreach (ExportItemDetail ItemDetail in exportItem.ExportItemDetails)
                        {
                            decimal rQuantity = new FEA_SABusinessLogic.ExportItemDetailManager().CheckForItemDetail(exportItem.ExportItemAdjustID, ItemDetail.ItemDetailID, ItemDetail.Quantity.Value);
                            if (rQuantity < 0)
                            {
                                sResult += Resources.Resource.msgInvalidQuantityAdjust + ", tại: " + item.Key + " - " + ItemDetail.ItemDetail.ItemDetailName + " - số lượng: " + rQuantity + "</br>";
                            }
                        }

                    }
                    else { sResult += "không tìm thấy đơn: " + item.Key; }
                }


                // Check validation for Donate Application
                if (int.Parse(item.Value) == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative)
                {
                    // Check for Quantity
                    var exportItem = new FEA_BusinessLogic.Base.Connection().db.ExportItems.Where(i => i.OrderCode == item.Key && (i.Status == (int)FEA_SABusinessLogic.ExportItemManager.OrderStatus.CHECKED)).SingleOrDefault();
                    if (exportItem != null)
                    {
                        int? totalQuantity = exportItem.ExportItemDetails.Sum(i => (int)i.Quantity);
                        if (totalQuantity == null) totalQuantity = 0;

                        sp_CheckLimitForDonative_Result CheckLimitForDonative = new FEA_BusinessLogic.Base.Connection().db.sp_CheckLimitForDonative(exportItem.ReasonID, exportItem.CreatorID.Value, DateTime.Now, totalQuantity.Value).SingleOrDefault();
                        if (CheckLimitForDonative != null)
                        {
                            if (CheckLimitForDonative.Quantity < 0)
                                sResult += "Số lượng người dùng muốn tặng cho còn lại không đủ, tại: " + item.Key + ", số lượng còn: " + CheckLimitForDonative.Quantity.ToString() + "</br>";
                        }
                    }
                    else { sResult += "không tìm thấy đơn: " + item.Key; }

                }
            }
            return sResult;
        }

        [ValidateInput(false)]
        public ActionResult ApprovePartial()
        {

            var frm = new FEA_SABusinessLogic.ExportItemManager().GetItemsForApprove();
            return GetGridView(frm, Models.Helper.PartialParameter.SA_PPROVE_ORDER_GRID);
        }

        public ActionResult FinishedManagementForSA()
        {
            object x = new FEA_SABusinessLogic.ExportItemManager().GetItemsForHistory();
            return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.FinishedForSA);
        }

        /// <summary>
        /// For sa process document (Export and Import good)
        /// </summary>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ProcessApplication(string OrderID)
        {
            ExportItem item = new FEA_SABusinessLogic.ExportItemManager().GetItem(OrderID);

            Session["ItemInfoListForSAProcess"] = new List<ItemInfo>();
            Session["ItemInfoList"] = ConvertToItemInfos(item.ExportItemDetails);

            return View(item);
        }
        [HttpPost]
        public ActionResult ProcessApplication([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] FEA_BusinessLogic.ExportItem item)
        {
            if (Request["btnAccept"] != null)
            {
                try
                {
                    //Save to DB
                    List<ItemInfo> lst = (List<ItemInfo>)Session["ItemInfoListForSAProcess"];
                    List<ExportItemChecking> lstExportItem = new List<ExportItemChecking>();
                    ExportItemChecking exportItem;

                    var flag = true;
                    //string sOperation = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("chkCheckType");
                    string sValidate = string.Empty;

                    if (lst.Count == 0) //-- Added by Steven 2017-04-19 -- Check open multi tabs in processing stage
                    {
                        flag = false;
                        ViewBag.EditStatus = Resources.Resource.msgSelectItem;
                    }
                    else
                    {
                        foreach (ItemInfo iInfo in lst)
                        {
                            sValidate += CheckItemDetailProcessValidate(item.ID, iInfo.Operate, iInfo.ItemID, iInfo.SAQuantity, 999999999, item.OrderType.Value);

                            if (sValidate.Length == 0)
                            {
                                exportItem = new ExportItemChecking()
                                {
                                    CreateDate = DateTime.Now,
                                    CreatorID = FEA_ITS_Site.Helper.UserLoginInfo.UserId,
                                    ExportItemDetailID = iInfo.ItemDetailID,
                                    Note = iInfo.Des,
                                    OperationType = iInfo.Operate,
                                    Quantity = iInfo.SAQuantity,
                                    Temp1 = item.OrderType.ToString(), // assign OrderType for Temp1 column
                                    Temp2 = ""
                                };
                                lstExportItem.Add(exportItem);
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                    }
                                     

                    if (flag)
                    {
                        // Insert to database          
                        int result = new FEA_SABusinessLogic.ExportItemCheckingManager().InsertItems(lstExportItem);

                        



                        if(result > 0)
                        {
                            //if OrderType is Normal, we will closed order
                            if (item.OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Normal)
                            {
                                
                              item.Status = (int) FEA_SABusinessLogic.ExportItemManager.OrderStatus.FINSHED;
                              flag =   new FEA_SABusinessLogic.ExportItemManager().UpdateStatus(item, i => i.Status);
                              if (flag)
                              {
                                  Session["ItemInfoListForSAProcess"] = new List<ItemInfo>();// Clear session
                              }

                            }
                        }
                        //true;
                        ViewBag.EditStatus = (result > 0) ? (int)Models.Helper.EditItemStatus.success : (int)Models.Helper.EditItemStatus.failed;
                    }
                    else
                    {
                        ViewBag.EditStatus =  (int)Models.Helper.EditItemStatus.failed;
                        ViewBag.EditInfo = sValidate;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.EditStatus = (int)Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = ex.Message;
                }
                Session["ItemInfoListForSAProcess"] = new List<ItemInfo>();// Clear session
            }
            item = new FEA_SABusinessLogic.ExportItemManager().GetItem(item.ID);
            return View(item);
        }

        public ActionResult GetListItemProcessDetailPartial(int type)
        {
            ViewData["OrderType"] = type;

            return GetGridView(Session["ItemInfoListForSAProcess"], Models.Helper.PartialParameter.ITEM_DETAIL_SA_PROCESS_LIST_GRID);
        }

        public ActionResult EditModesDeleteItemProcessDetail(string ID, int type)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoListForSAProcess"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoListForSAProcess"];

            ItemInfo i = lst.Where(o => o.ID == ID).SingleOrDefault();
            lst.Remove(i);

            ViewData["OrderType"] = type;
            return GetGridView(Session["ItemInfoListForSAProcess"], Models.Helper.PartialParameter.ITEM_DETAIL_SA_PROCESS_LIST_GRID);
        }

        [HttpPost]
        public ActionResult AddItemToProcess(string OrderID, int OrderType, string ExportItemDetailID, decimal Quantity, string Description, string Operate)
        {
            if (ExportItemDetailID == null || ExportItemDetailID.Length == 0 || Quantity <= 0 || Operate == null || Operate.Length == 0)
            {

                string _sErr = Resources.Resource.msgInputError;
                if (ExportItemDetailID.Trim().Length == 0 || ExportItemDetailID == null)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.MaterialName);

                if (Convert.ToDecimal(Quantity) <= 0)
                    _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Quantity);

                if (Operate.Trim().Length == 0 || Operate == null)
                    _sErr += string.Format("<br/>" + "Vui lòng chọn thao tác xuất nhập");
                ViewData["EditError"] = _sErr;
            }
            else
            {
                bool flag = FindItemDetailInProcessList(ExportItemDetailID);
                if (flag) // Đã tồn tại trong list
                    ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                else
                {
                    try
                    {
                        //If Order Type was: Adjust, must be check quantity in ExportItemChecking and ExportitemDetail, 


                        ExportItemDetail exportItemDetail = new FEA_SABusinessLogic.ExportItemDetailManager().GetItem(ExportItemDetailID);
                        string sValidate = CheckItemDetailProcessValidate(OrderID, Operate, exportItemDetail.ItemDetailID, Quantity, exportItemDetail.Quantity.Value, OrderType);
                        if (sValidate.Length == 0)
                            flag = true;
                        else
                        {
                            flag = false;
                            ViewData["EditError"] = sValidate;
                        }

                        if (flag)
                        {
                            ItemDetail item_detail = exportItemDetail.ItemDetail;
                            Unit unit = new UnitManager().GetItem(item_detail == null ? "" : item_detail.Temp2); // temp2 was unit id

                            Models.ItemInfo item = new ItemInfo()
                            {
                                ID = Guid.NewGuid().ToString(),
                                ItemID = item_detail.ID,
                                ItemName = item_detail.ItemDetailName,

                                ItemDetailID = ExportItemDetailID,

                                Des = Description,
                                SAQuantity = Quantity,

                                UnitID = unit.ID,
                                UnitName = unit.NAME,

                                Operate = Operate,
                            };


                            List<ItemInfo> lst = new List<ItemInfo>();
                            if (Session["ItemInfoListForSAProcess"] != null)
                            {
                                lst = (List<ItemInfo>)Session["ItemInfoListForSAProcess"];
                                lst.Add(item);
                                Session["ItemInfoListForSAProcess"] = lst;
                            }
                            else
                            {
                                lst.Add(item);
                                Session["ItemInfoListForSAProcess"] = lst;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewData["EditError"] = ex.Message;
                    }
                }
            }

            ViewData["OrderType"] = OrderType;
            return GetGridView(Session["ItemInfoListForSAProcess"], Models.Helper.PartialParameter.ITEM_DETAIL_SA_PROCESS_LIST_GRID);

        }

        /// <summary>
        /// Check for validate Item
        /// </summary>
        /// <param name="OrderID"></param>
        /// <param name="Operate"></param>
        /// <param name="ItemDetailID"></param>
        /// <param name="Quantity"></param>
        /// <returns></returns>
        private string CheckItemDetailProcessValidate(string OrderID, string Operate, string ItemDetailID, decimal Quantity, decimal OrderQuantity, int OrderType)
        {
            string sResult = string.Empty;

            if(OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Normal)
            {
                if (Quantity > OrderQuantity)
                {
                    sResult += " Số lượng không hợp lệ, số lượng chênh lệch: " + (OrderQuantity - Quantity).ToString() + "</br> ";
                }
            }
            else
            {           
                var reQuantity = new FEA_BusinessLogic.Base.Connection().db.sp_CheckItemQuantityForSecurity(OrderID, Operate, ItemDetailID, Quantity).SingleOrDefault();
                if (Operate == "OUT" && reQuantity.Quantity < 0)
                {
                    sResult +=  reQuantity.ItemDetailName + ", Số lượng không hợp lệ, số lượng chênh lệch: " + reQuantity.Quantity.ToString() + "</br> ";
                }
                if (Operate == "IN" && reQuantity.Quantity < 0)
                {
                    sResult +=  reQuantity.ItemDetailName + ", Số lượng không hợp lệ: " + reQuantity.Quantity.ToString() + "</br> ";

                }
            }

            return sResult;
        }



        private bool FindItemDetailInProcessList(string ExportItemDetailID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session["ItemInfoListForSAProcess"] != null)
                lst = (List<ItemInfo>)Session["ItemInfoListForSAProcess"];

            return lst.Where(i => i.ItemDetailID == ExportItemDetailID).Count() > 0 ? true : false;

        }

        public ActionResult SAPendingList()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult GetSAPendingListPartial()
        {

            List<ExportItem> lstItem = new FEA_SABusinessLogic.ExportItemManager().GetItemByStatus((int)FEA_SABusinessLogic.ExportItemManager.OrderStatus.SPENDING);
            return GetGridView(lstItem, Models.Helper.PartialParameter.SA_ITEM_PENDING_LIST_GRID);
        }


        public ActionResult SAMainPage()
        {
            return View();
        }
        #endregion

        #region "Common funciton"
        /// <summary>
        /// Get Dcoument typename by Order Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDocumentTypeName(int type, bool ?IsUseEnglish= false)
        {
            string sReturn = "Unknow status";

            if (!IsUseEnglish.Value)
            {
                if (type == (int)ExportItemManager.OrderType.Adjust)
                    sReturn = Resources.Resource.SA_AdjustApplication;
                else if (type == (int)ExportItemManager.OrderType.Borrow)
                    sReturn = Resources.Resource.SA_BorrowApplication;
                else if (type == (int)ExportItemManager.OrderType.Donative)
                    sReturn = Resources.Resource.SA_DonativeApplication;
                else if (type == (int)ExportItemManager.OrderType.Lend)
                    sReturn = Resources.Resource.SA_LendApplication;
                else if (type == (int)ExportItemManager.OrderType.Normal)
                    sReturn = Resources.Resource.SA_NomalApplication;
            }
            else
            {
                if (type == (int)ExportItemManager.OrderType.Adjust)
                    sReturn ="SA Product Adjust Application";
                else if (type == (int)ExportItemManager.OrderType.Borrow)
                    sReturn = "SA Product Borrow Application";
                else if (type == (int)ExportItemManager.OrderType.Donative)
                    sReturn = "SA Product Donative Application";
                else if (type == (int)ExportItemManager.OrderType.Lend)
                    sReturn = "SA Product Lend Application";
                else if (type == (int)ExportItemManager.OrderType.Normal)
                    sReturn = "SA Product Nomal Application";
            }

            return sReturn;
        }
        #endregion

        #region Added by Steven 2015-02-13

        public ActionResult Statistic()
        {
            if (!IsPostback)
            {
                Session["SAStatistic"] = null;
            }
            return View(Session["SAStatistic"] as Object);
        }

        [HttpPost]
        public ActionResult Statistic(FormCollection frm)
        {
            string DateTo;
            string DateFrom;

            if (frm["date"].ToString().Trim() != "" && frm["date2"].ToString().Trim() != "" && frm["date"].ToString().Trim() != "1901-01-01" && frm["date2"].ToString().Trim() != "1901-01-01")
            {
                DateTo = Convert.ToDateTime(frm["date2"]).ToString("yyyy-MM-dd");
                DateFrom = frm["date"];
            }
            else
            {
                DateTo = "2015-01-01";
                DateFrom = "2015-01-01";
                ViewBag.ErrorMessage = "Can't query. Please select date";
            }
            ViewBag.DateTo = DateTo;
            ViewBag.DateFrom = DateFrom;

            Object[] Model = new Object[2];
            Model[0] = new FEA_SABusinessLogic.ExportItemManager().GetSADynamicReport(DateFrom, DateTo);
            Model[1] = new FEA_SABusinessLogic.ExportItemManager().GetDataForSAPivotGrid(DateFrom, DateTo);
            Session["SAStatistic"] = Model;
            return View(Model);
        }

        public ActionResult CallbackPivotGrid(FormCollection frm, string Type, string DateFrom, string DateTo)
        {
            string LoadGridName = string.Empty;
            ViewBag.DateTo = DateTo;
            ViewBag.DateFrom = DateFrom;
            switch (Type)
            {
                case "Pivot":
                    LoadGridName = FEA_ITS_Site.Models.Helper.PartialParameter.GetDataForSAPivotGrid;
                    break;
                case "Dynamic":
                    LoadGridName = FEA_ITS_Site.Models.Helper.PartialParameter.GetSADynamicReport;
                    break;

                default:
                    break;
            }
            return GetGridView(Session["SAStatistic"] as Object, LoadGridName);
        }

        public ActionResult ExportToExcel()
        {
            return ExportTo();
        }

        public ActionResult GetDetailSADynamic(string ItemDetailID, int Type, string DateFrom, string DateTo)
        {
            ViewData["ItemDetailID"] = ItemDetailID + Type.ToString();
            return GetGridView(new FEA_SABusinessLogic.ExportItemManager().GetDetailSADynamicReport(DateFrom, DateTo, Type, ItemDetailID) as List<sp_GetDetailSADynamicReport_Result>, FEA_ITS_Site.Models.Helper.PartialParameter.GetDetailSADynamicReport);
        }

        public ActionResult GetSADynamicReportChart(FormCollection frm, string DateFrom, string DateTo)
        {
            return GetGridView((new FEA_SABusinessLogic.ExportItemManager().GetDataForSAPivotGridUseForChart(DateFrom, DateTo)) as List<sp_GetDataForSAPivotGridUseForChart_Result>, FEA_ITS_Site.Models.Helper.PartialParameter.GetSADynamicReportChart);
        }

        public ActionResult ExportTo()
        {
            Object[] _object = (Object[])Session["SAStatistic"];
            PrintingSystem ps = new PrintingSystem();

            PrintableComponentLink link1 = new PrintableComponentLink(ps);
            PivotGridSettings PivotSetting = PivotGridSetting();
            link1.Component = PivotGridExtension.CreatePrintableObject(PivotSetting, _object[1] as List<sp_GetDataForSAPivotGrid_Result>);

            PrintableComponentLink link2 = new PrintableComponentLink(ps);
            GridViewSettings DynamicGrid = GetGridSettings();

            link2.Component = GridViewExtension.CreatePrintableObject(DynamicGrid, _object[0] as List<sp_GetSADynamicReport_Result>);

            CompositeLink compositeLink = new CompositeLink(ps);
            compositeLink.Links.AddRange(new object[] { link1, link2 });
            compositeLink.EnablePageDialog = true;
            compositeLink.CreateDocument();
            FileStreamResult result = CreateExcelExportResult(compositeLink);
            ps.Dispose();

            return result;
        }

        FileStreamResult CreateExcelExportResult(CompositeLink link)
        {
            MemoryStream stream = new MemoryStream();
            link.PrintingSystem.ExportToXls(stream);
            stream.Position = 0;
            FileStreamResult result = new FileStreamResult(stream, "application/xls");
            result.FileDownloadName = "Export Item Statistic_" + DateTime.Now + ".xls";
            return result;
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
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 0;
                field.FieldName = "CreateDate";
                field.Caption = "Data";
                field.CellFormat.FormatType = FormatType.Custom;
                field.RunningTotal = true;
                field.SummaryType = PivotSummaryType.Count;
                field.CustomTotals.Add(PivotSummaryType.Count);
            });
            settings.Fields.Add(field =>
            {
                field.AreaIndex = 0;
                field.FieldName = "Remark";
                field.Caption = @Resources.Resource.Department;
                field.Visible = false;
            });

            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.ColumnArea;
                field.AreaIndex = 1;
                field.FieldName = "CreateDate";
                field.Caption = "Month";
                field.GroupInterval = PivotGroupInterval.DateMonth;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.FilterArea;
                field.AreaIndex = 0;
                field.FieldName = "Creator";
                field.Caption = @Resources.Resource.Creator;
            });
            settings.Fields.Add(field =>
            {
                // field.Area = PivotArea.FilterArea;
                field.AreaIndex = 2;
                field.FieldName = "Reason";
                field.Caption = @Resources.Resource.Reason;
            });
            settings.Fields.Add(field =>
            {
                //  field.Area = PivotArea.FilterArea;
                field.AreaIndex = 1;
                field.FieldName = "TypeName";
                field.Caption = @Resources.Resource.Type;
            });

            return settings;
        }

        public GridViewSettings GetGridSettings()
        {
            GridViewSettings settings = new GridViewSettings();

            settings.Name = "StatisticDynamic";
            settings.KeyFieldName = "ItemID;Type";
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.CommandColumn.Visible = false;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.Settings.VerticalScrollBarMode = ScrollBarMode.Auto;
            settings.Settings.VerticalScrollableHeight = 450;
            settings.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
            settings.SettingsBehavior.AllowSelectByRowClick = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowFooter = true;
            settings.SettingsBehavior.AllowFocusedRow = true;
            settings.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowFilterRowMenuLikeItem = true;
            settings.Settings.ShowGroupPanel = true;
            settings.SettingsDetail.ShowDetailRow = true;
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "Quantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "ExportQuantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "LendQuantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "ReceivedQuantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "ReturnedQuantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "AdjustedQuantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "RemainingQuantity");
            settings.Columns.Add(c =>
            {
                c.FieldName = "ItemID";
                c.Caption = "ItemID";
                c.SetColVisible(false);
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ItemDetailName";
                c.Caption = @Resources.Resource.ItemNameDetail;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "Type";
                c.Caption = @Resources.Resource.Type;
                c.SetColVisible(false);
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TypeName";
                c.Caption = @Resources.Resource.Type;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "Unit";
                c.Caption = @Resources.Resource.Unit;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "Quantity";
                c.Caption = @Resources.Resource.Quantity;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ExportQuantity";
                c.Caption = @Resources.Resource.TotalExportQuantity;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "LendQuantity";
                c.Caption = @Resources.Resource.TotalLendQuantity;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ReceivedQuantity";
                c.Caption = @Resources.Resource.ReceivedQuantity;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ReturnedQuantity";
                c.Caption = @Resources.Resource.ReturnedQuantity;
                c.Visible = true;
            });

            settings.Columns.Add(c =>
            {
                c.FieldName = "AdjustedQuantity";
                c.Caption = @Resources.Resource.AdjustedQuantity;
                c.Visible = true;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "RemainingQuantity";
                c.Caption = @Resources.Resource.RemainingQuantity;
                c.Visible = true;

            });
            return settings;
        }

        public PivotGridSettings PieChartSettingForPivot()
        {
            PivotGridSettings settings = new PivotGridSettings();
            settings.Name = "Export item pivot -" + DateTime.Today;
            settings.CallbackRouteValues = new { Controller = "SA", Action = "CallbackPivotGrid", Type = "Pivot", DateFrom = ViewBag.DateFrom, DateTo = ViewBag.DateTo };
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.OptionsView.ShowHorizontalScrollBar = true;
            settings.OptionsChartDataSource.DataProvideMode = PivotChartDataProvideMode.UseCustomSettings;
            settings.OptionsView.ShowFilterHeaders = false;
            settings.OptionsChartDataSource.ProvideDataByColumns = false;
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.DataArea;
                field.AreaIndex = 0;
                field.FieldName = "CreateDate";
                field.Caption = "Data";
                field.CellFormat.FormatType = FormatType.Custom;
                field.RunningTotal = true;
                field.SummaryType = PivotSummaryType.Count;
                field.CustomTotals.Add(PivotSummaryType.Count);
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.RowArea;
                field.AreaIndex = 0;
                field.FieldName = "Remark";
                field.Caption = @Resources.Resource.Department;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.ColumnArea;
                field.AreaIndex = 0;
                field.FieldName = "CreateDate";
                field.Caption = @Resources.Resource.Year;
                field.GroupInterval = PivotGroupInterval.DateYear;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.ColumnArea;
                field.AreaIndex = 1;
                field.FieldName = "CreateDate";
                field.Caption = @Resources.Resource.Month;
                field.GroupInterval = PivotGroupInterval.DateMonth;
            });
            settings.Fields.Add(field =>
            {
                field.Area = PivotArea.FilterArea;
                field.AreaIndex = 0;
                field.FieldName = "Creator";
                field.Caption = @Resources.Resource.Creator;
            });
            settings.Fields.Add(field =>
            {
                // field.Area = PivotArea.FilterArea;
                field.AreaIndex = 2;
                field.FieldName = "Reason";
                field.Caption = @Resources.Resource.Reason;
            });
            settings.Fields.Add(field =>
            {
                //  field.Area = PivotArea.FilterArea;
                field.AreaIndex = 1;
                field.FieldName = "TypeName";
                field.Caption = @Resources.Resource.Type;
            });

            settings.PreRender = (sender, e) =>
            {
                int selectNumber = 100;
                var field = ((MVCxPivotGrid)sender).Fields["Remark"];
                object[] values = field.GetUniqueValues();
                List<object> includedValues = new List<object>(values.Length / selectNumber);
                for (int i = 0; i < values.Length; i++)
                {
                    if (i % selectNumber == 0)
                        includedValues.Add(values[i]);
                }
                field.FilterValues.ValuesIncluded = includedValues.ToArray();
            };
            settings.ClientSideEvents.BeginCallback = "OnBeforePivotGridCallback";
            settings.ClientSideEvents.EndCallback = "UpdateChart";
            return settings;
        }

        public ActionResult GetDocumentRequested()
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
        public ActionResult DocumentRequestedPartial(string OrderCode, DateTime BeginDate, DateTime EndDate)
        {
            ViewBag.BeginDate = BeginDate; 
            ViewBag.EndDate = EndDate;
            ViewBag.OrderCode = (OrderCode == null) ? "" : OrderCode;

            var frm = new FEA_SABusinessLogic.ExportItemManager().GetSARequestList(ViewBag.OrderCode, ViewBag.BeginDate, ViewBag.EndDate);
            return GetGridView(frm, Models.Helper.PartialParameter.SARequestList);
        }
        #endregion
    }
}
