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
using FEA_BusinessLogic.ERP;

namespace FEA_ITS_Site.Controllers
{
    public class ERPDocumentController : BaseController
    {
       
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ERPDocumentMainPage()
        {
            return View();
        }
        public ActionResult GETERPListItemDetailPartial(string id, string docType)
        {
            List<ERPDocumentDetail> list = new List<ERPDocumentDetail>();
            ViewBag.ID = id;
            ViewBag.DocType = docType;
            list = new Order().GetItemDetail(id);
            return GetGridViewV2(list, Models.Helper.PartialParameter.ERPItemListGrid, docType);
        }
        public ActionResult ERPListItemDetailPartial(string ID, string NodeID, string TypeUser, string MainDetailID, string MainID, int? CheckUserID, string DelegateID, int? DelegateUserID, int? editStatus)
        {
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/ERPDocument/ERPListItemDetailPartial") });
          

            ERPDocument ERPItem;
            User uInfo = new User();
            ERPItem = new Order().GetItem(ID);
            uInfo = new UserManager().GetItem(ERPItem.UserID);
            if (editStatus != null)
                ViewBag.EditStatus = editStatus;
            ViewBag.User = uInfo;
            ViewBag.NodeID = NodeID == null ? "" : NodeID;
            ViewBag.TypeUser = TypeUser == null ? "" : TypeUser;
            ViewBag.MainDetailID = MainDetailID == null ? "" : MainDetailID;
            ViewBag.MainID = MainID == null ? "" : MainID;
            ViewBag.CheckUserID = CheckUserID;
            ViewBag.DelegateID = DelegateID;
            ViewBag.DelegateUserID = DelegateUserID;
            ViewBag.OrderCode = ERPItem.OrderCode;
            ViewBag.ID = ERPItem.ID;
            ViewBag.OrtherType = ERPItem.OrderType;
            ViewBag.DocTitle = GetERPDocumentName(ERPItem.OrderType);
            return View(ERPItem);
        }

        public static string GetERPDocumentName(string Type)
        {
            string _sResult = Resources.Resource.STATUS_UNKNOW;
            if (Type == Order.OrderType.ACCESSORYDEVELOPOUT.ToString())
                _sResult = Resources.Resource.ACCESSORYDEVELOPOUT;
            else if (Type == Order.OrderType.ACCESSORYMOVEOUT.ToString())
                _sResult = Resources.Resource.ACCESSORYMOVEOUT;
            else if (Type == Order.OrderType.ACCESSORYMOVEOUTMULTI.ToString())
                _sResult = Resources.Resource.ACCESSORYMOVEOUTMULTI;
            else if (Type == Order.OrderType.ACCESSORYOUT.ToString())
                _sResult = Resources.Resource.ACCESSORYOUT;
            else if (Type == Order.OrderType.DEVELOPPRODUCT.ToString())
                _sResult = Resources.Resource.DEVELOPPRODUCT;
            else if (Type == Order.OrderType.FABRICDEVELOPOUT.ToString())
                _sResult = Resources.Resource.FABRICDEVELOPOUT;
            else if (Type == Order.OrderType.FABRICMOVEOUT.ToString())
                _sResult = Resources.Resource.FABRICMOVEOUT;
            else if (Type == Order.OrderType.FABRICMOVEOUTMULTI.ToString())
                _sResult = Resources.Resource.FABRICMOVEOUTMULTI;
            else if (Type == Order.OrderType.FABRICOUT.ToString())
                _sResult = Resources.Resource.FABRICOUT;
            return _sResult;
        }

        public ActionResult SignDocument(FormCollection Form, string OrderCode, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] ERPDocument o)
        {
            int Status = 0;
            string _sApprovedUser = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboNextResUser");
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtcomment");
            ERPDocument erpitem = new Order().GetItem(o.ID);
            if (new FEA_BusinessLogic.WaitingArea.WaitingArea().CheckAlreadySigned(MainDetailID) == 0)
            {
                Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Sign", _sApprovedUser, comment, erpitem.OrderType,
                                                                                            erpitem.User.CostCenterCode, NodeID  /*current node*/, o.OrderCode, "", MainID, MainDetailID,
                                                                                            Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, erpitem.UserID,
                                                                                            CheckUserID, DelegateID, DelegateUserID);
            }
            else
            {
                Status = 0;
            }
            return RedirectToAction("ERPListItemDetailPartial", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });
        }
        public ActionResult RejectDocument(FormCollection Form, string NodeID, string MainDetailID, string MainID, int CheckUserID, string DelegateID, int DelegateUserID, [ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] ERPDocument o)
        {
            int Status = 0;
            string comment = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtComment");
            FEA_BusinessLogic.User UI = new UserManager().GetItem(Helper.UserLoginInfo.UserId);
            ERPDocument erpitem = new Order().GetItem(o.ID);
            Status = FEA_ITS_Site.Controllers.WaitingAreaController.SignRejectDocument("Reject", "", comment, erpitem.OrderType, erpitem.User.CostCenterCode, NodeID  /*current node*/, o.OrderCode, "", MainID, MainDetailID, Session["NodeID"] == null ? "" : Session["NodeID"].ToString() /*Next Node*/, erpitem.UserID, CheckUserID, DelegateID, DelegateUserID);
            return RedirectToAction("ERPListItemDetailPartial", new { ID = o.ID, NodeID = NodeID, TypeUser = "manager", MainDetailID = "", MainID = MainID, editStatus = Status, CheckUserID = CheckUserID, DelegateID = DelegateID, DelegateUserID = DelegateUserID });
        }

        [HttpPost]
        public ActionResult SaveData([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] ERPDocument o)
        {
            return RedirectToAction("ERPListItemDetailPartial", new { ID = "", NodeID = "", TypeUser = "", MainDeTailID = "", MainID = "", CheckUserID = "", DelegateID = "", DelegateUserID = "", editStatus = (int)Models.Helper.EditItemStatus.failed });//Index(sResult, "", "", (int)Models.Helper.EditItemStatus.failed);
        }

        public ActionResult ERPDocumentQuery()
        {
            if(Request.Form["btnQuery"]!=null)
            {
                ViewBag.OrderCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtOrderCode") != null ? "%"+DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtOrderCode")+"%" : "%";
                ViewBag.FEPOCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtFEPOCode") != null ? "%"+DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtFEPOCode")+"%" : "%";
                ViewBag.Status = "%";
                switch (DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cbTypeQuery"))
         {
             case "Sending":
                 ViewBag.Status = 2;
                 break;
             case "Finished":
                 ViewBag.Status = 3;
                 break;
             case "Returned":
                 ViewBag.Status = 4;
                 break;
         }
         ViewBag.UserID = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtUserCode") != null ? DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtUserCode") : "%";
            }
            return View();
        }
        public ActionResult ERPDocumentQueryPartial(string UserID, string OrderCode, string FEPOCode, string Status)
        {
            ViewBag.UserID=UserID;
            ViewBag.OrderCode=OrderCode;
            ViewBag.FEPOCode=FEPOCode;
            ViewBag.Status=Status;
            var frm = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetERPHitory(ViewBag.UserID, ViewBag.OrderCode, ViewBag.FEPOCode, ViewBag.Status);
            return GetGridView(frm, FEA_ITS_Site.Models.Helper.PartialParameter.ERPHistory);
        }
    }
}