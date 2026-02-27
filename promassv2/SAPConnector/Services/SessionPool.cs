using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using SAPbobsCOM;
using SAPConnector.VO;

namespace SAPConnector.Services
{
    public class SessionPool
    {
        private static ILog log = LogManager.GetLogger(typeof(SessionPool));

        private static readonly ConcurrentQueue<Sesion> sesionesDisponibles = new ConcurrentQueue<Sesion>();
        private static readonly Dictionary<Guid, Sesion> sesionesUso = new Dictionary<Guid, Sesion>();
        private static int poolSessionsMin = 0;
        private static int poolSessionsMax = 0;

        static SessionPool()
        {
            poolSessionsMin = int.Parse(ConfigurationManager.AppSettings["PoolSessionsMin"]);
            poolSessionsMax = int.Parse(ConfigurationManager.AppSettings["PoolSessionsMax"]);

            log.Info("Iniciando pool de sesiones SAP: " + poolSessionsMin);

            //CrearSesiones(poolSessionsMin);

            log.Info("Sesiones Creadas: " + poolSessionsMin);
        }

        private static void CrearSesiones(int cantidad)
        {
            for (var i = 0; i < cantidad; i++)
            {
                sesionesDisponibles.Enqueue(CrearSesion());
            }
        }

        // Este metodo es solo para invocar el constructor
        public static void Init()
        {
        }

        public static void ShutDown()
        {
            // Cerrar las sesiones disponibles
            foreach (Sesion sesion in sesionesDisponibles)
                if (sesion.company != null && sesion.company.Connected)
                    sesion.company.Disconnect();

            // Cerrar las sesiones en uso, hacer rollback a transacciones pendientes
            foreach (Guid sesionKey in sesionesUso.Keys)
            {
                Sesion sesion = sesionesUso[sesionKey];

                if (sesion.company != null && sesion.company.Connected)
                {
                    if (sesion.company.InTransaction) sesion.company.EndTransaction(BoWfTransOpt.wf_RollBack);
                    sesion.company.Disconnect();
                }
            }
        }

        private static Sesion CrearSesion()
        {
            log.Debug("   Creando sesion SAP");

            var company = new Company();

            company.DbServerType = BoDataServerTypes.dst_HANADB;

            company.Server = ConfigurationManager.AppSettings["DBServer"];
            company.DbUserName = ConfigurationManager.AppSettings["DBUserName"];
            company.DbPassword = ConfigurationManager.AppSettings["DBPassword"];
            company.CompanyDB = ConfigurationManager.AppSettings["CompanyDB"];
            company.UseTrusted = false;

            company.LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
            company.UserName = ConfigurationManager.AppSettings["SAPUserName"];
            company.Password = ConfigurationManager.AppSettings["SAPPassword"];

            company.language = BoSuppLangs.ln_Spanish_La;

            if (company.Connect() != 0)
            {
                string messageError = company.GetLastErrorDescription();
                throw new Exception(messageError);
            }                

            Sesion sesion = new Sesion();
            sesion.company = company;

            return sesion;
        } // Conectar

        public static Sesion getSession()
        {
            Sesion sesion = null;

            if (sesionesDisponibles.Count == 0) SessionPool.Refresh();

            if (sesionesDisponibles.TryDequeue(out sesion))
            {
                sesionesUso.Add(sesion.uui, sesion);
            }

            return sesion;
        }

        public static void DevolverSesion(Guid uuid)
        {
            Sesion sesion = sesionesUso[uuid];

            if (sesion != null)
            {
                sesionesUso.Remove(sesion.uui);
                sesionesDisponibles.Enqueue(sesion);
            }
            else
            {
                log.Error("No se encontro la sesion a devolver");
            }
        }

