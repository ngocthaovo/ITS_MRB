using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.GenerateCode
{
    public class GenerateCode:Base.Connection
    {
        public List<FEA_BusinessLogic.TagPrefix> GetItem(string DocumentTypeName)
        {
            return db.TagPrefixes.Where(i => i.DocumentTypeName == DocumentTypeName).ToList();
        }

        public int GetDRForCheck(string OrderCode)
        {
            return db.DeviceRegistrations.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList().Count;
        }
        public int GetSIForCheck(string OrderCode)
        {
            return db.StockInEquipments.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList().Count;
        }
        public int GetSOForCheck(string OrderCode)
        {
            return db.StockOutEquipments.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList().Count;
        }
        public int GetWHAForCheck(string OrderCode)
        {
            return db.PackingManifests.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList().Count;
        }
        public int GetWHAExportForCheck(string OrderCode)
        {
            return db.WHExportOrders.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList().Count;
        }
        public int GetSAForCheck(string OrderCode)
        {
            return db.ExportItems.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList().Count;
        }
        public List<FEA_BusinessLogic.HardwareRequirement> GetPRForCheck(string OrderCode)
        {
            return db.HardwareRequirements.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList();
        }
        public int GetWHIForCheck(string OrderCode)
        {
            return db.WHImportOrders.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList().Count;
        }
        public List<FEA_BusinessLogic.GAItem> GetGAForCheck(string OrderCode)
        {
            return db.GAItems.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList();
        }
        //Added by Tony (2017-04-19)
        public List<FEA_BusinessLogic.MNRequestMain> GetMNForCheck(string OrderCode)
        {
            return db.MNRequestMains.Where(i => i.OrderCode.Trim() == OrderCode.Trim()).ToList();
        }
        public int GetMNSForCheck(string OrderCode, string strDocType, int iOrderType)
        {
            return db.MNStockEquipments.Where(i=>(i.OrderCode.Trim()==OrderCode.Trim()) && (i.DocType.Trim()==strDocType.Trim())
                && (i.OrderType== iOrderType)).ToList().Count;
        }
    }
}
