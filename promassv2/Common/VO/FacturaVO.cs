using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPConnector.VO;

namespace Common.VO
{
    public class FacturaVO : DocumentoVO
    {
        public string CodigoProveedor = "";
        public DateTime FechaContabilizacion = DateTime.Now;
        public string ArchivoPDF = "";
        public string ArchivoXML = "";
        public string CodigoAlmacen = "";
        public string UUID = "";
        public int DocEntry = 0;

        public List<FacturaPartidaVO> partidas = new List<FacturaPartidaVO>();

        public string Items
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("\n*** inicio partidas ***");
                foreach (var itemVO in partidas)
                {
                    sb.AppendLine(itemVO.ToString());
                }
                sb.AppendLine("*** fin de partidas ***");

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return $"CodigoProveedor: {CodigoProveedor}, FechaContabilizacion: {FechaContabilizacion}, CodigoAlmacen: {CodigoAlmacen}, Uuid: {UUID}, DocEntry: {DocEntry}, Partidas: {Items}";
        }

    } // FacturaVO
}
