using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;


namespace HormesaFILEIDS.model
{
    internal class DAO

    {
        #region Private fields
        // Data access object.
        private string conName;
        private string conStr;
        private queryDump q = new queryDump();

        //Propiedades de conexión

        #endregion

        #region Conexión a DB

        //constructor de producción
        public DAO()
        {
            AuthenticationHandler authHdlr = new AuthenticationHandler();
            conStr = string.Format("Server={0};Initial Catalog={1};User Id={2};Password={3};Connect Timeout=5", authHdlr.DbServerIp, authHdlr.DbName, authHdlr.DbLogin, authHdlr.DbPassword);
            
            conName = authHdlr.DbServerIp;
        }

        //Probar la conexión a la DB
        public bool IsServerConnected()
        {
            using (SqlConnection tempConn = new SqlConnection(conStr))
            {
                try
                {
                    tempConn.Open();
                    return true;
                }
                catch (SqlException ex)
                {
                    
                    return false;
                }
            }
        }

        //Iniciar conexión.
        private SqlConnection instanceConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection(conStr);
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                //Esta excepcion ocurre si no es posible establecer una conexión. Es necesario manejarla silenciosamente.
            }
            return null;
        }

        #endregion

        #region Core queries

        //Ejecución sin retornos.


        // Consulta generica "select" que retorna un solo string.
        public string singleReturnQuery(string query)
        {
            try
            {
                //Crear nuevo objeto de conexion
                SqlConnection connection = instanceConnection();
                if (connection != null)
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    var dbResponse = command.ExecuteScalar();
                    if (dbResponse != null)
                    {
                        return dbResponse.ToString();
                    }
                }
                return string.Empty;
            }
            catch (SqlException ex)
            {
                sqlErrorLogger(conName, query, "DAO.singleReturnQuery", ex);
            }
            return string.Empty;
        }

        // Consulta generica "select" que retorna un datatable
        public DataTable tableReturnQuery(string query)
        {

            try
            {
                // Check for an available connection
                SqlConnection connection = instanceConnection();
                if (connection != null)
                {
                    //Dataset
                    DataSet ds = new DataSet();
                    //Data adapter
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                    //Llenar dataset con el resultado de dataAdapter
                    dataAdapter.Fill(ds, "genericSelectQuery");
                    DataTable tbl = ds.Tables["genericSelectQuery"];
                    ds = null;
                    return tbl;
                }
                return new DataTable();
            }
            catch (SqlException ex)
            {
                sqlErrorLogger(conName, query, "DAO.tableReturnQuery", ex);
            }
            return new DataTable();
        }

        // Consulta genérica que retorna una lista vertical.
        public ObservableCollection<string> verticalCollectionReturnQuery(string query)
        {
            //Crear nuevo objeto de conexion
            SqlConnection connection = instanceConnection();
            ObservableCollection<string> lista = new ObservableCollection<string>();
            if (connection != null)
            {
                DataTable tabla = tableReturnQuery(query);
                if (tabla != null)
                {
                    foreach (DataRow row in tabla.Rows)
                    {
                        lista.Add(row[0].ToString());
                    }
                }
            }
            return lista;

        }

        #endregion

        #region Error Loggers y getters
        /// <summary>
        /// Muestra mensajes de error SQl en pantalla.
        /// </summary>
        /// <param name="connection">Nombre de la conexión</param>
        /// <param name="query">Consulta</param>
        /// <param name="method">Método</param>
        /// <param name="exmsg">Mensaje de la excepcion</param>
        /// <param name="ex">Excepcion</param>
        public void sqlErrorLogger(string connection, string query, string method, SqlException ex)
        {
            MessageBox.Show(string.Format("Ha ocurrido un error de base de datos\n" +
                "Conexión: {0}\nConsulta: {1}\nCaller: {2}\nExcepción: {3}", connection, query, method, ex.Message));
            try
            {
                singleReturnQuery(q.logSQLErrores(ex));
            }
            catch (Exception)
            {
                //Este metodo deberia fallar silenciosamente porque significa que la conexión falló
            }

        }
        /// <summary>
        /// Logea en la base de datos cualquier excepcion.
        /// Esto puede mejorarse con algun delegado.
        /// </summary>
        /// <param name="ex"></param>
        public void exceptionLogger(Exception ex)
        {
            try
            {
                singleReturnQuery(q.logErrores(ex));
            }
            catch (Exception)
            {
                //Este metodo deberia fallar silenciosamente porque significa que la conexión falló
            }

        }

        /// <summary>
        /// Log de eventos de serilog
        /// </summary>
        /// <returns></returns>
        internal DataTable getEventLogs()
        {
            try
            {
                DataTable dt = tableReturnQuery(q.mostrarLogEventos());
                return dt;
            }
            catch (Exception)
            {
                //Este metodo deberia fallar silenciosamente porque significa que la conexión falló
            }

            return new DataTable();
        }

        /// <summary>
        /// Obtener listado de errores.
        /// </summary>
        /// <returns></returns>
        public DataTable getErrorLogs()
        {
            try
            {
                DataTable dt = tableReturnQuery(q.mostrarLogErrores());
                return dt;
            }
            catch (Exception)
            {
                //Este metodo deberia fallar silenciosamente porque significa que la conexión falló
            }

            return new DataTable();
        }

        #endregion
    }
}
