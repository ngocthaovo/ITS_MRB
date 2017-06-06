using FEA_ITS_Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
using FEA_ITS_Site;
using FEA_ITS_Site.Models.Helper;
using System.Transactions;
using System.IO;
namespace FEA_ITS_Site.Controllers
{
    public class DeviceRegistrationController : BaseController
    {

        public string SessionTmpName = "ItemInfoListDR";
        public ActionResult MainPage()
        {
            return View();
        }

        #region "Load Page"
        // GET: /DeviceRegistration/
                
        /// <summary>
        /// Create Or Update DeviceRegistration WorkFlow
        /// DeviceRegID == null: Create else: update
        /// </summary>
        /// <param name="DeviceRegID"></param>
        /// <returns></returns>
        /// 

        public ActionResult Index(string DeviceRegID, string NodeID, string TypeUser, string MainDetailID, string MainID, int? CheckUserID, string DelegateID, int? DelegateUserID, int? editStatus)
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/DeviceRegistration/Index") });
            //

            DeviceRegistration deRegItem;
            User uInfo = new User();
            Boolean bIsCreateNew = false;

            FEA_ITS_Site.Models.ItemUpload ItemUpload = new ItemUpload();

            if (DeviceRegID == null || DeviceRegID.Trim().Length == 0) // Create New
            {
                // Get User Logged info
                uInfo = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
                // get DeviceReg Info

                deRegItem = new DeviceRegistration()
                {
                    CreateDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    CreatorID = Helper.UserLoginInfo.UserId,
                    Status = (int)DeviceRegistrationManager.OrderStatus.New,
                    IsUrgent = 0,
                    OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.DEVICE_REGISTRATION),
                    ID = Guid.NewGuid().ToString()
                };
                ItemUpload.Guid = deRegItem.ID;    // Demo - Please place with Guid Auto Generate  
                // Item Detail
                Session[SessionTmpName] = new List<ItemInfo>();
                bIsCreateNew = true;
            }
            else // Load Info and show to View Page
            {
                deRegItem = new DeviceRegistrationManager().GetItem(DeviceRegID);
                ItemUpload.Guid = deRegItem.ID; // Get GuID 
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
                    Session[SessionTmpName] = ConvertToItemInfos(deRegItem.DeviceRegistrationDetails);
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
            ViewBag.ItemUploadGuid = ItemUpload.Guid;
            Session["ItemUpload"] = ItemUpload;
            ViewBag.CheckUserID = CheckUserID;
            ViewBag.DelegateID = DelegateID;
            ViewBag.DelegateUserID = DelegateUserID;
            return View(deRegItem);
        }

