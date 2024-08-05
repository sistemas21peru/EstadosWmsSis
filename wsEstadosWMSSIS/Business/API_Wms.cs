using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ServicioEstadosWmsSis.Model;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using ServicioEstadosWmsSis;
using System.IO;
using System.Diagnostics;

namespace wsEstadosWMSSIS.Business
{
    public class API_Wms
    {
        List<WmsDataFinal> listaFinal = new List<WmsDataFinal>();

        public API_Wms()
        {
            listaFinal.Clear();
        }

        public void EnviarCorreoReporteEstados()
        {
            int diasConsulta = int.Parse(ConfigurationManager.AppSettings["dias_consulta_cancelados_email"].ToString());
            DateTime fechaActual = DateTime.Now;
            DateTime fechaInicial = fechaActual.AddDays(diasConsulta * -1);

            List<WmsData> listaWms = ConsultarApiEstadosWms(diasConsulta, fechaInicial, fechaActual);
            if (listaWms == null)
            {
                Environment.Exit(0);
                return;
            }

            List<WmsData> listaWmsFiltrado = listaWms.Where(n => n.dest_dept_nbr.Trim() != "CA" && n.dest_dept_nbr.Trim() != "CE" && n.dest_dept_nbr.Trim() != "" && n.order_nbr.Length == 7).ToList();

            EnviarReporteEstadosWms(listaWmsFiltrado, diasConsulta, fechaInicial, fechaActual);
            Environment.Exit(0);
        }

        public void ProcesarEstadosWmsSis()
        {

            try
            {
                LogUtil.Graba_Log("Log_Estados_Sis", "   Inicio de Proceso", false, "");


                int diasConsulta = int.Parse(ConfigurationManager.AppSettings["dias_consulta_cancelados"].ToString());
                DateTime fechaActual = DateTime.Now;
                DateTime fechaInicial = fechaActual.AddDays(diasConsulta * -1);

                //OBTENIENDO DATOS DESDE WMS
                LogUtil.Graba_Log("Log_Estados_Sis", "OBTENIENDO DATOS DESDE WMS", false, "");
                List<WmsData> listaWms = ConsultarApiEstadosWms(diasConsulta, fechaInicial, fechaActual);
                if (listaWms == null || listaWms.Count == 0)
                {
                    LogUtil.Graba_Log("Log_Estados_Sis", "NO SE ENCONTRARON DATOS DESDE WMS", false, "");
                    Environment.Exit(0);
                    return;
                }

                //VALIDANDO QUE ORDENES NO SE VUELVAN A REPETIR EN TABLA INTERMEDIA BDWMS
                LogUtil.Graba_Log("Log_Estados_Sis", "VALIDANDO QUE ORDENES NO SE VUELVAN A REPETIR EN TABLA INTERMEDIA BDWMS", false, "");
                List<WmsData> listaWmsVerificado = VerificarPedidosAnuladosRegistrados(listaWms);
                if (listaWmsVerificado == null || listaWmsVerificado.Count == 0)
                {
                    LogUtil.Graba_Log("Log_Estados_Sis", "LOS PEDIDOS YA SE PROCESARON ANTERIORMENTE", false, "");
                    Environment.Exit(0);
                    return;
                }

                //FILTRANDO SOLO PEDIDOS DEL ALMACEN Y EXCLUIR CADENAS QUE NO VAN EN CONSULTA
                LogUtil.Graba_Log("Log_Estados_Sis", "FILTRANDO SOLO PEDIDOS DEL ALMACEN Y EXCLUIR CADENAS QUE NO VAN EN CONSULTA", false, "");

                List<WmsData> listaAlmacen = FiltrarDatos(listaWmsVerificado);
                if (listaAlmacen == null || listaAlmacen.Count == 0)
                {
                    LogUtil.Graba_Log("Log_Estados_Sis", "DATOS FILTRADOS SIN CONTENIDO", false, "");
                    Environment.Exit(0);
                    return;
                }
                //CONSULTANDO EN DBF SI LAS ORDENES NO ESTAN ANULADAS
                LogUtil.Graba_Log("Log_Estados_Sis", "CONSULTANDO EN DBF SI LAS ORDENES NO ESTAN ANULADAS", false, "");

                List<WmsDataFinal> listaValidada = ObtenerOrdenesSinAnular(listaAlmacen);
                if (listaValidada == null || listaValidada.Count == 0)
                {
                    LogUtil.Graba_Log("Log_Estados_Sis", "LOS PEDIDOS YA SE ANULARON ANTERIORMENTE EN SIS", false, "");
                    Environment.Exit(0);
                    return;
                }
                //CREANDO ARCHIVO TXT
                LogUtil.Graba_Log("Log_Estados_Sis", "CREANDO ARCHIVO TXT", false, "");
                CrearArchivoAnulados(listaValidada);

                //REGISTRAR PEDIDOS ANULADOS EN TABLA INTERMEDIA BDWMS
                LogUtil.Graba_Log("Log_Estados_Sis", "REGISTRAR PEDIDOS ANULADOS EN TABLA INTERMEDIA BDWMS", false, "");
                RegistrarPedidosAnulados(listaAlmacen);

                //LLAMAR PROGRAMA DE ANULACION DE FOX
                LogUtil.Graba_Log("Log_Estados_Sis", "LLAMAR PROGRAMA DE ANULACION DE FOX", false, "");
                string exepath = ConfigurationManager.AppSettings["pathExeFox"] + ConfigurationManager.AppSettings["NombreExe"];
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = exepath;
                psi.WorkingDirectory = Path.GetDirectoryName(exepath);
                Process.Start(psi);

                LogUtil.Graba_Log("Log_Estados_Sis", "PROCESO FINALIZADO", false, "");

                Environment.Exit(0);
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");

            }
        }

