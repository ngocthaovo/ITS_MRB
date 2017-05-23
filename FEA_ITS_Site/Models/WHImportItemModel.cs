using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Models
{
    public class WHImportItemModel
    {
        public string ID { get; set; }
        public string PackingManifestDetailID { get; set; }
        public bool IsChecked { get; set; }
        public string ShelfID { get; set; }
        public string ShelfName { get; set; }
        public string SerialNo { get; set; }
        public string CustomerPO { get; set; }
        public string Item { get; set; }
        public string ColorName { get; set; }
        public string Style { get; set; }
        public string Size { get; set; }
        public string Quantity { get; set; }
        public int? IsExported { get; set; }
        public int? IsReturned { get; set; }
        public int? IsSetShelf { get; set; }
    }
}