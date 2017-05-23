using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_Ultil
{
  public class FEAEmail
    {
        public static string EmailExt = "feavn.com.vn";
        public static string FENVEmailExt = "fenv.com.vn";

        public static bool CheckValidateEmail(string sEmail)
        {
            string[] arr = sEmail.Split('@');
            if(arr.Length >=2)
            {
                if (arr[1] == EmailExt || arr[1] == FENVEmailExt)
                    return true;
            }
            return false;
        }
    }
}