        public List<WmsData> ConsultarApiEstadosWms(int diasConsulta, DateTime fechaInicial, DateTime fechaFinal)
        {
            try
            {
                DateTime startDayApi = fechaInicial.Date;
                DateTime endDayApi = fechaFinal.Date;

                endDayApi = endDayApi.AddHours(23.9999);
                List<WmsData> listaWms = new List<WmsData>();

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings["Api_User"].ToString() + ":" + ConfigurationManager.AppSettings["Api_Pass"].ToString())));
                    using (StringContent jsonContent = new StringContent(""))
                    {
                        jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                        string url = ConfigurationManager.AppSettings["Api_Url"].ToString() + "order_hdr?status_id=99&mod_ts__gte=" + startDayApi.ToString("yyyy-MM-ddTHH:mm:ss") + "&mod_ts__lte=" + endDayApi.ToString("yyyy-MM-ddTHH:mm:ss");
                        var request = client.GetAsync(url);
                        dynamic response = request.Result.Content.ReadAsStringAsync().Result;
                        JObject root = JObject.Parse(response);
                        var listaJson = JsonConvert.SerializeObject(root["results"]);
                        List<WmsData> listaWmsPart = JsonConvert.DeserializeObject<List<WmsData>>(listaJson);
                        listaWms.AddRange(listaWmsPart);

                        string nextPage = root["next_page"].ToString().Trim();
                        nextPage = nextPage.Replace("%3A", ":");

                        NextPage:
                        if (nextPage != "")
                        {
                            request = client.GetAsync(nextPage);
                            response = request.Result.Content.ReadAsStringAsync().Result;
                            root = JObject.Parse(response);
                            listaJson = JsonConvert.SerializeObject(root["results"]);
                            listaWmsPart = JsonConvert.DeserializeObject<List<WmsData>>(listaJson);
                            listaWms.AddRange(listaWmsPart);
                            nextPage = root["next_page"].ToString().Trim();
                            goto NextPage;
                        }

                        return listaWms;
                    }
                }
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
                return null;
            }
        }

        public void EnviarReporteEstadosWms(List<WmsData> lista, int diasConsulta, DateTime fechaInicial, DateTime fechaFinal)
        {
            try
            {
                DateTime startDayEmail = fechaInicial;
                DateTime endDayEmail = fechaFinal.AddDays(-1);

                DataTable listWms = new DataTable();
                listWms.Columns.Add("mod_ts");
                listWms.Columns.Add("facility_id");
                listWms.Columns.Add("order_nbr");
                listWms.Columns.Add("dest_dept_nbr");
                listWms.Columns.Add("total_orig_ord_qty");
                listWms.Columns.Add("orig_sku_count");
                listWms.Columns.Add("cust_field_2");
                listWms.Columns.Add("cust_field_3");
                listWms.Columns.Add("create_ts");
                foreach (WmsData data in lista)
                {
                    DataRow dRow = listWms.NewRow();
                    dRow["mod_ts"] = data.mod_ts;
                    dRow["facility_id"] = data.facility_id.key;
                    dRow["order_nbr"] = data.order_nbr;
                    dRow["dest_dept_nbr"] = data.dest_dept_nbr;
                    dRow["total_orig_ord_qty"] = data.total_orig_ord_qty;
                    dRow["orig_sku_count"] = data.orig_sku_count;
                    dRow["cust_field_2"] = data.cust_field_2;
                    dRow["cust_field_3"] = data.cust_field_3;
                    dRow["create_ts"] = data.create_ts;
                    listWms.Rows.Add(dRow);
                }

                string conn = ConfigurationManager.ConnectionStrings["SQL"].ConnectionString;
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    if (cn.State == 0) cn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            cmd.Connection = cn;
                            cmd.CommandTimeout = 0;
                            cmd.CommandText = "USP_REPORTE_ESTADOS_WMS_MASIVO";
                            cmd.CommandType = CommandType.StoredProcedure;
                            var parameter = new SqlParameter
                            {
                                SqlDbType = SqlDbType.Structured,
                                ParameterName = "@T_ESTADOS",
                                Value = listWms
                            };
                            cmd.Parameters.Add(parameter);
                            cmd.Parameters.AddWithValue("@FECHA_INI", startDayEmail);
                            cmd.Parameters.AddWithValue("@FECHA_FIN", endDayEmail);

                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
            }
        }

        public List<WmsData> FiltrarDatos(List<WmsData> listaWms)
        {
            try
            {
                return listaWms.Where(n => n.facility_id.key.Contains(ConfigurationManager.AppSettings["Sucursal"].ToString()) && n.dest_dept_nbr.Trim() != "CA" && n.dest_dept_nbr.Trim() != "CE" && n.dest_dept_nbr.Trim() != "").ToList();
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
                return null;
            }
        }



        public List<WmsDataFinal> ObtenerOrdenesSinAnular(List<WmsData> listaWmsAlmacen)
        {
            try
            {
                string ordenes = "";
                int count = 0;

                List<WmsDataFinal> listaValidada = new List<WmsDataFinal>();

                //ORDENES RETAIL
                //==========================================================================
                List<WmsData> listaWmsRetail = listaWmsAlmacen.Where(n => n.cust_field_3.Trim() == "5").ToList();

                foreach (WmsData data in listaWmsRetail)
                {
                    ordenes = ordenes + ",'" + data.order_nbr + "'";
                    count += 1;
                    if (count == 100)
                    {
                        listaValidada.AddRange(ValidarAnuladosEnSisRetail(ordenes.Substring(1)));
                        ordenes = "";
                        count = 0;
                    }
                }
                if (ordenes != "")
                {
                    listaValidada.AddRange(ValidarAnuladosEnSisRetail(ordenes.Substring(1)));
                    ordenes = "";
                    count = 0;
                }
                //===========================================================================

                //ORDENES NO RETAIL
                //==========================================================================
                List<WmsData> listaWmsNoRetail = listaWmsAlmacen.Where(n => n.cust_field_3.Trim() == "6").ToList();

                foreach (WmsData data in listaWmsNoRetail)
                {
                    ordenes = ordenes + ",'" + data.order_nbr + "'";
                    count += 1;
                    if (count == 100)
                    {
                        listaValidada.AddRange(ValidarAnuladosEnSisNoRetail(ordenes.Substring(1)));
                        ordenes = "";
                        count = 0;
                    }
                }
                if (ordenes != "")
                {
                    listaValidada.AddRange(ValidarAnuladosEnSisNoRetail(ordenes.Substring(1)));
                }

                //============================================================================

                return listaValidada;

            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
                return null;
            }
        }

        public List<WmsDataFinal> ValidarAnuladosEnSisRetail(string ordenes)
        {
            try
            {
                DataTable dt = null;

                string sql = "SELECT cgud_gudis,CGUD_ESTAD FROM SCCCGUD WHERE cgud_gudis in (" + ordenes + ") AND (CGUD_ESTAD<>'Anu' AND CGUD_ESTAD<>'AnG')";
                //string sql = "SELECT cgud_gudis,CGUD_ESTAD FROM SCCCGUD WHERE cgud_gudis in (" + ordenes + ")";

                string connDbf = ConfigurationManager.ConnectionStrings["connDbfRetail"].ConnectionString; ;
                NetworkShare.ConnectToShare(ConfigurationManager.AppSettings["pathDbf_retail"], ConfigurationManager.AppSettings["pathDbf_user"], ConfigurationManager.AppSettings["pathDbf_pass"]);

                using (OleDbConnection dbConn = new OleDbConnection(connDbf))
                {
                    dbConn.Open();

                    using (OleDbCommand cmd = dbConn.CreateCommand())
                    {
                        cmd.CommandText = "set enginebehavior 70";
                        cmd.ExecuteNonQuery();
                    }

                    System.Data.OleDb.OleDbCommand com = new System.Data.OleDb.OleDbCommand(sql, dbConn);
                    com.CommandTimeout = 0;
                    System.Data.OleDb.OleDbDataAdapter ada = new System.Data.OleDb.OleDbDataAdapter(com);
                    dt = new DataTable();
                    ada.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            listaFinal.Add(new WmsDataFinal()
                            {
                                Sucursal = ConfigurationManager.AppSettings["Sucursal_Sis"].ToString(),
                                order_nbr = row["cgud_gudis"].ToString(),

                            });
                        }
                    }
                    if (dbConn != null)
                        if (dbConn.State == ConnectionState.Open) dbConn.Close();
                }

                return listaFinal;

            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
                return null;
            }
        }

        public List<WmsDataFinal> ValidarAnuladosEnSisNoRetail(string ordenes)
        {
            try
            {
                DataTable dt = null;

                string sql = "SELECT oc_nord FROM VMAOC WHERE oc_nord in (" + ordenes + ") and (OC_Estado<>'Anu' AND OC_Estado<>'AnG')";
                // string sql = "SELECT oc_nord FROM VMAOC WHERE oc_nord in (" + ordenes + ")";

                string connDbf = ConfigurationManager.ConnectionStrings["connDbfNoRetail"].ConnectionString; ;
                NetworkShare.ConnectToShare(ConfigurationManager.AppSettings["pathDbf_noretail"], ConfigurationManager.AppSettings["pathDbf_user"], ConfigurationManager.AppSettings["pathDbf_pass"]);

                using (OleDbConnection dbConn = new OleDbConnection(connDbf))
                {
                    dbConn.Open();

                    using (OleDbCommand cmd = dbConn.CreateCommand())
                    {
                        cmd.CommandText = "set enginebehavior 70";
                        cmd.ExecuteNonQuery();
                    }

                    System.Data.OleDb.OleDbCommand com = new System.Data.OleDb.OleDbCommand(sql, dbConn);
                    com.CommandTimeout = 0;
                    System.Data.OleDb.OleDbDataAdapter ada = new System.Data.OleDb.OleDbDataAdapter(com);
                    dt = new DataTable();
                    ada.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            listaFinal.Add(new WmsDataFinal()
                            {
                                Sucursal = ConfigurationManager.AppSettings["Sucursal_Sis"].ToString(),
                                order_nbr = row["oc_nord"].ToString(),

                            });
                        }
                    }
                    if (dbConn != null)
                        if (dbConn.State == ConnectionState.Open) dbConn.Close();
                }

                return listaFinal;
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
                return null;
            }
        }

        public void RegistrarPedidosAnulados(List<WmsData> lista)
        {
            try
            {

                DataTable listWms = new DataTable();
                listWms.Columns.Add("mod_ts");
                listWms.Columns.Add("facility_id");
                listWms.Columns.Add("order_nbr");
                listWms.Columns.Add("dest_dept_nbr");
                listWms.Columns.Add("total_orig_ord_qty");
                listWms.Columns.Add("orig_sku_count");
                listWms.Columns.Add("cust_field_2");
                listWms.Columns.Add("cust_field_3");
                listWms.Columns.Add("create_ts");
                foreach (WmsData data in lista)
                {
                    DataRow dRow = listWms.NewRow();
                    dRow["mod_ts"] = data.mod_ts;
                    dRow["facility_id"] = data.facility_id.key;
                    dRow["order_nbr"] = data.order_nbr;
                    dRow["dest_dept_nbr"] = data.dest_dept_nbr;
                    dRow["total_orig_ord_qty"] = data.total_orig_ord_qty;
                    dRow["orig_sku_count"] = data.orig_sku_count;
                    dRow["cust_field_2"] = data.cust_field_2;
                    dRow["cust_field_3"] = data.cust_field_3;
                    dRow["create_ts"] = data.create_ts;
                    listWms.Rows.Add(dRow);
                }

                string conn = ConfigurationManager.ConnectionStrings["SQL_WMS"].ConnectionString;
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    if (cn.State == 0) cn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            cmd.Connection = cn;
                            cmd.CommandTimeout = 0;
                            cmd.CommandText = "USP_REGISTRAR_ANULACIONES_WMS_SIS";
                            cmd.CommandType = CommandType.StoredProcedure;
                            var parameter = new SqlParameter
                            {
                                SqlDbType = SqlDbType.Structured,
                                ParameterName = "@T_ESTADOS",
                                Value = listWms
                            };
                            cmd.Parameters.Add(parameter);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
            }
        }

        public List<WmsData> VerificarPedidosAnuladosRegistrados(List<WmsData> lista)
        {
            List<WmsData> listaValidada = null;
            try
            {

                DataTable dt = null;

                DataTable listWms = new DataTable();
                listWms.Columns.Add("mod_ts");
                listWms.Columns.Add("facility_id");
                listWms.Columns.Add("order_nbr");
                listWms.Columns.Add("dest_dept_nbr");
                listWms.Columns.Add("total_orig_ord_qty");
                listWms.Columns.Add("orig_sku_count");
                listWms.Columns.Add("cust_field_2");
                listWms.Columns.Add("cust_field_3");
                listWms.Columns.Add("create_ts");
                foreach (WmsData data in lista)
                {
                    DataRow dRow = listWms.NewRow();
                    dRow["mod_ts"] = data.mod_ts;
                    dRow["facility_id"] = data.facility_id.key;
                    dRow["order_nbr"] = data.order_nbr;
                    dRow["dest_dept_nbr"] = data.dest_dept_nbr;
                    dRow["total_orig_ord_qty"] = data.total_orig_ord_qty;
                    dRow["orig_sku_count"] = data.orig_sku_count;
                    dRow["cust_field_2"] = data.cust_field_2;
                    dRow["cust_field_3"] = data.cust_field_3;
                    dRow["create_ts"] = data.create_ts;
                    listWms.Rows.Add(dRow);
                }

                string conn = ConfigurationManager.ConnectionStrings["SQL_WMS"].ConnectionString;
                using (SqlConnection cn = new SqlConnection(conn))
                {
                    if (cn.State == 0) cn.Open();
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            cmd.Connection = cn;
                            cmd.CommandTimeout = 0;
                            cmd.CommandText = "USP_VERIFICAR_ANULACIONES_WMS_SIS_REGISTRADOS";
                            cmd.CommandType = CommandType.StoredProcedure;
                            var parameter = new SqlParameter
                            {
                                SqlDbType = SqlDbType.Structured,
                                ParameterName = "@T_ESTADOS",
                                Value = listWms
                            };
                            cmd.Parameters.Add(parameter);
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                dt = new DataTable();
                                da.Fill(dt);
                                listaValidada = new List<WmsData>();
                                listaValidada = (from DataRow dr in dt.Rows
                                                 select new WmsData()
                                                 {
                                                     mod_ts = dr["mod_ts"].ToString(),
                                                     facility_id = new Facility_Id { id = "1", key = dr["facility_id"].ToString(), url = "" },
                                                     order_nbr = dr["order_nbr"].ToString(),
                                                     dest_dept_nbr = dr["dest_dept_nbr"].ToString(),
                                                     total_orig_ord_qty = dr["total_orig_ord_qty"].ToString(),
                                                     orig_sku_count = dr["orig_sku_count"].ToString(),
                                                     cust_field_2 = dr["orig_sku_count"].ToString(),
                                                     cust_field_3 = dr["cust_field_3"].ToString(),
                                                     create_ts = dr["create_ts"].ToString(),

                                                 }).ToList();
                            }
                        }
                        catch (Exception Ex)
                        {
                            LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
                            throw;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
                listaValidada = null;
            }
            return listaValidada;
        }




        public void CrearArchivoAnulados(List<WmsDataFinal> listaWmsFinal)
        {
            try
            {
                string fileTXT = "";
                string fechor = "WA" + DateTime.Now.ToString("ddMMyy") + ".TXT";
                fileTXT = Path.Combine(ConfigurationManager.AppSettings["pathTxtFinal"].ToString().Trim(), fechor);
                StringBuilder str = new StringBuilder();
                foreach (WmsDataFinal listaFinal in listaWmsFinal)
                {
                    str.Append(listaFinal.Sucursal + ",");
                    str.Append(listaFinal.order_nbr);
                    str.Append("\r\n");
                }
                File.WriteAllText(fileTXT, str.ToString());
            }
            catch (Exception Ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", Ex.Message.ToString(), false, "");
            }

        }

    }
}
