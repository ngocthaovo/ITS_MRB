using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using FEA_BusinessLogic;
using FEA_ITS_Site.Models.Helper;
using DevExpress.Web.Mvc;
using FEA_ITS_Site.Models;
using DevExpress.Web;
using DevExpress.Utils;
namespace FEA_ITS_Site.Controllers
{
    public class WarehouseAreaExportController : BaseController
    {

        private string SessionName = "WHExportItemModelList";
        private string SessionNameCusCode = "CustoperCode";

        #region "MainPage"
        public ActionResult MainPage()
        {
            return View();
        }
        #endregion

        #region "Stock Out Order"

        public ActionResult StockOutOrder(string ID)
        {
            WHExportOrder model;
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "WarehouseAreaExport/StockOutOrder" + string.Format("{0}", ID == null ? "" : "?ID=" + ID)) });
            //

            Session[SessionName] = new List<Models.WHExportItemModel>();
            if (ID != null && ID.Length > 0)
            {
                model = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().GetItem(ID);

                Session[SessionName] = ConvertToItemInfos(model.WHExportOrderDetails);
            }
            else
            {
                model = new FEA_BusinessLogic.WHExportOrder();
                model.Status = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.OrderStatus.New;
                model.isConfirm = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.DRAFF;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult StockOutOrder(FEA_BusinessLogic.WHExportOrder model)
        {
            string sError = "";

            

            if (Request["btnSave"] != null)
            {
                PopulateDetailData(Request.Form["mydata"].ToString());

                WHExportOrder newModel = SaveData(model, out sError);
                if (sError.Length == 0 && newModel != null)
                {
                    model = newModel;
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgSuccess;
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                }
            }
            else if (Request["btnUpdate"] != null) // update data
            {
                PopulateDetailData(Request.Form["mydata"].ToString());

                Boolean flag = UpdateData(model, out sError);
                if (flag)
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
                Boolean flag = Confirm_Cancel_Data(model.ID, FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.CONFIRMED, out sError);
                if (flag)
                {
                    model.isConfirm = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.CONFIRMED;
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
                Boolean flag = Confirm_Cancel_Data(model.ID, FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.DRAFF, out sError);
                if (flag)
                {
                    model.isConfirm = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.DRAFF;
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgUpdateSuccess;
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;

                }
            }
            else if (Request["btnDelete"] != null)
            {
                if (model.ID.Length != 0)
                {
                    try
                    {
                        bool flag = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().DeleteItem(model.ID, Helper.UserLoginInfo.UserId);
                        if(!flag)
                        {
                            ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                            ViewBag.EditInfo = Resources.Resource.msgDeleteWHImportFailed + " Or " + Resources.Resource.msgDeleteStockInFailed; ;
                        }
                        else
                        {
                            return RedirectToAction("StockOutOrder", "WarehouseAreaExport"); // create new page.
                        }
                    }
                    catch (Exception e)
                    {
                        ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                        ViewBag.EditInfo = e.Message;
                    }
                }
            }


            ModelState.Clear();
            return View(model);
        }

        /// <summary>
        /// Insert data to database
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        private WHExportOrder SaveData(WHExportOrder data, out string sError)
        {
            try
            {
                sError = "";

                data.OrderCode = Controllers.HelperController.GenerateCode(TagPrefixParameter.WAREHOUSEAREAEXPORT);
                data.CreatorID = FEA_ITS_Site.Helper.UserLoginInfo.UserId;
                data.CreateDate = DateTime.Now;
                data.LastUpdateDate = DateTime.Now;

                data.Status = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.OrderStatus.NORMAL;
                data.isConfirm = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.DRAFF;

                data.WHExportOrderDetails = WHExportOrderDetailList();

                var result = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().InsertItem(data);
                if (result.Length > 0)
                    return data;
                else
                {
                    sError = "Cannot insert data";
                    return null;
                }
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                return null;
            }
        }

        private bool UpdateData(WHExportOrder data, out string sError)
        {
            sError = "";

            try

            {

                data.WHExportOrderDetails = WHExportOrderDetailList();
                Boolean flag = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().UpdateItem(data, i => i.CustomerPO, i => i.InvoiceNumber
                                                                                                            , i => i.ShipmentDate,i=>i.Note
                                                                                                            , i => i.WHExportOrderDetails);
                return flag;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
            }

            return false;
        }

        private bool Confirm_Cancel_Data(string ID, FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus status, out string sError)
        {
            sError = string.Empty;
            try
            {


                int flag;

                if (status == FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.CONFIRMED)
                {
                    flag = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().ConfirmOrder(ID);
                    if (flag <= 0)
                    {
                        sError = Resources.Resource.msgCancelConfirmWHImportFailed;
                        return false;
                    }
                }

                else if (status == FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.ConfirmStatus.DRAFF)
                {
                    flag = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().CancelOrder(ID);
                    if (flag <= 0)
                    {
                        sError = Resources.Resource.msgCancelConfirmWHImportFailed;
                        return false;
                    }
                }
                else return false;
                return true;
            }
            catch (Exception ex)
            {
                sError = ex.Message + " - " + ex.StackTrace;
                return false;
            }
        }


        private List<WHExportItemModel> ConvertToItemInfos(ICollection<WHExportOrderDetail> OrderItemDetail)
        {
            List<Models.WHExportItemModel> lstResult = new List<WHExportItemModel>();
            WHExportItemModel _itemInfo;

            FEA_BusinessLogic.Base.Connection conn = new FEA_BusinessLogic.Base.Connection();
            foreach (WHExportOrderDetail itemdetail in OrderItemDetail)
            {
                _itemInfo = new WHExportItemModel()
                {
                    ID = itemdetail.ID,
                    PackingManifestDetailID =itemdetail.PackingManifestDetailID,
                    IsChecked =true,
                    SerialNo = itemdetail.PackingManifestDetail.SerialNo.ToString(),
                    CustomerPO = itemdetail.PackingManifestDetail.PackingManifest.CustomerPO,
                    InvoiceCode = itemdetail.WHExportOrder.InvoiceNumber,
                    Item = itemdetail.PackingManifestDetail.MainLine_,
                    ColorName = itemdetail.PackingManifestDetail.ColorName,
                    Style = itemdetail.PackingManifestDetail.ShortDescription,
                    RANGE = itemdetail.PackingManifestDetail.RANGE,
                    IsExported = itemdetail.IsExported,
                    IsReturned = itemdetail.IsReturned,
                    Size = itemdetail.PackingManifestDetail.Size,
                    //Size = (itemdetail.PackingManifestDetail.PackingManifest.isCoordinate == 1) ?
                    //string.Join(", ", conn.db.PackingManifestDetailCoordinations.Where(i => i.PackingManifestDetailID == itemdetail.PackingManifestDetail.ID
                    //                                                                                                && i.SerialNo == itemdetail.PackingManifestDetail.SerialNo && i.RANGE == itemdetail.PackingManifestDetail.RANGE && i.PackCode == itemdetail.PackingManifestDetail.PackCode && i.PO_ == itemdetail.PackingManifestDetail.PO_
                    //                                                                                                ).OrderBy(i => i.Size).Select(i => i.Size).ToList()) : itemdetail.PackingManifestDetail.Size,
                    //Quantity = (itemdetail.PackingManifestDetail.PackingManifest.isCoordinate == 1) ?
                    //string.Join(", ", conn.db.PackingManifestDetailCoordinations.Where(i => i.PackingManifestDetailID == itemdetail.PackingManifestDetail.ID
                    //                                                                                                && i.SerialNo == itemdetail.PackingManifestDetail.SerialNo && i.RANGE == itemdetail.PackingManifestDetail.RANGE && i.PackCode == itemdetail.PackingManifestDetail.PackCode && i.PO_ == itemdetail.PackingManifestDetail.PO_
                    //                                                                                                    ).OrderBy(i => i.Size).Select(i => i.ItemQtyPerCtnPack).ToList()) : itemdetail.PackingManifestDetail.ItemQuantity.ToString()
                };

                lstResult.Add(_itemInfo);
            }
            lstResult = lstResult.OrderBy(i => i.SerialNo).ToList(); // Modify by jason
            return lstResult;
        }

        private List<WHExportOrderDetail> WHExportOrderDetailList()
        {
            List<WHExportOrderDetail> lst = new List<WHExportOrderDetail>();
            if (Session[SessionName] != null)
            {
                List<Models.WHExportItemModel> lstItemInfo = (List<WHExportItemModel>)Session[SessionName];
                WHExportOrderDetail expItemDetail;
                foreach (WHExportItemModel i in lstItemInfo)
                {
                    if(i.IsChecked)
                    {
                        expItemDetail = new WHExportOrderDetail()
                        {
                            ID = Guid.NewGuid().ToString(),
                            PackingManifestDetailID = i.PackingManifestDetailID,
                            IsExported = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderDetailManager.ExportStatus.UnExported,
                            IsReturned = (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderDetailManager.ReturnStatus.UnReturned,
                            Temp1 = "",
                            Temp2 = "",
                            Temp3 = ""
                        };
                        lst.Add(expItemDetail);
                    }
                }
            }
            return lst;
        }

        #endregion

        #region "Stockoutorder detail"

        /// <summary>
        /// Used at Device manage and hardmanage
        /// </summary>
        /// <returns></returns>
        public ActionResult ItemPartial(string CustomerPO)
        {
            CustomerPO = (Request.Params["CustomerPO"] != null) ? Request.Params["CustomerPO"].ToString() : CustomerPO;
            string CustomerCode = "";
            ViewData["CustomerName"] = "N/A";

            Dictionary<string, string> lst = new FEA_BusinessLogic.WarehouseArea.WHExportOrderDetailManager().GetItemByCustomerPO(CustomerPO,out CustomerCode);

            if (CustomerCode != "" && CustomerCode.Trim().Length > 0)
            { 
                Session[SessionNameCusCode] = CustomerCode;
                ViewData["CustomerName"] = WarehouseArea.ExcelReader.CustomerTypeCode.CustomerTypeCodeList()[CustomerCode];
            }
           
            return PartialView("_ItemPartial", lst);
        }


        [ValidateInput(false)]
        public ActionResult GetListPartial()
        {
            return GetGridView(Session[SessionName], "WareHouseExportOrderDetailGrid");
        }


         [HttpPost]
        public ActionResult ClearItemOrder()
        {
           Session[SessionName] = new List<WHExportItemModel>();
           return GetListPartial();
        }

        [HttpPost]
        public ActionResult AddItemToOrder(string ItemValue, string CustomerPO, string SelectData)
        {
            PopulateDetailData(SelectData);

            if (ItemValue == null)
            {
                string _sErr = Resources.Resource.msgInputError;
                _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, "Item");
                ViewData["EditError"] = _sErr;
            }
            else
            {
               
                if (FindItemDetailInList(ItemValue))
                {
                    // ViewData["EditError"] = Resources.Resource.msgDataduplicate;
                }
                else
                {
                    List<FEA_BusinessLogic.PackingManifestDetail> lst = new FEA_BusinessLogic.WarehouseArea.PackingManifestDetailManager().GetItemDetailByItem(CustomerPO, ItemValue, Session[SessionNameCusCode] == null ? "" : Session[SessionNameCusCode].ToString(), GetItemListInDetail());
                    if (lst != null && lst.Count > 0)
                    {
                        Models.WHExportItemModel itemModel;
                        List<WHExportItemModel> lstWHExportItem = new List<WHExportItemModel>();

                        FEA_BusinessLogic.Base.Connection conn = new FEA_BusinessLogic.Base.Connection();
                        foreach (PackingManifestDetail pkDetail in lst)
                        {
                            itemModel = new Models.WHExportItemModel()
                            {
                                ID = pkDetail.ID,
                                PackingManifestDetailID = pkDetail.ID,
                                IsChecked = true,
                                SerialNo = pkDetail.SerialNo.ToString(),
                                CustomerPO = CustomerPO,
                                InvoiceCode = pkDetail.PackingManifest.InvoiceNo,
                                Item = pkDetail.MainLine_,
                                ColorName = pkDetail.ColorName,
                                Style = pkDetail.ShortDescription,
                                RANGE = pkDetail.RANGE,
                                //Size = (pkDetail.PackingManifest.isCoordinate == 1) ? string.Join(", ", conn.db.PackingManifestDetailCoordinations.Where(i => i.PackingManifestDetailID == pkDetail.ID).OrderBy(i => i.Size).Select(i => i.Size).ToList()) : pkDetail.Size,
                                //Quantity = (pkDetail.PackingManifest.isCoordinate == 1) ? string.Join(", ", conn.db.PackingManifestDetailCoordinations.Where(i => i.PackingManifestDetailID == pkDetail.ID).OrderBy(i => i.Size).Select(i => i.ItemQtyPerCtnPack).ToList()) : pkDetail.ItemQuantity.ToString()

                            };
                            if (Session[SessionName] != null)
                            {
                                lstWHExportItem = (List<WHExportItemModel>)Session[SessionName];
                                lstWHExportItem.Add(itemModel);
                            }
                            else
                            {
                                lstWHExportItem.Add(itemModel);
                            }

                            // short Item
                            lstWHExportItem = lstWHExportItem.OrderBy(i => i.SerialNo).ToList();
                        }

                        Session[SessionName] = lstWHExportItem;
                    }
                }
            }
            return GetListPartial();
        }

        /// <summary>
        /// true la da ton tai
        /// </summary>
        /// <param name="ItemValue"></param>
        /// <returns></returns>
        private bool FindItemDetailInList(string ItemValue)
        {
            List<WHExportItemModel> lst = new List<WHExportItemModel>();
            if (Session[SessionName] != null)
                lst = (List<WHExportItemModel>)Session[SessionName];

            return lst.Where(i => i.Item == ItemValue).Count() > 0 ? true : false;

        }

        /// <summary>
        /// Get All SerialNo in detail to compare with database
        /// </summary>
        /// <returns></returns>
        public List<long> GetItemListInDetail()
        {
            List<long> result = new List<long>();
            if (Session[SessionName] != null)
                result = ((List<WHExportItemModel>)Session[SessionName]).Select(a=>long.Parse( a.SerialNo)).ToList();
            return result;
        }

        private void PopulateDetailData(string sData)
        {
            var lstItem = sData;
            try
            {
                Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(lstItem);
                if (values == null && sData.Trim().Length ==0)
                    values = new Dictionary<string, string>();

                if (values != null)
                {
                    List<string> lstToCheck = values.Select(i => i.Key).ToList<string>();
                    if (Session[SessionName] != null)
                    {
                        var lst = (List<WHExportItemModel>)Session[SessionName];
                        foreach (WHExportItemModel item in lst)
                        {
                            if (lstToCheck.Where(i => i == item.ID).Count() > 0)
                                item.IsChecked = true;
                            else
                                item.IsChecked = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }
        #endregion


        #region "Mangement"
        public ActionResult ExportManagement()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ExportManagement(FormCollection frm)
        {

            if (Request.Form["btnQuery"] != null)
            {
                DateTime DateTo;
                DateTime DateFrom;
                string sDateTo = "";
                string sDateFrom = "";
                if (frm["date"].ToString().Trim() != "" && frm["date2"].ToString().Trim() != "")
                {
                   
                    sDateTo =  frm["date2"].ToString() + " 23:00:00";
                    sDateFrom =  frm["date"].ToString() + " 01:00:00";
                    DateTo = Convert.ToDateTime(sDateTo);
                    DateFrom = Convert.ToDateTime(sDateFrom);
                    ViewBag.DateTo = sDateTo;
                    ViewBag.DateFrom = sDateFrom;  
                }
                else
                {
                    DateTo =  Convert.ToDateTime("2000-01-01 23:00:00");
                    DateFrom = Convert.ToDateTime("2000-01-01 01:00:00");
                    ViewBag.Message = "Please select Date";
                }
                //Object Model = (Object)new FEA_BusinessLogic.ELand.PurchaseReport().GetMain(Convert.ToDateTime(DateTo).ToString("yyyy-MM-dd"), Convert.ToDateTime(DateFrom).ToString("yyyy-MM-dd"));
                List<sp_GetWHExportOrderByMainLine_Result> Model = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().GetItems(DateFrom, DateTo);
                Session["ResultExport"] = Model;
                return View(Model);
            }
            else
            {
                return GridViewExtension.ExportToXlsx(CreateExportGridViewSettings(), Session["ResultExport"] as object, "Warehouse Export Report - " + DateTime.Now.Year + " - " + DateTime.Now.Month, true);
            }
        }

        [ValidateInput(false)]
        public ActionResult EditImportModesPartial(string DateTo, string DateFrom)
        {
            if (DateTo != "" && DateFrom != "")
            {
                List<sp_GetWHExportOrderByMainLine_Result> Model = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().GetItems(Convert.ToDateTime(DateFrom), Convert.ToDateTime(DateTo));
                Session["ResultExport"] = Model;
                ViewBag.DateTo = DateTo;
                ViewBag.DateFrom = DateFrom;  
            }
            return GetGridView(Session["ResultExport"] != null ? Session["ResultExport"] as List<sp_GetWHExportOrderByMainLine_Result>:null, FEA_ITS_Site.Models.Helper.PartialParameter.WHExport_list_grid);

        }

        public ActionResult CallbackGrid()
        {
            if (Session["ResultExport"] != null)
            {
                return GetGridView(Session["ResultExport"] != null ? Session["ResultExport"] as List<sp_GetWHExportOrderByMainLine_Result> : null, FEA_ITS_Site.Models.Helper.PartialParameter.WHExport_list_grid);
            }
            else
            {
                return GetGridView(null, FEA_ITS_Site.Models.Helper.PartialParameter.WHExport_list_grid);
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditImportModesDeletePartial(string ID,string DateTo, string DateFrom)
        {
            if (ID.Length != 0)
            {
                try
                {
                    bool flag = new FEA_BusinessLogic.WarehouseArea.WHExportOrderManager().DeleteItem(ID, Helper.UserLoginInfo.UserId); ;

                    if (!flag)
                        ViewData["DeleteError"] = Resources.Resource.msgDeleteWHImportFailed + " Or " + Resources.Resource.msgDeleteStockInFailed;
                }
                catch (Exception e)
                {
                    ViewData["DeleteError"] = e.Message;
                }
            }
            return EditImportModesPartial(DateTo, DateFrom);
        }

        public static GridViewSettings CreateExportGridViewSettings()
        {
            GridViewSettings settings = new GridViewSettings();
            settings.Name = "gvWhExportListGirdManagement";
            settings.KeyFieldName = "ID";
           // settings.CallbackRouteValues = new { Controller = "WarehouseAreaExport", Action = "EditImportModesPartial" };
            settings.Width = System.Web.UI.WebControls.Unit.Percentage(100);
            settings.SettingsPager.PageSize = FEA_ITS_Site.Helper.Ultilities.PageSize;
            settings.Columns.Add(column =>
            {
                column.FieldName = "OrderCode";
                column.Caption = Resources.Resource.OrderCode;
                column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.CellStyle.CssClass = "tCenter";
                column.EditFormSettings.Visible = DefaultBoolean.False;
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "CustomerPO";
                column.Caption = Resources.Resource.CustomerPO;
                column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "InvoiceNumber";
                column.Caption = Resources.Resource.InvoiceNum; 
                column.Width = System.Web.UI.WebControls.Unit.Percentage(10);

            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "MainLine_";
                column.Caption = "PO Item";
                column.Width = System.Web.UI.WebControls.Unit.Percentage(10);

            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "ShipmentDate";
                column.Caption = Resources.Resource.ShipmentDate;


                column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.ColumnType = MVCxGridViewColumnType.DateEdit;

                DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
                dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
                dateEdit.DisplayFormatInEditMode = true;
                dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);

            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "CreateDate";
                column.Caption = Resources.Resource.CreateDate;


                column.Width = System.Web.UI.WebControls.Unit.Percentage(10);
                column.ColumnType = MVCxGridViewColumnType.DateEdit;

                DateEditProperties dateEdit = column.PropertiesEdit as DateEditProperties;
                dateEdit.DisplayFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
                dateEdit.DisplayFormatInEditMode = true;
                dateEdit.EditFormatString = FEA_Ultil.FEAStringClass.DataDateFormat(FEA_ITS_Site.Helper.SessionManager.CurrentLang, false, true);
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "UserName";
                column.Caption = Resources.Resource.Creator;
                column.Width = System.Web.UI.WebControls.Unit.Percentage(10);

            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "TotalContainer";
                column.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                column.Caption = Resources.Resource.ContainerCount;
                column.Width = System.Web.UI.WebControls.Unit.Percentage(5);
            });
            settings.Columns.Add(column =>
            {
                column.FieldName = "TotalImportContainer";
                column.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                column.Caption = Resources.Resource.TotalContainerImported;
                column.Width = System.Web.UI.WebControls.Unit.Percentage(5);
            });

            settings.Columns.Add(column =>
            {
                column.FieldName = "TotalReturnContainer";
                column.UnboundType = DevExpress.Data.UnboundColumnType.Integer;
                column.Caption = Resources.Resource.TotalReturnContainer;
                column.Width = System.Web.UI.WebControls.Unit.Percentage(5);
            });
            

            settings.CommandColumn.Visible = true;
            settings.CommandColumn.Width = System.Web.UI.WebControls.Unit.Percentage(5);
            settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "TotalContainer");
            settings.Settings.ShowFilterRowMenu = true;
            settings.Settings.ShowFilterRow = true;
            settings.Settings.ShowGroupPanel = true;
            settings.Settings.ShowFooter = true;

            return settings;
        }
        #endregion

    }
}
