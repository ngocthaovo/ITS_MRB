using DevExpress.Web.Mvc;
using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FEA_ITS_Site.Controllers
{
    public class ITAssetDetailController : BaseController
    {

        #region "Import data"

        //
        // GET: /ITAssetDetail/

        public ActionResult ImportData()
        {
            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "ITAssetDetail/ImportData") });
            //


            string sError = "";
            if (Request["btnLoadExcel"] != null) // up Load  excel file
            {
                string filepath = DoUploadFile(out sError);
                if (sError.Length == 0)
                {
                    Session["FilePath"] = filepath;

                    // Save Data
                    int result = SaveData(out  sError);
                    if (result > 0)
                    {
                        ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                        ViewBag.EditInfo = Resources.Resource.msgSuccess;
                    }
                    else
                    {
                        ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                        ViewBag.EditInfo = sError;
                    }
                }
                else
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = sError;
                }
            }

            return View();
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


        private int  SaveData(out string sError)
        {
            sError = "";
            DataSet ds = GetDSDataFromExcel(out sError);

            if (ds != null && sError.Length == 0)
            {
                //Save the data into database
                List<FEA_BusinessLogic.ITSAssetDetail> lst = new FEA_ITS_Site.Helper.ITAsset.ConvertDataModel().ConvertData(ds, out sError);

                if(lst != null && sError =="")
                {
                    int result = new FEA_BusinessLogic.ITSAsset.ITAssetDetailManager().InsertItems(lst,true, out sError);
                    if (sError == "" && result > 0)
                    {
                        // Success
                        return result;
                    }
                    else
                    {
                        sError = "Cant upload";
                        return -1;
                    }
                }
            }
            return -1;
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
            catch (Exception ex)
            {
                sError = ex.Message + " - " + ex.StackTrace;
                return null;
            }
        }


        #endregion

        #region "Query"

        public ActionResult QueryData()
        {
            Session["Division"] = "";
            Session["DivisionName"] = "";

            Session["Dept"] = "";
            Session["DeptName"] = "";

            Session["Section"] = "";
            Session["SectionName"] = "";

            Session["RecCode"] = "";
            Session["RecName"] = "";

            Session["AssetType"] = "";
            Session["AssetTypeName"] = "";

            Session["Brand"] = "";

            if (Request["btnQuery"] != null)
            {

                Session["Division"] = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboDivision") == null ? "" : DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboDivision");
                Session["DivisionName"] = Request.Form["cboDivision"];

                Session["Dept"] = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboDept") == null ? "" : DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboDept");
                Session["DeptName"] = Request.Form["cboDept"];

                Session["Section"] = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboSection") == null ? "" : DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboSection");
                Session["SectionName"] = Request.Form["cboSection"];

                Session["RecCode"] = "";
                Session["RecName"] = Request.Form["txtUserName"];


                Session["AssetType"] = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboAssetType") == null ? "" : DevExpress.Web.Mvc.EditorExtension.GetValue<string>("cboAssetType");
                Session["AssetTypeName"] = Request.Form["cboAssetType"];

                Session["Brand"] = Request.Form["txtBrand"];
            }
            return View();
        }

        [ValidateInput(false)]
        public ActionResult EditModesPartial(/*string sFactory, string sDept, string sUserCode, string sUserName, string sAssetType, string sBrand*/)
        {
            string  sDept, sUserCode, sUserName, sAssetType, sBrand,sDivision,sSection;

            sDivision = Session["Division"].ToString();
            sDept = Session["Dept"].ToString();
            sSection = Session["Section"].ToString();

            sUserCode = Session["RecCode"].ToString();
            sUserName = Session["RecName"].ToString();
            sAssetType = Session["AssetType"].ToString();
            sBrand = Session["Brand"].ToString();

            List<FEA_BusinessLogic.ITSAssetDetail> lst = new FEA_BusinessLogic.ITSAsset.ITAssetDetailManager().GetItems(sDivision, sDept, sSection, sUserCode, sUserName, sAssetType, sBrand);
            return GetGridView(lst, "ItAssetItem_list_grid");
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesAddNewPartial([ModelBinder(typeof(DevExpressEditorsBinder))] FEA_BusinessLogic.ITSAssetDetail obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        if (obj.Division != null && obj.Division != "" && obj.Department != null && obj.Department != "" && obj.AssetType != "")
                        {
                            string sError="";

                            obj.ID = Guid.NewGuid().ToString();
                            obj.CreatedByHand = 1;
                            obj.UploadDate = DateTime.Now;
                            obj.UploadBy = Helper.UserLoginInfo.UserId;
                            obj.Status = 1;

                            if (obj.AssetID == null) obj.AssetID = "";
                            if (obj.AssetName == null) obj.AssetName = "";
                            if (obj.Brand == null) obj.Brand = "";
                            if (obj.RecName == null) obj.RecName = "";

                            if (obj.FactoryCode == null) obj.FactoryCode = "";
                            if (obj.DivisionCode == null) obj.DivisionCode = "";
                            if (obj.DepartmentCode == null) obj.DepartmentCode = "";
                            if (obj.EquipDate == null) obj.EquipDate = DateTime.Now;
                            if (obj.JobPosition == null) obj.JobPosition = "";
                            if (obj.Group == null) obj.Group = "";
                            if (obj.Model == null) obj.Model = "";
                            if (obj.Configuration == null) obj.Configuration = "";
                            if (obj.EmailCoding == null) obj.EmailCoding = "";
                            if (obj.EmailAddress == null) obj.EmailAddress = "";
                            if (obj.FJ_B_W == null) obj.FJ_B_W = "";
                            if (obj.FJ_Color == null) obj.FJ_Color = "";
                            if (obj.ExtNo == null) obj.ExtNo = "";
                            if (obj.Code == null) obj.Code = "";
                            if (obj.PassCode == null) obj.PassCode = "";
                            if (obj.DirectLine == null) obj.DirectLine = "";
                            if (obj.JobTitle == null) obj.JobTitle = "";

                            if (obj.Temp1 == null) obj.Temp1 = "";
                            if (obj.Temp2 == null) obj.Temp2 = "";
                            if (obj.Temp3 == null) obj.Temp3 = "";
                            if (obj.Temp4 == null) obj.Temp4 = "";
                            if (obj.Temp5 == null) obj.Temp5 = "";
                            int _result = new FEA_BusinessLogic.ITSAsset.ITAssetDetailManager().InsertItem(obj, out sError);
                            if (_result<= 0 || sError.Length !=0)
                                ViewData["EditError"] = sError;
                        }
                        else
                        {
                            ViewData["EditError"] = Resources.Resource.msgInputError;
                        }

                    }
                    catch (Exception e)
                    {
                        ViewData["EditError"] = e.Message;
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            else
                ViewData["EditError"] = Resources.Resource.msgInputError;
            return EditModesPartial();
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesUpdatePartial([ModelBinder(typeof(DevExpressEditorsBinder))] FEA_BusinessLogic.ITSAssetDetail obj)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (obj.Division != null && obj.Division != "" && obj.Department != null && obj.Department != "" && obj.AssetType != "")
                    {

                        if (obj.AssetID == null) obj.AssetID = "";
                        if (obj.AssetName == null) obj.AssetName = "";
                        if (obj.Brand == null) obj.Brand = "";
                        if (obj.RecName == null) obj.RecName = "";

                        if (obj.FactoryCode == null) obj.FactoryCode = "";
                        if (obj.DivisionCode == null) obj.DivisionCode = "";
                        if (obj.DepartmentCode == null) obj.DepartmentCode = "";
                        if (obj.EquipDate == null) obj.EquipDate = DateTime.Now;
                        if (obj.JobPosition == null) obj.JobPosition = "";
                        if (obj.Group == null) obj.Group = "";
                        if (obj.Model == null) obj.Model = "";
                        if (obj.Configuration == null) obj.Configuration = "";
                        if (obj.EmailCoding == null) obj.EmailCoding = "";
                        if (obj.EmailAddress == null) obj.EmailAddress = "";
                        if (obj.FJ_B_W == null) obj.FJ_B_W = "";
                        if (obj.FJ_Color == null) obj.FJ_Color = "";
                        if (obj.ExtNo == null) obj.ExtNo = "";
                        if (obj.Code == null) obj.Code = "";
                        if (obj.PassCode == null) obj.PassCode = "";
                        if (obj.DirectLine == null) obj.DirectLine = "";
                        if (obj.JobTitle == null) obj.JobTitle = "";



                        if (obj.Temp1 == null) obj.Temp1 = "";
                        if (obj.Temp2 == null) obj.Temp2 = "";
                        if (obj.Temp3 == null) obj.Temp3 = "";
                        if (obj.Temp4 == null) obj.Temp4 = "";
                        if (obj.Temp5 == null) obj.Temp5 = "";

                        bool _result = new FEA_BusinessLogic.ITSAsset.ITAssetDetailManager().UpdateItem(obj, o => o.Division, o => o.Department,o=>o.Section
                                             ,o=>o.AssetName, o => o.RecCode
                                             ,o=>o.RecName, o=>o.AssetType, o=>o.AssetID,o=>o.Brand, o=>o.Date
                                             ,o=>o.FactoryCode,o=>o.DivisionCode, o=>o.DepartmentCode, o=>o.EquipDate
                                             ,o=>o.JobPosition, o=>o.Group, o=>o.Model, o=>o.Configuration, o=>o.EmailCoding
                                             ,o=>o.EmailAddress, o=>o.FJ_B_W,o=>o.FJ_Color, o=>o.ExtNo, o=>o.Code, o=>o.PassCode
                                             ,o=>o.DirectLine, o=>o.JobTitle
                                             ,o=>o.Temp1, o=>o.Temp2,o=>o.Temp3,o=>o.Temp4,o=>o.Temp5);
                        if (!_result)
                            ViewData["EditError"] = Resources.Resource.msgInsertFail;
                    }
                    else
                    {
                        ViewData["EditError"] = Resources.Resource.msgInputError;
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return EditModesPartial();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditModesDeletePartial(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ID != "" && ID != "")
                    {


                        bool _result = new FEA_BusinessLogic.ITSAsset.ITAssetDetailManager().DeleteItem(ID);
                        if (!_result)
                            ViewData["EditError"] = Resources.Resource.msgInsertFail;
                    }
                    else
                    {
                        ViewData["EditError"] = Resources.Resource.msgInputError;
                    }

                }
                catch (Exception e)
                {
                    ViewData["EditError"] = e.Message;
                }
            }
            return EditModesPartial();
        }
        #endregion


    }
}
