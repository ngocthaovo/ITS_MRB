using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ITSProject.Ultility
{
    public class FileHelper
    {
        public static byte[] DownloadFile(String AttachmentLink, string FileName)
        {
            string RootValue = System.Configuration.ConfigurationManager.AppSettings["ITDocumentPath"].ToString();
            string SiteLink = System.Configuration.ConfigurationManager.AppSettings["SiteLink"].ToString();
            RootValue += AttachmentLink + @"\";

            string[] filePaths = Directory.GetFiles(RootValue);
            foreach (string file in filePaths)
            {
                if (Path.GetFileName(file) == FileName)
                {
                    System.IO.FileStream fs1 = null;
                    fs1 = System.IO.File.Open(RootValue + FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    //fs1 = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                    byte[] b1 = new byte[fs1.Length];
                    fs1.Read(b1, 0, (int)fs1.Length);
                    fs1.Close();
                    return b1; 
                }
            }

            return null;
        }
    }
}