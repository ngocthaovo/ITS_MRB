using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Models
{
    public class ItemUpload
    {
        public string Guid { get; set; }
        public string[] ListAddress { get; set; }
        public string Type { get; set; }
    }
}