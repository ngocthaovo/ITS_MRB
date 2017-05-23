using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using DevExpress.XtraPivotGrid;

namespace FEA_ITS_Site.Controllers
{
    public class StatisticController : BaseController
    {
        //
        // GET: /Statistic/

        public ActionResult Index()
        {
            if (!IsPostback)
            {
                Session["Result"] = null;
            }
            return View(Session["Result"] as Object);

        }
        [HttpPost]
        public ActionResult Index(FormCollection frm)
        {
            string DateTo;
            string DateFrom;
            if (frm["date"].ToString().Trim() != "" && frm["date2"].ToString().Trim() != "" && frm["date"].ToString().Trim() != "1901-01-01" && frm["date2"].ToString().Trim() != "1901-01-01")
            {
                DateTo = Convert.ToDateTime(frm["date2"]).AddDays(1).ToString("yyyy-MM-dd");
                DateFrom = frm["date"];
            }
            else
            {
                DateTo = "1901-01-01";
                DateFrom = "1901-01-01";
                ViewBag.Message = "Can't query. Please select date";
            }
            ViewBag.DateTo = DateTo;
            ViewBag.DateFrom = DateFrom;

            Object[] Model = new Object[3];
            Model[0] = new FEA_BusinessLogic.Statistic.Report().GetDRReport(DateTo, DateFrom);
            Model[1] = new FEA_BusinessLogic.Statistic.Report().GetHRReport(DateTo, DateFrom);
            Model[2] = new FEA_BusinessLogic.Statistic.Report().GetHRTotalReport(DateTo, DateFrom);
            Session["Result"] = Model;
            return View(Model);
        }

        public ActionResult CallbackPivotGrid(FormCollection frm, string Type)
        {
            string LoadGridName = string.Empty;
            switch (Type)
            {
                case "DR":
                    LoadGridName = FEA_ITS_Site.Models.Helper.PartialParameter.StatisticDR;
                    break;
                case "HR":
                    LoadGridName = FEA_ITS_Site.Models.Helper.PartialParameter.StatisticHR;
                    break;
                case "HRDetail":
                    LoadGridName = FEA_ITS_Site.Models.Helper.PartialParameter.StatisticHRDetail;
                    break;
                default:
                    break;
            }
            return GetGridView(Session["Result"] as Object, LoadGridName);
        }  
    }
}
