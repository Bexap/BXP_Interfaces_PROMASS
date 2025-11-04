using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.VO
{
    public class ProveedorVO
    {
        public int LogInstance = 0;
        public string CardCode = "";
        public string CardName = "";
        public string TaxID = "";
        public string RegimenFiscal = "";
        public bool IsNacional = true;
        public bool IsActivo = true;
        public string Accion = "";
        public string NombreContacto = "";
        public string EmailContacto = "";
        public DireccionVO DireccionFiscal = new DireccionVO();

        public List<DireccionVO> DireccionesEntrega = new List<DireccionVO>();

        public override string ToString()
        {
            return $"CardCode: {CardCode}, CardName: {CardName}, TaxId: {TaxID}, RegimenFiscal: {RegimenFiscal}, IsNacional: {IsNacional}, IsActivo: {IsActivo}, Accion: {Accion}, NombreContacto: {NombreContacto}, EmailContacto: {EmailContacto}, DireccionFiscal: {DireccionFiscal}, DireccionesEntrega: {DireccionesEntrega}";
        }

    } // ProveedorVO
}