        /// <summary>
        /// Convert List of DeviceRegistrationDetail to List of ItemInfo
        /// </summary>
        /// <param name="devRgDetails"></param>
        /// <returns></returns>
        private List<ItemInfo> ConvertToItemInfos(ICollection<DeviceRegistrationDetail> devRgDetails)
        {
            List<ItemInfo> lstResult = new List<ItemInfo>();
            ItemInfo _itemInfo;
            foreach (DeviceRegistrationDetail dvResDetailItem in devRgDetails)
            {
                _itemInfo = new ItemInfo()
                {
                    ID = dvResDetailItem.ID,
                    ItemID = dvResDetailItem.ItemID,
                    ItemName = dvResDetailItem.Item.ItemName,
                    ItemType = dvResDetailItem.DeviceRegistration.DocumentType,
                    ItemDetailID = dvResDetailItem.ItemDetailID,
                    ItemDetailName = dvResDetailItem.ItemDetail.ItemDetailName,
                    Des = dvResDetailItem.Description,
                    ReasonItemDetailID = dvResDetailItem.ReasonItemDetailID == null ? null : dvResDetailItem.ReasonItemDetailID,
                    ReasonItemDetailName = dvResDetailItem.ItemDetail2 == null ? null: dvResDetailItem.ItemDetail2.ItemDetailName

                };

                lstResult.Add(_itemInfo);
            }
            return lstResult;
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
        public ActionResult SaveData([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] DeviceRegistration o, bool CreateNew)
        {
            string sResult = "";

            if (Request.Form["btnSaveDraff"] != null) //Save Draft
                sResult = CreateDeviceRegistration(o, true);
            else if (Request.Form["btnSaveAndSend"] != null) // Save And Send
                if (CreateNew)
                    sResult = CreateDeviceRegistration(o, false);//isSaveDraff = false - Create Order;
                else
                    sResult = UpdateDeviceRegistration(o, false);//isSaveDraff = false - Update Order;
            else if (Request.Form["btnUpdate"] != null) //Update - 
            {
                sResult = UpdateDeviceRegistration(o, true);
            }
            if (sResult.Length > 0)
                return RedirectToAction("Index", new { DeviceRegID = sResult, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.success });// Index(sResult,"","", (int)Models.Helper.EditItemStatus.success);
            else
                return RedirectToAction("Index", new { DeviceRegID = sResult, NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.failed });//Index(sResult, "", "", (int)Models.Helper.EditItemStatus.failed);
        }


        /// <summary>
        /// Create Device Registration Information
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <param name="wfMain"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string CreateDeviceRegistration(DeviceRegistration o, bool isSaveDraff)
        {
            //Master
            string _sApprovedUser = "";
            WFMain wfMain = new WFMain();
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.DeviceRegistrationDetails = DeviceRegistrationDetailList();
            o.OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.DEVICE_REGISTRATION);
            o.Temp1 = o.Temp2 = "";
            o.AttachmentLink = FEA_ITS_Site.Helper.Ultilities.UploadFolder + o.ID; // -- Added Attachment link
            var sadsa = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("DocumentType");
            if (isSaveDraff)
                o.Status = (int)DeviceRegistrationManager.OrderStatus.DRAFT;
            else
            {
                o.Status = (int)DeviceRegistrationManager.OrderStatus.SENDING;
                _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");

                wfMain = GetWFmain(o.OrderCode, _sApprovedUser);
            }

            string sID = new DeviceRegistrationManager().InsertItem(o, (DeviceRegistrationManager.OrderStatus)o.Status, wfMain);

            ///Sent mail to next approver
            if (!isSaveDraff && _sApprovedUser.Length > 0)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Reason, "");
            //End sent mail

