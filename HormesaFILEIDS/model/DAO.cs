﻿using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace HormesaFILEIDS.model
{
    public class DAO

    {
        // Data access object.
        private string conStr;
        private SqlConnection connection;
        private string conName;
        private queryDump q = new queryDump();
        public event EventHandler<string> exceptionRaised;

        #region Conexión a DB
        // Database index:  1:localtest,  2: azureproductionmirs

        private int dbIndex = 1;
        public DAO() { }
        public DAO(int dbindex)
        {
            dbIndex = dbindex;
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
                        conStr = "Server=.;Initial Catalog=HORMESAFILEIDS;Integrated Security=true";
                        break;
                    default:
                        break;
                }

                connection = new SqlConnection(conStr);
                connection.Open();
            }
            catch (Exception e)
            {
                exceptionRaised?.Invoke(this, ErrorHandler.handler(EnumMensajes.errorEnConexionDB) + conName + "\\n" + e.Message);
            }
        }


        // Get connection name
        public string getConnectionName()
        {
            return conName;
        }
        #endregion

        #region Core queries

        // Consulta generica "select" que retorna un solo string. 
        public string singleReturnQuery(string query)
        {

            try
            {
                // Check for an available connection
                startConnection();

                //Todo esto parece estar mal. Deberia usarse sqlcommand.executescalar. Revisar.
                //Dataset
                DataSet ds = new DataSet();
                //Data adapter
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);
                //Llenar dataset con el resultado de dataAdapter
                dataAdapter.Fill(ds, "resultado");
                DataTable tbl = ds.Tables["resultado"];
                return tbl.Rows[0][0].ToString();

            }
            catch (Exception ex)
            {
                exceptionRaised?.Invoke(this, ErrorHandler.handler(EnumMensajes.errorSQL) + " " + conName + " " + ex.Message + "DAO.singleReturnQuery");
            }
            finally
            {
                connection.Close();
            }
            return null;
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
                exceptionRaised?.Invoke(this, ErrorHandler.handler(EnumMensajes.errorSQL) + " " + conName + " " + ex.Message + "DAO.genericSelectQuery");
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