using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using FEA_BusinessLogic;
using FEA_BusinessLogic.WarehouseArea;
namespace WarehouseArea.ExcelReader
{
    public class ColumnIdentifier
    {
        //060: Columbia
        //078: FIla
        //C34: NIKE
        //UA7: UNDERAMOUR

        public static string CustomerCode { get; set; }

        public static string Range { get { return "Range"; } }
        public static int Range_Index { get { return 0; } }

        public static string SerialFrom { get { return "Serial From"; } }
        public static int SerialFrom_Index { get { return 1; } }

        public static string SerialTo { get { return "Serial To"; } }
        public static int SerialTo_Index { get { return 2; } }

        public static string PackCode { get { return "Pack Instruction Code"; } } //Pack Code
        public static int PackCode_Index { get { return 3; } }

        public static string LineNumber { get { return "Line #"; } }
        public static int LineNumber_Index { get { return 4; } }

        public static string BuyerItemNumber { get { return "Buyer Item #"; } }
        public int BuyerItemNumber_Index { get { return 5; } }

        public static string SKUNumber { get { return "SKU #"; } }
        public static int SKUNumber_Index { get { return 6; } }

        public static string PONumber { get { return "PO #"; } }
        public static int PONumber_Index { get { return 7; } }

        public static string MainLine { get {

            if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia.ToString() || CustomerCode == PackingManifestManager.CustomerCodeType.UnderAmour.ToString()) return "Line #";
            else if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString()) return "";

