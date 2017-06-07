using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using DevExpress.Web;
using System.Web.UI.WebControls;

namespace FEA_ITS_Site.Controllers
{
    public class ELandController : BaseController
    {

        public ActionResult QuerySignFlow()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuerySignFlow(FormCollection frm)
        {
            ViewBag.OrderCode = frm["txtOrderCode"];
            return View((Object)new FEA_BusinessLogic.ELand.SignFlow().GetMain(frm["txtOrderCode"]));
        }

        public ActionResult PurchaseReport()
        {
            if (!IsPostback)
            {
                Session["Result"] = null;
            }
            return View(Session["Result"] as Object); 
        }
        
        [HttpPost]
        public ActionResult PurchaseReport(FormCollection frm)
        {

            if (Request.Form["btnQuery"] !=null)
            {
                string DateTo;
                string DateFrom;
                if (frm["date"].ToString().Trim() != "" && frm["date2"].ToString().Trim() != "")
                {
                    DateTo = Convert.ToDateTime(frm["date2"]).AddDays(1).ToString("yyyy-MM-dd");
                    DateFrom = frm["date"];
                    ViewBag.DateTo = DateTo;
                    ViewBag.DateFrom = DateFrom;
                }
                else
                {
                    DateTo = "1901-01-01";
                    DateFrom = "1901-01-01";
                    ViewBag.Message = "Please select Date";
                }
                Object Model = (Object)new FEA_BusinessLogic.ELand.PurchaseReport().GetMain(Convert.ToDateTime(DateTo).ToString("yyyy-MM-dd"), Convert.ToDateTime(DateFrom).ToString("yyyy-MM-dd"));
                Session["Result"] = Model;
                return View(Model);
            }
            else
            {
                    return GridViewExtension.ExportToXlsx(CreateExportGridViewSettings(), Session["Result"] as object, "Purchase Report - " + DateTime.Now.Year + " - " + DateTime.Now.Month, true);  
            }
           
        }

        public ActionResult CallbackGrid()
        {
            Object x = new Object[1];
            if (Session["Result"] != null)
            {
                x = Session["Result"] as Object;
            }
            else
            {
                x = null;

            }
          return GetGridView(x, FEA_ITS_Site.Models.Helper.PartialParameter.PurchaseReport);
        }

        public static GridViewSettings CreateExportGridViewSettings()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "gvPurchaseReport";
            settings.CallbackRouteValues = new { Controller = "ELand", Action = "CallbackGrid" };
            settings.Width = Unit.Percentage(100);
            settings.KeyFieldName = "PR Code";
            settings.SettingsBehavior.AllowSort = false;
            settings.SettingsPager.AlwaysShowPager = false;
            settings.Settings.VerticalScrollBarMode = ScrollBarMode.Auto;
            settings.Settings.VerticalScrollableHeight = 450;
            settings.SettingsPager.Mode = GridViewPagerMode.ShowPager;
            settings.SettingsPager.PageSize = 20;
            settings.SettingsBehavior.AllowSelectByRowClick = true;
            settings.SettingsBehavior.ColumnResizeMode = ColumnResizeMode.Control;
            settings.Settings.HorizontalScrollBarMode = ScrollBarMode.Visible;
            settings.Settings.ShowFilterBar = GridViewStatusBarMode.Auto;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowFilterRowMenuLikeItem = true;

            settings.Columns.Add(column =>
            {
                column.FieldName = "Company";
                // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);

            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "PR_Code";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);

            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "PH_Code";
                //column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Level";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Apply_Dept";
              //  column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Type";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Supplier_code";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Supplier_name";
              //  column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Description";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Purchase_Items";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Quantity";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Currency";
                // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "TaxName";
                // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Unit";
                column.Caption = "Unit";
                column.Width = 100;
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Price_Px__Forecast_";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Amount__Forecast_";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            // Added by Tony (2016-10-07)
            settings.Columns.Add(column =>
            {
                column.FieldName = "Price__Discount_";
                // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Amount__Discount_";
                // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            //
            settings.Columns.Add(column =>
            {
                column.FieldName = "Price__Purchased_";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Amount__Purchased_";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Apply_Apporve_Date";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.ColumnType = MVCxGridViewColumnType.DateEdit;
                DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
                dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
                dateEdit.DisplayFormatInEditMode = true;
                dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Assignment";
              //  column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.ColumnType = MVCxGridViewColumnType.DateEdit;
                DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
                dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
                dateEdit.DisplayFormatInEditMode = true;
                dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Purchaser";
                column.Caption = "Purchaser";
                column.Width = 100;
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "Last_approver";
                // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.ColumnType = MVCxGridViewColumnType.DateEdit;
                DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
                dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
                dateEdit.DisplayFormatInEditMode = true;
                dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Last_approval_date";
                //  column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.ColumnType = MVCxGridViewColumnType.DateEdit;
                DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
                dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
                dateEdit.DisplayFormatInEditMode = true;
                dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            });

            //settings.Columns.Add(column =>
            //{
            //    column.FieldName = "Purchase_Manager_Approve";
            //   // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            //    column.ColumnType = MVCxGridViewColumnType.DateEdit;
            //    DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
            //    dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            //    dateEdit.DisplayFormatInEditMode = true;
            //    dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            //});
            //settings.Columns.Add(column =>
            //{
            //    column.FieldName = "Auditor_Approve";
            //   // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            //    column.ColumnType = MVCxGridViewColumnType.DateEdit;
            //    DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
            //    dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            //    dateEdit.DisplayFormatInEditMode = true;
            //    dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            //});
            //settings.Columns.Add(column =>
            //{
            //    column.FieldName = "Division_Manager_Approve";
            //  //  column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            //    column.ColumnType = MVCxGridViewColumnType.DateEdit;
            //    DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
            //    dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            //    dateEdit.DisplayFormatInEditMode = true;
            //    dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            //});
            settings.Columns.Add(column =>
            {
                column.FieldName = "GM_Approve";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.ColumnType = MVCxGridViewColumnType.DateEdit;
                DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
                dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
                dateEdit.DisplayFormatInEditMode = true;
                dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Current_approver";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Pending_time";
               // column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });
            return settings;
        }
    }
}
