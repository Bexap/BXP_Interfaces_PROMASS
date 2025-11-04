using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiciosSalida.VO
{
    public class ConfiguracionVO
    {
        public static string UserNameBuzonE = ConfigurationManager.AppSettings["UserNameBuzonE"];
        public static string PasswordBuzonE = ConfigurationManager.AppSettings["PasswordBuzonE"];
        public static string RFCPromass = ConfigurationManager.AppSettings["RFCPromass"];
    }
}
