using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FEA_BusinessLogic;
using System.Configuration;
namespace FEA_ITS_Site.Helper
{
    public class Ultilities
    {
        /// <summary>
        /// 
        /// </summary>
        public static string Root { get { return ConfigurationManager.AppSettings["RootValue"].ToString(); } }

        public static string UploadFolder { get { return ConfigurationManager.AppSettings["UploadFolder"].ToString(); } }

        public static string SAUploadFolder { get { return ConfigurationManager.AppSettings["SAUploadFolder"].ToString(); } }

        public static string WarehouseAreaUploadFile { get { return ConfigurationManager.AppSettings["WarehouseAreaUploadFolder"].ToString(); } }
        /// <summary>
        /// Number of Record in Grid
        /// </summary>
        public static int PageSize { get { return ConfigurationManager.AppSettings["PageSize"] == null ? 10 : int.Parse(ConfigurationManager.AppSettings["PageSize"].ToString()); } }


        /// <summary>
        /// web Site Address
        /// </summary>
        public static string SiteAddress { get { return ConfigurationManager.AppSettings["SiteLink"] == null ? "" : ConfigurationManager.AppSettings["SiteLink"].ToString(); } }


        public static string ITSServiceMail { get { return ConfigurationManager.AppSettings["ITSMailService"] == null ? "ITServices@feavn.com.vn" : ConfigurationManager.AppSettings["ITSMailService"].ToString(); } }

        public static string ITSecurityService { get { return ConfigurationManager.AppSettings["ITSecurityService"] == null ? "security@feavn.com.vn" : ConfigurationManager.AppSettings["ITSecurityService"].ToString(); } }
        public static string GAMailService { get { return ConfigurationManager.AppSettings["GAMailService"] == null ? "nguyen.tran@feavn.com.vn" : ConfigurationManager.AppSettings["GAMailService"].ToString(); } }
        //Added by Tony (2017-06-26)
        public static string MNMailService { get { return ConfigurationManager.AppSettings["MNMailService"] == null ? "bao.do@feavn.com.vn" : ConfigurationManager.AppSettings["MNMailService"].ToString(); } }

        /// <summary>
        /// Get Image avatar by Gender
        /// </summary>
        /// <param name="gender"></param>
        /// <returns></returns>
        public static string GetUserAvatar(string gender)
        {
            try
            {
                string avatar = Root;
                if (gender.ToLower() == "m")
                    avatar += ConfigurationManager.AppSettings["UserAavatar_Male"].ToString();
                else
                    avatar += ConfigurationManager.AppSettings["UserAavatar_FeMale"].ToString();

                return avatar;
            }
            catch { return ""; }
        }


        /// <summary>
        /// Load Left Menu
        /// </summary>
        /// <param name="parentid"></param>
        /// <param name="level"></param>
        /// <param name="lstMenu"></param>
        /// <returns></returns>
        public static string loadmenu(int parentid, int level, List<FEA_BusinessLogic.SiteFunction> lstMenu)
        {
            string result = string.Empty;
            List<FEA_BusinessLogic.SiteFunction> lstChildMenu = lstMenu.OrderBy(i=>i.Order).Where(i => i.ParentID == parentid).ToList();
            if (lstChildMenu.Count ==0)
            {
                return result;
            }
            else
            {
                foreach(SiteFunction item in lstChildMenu)
                {
                    int countChild = lstMenu.Where(i => i.ParentID == item.SiteFunctionID).Count();
                    string smenuName = (Helper.SessionManager.CurrentLang == FEA_Ultil.FEALanguage.LangCode_VN) ? item.SiteFunctionName : (Helper.SessionManager.CurrentLang == FEA_Ultil.FEALanguage.LangCode_CN?item.SiteFunctionNameCN:item.SiteFunctionNameEN);
                    if(item.ParentID ==0)
                    { 
                    result += "<li id='mn_"+item.SiteFunctionID+"'>";
                    result += " <a href='" + ((item.URL.Length > 0) ? Root + item.URL :"")+ "' class='active'>";
                    result += "     <i class='"+item.IconCssClass+"'></i>";
                    result += "     <span class='title'>" + smenuName + " </span>" + (countChild > 0 ? "<i class='icon-arrow'></i>" : "");
                    result += "     <span class='selected'></span>";
                    result += " </a>";
                    result += countChild >0? "<ul class='sub-menu'>" + loadmenu(item.SiteFunctionID, level + 1, lstMenu) + "</ul>":"";
                    result += "</li>";
                    }
                    else
                    {
                        result += "<li id='mn_" + item.SiteFunctionID + "'>";
                        result += " <a href='" + ((item.URL.Length > 0) ? Root + item.URL : "") + "' class='active'>" + smenuName;
                        result += "     <i class='" + item.IconCssClass + "'></i>";
                        result += (countChild > 0 ? "<i class='icon-arrow'></i>" : "");
                        result += " </a>";
                        result += countChild > 0 ? "<ul class='sub-menu'>" + loadmenu(item.SiteFunctionID, level + 1, lstMenu) + "</ul>" : "";
                        result += "</li>";
                    }
                }
            }
            return result;
        }

    }
}