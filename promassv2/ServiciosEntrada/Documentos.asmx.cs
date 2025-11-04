using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Common.VO;
using SAPConnector.VO;
using ServiciosEntrada.Services;
using log4net;

namespace ServiciosEntrada
{
    /// <summary>
    /// Summary description for Documentos
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Documentos : System.Web.Services.WebService
    {
        private static ILog log = LogManager.GetLogger(typeof(Documentos));

        [WebMethod]
        public ResultadoVO CrearOrdenCompra(OrdenCompraVO ordenCompraVO)
        {
            ResultadoVO resultadoVO = new ResultadoVO();

            OrdenCompraService ordenCompra = new OrdenCompraService();

            try
            {
                log.Debug("CrearOrdenCompra");
                log.Debug(ordenCompraVO);

                if (OrdenCompraService.peticionesList.TryAdd(ordenCompraVO.FolioCAS, ordenCompraVO))
                {
                    resultadoVO = ordenCompra.crearDocumento(ordenCompraVO);
                }
                else
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje =
                        $"Ya se esta procesando un Pedido con folio CAS {ordenCompraVO.FolioCAS}. Intente mas tarde";
                }
            }
            catch (Exception e)
            {
                resultadoVO = new ResultadoVO();
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = e.Message;   
            }
            finally
            {
                OrdenCompraService.peticionesList.TryRemove(ordenCompraVO.FolioCAS, out _);
            }

            return resultadoVO;
        }

        [WebMethod]
        public ResultadoVO CrearOrdenVenta(OrdenVentaVO ordenVentaVO)
        {
            var resultadoVO = new ResultadoVO();
            var ordenVenta = new OrdenVentaService();

            try
            {
                log.Debug("CrearOrdenVenta");
                log.Debug(ordenVentaVO);

                if (OrdenVentaService.peticionesList.TryAdd(ordenVentaVO.FolioCAS, ordenVentaVO))
                {
                    resultadoVO = ordenVenta.crearDocumento(ordenVentaVO);
                }
                else
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje =
                        $"Ya se esta procesando un Pedido con folio CAS {ordenVentaVO.FolioCAS}. Intente mas tarde";
                }
            }
            catch (Exception e)
            {
                resultadoVO = new ResultadoVO();
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = e.Message;
            }
            finally
            {
                OrdenVentaService.peticionesList.TryRemove(ordenVentaVO.FolioCAS, out _);
            }

            log.Debug(resultadoVO);

            return resultadoVO;
        }

        [WebMethod]
        public ResultadoVO CrearFactura(FacturaVO facturaVO)
        {
            var guid = Guid.NewGuid().ToString();

            log.Debug("Dentro del WebService CrearFactura");
            log.Debug($"{guid} = {facturaVO}");

            ResultadoVO resultadoVO = new ResultadoVO();

            FacturaService facturaService = new FacturaService();

            try
            {
                log.Debug("Ejecutando servicio CrearFactura");
                resultadoVO = facturaService.crearDocumento(facturaVO);
            }
            catch (Exception ex)
            {
                log.Error(guid + " = Ocurrio una excepcion");
                log.Error(guid + " + " + ex.Message);

                resultadoVO = new ResultadoVO();
                resultadoVO.Exito = false;
                resultadoVO.DocEntry = "";
                resultadoVO.Mensaje = ex.Message;
            }

            if (resultadoVO.Exito && (String.IsNullOrEmpty(resultadoVO.DocEntry) || resultadoVO.DocEntry == "0"))
            {
                log.Error($"{guid} = ***** REVISAR *****");
                resultadoVO.Exito = false;
            }

            log.Debug($"{guid} + {resultadoVO}");

            return resultadoVO;
        }

    } // Documentos
}
