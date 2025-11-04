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
    public class OrdenCompraService : DocumentoService
    {
        private static ILog log = LogManager.GetLogger(typeof(OrdenCompraService));
        private OrdenCompraVO ordenCompraVO = null;
        public static ConcurrentDictionary<string, OrdenCompraVO> peticionesList = new ConcurrentDictionary<string, OrdenCompraVO>();

        protected override void CrearDocumentoSAP()
        {
            this.ordenCompraVO = (OrdenCompraVO)this.documentoVO;

            log.Info("Procesando Orden de Compra");
            log.Info(this.ordenCompraVO);

            ResultadoVO validacionVO = ValidarRequest();
            if (validacionVO.Exito == false)
            {
                log.Error("La orden de compra no es valida");
                this.resultadoVO = validacionVO;
                return;
            }

            Documents ordenCompra = (Documents)company.GetBusinessObject(BoObjectTypes.oPurchaseOrders);

            ordenCompra.CardCode = ordenCompraVO.CardCode;
            ordenCompra.DocDate = ordenCompraVO.DocDate;
            ordenCompra.DocCurrency = ordenCompraVO.CodigoMoneda;
            ordenCompra.Series = int.Parse(ordenCompraVO.Serie);
            ordenCompra.UserFields.Fields.Item("U_B1SYS_MainUsage").Value = "G03";

            asignarCampoString(ordenCompra.UserFields.Fields, "U_BXP_CAS", ordenCompraVO.FolioCAS);
            asignarCampoString(ordenCompra.UserFields.Fields, "U_BXP_Folio", ordenCompraVO.FolioRecepcion);

            if (ConfigurationManager.AppSettings["CamposInformativos"] == "SI")
            {
                #region Campos Informativos de Promass

                asignarCampoString(ordenCompra.UserFields.Fields, "U_CUENTA", ordenCompraVO.Cuenta);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_CLIENTE", ordenCompraVO.Cliente);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ID_PROVEEDOR", ordenCompraVO.IDProveedor);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_PROVEEDOR", ordenCompraVO.Proveedor);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ID_COMPANIA", ordenCompraVO.IDCompania);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_COMPANIA", ordenCompraVO.Compania);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ESTATUS_REPORTE", ordenCompraVO.EstatusReporte);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ESTATUS_SERVICIO", ordenCompraVO.EstatusServicio);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ID_SERVICIO", ordenCompraVO.IDServicio);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_SERVICIO", ordenCompraVO.Servicio);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ID_SUBSERVICIO", ordenCompraVO.IDSubServicio);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_SUBSERVICIO", ordenCompraVO.SubServicio);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_RETENCION", ordenCompraVO.Retencion);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_SUB_TOTAL", ordenCompraVO.SubTotal);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_IVA", ordenCompraVO.Iva);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_TOTAL", ordenCompraVO.Total);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_CARGO_CLIENTE", ordenCompraVO.CargoCliente);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_FORANEO_LOCAL", ordenCompraVO.ForaneoLocal);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_TIPO_ASIGNADOR", ordenCompraVO.TipoAsignador);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_COSTO_REAL", ordenCompraVO.CostoReal);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_FECHA_HORA_ALTA_CAS", ordenCompraVO.FechaHoraAltaCAS.ToString());
                asignarCampoString(ordenCompra.UserFields.Fields, "U_FECHA_HORA_ASIGNACION", ordenCompraVO.FechaHoraAsignacion.ToString());
                asignarCampoString(ordenCompra.UserFields.Fields, "U_FECHA_HORA_ARRIBO", ordenCompraVO.FechaHoraArribo.ToString());
                asignarCampoString(ordenCompra.UserFields.Fields, "U_FECHA_HORA_TERMINO", ordenCompraVO.FechaHoraTermino.ToString());
                asignarCampoString(ordenCompra.UserFields.Fields, "U_CIUDAD_ORIGEN", ordenCompraVO.CiudadOrigen);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ESTADO_ORIGEN", ordenCompraVO.EstadoOrigen);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_CIUDAD_DESTINO", ordenCompraVO.CiudadDestino);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_ESTADO_DESTINO", ordenCompraVO.EstadoDestino);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_LATITUD_ORIGEN", ordenCompraVO.LatitudOrigen);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_LONGITUD_ORIGEN", ordenCompraVO.LongitudOrigen);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_LATITUD_DESTINO", ordenCompraVO.LatitudDestino);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_LONGITUD_DESTINO", ordenCompraVO.LongitudDestino);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_LATITUD_PROVEEDOR", ordenCompraVO.LatitudProveedor);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_LONGITUD_PROVEEDOR", ordenCompraVO.LongitudProveedor);
                asignarCampoString(ordenCompra.UserFields.Fields, "U_EJECUTIVO", ordenCompraVO.Ejecutivo);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_KILOMETROS_CLIENTE", (double)ordenCompraVO.KilometrosCliente);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_KILOMETROS_PROV_CLIENTE", (double)ordenCompraVO.KilometrosProvCliente);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_ARRASTRE_SERVICIO", (double)ordenCompraVO.ArrastreServicio);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_BANDERAZO", (double)ordenCompraVO.Banderazo);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_COSTO_KM", (double)ordenCompraVO.CostoKM);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_MANIOBRAS", (double)ordenCompraVO.Maniobras);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_GASOLINA", (double)ordenCompraVO.Gasolina);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_CASETAS", (double)ordenCompraVO.Casetas);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_CORRESPONSALIA", (double)ordenCompraVO.Corresponsalia);
                asignarCampoDouble(ordenCompra.UserFields.Fields, "U_MATERIAL", (double)ordenCompraVO.Material);

                #endregion
            }

            for (int i = 0; i < ordenCompraVO.partidas.Count; i++)
            {
                OrdenCompraPartidaVO partida = ordenCompraVO.partidas[i];

                if (i > 0) ordenCompra.Lines.Add();

                ordenCompra.Lines.ItemCode = partida.ItemCode;
                ordenCompra.Lines.Quantity = (double)partida.Quantity;
                ordenCompra.Lines.UnitPrice = (double)partida.Precio;
                ordenCompra.Lines.ProjectCode = ordenCompraVO.CodigoProyecto;
                ordenCompra.Lines.CostingCode = ordenCompraVO.CentroCostos;
            }

            ObtenerResultado(ordenCompra.Add() == 0);
            peticionesList.TryRemove(ordenCompraVO.FolioCAS, out _);

            if (resultadoVO.Exito)
            {
                int docEntry = int.Parse(resultadoVO.DocEntry);
                if (ordenCompra.GetByKey(docEntry))
                {
                    resultadoVO.DocNum = ordenCompra.DocNum.ToString();
                }
            }

            log.Info("Resultado " + resultadoVO);
        }

        private ResultadoVO ValidarRequest()
        {
            int entero;
            ResultadoVO resultadoVO = new ResultadoVO();

            if (EntidadVO.getEntidades().ContainsKey(ordenCompraVO.IDBaseDatos.ToString()) == false)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "ID de Base de Datos Invalido";

                return resultadoVO;
            }

            EntidadVO.EntidadActual = EntidadVO.getEntidades()[ordenCompraVO.IDBaseDatos.ToString()];
            CatalogosDAO catalogosDAO = new CatalogosDAO(EntidadVO.EntidadActual.ConnectionString);
            Conectar(EntidadVO.EntidadActual.CompanyDB);            

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

            if (ordenCompraVO.partidas.Count == 0)
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

    } // OrdenCompraService
}