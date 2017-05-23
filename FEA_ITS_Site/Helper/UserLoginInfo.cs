using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FEA_ITS_Site.Helper
{
    public class UserLoginInfo
    {
        public static bool IsLogin
        {
            get
            {
                if (HttpContext.Current.Session["login"] == null)
                {
                    return false;
                }
                else
                {
                    if (HttpContext.Current.Session["login"] is bool)
                        return Convert.ToBoolean(HttpContext.Current.Session["login"]);
                    return false;
                }
            }
        }
        public static string Email
        {
            get{return HttpContext.Current.Session["useremail"].ToString();}
        }

        public static int UserId
        {
            get{return int.Parse(HttpContext.Current.Session["userid"].ToString());}
        }
        public static string UserCode
        {
            get { return HttpContext.Current.Session["usercode"].ToString(); }
        }

        public static string UserName
        {
            get { return HttpContext.Current.Session["username"].ToString(); }
        }
        public static int UserGroup
        {
            get { return int.Parse (HttpContext.Current.Session["usergroup"].ToString()); }
        }

        public static FEA_BusinessLogic.User CurrentUser {
            get { return (FEA_BusinessLogic.User)HttpContext.Current.Session["current_user"]; }
        }

        public static string UserPosition
        {
            get { return HttpContext.Current.Session["userPosition"].ToString(); }
        }

        public static void SetLoginInfo(FEA_BusinessLogic.User u)
        {

            HttpContext.Current.Session["current_user"] = u;

            HttpContext.Current.Session["userid"] = u.UserID;
            HttpContext.Current.Session["usercode"] = u.UserCodeID;
            HttpContext.Current.Session["useremail"] = u.UserEmail;
            HttpContext.Current.Session["login"] = true;
            HttpContext.Current.Session["username"] = u.UserName;
            HttpContext.Current.Session["usernameen"] = u.UserNameEN;
            HttpContext.Current.Session["usergroup"] = u.UserGroupID;
            HttpContext.Current.Session["userPosition"] = u.UserPosstion;

        }

        public static void ClearLoginInfo()
        {
            HttpContext.Current.Session["current_user"] = null;
            HttpContext.Current.Session["userid"] = -1;
            HttpContext.Current.Session["usercode"] = "";
            HttpContext.Current.Session["useremail"] = "";
            HttpContext.Current.Session["login"] = false;
            HttpContext.Current.Session["username"] = "";
            HttpContext.Current.Session["usernameen"] = "";
            HttpContext.Current.Session["usergroup"] = -1;
            HttpContext.Current.Session["userPosition"] = "";
        }
    }
}
