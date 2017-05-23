using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
using FEA_SABusinessLogic;
namespace FEA_ITS_Site.Controllers
{
    public class HelperController : BaseController
    {
        //
        // GET: /Helper/

        #region Generate Code
        /// <summary>
        /// Get Order Code
        /// </summary>
        /// <param name="DocumentTypeName"></param>
        /// <returns></returns>
        public static string GenerateCode(string DocumentTypeName)
        {
            int NumberAutoIncrease = 0;
            string OrderCode = string.Empty;
            string Prefix = string.Empty;

            List<FEA_BusinessLogic.TagPrefix> PrefixList = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetItem(DocumentTypeName);
            if (PrefixList.Count == 0)
            {
                OrderCode = "";
            }
            else
            {
                Prefix = PrefixList[0].Prefix;
                OrderCode = CheckAvailableOrderCode(Prefix, NumberAutoIncrease,DocumentTypeName);

            }
            return OrderCode;
        }
        /// <summary>
        /// Check Order code in DB is different and return the Order Code
        /// </summary>
        /// <param name="Prefix"></param>
        /// <param name="NumberAutoIncrease"></param>
        /// <returns></returns>
        private static string CheckAvailableOrderCode(string Prefix, int NumberAutoIncrease,string DocumentTypeName)
        {
            string OrderCode = string.Empty;
            NumberAutoIncrease++;
            string Sequence = IncreseNumber(NumberAutoIncrease);
            OrderCode = Prefix + "-" + DateTime.Now.Year.ToString().Substring(2, 2) + GetMonth(DateTime.Now.Month.ToString()) + GetMonth(DateTime.Now.Day.ToString()) + "/" + Sequence;
            int _Result = 0;
            string strDocType = "";//Added by Tony (2017-04-26)
            int iOrderType = 0; //Added by Tony(2017-04-26)
            switch (DocumentTypeName)
            {
                case "DEVICEREGISTRATION":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetDRForCheck(OrderCode);
                    break;
                case "STOCKIN":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetSIForCheck(OrderCode);
                    break;
                case "STOCKOUT":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetSOForCheck(OrderCode);
                    break;
                case "WAREHOUSE":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetWHAForCheck(OrderCode);
                    break;
                case "WAREHOUSEEXPORT":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetWHAExportForCheck(OrderCode);
                    break;
                case "SECURITYAREA":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetSAForCheck(OrderCode);
                    break;
                case "WHIMPORT":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetWHIForCheck(OrderCode);
                    break;
                case "GENERALAFFAIR":
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetGAForCheck(OrderCode).Count();
                    break;
                case "MAINTENANCESTOCKIN":
                    strDocType = "MAINTENANCESTOCK";
                    iOrderType = 1;
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetMNSForCheck(OrderCode, strDocType, iOrderType);
                    break;
                case "MAINTENANCESTOCKOUT":
                    strDocType = "MAINTENANCESTOCK";
                    iOrderType = 2;
                    _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetMNSForCheck(OrderCode, strDocType, iOrderType);
                    break;
                default:
                    break;
            }

            if (_Result != 0)
            {
                OrderCode = CheckAvailableOrderCode(Prefix, NumberAutoIncrease,DocumentTypeName);
            }

            return OrderCode;

        }
        /// <summary>
        /// Generate number use 3 chraracter
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        private static string IncreseNumber(int Number)
        {
            string Sequence = string.Empty;
            if (Number < 10)
            {
                Sequence = "00" + Number;
            }
            if (Number < 100 && Number >= 10)
            {
                Sequence = "0" + Number;
            }
            if (Number >= 100)
            {
                Sequence = Number.ToString();
            }
            return Sequence;
        }
        #endregion

        #region     generate for eland
        public static string GenerateELandCode(string DocumentTypeName)
        {
            int NumberAutoIncrease = 9000;
            string companyCode= Helper.UserLoginInfo.CurrentUser.CostCenter.CompanyCode.ToString();
            if (companyCode == "7910" && DocumentTypeName != FEA_ITS_Site.Models.Helper.TagPrefixParameter.MAINTENANCE)
                NumberAutoIncrease = 9000;
            else if (companyCode == "7920" && DocumentTypeName != FEA_ITS_Site.Models.Helper.TagPrefixParameter.MAINTENANCE)
                NumberAutoIncrease = 8000;
            //Added by Tony (2017-04-19) Begin 
            else if (companyCode == "7910" && DocumentTypeName == FEA_ITS_Site.Models.Helper.TagPrefixParameter.MAINTENANCE)
                NumberAutoIncrease = 6000;
            //End
            string OrderCode = string.Empty;
            string Prefix = string.Empty;

            List<FEA_BusinessLogic.TagPrefix> PrefixList = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetItem(DocumentTypeName);
            if (PrefixList.Count == 0)
            {
                OrderCode = "";
            }
            else
            {
                Prefix = PrefixList[0].Prefix;
                OrderCode = CheckAvailableElandCode(Prefix, NumberAutoIncrease, DocumentTypeName);

            }
            return OrderCode;
        }

