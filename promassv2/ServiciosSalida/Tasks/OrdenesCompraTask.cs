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
    class OrdenesCompraTask
    {
        private static ILog log = LogManager.GetLogger(typeof(OrdenesCompraTask));

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
                log.Info("Ejecutando Tarea OrdenesCompraTask");
                bool isEnviar = false;

                OrdenCompraService ordenCompraService = new OrdenCompraService();

                if (ordenCompraService.isNuevos())
                {
                    log.Info("Hay NUEVAS Ordenes de Compra");
                    isEnviar = true;
                }
                else
                {
                    log.Info("No se encontraron NUEVAS Ordenes de Compra");
                }

                if (ordenCompraService.isActualizaciones())
                {
                    log.Info("Hay ACTUALIZACIONES de Ordenes de Compra por procesar");

                    ordenCompraService.InsertarActualizaciones();
                    isEnviar = true;
                }
                else
                {
                    log.Info("No se encontraron ACTUALIZACIONES de Ordenes de Compra por Procesar");
                }

                if (ordenCompraService.isPendientes())
                {
                    log.Info("Hay ordenes de compra PENDIENTES por procesar");

                    isEnviar = true;
                }

                if (isEnviar)
                {
                    List<OrdenCompraVO> ordenesList = ordenCompraService.getOrdenesCompra();

                    log.Info("Enviando ordenes de compra: " + ordenesList.Count);

                    ordenCompraService.procesarOrdenesCompra(ordenesList);
                }

                log.Info("Fin de tarea OrdenesCompraTask");
            }
            catch (Exception e)
            {
                log.Error("Excepcion en el proceso de Ordenes de Compra");
                log.Error(e.Message);

                // ravenClient.Capture(new SentryEvent(e));
            }
        }

    } // OrdenesCompraTask
}
