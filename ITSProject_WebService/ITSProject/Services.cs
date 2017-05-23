using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using FEA_BusinessLogic;

namespace ITSProject
{
    public class Services : IServices
    {

        public Models.UserDTO Login(string UserName, string UserPass) // Hàm Kiểm tra Login của User
        {
            User u = new FEA_BusinessLogic.UserManager().LogIn(UserName, UserPass);
            if (u != null)
            {
                return new Models.UserDTO() { UserID = u.UserID, UserName = u.UserName };
            }
            return null;
        }

        public Models.UserDTO GetUserInfomation(string UserID)
        {
            User u = new FEA_BusinessLogic.UserManager().GetItem(UserID);
            if (u != null)
            {
                return new Models.UserDTO()
                {
                    UserID = u.UserID,
                    UserCodeID = u.UserCodeID,
                    UserPass = u.UserPass,
                    UserName = u.UserName,
                    UserNameEN = u.UserNameEN,
                    CostCenterCode = u.CostCenterCode,
                    UserAddress = u.UserAddress,
                    UserSex = u.UserSex,
                    UserPosstion = u.UserPosstion,
                    UserEmail = u.UserEmail,
                    UserPhone = u.UserPhone,
                    UserStartDate = u.UserStartDate,
                    UserLastLogin = u.UserLastLogin,
                    UserExpired = u.UserExpired,
                    UserGroupID = u.UserGroupID,
                    Temp1 = u.Temp1,
                    Temp2 = u.Temp2,
                    Temp3 = u.Temp3,
                    Temp4 = u.Temp4,
                    Enabled = u.Enabled,
                    FactoryID = u.FactoryID
                };
            }
            return null;
        } // Hàm Lấy thông tin chi tiết của user

        public List<Models.UserDTO> GetAllUser()
        {
            var lst = new FEA_BusinessLogic.UserManager().GetItems("", "");

            List<Models.UserDTO> lstResult = new List<Models.UserDTO>();
            if (lst != null)
            {
                foreach (User u in lst)
                {
                    lstResult.Add(new Models.UserDTO()
                    {


                        UserID = u.UserID,
                        UserCodeID = u.UserCodeID,
                        UserPass = u.UserPass,
                        UserName = u.UserName,
                        UserNameEN = u.UserNameEN,
                        CostCenterCode = u.CostCenterCode,
                        UserAddress = u.UserAddress,
                        UserSex = u.UserSex,
                        UserPosstion = u.UserPosstion,
                        UserEmail = u.UserEmail,
                        UserPhone = u.UserPhone,
                        UserStartDate = u.UserStartDate,
                        UserLastLogin = u.UserLastLogin,
                        UserExpired = u.UserExpired,
                        UserGroupID = u.UserGroupID,
                        Temp1 = u.Temp1,
                        Temp2 = u.Temp2,
                        Temp3 = u.Temp3,
                        Temp4 = u.Temp4,
                        Enabled = u.Enabled,
                        FactoryID = u.FactoryID
                    });
                }
            }
            return lstResult;
        } //Hàm trả về thông tin của tất cả các User aaa


        #region "Waiting Area"
        public List<Models.WaitingAreDTO> GetDocumentSigned(int UserID)
        {
            List<Models.WaitingAreDTO> lstResult = new List<Models.WaitingAreDTO>();
            var items = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetDocumentSigned(UserID);
            foreach (FEA_BusinessLogic.sp_GetDocumentSigned_Result item in items)
            {
                /* Sửa lại dùm em chổ này, thông tin trả về giống với hàm GetDocumentApprove
                 */
                Models.WaitingAreDTO model = new Models.WaitingAreDTO()
                {
                    //CreatedDate = item.CreateDate,
                    //DepartmentID ="",
                    //DepartmentName=item.Department,
                    //Description = "",
                    //DocumentCode = item.OrderCode,
                    //StatusString = (item.isFinished == 0)?"Sending":"Finished",
                    //DocumentID = "",
                    //DocumentType = item.DocumentTypeName,
                    //EmployeeName = item.Creator,
                    //ApproverName = item.Approver

                    CreatedDate = item.CreateDate,
                    DepartmentID = "",
                    DepartmentName = item.Department, // thieu ten
                    Description = "",
                    DocumentCode = item.OrderCode,
                    StatusString = (item.isFinished == 0) ? "Sending" : "Finished",
                    DocumentID = "",
                    DocumentType = item.DocumentTypeName,
                    EmployeeName = item.Creator,
                    ApproverName = item.Approver,

                    NodeID = "",
                    TypeUser = "manager_viewsigned",
                    MainID = item.MainID,
                    MainDetailID = "",
                    CheckUserID = 0,
                    DelegateUserID = 0,
                    DelegateID = "",
                    IsUrgent = true

                };

                lstResult.Add(model);
            }
            return lstResult;
        }

