using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FEA_BusinessLogic;

namespace ITSProject
{
    public class DeviceRegistrationService
    {
        public int CheckAlreadySigned(string MainDetailID)
        {
            int A = Convert.ToInt16(new FEA_BusinessLogic.Base.Connection().db.WFMainDetails.Where(s => s.MainDetailID == MainDetailID).Select(x => x.isFinished).SingleOrDefault().ToString());
            return A;
        }

        public List<sp_GetApprover_Result> CheckDuplicateApprover(string DocTypeName, int CodeCenterCode, string NodeID, string OrderCode, int currentUser)
        {
            List<sp_GetApprover_Result> lstApprover = new DeviceRegistrationManager().GetApprover(DocTypeName, CodeCenterCode, NodeID, OrderCode, currentUser);
            if (NodeID != "")
            {
                if (lstApprover.Where(x => x.ApproverID == currentUser).ToList().Count > 0)
                {
                    lstApprover = CheckDuplicateApprover(DocTypeName, CodeCenterCode, lstApprover[0].NodeID, OrderCode, currentUser);
                }
            }
            return lstApprover;
        }

        public int SignRejectDocument(
                                    string Type, string _sApprovedUser, string comment
                                    , string DocumentTypeName, string NodeID
                                    , string OrderCode, string Reason, string MainID, string MainDetailID
                                    , string NextNodeID, int CheckUserID, string DelegateID, int DeleagateUserID
                                    , int CurrentUserID, Boolean? IsAdminDoIt = false
                                    )
        {
            int Status = 0;
            bool _result = false;
            int CostCenterCodeOfCreator = -1;
            int CreatorID = -1;

            if (DocumentTypeName == "DEVICEREGISTRATION")
            {
                DeviceRegistration degreg = new DeviceRegistrationManager().GetItem(OrderCode);
                 CostCenterCodeOfCreator = degreg.User1.CostCenterCode;
                 CreatorID = degreg.CreatorID.Value;
            }
            else if (DocumentTypeName == "HARDWAREREQUIREMENT")
            {
                HardwareRequirement degreg = new HardwareRequirementManager().GetItem(OrderCode);
                CostCenterCodeOfCreator = degreg.User1.CostCenterCode;
                CreatorID = degreg.CreatorID.Value;
            }



            List<sp_GetApprover_Result> ApproverList = CheckDuplicateApprover(DocumentTypeName, CostCenterCodeOfCreator, NodeID, OrderCode, CurrentUserID);//new FEA_BusinessLogic.WaitingArea.WaitingArea().GetApprover(DocumentTypeName, CostCenterCodeOfCreator, NodeID,OrderCode);
            switch (Type)
            {
                case "Sign":
                    if (ApproverList.Count == 0)
                    {
                        _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateEndWorkFlow(OrderCode, DocumentTypeName, MainDetailID, comment, DelegateID, DeleagateUserID);
                        // if (_result)
                        // Controllers.HelperController.SendMailToITServices(CreatorID, string.Format("{0}-", OrderCode) + Reason, comment);
                    }
                    else
                    {
                        _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().UpdateNextWorkFlow(_sApprovedUser, MainDetailID, NextNodeID, MainID, comment, CheckUserID, DelegateID, DeleagateUserID);
                        // if (_result)
                        // Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(_sApprovedUser), Helper.UserLoginInfo.UserId, Reason, comment);

                    }
                    switch (_result)
                    {
                        case true:
                            Status = 1;
                            break;
                        case false:
                            Status = 0;
                            break;
                        default:
                            Status = -1;
                            break;
                    }
                    break;
                case "Reject":
                    _result = new FEA_BusinessLogic.WaitingArea.WaitingArea().RejectDocument(OrderCode, MainDetailID, DocumentTypeName, comment, CheckUserID, MainID, Convert.ToInt32(CreatorID), DelegateID, DeleagateUserID, IsAdminDoIt);
                    switch (_result)
                    {
                        case true:
                            Status = 1;
                            // Controllers.HelperController.SendMailToNextApprover(Convert.ToInt32(CreatorID), Helper.UserLoginInfo.UserId, Reason, comment);   // Aded by Steven 2014-08-21
                            break;
                        case false:
                            Status = 0;
                            break;
                        default:
                            Status = -1;
                            break;
                    }
                    break;
                default:
                    break;
            }
            return Status;
        }
    }
}