using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using wsEstadosWMSSIS.Business;
using System.Configuration;
using ServicioEstadosWmsSis;

namespace wsEstadosWMSSIS
{
    public partial class ServicioEstadosWmsSis : ServiceBase
    {
        Timer tmProcesaEstado = null;
        Int32 wenProceso_Estados = 0;
        Int32 wenEnvio_Correo_Estados = 0;
        int wenDiaEnvioAnterior = 0;
        List<string> diasEnvioCorreo = new List<string>();
        
        public ServicioEstadosWmsSis()
        {
            //double minutos = 60000;
            InitializeComponent();
            LogUtil.Graba_Log("Log_Estados_Sis", "Inicio de Servicio Estado Wms-SIS", false, "");
            //tmProcesaEstado = new Timer(1 * minutos); //10 min
            //tmProcesaEstado.Elapsed += new ElapsedEventHandler(tmpProcesa_Estados_Elapsed);
        }

        void tmpProcesa_Estados_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (wenProceso_Estados == 0)
            {
                wenProceso_Estados = 1;
                API_Wms api_Wms = new API_Wms();
                api_Wms.ProcesarEstadosWmsSis();
                wenProceso_Estados = 0;
                EnviaCorreoEstados();
            }
        }

        void EnviaCorreoEstados()
        {
           DateTime fechaActual = DateTime.Now;
            if (wenEnvio_Correo_Estados == 1)
            {
                if (fechaActual.Day != wenDiaEnvioAnterior)
                {
                    wenEnvio_Correo_Estados = 0;
                }
                return;
            }

                     
            if (diasEnvioCorreo.Contains(fechaActual.DayOfWeek.ToString()))
            {
                wenDiaEnvioAnterior = fechaActual.Day;
                wenEnvio_Correo_Estados = 1;
                API_Wms api_Wms = new API_Wms();
                api_Wms.EnviarCorreoReporteEstados();
            }


        }


        private void BindListaDiasEnvioCorreo()
        {
            if (int.Parse(ConfigurationManager.AppSettings["dia_envio_correo_lunes"].ToString()) == 1)
            {
                diasEnvioCorreo.Add(DayOfWeek.Monday.ToString());
            }
            if (int.Parse(ConfigurationManager.AppSettings["dia_envio_correo_martes"].ToString()) == 1)
            {
                diasEnvioCorreo.Add(DayOfWeek.Tuesday.ToString());
            }
            if (int.Parse(ConfigurationManager.AppSettings["dia_envio_correo_miercoles"].ToString()) == 1)
            {
                diasEnvioCorreo.Add(DayOfWeek.Wednesday.ToString());
            }
            if (int.Parse(ConfigurationManager.AppSettings["dia_envio_correo_jueves"].ToString()) == 1)
            {
                diasEnvioCorreo.Add(DayOfWeek.Thursday.ToString());
            }
            if (int.Parse(ConfigurationManager.AppSettings["dia_envio_correo_viernes"].ToString()) == 1)
            {
                diasEnvioCorreo.Add(DayOfWeek.Friday.ToString());
            }
            if (int.Parse(ConfigurationManager.AppSettings["dia_envio_correo_sabado"].ToString()) == 1)
            {
                diasEnvioCorreo.Add(DayOfWeek.Saturday.ToString());
            }
            if (int.Parse(ConfigurationManager.AppSettings["dia_envio_correo_doningo"].ToString()) == 1)
            {
                diasEnvioCorreo.Add(DayOfWeek.Sunday.ToString());
            }

        }
        protected override void OnStart(string[] args)
        {
            BindListaDiasEnvioCorreo();
            EnviaCorreoEstados();

            //API_Wms api_Wms = new API_Wms();
            //api_Wms.ProcesarEstadosWmsSis();
            // tmProcesaEstado.Start();

        }

        protected override void OnStop()
        {
          //  tmProcesaEstado.Stop();
        }

        internal void TestStartupAndStop()
        {
            string[] arg = new string[] { }; ;
            this.OnStart(arg);
            Console.ReadLine();
            this.OnStop();
        }
    }
}