        public List<Models.WaitingAreDTO> GetDocumentApprove(int UserID)
        {
            List<Models.WaitingAreDTO> lstResult = new List<Models.WaitingAreDTO>();
            var items = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetDocument(UserID);
            foreach (FEA_BusinessLogic.sp_GetDocumentApprove_Result item in items)
            {

                int _status = 0;
                if (item.DocumentTypeName == "DEVICEREGISTRATION")
                    _status = int.Parse(new FEA_BusinessLogic.DeviceRegistrationManager().GetItem(item.OrderCode).Status.ToString());
                else if (item.DocumentTypeName == "HARDWAREREQUIREMENT")
                    _status = int.Parse(new FEA_BusinessLogic.HardwareRequirementManager().GetItem(item.OrderCode).Status.ToString());

                Models.WaitingAreDTO model = new Models.WaitingAreDTO()
                {
                    /* them vào tên department, Urgent, create date
                     */
                    CreatedDate = item.CreateDate,
                    DepartmentID = "",
                    DepartmentName = item.CreatorDepartment, // thieu ten
                    Description = "",
                    DocumentCode = item.OrderCode,
                    StatusString = item.SignType,
                    DocumentID = "",
                    DocumentType = item.DocumentTypeName,
                    EmployeeName = item.Creator,
                    ApproverName = item.Approver,

                    NodeID = item.NodeID,
                    TypeUser = (_status == 4) ? "user" : "manager",
                    MainID = item.MainID,
                    MainDetailID = item.MainDetailID,
                    CheckUserID = item.CheckUserID,
                    DelegateUserID = item.DelegateUserID,
                    DelegateID = item.DelegateID,

                    IsUrgent = true,


                };

                lstResult.Add(model);
            }
            return lstResult;
        }
        #endregion

        #region "DeviceRegistration"
        public Models.DeviceRegistrationDTO GetDeviceRegistrationItem(string OrderCode, string NodeID, string TypeUser
                                                                        , string MainID, string MainDetailID, int CheckUserID
                                                                        , string DelegateID, int DelegateUserID, int CurrentUserID)
        {

            return Ultility.ConvertHelper.DeviceRegistrationHelper.ConvertToDeviceRegistration(OrderCode, NodeID, TypeUser, MainID, MainDetailID, CheckUserID, DelegateID, DelegateUserID, CurrentUserID);
        }
        #endregion

        #region Hardware Requitement
        public Models.HardwareRequirementDTO GetHardwareRequirementItem(string OrderCode, string NodeID, string TypeUser
                                                                , string MainID, string MainDetailID, int CheckUserID
                                                                , string DelegateID, int DelegateUserID, int CurrentUserID)
        {

            return Ultility.ConvertHelper.HardwareRequirementHelper.ConvertToHardwareRequirement(OrderCode, NodeID, TypeUser, MainID, MainDetailID, CheckUserID, DelegateID, DelegateUserID, CurrentUserID);
        }
        #endregion

        #region "Common function"
        /// <summary>
        /// Approver to sign a document
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="_sApprovedUser"></param>
        /// <param name="comment"></param>
        /// <param name="DocumentTypeName"></param>
        /// <param name="NodeID"></param>
        /// <param name="OrderCode"></param>
        /// <param name="Reason"></param>
        /// <param name="MainID"></param>
        /// <param name="MainDetailID"></param>
        /// <param name="NextNodeID"></param>
        /// <param name="CheckUserID"></param>
        /// <param name="DelegateID"></param>
        /// <param name="DeleagateUserID"></param>
        /// <param name="CurrentUserID"></param>
        /// <param name="IsAdminDoIt"></param>
        /// <returns></returns>
        public bool RejectDocument(string _sApprovedUser, string comment
                                    , string DocumentTypeName, string NodeID
                                    , string OrderCode, string Reason, string MainID, string MainDetailID
                                    , string NextNodeID, int CheckUserID, string DelegateID, int DeleagateUserID
                                    , int CurrentUserID, int IsAdminDoIt)
        {
            bool admin = false;
            if (IsAdminDoIt == 1)
                admin = true;

            return new DeviceRegistrationService().SignRejectDocument("Reject", _sApprovedUser, comment, DocumentTypeName, NodeID,
                                                                       OrderCode, Reason, MainID, MainDetailID, NextNodeID, CheckUserID
                                                                       , DelegateID, DeleagateUserID, CurrentUserID, admin) == 1 ? true : false;


        }


        /// <summary>
        /// Approver to Reject a document
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="_sApprovedUser"></param>
        /// <param name="comment"></param>
        /// <param name="DocumentTypeName"></param>
        /// <param name="NodeID"></param>
        /// <param name="OrderCode"></param>
        /// <param name="Reason"></param>
        /// <param name="MainID"></param>
        /// <param name="MainDetailID"></param>
        /// <param name="NextNodeID"></param>
        /// <param name="CheckUserID"></param>
        /// <param name="DelegateID"></param>
        /// <param name="DeleagateUserID"></param>
        /// <param name="CurrentUserID"></param>
        /// <param name="IsAdminDoIt"></param>
        /// <returns></returns>
        public bool SignDocument(string _sApprovedUser, string comment
                                    , string DocumentTypeName, string NodeID
                                    , string OrderCode, string Reason, string MainID, string MainDetailID
                                    , string NextNodeID, int CheckUserID, string DelegateID, int DeleagateUserID
                                    , int CurrentUserID, int IsAdminDoIt)
        {
            bool admin = false;
            if (IsAdminDoIt == 1)
                admin = true;

            return new DeviceRegistrationService().SignRejectDocument("Sign", _sApprovedUser, comment, DocumentTypeName, NodeID,
                                                                       OrderCode, Reason, MainID, MainDetailID, NextNodeID, CheckUserID
                                                                       , DelegateID, DeleagateUserID, CurrentUserID, admin)==1?true:false;
        }
        #endregion




        public byte[] DownloadFile(String AttachmentLink, string FileName)
        {
            return Ultility.FileHelper.DownloadFile(AttachmentLink, FileName);
        }

        

    }
}