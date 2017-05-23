using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEA_BusinessLogic.WarehouseArea;
using System.Transactions;
namespace FEA_BusinessLogic.WarehouseArea
{
    public class PackingManifestDetailManager:Base.Connection
    {
        /// <summary>
        /// Get List by  Creator, this function using for user
        /// </summary>
        /// <param name="iCreatorID"></param>
        /// <returns></returns>
        public List<PackingManifestDetail> GetItems(string PackingManifestID)
        {
            return db.PackingManifestDetails.Where(i => (i.PackingManifestID == PackingManifestID)
                                                         &&(i.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                                        ).OrderBy(i => i.SerialNo).ToList();
        }

        public List<PackingManifestDetail> GetPackingManifestDetailForScan(string CustomerPO)
        {
            return db.PackingManifestDetails.Where(i => i.PackingManifest.CustomerPO == CustomerPO && i.PackingManifest.STATUS == 1 && i.PackingManifest.isConfirm == 1 && i.STATUS == 1).OrderBy(x => x.SerialNo).ToList();
        }
        /// <summary>
        /// Already Confirmed, not imported
        /// </summary>
        /// <param name="CustomerPO"></param>
        /// <returns></returns>
        public List<PackingManifestDetail> GetConfirmedPackingManifestDetailForScan(string CustomerPO)
        {
            return db.PackingManifestDetails.Where(i => i.PackingManifest.CustomerPO == CustomerPO && i.isCOnfirm == 1 && i.isStockin == 0 && i.PackingManifest.STATUS == 1 && i.PackingManifest.isConfirm == 1 && i.STATUS == 1).OrderBy(x => x.SerialNo).ToList();
        }
        /// <summary>
        /// Still not confirmed, not imported
        /// </summary>
        /// <param name="CustomerPO"></param>
        /// <returns></returns>
        public Object GetNotConfirmedPackingManifestDetailForScan(string CustomerPO)
        {
            return db.PackingManifestDetails.Where(i => i.PackingManifest.CustomerPO == CustomerPO && i.isCOnfirm == 0 && i.isStockin == 0 && i.PackingManifest.STATUS == 1 && i.PackingManifest.isConfirm == 1 && i.STATUS == 1).Select(x => new
            {
                x.RANGE,
                x.SerialNo,
                //   x.PackCode,
                x.BuyerItem_,
                //    x.SKU_,
                x.MainLine_,
                x.Line_,
                x.PO_,
                x.ColorName,
                x.Size,
                //   x.ShortDescription,
                x.ShipmentMethod,
                x.ItemQuantity,
                //  x.ItemQtyPerCtnPack,
                //x.InnetPkgCount,
                //   x.R,
                x.CtnCode,
                //  x.NETNET,
                //  x.NET,
                //  x.GROSS,
                //  x.UNIT,
                //  x.L,
                //  x.W,
                //   x.H,
                //   x.UNIT2,
                x.isCOnfirm,
                x.isStockin,
                x.STATUS
            }).OrderBy(x => x.SerialNo).ToList();
        }
        /// <summary>
        /// Already confirmed, not imported
        /// </summary>
        /// <param name="CustomerPO"></param>
        /// <returns></returns>
        public Object GetNotImportedPackingManifestDetailForScan(string CustomerPO)
        {
            return db.PackingManifestDetails.Where(i => i.PackingManifest.CustomerPO == CustomerPO && i.isCOnfirm == 1 && i.isStockin == 0 && i.PackingManifest.STATUS == 1 && i.PackingManifest.isConfirm == 1 && i.STATUS == 1).Select(x => new
            {
                x.RANGE,
                x.SerialNo,
                //   x.PackCode,
                x.BuyerItem_,
                //    x.SKU_,
                x.MainLine_,
                x.Line_,
                x.PO_,
                x.ColorName,
                x.Size,
                //   x.ShortDescription,
                x.ShipmentMethod,
                x.ItemQuantity,
                //  x.ItemQtyPerCtnPack,
                //x.InnetPkgCount,
                //   x.R,
                x.CtnCode,
                //  x.NETNET,
                //  x.NET,
                //  x.GROSS,
                //  x.UNIT,
                //  x.L,
                //  x.W,
                //   x.H,
                //   x.UNIT2,
                x.isCOnfirm,
                x.isStockin,
                x.STATUS
            }).OrderBy(x => x.SerialNo).ToList();
        }
         /// <summary>
         /// 
         /// </summary>
         /// <param name="CustomerPO"></param>
         /// <returns></returns>
        public Object GetImportedPackingManifestDetailForScan(string CustomerPO)
        {
            return db.PackingManifestDetails.Where(i => i.PackingManifest.CustomerPO == CustomerPO && i.isCOnfirm == 1 && i.isStockin == 1 && i.PackingManifest.STATUS == 1 && i.PackingManifest.isConfirm == 1 && i.STATUS == 1).Select(x => new
            {
                x.RANGE,
                x.SerialNo,
                //   x.PackCode,
                x.BuyerItem_,
                //    x.SKU_,
                x.MainLine_,
                x.Line_,
                x.PO_,
                x.ColorName,
                x.Size,
                //   x.ShortDescription,
                x.ShipmentMethod,
                x.ItemQuantity,
                //  x.ItemQtyPerCtnPack,
                //x.InnetPkgCount,
                //   x.R,
                x.CtnCode,
                //  x.NETNET,
                //  x.NET,
                //  x.GROSS,
                //  x.UNIT,
                //  x.L,
                //  x.W,
                //   x.H,
                //   x.UNIT2,
                x.isCOnfirm,
                x.isStockin,
                x.STATUS
            }).OrderBy(x => x.SerialNo).ToList();
        }

        public List<PackingManifestDetail> GetPackingManifestExported(string CustomerPO)
        {
            return db.PackingManifestDetails.Where(i =>
                                            (i.PackingManifest.CustomerPO == CustomerPO)
                                            && (i.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                            && (i.PackingManifest.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                            && (i.WHExportOrderDetails.Where(wh => (wh.WHExportOrder.Status != (int)WHExportOrderManager.OrderStatus.DELETED) && wh.IsExported == 1).Count() == 0 ? false : true)
                ).ToList();
        }

        public Object[] GetPackingManifestForScan(string CustomerPO)
        {
            Object[] _PackingManifest = new Object[3];
            _PackingManifest[0] = (PackingManifest)new PackingManifestManager().GetItem(CustomerPO);
            _PackingManifest[1] = (List <PackingManifestDetail>)GetPackingManifestDetailForScan(CustomerPO);
            _PackingManifest[2] = GetPackingManifestExported(CustomerPO);
            //_PackingManifest[3] = GetNotConfirmedPackingManifestDetailForScan(CustomerPO);
            //_PackingManifest[4] = GetNotImportedPackingManifestDetailForScan(CustomerPO);
            //_PackingManifest[5] = GetImportedPackingManifestDetailForScan(CustomerPO);
            return _PackingManifest;
        }


        public List<PackingManifestDetail> Query(string PONumber, string CusPO,DateTime? dtFrom,DateTime? dtTo,string TypeQuery,string Color, string Size, int OrderState)
        {
           // db.Configuration.LazyLoadingEnabled = false;

            if (dtFrom != null) dtFrom = dtFrom.Value.Date;
            if (dtTo != null) dtTo = new DateTime(dtTo.Value.Year, dtTo.Value.Month,dtTo.Value.Day,23,59,59);


            List<PackingManifestDetail> X = new List<PackingManifestDetail>();
            switch (TypeQuery)
            {
                case "Normal":
                    X = db.PackingManifestDetails.Where(i => i.PackingManifest.PONo.Contains(PONumber)
                                                       && i.PackingManifest.CustomerPO.Contains(CusPO)
                                                       && (dtFrom == null ? true : i.PackingManifest.CreateDate.Value >= dtFrom)
                                                       && (dtTo == null ? true : i.PackingManifest.CreateDate.Value <= dtTo)
                                                       && (i.STATUS != (int)PackingManifestManager.OrderStatus.DELETED)
                                                       && i.PackingManifest.STATUS != (int)PackingManifestManager.OrderStatus.DELETED
                                                       ).OrderBy(z => z.SerialNo).ToList();
                    break;
                case "Dynamic":
                    X =  db.PackingManifestDetails.Where(i => i.PackingManifest.PONo.Contains(PONumber)
                                                      && i.PackingManifest.CustomerPO.Contains(CusPO)
                                                      && i.ColorName == Color
                                                      && i.Size == Size
                                                      && (dtFrom == null ? true : i.PackingManifest.CreateDate.Value >= dtFrom)
                                                      && (dtTo == null ? true : i.PackingManifest.CreateDate <= dtTo)
                                                      && (i.STATUS != (int)PackingManifestManager.OrderStatus.DELETED)
                                                      && i.PackingManifest.STATUS != (int)PackingManifestManager.OrderStatus.DELETED
                                                      && i.PackingManifest.isConfirm == (int)PackingManifestManager.ConfirmStatus.CONFIRMED
                                                      ).OrderBy(i => i.SerialNo).ToList();
                
                    break;
                default:
                    break;
            }
            return X;
           
        }

        public PackingManifestDetail CheckBarCodeAvailable(string ID)
        {
            return db.PackingManifestDetails.Where(i => i.ID == ID && i.STATUS != 0 && i.PackingManifest.STATUS == 1 && i.PackingManifest.isConfirm == 1).SingleOrDefault();
        }
        public PackingManifestDetail CheckBarCodeAvailable(int BarCode,string CustPO)
        {
            return db.PackingManifestDetails.Where(i => i.SerialNo == BarCode &&i.PackingManifest.CustomerPO == CustPO && i.STATUS != 0 && i.PackingManifest.STATUS == 1 && i.PackingManifest.isConfirm == 1).SingleOrDefault();
        }

        public PackingManifestDetail UpdateScanBarcode(string UserGroup, string OperationType, int CurrentUserID, string Notes, int isReturn, string ID, out int Result)
        {

            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    PackingManifestDetail item = db.PackingManifestDetails.Where(i => (i.ID == ID)
                                                                                    && (i.STATUS != 0 && i.PackingManifest.STATUS == 1)
                                                                                    && (i.PackingManifest.isConfirm == 1)
                        //&& (i.WHExportOrderDetails.Where(a=>a.WHExportOrder.Status != (int)FEA_BusinessLogic.WarehouseArea.WHExportOrderManager.OrderStatus.DELETED).Count() == 0?true:false)
                                                                                    ).SingleOrDefault();

                    if (item != null)
                    {
                        item.isReturns = isReturn;
                        if (isReturn == 1)
                        {
                            // trong trường hợp trả về nếu thùng hàng đã xuất kho thì ko cho trả
                            var flag = (item.WHExportOrderDetails.Where(a => a.WHExportOrder.Status != (int)WHExportOrderManager.OrderStatus.DELETED && a.IsExported == 1).Count() == 0 ? true : false);
                            if (!flag)
                            {
                                Result = 2;     // Hang da xuat, khong the tra ve
                                return null;
                            }
                        }

                        switch (UserGroup)
                        {
                            case "Packing":
                                item.isCOnfirm = item.isCOnfirm == 0 ? 1 : 0;
                                
                                break;
                            case "Warehouse":
                                item.isStockin = item.isStockin == 0 ? 1 : 0;
                                //if (item.isStockin == 0)
                                //{
                                //    WHImportOrderDetail whItem = db.WHImportOrderDetails.Where(i => i.WHImportOrder.Status != 0 && i.PackingManifestDetailID == item.ID && i.Status != 3).;
                                //    if (whItem != null)
                                //    {
                                //        whItem.Status = 3;
                                //    } 
                                //}
      
                                break;
                            default:
                                break;
                        }
                        db.HistoryScans.Add(new HistoryScanManager().BindItem(item, UserGroup, OperationType, CurrentUserID, Notes));
                        PackingManifestDetail ItemOut = CheckBarCodeAvailable(ID);
                        db.SaveChanges();
                        scope.Complete();
                        Result = 1;
                        return ItemOut;
                    }
                }
                catch (Exception)
                {
                    scope.Dispose();
                }
            }
            Result = 0;
            return null;
        }


        public List<sp_GetDynamicQueryPackingManifest_Result> GetDynamicQueryPackingManifest(string PONumber, string CusPO, DateTime? dtFrom, DateTime? dtTo, int OrderState)
        {
            return db.sp_GetDynamicQueryPackingManifest(CusPO, PONumber, dtFrom, dtTo,OrderState).ToList();// OrderState = 1 (Open), 0 (Closed)
        }


        #region "WH Area Export"
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sCustomerPO"></param>
        /// <returns></returns>
        public List<PackingManifestDetail> GetItemDetailByItem(string CustomerPO, string ItemString,string CustomerCode, List<long> lstItemToCompare)
        {

            if (CustomerCode == PackingManifestManager.CustomerCodeType.Nike)
            {
                // If Cusotmer belong Nike, item type will get by MainLine (Item)
                return db.PackingManifestDetails.Where(i =>
                                                (i.PackingManifest.CustomerPO == CustomerPO)
                                                && ((ItemString == "" ? true : i.MainLine_ == ItemString)) // edited by jason - 2015/06/30

                                                && (i.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                                && (i.PackingManifest.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                                &&(!lstItemToCompare.Contains(i.SerialNo)) // added by jason
                                                    // &&(i.isCOnfirm == (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus.CONFIRMED) // Giờ thùng hàng chưa nhập kho vẫn làm dc phiếu xuất
                                                    // && (i.isStockin == (int) FEA_BusinessLogic.WarehouseArea.PackingManifestManager.SockinStatus.STOCKINED)
                                                && (i.WHExportOrderDetails.Where( // Xác nhận nếu Barcode này đã làm trong các phiếu xuất trước đó thì ko cho lấy nữa
                                                                                    wh =>
                                                                                        (wh.WHExportOrder.Status != (int)WHExportOrderManager.OrderStatus.DELETED)
                                                                                    ).Count() == 0 ? true : false)
                    ).ToList();
            }
            else if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia || CustomerCode==PackingManifestManager.CustomerCodeType.UnderAmour )
            {
                //If Cusotmer belong Columbia, item type will get by Color
                return db.PackingManifestDetails.Where(i =>
                                                (i.PackingManifest.CustomerPO == CustomerPO)
                                                && ((ItemString == "" ? true : i.ColorName == ItemString)) // edited by jason - 2015/06/30

                                                && (i.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                                && (i.PackingManifest.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                                && (!lstItemToCompare.Contains(i.SerialNo)) // added by jason
                                                    // &&(i.isCOnfirm == (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.ConfirmStatus.CONFIRMED) // Giờ thùng hàng chưa nhập kho vẫn làm dc phiếu xuất
                                                    // && (i.isStockin == (int) FEA_BusinessLogic.WarehouseArea.PackingManifestManager.SockinStatus.STOCKINED)
                                                && (i.WHExportOrderDetails.Where( // Xác nhận nếu Barcode này đã làm trong các phiếu xuất trước đó thì ko cho lấy nữa
                                                                                    wh =>
                                                                                        (wh.WHExportOrder.Status != (int)WHExportOrderManager.OrderStatus.DELETED)
                                                                                    ).Count() == 0 ? true : false)
                    ).ToList();
            }
            else if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila && ItemString == "") // xuất theo Item
            {
                return db.PackingManifestDetails.Where(i =>
                                                (i.PackingManifest.CustomerPO == CustomerPO)
                                                && (i.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                                && (i.PackingManifest.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                                && (!lstItemToCompare.Contains(i.SerialNo)) // added by jason
                                                && (i.WHExportOrderDetails.Where( // Xác nhận nếu Barcode này đã làm trong các phiếu xuất trước đó thì ko cho lấy nữa
                                                                                    wh =>
                                                                                        (wh.WHExportOrder.Status != (int)WHExportOrderManager.OrderStatus.DELETED)
                                                                                    ).Count() == 0 ? true : false)
                    ).ToList();
            }

            return null;
        }
        #endregion
    }
}
