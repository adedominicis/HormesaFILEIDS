using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
        private const string dbLogin = "FILEIDS";
        private const string dbName = "HORMESAFILEIDS";
        private const string thisAppPass = "capacillo";
        private const string thisAppUser = "admin";
        private const string serverFileName= "server";

        #endregion

        #region Campos privados
        private DAO dao;
        private ErrorHandler err;
        #endregion

        #region Constructor
        public AuthenticationHandler()
        {
            dao = new DAO(this);
            err = new ErrorHandler();
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
        public string DbLogin
        {
            get { return dbLogin; }
        }
        public string DbServerIp
        {
            get { return readServerFile(); }
        }

        //Objeto de acceso a datos.
        public DAO Dao
        {
            get { return dao; }
        }
        #endregion

        #region Métodos
        //Setear la IP del servidor en el archivo local.
        public bool updateServerIp(string newIp)
        {
            string serverFilePath = "";
            try
            {
                //Path entero del archivo server.
                serverFilePath = string.Format("{0}\\Resources\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), serverFileName);

                using (StreamWriter writer = new StreamWriter(serverFilePath))
                {
                    writer.Write(newIp);
                }
                //Actualizar el DAO
                string updatedIp = readServerFile();
                //Verificar si de verdad se pudo escribir el archivo.
                if (string.Equals(updatedIp, newIp))
                {
                    dao = new DAO(this);
                    return true;
                }
                else
                {
                    return false;
                }
               
            }
            catch (Exception ex)
            {
                err.handler(EnumMensajes.errorEscribiendoArchivoServer, ex.Message);
            }
            return false;
        }

        //Login interno de la aplicación. Usado para alterar algunos settings en la UI
        public bool authAppLogin(string user, string pass)
        {
            return string.Equals(user, thisAppUser) && string.Equals(pass, thisAppPass);
        }

        //Obtiene el nombre del servidor almacenado en el cookie.
        public string readServerFile()
        {
            string serverFilePath = "";
            string installFolder = "";
            try
            {
                //Directorio en donde está instalada la aplicación.
                installFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                serverFilePath = string.Format("{0}\\Resources\\{1}", installFolder, serverFileName);
                return File.ReadAllText(serverFilePath);
            }
            catch (Exception)
            {
                err.handler(EnumMensajes.errorLeyendoArchivoServer, serverFilePath);
            }
            return string.Empty;
        }

        //Validar administrador para actualizar IP.

        public bool validateAdmin(string password)
        {
            return string.Equals(password, thisAppPass,StringComparison.CurrentCulture);
        }
        #endregion
    }
}
