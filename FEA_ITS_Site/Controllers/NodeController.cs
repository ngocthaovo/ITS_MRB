using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
using DevExpress.Web.Mvc;
namespace FEA_ITS_Site.Controllers
{
    public class NodeController : BaseController
    {
        //
        // GET: /Node/
        FEA_BusinessLogic.NodeManager node = new NodeManager();
        public ActionResult Index(string sDocTypeID)
        {
            List<WFNode> lst = node.GetItems(sDocTypeID);
            ViewBag.DocType = new FEA_BusinessLogic.DocumentTypeManager().GetItem(sDocTypeID);
            return View(lst);
        }

        [ValidateInput(false)]
        public ActionResult EditModesPartial(string DoctypeID)
        {
            return GetGridView(node.GetItems(DoctypeID),Models.Helper.PartialParameter.NODE_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFNode obj, string DoctypeID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(obj.Sequence != null && obj.Sequence >= 0 )
                    {
                        obj.DocumentTypeID = DoctypeID;
                        string newID = new FEA_BusinessLogic.NodeManager().InsertItem(obj);
                    }
                    else
                    {
                        ViewData["EditError"] = "Invalid Index value";
                    }
                    
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return GetGridView(node.GetItems(DoctypeID),Models.Helper.PartialParameter.NODE_LIST_GRID);;
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFNode obj, string DoctypeID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.Sequence != null && obj.Sequence >= 0)
                    {
                        new FEA_BusinessLogic.NodeManager().UpdateItem(obj, o => o.Temp1, o => o.Sequence);
                    }
                    else
                    {
                        ViewData["EditError"] = "Invalid Index value";
                    }

                    
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;

            return GetGridView(node.GetItems(DoctypeID), Models.Helper.PartialParameter.NODE_LIST_GRID);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string NodeID, string DoctypeID)
        {
            if (NodeID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.NodeManager().DeleteItem(NodeID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(node.GetItems(DoctypeID), Models.Helper.PartialParameter.NODE_LIST_GRID);
        }
    }
}
