using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.WarehouseArea
{
    public class HistoryScanManager:Base.Connection
    {
        public enum OperationType
        {
           IN,
           OUT,
           REMOVED
        }

        public HistoryScan BindItem(PackingManifestDetail _PackingManifestDetail, string ObjectScan, string OperationType, int CurrentUserID, string Notes)
        {
                 HistoryScan _HistoryScan = new HistoryScan();
                _HistoryScan.ID = Guid.NewGuid().ToString();
                _HistoryScan.PackingManifestDetailID = _PackingManifestDetail.ID;
                _HistoryScan.ObjectScan = ObjectScan;
                _HistoryScan.OperationType = OperationType;
                _HistoryScan.CreatorID = CurrentUserID;
                _HistoryScan.CreateDate = DateTime.Now;
                _HistoryScan.PackingManifestID = _PackingManifestDetail.PackingManifest.ID;
                _HistoryScan.SerialNo = _PackingManifestDetail.SerialNo;
                _HistoryScan.TEMP3 = Notes;
                return _HistoryScan;
        }

        public List<HistoryScan> GetItemList(string PackingManifestDetailID, string ObjectScan)
        {
            return db.HistoryScans.Where(i =>
                                            (i.PackingManifestDetailID == PackingManifestDetailID)
                                            &&(ObjectScan.Length == 0?true:i.ObjectScan == ObjectScan) 
                                            ).OrderByDescending(i=>i.CreateDate).ToList();
        }
    }
}
