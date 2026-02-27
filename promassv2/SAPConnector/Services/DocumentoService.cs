using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using SAPConnector.VO;
using SAPbobsCOM;
using SharpRaven;
using SharpRaven.Data;
using System.Configuration;

namespace SAPConnector.Services
{
    public abstract class DocumentoService
    {
        private static ILog log = LogManager.GetLogger(typeof(DocumentoService));
        RavenClient ravenClient = new RavenClient("https://28ea473010444f369e380b7089fd3747@sentry.io/1217101");

        protected ResultadoVO resultadoVO = null;
        protected DocumentoVO documentoVO = null;
        protected SAPbobsCOM.Company company = null;
        protected string codigoError = "";
        protected string mensajeError = "";

        public ResultadoVO crearDocumento(DocumentoVO documento)
        {
            Sesion sesion = null;

            try
            {
                this.documentoVO = documento;

                CrearDocumentoSAP();
            }
            catch (Exception e)
            {
                log.Error(e);

                ResultadoVO resultadoVO = new ResultadoVO();
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = e.Message;

                this.resultadoVO = resultadoVO;

                ravenClient.Capture(new SharpRaven.Data.SentryEvent(e));
            }
            finally
            {
                Desconectar();
            }

            return this.resultadoVO;
        }

        protected abstract void CrearDocumentoSAP();

        protected void ObtenerResultado(bool exito)
        {
            if (exito)
            {
                string docEntry = this.company.GetNewObjectKey();
                resultadoVO = new ResultadoVO();
                resultadoVO.Exito = true;
                resultadoVO.DocEntry = docEntry;
                resultadoVO.Mensaje = "";
            }
            else
            {
                raiseError();
            }
        }

        protected void raiseError()
        {
            resultadoVO = new ResultadoVO();
            resultadoVO.Exito = false;
            resultadoVO.Mensaje = company.GetLastErrorDescription();

            log.Error(resultadoVO);
        }

        protected void Conectar (string companyDB, string empresaId)
        {
            log.Debug("   Creando conexion a SAP");

            var nuevaConexion = new Company();

            nuevaConexion.DbServerType = BoDataServerTypes.dst_HANADB;

            nuevaConexion.Server = ConfigurationManager.AppSettings["DBServer"];
            nuevaConexion.DbUserName = ConfigurationManager.AppSettings["DBUserName"];
            nuevaConexion.DbPassword = ConfigurationManager.AppSettings["DBPassword"];
            nuevaConexion.CompanyDB = companyDB;
            nuevaConexion.UseTrusted = false;

            nuevaConexion.LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
            nuevaConexion.UserName = ConfigurationManager.AppSettings["SAPUserName"];
            nuevaConexion.Password = ConfigurationManager.AppSettings["SAPPassword_" + empresaId];

            nuevaConexion.language = BoSuppLangs.ln_Spanish_La;

            if (nuevaConexion.Connect() != 0)
            {
                throw new Exception("No se pudo realizar la conexion a SAP. Base de datos: " + companyDB);
            }

            this.company = nuevaConexion;
        }

        private void Desconectar()
        {
            if (this.company != null && this.company.Connected)
            {
                this.company.Disconnect();
            }
        }

        protected void asignarCampoString(Fields campos, String name, String valor)
        {
            try
            {
                campos.Item(name).Value = valor;
            }
            catch (Exception e)
            {
                string mensaje = "Error en el campo " + name + ": " + e.Message;
                log.Error(mensaje);

                SentryEvent sentryEvent = new SentryEvent(e);
                sentryEvent.Tags.Add("Campo", name);
                sentryEvent.Tags.Add("Valor", valor);
                ravenClient.Capture(sentryEvent);

                throw new Exception(mensaje);                
            }
        }

        protected void asignarCampoDouble(Fields campos, String name, double valor)
        {
            try
            {
                campos.Item(name).Value = valor;
            }
            catch (Exception e)
            {
                string mensaje = "Error en el campo " + name + ": " + e.Message;
                log.Error(mensaje);

                SentryEvent sentryEvent = new SentryEvent(e);
                sentryEvent.Tags.Add("Campo", name);
                sentryEvent.Tags.Add("Valor", valor.ToString());
                ravenClient.Capture(sentryEvent);

                throw new Exception(mensaje);
            }
        }

    } // DocumentoService
}
