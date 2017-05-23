using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FEA_ITS_Site.Models.Helper
{

    public enum Status
    {
        enabled = 1,
        unenabled = 0

    }

    public enum EditItemStatus
    {
        success =1,
        failed =0,
        nomal = -1
    }

    public class PartialParameter
    {
        public static string QueryDivertPO { get { return "QueryDivertPO"; } }

        public static string DOC_TYPE_PARTIAL { get { return "doc_type_partial"; } } // Partial for document type grid

        public static string NODE_DOCTYPE_LIST_PARTIAL { get { return "node_doctype_list_grid"; } } // Partial for node detail

        public static string NODE_DETAIL_LIST_GRID { get { return "node_detail_list_grid"; } } // Partial for node detail

        public static string NODE_LIST_GRID { get { return "node_list_grid"; } } // Partial for node detail

        public static string SITE_FUNCTION_TREELIST { get { return "site_function_list"; } }

        public static string USER_GROUP_LIST_GRID { get { return "user_group_list_grid"; } }

        public static string SITEFUNCTION_USERGROUP_TREEVIEW { get { return "SiteFunction_UserGroup_TreeView"; } }

        public static string ITEM_DETAIL_LIST_GRID { get { return "item_detail_list_grid"; } }

        public static string DEVICE_REGISTRATION_LIST_GRID { get { return "device_registration_list_grid"; } }

        public static string ITEM_DETAIL_HARDWARE_LIST_GRID { get { return "item_detail_hardware_list_grid"; } }

        public static string HARDWARE_REQUIREMENT_LIST_GRID { get { return "hardware_requirement_list_grid"; } }

        public static string ITEM_DETAIL_STOCKITEM_LIST_GRID { get { return "item_detail_stockItem_list_grid"; } }

        public static string STOCK_IN_LIST_GRID { get { return "stock_in_list_grid"; } }

        public static string STOCK_OUT_LIST_GRID { get { return "stock_out_list_grid"; } }

        public static string APPROVE_ORDER_GRID { get { return "approve_order_grid"; } }
        public static string GA_APPROVE_ORDER_GRID { get { return "ga_approve_order_grid"; } }

        public static string ITEM_MANAGE_LIST_GRID { get { return "item_manage_list_grid"; } }

        public static string ITEM_DETAIL_MANAGE_LIST_GRID { get { return "item_detail_manage_list_grid"; } }

        public static string DELEGATE_MANAGE_LIST_GRID { get { return "delegate_manage_list_grid"; } }

        public static string REQUEST_ORDER_GRID { get { return "request_order_grid"; } }

        public static string WAREHOUSE_IMPORT_MANAGEMENT_LIST_GRID { get { return "warehousr_import_list_grid"; } }

        public static string ITEM_DETAIL_SA_LIST_GRID { get { return "item_detail_Sa_list_grid"; } }

        public static string SA_PPROVE_ORDER_GRID { get { return "sa_approve_order_grid"; } }

        public static string SA_ITEM_LIST_GRID { get { return "sa_item_list_grid"; } }

        public static string SA_ITEM_PENDING_LIST_GRID { get { return "sa_item_pending_list_grid"; } }
        public static string GAItemListGrid { get { return "GAItemListGrid"; } }
        public static string GaItemManagerListGrid { get { return "GaItemManagerListGrid"; } }
        // Add by Tony(2016-09-17)
        public static string GAPushDataList { get { return "GAPushDataList"; } }
        public static string GADetailPushData { get { return "GADetailPushData"; } }
        public static string QueryOpenOrder { get { return "QueryOpenOrder"; } }
        public static string MaintanceItemListGrid { get { return "MaintanceItemListGrid"; } }
        public static string MaintenanceItemDetail { get { return "Maintenance_Item_Detail"; } }
        public static string MNRequestList { get { return "MN_Request_List"; } }
        public static string MNStockList { get { return "MN_Stock_List"; } }
        public static string MNInventory { get { return "MaintenanceInventory"; } }
        public static string MNApproveDoc { get { return "MaintenanceApproveDocList"; } }
        public static string MNApproveQuantity { get { return "MaintenanceApproveDetailList"; } }
        public static string MNConfirmedGrid { get { return "MNConfirmedGrid"; } }
        //
        public static string ITEM_DETAIL_SA_PROCESS_LIST_GRID { get { return "item_detail_Sa_process_list_grid"; } }
        public static string ExportItemApproverItemList { get { return "ExportItemApproverItemList"; } }
        public static string GAAdjustmentList { get { return "GAAdjustmentList"; } }


        public static string QuerySignFlow { get { return "QuerySignFlow"; } }
        public static string PurchaseReport { get { return "PurchaseReport"; } }
        public static string WaitingDocument { get { return "WaitingDocument"; } }
        public static string WFHistory { get { return "WFHistory"; } }
        public static string FinishedForManager { get { return "FinishedForManager"; } }
        public static string DetailFinishedForManager { get { return "DetailFinishedForManager"; } }
        public static string GAFinishedForManager { get { return "GAFinishedForManager"; } }
        public static string GADetailFinishedForManager { get { return "GADetailFinishedForManager"; } }
        public static string StatisticDR { get { return "StatisticDR"; } }
        public static string StatisticHR { get { return "StatisticHR"; } }
        public static string StatisticHRDetail { get { return "StatisticHRDetail"; } }
        public static string DetailHRFinishedForManager { get { return "DetailHRFinishedForManager"; } }
        public static string QueryDynamicInventory { get { return "QueryDynamicInventory"; } }
        public static string QueryDetailDynamicInventory { get { return "QueryDetailDynamicInventory"; } }
        public static string DynamicQueryPackingManifest { get { return "DynamicQueryPackingManifest"; } }
        public static string GetSADynamicReport { get { return "GetSADynamicReport"; } }
        public static string GetDetailSADynamicReport { get { return "GetDetailSADynamicReport"; } }
        public static string GetDataForSAPivotGrid { get { return "GetDataForSAPivotGrid"; } }
        public static string FinishedForSA { get { return "FinishedForSA"; } }

        public static string SAConfigure { get { return "SAConfigure"; } }
        public static string SACustomerList { get { return "SACustomerList"; } }
        public static string SADestination { get { return "SADestination"; } }
        public static string SAReason { get { return "SAReason"; } }
        public static string GetSADynamicReportChart { get { return "GetSADynamicReportChart"; } }
        public static string GetSADynamicReportChartPie { get { return "GetSADynamicReportChartPie"; } }
        public static string SARequestList { get { return "SARequestList"; } }
        public static string GARequestList { get { return "GARequestList"; } }
        public static string GetCostComparingSpan { get { return "GetCostComparingSpan"; } }
        public static string WHExport_list_grid { get { return "WHExport_list_grid"; } }
        public static string RefferenceList { get { return "RefferenceList"; } }

        public static string WHImporterApproveList { get { return "WHImporterApproveList"; } }
        public static string TotalSheldReport { get { return "TotalSheldReport"; } }
        //Added by Tony(2017-02-08)
        public static string ERPItemListGrid { get { return "ERPItemListGrid"; } }
        public static string ERPHistory { get { return "ERPHistory"; } }
        public static string QueryMNDynamicInventory { get { return "QueryMNDynamicInventory"; } }
        public static string MNQueryRequestList { get { return "MNQueryRequestList"; } }
    }

    public class TagPrefixParameter
    {
        public static string DEVICE_REGISTRATION { get { return "DEVICEREGISTRATION"; } }
        public static string HARD_REGISTRATION { get { return "HARDWAREREQUIREMENT"; } }
        public static string STOCK_IN { get { return "STOCKIN"; } }
        public static string STOCK_OUT { get { return "STOCKOUT"; } }
        public static string WAREHOUSEAREA { get { return "WAREHOUSE"; } }
        public static string SECURITYAREA { get { return "SECURITYAREA"; } }
        public static string WAREHOUSEAREAEXPORT { get { return "WAREHOUSEEXPORT"; } }
        public static string WHIMPORT { get { return "WHIMPORT"; } }
        public static string GENERALAFFAIR { get { return "GENERALAFFAIR"; } }
        public static string MAINTENANCE { get { return "MAINTENANCE"; } } //Added by Tony (2017-04-19)
        public static string MAINTENANCESTOCKIN { get { return "MAINTENANCESTOCKIN"; } }
        public static string MAINTENANCESTOCKOUT { get { return "MAINTENANCESTOCKOUT"; } }

        //Added by Tony (2017-02-08)
        public static string ACCESSORYOUT { get { return "ACCESSORYOUT"; } }
        public static string FABRICOUT { get { return "FABRICOUT"; } }
        public static string FABRICMOVEOUT { get { return "FABRICMOVEOUT"; } }
        public static string FABRICMOVEOUTMULTI { get { return "FABRICMOVEOUTMULTI"; } }
        public static string ACCESSORYMOVEOUT { get { return "ACCESSORYMOVEOUT"; } }
        public static string ACCESSORYMOVEOUTMULTI { get { return "ACCESSORYMOVEOUTMULTI"; } }
        public static string FABRICDEVELOPOUT { get { return "FABRICDEVELOPOUT"; } }
        public static string ACCESSORYDEVELOPOUT { get { return "ACCESSORYDEVELOPOUT"; } }
        public static string DEVELOPPRODUCT { get { return "DEVELOPPRODUCT"; } }

      
    }

    public class ItemIDDefault
    {
        public static string NormalMaterial { get { return "42141F1B-3F06-4B01-9AD3-0B8DB359A5EF"; } }
        public static string Assets { get { return "DBA56258-B302-4C79-9D2F-97D4A088CF17"; } }
    }
}