using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEA_BusinessLogic.ITSAsset
{
    public class ITSAssetRecDataManager : Base.Connection
    {


        public List<ITSAssetRecData> GetItems(string Name, int status)
        {
            return db.ITSAssetRecDatas.Where(i=>
                                                (i.Name.Contains(Name))
                                                && (status > -1? i.Status == status:true)
                                                ).ToList();
        }
        public static string CheckAndInsertData(string Code, string Name, FEA_BusinessLogic.FEA_ITSEntities dbEntity, out string sError)
        {
            sError = "";

            try
            {
                var item = dbEntity.ITSAssetRecDatas.Where(i => 
                                                    (i.Name.Trim().ToLower() == Name.Trim().ToLower())
                                                     && (i.Code.Contains(Code))
                                                 ).SingleOrDefault();

                if (item == null)
                {
                    ITSAssetRecData itemNew = new ITSAssetRecData()
                    {
                        ID = Guid.NewGuid().ToString(),
                        Name = Name,
                        Status = 1,
                        Code = Code
                    };

                    dbEntity.ITSAssetRecDatas.Add(itemNew);
                    dbEntity.SaveChanges();

                    return itemNew.ID;
                }
                return item.ID;
            }
            catch (Exception ex)
            {
                sError = ex.Message;
                return "";
            }
        }
    }
}
