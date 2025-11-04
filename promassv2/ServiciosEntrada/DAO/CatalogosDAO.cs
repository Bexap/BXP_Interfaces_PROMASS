using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SAPConnector.Services;
using System.Data;
using System.Data.Common;

namespace ServiciosEntrada.DAO
{
    public class CatalogosDAO : GenericDAO
    {
        public CatalogosDAO(string connectionString) : base(connectionString)
        {
            
        }

        public bool IsValidProveedor(string cardCode)
        {
            bool isValid = false;

            using (DbConnection conn = GetConnection())
            {
                string query = @"SELECT TOP 1 1 FROM OCRD o WHERE o.|CardCode| = ? AND o.|CardType| = 'S'";
                query = query.Replace('|', '"');

                DbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                DbParameter cardCodeParam = cmd.CreateParameter();
                cardCodeParam.ParameterName = "@cardCode";
                cardCodeParam.Value = cardCode;
                cmd.Parameters.Add(cardCodeParam);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isValid = reader.Read();
                }
            }

            return isValid;

        }

        public bool IsValidCliente(string cardCode)
        {
            bool isValid = false;

            using (DbConnection conn = GetConnection())
            {
                string query = @"SELECT TOP 1 1 FROM OCRD o WHERE o.|CardCode| = ? AND o.|CardType| = 'C'";
                query = query.Replace('|', '"');

                DbCommand cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                DbParameter cardCodeParam = cmd.CreateParameter();
                cardCodeParam.ParameterName = "@cardCode";
                cardCodeParam.Value = cardCode;
                cmd.Parameters.Add(cardCodeParam);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isValid = reader.Read();
                }
            }

            return isValid;

        }

        public bool IsValidItemCode(string itemCode)
        {
            bool isValid = false;

            string query = "select TOP 1 1 from oitm where |ItemCode| = ?";
            query = query.Replace('|', '"');

            using (DbConnection conn = GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                DbParameter pItemCode = cmd.CreateParameter();
                pItemCode.Value = itemCode;
                cmd.Parameters.Add(pItemCode);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isValid = reader.HasRows;
                }
            }

            return isValid;
        }

        public bool IsSerieValidaOC(string serie)
        {
            bool isValid = false;

            string query = "SELECT TOP 1 1 FROM NNM1 o WHERE \"ObjectCode\" = 22 AND \"Series\" = ?";

            using (DbConnection conn = GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                DbParameter pSerie = cmd.CreateParameter();
                pSerie.Value = serie;
                cmd.Parameters.Add(pSerie);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isValid = reader.HasRows;
                }
            }

            return isValid;
        }

        public bool IsSerieValidaOV(string serie)
        {
            bool isValid = false;

            string query = "SELECT TOP 1 1 FROM NNM1 o WHERE \"ObjectCode\" = 17 AND \"Series\" = ?";

            using (DbConnection conn = GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                DbParameter pSerie = cmd.CreateParameter();
                pSerie.Value = serie;
                cmd.Parameters.Add(pSerie);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isValid = reader.HasRows;
                }
            }

            return isValid;
        }

        public bool isExisteCAS(string folioCAS)
        {
            bool isExiste = false;

            string query = $"SELECT TOP 1 1 FROM OPOR WHERE \"U_BXP_CAS\" = '{folioCAS}' ";

            using (DbConnection conn = GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        isExiste = true;
                    }
                }
            }

            return isExiste;
        }

        public bool isExisteCASOrdenVenta(string folioCAS)
        {
            bool isExiste = false;

            string query = $"SELECT TOP 1 1 FROM ORDR WHERE \"U_BXP_CAS\" = '{folioCAS}' ";

            using (DbConnection conn = GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        isExiste = true;
                    }
                }
            }

            return isExiste;
        }

    } // CatalogosDAO
}