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
    public partial class Application_Donative : DevExpress.XtraReports.UI.XtraReport
    {
        private int detailCounter = 0;
        private int detailCounter_2 = 0;
        public Application_Donative(string ID)
        {
            InitializeComponent();
            InitSettings();
            IntData();
            // Create a parameter and define its main properties.
            Parameter parameter1 = new Parameter();
            parameter1.Type = typeof(System.String);
            parameter1.Name = "ID";
            parameter1.Visible = false;
            parameter1.Value = ID;

            // Add the parameter to the report's collection,
            // and filter the report based on the parameter's value.
            this.Parameters.Add(parameter1);
            this.FilterString = "[ID] = [Parameters.ID]";
            this.RequestParameters = false;
        }

        private void IntData()
        {

        }

        private void InitSettings()
        {
            this.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
        }

        private void xrTableCell19_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            string sex = xrTableCell19.Text;
            if (sex.ToLower() == "m") { xrTableCell19.Text = Resources.Resource.Male; }
            else
                xrTableCell19.Text = Resources.Resource.Female;
        }

        private void xrTableCell55_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            string sex = xrTableCell55.Text;
            if (sex.ToLower() == "m") { xrTableCell55.Text = Resources.Resource.Male; }
            else
                xrTableCell55.Text = Resources.Resource.Female;
        }
        

    }
}
