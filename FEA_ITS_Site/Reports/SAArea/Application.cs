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
    public partial class Application : DevExpress.XtraReports.UI.XtraReport
    {
        private int detailCounter = 0;
        private int detailCounter_2 = 0;
        public Application(string ID, int OrderType)
        {
            InitializeComponent();
            InitSettings();
            IntData(OrderType);
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

        private void IntData(int OrderType)
        {
            lblOrderCode.Text = FEAStringClass.FormatDateString(FEA_ITS_Site.Helper.SessionManager.CurrentLang, DateTime.Now);

            if (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Borrow)
            {
                lblOrderType.Text = lblOrderType_2.Text = "Mượn Vào" + Environment.NewLine + "借入";

                txtDeliverydate.Text = txtDeliverydate_2.Text = "Ngày trả" + Environment.NewLine + "還回日期";
            }
            else if (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Lend)
            {
                lblOrderType.Text = lblOrderType_2.Text = "Cho mượn" + Environment.NewLine + " 借出";

                txtDeliverydate.Text = txtDeliverydate_2.Text = "Ngày trả" + Environment.NewLine + "還回日期";
            }
            else if (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Normal)
            {
                lblOrderType.Text = lblOrderType_2.Text = "Thông thường " + Environment.NewLine + "一般";

                txtDeliverydate.Text = txtDeliverydate_2.Text = "Ngày xuất" + Environment.NewLine + "出货日期";
            }
            else if (OrderType == (int)FEA_SABusinessLogic.ExportItemManager.OrderType.Adjust)
            {
                lblOrderType.Text = lblOrderType_2.Text = "Điều chỉnh ";

                txtDeliverydate.Text = txtDeliverydate_2.Text= "Ngày điều chỉnh" + Environment.NewLine + "調整日期";
            }

        }

        private void InitSettings()
        {
            this.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.PaperKind = System.Drawing.Printing.PaperKind.A4;
            this.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
        }
        
        private void xrTableCell18_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            xrTableCell18.Text = (++detailCounter).ToString();
        }

        private void lblConfirmDate_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
          //  (sender as XRLabel).Text = FEAStringClass.FormatDateString(FEA_ITS_Site.Helper.SessionManager.CurrentLang, lblConfirmDate.Text);
        }

        private void xrTableCell19_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //if (xrTableCell19.Text.Length >30)
            //{
            //    (sender as XRLabel).Text = "";
            //} 
        }

        private void xrTableCell70_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            xrTableCell70.Text = (++detailCounter_2).ToString();
        }

    }
}
