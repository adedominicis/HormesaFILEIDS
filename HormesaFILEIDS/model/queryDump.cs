using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HormesaFILEIDS.model
{
    public class queryDump
    {
        #region Procedimientos almacenados - Lecturas
        // Listar todas las configuraciones de un archivo, con referencia al PARTID
        public string listConfigsFromFilePartID(string partid)
        {
            return string.Format("exec listConfigsFromFilePartID {0}", partid);
        }
        // Retornar el partid de una configuracion suministrando el partid del archivo y el nombre de la configuración.
        public string getConfigPartIdFromConfigName(int filePartid,string configName)
        {
            return string.Format("exec getConfigPartIdFromConfigName {0},{1} ", filePartid, configName);
        }

        #endregion

        #region Procedimientos almacenados - Escrituras

        //Crear nuevo archivo en la base de datos desde el path suministrado.
        public string insertFileFromPath (string fullpath)
        {
            return string.Format("exec insertFileFromPath {0}", fullpath);
        }

        #endregion
    }
}
