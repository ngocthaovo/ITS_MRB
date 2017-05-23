using DevExpress.Web;
using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
namespace FEA_ITS_Site.Controllers
{
    public class WorkflowManagementController : BaseController
    {
        private FEA_BusinessLogic.DocumentTypeManager doctype = new FEA_BusinessLogic.DocumentTypeManager();
        public ActionResult DocumentType()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WorkflowManagement/DocumentType") });
            //

            return View(new FEA_BusinessLogic.DocumentTypeManager().GetItems(""));
        }
        [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {
            return GetGridView(doctype.GetItems(""), FEA_ITS_Site.Models.Helper.PartialParameter.DOC_TYPE_PARTIAL);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFDocumentType obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    obj.Status = (int)Models.Helper.Status.enabled;
                    obj.Temp2 = obj.Temp3 = "";

                    string newID = new FEA_BusinessLogic.DocumentTypeManager().InsertItem(obj);
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return GetGridView(doctype.GetItems(""), FEA_ITS_Site.Models.Helper.PartialParameter.DOC_TYPE_PARTIAL);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFDocumentType obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   // NorthwindDataProvider.UpdateProduct(product);
                    new FEA_BusinessLogic.DocumentTypeManager().UpdateItem(obj, o => o.DocumentTypeName, o => o.Description, o=>o.Temp1,o=>o.Parameter);
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;

            return GetGridView(doctype.GetItems(""), FEA_ITS_Site.Models.Helper.PartialParameter.DOC_TYPE_PARTIAL);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string DocumentTypeID)
        {
            if (DocumentTypeID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.DocumentTypeManager().DeleteItem(DocumentTypeID);
                    if(!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(doctype.GetItems(""), FEA_ITS_Site.Models.Helper.PartialParameter.DOC_TYPE_PARTIAL);
        }

        #region NodeList
        public ActionResult NodeListPartial(string DocumentTypeID)
        {
            ViewData["DocumentTypeID"] = DocumentTypeID;
            return GetGridView(new FEA_BusinessLogic.NodeManager().GetItems(DocumentTypeID), Models.Helper.PartialParameter.NODE_DOCTYPE_LIST_PARTIAL);// PartialView("Partials/_NodeListDetail", new FEA_BusinessLogic.NodeManager().GetItems(DocumentTypeID));
        }

        #endregion
    }
}