        private static string CheckAvailableElandCode(string Prefix, int NumberAutoIncrease, string DocumentTypeName)
        {
            string OrderCode = string.Empty;
            NumberAutoIncrease++;
            string Sequence = NumberAutoIncrease.ToString();
            OrderCode = Prefix + DateTime.Now.Year.ToString().Substring(2, 2) + GetMonth(DateTime.Now.Month.ToString()) + GetMonth(DateTime.Now.Day.ToString()) + Sequence;


            int count = 0;
            if (DocumentTypeName == FEA_ITS_Site.Models.Helper.TagPrefixParameter.HARD_REGISTRATION)
            {
                List<FEA_BusinessLogic.HardwareRequirement> _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetPRForCheck(OrderCode);
                count = _Result.Count;
            }

            else if (DocumentTypeName == FEA_ITS_Site.Models.Helper.TagPrefixParameter.GENERALAFFAIR)
            {
                List<FEA_BusinessLogic.GAItem> _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetGAForCheck(OrderCode);
                count = _Result.Count;
            }
            //Added by Tony (2017-04-19) Begin
            else if(DocumentTypeName == FEA_ITS_Site.Models.Helper.TagPrefixParameter.MAINTENANCE)
            {
                List<FEA_BusinessLogic.MNRequestMain> _Result = new FEA_BusinessLogic.GenerateCode.GenerateCode().GetMNForCheck(OrderCode);
                count = _Result.Count;
            }
            // End
            if (count > 0)
            {
                OrderCode = CheckAvailableElandCode(Prefix, NumberAutoIncrease, DocumentTypeName);
            }

            return OrderCode;
        }

        private static string GetMonth(string Month)
        {
            string _Month = string.Empty;
            if (Month.Length == 1)
                _Month = "0" + Month;
            else
                _Month = Month;
            return _Month;
        }

        #endregion  

        #region "Status Or Order"
        public static string GetStatusName(int status)
        {
            string _statusName=Resources.Resource.STATUS_UNKNOW;
            if (status == (int)DeviceRegistrationManager.OrderStatus.CHECKED)
                _statusName = Resources.Resource.STATUS_CHECKED;
            else if (status == (int)DeviceRegistrationManager.OrderStatus.DRAFT)
                _statusName = Resources.Resource.STATUS_DRAFT;
            else if (status == (int)DeviceRegistrationManager.OrderStatus.SENDING)
                _statusName = Resources.Resource.STATUS_SENDING;
            else if (status == (int)DeviceRegistrationManager.OrderStatus.RETURNED)
                _statusName = Resources.Resource.STATUS_RETURNED;
            else if (status == (int)DeviceRegistrationManager.OrderStatus.FINSHED)
                _statusName = Resources.Resource.STATUS_FINSHED;
            else if (status == (int)DeviceRegistrationManager.OrderStatus.DELETED)
                _statusName = Resources.Resource.STATUS_DELETED;
            else if (status == (int)DeviceRegistrationManager.OrderStatus.New)
                _statusName = Resources.Resource.STATUS_NEW;
            else if (status == (int)ExportItemManager.OrderStatus.SPENDING)
                _statusName = Resources.Resource.STATUS_SPENDING;
            return _statusName;

        }

        public static Dictionary<int,string> GetStatusList()
        {
            Dictionary<int, string> _result = new Dictionary<int,string>();
            _result.Add((int)DeviceRegistrationManager.OrderStatus.CHECKED, Resources.Resource.STATUS_CHECKED);
            _result.Add((int)DeviceRegistrationManager.OrderStatus.DRAFT, Resources.Resource.STATUS_DRAFT);
            _result.Add((int)DeviceRegistrationManager.OrderStatus.SENDING, Resources.Resource.STATUS_SENDING);
            _result.Add((int)DeviceRegistrationManager.OrderStatus.RETURNED, Resources.Resource.STATUS_RETURNED);
            _result.Add((int)DeviceRegistrationManager.OrderStatus.FINSHED, Resources.Resource.STATUS_FINSHED);
            _result.Add((int)DeviceRegistrationManager.OrderStatus.DELETED, Resources.Resource.STATUS_DELETED);
            _result.Add((int)ExportItemManager.OrderStatus.SPENDING, Resources.Resource.STATUS_SPENDING);
            //Add by Tony (2016-09-19)
            _result.Add((int)DeviceRegistrationManager.OrderStatus.PUSHDATA, Resources.Resource.Pushed);
            return _result;

        }

