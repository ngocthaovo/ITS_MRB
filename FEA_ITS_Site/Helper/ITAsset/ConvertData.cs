using FEA_BusinessLogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Helper.ITAsset
{
    public class ConvertDataModel
    {

        public List<FEA_BusinessLogic.ITSAssetDetail> ConvertData(DataSet ds, out string sError)
        {
            sError = "";
            List<FEA_BusinessLogic.ITSAssetDetail> lstResult = new List<FEA_BusinessLogic.ITSAssetDetail>();
            try
            {
                if (ds.Tables.Count == 0)
                {
                    sError = "Not have data";
                    return null;
                }
                else
                {
                    foreach (DataTable dt in ds.Tables)
                    {

                        int rowIndex = 1;
                        foreach (DataRow dr in dt.Rows)
                        {
                            rowIndex += 1;
                            try
                            {
                                lstResult.Add(BindData(dr, dt.Columns.Count));
                            }
                            catch (Exception ex)
                            {
                                sError += "</br>Error at: " + dt.TableName + " department, row: " + rowIndex +", "+ ex.Message;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                sError += ex.Message;
                return null;
            }

            return lstResult;
        }


        private ITSAssetDetail BindData(DataRow e, int columnCount)
        {
            try
            {
                ITSAssetDetail item = new ITSAssetDetail();
                //check validate
                if (DBNull.Value.Equals(e[1]) || e[1].ToString().Length == 0)
                    throw new Exception("Devision not be blank");
                if (DBNull.Value.Equals(e[2]) || e[2].ToString().Length == 0)
                    throw new Exception("Department not be blank");
                if (DBNull.Value.Equals(e[3]) || e[3].ToString().Length == 0)
                    throw new Exception("Section not be blank");


                item.Division = (DBNull.Value == e[1]) ? "" : e[1].ToString().Trim(); //Division
                item.Department = (DBNull.Value == e[2]) ? "" : e[2].ToString().Trim(); //Department
                item.Section = (DBNull.Value == e[3]) ? "" : e[3].ToString().Trim(); //Section
                item.RecName = (DBNull.Value == e[4]) ? "" : e[4].ToString().Trim(); //Name
                item.RecCode = (DBNull.Value == e[5]) ? "" : e[5].ToString().Trim(); //Name

                item.AssetType = (DBNull.Value == e[6]) ? "" : e[6].ToString().Trim(); //Type
                item.Brand = (DBNull.Value == e[7]) ? "" : e[7].ToString().Trim(); //Model
                item.AssetID = (DBNull.Value == e[8]) ? "" : e[8].ToString().Trim(); //ASSETID
                item.AssetName = (DBNull.Value == e[9]) ? "" : e[9].ToString().Trim(); //Computer Name

                item.FactoryCode = (DBNull.Value == e[10]) ? "" : e[10].ToString().Trim(); //FactoryCode
                item.DivisionCode = (DBNull.Value == e[11]) ? "" : e[11].ToString().Trim(); //DivisionCode
                item.DepartmentCode = (DBNull.Value == e[12]) ? "" : e[12].ToString().Trim(); //DepartmentCode

                item.EquipDate = (DBNull.Value == e[13]) ? DateTime.Now : DateTime.Parse(e[13].ToString().Trim()); //EquipDate
                item.JobPosition = (DBNull.Value == e[14]) ? "" : e[14].ToString().Trim(); //JobPosition
                item.Group = (DBNull.Value == e[15]) ? "" : e[15].ToString().Trim(); //Group
                item.Model = (DBNull.Value == e[16]) ? "" : e[16].ToString().Trim(); //Model
                item.Configuration = (DBNull.Value == e[17]) ? "" : e[17].ToString().Trim(); //Configuration
                item.EmailCoding = (DBNull.Value == e[18]) ? "" : e[18].ToString().Trim(); //EmailCoding
                item.EmailAddress = (DBNull.Value == e[19]) ? "" : e[19].ToString().Trim(); //EmailAddress
                item.FJ_B_W = (DBNull.Value == e[20]) ? "" : e[20].ToString().Trim(); //FJ - B/W
                item.FJ_Color = (DBNull.Value == e[21]) ? "" : e[21].ToString().Trim(); //FJ_Color
                item.ExtNo = (DBNull.Value == e[22]) ? "" : e[22].ToString().Trim(); //ExtNo
                item.Code = (DBNull.Value == e[23]) ? "" : e[23].ToString().Trim(); //Code
                item.PassCode = (DBNull.Value == e[24]) ? "" : e[24].ToString().Trim(); //PassCode
                item.DirectLine = (DBNull.Value == e[25]) ? "" : e[25].ToString().Trim(); //DirectLine
                item.JobTitle = (DBNull.Value == e[26]) ? "" : e[26].ToString().Trim(); //JobTitle

                item.ID = Guid.NewGuid().ToString();
                item.UploadDate = DateTime.Now;
                item.Status = 1;
                item.CreatedByHand = 0;
                item.UploadBy = Helper.UserLoginInfo.UserId;
                return item;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}