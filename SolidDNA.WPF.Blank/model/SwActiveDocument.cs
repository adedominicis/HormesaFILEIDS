using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HormesaFILEIDS.model
{
    public class SwActiveDocument
    {
        #region Delegates


        #endregion

        #region Private fields
        private const string CustomPropertyPartid = "PARTID";
        private CustomPropertyManager custPropMgr;
        private string partId;
        private ModelDoc2 swModel;
        private Configuration activeConfig;
        private DAO dao;
        private queryDump q;
        private string descriptorEs;
        private ErrorHandler err;
        #endregion

        #region Constructor

        public SwActiveDocument(ModelDoc2 swmodel)
        {
            // Obtener el swModel.
            swModel = swmodel;
            //Modelo de acceso a datos.
            dao = new DAO();
            // Querydump
            q = new queryDump();
            //Si el archivo actual está en la DB, setear el partid interno de la clase para tener identificado el archivo.
            partId = dao.singleReturnQuery(q.getFilePartIdFromPath(swModel.GetPathName()));
            //Reescribir partid en el archivo.
            writePartIdToFile();
            //Error handler
            err = new ErrorHandler();
        }

        #endregion

        #region Public Properties
        //Partid del componente.
        public string PartId
        {
            get { return partId; }
        }

        //Descriptor ES
        public string DescriptorEs
        {
            get { return descriptorEs; }
            set { descriptorEs = value; }
        }

        //La configuración activa.
        public string ActiveConfigName
        {
            get { return getActiveConfigName(); }
        }

        #endregion

        #region Methods

        //Obtener el partid formateado.
        public string getFormattedPartId(string configName)
        {
            if (!string.IsNullOrEmpty(partId) && !string.IsNullOrEmpty(configName))
            {
                if (string.Equals(configName,"@"))
                {
                    return partId.ToString().PadLeft(6, '0').Insert(3, "-");
                }
                else
                {
                    string configPartId = dao.singleReturnQuery(q.getConfigPartIdFromConfigName(partId, configName));
                    if (!string.IsNullOrEmpty(configPartId))
                    {
                        return configPartId.PadLeft(6, '0').Insert(3, "-");
                    }
                    
                }
            }
            return string.Empty;

        }

        //Obtener las configuraciones que hay guardadas en la DB con este partid.
        public DataTable getConfigListFromDB()
        {
            return dao.tableReturnQuery(q.listConfigsFromFilePartID(partId));
        }

        //Obtener las configuraciones de este archivo
        public ObservableCollection<string> getFileConfigList()
        {
            return (arrayToObsCollection((string[])swModel.GetConfigurationNames()));
        }

        //Inserta archivo en la DB y le asigna partid.
        public bool insertFileOnDb()
        {
            //Insertar el archivo activo en la DB y guardar el partid local
            partId = dao.singleReturnQuery(q.insertFileFromPath(swModel.GetPathName()));
            writePartIdToFile();
            return !string.IsNullOrEmpty(partId);
        }

        //Obtener listado de partids por configuracion.
        public DataTable getConfigPartidList()
        {
            return dao.tableReturnQuery(q.listConfigsFromFilePartID(partId));
        }

        //Asignar partid a la configuración seleccionada.
        public bool assignPartIdToConfig(string configName)
        {
            if (!string.IsNullOrEmpty(partId))
            {
                //Partid del archivo existe.
                dao.singleReturnQuery(q.assignPartIdToConfigFromFileID(partId, configName));
                //Escribir propiedad personalizada.
                writePartIdToConfig(configName);
                return true;
            }
            else
            {
                return false;
            }

        }

        //Obtener nombre de la configuración activa
        private string getActiveConfigName()
        {
            activeConfig = (Configuration)swModel.GetActiveConfiguration();
            return activeConfig.Name;
        }

        //Convertir un array de strings a un ObservableCollection
        private ObservableCollection<string> arrayToObsCollection(string[] arr)
        {
            ObservableCollection<string> obsCol = new ObservableCollection<string>();
            foreach (string row in arr)
            {
                obsCol.Add(row);
            }
            return obsCol;
        }

        //Asignar partid al property (archivo)
        public void writePartIdToFile()
        {
            custPropMgr = swModel.Extension.CustomPropertyManager[""];
            custPropMgr.Add3("PARTID", (int)swCustomInfoType_e.swCustomInfoText, getFormattedPartId("@"), (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd); ;
        }

        //Asignar partid al property (archivo)
        public void writePartIdToConfig(string configName)
        {

            custPropMgr = swModel.Extension.CustomPropertyManager[configName];
            custPropMgr.Add3("PARTID", (int)swCustomInfoType_e.swCustomInfoText, getFormattedPartId(configName), (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd); ;
        }

        //Renombrar el archivo.
        internal void renameFile()
        {
            err.thrower(err.handler(EnumMensajes.metodoNoImplementado, "SwActiveDocument::renameFile()"));
        }

        //Renombrar en la BD una configuración que fue renombrada en la interfaz.
        internal void renameConfigOnDatabase(string oldName, string newName)
        {
            //Renombrar en la base de datos.
            dao.singleReturnQuery(q.renameConfigFromFilePartId(partId, oldName, newName));
            
        }

        //Eliminar en la BD una configuración que fue eliminada en la interfaz
        internal void deleteConfigurationFromDatabase(string configName)
        {
            //Borrar de la BD
            dao.singleReturnQuery(q.deleteConfigFromFilePartID(partId,configName));
        }
        internal void renameFileOnDatabase(string oldName, string newName)
        {
            err.thrower(err.handler(EnumMensajes.metodoNoImplementado, "SwActiveDocument::renameFileOnDatabase()"));
        }

        //Reescribir en todas las configuraciones que tengan PARTID, el partid desde la DB.
        internal void writePartIdToAllConfigs()
        {
            err.thrower(err.handler(EnumMensajes.metodoNoImplementado, "SwActiveDocument::writePartIdToAllConfigs()"));
        }


        #endregion

    }
}
