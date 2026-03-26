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

namespace ServiciosEntrada.Services
{
    public class OrdenVentaService : DocumentoService
    {
        private static ILog log = LogManager.GetLogger(typeof(OrdenVentaService));
        private OrdenVentaVO ordenVentaVO = null;
        public static ConcurrentDictionary<string, OrdenVentaVO> peticionesList = new ConcurrentDictionary<string, OrdenVentaVO>();

        protected override void CrearDocumentoSAP()
        {
                this.ordenVentaVO = (OrdenVentaVO)this.documentoVO;
           
            log.Info("Procesando Orden de Venta");
            log.Info(this.ordenVentaVO);

            ResultadoVO validacionVO = ValidarRequest();
            if (validacionVO.Exito == false)
            {
                log.Error("La orden de venta no es valida");
                this.resultadoVO = validacionVO;
                return;
            }

            try
            {
                var entidad = EntidadVO.EntidadActual;
                var client = new ServiciosSAP.ServiceLayerClient();

                var documento = new Dictionary<string, object>()
                {
                    { "CardCode", ordenVentaVO.CardCode },
                    { "DocDate", ordenVentaVO.DocDate.ToString("yyyy-MM-dd") },
                    { "DocDueDate", ordenVentaVO.DocDate.ToString("yyyy-MM-dd") },
                    { "DocCurrency", ordenVentaVO.CodigoMoneda },
                    { "Series", int.Parse(ordenVentaVO.Serie) },
                    { "U_B1SYS_MainUsage", "G03" },
                    { "U_BXP_CAS", ordenVentaVO.FolioCAS },
                    { "U_BXP_Folio", ordenVentaVO.FolioRecepcion }
                };

                if (ConfigurationManager.AppSettings["CamposInformativos"] == "SI")
                {
                    #region Campos Informativos de Promass

                    documento.Add("U_CUENTA", ordenVentaVO.Cuenta);
                    documento.Add("U_CLIENTE", ordenVentaVO.Cliente);
                    documento.Add("U_ID_PROVEEDOR", ordenVentaVO.IDProveedor);
                    documento.Add("U_PROVEEDOR", ordenVentaVO.Proveedor);
                    documento.Add("U_ID_COMPANIA", ordenVentaVO.IDCompania);
                    documento.Add("U_COMPANIA", ordenVentaVO.Compania);
                    documento.Add("U_ESTATUS_REPORTE", ordenVentaVO.EstatusReporte);
                    documento.Add("U_ESTATUS_SERVICIO", ordenVentaVO.EstatusServicio);
                    documento.Add("U_ID_SERVICIO", ordenVentaVO.IDServicio);
                    documento.Add("U_SERVICIO", ordenVentaVO.Servicio);
                    documento.Add("U_ID_SUBSERVICIO", ordenVentaVO.IDSubServicio);
                    documento.Add("U_SUBSERVICIO", ordenVentaVO.SubServicio);
                    documento.Add("U_RETENCION", ordenVentaVO.Retencion);
                    documento.Add("U_SUB_TOTAL", ordenVentaVO.SubTotal);
                    documento.Add("U_IVA", ordenVentaVO.Iva);
                    documento.Add("U_TOTAL", ordenVentaVO.Total);
                    documento.Add("U_CARGO_CLIENTE", ordenVentaVO.CargoCliente);
                    documento.Add("U_FORANEO_LOCAL", ordenVentaVO.ForaneoLocal);
                    documento.Add("U_TIPO_ASIGNADOR", ordenVentaVO.TipoAsignador);
                    documento.Add("U_COSTO_REAL", ordenVentaVO.CostoReal);
                    documento.Add("U_FECHA_HORA_ALTA_CAS", ordenVentaVO.FechaHoraAltaCAS.ToString());
                    documento.Add("U_FECHA_HORA_ASIGNACION", ordenVentaVO.FechaHoraAsignacion.ToString());
                    documento.Add("U_FECHA_HORA_ARRIBO", ordenVentaVO.FechaHoraArribo.ToString());
                    documento.Add("U_FECHA_HORA_TERMINO", ordenVentaVO.FechaHoraTermino.ToString());
                    documento.Add("U_CIUDAD_ORIGEN", ordenVentaVO.CiudadOrigen);
                    documento.Add("U_ESTADO_ORIGEN", ordenVentaVO.EstadoOrigen);
                    documento.Add("U_CIUDAD_DESTINO", ordenVentaVO.CiudadDestino);
                    documento.Add("U_ESTADO_DESTINO", ordenVentaVO.EstadoDestino);
                    documento.Add("U_LATITUD_ORIGEN", ordenVentaVO.LatitudOrigen);
                    documento.Add("U_LONGITUD_ORIGEN", ordenVentaVO.LongitudOrigen);
                    documento.Add("U_LATITUD_DESTINO", ordenVentaVO.LatitudDestino);
                    documento.Add("U_LONGITUD_DESTINO", ordenVentaVO.LongitudDestino);
                    documento.Add("U_LATITUD_PROVEEDOR", ordenVentaVO.LatitudProveedor);
                    documento.Add("U_LONGITUD_PROVEEDOR", ordenVentaVO.LongitudProveedor);
                    documento.Add("U_EJECUTIVO", ordenVentaVO.Ejecutivo);
                    documento.Add("U_KILOMETROS_CLIENTE", ordenVentaVO.KilometrosCliente);
                    documento.Add("U_KILOMETROS_PROV_CLIENTE", ordenVentaVO.KilometrosProvCliente);
                    documento.Add("U_ARRASTRE_SERVICIO", ordenVentaVO.ArrastreServicio);
                    documento.Add("U_BANDERAZO", ordenVentaVO.Banderazo);
                    documento.Add("U_COSTO_KM", ordenVentaVO.CostoKM);
                    documento.Add("U_MANIOBRAS", ordenVentaVO.Maniobras);
                    documento.Add("U_GASOLINA", ordenVentaVO.Gasolina);
                    documento.Add("U_CASETAS", ordenVentaVO.Casetas);
                    documento.Add("U_CORRESPONSALIA", ordenVentaVO.Corresponsalia);
                    documento.Add("U_MATERIAL", ordenVentaVO.Material);

                #endregion
                }

                    documento["DocumentLines"] = ordenVentaVO.partidas.Select(p => new
                {
                    ItemCode = p.ItemCode,
                    Quantity = (double)p.Quantity,
                    UnitPrice = (double)p.Precio,
                    ProjectCode = ordenVentaVO.CodigoProyecto,
                    CostingCode = ordenVentaVO.CentroCostos
                }).ToList();

                if (this.resultadoVO == null)
                    this.resultadoVO = new ResultadoVO();

                var response = client.Post(entidad, "Orders", documento);

                var json = Newtonsoft.Json.Linq.JObject.Parse(response);

                this.resultadoVO.Exito = true;
                this.resultadoVO.DocEntry = json["DocEntry"]?.ToString();
                this.resultadoVO.DocNum = json["DocNum"]?.ToString();
                this.resultadoVO.Mensaje = "";
            }
            catch (Exception ex)
            {
                if (this.resultadoVO == null)
                    this.resultadoVO = new ResultadoVO();

                this.resultadoVO.Exito = false;
                this.resultadoVO.Mensaje = ex.Message;
            }
            
            log.Info("Resultado " + resultadoVO);
        }

