using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Helper
{
    public class ServerManager
    {
        public static string DefaultServername = ConfigurationManager.AppSettings["DefaultServer"] == null ? "FEAServer-Far Eastern Apparel" : ConfigurationManager.AppSettings["DefaultServer"].ToString();
        public static string CurrentServerName
        { 
            get
            {
                if (HttpContext.Current.Session["CurrentServerName"] == null)
                {
                    return "";
                }
                else
                {
                        return HttpContext.Current.Session["CurrentServerName"].ToString();
                }
            }
        }

        public static void InitServer(string servername)
        {
            if (servername == null || servername == "")
            {
                if (CurrentServerName != "")
                    ChangeServer(CurrentServerName);
                else
                    ChangeServer(DefaultServername);
            }
            else
            {
                ChangeServer(servername);
            }
        }

        public static Dictionary<string, string> GetConnections()
        {
            Dictionary<string, string> lst = new Dictionary<string, string>();
            foreach (ConnectionStringSettings c in System.Configuration.ConfigurationManager.ConnectionStrings)
            {
                if (c.Name.ToUpper().Contains("FEASERVER"))
                    lst.Add(c.Name, c.ConnectionString);
            }
            return lst;
        }

        public static String GetConncectionStringByName(string sName)
        {
            return Helper.ServerManager.GetConnections()[sName];
        }

        public static void ChangeServer(string Servername)
        {
            
            try
            {
                FEA_BusinessLogic.Base.Connection.SetConnectionString(GetConncectionStringByName(Servername));
                HttpContext.Current.Session["CurrentServerName"] = Servername;
            }
            catch
            {
                FEA_BusinessLogic.Base.Connection.SetConnectionString(GetConncectionStringByName(DefaultServername));
                HttpContext.Current.Session["CurrentServerName"] = DefaultServername;
            }
        }
    }
}