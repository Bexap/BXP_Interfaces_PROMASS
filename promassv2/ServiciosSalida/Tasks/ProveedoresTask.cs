using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.VO;
using log4net;
using ServiciosSalida.Services;

namespace ServiciosSalida.Tasks
{
    class ProveedoresTask
    {
        private static ILog log = LogManager.GetLogger(typeof(ProveedoresTask));

        public static void EnviarTodas()
        {
            Dictionary<string, EntidadVO> entidades = EntidadVO.getEntidades();

            foreach (KeyValuePair<string, EntidadVO> entidadVO in entidades)
            {
                EntidadVO.EntidadActual = entidadVO.Value;
                if (EntidadVO.EntidadActual.IsServiciosSalida)
                {
                    Enviar();
                }                
            }
        }

        public static void Enviar()
        {
            try
            {
                log.Info("Ejecutando tarea ProveedoresTask");
                bool isEnviar = false;

                ProveedorService proveedorService = new ProveedorService();

                if (proveedorService.isNuevos())
                {
                    log.Info("Hay NUEVOS proveedores");
                    isEnviar = true;
                }
                else
                {
                    log.Info("No hay NUEVOS proveedores por procesar");
                }

                if (proveedorService.isActualizaciones())
                {
                    log.Info("Hay ACTUALIZACIONES de proveedores por enviar");

                    proveedorService.InsertarActualizaciones();
                    isEnviar = true;
                }
                else
                {
                    log.Info("No se encontraron ACTUALIZACIONES de proveedores por enviar");
                }

                if (proveedorService.isPendientes())
                {
                    log.Info("Hay proveedores PENDIENTES por procesar");

                    isEnviar = true;
                }

                if (isEnviar)
                {
                    List<ProveedorVO> proveedoresList = proveedorService.getProveedores();

                    log.Info("Enviando proveedores: " + proveedoresList.Count);

                    proveedorService.procesarProveedores(proveedoresList);
                }

                log.Info("Fin de tarea ProveedoresTask");
            }
            catch (Exception e)
            {
                log.Error("Excepcion en el proceso de Proveedores");
                log.Error(e.Message);

                // ravenClient.Capture(new SentryEvent(e));
            }
        }

    } // ProveedoresTask
}
