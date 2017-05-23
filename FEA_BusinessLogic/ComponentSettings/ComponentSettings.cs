using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Objects;

namespace FEA_BusinessLogic.ComponentSettings
{
    public class ComponentSettings:Base.Connection
    {
        public List<ComponentDesign> getComponent(string DocType)
        {
            List<ComponentDesign> x = db.ComponentDesigns.Where(i => i.CD_DocType == DocType).ToList();
            return x;
        }
        public static string GetCDFiledName(string cdName, List<ComponentDesign> component)
        {
            return component.Where(i => i.CD_Name == cdName).SingleOrDefault().CD_FiledName;
        }
        public static string GetCDCaption(string currentLanguage, string cdName, List<ComponentDesign> component)
        {
            return (currentLanguage == FEA_Ultil.FEALanguage.LangCode_VN) ? component.Where(i => i.CD_Name == cdName).SingleOrDefault().CD_Caption :
                (currentLanguage == FEA_Ultil.FEALanguage.LangCode_CN ? component.Where(i => i.CD_Name == cdName).SingleOrDefault().CD_CaptionCN : component.Where(i => i.CD_Name == cdName).SingleOrDefault().CD_CaptionEN);

        }
        public static bool GetCDisVisiable(string cdName, List<ComponentDesign> component)
        {
            return component.Where(i => i.CD_Name == cdName).SingleOrDefault().CD_isVisiable == 1 ? true : false;
        }
        public static int GetCDIndex(string cdName, List<ComponentDesign> component)
        {
            return Convert.ToInt16(component.Where(i => i.CD_Name == cdName).SingleOrDefault().CD_Index);
        }
        public static double GetCDWidth(string cdName, List<ComponentDesign> component)
        {
            return Convert.ToDouble(component.Where(i => i.CD_Name == cdName).SingleOrDefault().CD_Width);
        }
    }
}
