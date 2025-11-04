using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.VO;
using log4net;
using Sap.Data.Hana;
using SAPConnector.Services;
using SAPConnector.VO;
using ServiciosSalida.buzone.ordenescompra;
using ServiciosSalida.VO;

namespace ServiciosSalida.Services
{
    public class CancelarOrdenCompraService
    {
        private static ILog log = LogManager.GetLogger(typeof(CancelarOrdenCompraService));
        private GenericDAO genericDAO = null;
        private const string IVA8 = "IVAA8";

        public CancelarOrdenCompraService()
        {
            this.genericDAO = new GenericDAO(EntidadVO.EntidadActual.ConnectionString);
        }

        public bool isCancelaciones()
        {
            bool isNuevos = false;
            string query = @"
                select 
	                TOP 1 1
                from OPOR o
                left join bxp_sync_cancelaciones_ordenes bsp on o.|DocEntry| = bsp.doc_entry
                where
                    o.|UpdateDate| >= ?
                    and o.|CANCELED| = 'Y'
                    and bsp.doc_entry IS NULL
            ";
            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                DbParameter pFecha = cmd.CreateParameter();
                pFecha.ParameterName = "@fecha";
                pFecha.Value = DateTime.Today.AddDays(-3);

                cmd.Parameters.Add(pFecha);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isNuevos = reader.HasRows;
                    if (isNuevos) InsertarCancelaciones();
                }
            }

