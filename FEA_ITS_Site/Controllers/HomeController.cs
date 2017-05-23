using FEA_ITS_Site.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FEA_ITS_Site.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User");
            return View();
        }

        public ActionResult Demo()
        {
            
            return View();
        }
        /// <summary>
        /// Change the Language
        /// </summary>
        /// <param name="id">0: Vietnam, 1: England</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ChangeCurrentCulture(int id=0)
        {
            SessionManager.CurrentCulture = id;
            Session["CurrentCulture"] = id;
            return RedirectToAction("Login", "User");
        }
    }
}
