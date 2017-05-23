using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace ITSProject
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public Models.UserDTO Login(string UserName, string Userpass)
        {
            return new Services().Login(UserName, Userpass);
        }

        [WebMethod]
        public Models.UserDTO GetUserInfomation(string UserID)
        {
            return new Services().GetUserInfomation(UserID);
        }

        [WebMethod]
        public List<Models.UserDTO> GetAllUser()
        {
            return new Services().GetAllUser();
        }

        [WebMethod]
        public List<Models.WaitingAreDTO> GetDocumentSigned(int UserID)
        {
            return new Services().GetDocumentSigned(UserID);
        }


        [WebMethod]
        public List<Models.WaitingAreDTO> GetDocumentApprove(int UserID)
        {
            return new Services().GetDocumentApprove(UserID);
        }

        [WebMethod]
        public Models.DeviceRegistrationDTO GetDeviceRegistrationItem(string OrderCode, string NodeID, string TypeUser
            , string MainID, string MainDetailID, int CheckUserID
            , string DelegateID, int DelegateUserID, int CurrentUserID)
        {            
            // testing
            //OrderCode = "DR-151005/006";
            //NodeID = "e24784e8-b5ed-4d14-8404-49afdac91b93";
            //TypeUser = "manager";
            //MainID = "23f8a4b6-4ce6-44dc-8b47-2b0a1b75ca6b";
            //MainDetailID = "a49da8b4-4556-44a2-a277-6338e33746b3";
            //CheckUserID = 4030;
            //DelegateID = "";
            //DelegateUserID = 0;
            //CurrentUserID=4030;

            return new Services().GetDeviceRegistrationItem(OrderCode, NodeID, TypeUser, MainID, MainDetailID, CheckUserID, DelegateID, DelegateUserID, CurrentUserID);
        }


        [WebMethod]
        public Models.HardwareRequirementDTO HardwareRequirementItem(string OrderCode, string NodeID, string TypeUser
            , string MainID, string MainDetailID, int CheckUserID
            , string DelegateID, int DelegateUserID, int CurrentUserID)
        {
            // testing
            //OrderCode = "PR1510199001";
            //NodeID = "cfd71b0e-fad4-47d8-b244-6b3e7205534c";
            //TypeUser = "manager";
            //MainID = "bc52877b-936b-41de-8f32-7911a54ec777";
            //MainDetailID = "ee5cca1c-4446-4c1a-b699-bcd2de24427b";
            //CheckUserID = 3762;
            //DelegateID = "";
            //DelegateUserID = 0;
            //CurrentUserID = 4030;

            return new Services().GetHardwareRequirementItem(OrderCode, NodeID, TypeUser, MainID, MainDetailID, CheckUserID, DelegateID, DelegateUserID, CurrentUserID);
        }

        [WebMethod]
        public List<Models.FileInfoDTO> GetFile(string filePath)
        {
            return Ultility.ConvertHelper.DeviceRegistrationHelper.GetFilesofOrder(filePath);
        }


        [WebMethod()]
        public byte[] DownloadFile(String AttachmentLink, string FileName)
        {
            return new Services().DownloadFile( AttachmentLink,  FileName);
        }
    }
}
