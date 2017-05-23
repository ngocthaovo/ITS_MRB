using FEA_ITS_Site.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using System.IO;

namespace FEA_ITS_Site.Controllers
{
    public class BaseController : Controller
    {
        public bool IsPostback
        {
            get { return Request.HttpMethod == "POST"; }
        }

        protected override void ExecuteCore()
        {
            int culture =1;
            if (this.Session == null || this.Session["CurrentCulture"] == null)
            {
                int.TryParse(System.Configuration.ConfigurationManager.AppSettings["Culture"], out culture);
                this.Session["CurrentCulture"] = culture;
            }
            else
            {
                culture = (int)this.Session["CurrentCulture"];
            }
            //
            SessionManager.CurrentCulture = culture;

            //try { base.ExecuteCore(); }
            //catch { }
            base.ExecuteCore(); 
        }

        protected override bool DisableAsyncSupport
        {
            get { return true; }
        }

        public ActionResult GetGridView(object frm,string GridName)
        {
            ViewBag.GridName = GridName;
            return PartialView("_LoadGridControl", frm);
        }
        //Added by Tony (2017-03-20)
        public ActionResult GetGridViewV2(object frm,string GridName,string DocType)
        {
            ViewBag.GridName = GridName;
            ViewBag.DocType = DocType;
            return PartialView("_LoadGridControlV2",frm);
        }

        public ActionResult GetGridViewForWF(object frm, string GridName,string OrderCode)
        {
            ViewBag.OrderCode = OrderCode;
            ViewBag.GridName = GridName;
            return PartialView("_LoadGridControl", frm);
        }

        public void RequitePermission(string sUrl)
        {
            if (!Helper.UserLoginInfo.IsLogin)
                 RedirectToAction("Login", "User", new { rUrl = sUrl });
        }

        /// Create current session for StockOut Order and StockInOrder
        /// Reason: the list of item will be replace when user choose Stockinform or stockout form o
        /// 

        public ActionResult LoadUploadControl(string ItemUploadGuid,int isEnable)
        {
            ViewBag.IsEnable = isEnable == (int)FEA_BusinessLogic.DeviceRegistrationManager.OrderStatus.SENDING
                                        || isEnable == (int)FEA_BusinessLogic.DeviceRegistrationManager.OrderStatus.FINSHED
                                        || isEnable == (int)FEA_BusinessLogic.DeviceRegistrationManager.OrderStatus.DELETED
                                        || isEnable == (int)FEA_BusinessLogic.DeviceRegistrationManager.OrderStatus.CHECKED ? false : true;
            return PartialView("UploadControlPartial/_LoadUploadControl");
        }

        public ActionResult UploadControlCallbackAction(string key)
        {
            UploadControlExtension.GetUploadedFiles("uc", FEA_ITS_Site.Helper.UploadControl.UploadControlValidationSettings, FEA_ITS_Site.Helper.UploadControl.uc_FileUploadComplete);
            return null;
        }

        [HttpPost]
        public ActionResult DeleteUploadItem(string GUID, string FileName,string Index)
        {
            FEA_ITS_Site.Models.ItemUpload ItemUpload = Session["ItemUpload"] as FEA_ITS_Site.Models.ItemUpload;
            foreach (string item in ItemUpload.ListAddress)
            {
                if (item.Contains(GUID) && item.Contains(FileName))
                {
                    System.IO.File.Delete(item);
                }
            } return null;
        }

    }
}