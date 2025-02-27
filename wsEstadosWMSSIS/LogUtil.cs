using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace ServicioEstadosWmsSis
{
    public static class LogUtil
    {


        public static void Graba_Log(string interfaz, string error_msg, bool flag_msg, string NomArchivo)
        {
            try
            {
           
                string filtxt = ConfigurationManager.AppSettings["pathExeFox"].ToString().Trim() +  interfaz.Trim() + ".TXT";

              //  string filtxt = @"d:\" + interfaz.Trim() + ".TXT";

                using (StreamWriter writer = new StreamWriter(filtxt, true))
                {
                    if (NomArchivo.Trim() == "")
                    {
                        writer.WriteLine("Fecha: " + DateTime.Now.ToString() + " " + error_msg);
                    }
                    else
                    {
                        writer.WriteLine("Fecha: " + DateTime.Now.ToString() + " " + error_msg + " Archivo: " + NomArchivo);
                    }

                }

            }
            catch (Exception ex)
            {
                LogUtil.Graba_Log("Log_Estados_Sis", "" + " ERROR: " + ex.ToString(), true, "");

            }

            return;
        }





    }
}
