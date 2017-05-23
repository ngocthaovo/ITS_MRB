using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.WarehouseArea
{


    public class WHExportOrderDetailManager : Base.Connection
    {

        public enum ExportStatus
        {
            Exported = 1,
            UnExported = 0

        }
        public enum ReturnStatus
        {
            Returned = 1,
            UnReturned = 0

        }
        public Dictionary<string,string> GetItemByCustomerPO(string CustomerPO, out string CustomerCode)
        {

            // Get customer code by CustomerPO

            CustomerCode = GetCustomerCodeByCusPO(CustomerPO);
            if(CustomerCode != "" && CustomerCode .Trim().Length > 0)
            {

                // If Cusotmer belong Nike, item type will get by MainLine (Item)
                if (CustomerCode == PackingManifestManager.CustomerCodeType.Nike)
                {
                    return db.PackingManifestDetails
                             .Where(i => i.PackingManifest.CustomerPO == CustomerPO 
                                    && (i.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                    && (i.PackingManifest.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                   &&(i.MainLine_ != null) // Edited by jason (2015/06/30)
                                   )
                            
                             .GroupBy(p => p.MainLine_)
                             .Select(g => new { g.Key, Count = g.Count() })
                             .ToDictionary(p => p.Key, p => p.Key);
                }
                else if (CustomerCode == PackingManifestManager.CustomerCodeType.Columbia || CustomerCode==PackingManifestManager.CustomerCodeType.UnderAmour)
                // If Cusotmer belong Columbia, item type will get by Color
                {
                    return db.PackingManifestDetails
                             .Where(i => i.PackingManifest.CustomerPO == CustomerPO 
                                    && (i.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                    && (i.PackingManifest.STATUS != (int)WarehouseArea.PackingManifestManager.OrderStatus.DELETED)
                                   &&(i.ColorName != null) // Edited by jason (2015/06/30)
                                   )
                            
                             .GroupBy(p => p.ColorName)
                             .Select(g => new { g.Key})
                             .ToDictionary(p => p.Key, p => p.Key);
                }
                else if (CustomerCode == PackingManifestManager.CustomerCodeType.Fila)
                {
                    // If Customer belong to Fila, we will return empty data
                    return new Dictionary<string, string>();
                }
            }
            return new Dictionary<string, string>();
        }


        #region common function
        private string GetCustomerCodeByCusPO(string cusPO)
        {
            var result = db.PackingManifests.Where(i => 
                i.CustomerPO == cusPO 
                && (i.STATUS != (int)WHExportOrderManager.OrderStatus.DELETED))
                .Select(s => new { CustomerCode = s.CustomerCode }).FirstOrDefault();
            return (result == null ? "" : result.CustomerCode.ToString());
          
        }
        #endregion
    }
}
