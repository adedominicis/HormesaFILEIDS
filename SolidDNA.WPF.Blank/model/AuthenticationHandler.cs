using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HormesaFILEIDS.model
{
    /// <summary>
    /// Clase que guarda y encripta credenciales de acceso a la base de datos y al servidor para casos en los que no se usa integrated security.
    /// </summary>
    internal class AuthenticationHandler
    {
        #region Constantes
        private const string dbPassword = "HfIdS1000210721";
        private const string dbUser = "FILEIDSUSER";
        private const string dbName = "HORMESAFILEIDS";
        private string dbServerName;
        #endregion


        #region Constructor
        public AuthenticationHandler(string serverName)
        {
            dbServerName = serverName;
        }
        #endregion

        #region Properties
        public string DbPassword
        {
            get { return dbPassword; }
        }
        public string DbUser
        {
            get { return dbUser; }
        }
        public string DbName
        {
            get { return dbName; }
        }
        public string DbServerName
        {
            get { return dbServerName; }
        }


        #endregion
    }
}
