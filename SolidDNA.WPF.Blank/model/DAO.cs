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
        private ErrorHandler err;

        //Propiedades de conexión

        #endregion

        #region Conexión a DB

        //constructor de producción
        public DAO(AuthenticationHandler authHdlr)
        {
            conStr = string.Format("Server={0};Initial Catalog={1};User Id={2};Password={3};Connect Timeout=5", authHdlr.DbServerIp, authHdlr.DbName, authHdlr.DbLogin, authHdlr.DbPassword);
            err = new ErrorHandler();
            conName = authHdlr.DbServerIp;
        }

        //Constructor para testing.
        public DAO()
        {
            conStr = "Server=.;Initial Catalog=HORMESAFILEIDS;Integrated Security=true";
            err = new ErrorHandler();
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
                catch (SqlException)
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
            catch (Exception ex)
            {
                err.sqlThrower(conName, query, "DAO.singleReturnQuery", ex.Message);
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
            catch (Exception ex)
            {
                err.sqlThrower(conName, query, "DAO.tableReturnQuery", ex.Message);
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


    }
}
