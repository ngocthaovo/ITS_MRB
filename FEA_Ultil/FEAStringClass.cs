using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace FEA_Ultil
{
    public class FEAStringClass
    {

        /// <summary>
        /// Encode to MD5
        /// </summary>
        /// <param name="sSource"></param>
        /// <returns></returns>
        public static string EnCodeMD5(string sSource)
        {
            return BitConverter.ToString(encryptData(sSource)).Replace("-", "").ToLower();
        }

        public static string DisNumberFormatString(int? numDecimal=null)
        {
           // return "n" + (numDecimal==null?"":numDecimal.ToString());

            return "";
        }
        public static byte[] encryptData(string data)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5Hasher = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashedBytes;
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            hashedBytes = md5Hasher.ComputeHash(encoder.GetBytes(data));
            return hashedBytes;
        }

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Format date
        /// </summary>
        /// <param name="sLang"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string FormatDateString(string sLang, object dt, bool?hasTime=false)
        {
            if (dt == null)
                return "";

            DateTime dtConvert;
            bool status = DateTime.TryParse(dt.ToString(), out dtConvert);
            if (status)
            {
                if (sLang == FEALanguage.LangCode_VN)
                    if(hasTime.Value)
                        return dtConvert.ToString("dd-MM-yyyy hh:mm tt");
                    else
                        return dtConvert.ToString("dd-MM-yyyy");
                else if (sLang == FEALanguage.LangCode_EN)
                    if(hasTime.Value)
                        return dtConvert.ToString("yyyy-MM-dd hh:mm tt");
                    else
                        return dtConvert.ToString("yyyy-MM-dd");
            }
            return "";
        }

        public static string DataDateFormat(string sLang, bool? fromJavascript = true, bool?hasTime=false)
        {
            if (fromJavascript.Value)
            {
                if (sLang == FEALanguage.LangCode_VN)
                    return hasTime.Value ? "dd-mm-yyyy hh:mm tt" : "dd-mm-yyyy";
                else if (sLang == FEALanguage.LangCode_EN)
                    return hasTime.Value ? "yyyy-mm-dd hh:mm tt" : "yyyy-mm-dd";
                return hasTime.Value ? "yyyy-mm-dd hh:mm tt" : "yyyy-mm-dd";
            }
            else
            {
                if (sLang == FEALanguage.LangCode_VN)
                    return hasTime.Value ? "dd-MM-yyyy hh:mm tt" : "dd-MM-yyyy";
                else if (sLang == FEALanguage.LangCode_EN)
                    return hasTime.Value ? "yyyy-MM-dd hh:mm tt" : "yyyy-MM-dd";
                return hasTime.Value ? "yyyy-MM-dd hh:mm tt" : "yyyy-MM-dd";
            }
        }


        public static string formatPrice(object Price, object Currency)
        {
            string sPrice = Price.ToString();
            if (sPrice == "" || sPrice == "0") return "Call";

            return formatNumber(Price) + Currency;
        }
        public static string formatNumber(object Number)
        {
            if (Number == null ||Number.ToString() == "") return "0";


            string[] array;
            if(Number.ToString().Contains("."))
                array = Number.ToString().Split('.');
            else if (Number.ToString().Contains(","))
                array = Number.ToString().Split(',');
            else
                array = Number.ToString().Split('.');

            string sPrice = array[0];
            
            string str = "";
            for (int i = sPrice.Length; i >= 3; i -= 3)
            {
                str = (i == 3 ? "" : ",") + sPrice.Substring(i - 3 < 0 ? 0 : i - 3, 3) + str;
            }
            str = sPrice.Substring(0, sPrice.Length % 3) + str;
            return str + (array.Count() <=1 ?" ":"."+array[1]);
        }
    }

}