        private ResultadoVO ValidarRequest()
        {
            int entero;
            ResultadoVO resultadoVO = new ResultadoVO();

            if (EntidadVO.getEntidades().ContainsKey(ordenVentaVO.IDBaseDatos.ToString()) == false)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "ID de Base de Datos Invalido";

                return resultadoVO;
            }

            EntidadVO.EntidadActual = EntidadVO.getEntidades()[ordenVentaVO.IDBaseDatos.ToString()];
            CatalogosDAO catalogosDAO = new CatalogosDAO(EntidadVO.EntidadActual.ConnectionString);
            

            if (ordenVentaVO == null)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Falta la informacion de Orden de Venta";

                return resultadoVO;
            }

            if (int.TryParse(ordenVentaVO.Serie, out entero) == false)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Serie de documento invalida";

                return resultadoVO;
            }

            if (catalogosDAO.IsSerieValidaOV(ordenVentaVO.Serie) == false)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Serie de documento no existe para Ordenes de Venta";

                return resultadoVO;
            }

            if (catalogosDAO.IsValidCliente(ordenVentaVO.CardCode) == false)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Codigo de Cliente Invalido";
                return resultadoVO;
            }

            if (ordenVentaVO.partidas.Count == 0)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Error, Orden de Venta SIN partidas";

                return resultadoVO;
            }

            for (int index = 1; index <= ordenVentaVO.partidas.Count; index++)
            {
                var partidaVO = ordenVentaVO.partidas[index - 1];
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

            if (catalogosDAO.isExisteCASOrdenVenta(ordenVentaVO.FolioCAS))
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = $"El folio CAS {ordenVentaVO.FolioCAS} ya esta registrado en SAP.";
                log.Error(resultadoVO.Mensaje);

                return resultadoVO;
            }

            resultadoVO.Exito = true;
            resultadoVO.Mensaje = "";
            return resultadoVO;
        }
    }
}