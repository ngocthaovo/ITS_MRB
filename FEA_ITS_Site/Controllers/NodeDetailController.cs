using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
using DevExpress.Web.Mvc;
namespace FEA_ITS_Site.Controllers
{
    public class NodeDetailController : BaseController
    {
        //
        // GET: /NodeDetail/
        NodeDetailManager nodedt = new NodeDetailManager();
        public ActionResult Index(string NodeID)
        {
            WFNode node = new FEA_BusinessLogic.NodeManager().GetItem(NodeID);
            if(node != null)
            {
                ViewBag.NodeName = node.Temp1;
                ViewBag.NodeIndex = node.Sequence.ToString();
                ViewBag.DocTypeName = node.WFDocumentType.DocumentTypeName;
                ViewBag.DocTypeParameter = node.WFDocumentType.Parameter;
            }

            

            List<WFNodeDetail> lst = nodedt.GetItems(NodeID);
            return View(lst);
        }
        [ValidateInput(false)]
        public ActionResult EditModesPartial(string NodeID)
        {
            return GetGridView(nodedt.GetItems(NodeID),Models.Helper.PartialParameter.NODE_DETAIL_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFNodeDetail obj, string NodeID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (nodedt.IsDuplicate(NodeID, obj.ApproverID.Value, obj.CostCenterCode.Value))
                        ViewData["EditError"] = Resources.Resource.msgDuplicateUser;
                    else
                    {

                        obj.DelegateID = ""; obj.isDelegate = 0;
                        if (obj.Condition == null) obj.Condition = "";
                        string newID = nodedt.InsertItem(obj);
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return GetGridView(nodedt.GetItems(NodeID), Models.Helper.PartialParameter.NODE_DETAIL_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] WFNodeDetail obj, string NodeID)
        {
            if (ModelState.IsValid)
            {
                try
                {

                   new FEA_BusinessLogic.NodeDetailManager().UpdateItem(obj, o => o.CostCenter, o=>o.ApproverID, o=>o.Condition);
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;

            return GetGridView(nodedt.GetItems(NodeID), Models.Helper.PartialParameter.NODE_DETAIL_LIST_GRID);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string NodeDetailID, string NodeID)
        {
            if (NodeID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.NodeDetailManager().DeleteItem(NodeDetailID);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return GetGridView(nodedt.GetItems(NodeID), Models.Helper.PartialParameter.NODE_DETAIL_LIST_GRID);
        }
    }
}
