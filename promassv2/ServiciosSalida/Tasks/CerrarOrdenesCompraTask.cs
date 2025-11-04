using System;
using System.Collections.Generic;
using Common.VO;
using log4net;
using ServiciosSalida.Services;

namespace ServiciosSalida.Tasks
{
    class CerrarOrdenesCompraTask
    {
        private static ILog log = LogManager.GetLogger(typeof(CerrarOrdenesCompraTask));

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
                log.Info("Ejecutando Tarea CerrarOrdenesCompraTask");
                bool isEnviar = false;

                var ordenCompraService = new CerrarOrdenCompraService();

                if (ordenCompraService.isCancelaciones())
                {
                    log.Info("Hay CIERRES de Ordenes de Compra");
                    isEnviar = true;
                }
                else
                {
                    log.Info("No se encontraron CIERRES de Ordenes de Compra");
                }

                if (ordenCompraService.isPendientes())
                {
                    log.Info("Hay CIERRES de Ordenes de Compra PENDIENTES por procesar");

                    isEnviar = true;
                }

                if (isEnviar)
                {
                    List<OrdenCompraVO> ordenesList = ordenCompraService.getOrdenesCompra();

                    log.Info("Enviando cierres ordenes de compra: " + ordenesList.Count);

                    ordenCompraService.procesarOrdenesCompra(ordenesList);
                }

                log.Info("Fin de tarea CerrarOrdenesCompraTask");
            }
            catch (Exception e)
            {
                log.Error("Excepcion en el proceso de Cierre de  Ordenes de Compra");
                log.Error(e.Message);
            }
        }
    }
}
