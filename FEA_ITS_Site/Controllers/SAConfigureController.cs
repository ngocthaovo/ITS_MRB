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
    public class SAConfigureController : BaseController
    {
        //
        // GET: /SAConfigure/

        public ActionResult SAConfigure()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult GetAllReason()
        {
            return GetGridView(new SAReasonManager().GetItems(), Models.Helper.PartialParameter.SAReason);
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult AddNewReason([ModelBinder(typeof(DevExpressEditorsBinder))] SAReason obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.Reason != null && obj.Reason != "" && obj.isConstraint != null && obj.Temp1 != null && obj.Temp1 != "")
                    {
                        int _result = new FEA_SABusinessLogic.SAReasonManager().InsertItem(obj);
                        if (_result < 1)
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
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return GetAllReason();
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateReason([ModelBinder(typeof(DevExpressEditorsBinder))] SAReason obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.Reason != null && obj.Reason != "" && obj.isConstraint != null && obj.Temp1 != null && obj.Temp1 != "")
                    {
                        bool _result = new FEA_SABusinessLogic.SAReasonManager().UpdateItem(obj, o => o.ID, o => o.Reason, o => o.isConstraint, o=> o.Status,o=>o.Temp1 /*Order Type*/);
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
            return GetAllReason();
        }

        public ActionResult LoadConstraintByPosition(string ReasonID)
        {
            ViewData["ReasonID"] = ReasonID;
            return GetGridView(new SAConfigureManager().GetItems(ReasonID) as List<SAConfigure>, Models.Helper.PartialParameter.SAConfigure);
        }

       [ValidateInput(false)] 
        public ActionResult GetAllConfigure([ModelBinder(typeof(DevExpressEditorsBinder))] string ReasonID)
        {
            ViewData["ReasonID"] = ReasonID;
            return GetGridView(new SAConfigureManager().GetItems(ReasonID), Models.Helper.PartialParameter.SAConfigure);
        }


        public ActionResult AddNewConfigure([ModelBinder(typeof(DevExpressEditorsBinder))] SAConfigure obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.ReasonID != null && obj.PositionID != null && obj.Quantity != null && obj.ReasonID != "")
                    {
                        int _result = new FEA_SABusinessLogic.SAConfigureManager().InsertItem(obj);
                        if (_result < 1)
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
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return GetAllConfigure(obj.ReasonID);
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateConfigure([ModelBinder(typeof(DevExpressEditorsBinder))] SAConfigure obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.ReasonID != null && obj.PositionID != null && obj.Quantity != null && obj.ReasonID != "")
                    {
                        bool _result = new FEA_SABusinessLogic.SAConfigureManager().UpdateItem(obj, o => o.PositionID, o => o.ReasonID, o => o.Quantity);
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
            return GetAllConfigure(obj.ReasonID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult DeleteConfigure(string ReasonID, string PositionID)
        {
            string[] key = Request.Params["ReasonID;PositionID;Temp1"].Split('|');
                try
                {
                    bool flag = new FEA_SABusinessLogic.SAConfigureManager().DeleteItem(key[2]);
                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteRealatedNeeded;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
                return GetAllConfigure(key[0]);
        }

        [ValidateInput(false)]
        public ActionResult GetAllDestination()
        {
            return GetGridView(new SADestinationManager().GetItems(), Models.Helper.PartialParameter.SADestination);
        }


        public ActionResult AddNewDestination([ModelBinder(typeof(DevExpressEditorsBinder))] SADestination obj)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    if (obj.CompanyCode != null && obj.CompanyName != null)//obj.ID != null && 
                    {
                        int _result = new FEA_SABusinessLogic.SADestinationManager().InsertItem(obj);
                        if (_result < 1)
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
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return GetAllDestination();
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateDestination([ModelBinder(typeof(DevExpressEditorsBinder))] SADestination obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.ID != null && obj.CompanyCode != null && obj.CompanyName != null)
                    {
                        bool _result = new FEA_SABusinessLogic.SADestinationManager().UpdateItem(obj, o => o.CompanyCode, o => o.CompanyName, o => o.Status);
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
            return GetAllDestination();
        }

    }
}
