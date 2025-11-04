using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.VO;
using log4net;
using SAPConnector.Services;
using SAPConnector.VO;
using ServiciosSalida.buzone.proveedores;
using ServiciosSalida.VO;

namespace ServiciosSalida.Services
{
    class ProveedorService
    {
        private static ILog log = LogManager.GetLogger(typeof(ProveedorService));
        private GenericDAO genericDAO = null;

        public ProveedorService()
        {
            this.genericDAO = new GenericDAO(EntidadVO.EntidadActual.ConnectionString);
        }

        public bool isActualizaciones()
        {
            bool isNuevos = false;
            string query = @"
	            SELECT TOP 1 1 
	            FROM ACRD a
	            LEFT JOIN bxp_sync_proveedores bsp 
					ON a.|CardCode| = bsp.card_code 
					AND a.|LogInstanc| = bsp.log_instance
	            WHERE a.|CardCode| in (
					SELECT o.|CardCode| 
                    FROM OCRD o 
                    WHERE o.|UpdateDate| >= ?
                        AND o.|CardType| = 'S'
	            )
					AND bsp.log_instance IS NULL    
            ";
            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                DbParameter pFecha = cmd.CreateParameter();
                pFecha.Value = DateTime.Today.AddDays(-3);

                cmd.Parameters.Add(pFecha);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isNuevos = reader.HasRows;
                }
            }

