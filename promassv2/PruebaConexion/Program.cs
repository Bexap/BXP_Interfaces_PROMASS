using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace PruebaConexion
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var company = new Company();

            company.DbServerType = BoDataServerTypes.dst_HANADB;

            company.Server = "NDB@PROMASS-SAPHANA:30013";
            company.DbUserName = "SYSTEM";
            company.DbPassword = "Promass2017";
            company.CompanyDB = "CAS";
            company.UseTrusted = false;

            company.LicenseServer = "PROMASS-SAPHANA:40000";
            company.UserName = "manager";
            company.Password = "sapb1";

            company.language = BoSuppLangs.ln_English;

            if (company.Connect() != 0)
            {
                string messageError = company.GetLastErrorDescription();
                throw new Exception(messageError);
            }
        }
    }
}
