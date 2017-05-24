﻿using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FEA_BusinessLogic;
using FEA_ITS_Site.Models.Helper;
using DevExpress.Web.Mvc;
using DevExpress.Web;
namespace FEA_ITS_Site.Controllers
{
    public class WarehouseAreaController : BaseController
    {
        //
        // GET: /WarehouseArea/

        public ActionResult MainPage(string CustomerCodeType)
        {
            ViewBag.CustomerCodeType = CustomerCodeType;
            return View();
        }


        #region Import Excel File

        #region Insert,Update

        [HttpGet]
        public ActionResult ImportDataFromExcel(string ID, string CustomerCodeType)
        {
            PackingManifest model;

            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WarehouseArea/ImportDataFromExcel?CustomerCodeType=" + CustomerCodeType) });
            //

            if(ID != null && ID.Length > 0)
            {
                model = new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().GetItem(ID,-1);
                Session["FilePath"] = Path.Combine(Server.MapPath(Helper.Ultilities.WarehouseAreaUploadFile), model.AttachmentLink);
            }
            else
            {
                model = new FEA_BusinessLogic.PackingManifest();
                model.STATUS = (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.OrderStatus.New;
                model.isConfirm = (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus.UNCONFIRM;
                model.OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.WAREHOUSEAREA);
                model.isCoordinate = 0;
                Session["FilePath"] = "";
                model.CustomerCode = CustomerCodeType;
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult ImportDataFromExcel(FEA_BusinessLogic.PackingManifest model)
        {
            string sError = "";
            if (Request["btnLoadExcel"] != null) // up Load  excel file
            {
                string filepath = DoUploadFile(out sError);
                if (sError.Length == 0){
                    Session["FilePath"] = filepath;
                    model.isCoordinate = CheckIsCoordination();
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                }
            }
            else if (Request["btnSave"] != null) // Save to DB
            {
                PackingManifest newModel = SaveData(out sError, model.CustomerCode);
                if (sError.Length == 0 && newModel != null)
                {
                    model = newModel;
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgSuccess;

                  //  return View(new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().GetItem(model.ID, -1));
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                }
            }
            else if(Request["btnUpdate"]!= null) // update data
            {
                Boolean flag = UpdateData(out sError, model.CustomerCode);
                if(flag)
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                }
            }

            else if (Request["btnConfirm"] != null) // confirm
            {
                Boolean flag = Confirm_Cancel_Data(model.ID, FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus.CONFIRMED, out sError);
                if(flag)
                {
                    model.isConfirm = (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus.CONFIRMED;
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                    
                }
            }

            else if (Request["btnCancelConfirm"] != null) // cancel confirm
            {
                Boolean flag = Confirm_Cancel_Data(model.ID, FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus.UNCONFIRM, out sError);
                if (flag)
                {
                    model.isConfirm = (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus.UNCONFIRM;
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;

                }
            }

            else if (Request["btnRemoveBarcode"] != null)
            {
                var lstItem = Request.Form["mydata"];
                int result = RemoveBarcodes(lstItem, out sError);
                if (result> 0)
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                }
            }

            ModelState.Clear();

            return View(model);
        }


        public ActionResult LoadWarehouseDetail(string CustomerCodeType)
        {

            WarehouseArea.ExcelReader.ColumnIdentifier.CustomerCode = CustomerCodeType;

            ViewData["CustomerCodeType"] = CustomerCodeType;
            string sError = "";

            DataSet ds = GetDSDataFromExcel(out sError);
            if (sError.Length == 0 && ds != null && ds.Tables.Count > 0)
                return GetGridView(ds, "warehouse_detail_data");
            else if (sError.Length > 0)
            {
                ViewData["EditError"] = sError;
            }
            return GetGridView(null, "warehouse_detail_data");

       
        }

        public ActionResult LoadPackingManifestDetailDetail(string PackingManifestID)
        {

            ViewData["IsFromImport"] = "1";
            ViewData["PackingManifestID"] = PackingManifestID;
            List<PackingManifestDetail> lst  = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().GetItems(PackingManifestID);

            return GetGridView(lst, "warehouse_packingManifest_detail_data");

        }

        public ActionResult LoadPackingManifestDetailDetailOnQuery(string PoNumber, string CustomerPO, DateTime dtFrom, DateTime dtTo, string TypeQuery, string Color, string Size, string OrderState)
        {
            PoNumber = (PoNumber == null ? "" : PoNumber);
            CustomerPO = (CustomerPO == null ? "" : CustomerPO);
            ViewData["IsFromImport"] = "0";
            ViewData["PoNumber"] = PoNumber;
            ViewData["CustomerPO"] = CustomerPO;
            ViewData["dtFrom"] = dtFrom;
            ViewData["dtTo"] = dtTo;
            ViewData["TypeQuery"] = TypeQuery;

            if (Session["ListPackingManifest"] == null)
            {
                Session["ListPackingManifest"] = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().Query(PoNumber, CustomerPO, dtFrom, dtTo, TypeQuery, Color, Size, OrderState == null || OrderState == "" ? 1 : (OrderState == "Close" ? 0 : 1));
            }
            return GetGridView(Session["ListPackingManifest"] as List<PackingManifestDetail>, "warehouse_packingManifest_detail_data");

            //List<PackingManifestDetail> _result = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().Query(PoNumber, CustomerPO, dtFrom, dtTo, TypeQuery, Color, Size, OrderState == null || OrderState == "" ? 1 : (OrderState == "Close" ? 0 : 1));
            //return GetGridView(_result, "warehouse_packingManifest_detail_data");
        }

        private PackingManifest SaveData(out string sError,string CustomerCode)
        {
            sError = "";
            DataSet ds = GetDSDataFromExcel(out sError);

            if (ds != null && sError.Length == 0)
            {
                string OrderCode, PONumber, InvoiceNo, CustomerPO;
                int isCoordinate =0;
                // Coordinate for UA
                if (CustomerCode == "UA7" && ds.Tables[0].Rows[0]["R"].ToString() == "1")
                {
                    string PrevSerialFrom = "";
                    string PrevSerialTo = "";
                  
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        string SerialFrom = "";
                        string SerialTo = "";
                        SerialFrom = ds.Tables[0].Rows[i]["Serial From"].ToString();
                        SerialTo = ds.Tables[0].Rows[i]["Serial To"].ToString();
                        if (SerialFrom != PrevSerialFrom && SerialTo != PrevSerialTo)
                        {
                            PrevSerialFrom = SerialFrom;
                            PrevSerialTo = SerialTo;
                        }
                        else
                        {
                            ds.Tables[0].Rows[i]["Serial From"] = "";
                            ds.Tables[0].Rows[i]["Serial To"]= "";
                            ds.Tables[0].Rows[i]["Range"] = "";
                        }
                    }
                }

                OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.WAREHOUSEAREA);
                PONumber = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("PONo");
                PONumber = (PONumber == null) ? "" : PONumber.Trim();
                InvoiceNo = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("InvoiceNo");
                InvoiceNo = (InvoiceNo == null) ? "" : InvoiceNo.Trim();
                CustomerPO = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("CustomerPO").Trim();
                isCoordinate = DevExpress.Web.Mvc.EditorExtension.GetValue<int>("isCoordinate");
                FEA_BusinessLogic.PackingManifest data = new WarehouseArea.ExcelReader.ConvertDataModel(CustomerCode).ConvertData(ds.Tables[0], OrderCode, CustomerPO, PONumber, InvoiceNo, isCoordinate, out sError);
               
                if (!(data == null || data.PackingManifestDetails.Count == 0 || sError.Length > 0))
                {
                    data.AttachmentLink = Path.GetFileName(Session["FilePath"].ToString());
                    data.Description = "";
                    data.TEMP1 = data.TEMP2 = data.TEMP3 = "";
                    data.isCoordinate = isCoordinate;

                    try
                    {
                                            // Save to db
                        string sID = new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().InsertItem(data, Helper.UserLoginInfo.UserId);
                        if (sID.Length != 0)
                        {
                            data.ID = sID;
                            data.STATUS = (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.OrderStatus.SAVED;
                            return data;
                        }
                    }
                    catch(Exception ex)
                    {
                        sError = ex.Message;
                    }
                }
            }
            return null;
        }

        private bool UpdateData(out string sError,string CustomerCode )
        {
            sError = "";
            DataSet ds = GetDSDataFromExcel(out sError);
            if (ds != null && sError.Length == 0)
            {
                string OrderCode, PONumber, InvoiceNo, CustomerPO;
                int isCoordinate = 0;

                OrderCode = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("OrderCode");

                PONumber = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("PONo");
                PONumber = (PONumber == null) ? "" : PONumber.Trim();

                InvoiceNo = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("InvoiceNo");
                InvoiceNo = (InvoiceNo == null) ? "" : InvoiceNo.Trim();

                CustomerPO = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("CustomerPO").Trim();

                isCoordinate = DevExpress.Web.Mvc.EditorExtension.GetValue<int>("isCoordinate");

                FEA_BusinessLogic.PackingManifest data = new WarehouseArea.ExcelReader.ConvertDataModel(CustomerCode).ConvertData(ds.Tables[0], OrderCode, CustomerPO, PONumber, InvoiceNo, isCoordinate, out sError);
                

                if (!(data == null || data.PackingManifestDetails.Count == 0 || sError.Length > 0))
                {
                    data.AttachmentLink = Path.GetFileName(Session["FilePath"].ToString());

                    // update data to db

                    try
                    {
                        Boolean flag = new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().UpdateItem(data, i => i.PONo, i => i.InvoiceNo, i => i.CustomerPO,i=>i.AttachmentLink, i => i.PackingManifestDetails, i=>i.isCoordinate);
                        return flag;
                    }
                    catch(Exception ex)
                    {
                        sError = ex.Message;
                    }
                }
            }
            return false;
        }

        private bool Confirm_Cancel_Data(string ID, FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus status,out string sError)
        {
            sError = string.Empty;
            try
            {
                Boolean flag = new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().ChangeConfirmStatus(ID, status);
                if (!flag)
                { sError = Resources.Resource.msgCancelConfirmWHImportFailed;
                return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                sError = ex.Message + " - " + ex.StackTrace;
                return false;
            }
        }


        /// <summary>
        /// Get dataset from excel file
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        private DataSet GetDSDataFromExcel(out string sError)
        {
            sError = "";
            try
            {
                string filePath = (Session["FilePath"] != null) ? Session["FilePath"].ToString() : "";
                if (filePath.Length != 0)
                {
                    FileStream stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);
                    string fileExt = Path.GetExtension(filePath);

                    IExcelDataReader excelReader;

                    if (fileExt.ToLower().Contains(".xlsx"))
                    {
                        excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        
                    }
                    else if (fileExt.ToLower().Contains(".xls"))
                    { excelReader = ExcelReaderFactory.CreateBinaryReader(stream); }
                    else
                        excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                    //...
                    //4. DataSet - Create column names from first row
                    excelReader.IsFirstRowAsColumnNames = true;
                    DataSet result = excelReader.AsDataSet();


                    //6. Free resources (IExcelDataReader is IDisposable)
                    excelReader.Close();
                    return result;
                }
                else
                    return null;
            }
            catch(Exception ex)
            {
                sError = ex.Message + " - "+ ex.StackTrace;
                return null;
            }
        }


        /// <summary>
        /// Upload file excel to server
        /// </summary>
        /// <param name="sError"></param>
        /// <returns>Session contain the URL link to file</returns>
        private string DoUploadFile(out string sError)
        {
            var filePathUploaded = "";
            sError = "";
            try
            {
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[1];

                    if (file != null && file.ContentLength > 0)
                    {
                        if (!(Path.GetExtension(file.FileName).Contains("xlsx") || Path.GetExtension(file.FileName).Contains("xls")))
                        {
                            sError = Resources.Resource.msgInvalidExcelFile;

                        }
                        else
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var fileNameUploaded = Path.GetFileNameWithoutExtension(fileName) + string.Format("_{0:yy_MM_dd_hhmmss}", DateTime.Now) + Path.GetExtension(fileName);
                            filePathUploaded = Path.Combine(Server.MapPath(Helper.Ultilities.WarehouseAreaUploadFile), fileNameUploaded);
                            file.SaveAs(filePathUploaded);
                            sError = "";
                        }
                    }
                    else
                    {
                        sError = Resources.Resource.msgInputFile;
                    }
                }
                else
                {
                    sError = "File not found";
                }
                
            }
            catch (Exception ex)
            {
                sError = ex.Message;
            }

            return filePathUploaded;
        }