            return sID;
        }

        /// <summary>
        /// this function using for Update (update, save and send) when document Returned
        /// </summary>
        /// <param name="o"></param>
        /// <param name="isSaveDraff"></param>
        /// <returns>the ID of DeviceRegistration when update success</returns>
        private string UpdateDeviceRegistration(DeviceRegistration o, bool isSaveDraff)
        {
            string _sApprovedUser = "";
            WFMainDetail wfMainDetail = new WFMainDetail();
            bool isReturned = (o.Status == (int)DeviceRegistrationManager.OrderStatus.RETURNED) ? true : false;

            //Master
            o.CreatorID = Helper.UserLoginInfo.UserId;
            o.DeviceRegistrationDetails = DeviceRegistrationDetailList();
            o.Temp1 = o.Temp2 = "";
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
            bool _result = new DeviceRegistrationManager().UpdateItem(o, isSaveDraff, isReturned, wfMainDetail,
                                                                         i => i.Description, i => i.Reason, i => i.CreatorID, i => i.CreateDate,
                                                                         i => i.LastUpdateDate,i=>i.IsUrgent,i=>i.DocumentType
                                                                         );

            ///Sent mail to next approver
            if (!isSaveDraff && _result)
                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), o.CreatorID.Value, o.Reason, "");
            //End sent mail

            return _result == true ? o.ID : "";
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
        /// Get DeviceRegistrationDetail to save to DB
        /// </summary>
        /// <returns></returns>
        private List<DeviceRegistrationDetail> DeviceRegistrationDetailList()
        {
            List<DeviceRegistrationDetail> lst = new List<DeviceRegistrationDetail>();
            if (Session[SessionTmpName] != null)
            {
                List<ItemInfo> lstItemInfo = (List<ItemInfo>)Session[SessionTmpName];
                DeviceRegistrationDetail devicedetail;
                foreach (ItemInfo i in lstItemInfo)
                {
                    devicedetail = new DeviceRegistrationDetail()
                    {
                        Description = i.Des,
                        ItemDetailID = i.ItemDetailID,
                        ItemID = i.ItemID,
                        Temp1 = "",
                        ReasonItemDetailID = i.ReasonItemDetailID
                    };
                    lst.Add(devicedetail);
                }
            }
            return lst;
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
        public ActionResult AddItemToOrder(string ItemID, string ItemDetail, string Description,string Reason)
        {

            if (ItemDetail == "fc07bf47-ebd0-4a90-869c-b01c8175abc7" /*ItemID: DRReason*/ && Description.Length != 11)
            {
                string _sErr = Resources.Resource.msgErrorOpenSalesOrder;
                ViewData["EditError"] = _sErr;
            }
            else
            {
                if (ItemID == "" || ItemID.Trim().Length == 0 || ItemDetail == "" || ItemDetail.Trim().Length == 0 || ((Reason == "" || Reason.Trim().Length == 0) && ItemDetail == "fc07bf47-ebd0-4a90-869c-b01c8175abc7" /*ItemID: DRReason*/ ))
                {
                    string _sErr = Resources.Resource.msgInputError;
                    if (ItemID.Trim().Length == 0)
                        _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.ItemType);
                    if (ItemDetail.Trim().Length == 0)
                        _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Using);
                    if (Reason.Trim().Length == 0 && ItemDetail == "fc07bf47-ebd0-4a90-869c-b01c8175abc7" /*ItemID: DRReason*/ )
                        _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.Reason);

                    ViewData["EditError"] = _sErr;
                }
                else if ((new DeviceRegistrationManager().CheckFEPO(Description)) == false && ItemDetail == "fc07bf47-ebd0-4a90-869c-b01c8175abc7" /*ItemID: DRReason*/)
                {
                    string _sErr = Resources.Resource.CheckFEPO;
                    ViewData["EditError"] = _sErr;
                }
                else
                {
                    Item i = new ItemManager().GetItem(ItemID);

                    var flag = FindItemInList(ItemID, ItemDetail);
                    if (flag) // Đã tồn tại trong list
                        ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                    else if (CheckOrderType(i.Temp1))
                    {
                        ViewData["EditError"] = Resources.Resource.msgDeviceResOnlyOneDoc;
                    }
                    else
                    {
                        try
                        {
                            ItemDetail item_detail = new ItemDetailManager().GetItem(ItemDetail);
                            Models.ItemInfo item = new ItemInfo()
                            {
                                ID = Guid.NewGuid().ToString(),
                                ItemID = i.ID,
                                ItemName = i.ItemName,
                                ItemDetailID = item_detail.ID,
                                ItemDetailName = item_detail.ItemDetailName,
                                Des = Description,
                                ItemType = i.Temp1,
                                ReasonItemDetailID = Reason == "" ? null: new ItemDetailManager().GetItem(Reason).ID,
                                ReasonItemDetailName = Reason == "" ? null : new ItemDetailManager().GetItem(Reason).ItemDetailName
                            };

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
            ViewData["ShowCommand"] = true;
            return GetGridView(Session[SessionTmpName], Models.Helper.PartialParameter.ITEM_DETAIL_LIST_GRID);
        }


        /// <summary>
        /// check item exist on list
        /// </summary>
        /// <param name="ItemID"></param>
        /// <param name="ItemDetailID"></param>
        /// <returns></returns>
        private bool FindItemInList(string ItemID, string ItemDetailID)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
                lst = (List<ItemInfo>)Session[SessionTmpName];

            return lst.Where(i => i.ItemID == ItemID && i.ItemDetailID == ItemDetailID && i.ItemDetailID != "fc07bf47-ebd0-4a90-869c-b01c8175abc7" /*Open Sales Order*/).Count() > 0 ? true : false;
        }

        private bool CheckOrderType(string OrderType)
        {
            List<ItemInfo> lst = new List<ItemInfo>();
            if (Session[SessionTmpName] != null)
                lst = (List<ItemInfo>)Session[SessionTmpName];

            return lst.Where(i => i.ItemType!= OrderType).Count() > 0 ? true : false;
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="ShowCommand"></param>
        /// <returns></returns>
        public ActionResult GetListItemDetailPartial(bool? ShowCommand = false)
        {
            ViewData["ShowCommand"] = ShowCommand;
            return GetGridView((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.ITEM_DETAIL_LIST_GRID);
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
            ViewData["ShowCommand"] = true;
            return GetGridView((List<ItemInfo>)Session[SessionTmpName], Models.Helper.PartialParameter.ITEM_DETAIL_LIST_GRID);
        }

        #endregion

        #region Get Approver Partial
        public ActionResult GetApproverPartial(string DocTypeName, int CodeCenterCode, string NodeID, string TypeUser,string OrderCode)
        {
            List<sp_GetApprover_Result> lstApprover = CheckDuplicateApprover(DocTypeName, CodeCenterCode, NodeID, OrderCode);
            if (lstApprover.Count > 0)
            {
                // Create Session to get 
                sp_GetApprover_Result i = lstApprover[0];
                Session["NodeID"] = i.NodeID;          // Next Node ID
                Session["DocumentTypeID"] = i.DocumentTypeID;
            }
            ViewBag.TypeUser = TypeUser;
            return PartialView("DeviceRegistration/_GetApprover", lstApprover);
        }

        public List<sp_GetApprover_Result> CheckDuplicateApprover(string DocTypeName, int CodeCenterCode, string NodeID,string OrderCode)
        {
            List<sp_GetApprover_Result> lstApprover = new DeviceRegistrationManager().GetApprover(DocTypeName, CodeCenterCode, NodeID, OrderCode, Helper.UserLoginInfo.UserId);
            if (NodeID !="")
            {
                if (lstApprover.Where(x => x.ApproverID == Helper.UserLoginInfo.UserId).ToList().Count > 0)
                {
                    lstApprover = CheckDuplicateApprover(DocTypeName, CodeCenterCode, lstApprover[0].NodeID, OrderCode);
                }
            }
            return lstApprover;
        }
        #endregion

        public ActionResult SignDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] DeviceRegistration o)
        {
            int Status = 0;
            string _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");  // lỗi chổ này, nếu level 1 có 1 người ký duyệt thay vì truyền vào ApprovalID thì nó lại truyền vào CheckUserName
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");             //Đã kiểm tra store với 2 người ký duyệt cho level 1 thì nó trả về đúng ApprovalID
            DeviceRegistration degreg = new DeviceRegistrationManager().GetItem(o.ID);                     
            if (new FEA_BusinessLogic.WaitingArea.WaitingArea().CheckAlreadySigned(MainDetailID) == 0)
            {
                Status = WaitingAreaController.SignRejectDocument("Sign", _sApprovedUser, comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION, degreg.User1.CostCenterCode, NodeID, o.OrderCode, degreg.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, degreg.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);
            }
            #region Removed by Steven 2014-09-03
            //FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);     // Current approver
            //List<sp_GetApprover_Result> ApproverList = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetApprover(FEA_ITS_Site.Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION, degreg.User1.CostCenterCode, NodeID);
            //if (ApproverList.Count == 0)
            //{
            //    _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateEndWorkFlow(o.OrderCode, FEA_ITS_Site.Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION, MainDetailID, comment);
            //    if (_result)
            //        Controllers.HelperController.SendMailToITServices(degreg.CreatorID.Value, string.Format("{0}-", degreg.OrderCode) + degreg.Reason, comment);
            //    // End workflow
            //}
            //else
            //{
            //    _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateNextWorkFlow(_sApprovedUser, MainDetailID, Session["NodeID"].ToString(), MainID, comment, Helper.UserLoginInfo.UserId);

            //    // Send Email
            //    if (_result)
            //        Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), Helper.UserLoginInfo.UserId, degreg.Reason, comment);

            //    // Find next Approver
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
            return RedirectToAction("Index", new { DeviceRegID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });

        }

        public ActionResult RejectDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] DeviceRegistration o)
        {
            int Status = 0;
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
            DeviceRegistration degreg = new DeviceRegistrationManager().GetItem(o.ID);
            Status = WaitingAreaController.SignRejectDocument("Reject", "", comment, FEA_ITS_Site.Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION, degreg.User1.CostCenterCode, NodeID, o.OrderCode, degreg.Reason, MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, degreg.CreatorID.Value, CheckUserID, DelegateID, DelegateUserID);

            #region Removed by Steven 2014-09-03
            //_result = new FEA_BusinessLogic.WaitingArea.WaitingArea().RejectDocument(device.OrderCode, MainDetailID, FEA_ITS_Site.Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION, comment, UI.UserID, MainID, Convert.ToInt32(device.CreatorID));
            //switch (_result)
            //{
            //    case true:
            //        Status = (int)Models.Helper.EditItemStatus.success;
            //        Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(device.CreatorID), Helper.UserLoginInfo.UserId, device.Reason, comment);   // Aded by Steven 2014-08-21
            //        break;
            //    case false:
            //        Status = (int)Models.Helper.EditItemStatus.failed;
            //        break;
            //    default:
            //        Status = (int)Models.Helper.EditItemStatus.nomal;
            //        break;
            //}
            #endregion
            return RedirectToAction("Index", new { DeviceRegID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });
        }

        #region "List of Device Registration Management - User"

        public ActionResult DeviceRegistrationManagement()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {
            List<DeviceRegistration> lstItem = new DeviceRegistrationManager().GetItems(Helper.UserLoginInfo.UserId);
            return GetGridView(lstItem, Models.Helper.PartialParameter.DEVICE_REGISTRATION_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string ID)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.DeviceRegistrationManager().DeleteItem(ID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteDeviceRegFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(new DeviceRegistrationManager().GetItems(Helper.UserLoginInfo.UserId), Models.Helper.PartialParameter.DEVICE_REGISTRATION_LIST_GRID);
        }
        #endregion

        public ActionResult FinishedManagementForAdmin()
        {
            object x = new FEA_BusinessLogic.DeviceRegistrationManager().GetFinishedDocument(FEA_ITS_Site.Helper.UserLoginInfo.UserId);
            return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.FinishedForManager);
        }

        #region "Function for Admin"

        public ActionResult ApproveManager()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/DeviceRegistration/ApproveManager") });
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

                            // List of Device Registration - IDs
                            List<string> lstTosign = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION).Select(i => i.Key).ToList<string>();
                            if (lstTosign != null)
                                iResult = new FEA_BusinessLogic.DeviceRegistrationManager().AdminApproveOrder(lstTosign, Helper.UserLoginInfo.UserId);                                         

                            // List of Hardware Requitement - IDs
                            lstTosign = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.HARD_REGISTRATION).Select(i => i.Key).ToList<string>();
                            if (lstTosign != null && lstTosign.Count > 0)
                            {
                                int status = new FEA_BusinessLogic.HardwareRequirementManager().AdminApproveOrder(lstTosign, Helper.UserLoginInfo.UserId);
                                if (status <= 0)
                                {
                                    if (status == -1)
                                        ViewBag.EditInfo = "Can not update the data";
                                    else if (status == -2)
                                        ViewBag.EditInfo = "Can not calculate the quantity in the Stock";
                                    else if (status == -3)
                                        ViewBag.EditInfo = "Can not send data to Eland system";

                                    ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                                    return View();
                                }
                                else
                                    iResult += status;
                            }


                            if (iResult > 0)
                            {
                                SendMailToUser(lstItem, false, DateTime.Now);
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
            else if (Request.Form["btnReject"] != null) // IT Dept Reject Document(s)
            {
                var lstItem = Request.Form["mydata"];
                var it_commnet = "IT Department: --> " + Request.Form["it_comment"];
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
            //else if (Request.Form["btnProcess"] != null)
            //{
            //    var lstItem = Request.Form["mydata"];
            //    try
            //    {
            //        if (lstItem.Trim().Length != 0)
            //        {
            //            Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
            //            if (values != null)
            //            {
            //                int iResult = 0;

            //                // List of Device Registration - IDs
            //                List<string> lstToprocess = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION).Select(i => i.Key).ToList<string>();
            //                if (lstToprocess != null)
            //                    iResult = new FEA_BusinessLogic.DeviceRegistrationManager().AdminProcessOrder(lstToprocess, string.Format("{0}({1})", Helper.UserLoginInfo.UserName,Helper.UserLoginInfo.UserCode));


            //                // List of Hardware Requitement - IDs
            //                lstToprocess = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.HARD_REGISTRATION).Select(i => i.Key).ToList<string>();
            //                if (lstToprocess != null && lstToprocess.Count > 0)
            //                {
            //                    int status = new FEA_BusinessLogic.HardwareRequirementManager().AdminProcessOrder(lstToprocess, string.Format("{0}({1})", Helper.UserLoginInfo.UserName, Helper.UserLoginInfo.UserCode));
            //                    if (status <= 0)
            //                    {
            //                        if (status == -1)
            //                            ViewBag.EditInfo = "Cannot set processor. Please try again";

            //                        ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
            //                        return View();
            //                    }
            //                    else
            //                        iResult += status;
            //                }


            //                if (iResult > 0)
            //                {
            //                    ViewBag.EditStatus = Models.Helper.EditItemStatus.success;
            //                    ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
            //                }
            //                else
            //                {
            //                    ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
            //                    ViewBag.EditInfo = Resources.Resource.msgUpdateFailed;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
            //            ViewBag.EditInfo = Resources.Resource.msgSelectItem;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
            //        ViewBag.EditInfo = ex.Message;
            //    }
            //}

            return View();
        }


        //private Dictionary<string, string> ConvertObject(string lstItem)
        //{
        //}


        //[HttpPost]
        //public ActionResult ApproveManager()
        //{
        //    return View();
        //}
        [ValidateInput(false)]
        public ActionResult ApprovePartial()
        {

            var frm = new FEA_BusinessLogic.DeviceRegistrationManager().GetItems("", DeviceRegistrationManager.OrderStatus.CHECKED);
            return GetGridView(frm, Models.Helper.PartialParameter.APPROVE_ORDER_GRID);
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
            ViewBag.BeginDate = BeginDate; // used on _getGridView partical
            ViewBag.EndDate = EndDate;// used on _getGridView partical
            ViewBag.OrderCode = (OrderCode == null) ? "" : OrderCode;// used on _getGridView partical

            var frm = new FEA_BusinessLogic.DeviceRegistrationManager().GetOrderRequested(ViewBag.OrderCode, ViewBag.BeginDate, ViewBag.EndDate);
            return GetGridView(frm, Models.Helper.PartialParameter.REQUEST_ORDER_GRID);
        }

        #endregion

        public ActionResult CallbackDetail(string ID, string DocumentTypeName)
        {
            ViewData["ID"] = ID;
            string GridName = string.Empty;
            object x = new FEA_BusinessLogic.Statistic.Report().DetailGetFinishedDocument(ID, DocumentTypeName);
            switch (DocumentTypeName)
            {
                case "DEVICEREGISTRATION":
                    GridName = Models.Helper.PartialParameter.DetailFinishedForManager;
                    break;
                case "HARDWAREREQUIREMENT":
                    GridName = Models.Helper.PartialParameter.DetailHRFinishedForManager;
                    break;
                default:
                    break;
            }
            return GetGridView(x, GridName);
        }

        public ActionResult CallbackFinishedDocumentGrid()
        {
            object x = new FEA_BusinessLogic.DeviceRegistrationManager().GetFinishedDocument(FEA_ITS_Site.Helper.UserLoginInfo.UserId);
            return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.FinishedForManager);
        }

        [HttpGet]
        public ActionResult AdminDoProcessRequestOrder(string data)
        {
            ViewBag.Data = data;
            return View();
        }

        [HttpPost]
        public ActionResult AdminDoProcessRequestOrder([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] DateTime dtCompleteEstimateDate)
        {
            //var lstItem = Request.Form["mydata"];
            var lstItem = Request.Form["mydata"];
            ViewBag.Data = lstItem;
            try
            {
                if (lstItem.Trim().Length != 0)
                {
                    Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
                    if (values != null)
                    {
                        int iResult = 0;

                        // List of Device Registration - IDs
                        List<string> lstToprocess = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION).Select(i => i.Key).ToList<string>();
                        if (lstToprocess != null)
                            iResult = new FEA_BusinessLogic.DeviceRegistrationManager().AdminProcessOrder(lstToprocess, string.Format("{0}({1})", Helper.UserLoginInfo.UserName, Helper.UserLoginInfo.UserCode), dtCompleteEstimateDate);


                        // List of Hardware Requitement - IDs
                        lstToprocess = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.HARD_REGISTRATION).Select(i => i.Key).ToList<string>();
                        if (lstToprocess != null && lstToprocess.Count > 0)
                        {
                            int status = new FEA_BusinessLogic.HardwareRequirementManager().AdminProcessOrder(lstToprocess, string.Format("{0}({1})", Helper.UserLoginInfo.UserName, Helper.UserLoginInfo.UserCode), dtCompleteEstimateDate);
                            if (status <= 0)
                            {
                                if (status == -1)
                                    ViewBag.EditInfo = "Cannot set processor. Please try again";

                                ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                                return View();
                            }
                            else
                                iResult += status;
                        }

                        if (iResult > 0)
                        {
                            SendMailToUser(lstItem, true, dtCompleteEstimateDate);
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

            return View();
        }

        public void SendMailToUser(String lstItem,  bool isProcess, DateTime dtCompleteDate)
        {
            Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
            List<string> lstToProcess;
            User _UserProcess = new FEA_BusinessLogic.UserManager().GetItem(Helper.UserLoginInfo.UserId);
            lstToProcess   = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.DEVICE_REGISTRATION).Select(i => i.Key).ToList<string>();
            if(lstToProcess!=null)
            {
                List<DeviceRegistration> drItem = new DeviceRegistrationManager().GetListItemToProcess(lstToProcess);
                foreach (DeviceRegistration item in drItem)
                {
                    string _content = string.Format( (isProcess ?  Resources.Resource.bodyProcessMail: Resources.Resource.bodyCompleteMail),item.OrderCode,item.User1.UserName,_UserProcess.UserName,_UserProcess.UserNameEN,_UserProcess.CostCenter.Remark,dtCompleteDate,_UserProcess.UserPhone,item.Reason);
                    FEA_Ultil.FEASendMail.SendMailMessage(item.User1.UserEmail, "", "", "[ITS] "+item.OrderCode+" Service registration", _content);
                }
            }
            lstToProcess = values.Where(i => i.Value == Models.Helper.TagPrefixParameter.HARD_REGISTRATION).Select(i => i.Key).ToList<string>();
            if(lstToProcess !=null)
            {
                List<HardwareRequirement> hrItem = new HardwareRequirementManager().GetListItemToProcess(lstToProcess);
                foreach(HardwareRequirement item in hrItem)
                {
                    string _content = string.Format((isProcess ? Resources.Resource.bodyProcessMail : Resources.Resource.bodyCompleteMail), item.OrderCode, item.User1.UserName, _UserProcess.UserName, _UserProcess.UserNameEN, _UserProcess.CostCenter.Remark, dtCompleteDate,_UserProcess.UserPhone,item.Reason);
                    FEA_Ultil.FEASendMail.SendMailMessage(item.User1.UserEmail, "", "", "[ITS] "+ item.OrderCode+" Hardware requirement", _content);
                }
            }

        }

    }
}
