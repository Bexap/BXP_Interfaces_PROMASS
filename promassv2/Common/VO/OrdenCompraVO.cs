using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPConnector.VO;

namespace Common.VO
{
    public class OrdenCompraVO : DocumentoVO
    {
        public int IDBaseDatos = 0;
        public string CardCode = "";
        public DateTime DocDate = DateTime.MinValue;
        public string CodigoMoneda = "";
        public string Serie = "";
        public string TaxID = "";
        public int DocEntry = 0;
        public int LogInstance = 0;

        public string FolioCAS = "";
        public string FolioRecepcion = "";
        public string CodigoProyecto = "";
        public string CentroCostos = "";
        public string Accion = "";
        public string EstatusDocumento = "";

        public decimal ImpuestoRetenido = 0;
        public decimal ImpuestoTraslado = 0;
        public decimal Descuento = 0;
        public decimal TipoCambio = 0;
        public string FormaPago = "";
        public string MetodoPago = "";
        public string TipoComprobante = "";
        public string UsoCFDI = "";

        public String Cuenta = "";
        public String Cliente = "";
        public String IDProveedor = "";
        public String Proveedor = "";
        public string IDCompania = "";
        public String Compania = "";
        public String EstatusReporte = "";
        public String EstatusServicio = "";
        public String IDServicio = "";
        public String Servicio = "";
        public String IDSubServicio = "";
        public String SubServicio = "";
        public Double Retencion = 0.0;
        public Double SubTotal = 0.0;
        public Double Iva = 0.0;
        public Double Total = 0.0;
        public String CargoCliente = "";
        public String ForaneoLocal = "";
        public String TipoAsignador = "";
        public Double CostoReal = 0.0;
        public DateTime FechaHoraAltaCAS = DateTime.MinValue;
        public DateTime FechaHoraAsignacion = DateTime.MinValue;
        public DateTime FechaHoraArribo = DateTime.MinValue;
        public DateTime FechaHoraTermino = DateTime.MinValue;
        public String CiudadOrigen = "";
        public String EstadoOrigen = "";
        public String CiudadDestino = "";
        public String EstadoDestino = "";
        public String LatitudOrigen = "";
        public String LongitudOrigen = "";
        public String LatitudDestino = "";
        public String LongitudDestino = "";
        public String LatitudProveedor = "";
        public String LongitudProveedor = "";
        public String Ejecutivo = "";
        public Decimal KilometrosCliente = 0.0M;
        public Decimal KilometrosProvCliente = 0.0M;
        public Decimal ArrastreServicio = 0.0M;
        public Double Banderazo = 0.0;
        public Double CostoKM = 0.0;
        public Double Maniobras = 0.0;
        public Double Gasolina = 0.0;
        public Double Casetas = 0.0;
        public Double Corresponsalia = 0.0;
        public Double Material = 0.0;

        public List<OrdenCompraPartidaVO> partidas = new List<OrdenCompraPartidaVO>();

        public override string ToString()
        {
            return $"CardCode: {CardCode}, DocDate: {DocDate}, CodigoMoneda: {CodigoMoneda}, Serie: {Serie}, TaxId: {TaxID}, DocEntry: {DocEntry}, FolioCas: {FolioCAS}, FolioRecepcion: {FolioRecepcion}, CodigoProyecto: {CodigoProyecto}, CentroCostos: {CentroCostos}, Accion: {Accion}, EstatusDocumento: {EstatusDocumento}, ImpuestoRetenido: {ImpuestoRetenido}, ImpuestoTraslado: {ImpuestoTraslado}, Descuento: {Descuento}, TipoCambio: {TipoCambio}, FormaPago: {FormaPago}, MetodoPago: {MetodoPago}, TipoComprobante: {TipoComprobante}, UsoCfdi: {UsoCFDI}, Cuenta: {Cuenta}, Cliente: {Cliente}, IdProveedor: {IDProveedor}, Proveedor: {Proveedor}, IdCompania: {IDCompania}, Compania: {Compania}, EstatusReporte: {EstatusReporte}, EstatusServicio: {EstatusServicio}, IdServicio: {IDServicio}, Servicio: {Servicio}, IdSubServicio: {IDSubServicio}, SubServicio: {SubServicio}, Retencion: {Retencion}, SubTotal: {SubTotal}, Iva: {Iva}, Total: {Total}, CargoCliente: {CargoCliente}, ForaneoLocal: {ForaneoLocal}, TipoAsignador: {TipoAsignador}, CostoReal: {CostoReal}, FechaHoraAltaCas: {FechaHoraAltaCAS}, FechaHoraAsignacion: {FechaHoraAsignacion}, FechaHoraArribo: {FechaHoraArribo}, FechaHoraTermino: {FechaHoraTermino}, CiudadOrigen: {CiudadOrigen}, EstadoOrigen: {EstadoOrigen}, CiudadDestino: {CiudadDestino}, EstadoDestino: {EstadoDestino}, LatitudOrigen: {LatitudOrigen}, LongitudOrigen: {LongitudOrigen}, LatitudDestino: {LatitudDestino}, LongitudDestino: {LongitudDestino}, LatitudProveedor: {LatitudProveedor}, LongitudProveedor: {LongitudProveedor}, Ejecutivo: {Ejecutivo}, KilometrosCliente: {KilometrosCliente}, KilometrosProvCliente: {KilometrosProvCliente}, ArrastreServicio: {ArrastreServicio}, Banderazo: {Banderazo}, CostoKm: {CostoKM}, Maniobras: {Maniobras}, Gasolina: {Gasolina}, Casetas: {Casetas}, Corresponsalia: {Corresponsalia}, Material: {Material}, Partidas: {partidas}";
        }

    } // OrdenCompraVO
}
