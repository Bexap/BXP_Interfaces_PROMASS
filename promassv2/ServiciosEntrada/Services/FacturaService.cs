using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Common.VO;
using log4net;
using SAPbobsCOM;
using SAPConnector.Services;
using SAPConnector.VO;
using ServiciosEntrada.DAO;
using System.IO;

namespace ServiciosEntrada.Services
{
    public class FacturaService : DocumentoService
    {
        private static ILog log = LogManager.GetLogger(typeof(FacturaService));
        private FacturaVO facturaVO = null;

        protected override void CrearDocumentoSAP()
        {
            string formaPago = "";
            string metodoPago = "";
            string usoPrincipal = "";
            string codigoClasificacion = "01010101";

            this.facturaVO = (FacturaVO)this.documentoVO;
            log.Info("Procesando Factura");
            log.Info(this.facturaVO);

            ResultadoVO validacionVO = ValidarRequest();
            if (validacionVO.Exito == false)
            {
                this.resultadoVO = validacionVO;
                return;
            }

            // Validar si la Orden esta cerrada
           validarCerrada(); 

            Documents factura = (Documents)company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            Documents ordenCompra = (Documents)company.GetBusinessObject(BoObjectTypes.oPurchaseOrders);

            factura.CardCode = facturaVO.CodigoProveedor;
            factura.DocDate = facturaVO.FechaContabilizacion;

            guardarArchivos(facturaVO);

            if (resultadoVO.Exito == false)
            {
                log.Error(resultadoVO.Mensaje);
                return;
            }

            factura.AttachmentEntry = int.Parse(resultadoVO.DocEntry);
            int totalPartidas = 0;
            for (int i = 0; i < facturaVO.partidas.Count; i++)
            {
                FacturaPartidaVO partida = facturaVO.partidas[i];
                
                log.Debug("Buscando orden de compra " + partida.DocEntry);
                string docEntryStr = partida.DocEntry.ToString();
                int docEntryAislado = int.Parse(docEntryStr.Substring(2, docEntryStr.Length - 2));
                if (ordenCompra.GetByKey(docEntryAislado))
                {
                    if (String.IsNullOrEmpty(formaPago))
                    {
                        formaPago = ordenCompra.UserFields.Fields.Item("U_FORMAPAGO").Value;
                        metodoPago = ordenCompra.UserFields.Fields.Item("U_MetodoPago").Value;
                        usoPrincipal = ordenCompra.UserFields.Fields.Item("U_B1SYS_MainUsage").Value;
                    }

                    for (int index = 0; index < ordenCompra.Lines.Count; index++)
                    {
                        totalPartidas++;
                        if (totalPartidas > 1) factura.Lines.Add();

                        ordenCompra.Lines.SetCurrentLine(index);

                        factura.Lines.BaseEntry = ordenCompra.DocEntry;
                        factura.Lines.BaseLine = ordenCompra.Lines.LineNum;
                        factura.Lines.BaseType = (int)BoAPARDocumentTypes.bodt_PurchaseOrder;
                        factura.Lines.ItemCode = ordenCompra.Lines.ItemCode;
                        factura.Lines.Quantity = ordenCompra.Lines.Quantity;
                        factura.Lines.UnitPrice = ordenCompra.Lines.UnitPrice;
                        factura.Lines.TaxCode = ordenCompra.Lines.TaxCode;

                        log.Debug(company.CompanyDB);
                        log.Debug(company.CompanyName);
                        log.Debug("****************************************");
                        for (int indice = 0; indice < factura.Lines.UserFields.Fields.Count; indice++)
                        {
                            log.Debug(factura.Lines.UserFields.Fields.Item(indice).Name);
                        }

                        factura.Lines.UserFields.Fields.Item("U_ClasifItm").Value = codigoClasificacion;
                    }
                }
                else
                {
                    resultadoVO = new ResultadoVO();
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "No existe la Orden de Compra con DocEntry " + partida.DocEntry;

                    log.Error(resultadoVO.Mensaje);

                    return;
                }
            }

            factura.UserFields.Fields.Item("U_MetodoPago").Value = metodoPago;
            factura.UserFields.Fields.Item("U_FORMAPAGO").Value = formaPago;
            factura.UserFields.Fields.Item("U_B1SYS_MainUsage").Value = usoPrincipal;            

            ObtenerResultado(factura.Add() == 0);

            if (resultadoVO.Exito)
            {
                int docEntry = int.Parse(resultadoVO.DocEntry);
                if (factura.GetByKey(docEntry))
                {
                    resultadoVO.DocNum = factura.DocNum.ToString();
                }

                // Cerrar las ordenes de Compra
                for (int i = 0; i < facturaVO.partidas.Count; i++)
                {
                    FacturaPartidaVO partida = facturaVO.partidas[i];
                    if (ordenCompra.GetByKey(int.Parse(partida.DocEntry)))
                    {
                        ordenCompra.Close();
                    }
                }
            }
            else
            {
                if (resultadoVO.Mensaje.StartsWith("Se ha cerrado uno de los documentos base"))
                {
                    bool isTodasFacturadas = true;
                    var ordenCompraDAO = new OrdenCompraDAO(EntidadVO.EntidadActual.ConnectionString);
                    foreach (var ordenVO in facturaVO.partidas)
                    {
                        int docEntryAislado = int.Parse(ordenVO.DocEntry.Substring(2, ordenVO.DocEntry.Length - 2));
                        if (ordenCompraDAO.isAlgunaNoFacturada(docEntryAislado))
                        {
                            log.Info("Al menos una partida esta abierta o bien cerrada por un documento que no es factura: " + docEntryAislado);
                            isTodasFacturadas = false;
                            break;
                        }
                    }

                    if (isTodasFacturadas)
                    {
                        log.Info("todas las partidas estan facturadas");
                        var ordenVO = facturaVO.partidas[0];
                        int docEntryAislado = int.Parse(ordenVO.DocEntry.Substring(2, ordenVO.DocEntry.Length - 2));
                        (int docEntry, int docNum) = ordenCompraDAO.getIDFactura(docEntryAislado);
                        this.resultadoVO.Exito = true;
                        this.resultadoVO.Mensaje = "";
                        this.resultadoVO.DocEntry = docEntry.ToString();
                        this.resultadoVO.DocNum = docNum.ToString();
                    }
                }
            }

            log.Info("Resultado " + this.resultadoVO);
        }

