using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Parameters;

using FEA_Ultil;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
namespace FEA_ITS_Site.Reports
{
    public partial class StockOutReport : DevExpress.XtraReports.UI.XtraReport
    {
        private int detailCounter = 0;
        public StockOutReport(string StockID)
        {
            InitializeComponent();
            InitSettings();
            IntData();
            // Create a parameter and define its main properties.
            Parameter parameter1 = new Parameter();
            parameter1.Type = typeof(System.String);
            parameter1.Name = "StockOutID";
            parameter1.Visible = false;
            parameter1.Value = StockID;

            // Add the parameter to the report's collection,
            // and filter the report based on the parameter's value.
            this.Parameters.Add(parameter1);
            this.FilterString = "[ID] = [Parameters.StockOutID]";
            this.RequestParameters = false;
        }

        private void IntData()
        {
            lblCreateDate.Text = FEAStringClass.FormatDateString(FEA_ITS_Site.Helper.SessionManager.CurrentLang, DateTime.Now);
            lblConfirmDate.Text = FEAStringClass.FormatDateString(FEA_ITS_Site.Helper.SessionManager.CurrentLang,lblConfirmDate.Text);

        }

        private void InitSettings()
        {
            this.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
        }
        
        private void xrTableCell18_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            (sender as XRTableCell).Text = (++detailCounter).ToString();
        }

        private void lblConfirmDate_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            (sender as XRLabel).Text = FEAStringClass.FormatDateString(FEA_ITS_Site.Helper.SessionManager.CurrentLang, lblConfirmDate.Text);
        }

        private void xrTableCell19_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            if (xrTableCell19.Text.Length >30)
            {
                (sender as XRLabel).Text = "";
            } 
        }

    }
}
