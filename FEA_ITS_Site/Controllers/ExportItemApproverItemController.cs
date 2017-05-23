using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using FEA_BusinessLogic;
using FEA_SABusinessLogic;
namespace FEA_ITS_Site.Controllers
{
    public class ExportItemApproverItemController : BaseController
    {
        //
        // GET: /ExportItemApproverItem/

        public ActionResult Index()
        {
            return View();
        }


        #region mainPage
        public ActionResult ExportItemDepartmentPartial()
        {
           Dictionary<int,string> lstItem = new FEA_SABusinessLogic.SAExportApprovalItemManager().GetCostCenter();
           return PartialView("_DepartmentListPartial", lstItem);
        
        }

        public ActionResult ExportItemPartial()
        {
 
            string ExportItemDepartmentID = (Request.Params["ExportItemDepartmentID"] != null) ? Request.Params["ExportItemDepartmentID"].ToString() : "-1";
            Dictionary<string, string> lstItem = new FEA_SABusinessLogic.SAExportApprovalItemManager().GetItemType(int.Parse(ExportItemDepartmentID));
            return PartialView("_ItemListPartial", lstItem);
        }

        public ActionResult ExportItemDetailPartial()
        {

            string ExportItemDepartmentID = (Request.Params["ExportItemDepartmentID"] != null) ? Request.Params["ExportItemDepartmentID"].ToString() : "-1";
            string ExportItemID = (Request.Params["ExportItemID"] != null) ? Request.Params["ExportItemID"].ToString() : "-1";
            Dictionary<string, string> lstItem = new FEA_SABusinessLogic.SAExportApprovalItemManager().GetItemDetail(int.Parse(ExportItemDepartmentID), ExportItemID);

            return PartialView("_ItemDetailListPartial", lstItem);
        }
        #endregion

        #region mamagement
        public ActionResult Management()
        {
            return View();
        }

       [ValidateInput(false)]
        public ActionResult EditModesPartial()
        {
            return GetGridView(new SAExportApprovalItemManager().GetItems(-1), Models.Helper.PartialParameter.ExportItemApproverItemList);
        }
       [HttpPost, ValidateInput(false)]
       public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] ExportItemApproverItem obj)
       {
           if (ModelState.IsValid)
           {
               try
               {
                   if (obj.CostCenterCode != null && obj.ItemID != null && obj.ItemID != "")
                   {
                       int _result = new SAExportApprovalItemManager().InsertExportItemApproverItem(obj);
                       if (_result < 1)
                       {
                           if (_result == -1)
                               ViewData["EditError"] = "Dữ liệu đã tồn tại";
                           else 
                               ViewData["EditError"] = Resources.Resource.msgInsertFail;
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
           return EditModesPartial();
       }


       [HttpPost, ValidateInput(false)]
       public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] ExportItemApproverItem obj)
       {
           if (ModelState.IsValid)
           {
               try
               {
                   if (obj.CostCenterCode != null && obj.ItemID != null && obj.ItemID != "")
                   {
                       bool _result = new SAExportApprovalItemManager().UpdateItem(obj, o => o.ApproverID
                                                                                      // , o => o.CostCenterCode, o => o.ItemID, o => o.ItemDetailID
                                                                                       , o=>o.Status);
                       if (!_result)
                           ViewData["EditError"] = Resources.Resource.msgInsertFail;
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
           return EditModesPartial();
       }

       [HttpPost, ValidateInput(false)]
       public ActionResult EditModesDeletePartial(string ID)
       {
           if (ID.Length != 0)
           {
               try
               {
                   bool flag = new SAExportApprovalItemManager().DeleteExportItemApproverItem(ID);
                   if (!flag)
                       ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
               }
               catch (Exception e)
               {
                   ViewData["DeleteError"] = e.Message;
               }
           }
           return EditModesPartial();
       }

       public ActionResult CountryComboBoxPartial()
       {
           return PartialView("_ItemGridPartial", new FEA_BusinessLogic.ItemManager().GetItems(FEA_ITS_Site.Models.Helper.TagPrefixParameter.SECURITYAREA, "", 1));
       }
        #endregion
    }
}
