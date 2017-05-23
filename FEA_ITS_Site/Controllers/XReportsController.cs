using DevExpress.Web.Mvc;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using FEA_ITS_Site.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FEA_ITS_Site.Controllers
{
    public class XReportsController : BaseController
    {
        //
        // GET: /XReports/
        /// <summary>
        /// Stock out Report
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult StockOutReport(string ID)
        {
            ViewBag.ID = ID;
            return View();
        }

        /// <summary>
        /// Sa Are Report
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public ActionResult SAApplicationReport(string ID, int Type)
        {
            ViewBag.ID = ID;
            ViewBag.Type = Type;
            return View();
        }


        /// <summary>
        ///  Common Function
        /// </summary>
        /// <param name="param"></param>
        /// <param name="reportID"></param>
        /// <returns></returns>
        public ActionResult ReportPartial(object param, string reportID, string Temp1 = "", string Temp2="", string Temp3="")
        {
            Models.ReportsModel reportModel = new ReportsModel();
            reportModel.ReportID = reportID;
            reportModel.Parameter = param;

            reportModel.Temp1 = Temp1==null?"":Temp1;
            reportModel.Temp2 = Temp2 == null ? "" : Temp2;
            reportModel.Temp3 = Temp3 == null ? "" : Temp3;

            switch(reportID)
            {
                case"StockOutReport":
                    reportModel.Report = new Reports.StockOutReport(param.ToString());
                    break;
                case "SaApplicationReport":
                    // Temp 1: is OrderType
                    if(int.Parse(Temp1) == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative)
                        reportModel.Report = new Reports.Application_Donative(reportModel.Parameter.ToString());
                    else
                        reportModel.Report = new Reports.Application(reportModel.Parameter.ToString(), int.Parse(reportModel.Temp1.ToString()));

                    break;
                default:
                    break;
            }

            return PartialView("TableReportViewerPartial", reportModel);
        }

        public ActionResult ReportExportTo(object param, string reportID, string Temp1 = "", string Temp2 = "", string Temp3 = "")
        {
            XtraReport Report = new XtraReport();
            switch (reportID)
            {
                case "StockOutReport":
                    Report = new Reports.StockOutReport(param.ToString());
                    break;
                case "SaApplicationReport":
                    if (int.Parse(Temp1) == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Donative)
                        Report = new Reports.Application_Donative(param.ToString());
                    else
                        Report = new Reports.Application(param.ToString(), int.Parse(Temp1.ToString()));
                    break;
                default:
                    break;
            }

            return DocumentViewerExtension.ExportTo(Report);
        }

    }
}
