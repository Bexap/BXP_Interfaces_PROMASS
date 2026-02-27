using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using log4net;
using log4net.Config;
using SAPConnector.Services;

namespace ServiciosEntrada
{
    public class Global : System.Web.HttpApplication
    {
        private static ILog log = LogManager.GetLogger(typeof(Global));

        protected void Application_Start(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();

            log.Info("Iniciando Interfases SAP");

            //SessionPool.Init();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            log.Info("Deteniendo Interfases SAP");

            // Cerrar las sesiones del pool
            SessionPool.ShutDown();

            log.Info("Pool terminado");
        }

    } // Global
}