        public static Dictionary<int, string> GetWHStatusList()
        {
            Dictionary<int, string> _result = new Dictionary<int, string>();
            _result.Add((int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.OrderStatus.SAVED, Resources.Resource.STATUS_SAVED);
            _result.Add((int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.OrderStatus.DELETED, Resources.Resource.STATUS_DELETED);
            _result.Add((int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.OrderStatus.New, Resources.Resource.STATUS_NEW);
            return _result;

        }
        #endregion

        #region Sent email when approve document - deviceres,hard...
        /// <summary>
        /// 
        /// </summary>
        /// <param name="approverID"></param>
        /// <param name="senderID"></param>
        /// <param name="sReason"></param>
        public static void SendMailToNextApprover(int approverID, int senderID, string sReason,string sComment, string sTitle="")
        {
            string emailApproveDocTitle = (sTitle == "" || sTitle.Length == 0) ? Resources.Resource.emailApproveDocTitle : string.Format(Resources.Resource.emailApproveDocTitle," - " + "[" + sTitle + "]"); // Jasodn 2016-04-15

            try
            {
                User _approver = new FEA_BusinessLogic.UserManager().GetItem(approverID);
                User _sender = new FEA_BusinessLogic.UserManager().GetItem(senderID);
                string _content = string.Format(Resources.Resource.emailApproveDocBody, _sender.UserName, _sender.UserNameEN, _sender.CostCenter.Remark, sReason, Helper.Ultilities.SiteAddress,_approver.UserName,sComment);
                FEA_Ultil.FEASendMail.SendMailMessage(_approver.UserEmail, "", "", emailApproveDocTitle, _content);


                // Check for approval delegation , if there have approval delegation, system will sent email to delegation
                WFDelegate wfde = new FEA_BusinessLogic.WFDelegateManager().GetDelegateByDelegator(approverID);
                if (wfde != null)
                {
                    User _DelegateUser = wfde.User;
                    if(_DelegateUser != null)
                    {
                        _content = string.Format(Resources.Resource.emailApproveDocBody, _sender.UserName, _sender.UserNameEN, _sender.CostCenter.Remark, sReason, Helper.Ultilities.SiteAddress, _DelegateUser.UserName, sComment);
                        FEA_Ultil.FEASendMail.SendMailMessage(_DelegateUser.UserEmail, "", "", emailApproveDocTitle + " - Authorised by " + _approver.UserName, _content);

                    }
                }

            }
            catch { }
        }

        public static void SendMailToITServices(int senderID, string sReason,string sComment)
        {
            try
            {
                User _sender = new FEA_BusinessLogic.UserManager().GetItem(senderID);
                string _mail = Helper.Ultilities.ITSServiceMail;
                string _content = string.Format(Resources.Resource.emailApproveDocBody, _sender.UserName, _sender.UserNameEN, _sender.CostCenter.Remark, sReason, Helper.Ultilities.SiteAddress, " Administrator", sComment);
                FEA_Ultil.FEASendMail.SendMailMessage(_mail, "", "", "Document(s) need to approved", _content);
            }
            catch { }
        }
        public static void SendMailToSEServices(int senderID, string sReason, string sComment)
        {
            try
            {
                User _sender = new FEA_BusinessLogic.UserManager().GetItem(senderID);
                string _mail = Helper.Ultilities.ITSecurityService;
                string _content = string.Format(Resources.Resource.emailApproveDocBody, _sender.UserName, _sender.UserNameEN, _sender.CostCenter.Remark, sReason, Helper.Ultilities.SiteAddress, " Administrator", sComment);
                FEA_Ultil.FEASendMail.SendMailMessage(_mail, "", "", "Document(s) need to approved", _content);
            }
            catch { }
        }
        public static void SendMailToGAServices(int senderID, string sReason, string sComment)
        {
            try
            {
                User _sender = new FEA_BusinessLogic.UserManager().GetItem(senderID);
                string _mail = Helper.Ultilities.GAMailService;
                string _content = string.Format(Resources.Resource.emailApproveDocBody, _sender.UserName, _sender.UserNameEN, _sender.CostCenter.Remark, sReason, Helper.Ultilities.SiteAddress, " Administrator", sComment);
                FEA_Ultil.FEASendMail.SendMailMessage(_mail, "", "", "Document(s) need to approved", _content);
            }
            catch { }
        }
        //Added by Tony (2017-02-09)
        public static void SendMailToERPCreator(int senderID, string sReason, string sComment)
        {
            try
            {
                User _sender= new FEA_BusinessLogic.UserManager().GetItem(senderID);
                string _mail=_sender.UserEmail;
                string _content =string.Format(Resources.Resource.erpMailBody,_sender.UserName,_sender.UserNameEN,_sender.CostCenter.Remark,sReason,Helper.Ultilities.SiteAddress,_sender.UserName,sComment);
                FEA_Ultil.FEASendMail.SendMailMessage(_mail,"","","Document(s) need to check",_content);
            }
            catch {}
        }
        //
        #endregion

        #region "References"
        public static void SendMailToReferences(int[]receiverIDs, int senderID,string comment)
        {
            try
            {
                User _sender = new FEA_BusinessLogic.UserManager().GetItem(senderID);
                List<User> lstReceiver = new FEA_BusinessLogic.Base.Connection().db.Users.Where(i => receiverIDs.Contains(i.UserID)).ToList();

                List<string> lstEmail = lstReceiver.Select(i => i.UserEmail).ToList();

                string _content = string.Format(Resources.Resource.emailReferences, "!", _sender.UserName, Helper.Ultilities.SiteAddress, comment);
                //string _content =Resources.Resource.emailReferences;
                FEA_Ultil.FEASendMail.SendMailMessage(string.Join(",",lstEmail.ToArray()), "", "", "References email", _content);
            }
            catch { }
        }
        #endregion
    }
}
