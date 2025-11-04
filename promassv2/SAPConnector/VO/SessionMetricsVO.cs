using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPConnector.VO
{
    public class SessionMetricsVO
    {
        public List<MetricVO> SesionesDisponibles = new List<MetricVO>();
        public List<MetricVO> SesionesEnUso = new List<MetricVO>();

        public override string ToString()
        {
            return $"SesionesDisponibles: {SesionesDisponibles}, SesionesEnUso: {SesionesEnUso}";
        }

    } // SessionMetrics
}
