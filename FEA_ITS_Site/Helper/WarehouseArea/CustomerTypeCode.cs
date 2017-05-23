using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FEA_BusinessLogic.WarehouseArea;
namespace WarehouseArea.ExcelReader
{
    public class CustomerTypeCode
    {
        //public static string Columbia = "060";
        //public static string Fila = "078";
        //public static string Nike = "C34";
        // ,Fila ="078",Nike="C34", UnderAmour="UA7"
        public static Dictionary<string, string> CustomerTypeCodeList()
        {
            Dictionary<string, string> lst = new Dictionary<string, string>();
            lst.Add(PackingManifestManager.CustomerCodeType.Columbia, string.Format("Columbia({0})", PackingManifestManager.CustomerCodeType.Columbia));
            lst.Add(PackingManifestManager.CustomerCodeType.Fila, string.Format("Fila({0})", PackingManifestManager.CustomerCodeType.Fila));
            lst.Add(PackingManifestManager.CustomerCodeType.Nike, string.Format("Nike({0})", PackingManifestManager.CustomerCodeType.Nike));
            lst.Add(PackingManifestManager.CustomerCodeType.UnderAmour, string.Format("UnderAmour({0})", PackingManifestManager.CustomerCodeType.UnderAmour));
            return lst;

        }
    }
}