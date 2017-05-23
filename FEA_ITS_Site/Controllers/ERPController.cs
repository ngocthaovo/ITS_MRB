using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Aspose;
using DevExpress.Web.Mvc;
using FEA_BusinessLogic;
namespace FEA_ITS_Site.Controllers
{
    public class ERPController : BaseController
    {
        /// <summary>
        /// Allow user to query PO diverted
        /// </summary>
        /// <returns></returns>
        public ActionResult QueryDivertPO()
        {
            return View();
        }
        /// <summary>
        /// Allow user to query PO diverted
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public ActionResult QueryDivertPO(FormCollection frm)
        {
            ViewBag.FEPOCode = frm["txtFEPOCode"];
            Object[] _object = new Object[2];
            _object[0] = (List<FEA_BusinessLogic.sp_QueryPODivert_Result>)new FEA_BusinessLogic.ERP.QueryPODivert().GetMain(frm["txtFEPOCode"]);
            _object[1] = (List<FEA_BusinessLogic.sp_QueryPODivertDetail_Result>)new FEA_BusinessLogic.ERP.QueryPODivert().GetDetail(frm["txtFEPOCode"]);
            return View(_object);
        }


        public ActionResult UpdateOrderStatus()
        {
            string sFepos = "";
            if (Request.Form["btnUpdate"] != null)
            {
                try
                {
                    sFepos = Request.Form["txtFEPOs"];
                    string materialType = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("radMaterialType");
                    string FepoFormated = sFepos;

                    FepoFormated = FepoFormated.Trim();
                    FepoFormated = FepoFormated.Replace("'", "");
                    FepoFormated = FepoFormated.Replace("\r\n", ",");

                    int _result = new FEA_BusinessLogic.ERP.Order().UpdateOrderStatus(FepoFormated, materialType);

                    ViewBag.EditStatus = (_result > 0) ? Models.Helper.EditItemStatus.success : Models.Helper.EditItemStatus.failed;

                    if(_result > 0)
                    {
                        string _smessage  = Resources.Resource.msgUpdateSuccess;
                        if(_result ==2)
                        {
                            _smessage += "<br>Một vài customer đã được cập nhật status sang active(0) - Vui lòng phân quyền lại cho Customer";
                        }
                        ViewBag.EditInfo = _smessage;
                    }
                    else
                        ViewBag.EditInfo = "Không tìm thấy nhà cung cấp phù hợp";
                }
                catch (Exception ex)
                {
                    ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = ex.Message + ex.StackTrace + ex.InnerException;

                }
            }

            ViewBag.Fepo = sFepos;
            return View();
        }

