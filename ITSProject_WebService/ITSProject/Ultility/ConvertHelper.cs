using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ITSProject.Models;
using FEA_BusinessLogic;
using System.IO;
namespace ITSProject.Ultility
{
    public class ConvertHelper
    {

        public class DeviceRegistrationHelper
        {
            public static DeviceRegistrationDTO ConvertToDeviceRegistration(string OrderCode, string NodeID, string TypeUser
                , string MainID, string MainDetailID, int CheckUserID
                , string DelegateID, int DelegateUserID, int CurrentUserID)
            {
                DeviceRegistration o = new DeviceRegistrationManager().GetItem(OrderCode);
                Models.DeviceRegistrationDTO item = new Models.DeviceRegistrationDTO()
                {
                    AttachmentLink = o.AttachmentLink,
                    CompleteDate = o.CompleteDate,
                    CreateDate = o.CreateDate,
                    CreatorID = o.CreatorID,
                    Description = o.Description,
                    DocumentType = o.DocumentType,
                    ID = o.ID,
                    IsUrgent = o.IsUrgent,
                    LastUpdateDate = o.LastUpdateDate,
                    OrderCode = o.OrderCode,
                    ProcessedBy = o.ProcessedBy,
                    Reason = o.Reason,
                    Status = o.Status,
                    TechnicianID = o.TechnicianID,
                    Temp1 = o.Temp1,
                    Temp2 = o.Temp2
                };

                ///Get detail of document
                if (o.DeviceRegistrationDetails != null)
                {
                    item.DeviceRegistrationDetailDTOs = new List<DeviceRegistrationDetailDTO>();
                    foreach (DeviceRegistrationDetail dt in o.DeviceRegistrationDetails)
                    {
                        item.DeviceRegistrationDetailDTOs.Add(ConvertToDeviceRegistrationDetail(dt));
                    }
                }

                // Get Next Approver
                List<sp_GetApprover_Result> lstApprover = CheckDuplicateApprover("DEVICEREGISTRATION", o.User1.CostCenterCode, NodeID, OrderCode, CurrentUserID);
                if (lstApprover != null && lstApprover.Count > 0)
                {
                    item.ApproverDTOs = new List<ApproverDTO>();
                    foreach (sp_GetApprover_Result apr in lstApprover)
                    {
                        item.ApproverDTOs.Add(ConvertToApproverDTO(apr));
                    }
                }

                // History of Worklow
                List<sp_GetWFHistory_Result> WFHistoryDTOs = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetHistory(o.OrderCode);
                if (WFHistoryDTOs != null && WFHistoryDTOs.Count > 0)
                {
                    item.WFHistoryDTOs = new List<WFHistoryDTO>();
                    foreach (sp_GetWFHistory_Result wfh in WFHistoryDTOs)
                    {
                        item.WFHistoryDTOs.Add(ConvertToWFHistoryDTO(wfh));
                    }
                }


                // file
                item.FileInfoDTOs = GetFilesofOrder(o.AttachmentLink);

                return item;
            }


            /// <summary>
            /// Convert model from DeviceRegistrationDetail to DeviceRegistrationDetailDTO 
            /// </summary>
            /// <param name="o"></param>
            /// <returns></returns>
            public static DeviceRegistrationDetailDTO ConvertToDeviceRegistrationDetail(DeviceRegistrationDetail o)
            {

                return new DeviceRegistrationDetailDTO()
                {
                    Description = o.Description,
                    DeviceRegistrationID = o.DeviceRegistrationID,
                    ID = o.ID,
                    ItemDetailID = o.ItemDetailID,
                    ItemDetailName = o.ItemDetail.ItemDetailName,
                    ItemID = o.ItemID,
                    ItemName = o.Item.ItemName,
                    Temp1 = o.Temp1
                };
            }

            public static ApproverDTO ConvertToApproverDTO(sp_GetApprover_Result o)
            {
                return new ApproverDTO()
                {
                    ApproverID = o.ApproverID,
                    CheckUserID = o.CheckUserID,
                    CheckUserName = o.CheckUserName,
                    DocumentTypeID = o.DocumentTypeID,
                    NodeID = o.NodeID,
                    Sequence = o.Sequence
                };
            }

            public static WFHistoryDTO ConvertToWFHistoryDTO(sp_GetWFHistory_Result o)
            {
                return new WFHistoryDTO()
                {
                    Approver = o.Approver,
                    CheckDate = o.CheckDate,
                    Comment = o.Comment,
                    OrderCode = o.OrderCode,
                    Sender = o.Sender,
                    Sequence = o.Sequence,
                    Status = o.Status
                };
            }

            public static List<sp_GetApprover_Result> CheckDuplicateApprover(string DocTypeName, int CodeCenterCode, string NodeID, string OrderCode, int CurrentUserID)
            {
                List<sp_GetApprover_Result> lstApprover = new DeviceRegistrationManager().GetApprover(DocTypeName, CodeCenterCode, NodeID, OrderCode, CurrentUserID);
                if (NodeID != "")
                {
                    if (lstApprover.Where(x => x.ApproverID == CurrentUserID).ToList().Count > 0)
                    {
                        lstApprover = CheckDuplicateApprover(DocTypeName, CodeCenterCode, lstApprover[0].NodeID, OrderCode, CurrentUserID);
                    }
                }
                return lstApprover;
            }


