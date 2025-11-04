using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPConnector.VO
{
    public class MetricVO
    {
        public string uui;
        public string born;


        public override string ToString()
        {
            return $"Uui: {uui}, Born: {born}";
        }
    } // MetricVO
}
