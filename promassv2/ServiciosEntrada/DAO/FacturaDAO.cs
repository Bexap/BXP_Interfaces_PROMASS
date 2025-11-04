using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using SAPConnector.Services;

namespace ServiciosEntrada.DAO
{
    public class FacturaDAO : GenericDAO
    {
        public bool IsValidProveedor(string cardCode)
        {
            bool isValid = false;

            using (DbConnection conn = GetConnection())
            {
                // TODO: Query HANA
                string query = @"SELECT TOP 1 1 FROM OCRD o WHERE o|CardCode| = @cardCode AND o.|CardType| = 'S'";
                query = query.Replace('|', '"');

                DbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                DbParameter cardCodeParam = cmd.CreateParameter();
                cardCodeParam.ParameterName = "@cardCode";
                cardCodeParam.Value = cardCode;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isValid = reader.Read();
                }
            }

            return isValid;
        }

    } // FacturaDAO
}