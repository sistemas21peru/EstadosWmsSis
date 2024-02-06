using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace wsEstadosWMSSIS.Business
{
    public class API_Wms
    {


        public void ConsultarApiEstadosWms()
        {
            try
            {
                string url = ConfigurationManager.AppSettings["Api_Url"].ToString() + "order_hdr?status_id=99&mod_ts__month=01&mod_ts__year=2023";
                dynamic response = "";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings["Api_User"].ToString() + ":" + ConfigurationManager.AppSettings["Api_Pass"].ToString())));

                    using (StringContent jsonContent = new StringContent(""))
                    {
                        jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        System.Net.ServicePointManager.SecurityProtocol =  System.Net.SecurityProtocolType.Tls12;
                        var request = client.GetAsync(url);
                        response = request.Result.Content.ReadAsStringAsync().Result;

                        JObject root = JObject.Parse(response);
                        var lista = JsonConvert.SerializeObject(root["results"]);



                    }
                }

            }
            catch (Exception Ex)
            {
                // DatosGenerales.TXTLog("ESTADO_HISTORIAL_PEDIDO_COURIER", Ex.Message.ToString());
            }
        }
    }
}