            public static List<FileInfoDTO> GetFilesofOrder(string AttachmentLink)
            {
                try
                {
                    List<FileInfoDTO> sResult = new List<FileInfoDTO>();

                    string RootValue = System.Configuration.ConfigurationManager.AppSettings["ITDocumentPath"].ToString();
                    string SiteLink = System.Configuration.ConfigurationManager.AppSettings["SiteLink"].ToString();
                    RootValue += AttachmentLink;

                    string[] filePaths = Directory.GetFiles(RootValue);
                    FileInfoDTO fileInfo;

                    foreach (string file in filePaths)
                    {
                        fileInfo = new FileInfoDTO()
                        {
                            FileName = Path.GetFileName(file),
                            //FilePath = SiteLink + AttachmentLink + "/" + Path.GetFileName(file),
                            FilePath=AttachmentLink,
                            ID = Guid.NewGuid().ToString()
                        };
                        sResult.Add(fileInfo);
                    }

                    return sResult;
                }
                catch (Exception ex)
                {
                    return new List<FileInfoDTO>();
                }
            }
        }

        public class HardwareRequirementHelper
        {
            public static HardwareRequirementDTO ConvertToHardwareRequirement(string OrderCode, string NodeID, string TypeUser
                                                                            , string MainID, string MainDetailID, int CheckUserID
                                                                            , string DelegateID, int DelegateUserID, int CurrentUserID)
            {
                HardwareRequirement o = new HardwareRequirementManager().GetItem(OrderCode);
                Models.HardwareRequirementDTO item = new Models.HardwareRequirementDTO()
                {
                    AttachmentLink = o.AttachmentLink,
                    ConfirmDate = o.ConfirmDate,
                    CreateDate = o.CreateDate,
                    CreatorID = o.CreatorID,
                    CurrencyID = o.CurrencyID,
                    Description = o.Description,
                    EstimatedAmount = o.EstimatedAmount,
                    ID = o.ID,
                    IsUrgent = o.IsUrgent,
                    OrderCode = o.OrderCode,
                    ProcessedBy = o.ProcessedBy,
                    Reason = o.Reason,
                    Status =o.Status,
                    TechnicianID = o.TechnicianID,
                    Temp1 = o.Temp1,
                    Temp2 =o.Temp2,
                    

                };

                ///Get detail of document
                if (o.HardwareRequirementDetails != null)
                {
                    item.HardwareRequirementDetailDTOs = new List<HardwareRequirementDetailDTO>();
                    foreach (HardwareRequirementDetail dt in o.HardwareRequirementDetails)
                    {
                        item.HardwareRequirementDetailDTOs.Add(ConvertToHardwareRequirementDetail(dt));
                    }
                }

                // Get Next Approver
                List<sp_GetApprover_Result> lstApprover = DeviceRegistrationHelper.CheckDuplicateApprover("HARDWAREREQUIREMENT", o.User1.CostCenterCode, NodeID, OrderCode, CurrentUserID);
                if (lstApprover != null && lstApprover.Count > 0)
                {
                    item.ApproverDTOs = new List<ApproverDTO>();
                    foreach (sp_GetApprover_Result apr in lstApprover)
                    {
                        item.ApproverDTOs.Add(DeviceRegistrationHelper.ConvertToApproverDTO(apr));
                    }
                }

                // History of Worklow
                List<sp_GetWFHistory_Result> WFHistoryDTOs = new FEA_BusinessLogic.WaitingArea.WaitingArea().GetHistory(o.OrderCode);
                if (WFHistoryDTOs != null && WFHistoryDTOs.Count > 0)
                {
                    item.WFHistoryDTOs = new List<WFHistoryDTO>();
                    foreach (sp_GetWFHistory_Result wfh in WFHistoryDTOs)
                    {
                        item.WFHistoryDTOs.Add(DeviceRegistrationHelper.ConvertToWFHistoryDTO(wfh));
                    }
                }

                //files
                item.FileInfoDTOs = DeviceRegistrationHelper.GetFilesofOrder(o.AttachmentLink);


                return item;
            }

            /// <summary>
            /// Convert model from DeviceRegistrationDetail to DeviceRegistrationDetailDTO 
            /// </summary>
            /// <param name="o"></param>
            /// <returns></returns>
            public static HardwareRequirementDetailDTO ConvertToHardwareRequirementDetail(HardwareRequirementDetail o)
            {

                return new HardwareRequirementDetailDTO()
                {
                    DeliveryDate = o.DeliveryDate,
                    Description = o.Description,
                    EstimatedAmount = o.EstimatedAmount,
                    EstimatedPrice = o.EstimatedPrice,
                    HardwareRequirementID = o.HardwareRequirementID,
                    ID = o.ID,
                    ItemDetailID = o.ItemDetailID,
                    ItemDetailName = o.ItemDetail.ItemDetailName,
                    ItemID = o.ItemID,
                    ItemName = o.Item.ItemName,
                    ItemNo = o.ItemNo,
                    Quantity = o.Quantity,
                    Temp1 = o.Temp1,
                    Temp2 = o.Temp2,
                    UnitID = o.UnitID,
                    UnitName = o.Unit.NAME
                };
            }

        }
    }
}