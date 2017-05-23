using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
using System.Configuration;
namespace FEA_ITS_Site.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /User/
        [HttpGet]
        public ActionResult Login(string url, string servername) 
        {
            Helper.ServerManager.InitServer(servername); // Thiet lap tạm 

            if (Helper.UserLoginInfo.IsLogin)
                if (url == null)
                    return RedirectToAction("Index", "Home");
                else
                    return Redirect(url);


            ViewBag.ServerList = new SelectList(Helper.ServerManager.GetConnections(), "Key","Key", Helper.ServerManager.CurrentServerName);
            ViewBag.CodeCenters = new SelectList(new FEA_BusinessLogic.CodeCenterManager().GetItems(1),
                                                    "CostCenterCode", "Remark");
            ViewBag.UserPosition = new SelectList(new FEA_BusinessLogic.UserPositionManager().GetItems(),
            "PositionID", "PositionName");

            return View();
        }


        /// <summary>
        /// User login to System
        /// </summary>
        /// <param name="form"></param>
        /// <returns>-1: password Expired,0: login false; 1: login success</returns>
        [HttpPost]
        public JsonResult Login(FormCollection form)
        {
            try
            {
                string sUserCode, sUserPass;
                sUserCode = form["username"].ToString();
                sUserPass = form["password"].ToString();

                //Check password must be change
                if (new FEA_BusinessLogic.UserManager().UserPassExpired(sUserCode,sUserPass))
                    return Json(new BaseJsonResult(-1, Resources.Resource.msgPasswordExpired));


                User u = new FEA_BusinessLogic.UserManager().LogIn(sUserCode, sUserPass);
                if (u != null)//Login success
                {

                    //Make userlogin Info
                    FEA_ITS_Site.Helper.UserLoginInfo.SetLoginInfo(u);
                    return Json(new BaseJsonResult(1,"Success"));
                }
                else
                {
                    return Json(new BaseJsonResult(0, ""));
                } 
            }
            catch (Exception ex)
            {

                return Json(new BaseJsonResult(ex));
            }
          
        }

        /// <summary>
        /// Lout out from System
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            Helper.UserLoginInfo.ClearLoginInfo();
            return RedirectToAction("Login", "User");
        }

        #region "User Registration"
        /// <summary>
        /// User Registration function
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UserRegistration(FormCollection form)
        {
            try
            {
                string sUserCode, sUserName, sUserName_EN, sSex, sEmail, sPhone;
                int iCenterCode,iUserGroup ;
                bool isDelegate = false;
                DateTime dtStartUpDate;
                Guid sUserPos;

                sUserCode = form["res_employee_code"].ToString();
                sUserName = form["res_employee_name"].ToString();
                sUserName_EN = form["res_employee_english_name"].ToString();
                sSex = form["gender"].ToString();
                sEmail = form["res_email"].ToString();
                sPhone = form["res_phone"].ToString();
                iCenterCode = int.Parse(form["cboCodeCenter"].ToString());
                sUserPos = new Guid( form["cboUserPosition"].ToString());
                iUserGroup = ConfigurationManager.AppSettings["GroupDefaultWhenUserRes"] != null ? int.Parse(ConfigurationManager.AppSettings["GroupDefaultWhenUserRes"].ToString()) : 1;
                if (!DateTime.TryParse(form["res_date_working"], out dtStartUpDate))
                { dtStartUpDate = DateTime.Now; }

                //Check Email Validate
                bool bIsvalidEmail = FEA_Ultil.FEAEmail.CheckValidateEmail(sEmail);
                if(!bIsvalidEmail)
                    return  Json(new BaseJsonResult(-1,Resources.Resource.msgEmailExtInvalid+FEA_Ultil.FEAEmail.EmailExt ));
                else
                {
                    var checkbox = Request.Form["res_delegate"];
                    if (checkbox != null && checkbox == "on")
                        isDelegate = true;
                }

                //Check user exist
                bool IsExisted = new FEA_BusinessLogic.UserManager().CheckUserExists(sUserCode);
                if (IsExisted)
                    return Json(new BaseJsonResult(-1, Resources.Resource.msgUserExist));

                // Insert User to database
                User o = new User()
                {
                    UserCodeID = sUserCode,UserName = sUserName,UserNameEN = sUserName_EN,
                    UserSex = sSex,UserEmail =isDelegate ? "": sEmail, UserPhone = sPhone,
                    CostCenterCode = iCenterCode,UserStartDate = dtStartUpDate,
                    UserGroupID = iUserGroup,UserExpired = DateTime.Now.AddMonths(3),
                    UserPosstion= sUserPos
                };

                int result = new FEA_BusinessLogic.UserManager().InsertItem(o);
                if (result > 0)
                {
                    try
                    {
                        FEA_Ultil.FEASendMail.SendMailMessage(sEmail, "", "", Resources.Resource.emailResSuccessTitle,
                                                          string.Format(Resources.Resource.emailResSuccessBody, sUserCode, o.UserPass, Helper.Ultilities.SiteAddress,o.UserName));
                    }
                    catch (Exception ex) { }
                    return Json(new BaseJsonResult(0, Resources.Resource.msgResSuccess));
                }         
                else
                    return Json(new BaseJsonResult(-1,"Cannot Registration"));
            }
            catch (Exception ex)
            {
                return Json(new BaseJsonResult(ex));

            }
        }

        /// <summary>
        /// Check for user existing in database? 
        /// </summary>
        /// <param name="sUserCode"></param>
        /// <returns>0: not Exist, 1:Exist</returns>
        [HttpPost]
        public JsonResult CheckUserExists(string sUserCode)
        {
            bool IsExisted= new FEA_BusinessLogic.UserManager().CheckUserExists(sUserCode);
            if (IsExisted)
                return Json(new BaseJsonResult(-1, Resources.Resource.msgUserExist));
            return Json(new  BaseJsonResult(0, ""));
        }
        #endregion
        
        /// <summary>
        /// User change password
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UserChangePass(FormCollection form)
        {
            string sUserCode, sOldPass, sNewPass;
            sUserCode = form["change_pass_employeeid"].ToString();
            sOldPass = form["change_pass_current_pass"].ToString();
            sNewPass = form["change_pass_new_pass"].ToString();

            if (Helper.UserLoginInfo.IsLogin) // If user was Login, get the UserCode from sesssion
                sUserCode = Helper.UserLoginInfo.UserCode;

            bool flag = new FEA_BusinessLogic.UserManager().ChangePass(sUserCode, sOldPass, sNewPass);
            if(flag)
                return Json(new BaseJsonResult(1,"Success"));
            return  Json(new BaseJsonResult(0,"not success"));
        }

    }
}
