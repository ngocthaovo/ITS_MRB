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
    public class WaitingAreaController : BaseController
    {

        public ActionResult MainPage()
        {

            return View();
        }

        //
        // GET: /WaitingArea/

        public ActionResult Index()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WaitingArea/Index") });
            //

            if (Request.Form["btnSubmitAll"] != null)
            {
                var lstItem = Request.Form["mydata"];
                var lstDelegate = Request.Form["DelegateData"];
                try
                {
                    if (lstItem.Trim().Length != 0)
                    {
                        Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
                        Dictionary<string, string> DelegateValues = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstDelegate); // "" - 0

                        if (values != null)
                        {
                            int iResult = 0;
                            foreach (KeyValuePair<string, string> entry in values)
                            {
                                sp_GetDataForMultiSign_Result DataForMultiSign = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetDataMultiSign(entry.Key);
                                if (DataForMultiSign != null)
                                {
                                    iResult = SignRejectDocument("Sign"
                                    , DataForMultiSign.sApprovedUser.ToString()
                                    , DataForMultiSign.comment
                                    , DataForMultiSign.DocumentTypeName
                                    , Convert.ToInt32(DataForMultiSign.CostCenterCodeOfCreator)
                                    , DataForMultiSign.NodeID
                                    , DataForMultiSign.OrderCode
                                    , DataForMultiSign.Reason
                                    , DataForMultiSign.MainID
                                    , DataForMultiSign.MainDetailID
                                    , DataForMultiSign.NextNodeID
                                    , Convert.ToInt32(DataForMultiSign.CreatorID)
                                    , Convert.ToInt32(DataForMultiSign.CheckUserID)
                                    , DelegateValues.Single().Key
                                    , Convert.ToInt32(DelegateValues.Single().Value) == 0 ? (int?)null : Convert.ToInt32(DelegateValues.Single().Value)
                                    , false);
                                }
                                else
                                {
                                    iResult = 0;
                                }

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




            Object[] _object = new Object[2];
            _object[0] = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetDocument(FEA_ITS_Site.Helper.UserLoginInfo.UserId);
            _object[1] = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetReferrenceDocumentList(FEA_ITS_Site.Helper.UserLoginInfo.UserId);
            return View(_object);
        }



        public ActionResult DocumentSigned()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WaitingArea/DocumentSigned") });
            //

            return View(new FEA_BusinessLogic.WaitingArea.WaitingArea().GetDocumentSigned(FEA_ITS_Site.Helper.UserLoginInfo.UserId));
        }
        //public ActionResult ERPHistory()
        //{
        //    return GetGridView(new FEA_BusinessLogic.WaitingArea.WaitingArea().GetERPHitory("%"+FEA_ITS_Site.Helper.UserLoginInfo.UserId+"%","%","%","%"), FEA_ITS_Site.Models.Helper.PartialParameter.ERPHistory);
        //}
        public ActionResult CallbackResult()
        {
            return GetGridView(new FEA_BusinessLogic.WaitingArea.WaitingArea().GetDocument(FEA_ITS_Site.Helper.UserLoginInfo.UserId), FEA_ITS_Site.Models.Helper.PartialParameter.WaitingDocument);
        }

        public ActionResult RefferenceListCallbackResult()
        {
            return GetGridView(new FEA_BusinessLogic.WaitingArea.WaitingArea().GetReferrenceDocumentList(FEA_ITS_Site.Helper.UserLoginInfo.UserId), FEA_ITS_Site.Models.Helper.PartialParameter.RefferenceList);
        }

        public ActionResult DocSignedCallbackResult()
        {
            return GetGridView(new FEA_BusinessLogic.WaitingArea.WaitingArea().GetDocumentSigned(FEA_ITS_Site.Helper.UserLoginInfo.UserId), "SignedDocument");
        }
        public static int SignRejectDocument(string Type, string _sApprovedUser, string comment
                                            , string DocumentTypeName, int CostCenterCodeOfCreator, string NodeID
                                            , string OrderCode, string Reason, string MainID, string MainDetailID
                                            , string NextNodeID,int CreatorID,int CheckUserID,string DelegateID,int? DeleagateUserID
                                            , Boolean?IsAdminDoIt = false, string DocumentTypeNameTitle="")
        {
            int Status = 0;
            bool _result = false;
            List<sp_GetApprover_Result> ApproverList = new DeviceRegistrationController().CheckDuplicateApprover(DocumentTypeName, CostCenterCodeOfCreator, NodeID, OrderCode);//new FEA_BusinessLogic.WaitingArea.WaitingArea().GetApprover(DocumentTypeName, CostCenterCodeOfCreator, NodeID,OrderCode);
            switch (Type)
            {
                case "Sign":
                    if (ApproverList.Count == 0)
                    {
                        if (DocumentTypeName == TagPrefixParameter.ACCESSORYOUT 
                            || DocumentTypeName == TagPrefixParameter.FABRICOUT
                            || DocumentTypeName == TagPrefixParameter.ACCESSORYMOVEOUT
                            || DocumentTypeName == TagPrefixParameter.ACCESSORYMOVEOUTMULTI
                            || DocumentTypeName == TagPrefixParameter.FABRICMOVEOUT
                            || DocumentTypeName == TagPrefixParameter.FABRICMOVEOUTMULTI
                            || DocumentTypeName == TagPrefixParameter.FABRICDEVELOPOUT
                            || DocumentTypeName == TagPrefixParameter.ACCESSORYDEVELOPOUT
                            || DocumentTypeName == TagPrefixParameter.DEVELOPPRODUCT
                            || DocumentTypeName == TagPrefixParameter.SUGGESTRUINOUT)
                        {
                            int update = new FEA_BusinessLogic.ERP.Order().ERPUpdateStatus(OrderCode, comment, 3);
                            if (update == 1)
                            {
                                _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateEndWorkFlow(OrderCode, DocumentTypeName, MainDetailID, comment, DelegateID, DeleagateUserID);
                            }
                           else
                            {
                                Status = (int)Models.Helper.EditItemStatus.failed;
                                return Status;
                            }

                           if(_result)
                           {
                               Controllers.HelperController.SendMailToERPCreator(CreatorID, string.Format("{0}", OrderCode) + Reason, comment);
                           }
                        }
                        else
                        {
                            _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateEndWorkFlow(OrderCode, DocumentTypeName, MainDetailID, comment, DelegateID, DeleagateUserID);
                            if (_result)
                            {
                                if (DocumentTypeName == TagPrefixParameter.SECURITYAREA)
                                {
                                    Controllers.HelperController.SendMailToSEServices(CreatorID, string.Format("{0}-", OrderCode) + Reason, comment);
                                }
                                else if (DocumentTypeName == TagPrefixParameter.GENERALAFFAIR)
                                {
                                    Controllers.HelperController.SendMailToGAServices(CreatorID, string.Format("{0}-", OrderCode) + Reason, comment);
                                }
                                else if(DocumentTypeName == TagPrefixParameter.MAINTENANCE)
                                {
                                    Controllers.HelperController.SendMailToMaintenance(CreatorID, string.Format("{0}-", OrderCode) + Reason, comment);
                                }
                                else
                                {
                                    Controllers.HelperController.SendMailToITServices(CreatorID, string.Format("{0}-", OrderCode) + Reason, comment);
                                }
                            }
                        }
                    }
                    else
                    {
                        _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateNextWorkFlow(_sApprovedUser, MainDetailID, NextNodeID, MainID, comment, CheckUserID,DelegateID,DeleagateUserID);
                        if (_result)
                            Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), Helper.UserLoginInfo.UserId, Reason, comment, DocumentTypeNameTitle);

                    }
                    switch (_result)
                    {
                        case true:
                            Status = (int)Models.Helper.EditItemStatus.success;
                            break;
                        case false:
                            Status = (int)Models.Helper.EditItemStatus.failed;
                            break;
                        default:
                            Status = (int)Models.Helper.EditItemStatus.nomal;
                            break;
                    }
                    break;
                case "Reject":
                    if (DocumentTypeName == TagPrefixParameter.ACCESSORYOUT 
                        || DocumentTypeName == TagPrefixParameter.FABRICOUT
                        || DocumentTypeName == TagPrefixParameter.ACCESSORYMOVEOUT
                        || DocumentTypeName == TagPrefixParameter.ACCESSORYMOVEOUTMULTI
                        || DocumentTypeName == TagPrefixParameter.FABRICMOVEOUT
                        || DocumentTypeName == TagPrefixParameter.FABRICMOVEOUTMULTI
                        || DocumentTypeName == TagPrefixParameter.FABRICDEVELOPOUT
                        || DocumentTypeName == TagPrefixParameter.ACCESSORYDEVELOPOUT
                        || DocumentTypeName == TagPrefixParameter.DEVELOPPRODUCT
                        || DocumentTypeName == TagPrefixParameter.SUGGESTRUINOUT)
                    {
                       
                        int update = new FEA_BusinessLogic.ERP.Order().ERPUpdateStatus(OrderCode, comment, 4);
                        if(update ==1)
                        {
                            _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().RejectDocument(OrderCode, MainDetailID, DocumentTypeName, comment, CheckUserID, MainID, Convert.ToInt32(CreatorID), DelegateID, DeleagateUserID, IsAdminDoIt);
                        }
                        else
                        {
                            Status = (int)Models.Helper.EditItemStatus.failed;
                            return Status;
                        }
                        switch (_result)
                        {
                            case true:
                                Status = (int)Models.Helper.EditItemStatus.success;
                                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(CreatorID), Helper.UserLoginInfo.UserId, Reason, comment);   // Aded by Steven 2014-08-21   
                                break;
                            case false:
                                Status = (int)Models.Helper.EditItemStatus.failed;
                                break;
                            default:
                                Status = (int)Models.Helper.EditItemStatus.nomal;
                                break;
                        }
                    }
                    else
                    {
                        _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().RejectDocument(OrderCode, MainDetailID, DocumentTypeName, comment, CheckUserID, MainID, Convert.ToInt32(CreatorID), DelegateID, DeleagateUserID, IsAdminDoIt);
                        switch (_result)
                        {
                            case true:
                                Status = (int)Models.Helper.EditItemStatus.success;
                                Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(CreatorID), Helper.UserLoginInfo.UserId, Reason, comment);   // Aded by Steven 2014-08-21   
                                break;
                            case false:
                                Status = (int)Models.Helper.EditItemStatus.failed;
                                break;
                            default:
                                Status = (int)Models.Helper.EditItemStatus.nomal;
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            return Status;
        }
    }
}
