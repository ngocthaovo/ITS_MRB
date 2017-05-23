using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FEA_BusinessLogic;
using System.Collections.Generic;
using FEA_ITS_Site.Models;
namespace FEA_ITS_Site.Controllers
{
    public class WHImporterController : BaseController
    {
        private string SessionName = "WHImportItemModelList";

        //
        // GET: /WHImporter/

        public ActionResult Index(string ID)
        {
            WHImportOrder item;

            // Requite Permission
            if (!Helper.UserLoginInfo.IsLogin)
                return RedirectToAction("Login", "User", new { url = string.Format("{0}{1}", Helper.Ultilities.Root, "WHImporter/Index" + string.Format("{0}", ID == null ? "" : "?ID=" + ID)) });
            //

            if (ID == null)
            {
                item = new WHImportOrder();
                item.Status = -1;
            }

            else
            {
                item = new FEA_BusinessLogic.WarehouseArea.WHImportOrderManager().GetItem(ID);
                Session[SessionName] = ConvertToItemInfos(item.WHImportOrderDetails);
            }
            return View(item);
        }

        [HttpPost]
        public ActionResult Index(FEA_BusinessLogic.WHImportOrder model)
        {
            string sError = "";
            if (Request["btnDelete"] != null)
            {
                int result = new FEA_BusinessLogic.WarehouseArea.WHImportOrderManager().Deleteitem(model.ID);
                if(result > 0)
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.success;
                    ViewBag.EditInfo = Resources.Resource.msgSuccess;
                }
                else if (result == 0)
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = "Không tìm thấy Đơn";
                }
                else if (result == -1)
                {
                    ViewBag.EditStatus = FEA_ITS_Site.Models.Helper.EditItemStatus.failed;
                    ViewBag.EditInfo = "Đơn này đã có hàng lên kệ hoặc đã xuất nên không thể xóa";
                }
            }

            ModelState.Clear();
            return View(model);
        }
        private List<WHImportItemModel> ConvertToItemInfos(ICollection<WHImportOrderDetail> OrderItemDetail)
        {
            List<Models.WHImportItemModel> lstResult = new List<WHImportItemModel>();
            WHImportItemModel _itemInfo;

            FEA_BusinessLogic.Base.Connection conn = new FEA_BusinessLogic.Base.Connection();
            foreach (WHImportOrderDetail itemdetail in OrderItemDetail)
            {
                _itemInfo = new WHImportItemModel()
                {
                    ID = itemdetail.ID,
                    PackingManifestDetailID = itemdetail.PackingManifestDetailID,
                    IsChecked = false,
                    ShelfID = itemdetail.ShelfID,
                    ShelfName = itemdetail.ItemDetail == null ? "" : itemdetail.ItemDetail.ItemDetailName,
                    SerialNo = itemdetail.PackingManifestDetail.SerialNo.ToString(),
                    CustomerPO = itemdetail.WHImportOrder.CustomerPO,

                    Item = itemdetail.PackingManifestDetail.MainLine_,
                    ColorName = itemdetail.PackingManifestDetail.ColorName,
                    Style = itemdetail.PackingManifestDetail.ShortDescription,

                    IsExported = (itemdetail.Status == (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderDetailManager.ItemStatus.Exported) ? 1 : 0,
                    IsReturned = (itemdetail.Status == (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderDetailManager.ItemStatus.Returned) ? 1 : 0,
                    IsSetShelf = (itemdetail.ShelfID == null?0:1)
                };

                lstResult.Add(_itemInfo);
            }
            lstResult = lstResult.OrderBy(i => i.SerialNo).ToList(); // Modify by jason
            return lstResult;
        }


        [ValidateInput(false)]
        public ActionResult GetListPartial()
        {
            return GetGridView(Session[SessionName], "WHImportOrderList");
        }

        [HttpPost]
        public ActionResult AddItemToOrder(string ItemValue, string CustomerPO, string SelectData)
        {

            if (ItemValue == null || ItemValue =="")
            {
                string _sErr = Resources.Resource.msgInputError;
                _sErr += string.Format("<br/>" + Resources.Resource.msgInputRequite, Resources.Resource.ShelfName);
                ViewData["EditError"] = _sErr;
            }
            else
            {

                try
                {
                    Dictionary<string, string> values = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(SelectData);
                    if (values == null && SelectData.Trim().Length == 0)
                        values = new Dictionary<string, string>();

                    if (values != null)
                    {
                        List<string> lstToCheck = values.Select(i => i.Key).ToList<string>();
                        if (Session[SessionName] != null)
                        {
                            var lst = (List<WHImportItemModel>)Session[SessionName];
                            ItemDetail itemdetail = new FEA_BusinessLogic.ItemDetailManager().GetItem(ItemValue);

                            List<WHImportItemModel> lstaaa = lst.Where(i=>lstToCheck.Contains(i.ID)).ToList();
                            foreach (WHImportItemModel item in lstaaa)
                            {
                                // item.Status = (int)FEA_BusinessLogic.WarehouseArea.WHImportOrderDetailManager.ItemStatus.Shelfed;
                                item.ShelfID = itemdetail.ID;
                                item.ShelfName = itemdetail.ItemDetailName;
                                item.IsSetShelf = 1;
                            }

                            // Update to DB
                            int result = new FEA_BusinessLogic.WarehouseArea.WHImportOrderDetailManager().UpdateShelf(lstToCheck, ItemValue);
                            if (result == 0)
                            {
                                ViewData["EditError"] = "Cannot update data";
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    ViewData["EditError"] = ex.Message;
                }
            }

            return GetListPartial();
        }

        #region "List"

        public ActionResult ApproveList()
        {
            Session["BDate"] = DateTime.Now.AddMonths(-1);
            Session["EDate"] = DateTime.Now;
            Session["OCode"] = "";

            if (Request.Form["btnQuery"] != null)
            {
                Session["BDate"] = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtBeginDate");
                Session["EDate"] = DevExpress.Web.Mvc.EditorExtension.GetValue<DateTime>("dtEndDate");
                Session["OCode"] = DevExpress.Web.Mvc.EditorExtension.GetValue<string>("txtOrderCode");
                if (Session["OCode"] == null) Session["OCode"] = "";
            }

            ViewBag.BeginDate = Session["BDate"];
            ViewBag.EndDate = Session["EDate"];
            ViewBag.OrderCode = Session["OCode"];
            return View();
        }

        [ValidateInput(false)]
        public ActionResult ApprovePartial()
        {
            DateTime BDate = Convert.ToDateTime(Session["BDate"]);
            DateTime EDate = Convert.ToDateTime(Session["EDate"]);
            String sOrderCode = Session["OCode"].ToString();

            List<WHImportOrder> lst;
            lst = new FEA_BusinessLogic.WarehouseArea.WHImportOrderManager().GetItems(sOrderCode,BDate,EDate, -1, -1);
            return GetGridView(lst, Models.Helper.PartialParameter.WHImporterApproveList);
        }
        #endregion
    }
}
