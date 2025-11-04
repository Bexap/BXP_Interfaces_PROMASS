using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.VO
{
    public class OrdenVentaPartidaVO
    {
        public int DocEntry = 0;
        public int LineNum = 0;
        public string ItemCode = "";
        public string ItemName = "";
        public string WTLiable = "";

        public decimal Precio = 0;
        public decimal Quantity = 0;
        public decimal SubTotal = 0;
        public decimal TotalLinea = 0;

        public string FolioCAS = "";
        public string FolioRecepcion = "";
        public string CodigoUnidadMedida = "";
        public decimal Descuento = 0;

        public string ClaveSAT = "";
        public string UnidadSAT = "";
        public string IdentificacionSAT = "";
        public string CuentaPredial = "";

        public ImpuestoVO TrasladoIVA = new ImpuestoVO();
        public ImpuestoVO TrasladoIEPS = new ImpuestoVO();
        public ImpuestoVO RetencionIVA = new ImpuestoVO();
        public ImpuestoVO RetencionISR = new ImpuestoVO();
        public ImpuestoVO RetencionIEPS = new ImpuestoVO();

        public override string ToString()
        {
            return $"DocEntry: {DocEntry}, LineNum: {LineNum}, ItemCode: {ItemCode}, ItemName: {ItemName}, Precio: {Precio}, Quantity: {Quantity}, SubTotal: {SubTotal}, TotalLinea: {TotalLinea}, FolioCas: {FolioCAS}, FolioRecepcion: {FolioRecepcion}, CodigoUnidadMedida: {CodigoUnidadMedida}, Descuento: {Descuento}, ClaveSat: {ClaveSAT}, UnidadSat: {UnidadSAT}, IdentificacionSat: {IdentificacionSAT}, CuentaPredial: {CuentaPredial}, TrasladoIva: {TrasladoIVA}, TrasladoIeps: {TrasladoIEPS}, RetencionIva: {RetencionIVA}, RetencionIsr: {RetencionISR}, RetencionIeps: {RetencionIEPS}";
        }
    }
}
