using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.ITSAsset
{
    public class ITSAssetFTYManager:Base.Connection
    {

        public enum AssetType
        {
            Division = 1,
            Department = 2,
            Section =3,
            Asset =4
        }

        public List<ITSAssetFTY> GetItems(int AssetType, int Status, string ParentID)
        {
            return db.ITSAssetFTies.Where(i =>
                        (AssetType > -1 ? i.Type == AssetType : true)
                        && (Status > -1 ? i.Status == Status : true)
                        && (ParentID.Length == 0 ? true : i.ParentID == ParentID)
                    ).ToList();
        }


        public static string CheckAndInsertData(string Name,int AssetType,FEA_BusinessLogic.FEA_ITSEntities dbEntity, out string sError)
        {
            sError = "";

            try
            {
                var item = dbEntity.ITSAssetFTies.Where(i => ((i.Name.Trim().ToLower() == Name.Trim().ToLower()))
                                                     && (i.Type == AssetType)
                                                 ).SingleOrDefault();

                if (item == null)
                {
                    ITSAssetFTY itemNew = new ITSAssetFTY() {
                    ID = Guid.NewGuid().ToString(),
                    Name = Name,
                    Status =1,
                    Type = AssetType
                    };

                    dbEntity.ITSAssetFTies.Add(itemNew);
                    dbEntity.SaveChanges();

                    return itemNew.ID;
                }
                return item.ID;
            }
            catch(Exception ex)
            {
                sError = ex.Message;
                return "";
            }
        }
    }
}
