using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace SAPConnector.Services
{
    public class GenericDAO
    {
        private static ILog log = LogManager.GetLogger(typeof(GenericDAO));
        // protected static string connectionString = ConfigurationManager.ConnectionStrings["BD_SAP"].ConnectionString;
        private string connectionString = "";

        private GenericDAO()
        {

        }

        public GenericDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbConnection GetConnection()
        {           
            Sap.Data.Hana.HanaConnection conn = new Sap.Data.Hana.HanaConnection(this.connectionString);
            conn.Open();

            return conn;
        }

    } // GenericDAO
}
