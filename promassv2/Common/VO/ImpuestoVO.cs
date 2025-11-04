using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.VO
{
    public class ImpuestoVO
    {
        public decimal BaseCalculo = 0;
        public string ClaveSAT = "";
        public string TipoFactor = "";
        public decimal Tasa = 0;
        public decimal Importe = 0;

        public override string ToString()
        {
            return $"{nameof(BaseCalculo)}: {BaseCalculo}, {nameof(ClaveSAT)}: {ClaveSAT}, {nameof(TipoFactor)}: {TipoFactor}, {nameof(Tasa)}: {Tasa}, {nameof(Importe)}: {Importe}";
        }

    } // ImpuestoVO
}
