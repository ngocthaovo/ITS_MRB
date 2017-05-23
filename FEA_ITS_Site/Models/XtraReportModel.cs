using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Models
{
    public class XtraReportModel
    {
    }
    public class ReportDesignerDemoModel : ReportsModel
    {
        public string RedirectUrl { get; set; }
    }
    public class ReportsModel
    {
        public string ReportID { get; set; }
        public object Parameter { get; set; }
        public object Temp1 { get; set; }
        public object Temp2 { get; set; }
        public object Temp3 { get; set; }

        public XtraReport Report { get; set; }
    }
}