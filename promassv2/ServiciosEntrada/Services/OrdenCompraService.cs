using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Common.VO;
using log4net;
using SAPbobsCOM;
using SAPConnector.Services;
using SAPConnector.VO;
using ServiciosEntrada.DAO;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace ServiciosEntrada.Services
{
    public class OrdenCompraService : DocumentoService
    {
        private static ILog log = LogManager.GetLogger(typeof(OrdenCompraService));
        private OrdenCompraVO ordenCompraVO = null;
        public static ConcurrentDictionary<string, OrdenCompraVO> peticionesList = new ConcurrentDictionary<string, OrdenCompraVO>();

        protected override void CrearDocumentoSAP()
        {
            try
            {
                this.ordenCompraVO = (OrdenCompraVO)this.documentoVO;
                
                log.Info("Procesando Orden de Compra");
                log.Info(this.ordenCompraVO);
            }
            catch (Exception ex)
            {

                log.Error("Error al hacer cast de documentoVO a OrdenCompraVO", ex);
                throw;
            }

            ResultadoVO validacionVO = ValidarRequest();
            if (validacionVO.Exito == false)
            {
                log.Error("La orden de compra no es valida");
                this.resultadoVO = validacionVO;
                return;
            }

            try
            {
                log.Info("Validación OK");

                var entidad = EntidadVO.EntidadActual;
                var client = new ServiciosSAP.ServiceLayerClient();

                log.Info("Construyendo documento para SAP");

                var documento = new Dictionary<string, object>()
        {
            { "CardCode", ordenCompraVO.CardCode },
            { "DocDate", ordenCompraVO.DocDate.ToString("yyyy-MM-dd") },
            { "DocCurrency", ordenCompraVO.CodigoMoneda },
            { "Series", int.Parse(ordenCompraVO.Serie) },
            { "U_B1SYS_MainUsage", "G03" },
            { "U_BXP_CAS", ordenCompraVO.FolioCAS },
            { "U_BXP_Folio", ordenCompraVO.FolioRecepcion }
        };

                if (ConfigurationManager.AppSettings["CamposInformativos"] == "SI")
                {
                    log.Info("Agregando campos informativos");

                    documento.Add("U_CUENTA", ordenCompraVO.Cuenta);
                    documento.Add("U_CLIENTE", ordenCompraVO.Cliente);
                    documento.Add("U_ID_PROVEEDOR", ordenCompraVO.IDProveedor);
                    documento.Add("U_PROVEEDOR", ordenCompraVO.Proveedor);
                    documento.Add("U_ID_COMPANIA", ordenCompraVO.IDCompania);
                    documento.Add("U_COMPANIA", ordenCompraVO.Compania);
                    documento.Add("U_ESTATUS_REPORTE", ordenCompraVO.EstatusReporte);
                    documento.Add("U_ESTATUS_SERVICIO", ordenCompraVO.EstatusServicio);
                    documento.Add("U_ID_SERVICIO", ordenCompraVO.IDServicio);
                    documento.Add("U_SERVICIO", ordenCompraVO.Servicio);
                    documento.Add("U_ID_SUBSERVICIO", ordenCompraVO.IDSubServicio);
                    documento.Add("U_SUBSERVICIO", ordenCompraVO.SubServicio);
                    documento.Add("U_RETENCION", ordenCompraVO.Retencion);
                    documento.Add("U_SUB_TOTAL", ordenCompraVO.SubTotal);
                    documento.Add("U_IVA", ordenCompraVO.Iva);
                    documento.Add("U_TOTAL", ordenCompraVO.Total);
                    documento.Add("U_CARGO_CLIENTE", ordenCompraVO.CargoCliente);
                    documento.Add("U_FORANEO_LOCAL", ordenCompraVO.ForaneoLocal);
                    documento.Add("U_TIPO_ASIGNADOR", ordenCompraVO.TipoAsignador);
                    documento.Add("U_COSTO_REAL", ordenCompraVO.CostoReal);
                    documento.Add("U_FECHA_HORA_ALTA_CAS", ordenCompraVO.FechaHoraAltaCAS.ToString("dd/MM/yyyy hh:mm:ss tt", new CultureInfo("es-MX")));
                    documento.Add("U_FECHA_HORA_ASIGNACION", ordenCompraVO.FechaHoraAsignacion.ToString("dd/MM/yyyy hh:mm:ss tt", new CultureInfo("es-MX")));
                    documento.Add("U_FECHA_HORA_ARRIBO", ordenCompraVO.FechaHoraArribo.ToString("dd/MM/yyyy hh:mm:ss tt", new CultureInfo("es-MX")));
                    documento.Add("U_FECHA_HORA_TERMINO", ordenCompraVO.FechaHoraTermino.ToString("dd/MM/yyyy hh:mm:ss tt", new CultureInfo("es-MX")));
                    documento.Add("U_CIUDAD_ORIGEN", ordenCompraVO.CiudadOrigen);
                    documento.Add("U_ESTADO_ORIGEN", ordenCompraVO.EstadoOrigen);
                    documento.Add("U_CIUDAD_DESTINO", ordenCompraVO.CiudadDestino);
                    documento.Add("U_ESTADO_DESTINO", ordenCompraVO.EstadoDestino);
                    documento.Add("U_LATITUD_ORIGEN", ordenCompraVO.LatitudOrigen);
                    documento.Add("U_LONGITUD_ORIGEN", ordenCompraVO.LongitudOrigen);
                    documento.Add("U_LATITUD_DESTINO", ordenCompraVO.LatitudDestino);
                    documento.Add("U_LONGITUD_DESTINO", ordenCompraVO.LongitudDestino);
                    documento.Add("U_LATITUD_PROVEEDOR", ordenCompraVO.LatitudProveedor);
                    documento.Add("U_LONGITUD_PROVEEDOR", ordenCompraVO.LongitudProveedor);
                    documento.Add("U_EJECUTIVO", ordenCompraVO.Ejecutivo);
                    documento.Add("U_KILOMETROS_CLIENTE", ordenCompraVO.KilometrosCliente);
                    documento.Add("U_KILOMETROS_PROV_CLIENTE", ordenCompraVO.KilometrosProvCliente);
                    documento.Add("U_ARRASTRE_SERVICIO", ordenCompraVO.ArrastreServicio);
                    documento.Add("U_BANDERAZO", ordenCompraVO.Banderazo);
                    documento.Add("U_COSTO_KM", ordenCompraVO.CostoKM);
                    documento.Add("U_MANIOBRAS", ordenCompraVO.Maniobras);
                    documento.Add("U_GASOLINA", ordenCompraVO.Gasolina);
                    documento.Add("U_CASETAS", ordenCompraVO.Casetas);
                    documento.Add("U_CORRESPONSALIA", ordenCompraVO.Corresponsalia);
                    documento.Add("U_MATERIAL", ordenCompraVO.Material);
                }

                if (ordenCompraVO.partidas == null || !ordenCompraVO.partidas.Any())
                    throw new Exception("No hay partidas en la orden");

                documento["DocumentLines"] = ordenCompraVO.partidas.Select(p => new
                {
                    ItemCode = p.ItemCode,
                    Quantity = (double)p.Quantity,
                    UnitPrice = (double)p.Precio,
                    ProjectCode = ordenCompraVO.CodigoProyecto,
                    CostingCode = ordenCompraVO.CentroCostos
                }).ToList();

                if (this.resultadoVO == null)
                    this.resultadoVO = new ResultadoVO();

                var response = client.Post(entidad, "PurchaseOrders", documento);
            
                var json = Newtonsoft.Json.Linq.JObject.Parse(response);

                var docEntry = json.SelectToken("DocEntry")?.ToString();
                var docNum = json.SelectToken("DocNum")?.ToString();

                this.resultadoVO.Exito = true;
                this.resultadoVO.DocEntry = docEntry;
                this.resultadoVO.DocNum = docNum;
                this.resultadoVO.Mensaje = "";
            }
            catch (Exception ex)
            {

                if (this.resultadoVO == null)
                    this.resultadoVO = new ResultadoVO();

                this.resultadoVO.Exito = false;
                this.resultadoVO.Mensaje = ex.Message;
                log.Error("Error al crear documento SAP", ex);


            }
        }

        private ResultadoVO ValidarRequest()
        {
            int entero;
            ResultadoVO resultadoVO = new ResultadoVO();

            try
            {
                if (EntidadVO.getEntidades().ContainsKey(ordenCompraVO.IDBaseDatos.ToString()) == false)
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "ID de Base de Datos Invalido";
                    
                    return resultadoVO;
                }

                EntidadVO.EntidadActual = EntidadVO.getEntidades()[ordenCompraVO.IDBaseDatos.ToString()];
                CatalogosDAO catalogosDAO = new CatalogosDAO(EntidadVO.EntidadActual.ConnectionString);


                if (ordenCompraVO == null)
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "Falta la informacion de Orden de Compra";
                    
                    return resultadoVO;
                }

                if (int.TryParse(ordenCompraVO.Serie, out entero) == false)
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "Serie de documento invalida";
                    
                    return resultadoVO;
                }

                if (catalogosDAO.IsSerieValidaOC(ordenCompraVO.Serie) == false)
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "Serie de documento no existe para Ordenes de Compra";
                    
                    return resultadoVO;
                }

                if (catalogosDAO.IsValidProveedor(ordenCompraVO.CardCode) == false)
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "Codigo de Proveedor Invalido";
                    return resultadoVO;
                }

                if (ordenCompraVO.partidas == null || ordenCompraVO.partidas.Count == 0)
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "Error, Orden de Compra SIN partidas";
                    
                    return resultadoVO;
                }

                for (int index = 1; index <= ordenCompraVO.partidas.Count; index++)
                {
                    OrdenCompraPartidaVO partidaVO = ordenCompraVO.partidas[index - 1];
                    if (partidaVO.ItemCode.Trim() == "")
                    {
                        resultadoVO.Exito = false;
                        resultadoVO.Mensaje = "La partida " + index + ", no tiene codigo de producto";
                        
                        return resultadoVO;
                    }

                    if (catalogosDAO.IsValidItemCode(partidaVO.ItemCode) == false)
                    {
                        resultadoVO.Exito = false;
                        resultadoVO.Mensaje = "El codigo de producto de la partida " + index + " es invalido";
                        return resultadoVO;
                    }
                }

                if (catalogosDAO.isExisteCAS(ordenCompraVO.FolioCAS))
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = $"El folio CAS {ordenCompraVO.FolioCAS} ya esta registrado en SAP.";
                    log.Error(resultadoVO.Mensaje);

                    return resultadoVO;
                }

                resultadoVO.Exito = true;
                resultadoVO.Mensaje = "";
                return resultadoVO;
            }
            catch (Exception ex)
            {
                log.Error("Error en ValidarRequest", ex);
                throw;
            }
        }

    } // OrdenCompraService
}