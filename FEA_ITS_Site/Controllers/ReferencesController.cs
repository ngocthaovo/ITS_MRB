using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
using FEA_ITS_Site;
using FEA_ITS_Site.Models.Helper;
namespace FEA_ITS_Site.Controllers
{
    public class ReferencesController : BaseController
    {
        //
        // GET: /References/
        [HttpGet]
        public ActionResult Index(string ID, string OrderCode, string WFMainDetailID, int? editStatus)
        {            
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/References/Index") });
            //

            WFRefference o =new WFRefference();
            if (OrderCode != null && Helper.UserLoginInfo.UserId > 0 && WFMainDetailID != null)
            {
                o = new FEA_BusinessLogic.WFRefferenceManager().GetItem(OrderCode,WFMainDetailID, Helper.UserLoginInfo.UserId);
                if (o != null)
                    ViewBag.IsCreateNew = false;
                else if (o == null && ID != null)
                {
                    o = new FEA_BusinessLogic.WFRefferenceManager().GetItem(ID);
                    if (o != null)
                        ViewBag.IsCreateNew = false;
                }
                else
                {
                    o = new WFRefference()
                    {
                        MainDetailID = WFMainDetailID,
                        OrderCode = OrderCode,
                        SenderID = Helper.UserLoginInfo.UserId
                    };

                    ViewBag.IsCreateNew = true;
                }
            }

            if (editStatus != null)
                ViewBag.EditStatus = editStatus;

            if (o.SenderID == Helper.UserLoginInfo.UserId)
                ViewBag.IsOwner = true;
            else
                ViewBag.IsOwner = false;

            ViewData["OrderCode"] = OrderCode;
            return View(o);
        }

        [HttpPost]
        public ActionResult Index([ModelBinder(typeof(DevExpress.Web.Mvc.DevExpressEditorsBinder))] WFRefference o, bool IsCreateNew)
        {
            bool flag =false;
            string sMessage = "";
            if (Request["btnSentComment"] != null)
            {
                if (IsCreateNew)
                {
                    // Insert Into DB
                    if (FEA_ITS_Site.Helper.UserLoginInfo.IsLogin)
                    {
                        o.SenderID = Helper.UserLoginInfo.UserId;
                        o.SenderName = Helper.UserLoginInfo.UserName;
                    }

                    flag = new FEA_BusinessLogic.WFRefferenceManager().InsertItem(o);
                }
                else
                {
                    //Update data
                    flag = new FEA_BusinessLogic.WFRefferenceManager().UpdateItem(o, i => i.RefferenceList);
                }

                if (flag)
                {
                    sMessage = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtComment");
                    if (sMessage != null && sMessage.Trim().Length > 0)
                    {
                        WFRefferenceDetail detail = new WFRefferenceDetail()
                        {
                            Comment = sMessage,
                            RefUserID = Helper.UserLoginInfo.UserId,
                            RefUserName = Helper.UserLoginInfo.UserName,
                            WFRefferenceID = o.ID
                        };
                        flag = new FEA_BusinessLogic.WFRefferenceDetailManager().InsertItem(detail);
                    }
                    
                }


                if (flag)
                {

                    sMessage = o.OrderCode + " - " + sMessage;

                    if (o.SenderID == FEA_ITS_Site.Helper.UserLoginInfo.UserId)
                    {                    
                        //Sent Email to Users
                        int[] lstRec = Array.ConvertAll(o.RefferenceList.Split(','), s => int.Parse(s));
                        FEA_ITS_Site.Controllers.HelperController.SendMailToReferences(lstRec, Helper.UserLoginInfo.UserId, sMessage);
                    }
                    else
                    {
                        int[] lstRec = new int[]{o.SenderID.Value};
                        FEA_ITS_Site.Controllers.HelperController.SendMailToReferences(lstRec, Helper.UserLoginInfo.UserId, sMessage);
                    }
                 

                    return RedirectToAction("Index", new { ID = o.ID, OrderCode = o.OrderCode, WFMainDetailID = o.MainDetailID, editStatus = (int)Models.Helper.EditItemStatus.success });
                }   
            }

            return RedirectToAction("Index", new { ID = o.ID, OrderCode = o.OrderCode, WFMainDetailID = o.MainDetailID, editStatus = (int)Models.Helper.EditItemStatus.failed });
        }

        #region "GetReferencesListPartial"
        public ActionResult GetReferencesListPartial()
        {
            return PartialView("_ReferencesUserPartial");
        }
        #endregion

        #region "Comment Region"

        public ActionResult GetListCommentBySesionPartial(string OrderCode)
        {
            ViewData["OrderCode"] = OrderCode;
            List<WFRefference> lst = new FEA_BusinessLogic.WFRefferenceManager().GetItems(OrderCode);
            return GetGridView(lst, "HistoryCommentBySession");
        }

        public ActionResult GetListCommentPartial(string WFRefferenceID)
        {
            ViewData["WFRefferenceID"] = WFRefferenceID;
            List<WFRefferenceDetail> lst = new FEA_BusinessLogic.WFRefferenceDetailManager().GetItems(WFRefferenceID);
            return GetGridView(lst, "ReferencesCommentListGrid");
        }
        #endregion
    }
}
