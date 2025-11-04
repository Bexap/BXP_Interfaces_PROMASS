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
    class CancelarOrdenesCompraTask
    {
        private static ILog log = LogManager.GetLogger(typeof(CancelarOrdenesCompraTask));

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
                log.Info("Ejecutando Tarea CancelarOrdenesCompraTask");
                bool isEnviar = false;

                CancelarOrdenCompraService ordenCompraService = new CancelarOrdenCompraService();

                if (ordenCompraService.isCancelaciones())
                {
                    log.Info("Hay CANCELACIONES de Ordenes de Compra");
                    isEnviar = true;
                }
                else
                {
                    log.Info("No se encontraron CANCELACIONES de Ordenes de Compra");
                }

                if (ordenCompraService.isPendientes())
                {
                    log.Info("Hay CANCELACIONES de Ordenes de Compra PENDIENTES por procesar");

                    isEnviar = true;
                }

                if (isEnviar)
                {
                    List<OrdenCompraVO> ordenesList = ordenCompraService.getOrdenesCompra();

                    log.Info("Enviando cancelaciones ordenes de compra: " + ordenesList.Count);

                    ordenCompraService.procesarOrdenesCompra(ordenesList);
                }

                log.Info("Fin de tarea CancelarOrdenesCompraTask");
            }
            catch (Exception e)
            {
                log.Error("Excepcion en el proceso de Cancelacion de  Ordenes de Compra");
                log.Error(e.Message);
            }
        }
    }
}
