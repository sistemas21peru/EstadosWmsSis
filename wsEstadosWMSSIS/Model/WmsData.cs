using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioEstadosWmsSis.Model
{
    public class WmsData
    {
        public string mod_ts { get; set; }
        public Facility_Id facility_id { get; set; }
        public string order_nbr { get; set; }
        public string dest_dept_nbr { get; set; }
        public string total_orig_ord_qty { get; set; }
        public string orig_sku_count { get; set; }
        public string cust_field_2 { get; set; }
        public string cust_field_3 { get; set; }
        public string create_ts { get; set; }


    }


    public class Facility_Id
    {
        public string id { get; set; }
        public string key { get; set; }
        public string url { get; set; }
    }

    public class WmsDataFinal
    {
        public string Sucursal { get; set; }
        public string order_nbr { get; set; }
    }

}
