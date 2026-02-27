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

            Documents ordenVenta = (Documents)company.GetBusinessObject(BoObjectTypes.oOrders);

            ordenVenta.CardCode = ordenVentaVO.CardCode;
            ordenVenta.DocDate = ordenVentaVO.DocDate;
            ordenVenta.DocDueDate = ordenVenta.DocDate;
            ordenVenta.DocCurrency = ordenVentaVO.CodigoMoneda;
            ordenVenta.Series = int.Parse(ordenVentaVO.Serie);
            ordenVenta.UserFields.Fields.Item("U_B1SYS_MainUsage").Value = "G03";

            asignarCampoString(ordenVenta.UserFields.Fields, "U_BXP_CAS", ordenVentaVO.FolioCAS);
            asignarCampoString(ordenVenta.UserFields.Fields, "U_BXP_Folio", ordenVentaVO.FolioRecepcion);

            if (ConfigurationManager.AppSettings["CamposInformativos"] == "SI")
            {
                #region Campos Informativos de Promass

                asignarCampoString(ordenVenta.UserFields.Fields, "U_CUENTA", ordenVentaVO.Cuenta);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_CLIENTE", ordenVentaVO.Cliente);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ID_PROVEEDOR", ordenVentaVO.IDProveedor);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_PROVEEDOR", ordenVentaVO.Proveedor);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ID_COMPANIA", ordenVentaVO.IDCompania);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_COMPANIA", ordenVentaVO.Compania);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ESTATUS_REPORTE", ordenVentaVO.EstatusReporte);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ESTATUS_SERVICIO", ordenVentaVO.EstatusServicio);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ID_SERVICIO", ordenVentaVO.IDServicio);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_SERVICIO", ordenVentaVO.Servicio);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ID_SUBSERVICIO", ordenVentaVO.IDSubServicio);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_SUBSERVICIO", ordenVentaVO.SubServicio);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_RETENCION", ordenVentaVO.Retencion);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_SUB_TOTAL", ordenVentaVO.SubTotal);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_IVA", ordenVentaVO.Iva);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_TOTAL", ordenVentaVO.Total);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_CARGO_CLIENTE", ordenVentaVO.CargoCliente);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_FORANEO_LOCAL", ordenVentaVO.ForaneoLocal);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_TIPO_ASIGNADOR", ordenVentaVO.TipoAsignador);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_COSTO_REAL", ordenVentaVO.CostoReal);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_FECHA_HORA_ALTA_CAS", ordenVentaVO.FechaHoraAltaCAS.ToString());
                asignarCampoString(ordenVenta.UserFields.Fields, "U_FECHA_HORA_ASIGNACION", ordenVentaVO.FechaHoraAsignacion.ToString());
                asignarCampoString(ordenVenta.UserFields.Fields, "U_FECHA_HORA_ARRIBO", ordenVentaVO.FechaHoraArribo.ToString());
                asignarCampoString(ordenVenta.UserFields.Fields, "U_FECHA_HORA_TERMINO", ordenVentaVO.FechaHoraTermino.ToString());
                asignarCampoString(ordenVenta.UserFields.Fields, "U_CIUDAD_ORIGEN", ordenVentaVO.CiudadOrigen);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ESTADO_ORIGEN", ordenVentaVO.EstadoOrigen);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_CIUDAD_DESTINO", ordenVentaVO.CiudadDestino);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_ESTADO_DESTINO", ordenVentaVO.EstadoDestino);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_LATITUD_ORIGEN", ordenVentaVO.LatitudOrigen);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_LONGITUD_ORIGEN", ordenVentaVO.LongitudOrigen);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_LATITUD_DESTINO", ordenVentaVO.LatitudDestino);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_LONGITUD_DESTINO", ordenVentaVO.LongitudDestino);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_LATITUD_PROVEEDOR", ordenVentaVO.LatitudProveedor);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_LONGITUD_PROVEEDOR", ordenVentaVO.LongitudProveedor);
                asignarCampoString(ordenVenta.UserFields.Fields, "U_EJECUTIVO", ordenVentaVO.Ejecutivo);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_KILOMETROS_CLIENTE", (double)ordenVentaVO.KilometrosCliente);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_KILOMETROS_PROV_CLIENTE", (double)ordenVentaVO.KilometrosProvCliente);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_ARRASTRE_SERVICIO", (double)ordenVentaVO.ArrastreServicio);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_BANDERAZO", (double)ordenVentaVO.Banderazo);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_COSTO_KM", (double)ordenVentaVO.CostoKM);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_MANIOBRAS", (double)ordenVentaVO.Maniobras);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_GASOLINA", (double)ordenVentaVO.Gasolina);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_CASETAS", (double)ordenVentaVO.Casetas);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_CORRESPONSALIA", (double)ordenVentaVO.Corresponsalia);
                asignarCampoDouble(ordenVenta.UserFields.Fields, "U_MATERIAL", (double)ordenVentaVO.Material);

                #endregion
            }

            for (int i = 0; i < ordenVentaVO.partidas.Count; i++)
            {
                var partida = ordenVentaVO.partidas[i];

                if (i > 0) ordenVenta.Lines.Add();

                ordenVenta.Lines.ItemCode = partida.ItemCode;
                ordenVenta.Lines.Quantity = (double)partida.Quantity;
                ordenVenta.Lines.UnitPrice = (double)partida.Precio;
                ordenVenta.Lines.ProjectCode = ordenVentaVO.CodigoProyecto;
                ordenVenta.Lines.CostingCode = ordenVentaVO.CentroCostos;
            }

            ObtenerResultado(ordenVenta.Add() == 0);
            peticionesList.TryRemove(ordenVentaVO.FolioCAS, out _);

            if (resultadoVO.Exito)
            {
                int docEntry = int.Parse(resultadoVO.DocEntry);
                if (ordenVenta.GetByKey(docEntry))
                {
                    resultadoVO.DocNum = ordenVenta.DocNum.ToString();
                }
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
            Conectar(EntidadVO.EntidadActual.CompanyDB, EntidadVO.EntidadActual.Id);

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