            return isNuevos;
        }

        public void InsertarActualizaciones()
        {
            string query = @"
	            INSERT INTO bxp_sync_proveedores (doc_entry, card_code, log_instance, estatus, mensaje)
	            SELECT a.|DocEntry|, a.|CardCode|, a.|LogInstanc|, 0, ''
	            FROM ACRD a
	            LEFT JOIN bxp_sync_proveedores bsp 
					ON a.|CardCode| = bsp.card_code 
					AND a.|LogInstanc| = bsp.log_instance
	            WHERE a.|CardCode| in (
					SELECT o.|CardCode| 
                    FROM OCRD o 
                    WHERE o.|UpdateDate| >= ?
                        AND |CardType| = 'S'
	            )
					AND bsp.log_instance IS NULL
            ";

            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pFecha = cmd.CreateParameter();
                pFecha.Value = DateTime.Today.AddDays(-3);

                cmd.Parameters.Add(pFecha);

                cmd.ExecuteNonQuery();
            }
        }

        public bool isNuevos()
        {
            bool isNuevos = false;

            int lastEntry = getMaxDocEntry();

            isNuevos = isDocEntryMayores(lastEntry);

            if (isNuevos)
            {
                InsertarNuevos(lastEntry);
            }

            return isNuevos;
        }

        public bool isPendientes()
        {
            bool isPendientes = false;
            string query = "select top 1 1 from BXP_SYNC_PROVEEDORES where estatus = 0";

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isPendientes = reader.HasRows;
                }
            }

            return isPendientes;
        }

        public int getMaxDocEntry()
        {
            int lastEntry = 0;
            string query = "SELECT MAX(doc_entry) FROM bxp_sync_proveedores bsp";

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                object resultado = cmd.ExecuteScalar();

                if (resultado != DBNull.Value)
                {
                    lastEntry = (int)resultado;
                }
            }

            return lastEntry;
        }

        private bool isDocEntryMayores(int docEntry)
        {
            bool isNuevos = false;

            string query = @"
                SELECT TOP 1 1
                FROM OCRD o
                WHERE o.|DocEntry| > ?
                    AND o.|CardType| = 'S'
            ";

            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                DbParameter pEntry = cmd.CreateParameter();
                pEntry.Value = docEntry;

                cmd.Parameters.Add(pEntry);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    isNuevos = reader.HasRows;
                }
            }

            return isNuevos;
        }

        private void InsertarNuevos(int lastEntry)
        {
            string query = @"
                INSERT INTO bxp_sync_proveedores (doc_entry, card_code, log_instance, estatus, mensaje)
                SELECT DISTINCT o.|DocEntry|, o.|CardCode|, o.|LogInstanc|, 0, ''
                FROM OCRD o 
                WHERE o.|DocEntry| > ?
                    AND o.|CardType| = 'S'
            ";
            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pEntry = cmd.CreateParameter();
                pEntry.Value = lastEntry;

                cmd.Parameters.Add(pEntry);

                cmd.ExecuteNonQuery();
            }
        }

        public List<ProveedorVO> getProveedores()
        {
            string query = @"
                SELECT o.|CardCode|, o.|CardName|, o.|LicTradNum|, o.|VatIdUnCmp|,
		            o.|CmpPrivate|, o.|QryGroup1|, o.|validFor|, o.|CntctPrsn|, o.|E_Mail|,
                    bsp.log_instance
                FROM bxp_sync_proveedores bsp 
                INNER JOIN OCRD o ON bsp.card_code = o.|CardCode|
                WHERE bsp.estatus = 0
                    AND o.|CardType| = 'S'
            ";

            query = query.Replace('|', '"');

            List<ProveedorVO> proveedoresList = new List<ProveedorVO>();
            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProveedorVO proveedorVO = getInstancia(reader);
                        proveedorVO.DireccionFiscal = getDireccionFiscal(proveedorVO.CardCode);
                        proveedorVO.DireccionesEntrega = getDireccionesEntrega(proveedorVO.CardCode);

                        proveedoresList.Add(proveedorVO);
                    } // while
                }
            }

            return proveedoresList;
        }

        private ProveedorVO getInstancia(DbDataReader reader)
        {
            ProveedorVO proveedorVO = new ProveedorVO();

            proveedorVO.LogInstance = (int) reader["log_instance"];
            if (proveedorVO.LogInstance == 0)
            {
                proveedorVO.Accion = "A";
            }
            else
            {
                proveedorVO.Accion = "U";
            }

            proveedorVO.CardCode = reader["CardCode"].ToString();
            proveedorVO.CardName = reader["CardName"].ToString();

            object taxID = reader["VatIdUnCmp"].ToString();
            object rfc = reader["LicTradNum"].ToString();

            if (rfc != DBNull.Value)
            {
                proveedorVO.TaxID = rfc.ToString();
            }
            else
            {
                proveedorVO.TaxID = taxID.ToString();
            }

            proveedorVO.RegimenFiscal = reader["CmpPrivate"].ToString();

            string nacional = reader["QryGroup1"].ToString();
            proveedorVO.IsNacional = (nacional == "Y");

            string activo = reader["validFor"].ToString();

            proveedorVO.IsActivo = (activo == "Y");

            proveedorVO.NombreContacto = reader["CntctPrsn"].ToString();
            proveedorVO.EmailContacto = reader["E_Mail"].ToString();

            return proveedorVO;
        }

        private DireccionVO getDireccionFiscal(string cardCode)
        {
            DireccionVO direccionVO = new DireccionVO();

            string query = @"
                SELECT c.|Street|, c.|StreetNo|, c.|Building|, c.|City|,
	                c.|Block|, c.|County|, c.|State|, c.|Country|, 
	                c.|ZipCode|
                FROM CRD1 c 
                INNER JOIN OCRD o 
	                ON c.|CardCode| = o.|CardCode|
	                AND o.|BillToDef| = c.|Address|
                WHERE 
	                c.|CardCode| = ?
	                AND c.|AdresType| = 'B'
            ";

            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pCardCode = cmd.CreateParameter();
                pCardCode.Value = cardCode;

                cmd.Parameters.Add(pCardCode);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        direccionVO.Calle = reader["Street"].ToString();
                        direccionVO.NumeroExterior = reader["StreetNo"].ToString();
                        direccionVO.NumeroInterior = reader["Building"].ToString();
                        direccionVO.Ciudad = reader["City"].ToString();
                        direccionVO.Colonia = reader["Block"].ToString();
                        direccionVO.CodigoPostal = reader["ZipCode"].ToString();
                        direccionVO.Estado = reader["State"].ToString();
                        direccionVO.Pais = reader["Country"].ToString();
                    }
                }
            }

            return direccionVO;
        }

        private List<DireccionVO> getDireccionesEntrega(string cardCode)
        {
            List<DireccionVO> direccionesList = new List<DireccionVO>();

            string query = @"
                SELECT c.|Street|, c.|StreetNo|, c.|Building|, c.|Block|,
	                c.|ZipCode|, c.|City|, c.|State|, c.|Country|
                FROM CRD1 c
                WHERE c.|CardCode| = ?
	                AND c.|AdresType| = 'S'
            ";

            query = query.Replace('|', '"');

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pCardCode = cmd.CreateParameter();
                pCardCode.Value = cardCode;

                cmd.Parameters.Add(pCardCode);

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DireccionVO direccionVO = new DireccionVO();

                        direccionVO.Calle = reader["Street"].ToString();
                        direccionVO.NumeroExterior = reader["StreetNo"].ToString();
                        direccionVO.NumeroInterior = reader["Building"].ToString();
                        direccionVO.Colonia = reader["Block"].ToString();
                        direccionVO.CodigoPostal = reader["ZipCode"].ToString();
                        direccionVO.Ciudad = reader["State"].ToString();
                        direccionVO.Estado = reader["State"].ToString();
                        direccionVO.Pais = reader["Country"].ToString();

                        direccionesList.Add(direccionVO);
                    }
                }
            }

            return direccionesList;
        }

        public void procesarProveedores(List<ProveedorVO> proveedoresList)
        {
            foreach (ProveedorVO proveedorVO in proveedoresList)
            {
                ResultadoVO resultadoVO = enviarProveedorBuzonE(proveedorVO);
                actualizarEstatus(proveedorVO, resultadoVO);
            }
        }

        private ResultadoVO enviarProveedorBuzonE(ProveedorVO proveedorVO)
        {
            ResultadoVO resultadoVO = new ResultadoVO();

            proveedor proveedorBE = new proveedor();

            proveedorBE.user = ConfiguracionVO.UserNameBuzonE;
            proveedorBE.password = ConfiguracionVO.PasswordBuzonE;
            proveedorBE.codigoSN = proveedorVO.CardCode;
            proveedorBE.razonSocial = proveedorVO.CardName;
            proveedorBE.rfc = proveedorVO.TaxID;
            // TODO: no tenemos la clave SAT del regimen fiscal
            // proveedorBE.regimenFiscal = proveedorVO.RegimenFiscal;
            proveedorBE.regimenFiscal = 601; // HACK
            proveedorBE.lugarExpedicion = proveedorVO.DireccionFiscal.CodigoPostal;

            proveedorBE.nacional = (proveedorVO.IsNacional ? "Y" : "N");
            // proveedorBE.activo = proveedorVO.IsActivo.ToString();
            proveedorBE.activo = "Y";
            proveedorBE.accion = proveedorVO.Accion;
            proveedorBE.nombre = proveedorVO.NombreContacto;
            proveedorBE.email = proveedorVO.EmailContacto;

            direccion direccionBE = new direccion();
            direccionBE.calle = proveedorVO.DireccionFiscal.Calle;
            direccionBE.exterior = proveedorVO.DireccionFiscal.NumeroExterior;
            direccionBE.interior = proveedorVO.DireccionFiscal.NumeroInterior;
            direccionBE.colonia = proveedorVO.DireccionFiscal.Colonia;
            direccionBE.codigoPostal = proveedorVO.DireccionFiscal.CodigoPostal;
            direccionBE.municipio = proveedorVO.DireccionFiscal.Ciudad;
            direccionBE.estado = proveedorVO.DireccionFiscal.Estado;
            direccionBE.pais = proveedorVO.DireccionFiscal.Pais;
            proveedorBE.direcciones = new direccion[1];
            proveedorBE.direcciones[0] = direccionBE;

            try
            {
                log.Debug("Enviando proveedor " + proveedorVO.ToString());

                resultadoVO = new ResultadoVO();

                var proveedorService = new ProveedorServiceClient();
                var resultado = proveedorService.insertProveedor(proveedorBE);

                log.Debug("Response [estatus: " + resultado.estatus + ", mensaje: " + resultado.mensajeError + "]");

                if (String.IsNullOrEmpty(resultado.codigoError))
                {
                    resultadoVO.Exito = true;
                    resultadoVO.Mensaje = "";
                }
                else
                {
                    resultadoVO.Exito = false;
                    resultadoVO.Mensaje = resultado.mensajeError;
                }
            }
            catch (Exception e)
            {
                resultadoVO = new ResultadoVO();
                resultadoVO.Exito = false;
                resultadoVO.Mensaje = e.Message;
            }

            log.Debug("Resultado " + resultadoVO);

            return resultadoVO;
        }

        private void actualizarEstatus(ProveedorVO proveedorVO, ResultadoVO resultadoVO)
        {
            if (resultadoVO.Mensaje.Length > 254)
            {
                resultadoVO.Mensaje = resultadoVO.Mensaje.Substring(0, 253);
            }

            string query = @"
                UPDATE bxp_sync_proveedores
                SET estatus = ?, mensaje = ?, last_update = ?
                WHERE card_code = ? AND log_instance = ?
            ";

            using (DbConnection conn = genericDAO.GetConnection())
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                DbParameter pEstatus = cmd.CreateParameter();
                pEstatus.Value = resultadoVO.Exito ? 1 : 2;
                cmd.Parameters.Add(pEstatus);

                DbParameter pMensaje = cmd.CreateParameter();
                pMensaje.Value = resultadoVO.Mensaje;
                cmd.Parameters.Add(pMensaje);

                DbParameter pLastUpdate = cmd.CreateParameter();
                pLastUpdate.Value = DateTime.Now;
                pLastUpdate.DbType = DbType.DateTime;
                cmd.Parameters.Add(pLastUpdate);

                DbParameter pCardCode = cmd.CreateParameter();
                pCardCode.Value = proveedorVO.CardCode;
                cmd.Parameters.Add(pCardCode);

                DbParameter pLogInstance = cmd.CreateParameter();
                pLogInstance.Value = proveedorVO.LogInstance;
                cmd.Parameters.Add(pLogInstance);

                cmd.ExecuteNonQuery();
            }
        }

    } // ProveedorService
}

