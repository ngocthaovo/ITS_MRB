using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace FEA_ITS_Site.Helper
{
    public class SessionManager
    {
        protected HttpSessionState session;
        public SessionManager(HttpSessionState httpSessionState)
        {
            session = httpSessionState;
        }

        public static string CurrentLang
        { get { return Thread.CurrentThread.CurrentUICulture.Name; } }

        public static int CurrentCulture
        {
            get
            {
                if (Thread.CurrentThread.CurrentUICulture.Name == FEA_Ultil.FEALanguage.LangCode_VN)
                    return 0;
                else if (Thread.CurrentThread.CurrentUICulture.Name == FEA_Ultil.FEALanguage.LangCode_EN)
                    return 1;
                else if (Thread.CurrentThread.CurrentUICulture.Name == FEA_Ultil.FEALanguage.LangCode_CN)
                    return 2;
                else
                    return 1;
            }
            set
            {
                if (value == 0)
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(FEA_Ultil.FEALanguage.LangCode_VN);
                else if (value == 1)
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(FEA_Ultil.FEALanguage.LangCode_EN);
                else if (value == 2)
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(FEA_Ultil.FEALanguage.LangCode_CN);
                else
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            }
        }
         
    }
}