        public static SessionMetricsVO GetMetrics()
        {
            SessionMetricsVO metrics = new SessionMetricsVO();

            List<MetricVO> enusoMetrics = new List<MetricVO>();
            foreach (Sesion sesionUso in sesionesUso.Values)
            {
                MetricVO metric = new MetricVO();
                metric.uui = sesionUso.uui.ToString();
                metric.born = sesionUso.born.ToString();

                enusoMetrics.Add(metric);
            }

            List<MetricVO> disponiblesMetrics = new List<MetricVO>();
            foreach (Sesion sesionesDisponible in sesionesDisponibles)
            {
                MetricVO metric = new MetricVO();
                metric.uui = sesionesDisponible.uui.ToString();
                metric.born = sesionesDisponible.born.ToString();

                disponiblesMetrics.Add(metric);
            }

            metrics.SesionesDisponibles = disponiblesMetrics;
            metrics.SesionesEnUso = enusoMetrics;

            return metrics;
        }

        public static int Refresh()
        {            
            var sesionesMuertas = 0;

            log.Debug("Refrescando sesiones. Sesiones disponibles " + sesionesDisponibles.Count);

            // Solo refrescar las sesiones si hay 2 o mas disponibles
            if (sesionesDisponibles.Count > 1)
            {
                for (var i = 0; i < sesionesDisponibles.Count; i++)
                {
                    Sesion sesion = null;

                    if (sesionesDisponibles.TryDequeue(out sesion) == false) continue;

                    if (sesion.company == null)
                    {
                        sesionesMuertas++;
                        continue;
                    }

                    if (sesion.company.Connected == false)
                    {
                        sesionesMuertas++;
                        continue;
                    }

                    sesionesDisponibles.Enqueue(sesion);
                }

                //CrearSesiones(sesionesMuertas);
            }

            // Si solo hay 2 o menos sesiones disponibles, agregar 2 mas hasta alcanzar el maximo
            if (sesionesDisponibles.Count < 3)
            {
                log.Debug("Pocas sesiones disponibles: " + sesionesDisponibles.Count);


                int totalSesiones = sesionesDisponibles.Count + sesionesUso.Count;
                if (totalSesiones < poolSessionsMax) //CrearSesiones(2);

                log.Debug("Se agregaron sesiones. Disponibles: " + sesionesDisponibles.Count + ", En Uso: " + sesionesUso.Count);
            }

            return sesionesMuertas;
        }

        public static SessionMetricsVO Reiniciar()
        {
            log.Debug("Reiniciando pool");

            log.Debug("Matar las sesiones en uso: " + sesionesUso.Count);
            foreach (Guid sesionKey in sesionesUso.Keys)
            {
                Sesion sesion = sesionesUso[sesionKey];

                if (sesion.company != null && sesion.company.Connected)
                {
                    if (sesion.company.InTransaction)
                    {
                        log.Debug("Sesion en Transaccion, haciendo rollback");
                        sesion.company.EndTransaction(BoWfTransOpt.wf_RollBack);
                    }

                    log.Debug("Desconectando sesion");
                    sesion.company.Disconnect();

                    if (sesion.company.Connected)
                    {
                        log.Error("***** ERROR: No se pudo desconectar la sesion *****");
                    }
                    else
                    {
                        log.Debug("Sesion desconectada y nulificada");
                        sesion.company = null;
                        sesionesUso.Remove(sesionKey);
                    }

                }
            }

            log.Debug("Cerrar la cola de sesiones disponibles: " + sesionesDisponibles.Count);
            int totalSesiones = sesionesDisponibles.Count;
            for (int index = 0; index < totalSesiones; index++)
            {
                Sesion sesion = null;
                if (sesionesDisponibles.TryDequeue(out sesion) == false) continue;

                if (sesion.company != null)
                {
                    sesion.company.Disconnect();
                    sesion.company = null;
                }
            }

            log.Debug("Recargar el pool");
            CrearSesiones(5);

            log.Debug("Reinicio Terminado");

            return GetMetrics();
        }
    } // SessionPool
}