            return "MAIN LINE #"; 
        
        } }
        public static int MainLine_Index { get { return 8; } }


        public static string ColorName { get {

            if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia.ToString() || CustomerCode == PackingManifestManager.CustomerCodeType.UnderAmour.ToString()) return "Color Code";
            else if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString()) return "Color Description";

            return "Color Name";
        
        } }
        public static int ColorName_Index { get { return 9; } }

        public static string Size { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString()) return "Size Desc.";
            return "Size"; 
        } }
        public static int Size_Index { get { return 10; } }

        public static string ShortDescription { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString()) return "Vendor Style";
            return "Short Description";
        } }
        public static int ShortDescription_Index { get { return 11; } }


        public static string ShipmentMethod { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString()) return "";
            return "Shipment Method";
        } }
        public static int ShipmentMethod_Index { get { return 12; } }

        public static string ItemQty { get { return "Item Qty"; } }
        public static int ItemQty_Index { get { return 13; } }

        public static string ItemQtyPerCtn_Pack { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia.ToString() || CustomerCode == PackingManifestManager.CustomerCodeType.UnderAmour.ToString()) return "Qty per Pkg/Inner Pack";
            else if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString()) return "Qty per Pkg/Inner Pack";
            return "Qty per Pkg/Inner Pack";
        
        } }//Item Qty Per Ctn / Pack

        public static int ItemQtyPerCtn_Pack_Index { get { return 14; } }

        public static string InnerPkgCount { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia.ToString() || CustomerCode == PackingManifestManager.CustomerCodeType.UnderAmour.ToString()) return "Inner Pkg Count";
            else if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString()) return "Inner Pkg Count";
            return "Inner Pkg Count";
        
        } }
        public static int InnerPkgCount_Index { get { return 15; } }

        public static string CtnCount { get { return "Pkg Count"; } }//Ctn Count
        public static int CtnCount_Index { get { return 16; } }

        public static string R { get { return "R"; } }
        public static int R_Index { get { return 17; } }

        public static string CtnCode { get { return "Pkg Code"; } }//Ctn Code
        public static int CtnCode_Index { get { return 18; } }

        public static string NetNet { get { return "Net Net"; } }
        public static int NetNet_Index { get { return 19; } }

        public static string Net { get { return "Net"; } }
        public static int Net_Index { get { return 20; } }

        public static string Gross { get { return "Gross"; } }
        public static int Gross_Index { get { return 21; } }

        public static string Unit { get { return "Unit"; } }
        public static int Unit_Index { get { return 22; } }

        public static string L { get { return "L"; } }
        public static int L_Index { get { return 23; } }

        public static string W { get { return "W"; } }
        public static int W_Index { get { return 24; } }

        public static string H { get { return "H"; } }
        public static int H_Index { get { return 25; } }

        public static string Unit2 { get { return "Unit_1"; } }
        public static int Unit2_Index { get { return 26; } }

        //Modify by jason 2016/06/25
        public static string MaterialStyle { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia.ToString() || CustomerCode == PackingManifestManager.CustomerCodeType.UnderAmour.ToString()) return "Material Style";
            return "";
        } }
        public static int MaterialStyle_Index { get { return 27; } }

        public static string CustomerCrossReferenceNumber { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia.ToString() || CustomerCode == PackingManifestManager.CustomerCodeType.UnderAmour.ToString()) return "Customer Cross Reference Number";
            return "";
        } }
        public static int CustomerCrossReferenceNumber_Index { get { return 28; } }

        public static string UPC_EAN { get {
            if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila.ToString())  return "UPC/EAN (GTIN)";
            return "";
        } }
        public static int UPC_EAN_Index { get { return 29; } }
        
    }
    public class ConvertDataModel
    {

        long SerialFrom, SerialTo;

        List<PackingManifestDetail> lstPackingTMP;
        List<PackingManifestDetailCoordination> lstPackingCoordinationTMP;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="CustomerCode"></param>
        /// 
        public ConvertDataModel(string CustomerCode)
        {
            ColumnIdentifier.CustomerCode = CustomerCode;
        }

        public FEA_BusinessLogic.PackingManifest ConvertData(DataTable table, string sOrderCode, string sCustomerPO, string sPoNumber, string InvoiveNumber, int isCoordinate, out string sError)
        {
            sError = "";
            if (!FirstValidate(sPoNumber, table, out sError))
                return null;

            string PackingManifestID = Guid.NewGuid().ToString();
            PackingManifest lstReturn = new PackingManifest()
            {
                ID = PackingManifestID,
                OrderCode = sOrderCode,
                CustomerPO = sCustomerPO,
                PONo = sPoNumber,
                InvoiceNo = InvoiveNumber,
                Description = "",
                isCoordinate = isCoordinate,
                CustomerCode = ColumnIdentifier.CustomerCode
            };

            List<PackingManifestDetail> lstData = new List<PackingManifestDetail>();
            int counter = 0;
            foreach (DataRow _row in table.Rows)
            { 
                try
                {

                    if (isCoordinate == 0) // không là hàng phối
                    {
                        lstData.AddRange(ConvertRow(_row, PackingManifestID,isCoordinate));
                    }
                    else // là hàng phối
                    {
                        string sSerialFrom = "", sSerialTo = "";

                        sSerialFrom = _row[ColumnIdentifier.SerialFrom].ToString();
                        sSerialTo = _row[ColumnIdentifier.SerialTo].ToString();

                        if (sSerialFrom.Trim().Length != 0 && sSerialTo.Length != 0)
                        {
                            SerialFrom = Int64.Parse(sSerialFrom);
                            SerialTo = Int64.Parse(sSerialTo);

                            lstPackingTMP = ConvertRow(_row, PackingManifestID,isCoordinate);
                        }
                        else
                        {
                            lstPackingTMP = RepairPackingManifestDetailList(lstPackingTMP, _row);
                        }


                        // khối hàng phối kế tiếp
                        Boolean flag = false;
                        if ((counter + 1) >= table.Rows.Count)
                            flag = true;
                        else
                        {
                            sSerialFrom = table.Rows[counter +1 ][ColumnIdentifier.SerialFrom].ToString();
                            sSerialTo = table.Rows[counter + 1][ColumnIdentifier.SerialTo].ToString();

                            if (sSerialFrom.Trim().Length != 0 && sSerialTo.Trim().Length != 0)
                                flag = true;
                        }

                        if (flag)
                        { 
                            lstData.AddRange(lstPackingTMP);
                            lstPackingTMP = new List<PackingManifestDetail>();
                            lstPackingCoordinationTMP = new List<PackingManifestDetailCoordination>();
                        }

                    }
                    sError = "";
                }
                catch (Exception ex)
                {
                    sError ="Lỗi xảy ra - Vui lòng kiểm tra lại số Serial hoặc xem có phải đây là hàng phối hay không!...</br>" +  ex.Message + "</br>" + ex.StackTrace;
                    return null;
                }
                counter += 1;
            }

            lstReturn.PackingManifestDetails = lstData;
            if (!FinalValidate(lstReturn, out sError))
            {
                return null;
            }

            return lstReturn;
        }

        private List<PackingManifestDetail> ConvertRow(DataRow e, string PackingManifestID, int isCoordinate)
        {
            List<PackingManifestDetail> lstDetail = new List<PackingManifestDetail>();

            if(isCoordinate == 0)
            { 
                SerialFrom = Int64.Parse(e[ColumnIdentifier.SerialFrom].ToString());
                SerialTo = Int64.Parse(e[ColumnIdentifier.SerialTo].ToString());
            }

            if (SerialFrom > SerialTo)
                throw new Exception("Barcode Serial inValid(Serial From must be less than Serial To) at: " + SerialFrom + "-" + SerialTo);


            PackingManifestDetail item;
            for (long repear = SerialFrom; repear <= SerialTo; repear++)
            {
                item = new PackingManifestDetail();

                item.ID = Guid.NewGuid().ToString();
                item.PackingManifestID = PackingManifestID;
                item.SerialNo = (long)repear;
                item.RANGE = e.Field<string>(ColumnIdentifier.Range);
                item.PackCode = e[ColumnIdentifier.PackCode].ToString();
                item.Line_ = e.Field<string>(ColumnIdentifier.LineNumber);
                item.BuyerItem_ = e[ColumnIdentifier.BuyerItemNumber].ToString();
                item.SKU_ = e[ColumnIdentifier.SKUNumber].ToString();
                item.PO_ = e[ColumnIdentifier.PONumber].ToString();

                if (ColumnIdentifier.MainLine != "") //Edit by Jason 2015/06/26
                    item.MainLine_ = e[ColumnIdentifier.MainLine].ToString();

                item.ColorName = e[ColumnIdentifier.ColorName].ToString();
                item.Size = e[ColumnIdentifier.Size].ToString();
                item.ShortDescription = e[ColumnIdentifier.ShortDescription].ToString();

                if (ColumnIdentifier.ShipmentMethod != "") //Edit by Jason 2015/06/26
                    item.ShipmentMethod = e[ColumnIdentifier.ShipmentMethod].ToString();

                item.ItemQtyPerCtnPack = (DBNull.Value == e[ColumnIdentifier.ItemQtyPerCtn_Pack]) ? 0 : int.Parse(e[ColumnIdentifier.ItemQtyPerCtn_Pack].ToString());

                item.ItemQuantity = item.ItemQtyPerCtnPack;// (DBNull.Value == e[ColumnIdentifier.ItemQty]) ? 0 : int.Parse(e[ColumnIdentifier.ItemQty].ToString());// Custom there !important

                item.InnetPkgCount = (DBNull.Value == e[ColumnIdentifier.PackCode] || e[ColumnIdentifier.PackCode].ToString() == "") ? 0 : int.Parse(e[ColumnIdentifier.InnerPkgCount].ToString());
                // Ctn count
                item.R = e[ColumnIdentifier.R].ToString();
                item.CtnCode = e[ColumnIdentifier.CtnCode].ToString();
                item.NETNET = e[ColumnIdentifier.NetNet].ToString();
                item.NET = e[ColumnIdentifier.Net].ToString();
                item.GROSS = e[ColumnIdentifier.Gross].ToString();
                item.UNIT = e[ColumnIdentifier.Unit].ToString();
                item.L = e[ColumnIdentifier.L].ToString();
                item.W = e[ColumnIdentifier.W].ToString();
                item.H = e[ColumnIdentifier.H].ToString();
                item.UNIT2 = e[ColumnIdentifier.Unit2].ToString();

                //Edit by Jason 2015/06/26
                if (ColumnIdentifier.MaterialStyle != "")
                    item.MaterialStyle = e[ColumnIdentifier.MaterialStyle].ToString();
                if (ColumnIdentifier.CustomerCrossReferenceNumber != "")
                    item.CustomerCrossReferenceNumber = e[ColumnIdentifier.CustomerCrossReferenceNumber].ToString();
                if (ColumnIdentifier.UPC_EAN != "")
                    item.UPC_EAN = e[ColumnIdentifier.UPC_EAN].ToString();
                //

                item.isCOnfirm = 0;
                item.isStockin = 0;
                item.TEMP1 = item.TEMP2 = item.TEMP3 = "";
                item.isReturns = 0;
                item.STATUS = (int)FEA_BusinessLogic.WarehouseArea.PackingManifestManager.OrderStatus.SAVED;


                if (isCoordinate == 1)
                {
                    item.PackingManifestDetailCoordinations.Add(ConvertToCoordination(item, e));
                }

                lstDetail.Add(item);
            }

            return lstDetail;
        }


        private List<PackingManifestDetail> RepairPackingManifestDetailList(List<PackingManifestDetail> lst, DataRow e)
        {
            foreach(PackingManifestDetail item_detail in lst)
            {
                item_detail.PackingManifestDetailCoordinations.Add(ConvertToCoordination(item_detail,e));
            }

            return lst;
        }

        // Hàn phối
        private PackingManifestDetailCoordination ConvertToCoordination( PackingManifestDetail item_detail, DataRow e)
        {
            List<PackingManifestDetailCoordination> lst = new List<PackingManifestDetailCoordination>();

            PackingManifestDetailCoordination item;

            item = new PackingManifestDetailCoordination();

            item.ID = Guid.NewGuid().ToString();
            item.PackingManifestDetailID = item_detail.ID;
            item.SerialNo = item_detail.SerialNo;
            item.RANGE = item_detail.RANGE;
            item.PackCode = item_detail.PackCode;
            item.Line_ = e.Field<string>(ColumnIdentifier.LineNumber);
            item.BuyerItem_ = e[ColumnIdentifier.BuyerItemNumber].ToString();
            item.SKU_ = item_detail.SKU_;
            item.PO_ = e[ColumnIdentifier.PONumber].ToString();

            if (ColumnIdentifier.MainLine != "") //Edit by Jason 2015/06/26
                item.MainLine_ = e[ColumnIdentifier.MainLine].ToString();

            item.ColorName = e[ColumnIdentifier.ColorName].ToString();
            item.Size = e[ColumnIdentifier.Size].ToString();
            item.ShortDescription = e[ColumnIdentifier.ShortDescription].ToString();

            if (ColumnIdentifier.ShipmentMethod != "") //Edit by Jason 2015/06/26
                item.ShipmentMethod  = e[ColumnIdentifier.ShipmentMethod].ToString();

            item.ItemQtyPerCtnPack = (DBNull.Value == e[ColumnIdentifier.ItemQtyPerCtn_Pack]) ? 0 : int.Parse(e[ColumnIdentifier.ItemQtyPerCtn_Pack].ToString());

            item.ItemQuantity = item.ItemQtyPerCtnPack;// (DBNull.Value == e[ColumnIdentifier.ItemQty]) ? 0 : int.Parse(e[ColumnIdentifier.ItemQty].ToString());// Custom there !important

            item.InnetPkgCount = item_detail.InnetPkgCount;
            // Ctn count
            item.R = item_detail.R;
            item.CtnCode = item_detail.CtnCode;
            item.NETNET = item_detail.NETNET;
            item.NET = item_detail.NET;
            item.GROSS = item_detail.GROSS;
            item.UNIT = item_detail.UNIT;
            item.L = item_detail.L;
            item.W = item_detail.W;
            item.H = item_detail.H;
            item.UNIT2 = item_detail.UNIT2;

            //Edit by Jason 2015/06/26
            if (ColumnIdentifier.MaterialStyle != "")
                item.MaterialStyle = e[ColumnIdentifier.MaterialStyle].ToString();
            if (ColumnIdentifier.CustomerCrossReferenceNumber != "")
                item.CustomerCrossReferenceNumber = e[ColumnIdentifier.CustomerCrossReferenceNumber].ToString();
            if (ColumnIdentifier.UPC_EAN != "")
                item.UPC_EAN = e[ColumnIdentifier.UPC_EAN].ToString();
            //

            return item;
        }


        private bool FirstValidate(string sPoNumber, DataTable source, out string sError)
        {
            sError = "";
            if (sPoNumber == null || sPoNumber.Length == 0)
                return true;
            try
            {
                //Validate PO NUmber
                int count = source.Select(String.Format("convert([{0}], 'System.Int64')={1}", ColumnIdentifier.PONumber, sPoNumber)).Count();
                if (count != source.Rows.Count)
                {
                    sError = Resources.Resource.msgInvalidCusNumber;
                    return false;
                }
            }
            catch (Exception ex)
            {
                sError = String.Format(Resources.Resource.msgInvalidCusNumber + " - {0} - {1}", ex.Message, ex.StackTrace);
                return false;
            }

            return true;
        }

        private bool FinalValidate(PackingManifest data, out string sError)
        {
            sError = "";
            if (data.PackingManifestDetails != null)
            {
                var lstTmp = data.PackingManifestDetails.GroupBy(p => p.SerialNo)
                   .Select(g => new { SerialNo = g.Key, Count = g.Count() });

                lstTmp = lstTmp.Where(i => i.Count != 1).ToList();

                if (lstTmp.Count() > 0)
                {

                    sError = Resources.Resource.msgWHSerialDuplicate + string.Join(",\n", lstTmp);
                    return false;
                }

            }

            return true;
        }
    }
}