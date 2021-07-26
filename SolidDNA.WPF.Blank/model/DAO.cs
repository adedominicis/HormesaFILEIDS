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

        private SqlConnection connection;
        private string conName;
        private string conStr;
        private queryDump q = new queryDump();
        public event EventHandler<string> exceptionRaised;
        private ErrorHandler err;

        //Propiedades de conexión

        #endregion

        #region Conexión a DB
        // Database index:  1:localtest,  2: azureproductionmirs

        private int dbIndex = 1;

        //constructor de producción
        public DAO(string serverName)
        {
            AuthenticationHandler authHdlr = new AuthenticationHandler(serverName);
            conStr = string.Format("Server={0};Initial Catalog={1};User Id={2};Password={3};", authHdlr.DbServerName, authHdlr.DbName, authHdlr.DbUser, authHdlr.DbPassword);
            err = new ErrorHandler();
        }

        //Constructor para testing.
        public DAO(int dbindex = 1)
        {
            dbIndex = dbindex;
            err = new ErrorHandler();
        }

        public void startConnection()
        {
            try
            {
                switch (dbIndex)
                {
                    case 1:
                        conName = "localtest";
                        conStr = "Server=.;Initial Catalog=HORMESAFILEIDS;Integrated Security=true";
                        break;
                    case 2:
                        conName = "hormesaproduccion";
                        conStr = "Data Source=192.168.2.48.;Initial Catalog=HORMESAFILEIDS;Integrated Security=true";
                        break;
                    default:
                        conName = "localtest";
                        conStr = "Server=.;Initial Catalog=HORMESAFILEIDS;Integrated Security=true";
                        break;
                }

                connection = new SqlConnection(conStr);
                connection.Open();
            }
            catch (Exception ex)
            {
                err.sqlThrower(conName, conStr, "DAO.startConnection()", ex.Message);
                //exceptionRaised?.Invoke(this, err.handler(EnumMensajes.errorEnConexionDB) + conName + "\\n" + e.Message);
            }
        }


        // Get connection name
        public string getConnectionName()
        {
            return conName;
        }
        #endregion

        #region Core queries

        //Ejecución sin retornos.


        // Consulta generica "select" que retorna un solo string.
        public string singleReturnQuery(string query)
        {

            try
            {
                // Check for an available connection
                startConnection();

                SqlCommand command = new SqlCommand(query, connection);
                var dbResponse = command.ExecuteScalar();
                if (dbResponse != null)
                {
                    return dbResponse.ToString();
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                err.sqlThrower(conName, query, "DAO.singleReturnQuery", ex.Message);
                //exceptionRaised?.Invoke(this, err.handler(EnumMensajes.errorSQL) + " " + conName + " " + ex.Message + "DAO.singleReturnQuery");
            }
            finally
            {
                connection.Close();
            }
            return string.Empty;
        }

        // Consulta generica "select" que retorna un datatable
        public DataTable tableReturnQuery(string query)
        {

            try
            {
                // Check for an available connection
                startConnection();
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
            catch (Exception ex)
            {
                err.sqlThrower(conName, query, "DAO.tableReturnQuery", ex.Message);
                //exceptionRaised?.Invoke(this, err.handler(EnumMensajes.errorSQL) + " " + conName + " " + ex.Message + "DAO.genericSelectQuery");
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        // Consulta genérica que retorna una lista vertical.
        public ObservableCollection<string> verticalCollectionReturnQuery(string query)
        {
            startConnection();
            ObservableCollection<string> lista = new ObservableCollection<string>();
            DataTable tabla = tableReturnQuery(query);

            if (tabla != null)
            {
                foreach (DataRow row in tabla.Rows)
                {
                    lista.Add(row[0].ToString());
                }
            }

            return lista;
        }

        #endregion


    }
}