        private ResultadoVO ValidarRequest()
        {            
            ResultadoVO resultadoVO = new ResultadoVO();

            if (facturaVO == null)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Falta la informacion de la Factura";
                return resultadoVO;
            }

            if (String.IsNullOrEmpty(facturaVO.CodigoProveedor))
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Falta el Codigo de Proveedor";
                return resultadoVO;
            }

            if (String.IsNullOrEmpty(facturaVO.ArchivoPDF))
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Falta el archivo PDF";
                return resultadoVO;
            }

            if (String.IsNullOrEmpty(facturaVO.ArchivoXML))
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Falta el archivo XML";
                return resultadoVO;
            }

            if (String.IsNullOrEmpty(facturaVO.UUID))
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Falta el valor UUID del documento";
                return resultadoVO;
            }

            if (facturaVO.partidas == null || facturaVO.partidas.Count == 0)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "La factura no tiene partidas";
                return resultadoVO;
            }

            string IDBaseDatosAnterior = "";
            string IDBaseDatos = "";
            for (int index = 0; index < facturaVO.partidas.Count; index++)
            {
                FacturaPartidaVO partidaVO = facturaVO.partidas[index];
                int descartado = 0;
                if (int.TryParse(partidaVO.DocEntry, out descartado) == false)
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = "El valor DocEntry de la partida " + (index + 1) +
                                          " es invalido. Debe ser un entero";
                    return resultadoVO;
                } else
                {
                    string docEntryStr = descartado.ToString();

                    if (docEntryStr.Length < 3)
                    {
                        resultadoVO.Exito = false;
                        resultadoVO.Mensaje = "El valor DocEntry de la partida " + (index + 1) +
                                              " es invalido. Debe ser minimo de 3 digitos";
                        return resultadoVO;
                    }

                    IDBaseDatos = docEntryStr.Substring(0, 2);
                    if (EntidadVO.getEntidades().ContainsKey(IDBaseDatos) == false)
                    {
                        resultadoVO.Exito = false;
                        resultadoVO.Mensaje = "Prefijo Base de Datos Invalido para la partida " + (index + 1);

                        return resultadoVO;
                    }

                    if (IDBaseDatosAnterior == "") IDBaseDatosAnterior = IDBaseDatos;

                    if (IDBaseDatosAnterior != IDBaseDatos)
                    {
                        resultadoVO.Exito = false;
                        resultadoVO.Mensaje = "Error. Las ordenes de compra corresponden a diferentes bases de datos";

                        return resultadoVO;
                    }

                    EntidadVO.EntidadActual = EntidadVO.getEntidades()[IDBaseDatos];
                }
            }

            CatalogosDAO catalogosDAO = new CatalogosDAO(EntidadVO.EntidadActual.ConnectionString);
            Conectar(EntidadVO.EntidadActual.CompanyDB);

            if (catalogosDAO.IsValidProveedor(facturaVO.CodigoProveedor) == false)
            {
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = "Codigo de Proveedor NO EXISTE";
                return resultadoVO;
            }

            resultadoVO.Exito = true;
            resultadoVO.Mensaje = "";
            return resultadoVO;
        }

        private void guardarArchivos(FacturaVO facturaVO)
        {
            byte[] archivoPdf = Convert.FromBase64String(facturaVO.ArchivoPDF);
            byte[] archivoXml = Convert.FromBase64String(facturaVO.ArchivoXML);
            File.WriteAllBytes("c:/portal/archivos/factura-" + facturaVO.UUID + ".pdf", archivoPdf);
            File.WriteAllBytes("c:/portal/archivos/factura-" + facturaVO.UUID + ".xml", archivoXml);
            Attachments2 attachments = (Attachments2)company.GetBusinessObject(BoObjectTypes.oAttachments2);
            attachments.Lines.Add();
            attachments.Lines.FileName = "Factura-" + facturaVO.UUID;
            attachments.Lines.FileExtension = "pdf";
            attachments.Lines.SourcePath = "c:/portal/archivos";
            attachments.Lines.Override = BoYesNoEnum.tYES;
            attachments.Lines.Add();
            attachments.Lines.FileName = "Factura-" + facturaVO.UUID;
            attachments.Lines.FileExtension = "xml";
            attachments.Lines.SourcePath = "c:/portal/archivos";
            attachments.Lines.Override = BoYesNoEnum.tYES;

            ObtenerResultado(attachments.Add() == 0);
        }

        private void validarCerrada()
        {

        }

    } // FacturaService
}