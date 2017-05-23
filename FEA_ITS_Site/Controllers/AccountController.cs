using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
using DevExpress.Web.Mvc;
namespace FEA_ITS_Site.Controllers
{
    public class AccountController : BaseController
    {
        //
        // GET: /Account/
        /// <summary>
        /// Get Infomation of User
        /// </summary>
        /// <returns></returns>
        public ActionResult AccountInfo()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/Account/AccountInfo") });
            //


            FEA_BusinessLogic.User o = new FEA_BusinessLogic.UserManager().GetItem(Helper.UserLoginInfo.UserId);

            //List Deparment
            ViewBag.CodeCenters = new SelectList(new FEA_BusinessLogic.CodeCenterManager().GetItems(),
                                    "CostCenterCode", "Remark");

            ViewBag.UserPosition = new SelectList(new FEA_BusinessLogic.UserPositionManager().GetItems(),
                                     "PositionID", "PositionName");
            return View(o);
        }


        /// <summary>
        /// Update User Infomation
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AccountInfo(User o)
        {
            //List Deparment
            ViewBag.CodeCenters = new SelectList(new FEA_BusinessLogic.CodeCenterManager().GetItems(),
                                    "CostCenterCode", "Remark");
            ViewBag.UserPosition = new SelectList(new FEA_BusinessLogic.UserPositionManager().GetItems(),
                        "PositionID", "PositionName");
            ViewBag.UpdateStatus = false;
            try
            {
                bool bIsvalidEmail = FEA_Ultil.FEAEmail.CheckValidateEmail(o.UserEmail);
                if (!bIsvalidEmail)
                {
                    ViewBag.UpdateStatus = false;
                    ViewBag.UpdateInfo = string.Format(Resources.Resource.msgEmailExtInvalid, FEA_Ultil.FEAEmail.EmailExt);
                }
                else
                {
                    bool flag = new UserManager().UpdateItem(o, u => u.UserCodeID,
                                    u => u.UserName,
                                    u => u.UserNameEN,
                                    u => u.UserEmail,
                                    u => u.UserPhone,
                                    u => u.UserSex,
                                    u => u.CostCenterCode,
                                    u => u.UserPosstion,
                                    u => u.UserStartDate);

                    ViewBag.UpdateStatus = flag;
                    switch (flag)
                    {
                        case false:
                            ViewBag.UpdateInfo = @Resources.Resource.msgUpdateFailed;
                            break;
                        default:
                            ViewBag.UpdateInfo = @Resources.Resource.msgUpdateSuccess;
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                ViewBag.UpdateStatus = false;
                ViewBag.UpdateInfo = ex.Message;
            }
            o = new FEA_BusinessLogic.UserManager().GetItem(o.UserCodeID);
            return View(o);
            //return RedirectToAction("AccountInfo");
        }

        /// <summary>
        /// Member Change password
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangePass(FormCollection form)
        {

            if (Helper.UserLoginInfo.IsLogin) // If user was Login, get the UserCode from sesssion
            {
                string sUserCode = "", sOldPass, sNewPass;
                sOldPass = form["change_pass_current_pass"].ToString();
                sNewPass = form["change_pass_new_pass"].ToString();
                sUserCode = Helper.UserLoginInfo.UserCode;
                User o = new UserManager().GetItem(sUserCode);
                //List Deparment
                ViewBag.CodeCenters = new SelectList(new FEA_BusinessLogic.CodeCenterManager().GetItems(),
                                        "CostCenterCode", "Remark");
                ViewBag.UserPosition = new SelectList(new FEA_BusinessLogic.UserPositionManager().GetItems(),
                                        "PositionID", "PositionName");

                if (sOldPass.Trim() == sNewPass.Trim())
                {
                    ViewBag.UpdateStatus = false;
                    ViewBag.UpdateInfo = @Resources.Resource.msgSameOldPassword;
                    return View("AccountInfo", o);
                }

                bool flag = new FEA_BusinessLogic.UserManager().ChangePass(sUserCode, sOldPass, sNewPass);
                if (flag)
                    ViewBag.UpdateInfo = Resources.Resource.msgChangePassSuccess;
                else
                    ViewBag.UpdateInfo = Resources.Resource.msgLoginWrong;
                ViewBag.UpdateStatus = flag;
                return View("AccountInfo", o);
            }
            else
            {
                return RedirectToAction("Login", "User");
            }
        }


        public ActionResult AccountManager()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/Account/AccountManager") });
            //

            return View();
        }

        public ActionResult EditModesPartial()
        {
            List<User> items = new UserManager().GetItems("", "",1);
            return GetGridView(items, "account_list_grid");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] User obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.UserName == null || obj.UserNameEN == null || obj.UserExpired == null || obj.UserPhone == null)
                        ViewData["EditError"] = Resources.Resource.msgInputError;
                    else
                    {

                        bool flag = new FEA_BusinessLogic.UserManager().UpdateItem(obj, o => o.UserName, o => o.UserNameEN, o => o.CostCenterCode,
                                                                             o => o.UserPosstion, o => o.UserGroupID, o => o.UserEmail,
                                                                             o => o.UserPhone, o => o.UserExpired);
                        if (flag)
                        {
                            ViewData["EditSuccess"] = Resources.Resource.msgUpdateSuccess;
                        }
                        else
                            ViewData["EditError"] = Resources.Resource.msgUpdateFailed;
                    }
                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;

            List<User> items = new UserManager().GetItems("", "");
            return GetGridView(items, "account_list_grid");
        }

        [HttpPost]
        public ActionResult ResetUserPass(string UserList)
        {
            BaseJsonResult result = new BaseJsonResult();
            if (UserList != "" || UserList.Length > 0)
            {
                string[] sUsers = UserList.Split(';');
                if (sUsers.Length > 0)
                {
                    try
                    {
                        string newPass = FEA_Ultil.FEAStringClass.RandomString(10);
                        User u = new UserManager().GetItem(sUsers[0]);


                        User o = new FEA_BusinessLogic.User() { UserCodeID = sUsers[0], UserPass = newPass };
                        bool flag = new UserManager().UpdateItem(o, i => i.UserPass);
                        if (flag)
                        {
                            try
                            {
                                FEA_Ultil.FEASendMail.SendMailMessage(u.UserEmail, "", "", Resources.Resource.ChangePass,
                                                                  string.Format(Resources.Resource.emailResSuccessBody, sUsers[0], newPass, FEA_ITS_Site.Helper.Ultilities.SiteAddress,u.UserName));
                            }
                            catch (Exception ex) { }

                            result.ErrorCode = 0;
                            result.Message = Resources.Resource.msgUpdateSuccess;
                            result.ObjectResult = string.Format(Resources.Resource.msgChangePassAccountInfo, u.UserName, newPass);
                        }

                        else
                        {
                            result.ErrorCode = 1;
                            result.Message = Resources.Resource.msgUpdateFailed;
                            result.ObjectResult = Resources.Resource.msgUpdateFailed;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCode = 1;
                        result.Message = Resources.Resource.msgUpdateFailed;
                        result.ObjectResult = ex.Message;
                    }
                }
            }
            else
            {
                result.ErrorCode = 1;
                result.Message = Resources.Resource.msgUpdateFailed;
                result.ObjectResult = Resources.Resource.msgInputError;
            }
            return Json(result);
        }
    }
}
