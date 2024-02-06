using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using wsEstadosWMSSIS.Business;

namespace wsEstadosWMSSIS
{
    public partial class ServicioEstadosWmsSis : ServiceBase
    {
        public ServicioEstadosWmsSis()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            API_Wms api_Wms = new API_Wms();
            api_Wms.ConsultarApiEstadosWms();
        }

        protected override void OnStop()
        {
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
