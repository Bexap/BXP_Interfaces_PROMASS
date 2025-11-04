using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.VO
{
    public class FacturaPartidaVO
    {
        public string DocEntry = "";

        public override string ToString()
        {
            return $"DocEntry: {DocEntry}";
        }
    } // FacturaPartidaVO
}
