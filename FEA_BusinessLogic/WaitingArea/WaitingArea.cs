using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FEA_BusinessLogic.WaitingArea
{
    public class WaitingArea : Base.Connection
    {
        //Added by Tony (2017-02-08)
        public List<sp_GetERPHistory_Result> GetERPHitory(string UserID, string OrderCode, string FEPOCode, string Status)
        {
            return db.sp_GetERPHistory(UserID,OrderCode,FEPOCode,Status).ToList();
        }
        //
        public List<sp_GetDocumentApprove_Result> GetDocument(int UserID)
        {
            return db.sp_GetDocumentApprove(UserID).ToList();
        }

        /// <summary>
        /// Get list of document signed by Manager
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<sp_GetDocumentSigned_Result> GetDocumentSigned(int UserID)
        {
            return db.sp_GetDocumentSigned(UserID).ToList();
        }

        /// <summary>
        /// Jason - Get num of document to sign for user
        /// </summary>
        /// <param name="iUserID"></param>
        /// <returns></returns>
        public int CountSignDocument(int iUserID)
        {
            return db.sp_GetDocumentApprove(iUserID).Count();
        }


        public List<sp_GetWFHistory_Result> GetHistory(string OrderCode)
        {
            return db.sp_GetWFHistory(OrderCode).ToList();
        }

        public bool UpdateEndWorkFlow(string OrderCode, string DocumentTypeName, string MainDetailID, string comments,string DelegateID,int? DelegateUserID)
        {
            bool _result = false;

            // -- Update WF Main - Update isFinished = 1
            WFMain WFM = new WFMain()
            {
                OrderCode = OrderCode,
                isFinished = 1
            };
            // -- Update WF Main Detail -Update IsFinished = 1,CheckDate,Comments
            WFMainDetail WFMD = new WFMainDetail()
            {
                MainDetailID = MainDetailID,
                isFinished = 1,
                CheckDate = DateTime.Now,
                Comment = comments,
                Temp1 = "0",  // -- Add reject status into Temp1 Column. 0: Dont have reject. 1: Reject
                Temp2 = DelegateUserID == 0 ? "" : DelegateUserID.ToString(),       // -- store Userid who was delegated.
                Temp3 = "",
                DelegateID = DelegateID == null || DelegateID == "" ? null : DelegateID,        // -- store DelegateID of WFDelegate table

            };
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    switch (DocumentTypeName)
                    {
                        case "DEVICEREGISTRATION":
                            DeviceRegistration o = new DeviceRegistration() { OrderCode = OrderCode, Status = 3 };
                            _result = new DeviceRegistrationManager().UpdateStatus(o, i => i.Status);
                            break;
                        case "HARDWAREREQUIREMENT":
                            HardwareRequirement h = new HardwareRequirement() { OrderCode = OrderCode, Status = 3 };
                            _result = new HardwareRequirementManager().UpdateStatus(h, i => i.Status);
                            break;
                        case "SECURITYAREA":
                            ExportItem e = new ExportItem() { OrderCode = OrderCode, Status = (int)FEA_SABusinessLogic.ExportItemManager.OrderStatus.CHECKED };
                            _result = new FEA_SABusinessLogic.ExportItemManager().UpdateStatus(e, i => i.Status);
                            break;
                        case "GENERALAFFAIR":
                            GAItem ga = new GAItem() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_GABusinessLogic.GAItemManager().UpdateStatus(ga, i => i.Status);
                            break;
                        //Added by Tony (2017-02-08)
                        case "ACCESSORYOUT":
                            ERPDocument erp = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erp, i => i.Status);
                            break;
                        case "FABRICOUT":
                            ERPDocument erpf = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpf, i => i.Status);
                            break;
                        case "FABRICMOVEOUT":
                            ERPDocument erpfm = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpfm, i => i.Status);
                            break;
                        case "FABRICMOVEOUTMULTI":
                            ERPDocument erpfmm = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpfmm, i => i.Status);
                            break;
                        case "ACCESSORYMOVEOUT":
                            ERPDocument erpa = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpa, i => i.Status);
                            break;
                        case "ACCESSORYMOVEOUTMULTI":
                            ERPDocument erpam = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpam, i => i.Status);
                            break;
                        case "FABRICDEVELOPOUT":
                            ERPDocument erpdf = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpdf, i => i.Status);
                            break;
                        case "ACCESSORYDEVELOPOUT":
                            ERPDocument erpda = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpda, i => i.Status);
                            break;
                        case "DEVELOPPRODUCT":
                             ERPDocument erpdp = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpdp, i => i.Status);
                            break;
                        case "SUGGESTRUINOUT":
                            ERPDocument suggestruinout = new ERPDocument() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(suggestruinout, i => i.Status);
                            break; 
                        case "MAINTENANCE":
                             MNRequestMain mnr = new MNRequestMain() { OrderCode = OrderCode, Status = 3 };
                            _result = new FEA_BusinessLogic.Maintenance.MaintenanceManager().UpdateStatus(mnr, i => i.Status);
                            break;
                        default:
                            break;
                    }
                    _result = new WFMainManager().UpdateItem(WFM, i => i.OrderCode, i => i.isFinished);
                    _result = new WFMainDetailManager().UpdateItem(WFMD, i => i.MainDetailID, i => i.isFinished, i => i.CheckDate, i => i.Comment, i => i.Temp1, i => i.Temp2, i => DelegateID);
                    if (_result)
                    {
                        scope.Complete();

                    }
                    else
                    {
                        scope.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _result = false;
                    scope.Dispose();
                }

            }
            return _result;
        }

        public bool UpdateNextWorkFlow(string NextApproverID, string MainDetailID, string NextNodeID, string MainID, string Comment, int UserID,string DelegateID, int? DelegateUserID)
        {
            bool _result = false;
            //--------
            WFMainDetail WFMUpdate = new WFMainDetail()
            {
                MainDetailID = MainDetailID,
                isFinished = 1,
                CheckDate = DateTime.Now,
                Comment = Comment,
                DelegateID = DelegateID == "" || DelegateID == null ? null : DelegateID,       // -- store DelegateID of WFDelegate table
                Temp2 = DelegateUserID == 0 ? "" : DelegateUserID.ToString()    // -- store Userid who was delegated.
            };
            WFMainDetail WFMDInsert = new WFMainDetail()
            {
                MainDetailID = Guid.NewGuid().ToString(),
                MainID = MainID,
                NodeID = NextNodeID,
                DelegateID = null,
                PostUserID = UserID,
                CheckUserID = int.Parse(NextApproverID),
                isFinished = 0,
                Comment = "",
                Temp1 = "0",      // -- Add reject status into Temp1 Column. 0: Dont have reject. 1: Reject
                Temp2 = "",
                Temp3 = ""
            };
            #region Remove transaction scope
            //--------
            //using (TransactionScope scope = new TransactionScope())
            //{
            //    try
            //    {
            //        _result = new WFMainDetailManager().UpdateItem(WFMUpdate, i => i.MainDetailID, i => i.isFinished, i => i.CheckDate, i => i.Comment, i => i.DelegateID, i => i.Temp2, i => i.Temp2);
            //        _result = new WFMainDetailManager().InsertItem(WFMDInsert);
            //        if (_result)
            //        {
            //            scope.Complete();
            //            db.Database.Connection.Close();
            //        }
            //        else
            //        {
            //            scope.Dispose();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _result = false;
            //        scope.Dispose();
            //    }

            //}
            #endregion


            if (db.Database.Connection.State == System.Data.ConnectionState.Closed)
                db.Database.Connection.Open();
            using (var WFTransaction = db.Database.Connection.BeginTransaction())
            {

                try
                {
                    _result = new WFMainDetailManager().UpdateItem(WFMUpdate, i => i.MainDetailID, i => i.isFinished, i => i.CheckDate, i => i.Comment, i => i.DelegateID, i => i.Temp2, i => i.Temp2);
                    _result = new WFMainDetailManager().InsertItem(WFMDInsert);
                    if (_result)
                    {
                        WFTransaction.Commit();

                    }
                    else
                    {
                        WFTransaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    _result = false;
                    WFTransaction.Rollback();
                }
            }
            return _result;
        }

        public bool RejectDocument(string OrderCode, string MainDetailID, string DocumentTypeName, string comments, int CurrentApproverID, string MainID, int CreateUserID,string DelegateID,int? DelegateUserID, Boolean?IsAdminReject = false)
        {
            bool _result = false;

            WFMainDetail WFMDInsert = new WFMainDetail()
            {
                MainDetailID = Guid.NewGuid().ToString(),
                MainID = MainID,
                NodeID = "",
                DelegateID = null,
                PostUserID = CurrentApproverID,    // CheckUserID
                CheckUserID = CreateUserID,
                isFinished = 0,
                Comment = (IsAdminReject == true)?comments:"",
                Temp1 = "1",      // -- Add reject status into Temp1 Column. 0: Dont have reject. 1: Reject
                Temp2 = ""
            };
            if (IsAdminReject == true) WFMDInsert.CheckDate = DateTime.Now;


            WFMainDetail WFMUpdate = new WFMainDetail()
            {
                MainDetailID = MainDetailID,
                isFinished = 1,
                CheckDate = DateTime.Now,
                Comment = (IsAdminReject == true) ? "" : comments,
                Temp2 = DelegateUserID == 0 ? "" : DelegateUserID.ToString(),    // -- store Userid who was delegated.
                DelegateID = DelegateID == "" || DelegateID == null ? null : DelegateID       // -- store DelegateID of WFDelegate table
            };


            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    switch (DocumentTypeName)
                    {
                        case "DEVICEREGISTRATION":
                            DeviceRegistration o = new DeviceRegistration() { OrderCode = OrderCode, Status = 4 };
                            _result = new DeviceRegistrationManager().UpdateStatus(o, i => i.Status);
                            break;
                        case "HARDWAREREQUIREMENT":
                            HardwareRequirement h = new HardwareRequirement() { OrderCode = OrderCode, Status = 4 };
                            _result = new HardwareRequirementManager().UpdateStatus(h, i => i.Status);
                            break;
                        case "SECURITYAREA":
                            ExportItem e = new ExportItem() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_SABusinessLogic.ExportItemManager().UpdateStatus(e, i => i.Status);
                            break;
                        case "GENERALAFFAIR":
                            GAItem ga = new GAItem() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_GABusinessLogic.GAItemManager().UpdateStatus(ga, i => i.Status);
                            break;
                        //Added by Tony (2017-02-08)
                        case "ACCESSORYOUT":
                            ERPDocument erpf = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpf, i => i.Status);
                            break;
                        case "FABRICOUT":
                            ERPDocument erp = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erp, i => i.Status);
                            break;
                        case "FABRICMOVEOUT":
                            ERPDocument erpfm = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpfm, i => i.Status);
                            break;
                        case "FABRICMOVEOUTMULTI":
                            ERPDocument erpfmm = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpfmm, i => i.Status);
                            break;
                        case "ACCESSORYMOVEOUT":
                            ERPDocument erpa = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpa, i => i.Status);
                            break;
                        case "ACCESSORYMOVEOUTMULTI":
                            ERPDocument erpam = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpam, i => i.Status);
                            break;
                        case "FABRICDEVELOPOUT":
                            ERPDocument erpdf = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpdf, i => i.Status);
                            break;
                        case "ACCESSORYDEVELOPOUT":
                            ERPDocument erpda = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erpda, i => i.Status);
                            break;
                        case "DEVELOPPRODUCT":
                            ERPDocument erppd = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(erppd, i => i.Status);
                            break;
                        case "SUGGESTRUINOUT":
                            ERPDocument suggestruinout = new ERPDocument() { OrderCode = OrderCode, Status = 4 };
                            _result = new FEA_BusinessLogic.ERP.Order().UpdateStatus(suggestruinout, i => i.Status);
                            break;
                        case "MAINTENANCE":
                            MNRequestMain mnr = new MNRequestMain() { OrderCode = OrderCode, Status = 4 ,ConfirmID=CurrentApproverID,ConfirmDate=DateTime.Now};
                            _result = new FEA_BusinessLogic.Maintenance.MaintenanceManager().UpdateStatus(mnr, i => i.Status,i=>i.ConfirmID,i=>i.ConfirmDate);
                            break;
                        
                        default:
                            break;
                    }


                    if(IsAdminReject == true)
                    {
                        // Update IsFinish of WFMain when the Order Status from Checked to Returned
                        WFMain wfMain = new WFMain(){MainID = MainID,OrderCode = OrderCode,isFinished = 0};
                        _result = new WFMainManager().UpdateItem(wfMain, o => o.isFinished);
                    }

                    if(IsAdminReject == false)
                        _result = new WFMainDetailManager().UpdateItem(WFMUpdate, i => i.MainDetailID, i => i.isFinished, i => i.CheckDate, i => i.Comment, i => i.Temp2, i => i.DelegateID);

                    _result = new WFMainDetailManager().InsertItem(WFMDInsert);
                    bool isReturn = UpdateWFMainDeatailReject(MainID);
                    if (_result && isReturn)
                    {
                        scope.Complete();
                    }
                    else
                    {
                        scope.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _result = false;
                    scope.Dispose();
                }

            }
            return _result;
        }

        /// <summary>
        ///  GetApprover
        /// </summary>
        /// <param name="sDocTypeName"></param>
        /// <param name="iCodeCenterCode"></param>
        /// <param name="sNodeID"></param>
        /// <returns></returns>
        public List<sp_GetApprover_Result> GetApprover(string sDocTypeName, int iCodeCenterCode, string sNodeID, string sOrderCode, int UserID)
        {
            return db.sp_GetApprover(sDocTypeName, iCodeCenterCode, sNodeID, sOrderCode,UserID).ToList();
        }

        public int CheckAlreadySigned(string MainDetailID)
        {
            int A = Convert.ToInt16(db.WFMainDetails.Where(s => s.MainDetailID == MainDetailID).Select(x => x.isFinished).SingleOrDefault().ToString());
            return A;
        }

        public List<sp_GetReferrenceDocumentList_Result> GetReferrenceDocumentList(int UserID)
        {
            return db.sp_GetReferrenceDocumentList(UserID).ToList();
        }

        /// <summary>
        /// Add by Jason - 2015/06/16
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public int CountReferrenceDocumentList(int UserID)
        {
            return db.sp_GetReferrenceDocumentList(UserID).Count();
        }

        public sp_GetDataForMultiSign_Result GetDataMultiSign(string OrderCode)
        {
            return db.sp_GetDataForMultiSign(OrderCode).SingleOrDefault();
        }
        //Added by Tony (2017-06-09)
        public bool UpdateWFMainDeatailReject(string MainID)
        {
            List<WFMainDetail> lstDetail = db.WFMainDetails.Where(i => (i.MainID == MainID) && (i.Temp3 != "1" || i.Temp3 == null || i.Temp3 == string.Empty)).ToList();
            bool _result = false;
            foreach (WFMainDetail itemDetail in lstDetail)
            {
                itemDetail.Temp3 = "1";
                _result = new WFMainDetailManager().UpdateItem(itemDetail, i => i.Temp3);
            }
            return _result;
        }
    }
}