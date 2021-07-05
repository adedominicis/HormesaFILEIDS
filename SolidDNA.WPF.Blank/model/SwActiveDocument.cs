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

namespace HormesaFILEIDS.model
{
    public class SwActiveDocument
    {
        #region Delegates


        #endregion

        #region Private fields
        private const string CustomPropertyPartid = "PARTID";
        private CustomPropertyManager custPropMgr;
        private ModelDocExtension swModelDocExt;
        private string partId;
        private ModelDoc2 swModel;
        private ObservableCollection<string> dbConfigList;
        private Configuration activeConfig;
        private DAO dao;
        private queryDump q;
        private string descriptores;
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
        }

        #endregion

        #region Public Properties
        //Partid del componente.
        public string PartId
        {
            get { return partId; }
        }


        //La configuración activa.
        public string ActiveConfigName
        {
            get { return getActiveConfigName(); }
        }

        #endregion

        #region Methods

        //Obtener el partid estilizado.
        public string getFormattedPartId(string configName="")
        {
            if (!string.IsNullOrEmpty(partId))
            {
                if (string.IsNullOrEmpty(configName))
                {
                    return partId.ToString().PadLeft(6, '0').Insert(3, "-");
                }
                else
                {
                    string configPartId= dao.singleReturnQuery(q.getConfigPartIdFromConfigName(partId, configName));
                    return configPartId.PadLeft(6, '0').Insert(3, "-");
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
            partId=dao.singleReturnQuery(q.insertFileFromPath(swModel.GetPathName()));
            writePartIdToFile();
            return !string.IsNullOrEmpty(partId);
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
        private ObservableCollection<string> arrayToObsCollection (string [] arr)
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
            custPropMgr.Add3("PARTID", (int)swCustomInfoType_e.swCustomInfoText, getFormattedPartId(), (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd); ;
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
    
        }
        #endregion

    }
}
