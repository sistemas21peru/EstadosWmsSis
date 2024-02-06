using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioEstadosWmsSis.Model
{
    public class WmsData
    {
//        string id { get; set; }

//        string url { get; set; }
//        string create_user { get; set; }
//        string create_ts { get; set; }
//        string mod_user { get; set; } 
//        string mod_ts { get; set; }
//        string facility_id { get; set; }
//                "id": 111716,
//                "key": "50003",
//                "url": "https://ta10.wms.ocs.oraclecloud.com:443/bata_test2/wms/lgfapi/v10/entity/facility/111716"
//            },
//         string company_id { get; set; }
//                "id": 35113,
//                "key": "730",
//                "url": "https://ta10.wms.ocs.oraclecloud.com:443/bata_test2/wms/lgfapi/v10/entity/company/35113"
//            },
//        string order_nbr { get; set; }
//string order_type_id { get; set; }
//                "id": 161,
//                "key": "64",
//                "url": "https://ta10.wms.ocs.oraclecloud.com:443/bata_test2/wms/lgfapi/v10/entity/order_type/161"
//            },
//        string status_id { get; set; }
//string ord_date { get; set; }
//string exp_date { get; set; }
//string req_ship_date { get; set; }
//string dest_facility_id { get; set; }
//                "id": 148989,
//                "key": "1837503",
//                "url": "https://ta10.wms.ocs.oraclecloud.com:443/bata_test2/wms/lgfapi/v10/entity/facility/148989"
//            },
//      string shipto_facility_id { get; set; } 
//                "id": 148989,
//                "key": "1837503",
//                "url": "https://ta10.wms.ocs.oraclecloud.com:443/bata_test2/wms/lgfapi/v10/entity/facility/148989"
//            },
//     string cust_name { get; set; }
//string cust_addr { get; set; }
//string cust_addr2 { get; set; }
//string cust_addr3 { get; set; }
//string cust_city { get; set; }
//string cust_state { get; set; }
//string cust_zip { get; set; }
//string cust_country { get; set; }
//string cust_phone_nbr { get; set; }
//string cust_email { get; set; }
//string cust_nbr { get; set; }
//string shipto_name { get; set; }
//string shipto_addr { get; set; }
//string shipto_addr2 { get; set; }
//string shipto_addr3": "",
//   string shipto_city": "",
//    string shipto_state": "",
//    string shipto_zip": "",
//    string shipto_country": "",
//    string shipto_phone_nbr": "",
//    string shipto_email": "",
//    string ref_nbr": "",
//   string stage_location_id": null,
//  string ship_via_ref_code": "",
//    string route_nbr": "",
//    string external_route": "",
//    string destination_company_id": {
//                "id": 76392,
//                "key": "18375",
//                "url": "https://ta10.wms.ocs.oraclecloud.com:443/bata_test2/wms/lgfapi/v10/entity/company/76392"
//            },
//   string ship_via_id": null,
//  string priority": 10,
//   string host_allocation_nbr": "",
//    string sales_order_nbr": "",
//    string sales_channel": "",
//   string customer_po_nbr": "12/12/2019  ",
//    string carrier_account_nbr": "",
//    string payment_method_id": null,
//    string dest_dept_nbr": "BI",
//    string start_ship_date": null,
//    string stop_ship_date": "2019-12-17",
//   string vas_group_code": "",
//   string spl_instr": "",
//   string currency_code": "",
//     string record_origin_code": "",
//    string cust_contact": "",
//    string shipto_contact": "",
//    string ob_lpn_type": "",
//     string ob_lpn_type_id": null,
//     string total_orig_ord_qty": "1",
//      string orig_sku_count": 1,
//     string orig_sale_price": null,
//     string gift_msg": "",
//     string sched_ship_date": null,
//      string customer_po_type": "",
//    string customer_vendor_code": "",
//     string externally_planned_load_flg": false,
//    string work_order_kit_id": null,
//     string order_nbr_to_replace": "",
//     string stop_ship_flg": false,
//      string lpn_type_class": "",
//     string billto_carrier_account_nbr": "",
//     string duties_carrier_account_nbr": "",
//      string duties_payment_method_id": null,
//     string customs_broker_contact_id": null,
//     string order_shipped_ts": null,
//     string cust_field_1": "",
//    string cust_field_2": "4",
//    string cust_field_3": "6",
//    string cust_field_4": "20127765279",
//    string cust_field_5": "",
//     string cust_date_1": null,
//     string cust_date_2": null,
//    string cust_date_3": null,
//  string cust_date_4": null,
//   string cust_date_5": null,
//  string cust_number_1": 0,
//  string cust_number_2": 0,
//   string cust_number_3": 0,
//  string cust_number_4": 0,
//   string cust_number_5": 0,
//   string cust_decimal_1": "0",
//   string cust_decimal_2": "0",
//     string cust_decimal_3": "0",
//     string cust_decimal_4": "0",
// string cust_decimal_5": "0",
//  string cust_short_text_1": "",
//  string cust_short_text_2": "",
//   string cust_short_text_3": "",
//   string cust_short_text_4": "",
//   string cust_short_text_5": "",
//  string cust_short_text_6": "",
// string cust_short_text_7": "",
// string cust_short_text_8": "",
//  string cust_short_text_9": "",
//  string cust_short_text_10": "",
//    string cust_short_text_11": "",
//          string cust_short_text_12": "",
//         string cust_long_text_1": "",
//          string cust_long_text_2": "",
//          string cust_long_text_3": "",
//          string order_instructions_set": [],
//           string order_dtl_set": {
//                "result_count": 1,
//                "url": "https://ta10.wms.ocs.oraclecloud.com:443/bata_test2/wms/lgfapi/v10/entity/order_dtl/?order_id=1895223"
//            },
//          string order_lock_set": null,
//         string tms_parcel_shipment_nbr": "",
//          string erp_source_hdr_ref": "",
//          string erp_source_system_ref": "",
//          string tms_order_hdr_ref": ""
    }
}