            return isNuevos;
        }

        public void InsertarCancelaciones()
        {
            string query = @"
                insert into bxp_sync_cancelaciones_ordenes (doc_entry, log_instance, doc_num, estatus, mensaje, last_update)
                select 
	                o.|DocEntry|,
                    0 as LogInstance,
                    o.|DocNum|,
                    0 as estatus,
                    '' as mensaje,
                    null as last_update
                from OPOR o
                left join bxp_sync_cancelaciones_ordenes bsp on o.|DocEntry| = bsp.doc_entry
                where
                    o.|UpdateDate| >= ?
                    and o.|CANCELED| = 'Y'
                    and bsp.doc_entry IS NULL
            ";

            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pFecha = cmd.CreateParameter();
                pFecha.ParameterName = "@fecha";
                pFecha.Value = DateTime.Today.AddDays(-3);

                cmd.Parameters.Add(pFecha);

                cmd.ExecuteNonQuery();
            }
        }

        public bool isPendientes()
        {
            bool isPendientes = false;
            string query = "select top 1 1 from bxp_sync_cancelaciones_ordenes where estatus = 0";

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isPendientes = reader.HasRows;
                }
            }

            return isPendientes;
        }

        public List<OrdenCompraVO> getOrdenesCompra()
        {
            string query = @"
                SELECT bso.doc_entry, bso.log_instance,
	                o.|CardCode|, o1.|VatIdUnCmp|, o1.|LicTradNum|, o.|U_BXP_CAS|, o.|U_BXP_Folio|,
	                o.|DocCur|, o.|DocTotal|, o.|CANCELED|, o.|DocStatus|, o.|DiscSum|, o.|DocRate|,
                    o.|VatSum|, o.|U_FORMAPAGO|, o.|U_MetodoPago|, o.|U_B1SYS_MainUsage|
                FROM bxp_sync_cancelaciones_ordenes bso 
                INNER JOIN OPOR o ON bso.doc_entry = o.|DocEntry|
                INNER JOIN OCRD o1 ON o.|CardCode| = o1.|CardCode|
                WHERE bso.estatus = 0 AND o.|CANCELED| = 'Y'
            ";

            query = query.Replace('|', '"');

            List<OrdenCompraVO> ordenesList = new List<OrdenCompraVO>();
            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OrdenCompraVO ordenVO = getEncabezado(reader);
                        ordenVO.partidas = getPartidas(ordenVO);

                        ordenesList.Add(ordenVO);
                    } // while
                }
            }

            return ordenesList;
        }

        private OrdenCompraVO getEncabezado(DbDataReader reader)
        {
            OrdenCompraVO ordenVO = new OrdenCompraVO();
            ordenVO.CardCode = reader["CardCode"].ToString();

            object taxID = reader["VatIdUnCmp"].ToString();
            object rfc = reader["LicTradNum"].ToString();

            if (rfc != DBNull.Value)
            {
                ordenVO.TaxID = rfc.ToString();
            }
            else
            {
                ordenVO.TaxID = taxID.ToString();
            }

            ordenVO.DocEntry = (int)reader["doc_entry"];
            ordenVO.FolioCAS = reader["U_BXP_CAS"].ToString();
            ordenVO.FolioRecepcion = reader["U_BXP_Folio"].ToString();
            ordenVO.CodigoMoneda = reader["DocCur"].ToString();
            ordenVO.Total = (double)((HanaDecimal)reader["DocTotal"]).ToDecimal();
            ordenVO.ImpuestoTraslado = ((HanaDecimal)reader["VatSum"]).ToDecimal();
            ordenVO.SubTotal = ordenVO.Total - (double)ordenVO.ImpuestoTraslado;
            ordenVO.LogInstance = (int)reader["log_instance"];
            ordenVO.UsoCFDI = reader["U_B1SYS_MainUsage"].ToString();

            string metodoPago = reader["U_MetodoPago"].ToString();
            string formaPago = reader["U_FORMAPAGO"].ToString();

            if (String.IsNullOrWhiteSpace(metodoPago) == false)
            {
                ordenVO.MetodoPago = metodoPago.ToString();
            }
            else
            {
                ordenVO.MetodoPago = "PPD";
            }
            if (String.IsNullOrWhiteSpace(formaPago) == false)
            {
                ordenVO.FormaPago = formaPago.ToString();
            }
            else
            {
                ordenVO.FormaPago = "99";
            }

            if (ordenVO.LogInstance == 0)
            {
                ordenVO.Accion = "A";
            }
            else
            {
                ordenVO.Accion = "U";
            }

            ordenVO.EstatusDocumento = "A";

            return ordenVO;
        }

        private List<OrdenCompraPartidaVO> getPartidas(OrdenCompraVO ordenCompraVO)
        {
            List<OrdenCompraPartidaVO> partidasList = new List<OrdenCompraPartidaVO>();

            string query = @"
		       SELECT 
	                p.|LineNum|, p.|ItemCode|, p.|Dscription|, p.|LineTotal|, 
	                p.|LineVat|, p.|GTotal|, p.|TaxCode|,
                    p.|Quantity|, p.|UomCode|, p.|Price|, o.|WTLiable|,
	                o1.|NcmCode|, o.|BuyUnitMsr|,
                    c1.|U_Supplier_rules|, o.|U_Item_rules|
                FROM POR1 p 
                    INNER JOIN OPOR o2 on o2.|DocEntry| = p.|DocEntry|
                    INNER JOIN OCRD c1 on c1.|CardCode| = o2.|CardCode|
                    INNER JOIN OITM o ON p.|ItemCode| = o.|ItemCode|
                    LEFT JOIN ONCM o1 ON o.|NCMCode| = o1.|AbsEntry|
                WHERE p.|DocEntry| = ?
            ";

            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pEntry = cmd.CreateParameter();
                pEntry.ParameterName = "@docEntry";
                pEntry.Value = ordenCompraVO.DocEntry;
                cmd.Parameters.Add(pEntry);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    decimal retencionIVA = 0;
                    decimal retencionISR = 0;

                    while (reader.Read())
                    {
                        OrdenCompraPartidaVO partidaVO = new OrdenCompraPartidaVO();

                        partidaVO.LineNum = (int)reader["LineNum"];
                        partidaVO.ItemCode = reader["ItemCode"].ToString();
                        partidaVO.ItemName = reader["Dscription"].ToString();
                        partidaVO.ClaveSAT = reader["NcmCode"].ToString();
                        partidaVO.UnidadSAT = reader["BuyUnitMsr"].ToString();
                        partidaVO.WTLiable = reader["WTLiable"].ToString();
                        partidaVO.SubTotal = ((HanaDecimal)reader["LineTotal"]).ToDecimal();

                        partidaVO.TrasladoIVA = new ImpuestoVO();
                        partidaVO.TrasladoIVA.ClaveSAT = "002";
                        partidaVO.TrasladoIVA.BaseCalculo = partidaVO.SubTotal;
                        partidaVO.TrasladoIVA.Tasa = reader["TaxCode"].ToString() == IVA8 ? 0.08M : 0.16M;
                        partidaVO.TrasladoIVA.TipoFactor = "Tasa";
                        partidaVO.TrasladoIVA.Importe = ((HanaDecimal)reader["LineVat"]).ToDecimal();
                        int supplierRules = (int)reader["U_Supplier_rules"];
                        int itemRules = (int)reader["U_Item_rules"];
                        bool itemTieneRetenciones = itemRules > 0;
                        bool supplierTieneRetenciones = supplierRules > 0;

                        // 20221229: Los articulos deben tener activo el campo nativo de SAP de sujeto a retencion
                        // y una retencion valida en el campo de usuario.
                        if (partidaVO.WTLiable == "Y" && supplierTieneRetenciones && itemTieneRetenciones)
                        {
                            (ImpuestoVO retencionIVAProveedor, ImpuestoVO retencionISRProveedor) = calcularRetenciones(supplierRules, itemRules, partidaVO);

                            partidaVO.RetencionIVA = retencionIVAProveedor;
                            partidaVO.RetencionISR = retencionISRProveedor;

                            retencionIVA += partidaVO.RetencionIVA.Importe;
                            retencionISR += partidaVO.RetencionISR.Importe;
                        }

                        partidaVO.TotalLinea = ((HanaDecimal)reader["GTotal"]).ToDecimal();
                        partidaVO.Quantity = ((HanaDecimal)reader["Quantity"]).ToDecimal();
                        partidaVO.CodigoUnidadMedida = reader["UomCode"].ToString();
                        partidaVO.Precio = ((HanaDecimal)reader["Price"]).ToDecimal();

                        partidasList.Add(partidaVO);
                    }

                    ordenCompraVO.ImpuestoRetenido = retencionIVA + retencionISR;
                }
            }

            return partidasList;
        }

        private (ImpuestoVO, ImpuestoVO) calcularRetenciones(int supplierRules, int itemRules, OrdenCompraPartidaVO partidaVO)
        {
            const string TIPO_IMPUESTO_ISR = "001";
            const string TIPO_IMPUESTO_IVA = "002";

            // Retenciones Proveedor     
            const int RETENCION_SUPPLIER_10_ISR_10_67_IVA = 1;
            const int RETENCION_SUPPLIER_4_IVA = 2;
            const int RETENCION_SUPPLIER_10_ISR = 3;
            const int RETENCION_SUPPLIER_1_25_ISR_10_67_IVA = 4;
            const int RETENCION_SUPPLIER_1_25_ISR = 5;

            // Retenciones Item
            const int RETENCION_ITEM_4_IVA = 1;
            const int RETENCION_ITEM_PROVEEDOR = 2;

            ImpuestoVO retencionISR = new ImpuestoVO();
            ImpuestoVO retencionIVA = new ImpuestoVO();

            // 2022129: No se pueden configurar 2 retenciones de IVA en SAP
            // Doris, decide procesar el articulo con 4 de IVA como clasificacion "retencion del proveedor"
            // cuando ya hay una retencion de IVA por parte del proveedor
            switch (supplierRules)
            {
                case RETENCION_SUPPLIER_10_ISR_10_67_IVA:
                    retencionISR.ClaveSAT = TIPO_IMPUESTO_ISR;
                    retencionISR.BaseCalculo = partidaVO.SubTotal;
                    retencionISR.TipoFactor = "Tasa";
                    retencionISR.Tasa = 0.1M;
                    retencionISR.Importe = partidaVO.SubTotal * 0.1M;

                    retencionIVA.ClaveSAT = TIPO_IMPUESTO_IVA;
                    retencionIVA.BaseCalculo = partidaVO.SubTotal;
                    retencionIVA.TipoFactor = "Tasa";
                    retencionIVA.Tasa = 0.1067M;
                    retencionIVA.Importe = partidaVO.SubTotal * 0.1067M;
                    break;
                case RETENCION_SUPPLIER_4_IVA:
                    retencionIVA.ClaveSAT = TIPO_IMPUESTO_IVA;
                    retencionIVA.BaseCalculo = partidaVO.SubTotal;
                    retencionIVA.TipoFactor = "Tasa";
                    retencionIVA.Tasa = 0.04M;
                    retencionIVA.Importe = partidaVO.SubTotal * 0.04M;
                    break;
                case RETENCION_SUPPLIER_10_ISR:
                    retencionISR.ClaveSAT = TIPO_IMPUESTO_ISR;
                    retencionISR.BaseCalculo = partidaVO.SubTotal;
                    retencionISR.TipoFactor = "Tasa";
                    retencionISR.Tasa = 0.1M;
                    retencionISR.Importe = partidaVO.SubTotal * 0.1M;

                    if (itemRules == RETENCION_ITEM_4_IVA)
                    {
                        retencionIVA.ClaveSAT = TIPO_IMPUESTO_IVA;
                        retencionIVA.BaseCalculo = partidaVO.SubTotal;
                        retencionIVA.TipoFactor = "Tasa";
                        retencionIVA.Tasa = 0.04M;
                        retencionIVA.Importe = partidaVO.SubTotal * 0.04M;
                    }

                    break;
                case RETENCION_SUPPLIER_1_25_ISR_10_67_IVA:
                    retencionISR.ClaveSAT = TIPO_IMPUESTO_ISR;
                    retencionISR.BaseCalculo = partidaVO.SubTotal;
                    retencionISR.TipoFactor = "Tasa";
                    retencionISR.Tasa = 0.0125M;
                    retencionISR.Importe = partidaVO.SubTotal * 0.0125M;

                    retencionIVA.ClaveSAT = TIPO_IMPUESTO_IVA;
                    retencionIVA.BaseCalculo = partidaVO.SubTotal;
                    retencionIVA.TipoFactor = "Tasa";
                    retencionIVA.Tasa = 0.1067M;
                    retencionIVA.Importe = partidaVO.SubTotal * 0.1067M;

                    break;
                case RETENCION_SUPPLIER_1_25_ISR:
                    retencionISR.ClaveSAT = TIPO_IMPUESTO_ISR;
                    retencionISR.BaseCalculo = partidaVO.SubTotal;
                    retencionISR.TipoFactor = "Tasa";
                    retencionISR.Tasa = 0.0125M;
                    retencionISR.Importe = partidaVO.SubTotal * 0.0125M;

                    if (itemRules == RETENCION_ITEM_4_IVA)
                    {
                        retencionIVA.ClaveSAT = TIPO_IMPUESTO_IVA;
                        retencionIVA.BaseCalculo = partidaVO.SubTotal;
                        retencionIVA.TipoFactor = "Tasa";
                        retencionIVA.Tasa = 0.04M;
                        retencionIVA.Importe = partidaVO.SubTotal * 0.04M;
                    }

                    break;
                default:
                    retencionISR = new ImpuestoVO();
                    retencionIVA = new ImpuestoVO();
                    break;
            }

            retencionIVA.Importe = Math.Round(retencionIVA.Importe, 2);
            retencionISR.Importe = Math.Round(retencionISR.Importe, 2);

            return (retencionIVA, retencionISR);
        }

        public void procesarOrdenesCompra(List<OrdenCompraVO> ordenesList)
        {
            foreach (OrdenCompraVO ordenCompraVO in ordenesList)
            {
                ResultadoVO resultadoVO = enviarOrdenCompraBuzonE(ordenCompraVO);
                actualizarEstatus(ordenCompraVO, resultadoVO);
            }
        }

        private decimal redondear(decimal valor)
        {
            return Math.Round(valor, 2);
        }

        private decimal redondear(double valor)
        {
            return (decimal)Math.Round(valor, 2);
        }

        private ResultadoVO enviarOrdenCompraBuzonE(OrdenCompraVO ordenCompraVO)
        {
            ResultadoVO resultadoVO = new ResultadoVO();

            OrdenCompraServiceClient ordenesBuzonEService = new OrdenCompraServiceClient();
            ordenCompra ordenCompraBE = new ordenCompra();

            string docEntry = EntidadVO.EntidadActual.Id.ToString() + ordenCompraVO.DocEntry.ToString();
            int docEntryCompuesto = int.Parse(docEntry);

            ordenCompraBE.user = ConfiguracionVO.UserNameBuzonE;
            ordenCompraBE.password = ConfiguracionVO.PasswordBuzonE;
            ordenCompraBE.codigoSN = ordenCompraVO.CardCode;
            ordenCompraBE.rfcProveedor = ordenCompraVO.TaxID;
            ordenCompraBE.rfcReceptor = EntidadVO.EntidadActual.RFC;
            ordenCompraBE.docEntry = docEntryCompuesto;
            ordenCompraBE.cas = ordenCompraVO.FolioCAS;
            ordenCompraBE.folioRecepcion = ordenCompraVO.FolioRecepcion;
            ordenCompraBE.codigoMoneda = ordenCompraVO.CodigoMoneda == "MXP" ? "MXN" : ordenCompraVO.CodigoMoneda;

            ordenCompraBE.subTotal = (decimal)ordenCompraVO.SubTotal;
            ordenCompraBE.impuestoRetenido = ordenCompraVO.ImpuestoRetenido;
            ordenCompraBE.impuestoTrasladado = ordenCompraVO.ImpuestoTraslado;
            ordenCompraBE.total = (decimal)ordenCompraVO.Total - ordenCompraBE.impuestoRetenido;
            ordenCompraBE.accion = "U";
            ordenCompraBE.activo = "N";
            ordenCompraBE.descuento = ordenCompraVO.Descuento;
            ordenCompraBE.tipoCambio = ordenCompraVO.TipoCambio;
            ordenCompraBE.formaPago = ordenCompraVO.FormaPago;
            ordenCompraBE.metodoPago = ordenCompraVO.MetodoPago;
            ordenCompraBE.tipoComprobante = "I"; // Promass confirma que este valor queda fijo
            ordenCompraBE.usoCFDI = "G03"; // TODO: Cesar comenta que este valor es fijo G03

            // partidas
            ordenCompraBE.lineas = new linea[ordenCompraVO.partidas.Count];
            for (int index = 0; index < ordenCompraBE.lineas.Length; index++)
            {
                OrdenCompraPartidaVO partidaVO = ordenCompraVO.partidas[index];
                linea lineaBE = new linea();
                lineaBE.linea1 = partidaVO.LineNum + 1;
                lineaBE.codigoArticulo = partidaVO.ItemCode;
                lineaBE.descripcion = partidaVO.ItemName;
                lineaBE.importe = Math.Round(partidaVO.SubTotal, 2);
                lineaBE.cantidadArticulos = partidaVO.Quantity;
                lineaBE.unidad = partidaVO.CodigoUnidadMedida;
                lineaBE.precio = partidaVO.Precio;
                lineaBE.descuento = partidaVO.Descuento;
                lineaBE.cveProducto = partidaVO.ClaveSAT;
                lineaBE.claveUnidad = partidaVO.UnidadSAT;
                lineaBE.noIdentificacion = partidaVO.ItemName;

                // Traslados
                iva trasladoIVA = new iva();
                trasladoIVA.@base = partidaVO.TrasladoIVA.BaseCalculo;
                trasladoIVA.impuesto = partidaVO.TrasladoIVA.ClaveSAT;
                trasladoIVA.tipoFactor = partidaVO.TrasladoIVA.TipoFactor;
                trasladoIVA.tasa = partidaVO.TrasladoIVA.Tasa;
                trasladoIVA.importe = partidaVO.TrasladoIVA.Importe;

                List<iva> trasladosIVAList = new List<iva>();
                trasladosIVAList.Add(trasladoIVA);
                lineaBE.impuestosTrasladados = new impuestosTrasladados();
                lineaBE.impuestosTrasladados.listaIVA = trasladosIVAList.ToArray();

                ieps trasladoIEPS = new ieps();
                trasladoIEPS.@base = partidaVO.TrasladoIEPS.BaseCalculo;
                trasladoIEPS.impuesto = partidaVO.TrasladoIEPS.ClaveSAT;
                trasladoIEPS.tipoFactor = partidaVO.TrasladoIEPS.TipoFactor;
                trasladoIEPS.tasa = partidaVO.TrasladoIEPS.Tasa;
                trasladoIEPS.importe = partidaVO.TrasladoIEPS.Importe;

                List<ieps> trasladosIEPSList = new List<ieps>();
                trasladosIEPSList.Add(trasladoIEPS);
                lineaBE.impuestosTrasladados.listaIEPS = trasladosIEPSList.ToArray();

                // Retenciones
                iva retenidoIVA = new iva();
                retenidoIVA.@base = partidaVO.RetencionIVA.BaseCalculo;
                retenidoIVA.impuesto = partidaVO.RetencionIVA.ClaveSAT;
                retenidoIVA.tipoFactor = partidaVO.RetencionIVA.TipoFactor;
                retenidoIVA.tasa = partidaVO.RetencionIVA.Tasa;
                retenidoIVA.importe = partidaVO.RetencionIVA.Importe;

                List<iva> retenidosIVAList = new List<iva>();
                retenidosIVAList.Add(retenidoIVA);
                lineaBE.impuestosRetenidos = new impuestosRetenidos();
                lineaBE.impuestosRetenidos.listaIVA = retenidosIVAList.ToArray();

                isr retenidoISR = new isr();
                retenidoISR.@base = partidaVO.RetencionISR.BaseCalculo;
                retenidoISR.impuesto = partidaVO.RetencionISR.ClaveSAT;
                retenidoISR.tipoFactor = partidaVO.RetencionISR.TipoFactor;
                retenidoISR.tasa = partidaVO.RetencionISR.Tasa;
                retenidoISR.importe = partidaVO.RetencionISR.Importe;

                List<isr> retenidosISRList = new List<isr>();
                retenidosISRList.Add(retenidoISR);
                lineaBE.impuestosRetenidos.listaISR = retenidosISRList.ToArray();


                ieps retenidoIEPS = new ieps();
                retenidoIEPS.@base = partidaVO.RetencionIEPS.BaseCalculo;
                retenidoIEPS.impuesto = partidaVO.RetencionIEPS.ClaveSAT;
                retenidoIEPS.tipoFactor = partidaVO.RetencionIEPS.TipoFactor;
                retenidoIEPS.tasa = partidaVO.RetencionIEPS.Tasa;
                retenidoIEPS.importe = partidaVO.RetencionIEPS.Importe;

                List<ieps> retenidosIEPSList = new List<ieps>();
                retenidosIEPSList.Add(retenidoIEPS);
                lineaBE.impuestosRetenidos.listaIEPS = retenidosIEPSList.ToArray();

                ordenCompraBE.lineas[index] = lineaBE;
            }

            try
            {
                log.Debug("Enviando orden de compra " + ordenCompraVO.ToString());

                System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;

                resultadoVO = new ResultadoVO();
                var resultado = ordenesBuzonEService.insertOrdenCompra(ordenCompraBE);

                log.Debug("Response [estatus: " + resultado.estatus + ", mensaje: " + resultado.mensajeError + "]");

                if (String.IsNullOrEmpty(resultado.codigoError))
                {
                    resultadoVO.Exito = true;
                    resultadoVO.Mensaje = "";
                }
                else
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = resultado.mensajeError;
                }
            }
            catch (Exception e)
            {
                resultadoVO = new ResultadoVO();
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = e.Message;
                log.Error(e.Message);
            }

            log.Debug("Resultado " + resultadoVO);

            return resultadoVO;
        }

        private void actualizarEstatus(OrdenCompraVO ordenCompraVO, ResultadoVO resultadoVO)
        {
            string query = @"
                UPDATE bxp_sync_cancelaciones_ordenes 
                SET estatus = ?, mensaje = ?, last_update = ?
                WHERE doc_entry = ? and log_instance = ?
            ";

            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pEstatus = cmd.CreateParameter();
                pEstatus.ParameterName = "@estatus";
                pEstatus.Value = resultadoVO.Exito ? 1 : 2;
                cmd.Parameters.Add(pEstatus);

                DbParameter pMensaje = cmd.CreateParameter();
                pMensaje.ParameterName = "@mensaje";
                pMensaje.Value =
                    resultadoVO.Mensaje.Length < 255
                    ? resultadoVO.Mensaje
                    : resultadoVO.Mensaje.Substring(0, 254);
                cmd.Parameters.Add(pMensaje);

                DbParameter pFecha = cmd.CreateParameter();
                pFecha.DbType = DbType.DateTime;
                pFecha.ParameterName = "@fecha";
                pFecha.Value = DateTime.Now;
                cmd.Parameters.Add(pFecha);

                DbParameter pEntry = cmd.CreateParameter();
                pEntry.ParameterName = "@docEntry";
                pEntry.Value = ordenCompraVO.DocEntry;
                cmd.Parameters.Add(pEntry);

                DbParameter pInstance = cmd.CreateParameter();
                pInstance.ParameterName = "@logInstance";
                pInstance.Value = ordenCompraVO.LogInstance;
                cmd.Parameters.Add(pInstance);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