        public ActionResult UpdateOrderPrice()
        {
            string sOrders = "";

            if (Request.Form["btnUpdate"] != null)
            {
                try
                {
                    sOrders = Request.Form["txtOrders"];

                    string snewprice = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtPrice");
                    decimal dNewprice = 0;

                    dNewprice = Decimal.Parse(snewprice, System.Globalization.CultureInfo.InvariantCulture);

                    if (dNewprice > 0)
                    {
                        string FepoFormated = sOrders;
                        FepoFormated = FepoFormated.Trim();
                        FepoFormated = FepoFormated.Replace("'", "");
                        FepoFormated = FepoFormated.Replace("\r\n", ",");

                        int _result = new FEA_BusinessLogic.ERP.Order().UpdateOrderPrice(FepoFormated, dNewprice);

                        ViewBag.EditStatus = (_result > 0) ? Models.Helper.EditItemStatus.success : Models.Helper.EditItemStatus.failed;

                        if (_result > 0)
                        {
                            string _smessage = Resources.Resource.msgUpdateSuccess;
                            ViewBag.EditInfo = _smessage;
                        }
                        else
                        {
                            ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                            ViewBag.EditInfo = "Không thể cập nhật giá";
                        }
                    }
                    else
                    {
                        ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                        ViewBag.EditInfo = "Giá nhập vào không đúng";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.EditStatus = Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = ex.Message + ex.StackTrace + ex.InnerException;

                }
            }
            ViewBag.Orders = sOrders;
            return View();
        }


        public ActionResult GetCostComparingSpan()
        {
            ViewBag.BeginDate = DateTime.Now.AddDays(-1);
            ViewBag.EndDate = DateTime.Now;
            ViewBag.CustomerCode = "";
            ViewBag.FEPOCode = "";
            ViewBag.CustomerCode = "";

            if (Request.Form["btnQuery"] != null)
            {
                ViewBag.BeginDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtBeginDate");
                ViewBag.EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate");
                ViewBag.FEPOCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtFEPOCode");
                ViewBag.CustomerCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtCustomerCode");
                if (ViewBag.FEPOCode == null) ViewBag.FEPOCode = "";
                if (ViewBag.CustomerCode == null) ViewBag.CustomerCode = "";
                if (ViewBag.IsSearch == null) ViewBag.IsSearch = true;
            }

            if (Request.Form["btnExport"] != null)
            {
                DateTime BeginDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtBeginDate");
                DateTime EndDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate");
                string sBeginDate = BeginDate.ToString("yyyy-MM");
                string sEndDate = EndDate.ToString("yyyy-MM");


                string FEPOCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtFEPOCode");
                string CustomerCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtCustomerCode");

                if (FEPOCode == null) FEPOCode = "";
                if (CustomerCode == null) CustomerCode = "";
                List<FEA_ITS_Site.Models.ERPModels.GetCostComparingSpanEXCELModel> data = (List<Models.ERPModels.GetCostComparingSpanEXCELModel>)GetGetCostComparingSpanModels(FEPOCode, CustomerCode, BeginDate, EndDate, 1);


                if (data != null && data.Count > 0)
                {
                    int recs = data.Count;
                    //FEAERP.Business.Utility.SetAsposeLicense();
                    try
                    {
                        Assembly RuntimeAssembly = Assembly.GetExecutingAssembly();
                        string[] names = RuntimeAssembly.GetManifestResourceNames();
                        foreach (string name in names)
                        {
                            if (name.EndsWith("Aspose.Cells.lic"))
                            {
                                System.IO.Stream stream = RuntimeAssembly.GetManifestResourceStream(name);
                                Aspose.Cells.License license = new Aspose.Cells.License();
                                license.SetLicense(stream);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }



                    Aspose.Cells.Workbook exWorkBook = new Aspose.Cells.Workbook();
                    Aspose.Cells.Worksheet exWorkSheet = exWorkBook.Worksheets[0];
                    Aspose.Cells.Cells exCells = exWorkSheet.Cells;


                    //設定資料列 
                    int styleIndex = exWorkBook.Styles.Add();
                    Aspose.Cells.Style stylecell = exWorkBook.Styles[styleIndex];

                    Aspose.Cells.Range range = exCells.CreateRange(6, 0, (recs / 2 * 3), 27);
                    range.Style = stylecell;
                    range.RowHeight = 18;
                    range.Style.Font.Size = 12;
                    range.Style.Font.IsBold = false;
                    range.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    range.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    range.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    range.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;

                    //設定標題 
                    int styleTitleIndex = exWorkBook.Styles.Add();
                    Aspose.Cells.Style styletitlecel = exWorkBook.Styles[styleTitleIndex];
                    Aspose.Cells.Range Titlerange = exCells.CreateRange(0, 0, 1, 27);
                    Titlerange.Style = styletitlecel;
                    Titlerange.RowHeight = 20;
                    Titlerange.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    Titlerange.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    Titlerange.Style.Font.Size = 20;
                    Titlerange.Style.Font.IsBold = true;
                    exCells[0, 0].PutValue(Resources.Resource._000002);//远东服装（越南）有限公司



                    int styleTitleIndex2 = exWorkBook.Styles.Add();
                    Aspose.Cells.Style styletitlecel2 = exWorkBook.Styles[styleTitleIndex2];
                    Aspose.Cells.Range Titlerange2 = exCells.CreateRange(1, 0, 1, 27);
                    Titlerange2.Style = styletitlecel2;
                    Titlerange2.RowHeight = 20;
                    Titlerange2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    Titlerange2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    Titlerange2.Style.Font.Size = 16;
                    Titlerange2.Style.Font.IsBold = true;
                    exCells[1, 0].PutValue(Resources.Resource._013610);//So sánh giá thành tạm tính và thực tế

                    Titlerange2 = exCells.CreateRange(2, 0, 1, 27);
                    Titlerange2.Style = styletitlecel2;
                    Titlerange2.RowHeight = 20;
                    Titlerange2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    Titlerange2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    Titlerange2.Style.Font.Size = 16;
                    Titlerange2.Style.Font.IsBold = true;
                    if (sBeginDate == sEndDate)
                    {
                        exCells[2, 0].PutValue("(MR Closed)Year: " + sBeginDate.Substring(0, 4) 
                            + ", Month: " + sBeginDate.Substring(5, 2) );
                    }
                    else//--多个月 
                    {
                        exCells[2, 0].PutValue("(MR Closed)Year: " + sBeginDate.Substring(0, 4)
                            + ", Month: " +  sBeginDate.Substring(5, 2)  + " - "
                        + "Year: " +  sEndDate.Substring(0, 4) 
                        + ", Month: " + sEndDate.Substring(5, 2));
                    }
                    // Modify by Tony (24-08-2016)
                    exCells.SetColumnWidth(0, 16);
                    exCells.SetColumnWidth(1, 11);
                    exCells.SetColumnWidth(2, 16);
                    exCells.SetColumnWidth(3, 11);
                    exCells.SetColumnWidth(4, 11);
                    exCells.SetColumnWidth(5, 8);
                    exCells.SetColumnWidth(6, 8);
                    exCells.SetColumnWidth(7, 11);
                    exCells.SetColumnWidth(8, 8);
                    exCells.SetColumnWidth(9, 7);
                    exCells.SetColumnWidth(10, 11);
                    exCells.SetColumnWidth(11, 11);
                    exCells.SetColumnWidth(12, 7);
                    exCells.SetColumnWidth(13, 11);
                    exCells.SetColumnWidth(14, 7);
                    exCells.SetColumnWidth(15, 11);
                    exCells.SetColumnWidth(16, 7);
                    exCells.SetColumnWidth(17, 11);
                    exCells.SetColumnWidth(18, 7);
                    exCells.SetColumnWidth(19, 11);
                    exCells.SetColumnWidth(20, 7);
                    exCells.SetColumnWidth(21, 11);
                    exCells.SetColumnWidth(22, 7);
                    exCells.SetColumnWidth(23, 11);
                    exCells.SetColumnWidth(24, 7);
                    exCells.SetColumnWidth(25, 11);
                    exCells.SetColumnWidth(26, 11);

                    //設定表頭 
                    int styleHeaderIndex = exWorkBook.Styles.Add();
                    Aspose.Cells.Style styletitlecel3 = exWorkBook.Styles[styleHeaderIndex];
                    Aspose.Cells.Range range2 = exCells.CreateRange(3, 0, 3, 1);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    range2.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    range2.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    range2.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    range2.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    exCells[3, 0].PutValue(Resources.Resource._010394);//訂單


                    range2 = exCells.CreateRange(3, 1, 3, 1);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 1].PutValue(Resources.Resource._013611);//机缝工厂

                    range2 = exCells.CreateRange(3, 2, 3, 1);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 2].PutValue(Resources.Resource._013612);//成品名称

                    // Thêm cột Tên Loại
                    range2 = exCells.CreateRange(3, 3, 3, 1);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 3].PutValue(Resources.Resource._013616);
                    //
                    range2 = exCells.CreateRange(3, 4, 3, 1);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 4].PutValue(Resources.Resource._013613);//预估/实际

                    range2 = exCells.CreateRange(3, 5, 3, 1);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 5].PutValue(Resources.Resource._011687);//SL DH

                    range2 = exCells.CreateRange(3, 6, 3, 1);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 6].PutValue(Resources.Resource._011071);//SL bán hàng