        /// <summary>
        ///  Hàm kiểm tra xem có phải là hàng phối ko
        /// </summary>
        /// <returns></returns>
        private int CheckIsCoordination()
        {
           string sError = "";
            DataSet ds = GetDSDataFromExcel(out sError);
            if (ds != null && sError.Length == 0)
            { 
                foreach (DataRow _row in ds.Tables[0].Rows)
                {
                    string sSerialFrom = "", sSerialTo = "",R = "";

                    sSerialFrom = _row[WarehouseArea.ExcelReader. ColumnIdentifier.SerialFrom].ToString();
                    sSerialTo = _row[WarehouseArea.ExcelReader.ColumnIdentifier.SerialTo].ToString();
                    R =  _row[WarehouseArea.ExcelReader.ColumnIdentifier.R].ToString();

                    if ((sSerialFrom.Trim().Length == 0 && sSerialTo.Trim().Length == 0) || R == "1")
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }

        #endregion

        #region Management
        public ActionResult ImportManagement()
        {

            return View();
        }


        [ValidateInput(false)]
        public ActionResult EditImportModesPartial(string CustomerCodeType)
        {
            ViewBag.CustomerCodeType = CustomerCodeType;
            List<PackingManifest> lstItem = new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().GetItems(-1, CustomerCodeType);
            return GetGridView(lstItem, Models.Helper.PartialParameter.WAREHOUSE_IMPORT_MANAGEMENT_LIST_GRID);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditImportModesDeletePartial(string ID, string CustomerCodeType)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().DeleteItem(ID, Helper.UserLoginInfo.UserId); ;

                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteWHImportFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return EditImportModesPartial(CustomerCodeType);
        }
        #endregion


        #endregion

        #region Query Data

        public ActionResult QueryData()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WarehouseArea/QueryData") });
            //
            
            ViewBag.PONumber = string.Empty;
            ViewBag.CusPO = string.Empty;
            ViewBag.DateFrom = DateTime.Now.AddDays(-1);
            ViewBag.DateTo = DateTime.Now;
            return View();
        }

        [HttpPost]
        public ActionResult QueryData(FormCollection collections)
        {
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WarehouseArea/QueryData") });

            ViewBag.DateFrom = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtFrom");
            ViewBag.DateTo = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtTo");
            ViewBag.PONumber = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtPONumber");
            ViewBag.CusPO = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtCusPoNum");
            ViewBag.TypeQuery = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cbTypeQuery");
            ViewBag.State = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cbOrderState");
            ModelState.Clear();

            if (ViewBag.PONumber == null) ViewBag.PONumber = "";
            if (ViewBag.CusPO == null) ViewBag.CusPO = "";

            ViewBag.IsPostBack = "1";


            if (Request.Form["btnQuery"] != null)
            {
                Session["ListPackingManifest"] = null;
                Session["ListDynamic"] = null;  
                return View();
            }
            else if (Request["btnExportExcel"] != null)
            {
                return ExportToExcel(ViewBag.PONumber, ViewBag.CusPO, ViewBag.DateFrom, ViewBag.DateTo, ViewData["TypeQuery"] == null ? "" : ViewData["TypeQuery"].ToString());
            }
            else return View();
        }


        public GridViewSettings GetExportSetting()
        {
            GridViewSettings settings = new GridViewSettings();

            settings.Name = "exporttoXls";

            settings.Columns.Add(column =>
            {
                column.FieldName = "PackingManifest.OrderCode";
                column.Caption = "Order Code";

            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "PackingManifest.PONo";
                column.Caption = "PO Number";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "PO_";
                column.Caption = "PO #";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "SerialNo";
                column.Caption = "SerialNo";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "RANGE";
                column.Caption = "RANGE";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "PackCode";
                column.Caption = "Pack Code";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "Line_";
                column.Caption = "Line #";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "BuyerItem_";
                column.Caption = "Buyer Item #";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "SKU_";
                column.Caption = "SKU #";
            });


            settings.Columns.Add(column =>
            {
                column.FieldName = "MainLine_";
                column.Caption = "MAIN LINE";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "ColorName";
                column.Caption = "Color Name";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "Size";
                column.Caption = "Size";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "ShortDescription";
                column.Caption = "Short Description";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "ShipmentMethod";
                column.Caption = "Shipment Method";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "ItemQuantity";
                column.Caption = "Item Qty";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "ItemQtyPerCtnPack";
                column.Caption = "Item Qty Per Ctn / Pack";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "InnetPkgCount";
                column.Caption = "Inner Pkg Count";
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "R";
                column.Caption = "R";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "CtnCode";
                column.Caption = "Ctn Code";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "NETNET";
                column.Caption = "Net Net";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "NET";
                column.Caption = "Net";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "GROSS";
                column.Caption = "Gross";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "UNIT";
                column.Caption = "Unit";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "L";
                column.Caption = "L";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "W";
                column.Caption = "W";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "H";
                column.Caption = "H";
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "UNIT2";
                column.Caption = "Unit";
            });

            settings.Columns.Add(
                column =>
                {
                    column.FieldName = "isCOnfirm";
                    column.Caption = "IsConfirmed";
                    column.Width = 100;
                    column.ColumnType = MVCxGridViewColumnType.CheckBox;
                }
            );

            settings.Columns.Add(column =>
            {
                column.FieldName = "isStockin";
                column.Caption = "IsStockined";
                column.Width = 100;
                column.ColumnType = MVCxGridViewColumnType.CheckBox;
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "STATUS";
                column.Caption = Resources.Resource.Status;
                column.Width = 100;
                column.ColumnType = MVCxGridViewColumnType.ComboBox;
                settings.Settings.ShowFooter = false;

                var comboBox = column.PropertiesEdit as DevExpress.Web.ComboBoxProperties;
                comboBox.DataSource = FEA_ITS_Site.Controllers.HelperController.GetWHStatusList();
                comboBox.TextField = "Value";
                comboBox.ValueField = "Key";

                comboBox.ValueType = typeof(int);

                comboBox.IncrementalFilteringMode = DevExpress.Web.IncrementalFilteringMode.Contains;
                comboBox.DropDownStyle = DevExpress.Web.DropDownStyle.DropDown;
            });

            settings.SettingsExport.ExportedRowType = DevExpress.Web.GridViewExportedRowType.All;
            settings.SettingsExport.FileName = "query"+string.Format("{0}",DateTime.Now.Ticks)+".xls";
            settings.SettingsExport.PaperKind = System.Drawing.Printing.PaperKind.A4;
            settings.SettingsExport.RenderBrick = (sender, e) =>
            {
                if (e.VisibleIndex % 2 == 0)
                    e.BrickStyle.BackColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
            };

            
            return settings;
        }

        public GridViewSettings GetExportSettingForDynamic()
        {
            GridViewSettings settings = new GridViewSettings();

            settings.Name = "exporttoXls";

            settings.Columns.Add(c =>
            {
                c.FieldName = "CustomerPO";
                c.Caption = @Resources.Resource.CustomerPO;
                c.Visible = true;
                c.Width = 80;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "MainLine_";
                c.Caption = "PO Item";
                c.Visible = true;
                c.Width = 80;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "PONo";
                c.Caption = "PO Number";
                c.Visible = true;
                c.Width = 80;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "PackingManifestType";
                c.Caption = @Resources.Resource.Type;
                c.Visible = true;
                c.Width = 80;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ShortDescription";
                c.Caption = "COMBO";
                c.Visible = true;
                c.Width = 80;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "InvoiceNo";
                c.Caption = "Invoice No";
                c.Visible = true;
                c.Width = 80;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ContainerCount";
                c.Caption = @Resources.Resource.ContainerCount;
                c.Visible = true;
                c.Width = 100;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "Quantity";
                c.Caption = @Resources.Resource.Quantity;
                c.Visible = true;
                c.Width = 80;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalContainerNotConfirmed";
                c.Caption = @Resources.Resource.TotalContainerNotConfirmed;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalQuantityNotConfirmed";
                c.Caption = @Resources.Resource.TotalQuantityNotConfirmed;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalContainerConfirmed";
                c.Caption = @Resources.Resource.TotalContainerConfirmed;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalQuantityConfirmed";
                c.Caption = @Resources.Resource.TotalQuantityConfirmed;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalReturnContainer";
                c.Caption = @Resources.Resource.TotalReturnContainer;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalReturnQuantity";
                c.Caption = @Resources.Resource.TotalReturnQuantity;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalContainerImported";
                c.Caption = @Resources.Resource.TotalContainerImported;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotaQuantityImported";
                c.Caption = @Resources.Resource.TotaQuantityImported;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalStockedInContainerFromERP";
                c.Caption = @Resources.Resource.TotalStockedInContainerFromERP;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalStockedInQuantityFromERP";
                c.Caption = @Resources.Resource.TotalStockedInQuantityFromERP;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "EndTotalContainer";
                c.Caption = @Resources.Resource.EndTotalContainer;
                c.Visible = true;
                c.Width = 180;
            });

            settings.Columns.Add(c =>
            {
                c.FieldName = "EndTotalQuantity";
                c.Caption = @Resources.Resource.EndTotalQuantity;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ContainerDifferences";
                c.Caption = @Resources.Resource.ContainerDifferences;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "QuantityDifferences";
                c.Caption = @Resources.Resource.QuantityDifferences;
                c.Visible = true;
                c.Width = 180;
            });
            //settings.Columns.Add(c =>
            //{
            //    c.FieldName = "ContainerDifferencesBetweenWarehouseAndPacking";
            //    c.Caption = @Resources.Resource.ContainerDifferencesBetweenWarehouseAndPacking;
            //    c.Visible = true;
            //    c.Width = 180;
            //});
            //settings.Columns.Add(c =>
            //{
            //    c.FieldName = "QuantityDifferencesBetweenWarehouseAndPacking";
            //    c.Caption = @Resources.Resource.QuantityDifferencesBetweenWarehouseAndPacking;
            //    c.Visible = true;
            //    c.Width = 180;
            //});
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalExportItem";
                c.Caption = Resources.Resource.TotalExportItem;
                c.Visible = true;
                c.Width = 180;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalExportItemFromERP";
                c.Caption = Resources.Resource.TotalExportItemFromERP;
                c.Visible = true;
                c.Width = 180;

            });
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "ContainerCount");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "Quantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalContainerNotConfirmed");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalQuantityNotConfirmed");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalContainerConfirmed");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalQuantityConfirmed");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalReturnContainer");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalReturnQuantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalContainerImported");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotaQuantityImported");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalStockedInContainerFromERP");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalStockedInQuantityFromERP");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "EndTotalContainer");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "EndTotalQuantity");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "ContainerDifferences");
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "QuantityDifferences");
           // settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "ContainerDifferencesBetweenWarehouseAndPacking");
           // settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "QuantityDifferencesBetweenWarehouseAndPacking");

            settings.SettingsExport.ExportedRowType = DevExpress.Web.GridViewExportedRowType.All;
            settings.SettingsExport.FileName = "Packing Manifest Dynamic Query_"+DateTime.Now.Date+".xls";
            settings.SettingsExport.PaperKind = System.Drawing.Printing.PaperKind.A4;

            return settings;
        }

        public ActionResult ExportToExcel(string PoNumber, string CustomerPO, DateTime? dtFrom, DateTime? dtTo,string TypeQuery)
        {
            if (PoNumber == null) PoNumber = "";
            if (CustomerPO == null) CustomerPO = "";
            GridViewSettings GridSetting = new GridViewSettings();
            //List<PackingManifestDetail> lst = new List<PackingManifestDetail>();
           // List<sp_GetDynamicQueryPackingManifest_Result> lst2 = new List<sp_GetDynamicQueryPackingManifest_Result>();
            if (TypeQuery == "Normal")
            {
               // lst = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().Query(PoNumber, CustomerPO, dtFrom, dtTo, TypeQuery,"","");
                GridSetting = GetExportSetting();

                DevExpress.XtraPrinting.XlsExportOptions XlsExportOptions = new DevExpress.XtraPrinting.XlsExportOptions();
                XlsExportOptions.TextExportMode = DevExpress.XtraPrinting.TextExportMode.Text; // 2 dòng này để chỉnh file xls theo định dạng text để không bị hiện chữ +e khi chuổi số quá dài
                return GridViewExtension.ExportToXlsx(GridSetting, Session["ListPackingManifest"] as List<PackingManifestDetail>);
            }
            else if (TypeQuery == "Dynamic")
            {
               // lst2 = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().GetDynamicQueryPackingManifest(PoNumber, CustomerPO, dtFrom, dtTo);
                GridSetting = GetExportSettingForDynamic();
                return GridViewExtension.ExportToXlsx(GridSetting, Session["ListDynamic"] as List<sp_GetDynamicQueryPackingManifest_Result>);
            }
            return View();

        }
        #endregion

        #region Remove Barcode

        [HttpGet]
        public ActionResult RemoveBarcode()
        {            
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WarehouseArea/RemoveBarcode") });
            //

            ViewBag.CusPO = "";
            return View();
        }

        [HttpPost]
        public ActionResult RemoveBarcode(FormCollection collections)
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WarehouseArea/RemoveBarcode") });
            // 

            string CustomperPO;
            CustomperPO = ViewBag.CusPO = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtCusPoNum");
            ViewBag.IsPostBack = "1";
            ModelState.Clear();

            if (Request["btnRemoveBarcode"] != null)
            { 

                var lstItem = Request.Form["mydata"];

                string sError = "";
                int result = RemoveBarcodes(lstItem, out sError);
                if (result>0)
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess + ", " + string.Format(Resources.Resource.msgRemoveBarcodeCount, result);
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                }
                return View();
            }
            else if (Request["btnExportExcel"] != null)
            {
               return ExportToExcel("", CustomperPO, null, null,"Normal");
            }
            else
            {
                return View();
            }
          
        }


        private int RemoveBarcodes(string lstItem, out string sError)
        {
            sError = "";

            try
            {
                if (lstItem.Trim().Length != 0)
                {
                    Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
                    if (values != null)
                    {
                        try
                        {
                            List<string> lstBarcodes = values.Select(i => i.Key).ToList();

                            int result = new FEA_BusinessLogic.WarehouseArea.PackingManifestManager().RemoveBarcode(lstBarcodes, FEA_ITS_Site.Helper.UserLoginInfo.UserId, FEA_ITS_Site.Helper.UserLoginInfo.CurrentUser.UserGroup.UserGroupName);
                            if (result > 0)
                                return result;
                            else
                                sError = "Not have item(s) to be deleted";
                        }
                        catch (Exception ex)
                        {
                            sError = ex.Message;
                            return -1;
                        }
                    }
                }
                else
                {

                    sError = Resources.Resource.msgSelectItem;

                    return -1;
                }
            }
            catch (Exception ex)
            {

                sError = ex.Message;
                return -1;
            }

            return -1;
        }

        public ActionResult LoadPackingManifestDetailDetailOnRemoveBarcode(string CustomerPO)
        {
            CustomerPO = (CustomerPO == null ? "" : CustomerPO);

            ViewData["IsFromImport"] = "-1";
            ViewData["CustomerPO"] = CustomerPO;


            List<PackingManifestDetail> lst = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().Query("", CustomerPO, null, null,"Normal","","",1);
            return GetGridView(lst, "warehouse_packingManifest_detail_data");
        }
              /// <summary>
              /// Added by Steven 2014-10-15 - Dynamic Query
              /// </summary>
              /// <param name="PoNumber"></param>
              /// <param name="CustomerPO"></param>
              /// <param name="dtFrom"></param>
              /// <param name="dtTo"></param>
              /// <param name="TypeQuery"></param>
              /// <returns></returns>
        public ActionResult GetDynamicQueryPackingManifest(string PoNumber, string CustomerPO, DateTime dtFrom, DateTime dtTo,string TypeQuery, string OrderState)
        {
            PoNumber = (PoNumber == null ? "" : PoNumber);
            CustomerPO = (CustomerPO == null ? "" : CustomerPO);

            ViewData["IsFromImport"] = "0";
            ViewData["PoNumber"] = PoNumber;
            ViewData["CustomerPO"] = CustomerPO;
            ViewData["dtFrom"] = dtFrom;
            ViewData["dtTo"] = dtTo;
            ViewData["TypeQuery"] = TypeQuery;
            ViewData["OrderState"] = OrderState;
            if (Session["ListDynamic"] == null)
            {
                Session["ListDynamic"] = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().GetDynamicQueryPackingManifest(PoNumber, CustomerPO, dtFrom, dtTo, OrderState == null || OrderState == "" ? 1 : (OrderState == "Close" ? 0 : 1));
            }
            return GetGridView(Session["ListDynamic"] as List<sp_GetDynamicQueryPackingManifest_Result>, "DynamicQueryPackingManifest");

            // List<sp_GetDynamicQueryPackingManifest_Result> _result  = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().GetDynamicQueryPackingManifest(PoNumber, CustomerPO, dtFrom, dtTo, OrderState == null || OrderState == "" ? 1 : (OrderState == "Close" ? 0 : 1));
            // Session["ListDynamic"] = _result; 
            //return GetGridView(_result, "DynamicQueryPackingManifest");
        }

        #endregion

        #region Scan Barcode



        public ActionResult ScanBarcode()
        {
            return View();
        }
        #endregion

        #region Common  function
        public ActionResult BarcodeHistoryPartial (string PackingManifestDetailID)
        {
            ViewData["PackingManifestDetailID"] = PackingManifestDetailID;
            List<HistoryScan> lstHis = new FEA_BusinessLogic.WarehouseArea.HistoryScanManager().GetItemList(PackingManifestDetailID, "");
            return GetGridView (lstHis,"warehouse_history_list_grid");
        }
        #endregion

        #region Query Shelf


        public ActionResult QueryShelf()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WarehouseArea/QueryShelf") });
            //
            Session["ShelfDataReport"] = null;
            ViewBag.SerialNo = string.Empty;
            ViewBag.CusPO = string.Empty;
            return View();
        }

        [HttpPost]
        public ActionResult QueryShelf(FormCollection collections)
        {
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "/WarehouseArea/QueryData") });

            ViewBag.SerialNo = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtSerialNo");
            ViewBag.CusPO = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtCusPoNum");
            ModelState.Clear();

            if (ViewBag.SerialNo == null) ViewBag.SerialNo = 0;
            if (ViewBag.CusPO == null) ViewBag.CusPO = "";

            ViewBag.IsPostBack = "1";


            if (Request.Form["btnQuery"] != null)
            {
                Session["ShelfDataReport"] = null;
                return View();
            }
            else if (Request["btnExportExcel"] != null)
            {
                return ExportShelfToExcel(ViewBag.SerialNo, ViewBag.CusPO);
            }
            else return View();
        }

        public ActionResult LoadShelfData(int SerialNo, string CustomerPO)
        {
            SerialNo = (SerialNo == null ? 0 : SerialNo);
            CustomerPO = (CustomerPO == null ? "" : CustomerPO);
            ViewData["SerialNo"] = SerialNo;
            ViewData["CustomerPO"] = CustomerPO;
            List<sp_GetTotalShelfDataReport_Result> lst = new FEA_BusinessLogic.WarehouseArea.WHImportOrderManager().GetTotalShelfDataReport(CustomerPO, SerialNo);
            Session["ShelfDataReport"] = lst;
            return GetGridView(lst, FEA_ITS_Site.Models.Helper.PartialParameter.TotalSheldReport);
        }

        public ActionResult ExportShelfToExcel(string PoNumber, string CustomerPO)
        {
            if (PoNumber == null) PoNumber = "";
            if (CustomerPO == null) CustomerPO = "";
            GridViewSettings GridSetting = new GridViewSettings();

            GridSetting = GetExportSettingForShelfReport();
            return GridViewExtension.ExportToXls(GridSetting, Session["ShelfDataReport"] as List<sp_GetTotalShelfDataReport_Result>);
        }

        public ActionResult CallbackShelfReport(int SerialNo, string CustomerPO)
        {
            SerialNo = (SerialNo == null ? 0 : SerialNo);
            CustomerPO = (CustomerPO == null ? "" : CustomerPO);

            ViewData["SerialNo"] = SerialNo;
            ViewData["CustomerPO"] = CustomerPO;

            if (Session["ShelfDataReport"] == null)
            {
                Session["ShelfDataReport"] =  new FEA_BusinessLogic.WarehouseArea.WHImportOrderManager().GetTotalShelfDataReport(CustomerPO, SerialNo);
            }

            return GetGridView(Session["ShelfDataReport"] as List<sp_GetTotalShelfDataReport_Result>,  FEA_ITS_Site.Models.Helper.PartialParameter.TotalSheldReport);
        }

        public GridViewSettings GetExportSettingForShelfReport()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "exporttoXls";

            settings.Columns.Add(c =>
            {
                c.FieldName = "ShelfCode";
                c.Caption = "ShelfCode";
                c.Visible = true;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
                c.FixedStyle = GridViewColumnFixedStyle.Left;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "Customer";
                c.Caption = "Customer";
                c.Visible = true;
                c.FixedStyle = GridViewColumnFixedStyle.Left;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "CustomerPO";
                c.Caption = "CustomerPO";
                c.Visible = true;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
                c.FixedStyle = GridViewColumnFixedStyle.Left;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "Style";
                c.Caption = "Style";
                c.Visible = true;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
                c.FixedStyle = GridViewColumnFixedStyle.Left;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "ShipmentMethod";
                c.Caption = "Shipment Method";
                c.Visible = true;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
                c.FixedStyle = GridViewColumnFixedStyle.Left;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "CtnCode";
                c.Caption = "CtnCode";
                c.Visible = true;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
                c.FixedStyle = GridViewColumnFixedStyle.Left;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "TotalBox";
                c.Caption = "Total Box";
                c.Visible = true;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
                c.FixedStyle = GridViewColumnFixedStyle.Left;
            });
            settings.Columns.Add(c =>
            {
                c.FieldName = "Status";
                c.Caption = "Status";
                c.Visible = true;
                c.Width = System.Web.UI.WebControls.Unit.Percentage(12.5);
            });

            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalBox");
            settings.SettingsExport.ExportedRowType = DevExpress.Web.GridViewExportedRowType.All;
            settings.SettingsExport.FileName = "Total shelf report_" + DateTime.Now.Date + ".xls";
            settings.SettingsExport.PaperKind = System.Drawing.Printing.PaperKind.A4;

            return settings;
        }

        #endregion

    }
}
