using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using ITSProject;
namespace ITSProject
{
    public interface IServices
    {
        Models.UserDTO Login(string UserName, string UserPass);
        Models.UserDTO GetUserInfomation(string UserID);
        List<Models.UserDTO> GetAllUser();
        List<Models.WaitingAreDTO> GetDocumentSigned(int UserID);
        List<Models.WaitingAreDTO> GetDocumentApprove(int UserID);

        ITSProject.Models.DeviceRegistrationDTO GetDeviceRegistrationItem(string OrderCode, string NodeID, string TypeUser
            , string MainID, string MainDetailID, int CheckUserID, string DelegateID, int DelegateUserID, int CurrentUserID);

        ITSProject.Models.HardwareRequirementDTO GetHardwareRequirementItem(string OrderCode, string NodeID, string TypeUser
                                                                , string MainID, string MainDetailID, int CheckUserID
                                                                , string DelegateID, int DelegateUserID, int CurrentUserID);


        bool RejectDocument(string _sApprovedUser, string comment
                                    , string DocumentTypeName, string NodeID
                                    , string OrderCode, string Reason, string MainID, string MainDetailID
                                    , string NextNodeID, int CheckUserID, string DelegateID, int DeleagateUserID
                                    , int CurrentUserID, int IsAdminDoIt);

        bool SignDocument(string _sApprovedUser, string comment
                                    , string DocumentTypeName, string NodeID
                                    , string OrderCode, string Reason, string MainID, string MainDetailID
                                    , string NextNodeID, int CheckUserID, string DelegateID, int DeleagateUserID
                                    , int CurrentUserID, int IsAdminDoIt);

        byte[] DownloadFile(String AttachmentLink, string FileName);

    }
}