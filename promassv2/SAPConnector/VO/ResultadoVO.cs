using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPConnector.VO
{
    public class ResultadoVO
    {
        public bool Exito = false;
        public string Mensaje = "";
        public string DocEntry = "";
        public string DocNum = "";

        public override string ToString()
        {
            return $"Exito: {Exito}, Mensaje: {Mensaje}, DocEntry: {DocEntry}, DocNum: {DocNum}";
        }

    } // ResultadoVO
}