                    range2 = exCells.CreateRange(3, 7, 2, 2);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 7].PutValue(Resources.Resource._011072);//Số tiền bán hàng


                    range2 = exCells.CreateRange(3, 9, 1, 15);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 9].PutValue(Resources.Resource._011074);//Số tiền giá thành

                    range2 = exCells.CreateRange(4, 9, 1, 2);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 9].PutValue(Resources.Resource._011427);//Vải

                    range2 = exCells.CreateRange(4, 11, 1, 1);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 11].PutValue(Resources.Resource._010963);//Hao hụt tồn kho

                    range2 = exCells.CreateRange(4, 12, 1, 2);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 12].PutValue(Resources.Resource._011822);//Phụ liệu

                    range2 = exCells.CreateRange(4, 14, 1, 2);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 14].PutValue(Resources.Resource._010862);//Nhân công trực tiếp

                    range2 = exCells.CreateRange(4, 16, 1, 2);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 16].PutValue(Resources.Resource._010863);//Chi phí SX

                    range2 = exCells.CreateRange(4, 18, 1, 2);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 18].PutValue(Resources.Resource._010864);//Chi phí gia công in thêu

                    range2 = exCells.CreateRange(4, 20, 1, 2);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 20].PutValue(Resources.Resource._010865);//Phí gia công c.đoạn


                    range2 = exCells.CreateRange(4, 22, 1, 2);
                    range2.Style = styletitlecel3;
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[4, 22].PutValue(Resources.Resource._010789);//Tổng giá thành


                    range2 = exCells.CreateRange(3, 24, 2, 3);
                    range2.Style = styletitlecel3;
                    range2.Merge();
                    range2.RowHeight = 20;
                    range2.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.CenterAcross;
                    range2.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    range2.Style.Font.Size = 14;
                    range2.Style.Font.IsBold = true;
                    exCells[3, 24].PutValue(Resources.Resource._013614);//Lợi nhuận ròng


                    int styleHeaderIndexs = exWorkBook.Styles.Add();
                    Aspose.Cells.Style styletitlecels = exWorkBook.Styles[styleHeaderIndexs];
                    Aspose.Cells.Range ranges = exCells.CreateRange(5, 7, 1, 20); // Chỗ này vẽ 3 cái ô tô đậm
                    ranges.Style = styletitlecels;
                    ranges.RowHeight = 20;
                    ranges.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    ranges.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                    ranges.Style.Font.Size = 14;
                    ranges.Style.Font.IsBold = true;
                    ranges.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    ranges.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    ranges.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                    ranges.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;

                    exCells[5, 7].PutValue(Resources.Resource._010294);//Đơn giá
                    exCells[5, 8].PutValue(Resources.Resource._010207);//Số tiền

                    exCells[5, 9].PutValue(Resources.Resource._010294);
                    exCells[5, 10].PutValue(Resources.Resource._010207);

                    exCells[5, 11].PutValue(Resources.Resource._010207);

                    exCells[5, 12].PutValue(Resources.Resource._010294);
                    exCells[5, 13].PutValue(Resources.Resource._010207);

                    exCells[5, 14].PutValue(Resources.Resource._010294);
                    exCells[5, 15].PutValue(Resources.Resource._010207);

                    exCells[5, 16].PutValue(Resources.Resource._010294);
                    exCells[5, 17].PutValue(Resources.Resource._010207);

                    exCells[5, 18].PutValue(Resources.Resource._010294);
                    exCells[5, 19].PutValue(Resources.Resource._010207);

                    exCells[5, 20].PutValue(Resources.Resource._010294);
                    exCells[5, 21].PutValue(Resources.Resource._010207);

                    exCells[5, 22].PutValue(Resources.Resource._010294);
                    exCells[5, 23].PutValue(Resources.Resource._010207);

                    exCells[5, 24].PutValue(Resources.Resource._010294);
                    exCells[5, 25].PutValue(Resources.Resource._010207);

                    exCells[5, 26].PutValue(Resources.Resource._011081);//Mức lãi gộp (%)
                    
                    int kk = 0;
                    int j = 6;//从第六行开始写数据 
                    for (int i = 0; i < recs; i++)
                    {
                        if (kk != 0)
                        {
                        }
                        else
                        {
                            exCells[j, 0].PutValue(data[i].FEPOCode);
                            exCells[j, 1].PutValue(data[i].FactoryName);
                            exCells[j, 2].PutValue(data[i].StyleName);
                            exCells[j, 5].PutValue(Convert.ToDecimal(data[i].OrderQuantity));
                            exCells[j, 6].PutValue(Convert.ToDecimal(data[i].SalesQty));
                            exCells[j, 3].PutValue(data[i].EnglishName);
                        }
                        exCells[j, 4].PutValue(data[i].AccountType);
                        exCells[j, 7].PutValue(Math.Round(Convert.ToDecimal(data[i].SalesPrice), 2));
                        exCells[j, 8].PutValue(Math.Round(Convert.ToDecimal(data[i].SalesAmt), 2));
                        exCells[j, 9].PutValue(Math.Round(Convert.ToDecimal(data[i].FabricPrice), 2));
                        exCells[j, 10].PutValue(Math.Round(Convert.ToDecimal(data[i].FabricAmt), 2));
                        exCells[j, 11].PutValue(Math.Round(Convert.ToDecimal(data[i].DiscountAmt), 2));
                        exCells[j, 12].PutValue(Math.Round(Convert.ToDecimal(data[i].AccessoryPrice), 2));
                        exCells[j, 13].PutValue(Math.Round(Convert.ToDecimal(data[i].AccessoryAmt), 2));
                        exCells[j, 14].PutValue(Math.Round(Convert.ToDecimal(data[i].PayPrice), 2));
                        exCells[j, 15].PutValue(Math.Round(Convert.ToDecimal(data[i].PayAmt), 2));
                        exCells[j, 16].PutValue(Math.Round(Convert.ToDecimal(data[i].ProducePrice), 2));
                        exCells[j, 17].PutValue(Math.Round(Convert.ToDecimal(data[i].ProduceAmt), 2));
                        exCells[j, 18].PutValue(Math.Round(Convert.ToDecimal(data[i].ProcessPrice), 2));
                        exCells[j, 19].PutValue(Math.Round(Convert.ToDecimal(data[i].ProcessAmt), 2));
                        exCells[j, 20].PutValue(Math.Round(Convert.ToDecimal(data[i].OutwardPrice), 2));
                        exCells[j, 21].PutValue(Math.Round(Convert.ToDecimal(data[i].OutwardAmt), 2));
                        exCells[j, 22].PutValue(Math.Round(Convert.ToDecimal(data[i].SumPrice), 2));
                        exCells[j, 23].PutValue(Math.Round(Convert.ToDecimal(data[i].SumAmt), 2));
                        exCells[j, 24].PutValue(Math.Round(Convert.ToDecimal(data[i].ProfitPrice), 2));
                        exCells[j, 25].PutValue(Math.Round(Convert.ToDecimal(data[i].ProfitAmt), 2));
                        exCells[j, 26].PutValue(Math.Round(Convert.ToDecimal(data[i].ProfitRate), 2));
                       
                        j++;

                        if (kk == 1)
                        {
                            exCells[j, 4].PutValue(Resources.Resource._013615);//差异
                            exCells[j, 7].Formula = "=H" + (j - 1).ToString().TrimStart() + "-H" + j.ToString().TrimStart();
                            exCells[j, 8].Formula = "=I" + (j - 1).ToString().TrimStart() + "-I" + j.ToString().TrimStart();
                            exCells[j, 9].Formula = "=J" + (j - 1).ToString().TrimStart() + "-J" + j.ToString().TrimStart();
                            exCells[j, 10].Formula = "=K" + (j - 1).ToString().TrimStart() + "-K" + j.ToString().TrimStart();
                            exCells[j, 11].Formula = "=L" + (j - 1).ToString().TrimStart() + "-L" + j.ToString().TrimStart();
                            exCells[j, 12].Formula = "=M" + (j - 1).ToString().TrimStart() + "-M" + j.ToString().TrimStart();
                            exCells[j, 13].Formula = "=N" + (j - 1).ToString().TrimStart() + "-N" + j.ToString().TrimStart();
                            exCells[j, 14].Formula = "=O" + (j - 1).ToString().TrimStart() + "-O" + j.ToString().TrimStart();
                            exCells[j, 15].Formula = "=P" + (j - 1).ToString().TrimStart() + "-P" + j.ToString().TrimStart();
                            exCells[j, 16].Formula = "=Q" + (j - 1).ToString().TrimStart() + "-Q" + j.ToString().TrimStart();
                            exCells[j, 17].Formula = "=R" + (j - 1).ToString().TrimStart() + "-R" + j.ToString().TrimStart();
                            exCells[j, 18].Formula = "=S" + (j - 1).ToString().TrimStart() + "-S" + j.ToString().TrimStart();
                            exCells[j, 19].Formula = "=T" + (j - 1).ToString().TrimStart() + "-T" + j.ToString().TrimStart();
                            exCells[j, 20].Formula = "=U" + (j - 1).ToString().TrimStart() + "-U" + j.ToString().TrimStart();
                            exCells[j, 21].Formula = "=V" + (j - 1).ToString().TrimStart() + "-V" + j.ToString().TrimStart();
                            exCells[j, 22].Formula = "=W" + (j - 1).ToString().TrimStart() + "-W" + j.ToString().TrimStart();
                            exCells[j, 23].Formula = "=X" + (j - 1).ToString().TrimStart() + "-X" + j.ToString().TrimStart();
                            exCells[j, 24].Formula = "=Y" + (j - 1).ToString().TrimStart() + "-Y" + j.ToString().TrimStart();
                            exCells[j, 25].Formula = "=Z" + (j - 1).ToString().TrimStart() + "-Z" + j.ToString().TrimStart();
                            exCells[j, 26].Formula = "=AA" + (j - 1).ToString().TrimStart() + "-AA" + j.ToString().TrimStart();

                            int styleCellIndex = exWorkBook.Styles.Add();
                            Aspose.Cells.Style styleCellecel = exWorkBook.Styles[styleCellIndex];
                            Aspose.Cells.Range rangec = exCells.CreateRange(j - 2, 0, 3, 1);//合并单元格--订单 
                            rangec.Style = styleCellecel;
                            rangec.Merge();
                            rangec.Style.Font.Size = 12;
                            rangec.Style.Font.IsBold = false;
                            rangec.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Left;
                            rangec.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            rangec.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;

                            rangec = exCells.CreateRange(j - 2, 1, 3, 1);//合并单元格--机缝工厂 
                            rangec.Style = styleCellecel;
                            rangec.Merge();
                            rangec.Style.Font.Size = 12;
                            rangec.Style.Font.IsBold = false;
                            rangec.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Left;
                            rangec.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            rangec.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;

                            rangec = exCells.CreateRange(j - 2, 2, 3, 1);
                            rangec.Style = styleCellecel;
                            rangec.Merge();
                            rangec.Style.Font.Size = 12;
                            rangec.Style.Font.IsBold = false;
                            rangec.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Left;
                            rangec.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            rangec.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;

                            rangec = exCells.CreateRange(j - 2, 3, 3, 1); 
                            rangec.Style = styleCellecel;
                            rangec.Merge();
                            rangec.Style.Font.Size = 12;
                            rangec.Style.Font.IsBold = false;
                            rangec.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Left;
                            rangec.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            rangec.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangec.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;


                            int styleCellIndexq = exWorkBook.Styles.Add();
                            Aspose.Cells.Style styleCellecelq = exWorkBook.Styles[styleCellIndexq];
                            Aspose.Cells.Range rangeq = exCells.CreateRange(j - 2, 5, 3, 1);
                            rangeq.Style = styleCellecelq;
                            rangeq.Merge();
                            rangeq.Style.Font.Size = 12;
                            rangeq.Style.Font.IsBold = false;
                            rangeq.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Right;
                            rangeq.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;

                            rangeq = exCells.CreateRange(j - 2, 6, 3, 1);
                            rangeq.Style = styleCellecelq;
                            rangeq.Merge();
                            rangeq.Style.Font.Size = 12;
                            rangeq.Style.Font.IsBold = false;
                            rangeq.Style.HorizontalAlignment = Aspose.Cells.TextAlignmentType.Right;
                            rangeq.Style.VerticalAlignment = Aspose.Cells.TextAlignmentType.Center;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.TopBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.LeftBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.RightBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            rangeq.Style.Borders[Aspose.Cells.BorderType.BottomBorder].LineStyle = Aspose.Cells.CellBorderType.Thin;
                            j++;
                            kk = 0;
                        }
                        else
                        {
                            kk++;
                        }
                    }
                    try
                    {

                        string filename = DateTime.Now.ToString("yyyyMMdd") + "-" + "So sánh giá thành tạm tính và thực tế.xls";
                        System.IO.MemoryStream stream = new System.IO.MemoryStream();
                        exWorkBook.Save(stream, Aspose.Cells.FileFormatType.Excel2003);

                        stream.Position = 0;

                        return File(stream, "application/vnd.ms-excel", filename);

                       // exWorkBook.Save(localFilePath);
                       // MessageBox.Show(FEAERP.Business.Foundation.SystemConfig.GetConfig().StringMap.GetStringByID("013465"));//导出成功
                    }
                    catch (Exception ex)
                    {
                       // MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        //this.Cursor = Cursors.Arrow;
                    }
                }
            }
            return View();
        }

        [ValidateInput(false)]
        public ActionResult DocumentPartial(string FEPOCode, string CustomerCode, DateTime BeginDate, DateTime EndDate)
        {
            ViewBag.BeginDate = BeginDate;
            ViewBag.EndDate = EndDate;
            ViewBag.FEPOCode = (FEPOCode == null) ? "" : FEPOCode;
            ViewBag.CustomerCode = (CustomerCode == null) ? "" : CustomerCode;

            //var frm = new FEA_SABusinessLogic.ExportItemManager().GetSARequestList(ViewBag.OrderCode, ViewBag.BeginDate, ViewBag.EndDate);
           // var data = new FEA_BusinessLogic.Base.Connection().db.sp_WIPAccount_GetCostComparingSpan_ByCloseDate(BeginDate.ToString("yyyy-MM-dd"), EndDate.ToString("yyyy-MM-dd"), CustomerCode, FEPOCode, 0);

            var data = (List<FEA_ITS_Site.Models.ERPModels.GetCostComparingSpanModel>)GetGetCostComparingSpanModels(ViewBag.FEPOCode, ViewBag.CustomerCode, BeginDate, EndDate, 0);
           
            return GetGridView(
                data
                
               // null
                , Models.Helper.PartialParameter.GetCostComparingSpan);
        }

        private object GetGetCostComparingSpanModels(string FEPOCode, string CustomerCode, DateTime BeginDate, DateTime EndDate, int isExcel)
        {
            var prBeginDate = new SqlParameter
            {
                ParameterName = "BeginDate",
                Value = BeginDate.ToString("yyyy-MM")
            };

            var prEndDate = new SqlParameter
            {
                ParameterName = "EndDate",
                Value = EndDate.ToString("yyyy-MM")
            };

            var prCustomerCode = new SqlParameter
            {
                ParameterName = "CustomerID",
                Value = CustomerCode
            };
            var prFEPOCode = new SqlParameter
            {
                ParameterName = "FepoCode",
                Value = FEPOCode
            };

            var prIsExcel = new SqlParameter
            {
                ParameterName = "IsExcel",
                Value = isExcel
            };
            if (isExcel == 0)
            {

                return new FEA_BusinessLogic.Base.Connection().db.Database.SqlQuery<FEA_ITS_Site.Models.ERPModels.GetCostComparingSpanModel>("exec sp_WIPAccount_GetCostComparingSpan_ByCloseDate @BeginDate, @EndDate,@CustomerID,@FepoCode,@IsExcel   "
                                                                                                                                                  , prBeginDate, prEndDate, prCustomerCode, prFEPOCode, prIsExcel).ToList<FEA_ITS_Site.Models.ERPModels.GetCostComparingSpanModel>();
            }
            else

                return new FEA_BusinessLogic.Base.Connection().db.Database.SqlQuery<FEA_ITS_Site.Models.ERPModels.GetCostComparingSpanEXCELModel>("exec sp_WIPAccount_GetCostComparingSpan_ByCloseDate @BeginDate, @EndDate,@CustomerID,@FepoCode,@IsExcel   "
                                                                                                                                                  , prBeginDate, prEndDate, prCustomerCode, prFEPOCode, prIsExcel).ToList<FEA_ITS_Site.Models.ERPModels.GetCostComparingSpanEXCELModel>();
        }
        //Added by Tony (2017-03-07) Begin
        public ActionResult QueryOpenOrder()     
        {
            ViewBag.FromDate = DateTime.Now.AddDays(-10);
            ViewBag.ToDate = DateTime.Now;
            return View();
        }
        public ActionResult OpenOrderGird(string FEPOCode, DateTime FromDate, DateTime ToDate)
        {
            ViewBag.FEPOCode = (FEPOCode == "null" || FEPOCode == null || FEPOCode == "") ? "%" :"%"+ FEPOCode+ "%";
            ViewBag.FromDate =FromDate!=null?  FromDate.ToString("yyyy-MM-dd"):DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
            ViewBag.ToDate = ToDate!=null? ToDate.ToString("yyyy-MM-dd"): DateTime.Now.ToString("yyyy-MM-dd");
            var frm = new FEA_BusinessLogic.ERP.Order().GetOpenOrder(ViewBag.FEPOCode,ViewBag.FromDate,ViewBag.ToDate);
            return GetGridView(frm,Models.Helper.PartialParameter.QueryOpenOrder);
        }
 
        public ActionResult ExportToExcel()
        {
            string FEPO = (DevExpress.Web.Mvc.EditorExtension.GetValue<string>("FEPOCode") != null ?"%"+ DevExpress.Web.Mvc.EditorExtension.GetValue<string>("FEPOCode")+"%" : "%");
            string FromDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtBeginDate").ToString("yyyy-MM-dd");
            string ToDate = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate").ToString("yyyy-MM-dd");
            List<sp_GetOpenOrderDoc_Result> list = new FEA_BusinessLogic.ERP.Order().GetOpenOrder(FEPO, FromDate, ToDate);
            return GridViewExtension.ExportToXlsx(CreateGridViewSettings(), list as object, "Bảng theo dõi mở đơn hàng_重啟訂單管控表 From "+FromDate+"_ To "+ToDate, true);
        }
        static GridViewSettings CreateGridViewSettings()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "gvExportOpenOrder";
            settings.KeyFieldName = "OrderCode";
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.SettingsPager.PageSize = FEA_ITS_Site.Helper.Ultilities.PageSize;
            settings.Columns.Add(
                column =>
                {
                    column.FieldName = "OrderCode";
                    column.Caption = "單號\nMÃ ĐƠN";          
                    column.CellStyle.CssClass = "tCenter";
                });

            settings.Columns.Add(
                column =>
                {
                    column.FieldName = "Remark";
                    column.Caption = "申請單位\nĐƠN VỊ YÊU CẦU";
                    column.CellStyle.CssClass = "tCenter";
                });

            settings.Columns.Add(
                column =>
                {
                    column.FieldName = "FEPO";
                    column.Caption = "FEPO";
                    column.CellStyle.CssClass = "tCenter";
                });

            settings.Columns.Add(
               column =>
               {
                   column.FieldName = "Reason";
                   column.Caption = "原因\nNGUYÊN NHÂN";
                   column.CellStyle.CssClass = "tCenter";
               });

            settings.Columns.Add(
               column =>
               {
                   column.FieldName = "ItemDetailName";
                   column.Caption = "事由\nLÝ DO";
                   column.CellStyle.CssClass = "tCenter";
               });

            settings.Columns.Add(
               column =>
               {
                   column.FieldName = "Comment";
                   column.Caption = "長官意見\nÝ KIẾN CỦA CHỦ QUẢN";               
                   column.CellStyle.CssClass = "tCenter";
               });

            settings.Columns.Add(
               column =>
               {
                   column.FieldName = "Description";
                   column.Caption = "備註\nGHI CHÚ";
                   column.CellStyle.CssClass = "tCenter";
               });

            settings.CommandColumn.Visible = true;
            settings.CommandColumn.Width = System.Web.UI.WebControls.Unit.Percentage(5);
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;
            return settings;
        }
       //End
    }
}
