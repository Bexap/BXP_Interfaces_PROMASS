using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;
using SAPConnector.Services;

namespace ServiciosEntrada.DAO
{
    public class OrdenCompraDAO : GenericDAO
    {
        public OrdenCompraDAO(string connectionString) : base(connectionString)
        {

        }

        public bool isAlgunaNoFacturada(int docEntry)
        {
            bool isAlguna = false;
            string query = @"
                select TOP 1 1
                from POR1
                where ""DocEntry"" = ?
                    AND (""LineStatus"" != 'C' OR ""TargetType"" IS NULL OR ""TargetType"" != 18)
            ";

            using (DbConnection conn = GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                DbParameter pDocEntry = cmd.CreateParameter();
                pDocEntry.Value = docEntry;
                cmd.Parameters.Add(pDocEntry);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isAlguna = reader.HasRows;
                }
            }

            return isAlguna;
        }

        public (int docEntry, int docNum) getIDFactura(int docEntryOC)
        {
            int docEntry = 0;
            int docNum = 0;
            string query = @"
                select TOP 1 OPCH.""DocEntry"", OPCH.""DocNum""
                from POR1
                inner join OPCH on POR1.""TrgetEntry"" = OPCH.""DocEntry"" and POR1.""TargetType"" = 18
                where POR1.""DocEntry"" = " + docEntryOC;

            using (DbConnection conn = GetConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        docEntry = (int) reader["DocEntry"];
                        docNum = (int) reader["DocNum"];
                    }
                }
            }

            return (docEntry, docNum);
        }

    }
}