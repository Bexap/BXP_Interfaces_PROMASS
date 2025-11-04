using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using SAPConnector.Services;
using ServiciosSalida.Tasks;

namespace ServiciosSalida
{
    class Program
    {
        private static ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            log.Info("***** Ejecutando Interfases *****");

            ProveedoresTask.EnviarTodas();
            OrdenesCompraTask.EnviarTodas();
            CancelarOrdenesCompraTask.EnviarTodas();
            CerrarOrdenesCompraTask.EnviarTodas();

            log.Info("Fin de Ejecucion");
        }
    }
}
