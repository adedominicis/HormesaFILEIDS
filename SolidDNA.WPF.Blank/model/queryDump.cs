﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HormesaFILEIDS.model
{
    internal class queryDump
    {
        #region Procedimientos almacenados - Lecturas
        // Listar todas las configuraciones de un archivo, con referencia al PARTID
        public string listConfigsFromFilePartID(string partid)
        {
            return string.Format("exec listConfigsFromFilePartID '{0}'", partid);
        }


        // Retornar el partid de una configuracion suministrando el partid del archivo y el nombre de la configuración.
        public string getConfigPartIdFromConfigName(string filePartid, string configName)
        {
            return string.Format("exec getConfigPartIdFromConfigName '{0}','{1}' ", filePartid, configName);
        }

        //Revisa si un archivo posee una configuración con el nombre suministrado. Usa como referencia el partid del archivo.
        public string checkIfConfigExistsFromFilePartId(string filePartid, string configName)
        {
            return string.Format("exec checkIfConfigExistsFromFilePartId '{0}','{1}' ", filePartid, configName);
        }


        //Obtener el partid desde el path
        public string getFilePartIdFromPath(string fullpath)
        {
            return string.Format("exec getFilePartIdFromPath '{0}'", fullpath);
        }

        //Obtener el path desde el partid.
        public string getFilePathFromPartId(string partid)
        {
            return string.Format("exec getFilePathFromPartId {0}", partid);
        }
        #endregion

        #region Procedimientos almacenados - Escrituras

        //Crear nuevo archivo en la base de datos desde el path suministrado.
        public string insertFileFromPath(string fullpath)
        {
            return string.Format("exec insertFileFromPath '{0}'", fullpath);
        }

        //Asignar una configuración a un archivo, usando como referencia su partid.
        public string createConfigFromFilePartID(string filePartid, string configName)
        {
            return string.Format("exec createConfigFromFilePartID '{0}','{1}' ", filePartid, configName);
        }

        //Asignar una configuración a un archivo, usando como referencia su path
        public string createConfigFromFilePath(string fullPath, string configName)
        {
            return string.Format("exec createConfigFromFilePath '{0}','{1}' ", fullPath, configName);
        }

        //Asignar un partid a una configuracion que no lo tiene, usando como referencia el partid del archivo
        public string assignPartIdToConfigFromFileID(string filePartid, string configName)
        {
            return string.Format("exec assignPartIdToConfigFromFileID '{0}','{1}' ", filePartid, configName);
        }



        //Asignar un partid a una configuracion que no lo tiene, usando como referencia el path del archivo
        //Por definir

        #endregion

        #region Procedimientos Almacenados - Eliminaciones

        //Eliminar una configuracion, dando como referencia el path del archivo y el nombre de la configuración
        public string deleteConfigFromFilePath(string fullPath, string configName)
        {
            return string.Format("exec deleteConfigFromFilePath '{0}','{1}' ", fullPath, configName);
        }

        //Eliminar una configuracion, dando como referencia el partid del archivo y el nombre de la configuración
        public string deleteConfigFromFilePartID(string filePartid, string configName)
        {
            return string.Format("exec deleteConfigFromFilePartID '{0}','{1}' ", filePartid, configName);
        }

        #endregion

        #region Procedimientos Almacenados - Actualizaciones.
        //Renombrar una configuracion desde el path del archivo y el nombre de la configuracion
        public string renameConfigFromFilePath(string fullPath, string oldConfigName, string newConfigName)
        {
            return string.Format("exec renameConfigFromFilePath '{0}','{1}','{2}' ", fullPath, oldConfigName, newConfigName);
        }

        //Renombrar una configuracion desde el ID del archivo y el nombre de la configuracion
        public string renameConfigFromFilePartId(string filePartid, string oldConfigName, string newConfigName)
        {
            return string.Format("exec renameConfigFromFilePartId '{0}','{1}','{2}' ", filePartid, oldConfigName, newConfigName);
        }

        //Actualizar el path de un archivo desde el partid.
        public string updatePathFromPartId(string filePartId, string newPath)
        {
            return string.Format("exec updatePathFromPartId '{0}','{1}' ", filePartId, newPath);
        }
        #endregion

        #region Procedimientos Almacenados - Logging.
        //Logear excepciones comunes
        public string logErrores(Exception ex)
        {
            return string.Format("exec logErrores '{0}','{1}','{2}' ", ex.GetType().ToString(), ex.Message,ex.StackTrace);
        }
        //Logear excepciones SQL
        internal string logSQLErrores(SqlException ex)
        {
            
            return string.Format("exec logErrores '{0}','{1}','{2}' ", ex.GetType().ToString(), ex.Message, ex.StackTrace);
        }
        /// <summary>
        /// Consulta la vista mostrarLogErrores en orden inverso, ultimos primero.
        /// mostrarLogErrores
        /// </summary>
        /// <returns></returns>
        internal string mostrarLogErrores()
        {
            return "select * from mostrarLogErrores order by id desc";
        }
        /// <summary>
        /// Consuota la vista mostrarLogEventos en orden inverso, ultimos primero
        /// </summary>
        /// <returns></returns>
        internal string mostrarLogEventos()
        {
            return "select * from mostrarLogEventos order by id desc";
        }
        /// <summary>
        /// Consuota la vista mostrarLogEventos en orden inverso, ultimos primero
        /// </summary>
        /// <returns></returns>
        internal string listadoArchivosActivos()
        {
            return "select * from listadoArchivosActivos order by PARTID desc";
        }
        #endregion

    }
}
