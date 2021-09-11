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
using System.IO;

namespace HormesaFILEIDS.model
{
    internal class SwActiveDocument
    {
        #region Constantes
        private const string CustomPropertyPartid = "PARTID";
        private const string CustomPropertyDwgid = "DWGID";

        #endregion

        #region Private fields

        private CustomPropertyManager custPropMgr;
        private string partId;
        private ModelDoc2 swModel;
        private Configuration activeConfig;
        private DAO dao;
        private queryDump q;
        private string descriptorEs;
        private ErrorHandler err;
        private SwAttributeHandler attHandler;
        private SldWorks swApp;

        #endregion

        #region Constructor

        public SwActiveDocument(ModelDoc2 swmodel, SldWorks swapp, DAO daObject)
        {
            // Obtener el swModel.
            swModel = swmodel;
            //Obtener la aplicacion solidworks.
            swApp = swapp;
            //Modelo de acceso a datos.
            dao = daObject;
            // Querydump
            q = new queryDump();
            //Si el archivo actual está en la DB, setear el partid interno de la clase para tener identificado el archivo.
            partId = dao.singleReturnQuery(q.getFilePartIdFromPath(swModel.GetPathName()));
            //Reescribir partid en el archivo.
            writePartIdToFile();
            //Error handler
            err = new ErrorHandler();
            //Leer descriptorES desde el archivo.
            descriptorEs = swModel.Extension.CustomPropertyManager[""].Get("DESCRIPTORES");
        }

        #endregion

        #region Propiedades Publicas
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

        //El descriptorEs
        public string DescriptorEs
        {
            set
            {
                descriptorEs = value;
                writePropertyToFile("DESCRIPTORES", descriptorEs);
            }
            get
            {
                return descriptorEs;
            }
        }

        #endregion

        #region Métodos
        //Mostrar atributo.
        public void toggleAttVisibility(bool isVisible)
        {
            if (attHandler != null)
            {
                attHandler.toggleAttVisibility(isVisible);
            }

        }
        //Determinar si es un plano.
        public bool isDrawing()
        {
            return swModel.GetType() == (int)swDocumentTypes_e.swDocDRAWING;

        }

        //Obtener el partid formateado.
        public string getFormattedPartId(string configName)
        {
            if (!string.IsNullOrEmpty(partId) && !string.IsNullOrEmpty(configName))
            {
                if (string.Equals(configName, "@"))
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

        //Obtener las configuraciones de este archivo (Desde el archivo)
        public ObservableCollection<string> getFileConfigList()
        {
            return (arrayToObsCollection((string[])swModel.GetConfigurationNames()));
        }

        //Inserta archivo en la DB

        public bool insertFileOnDb()
        {
            //Insertar el archivo activo en la DB y guardar el partid local

            string tempPartId = dao.singleReturnQuery(q.insertFileFromPath(swModel.GetPathName()));
            //Si el registro fue exitoso...
            if (!string.IsNullOrEmpty(tempPartId))
            {
                //Asignar el nuevo partid.
                partId = tempPartId;
                //Escribir el partid como custom property.
                writePartIdToFile();
                //Escibir tambien en el atributo.
                setPartIdAsAttribute();
                //Notificar
                err.thrower(err.handler(EnumMensajes.registroExitoso, "PARTID: " + attHandler.getPartIdFromAttribute()));
                return true;
            }
            return false;
        }

        //Leer propiedad por nombre
        internal string getPropertyByName(string propertyName)
        {
            string valOut, resolvedValOut;
            swModel.Extension.CustomPropertyManager[""].Get2(propertyName, out valOut, out resolvedValOut);

            return valOut;
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
            if (arr != null)
            {
                foreach (string row in arr)
                {
                    obsCol.Add(row);
                }
            }
            return obsCol;
        }

        //Asignar partid al property (archivo)
        public void writePartIdToFile()
        {
            custPropMgr = swModel.Extension.CustomPropertyManager[""];
            if (isDrawing())
            {
                custPropMgr.Add3(CustomPropertyDwgid, (int)swCustomInfoType_e.swCustomInfoText, getFormattedPartId("@"), (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd);
            }
            else
            {
                custPropMgr.Add3(CustomPropertyPartid, (int)swCustomInfoType_e.swCustomInfoText, getFormattedPartId("@"), (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd);
            }
        }

        //Escribir cualquier propiedad en el archivo
        public void writePropertyToFile(string propertyName, string propertyValue)
        {
            custPropMgr = swModel.Extension.CustomPropertyManager[""];
            if (isDrawing())
            {
                custPropMgr.Add3(propertyName, (int)swCustomInfoType_e.swCustomInfoText, propertyValue, (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd);
            }
            else
            {
                custPropMgr.Add3(propertyName, (int)swCustomInfoType_e.swCustomInfoText, propertyValue, (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd);
            }
        }

        //Asignar partid al property (archivo)
        public void writePartIdToConfig(string configName)
        {
            custPropMgr = swModel.Extension.CustomPropertyManager[configName];
            custPropMgr.Add3("PARTID", (int)swCustomInfoType_e.swCustomInfoText, getFormattedPartId(configName), (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd); ;
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
            dao.singleReturnQuery(q.deleteConfigFromFilePartID(partId, configName));
        }

        //Reescribir en todas las configuraciones que tengan PARTID, el partid desde la DB.
        internal void writePartIdToAllConfigs()
        {
            //Obtener listado de partids y configs
            DataTable configList = getConfigPartidList();
            if (configList.Rows != null)
            {
                foreach (DataRow row in configList.Rows)
                {
                    writePartIdToConfig(row[0].ToString());
                }
            }
        }

        //Guardar atributos dentro del archivo
        public void setPartIdAsAttribute()
        {
            //Attribute handler
            attHandler = new SwAttributeHandler(swApp, swModel);
            //Escribir partid en los atributos.
            attHandler.writePartIdOnAttribute(partId);
        }
        /// <summary>
        /// Este método corrige inconsistencias con la data cuando un atributo se corrompe
        /// 
        /// </summary>
        /// <returns></returns>
        //Arreglar inconsistencias entre la ruta del modelo y la ruta guardada en la DB. Se usa el atributo como "cookie" o identificador permanente.
        public bool fixPathIntegrity()
        {

            if (attHandler == null)
            {
                attHandler = new SwAttributeHandler(swApp, swModel);
            }

            //Obtener PATH del archivo y el que existe en la BD para el partid interno
            string modelPath = swModel.GetPathName();
            //El partid almacenado en el atributo
            string attPartId = attHandler.getPartIdFromAttribute();
            //El partid almacenado en la DB.
            string dbPartid = dao.singleReturnQuery(q.getFilePartIdFromPath(modelPath));

            //Caso 1- El atributo tiene PARTID,la BD no. Posible archivo movido.
            if (string.IsNullOrEmpty(dbPartid) && !string.IsNullOrEmpty(attPartId))
            {
                //Reasignar partid  
                partId = attPartId;
                //Actualizar base de datos.
                dao.singleReturnQuery(q.updatePathFromPartId(partId, modelPath));
                return true;
            }
            //Caso 2- El atributo tiene partid, la BD tambien y son iguales y no vacios.
            if (string.Equals(dbPartid, attPartId) && !string.IsNullOrEmpty(attPartId) && !string.IsNullOrEmpty(dbPartid))
            {
                //Todo correcto.
                partId = attPartId;
                return true;
            }

            //Caso 3 - El atributo no tiene partid, la BD si tiene
            if (string.IsNullOrEmpty(attPartId) && !string.IsNullOrEmpty(dbPartid))
            {
                //Reasignar partid  
                partId = dbPartid;
                //Reescribir atributo
                attHandler.writePartIdOnAttribute(partId);
                return true;
            }

            //Caso 4 - El atributo no tiene partid, la BD tampoco

            if (string.IsNullOrEmpty(attPartId) && string.IsNullOrEmpty(dbPartid))
            {
                err.yesNoThrower(err.handler(EnumMensajes.atributoVacio));
                return false;
            }

            //Caso 5 - Ambos partids son diferentes y no vacios.
            //if (!string.Equals(dbPartid, attPartId) && !string.IsNullOrEmpty(attPartId) && !string.IsNullOrEmpty(dbPartid))
            //{
            //    //Todo correcto.
            //    partId = attPartId;
            //    //Actualizar base de datos.
            //    dao.singleReturnQuery(q.updatePathFromPartId(partId, modelPath));
            //    return true;
            //}
            return false;
        }

        //Guardar una copia del archivo actual, desacoplandolo de la base de datos al eliminar su atributo PARTID y cambiar su nombre.
        //Es posible registrarlo de nuevo despues.
        internal void saveDecoupledFile()
        {
            //Obtener la extensión actual del archivo.
            string fileExtension = Path.GetExtension(swModel.GetPathName());
            //Obtener directorio del archivo.
            string filePath = Path.GetDirectoryName(swModel.GetPathName());
            //Renombrar documento. Usar el descriptor que esta guardado en el documento
            int errors = 0;
            int warnings = 0;
            //Guardar el archivo como copia con nuevo nombre
            swModel.Extension.SaveAs(string.Format("{0}\\{1} {2}{3}", filePath, "Copia de", getFormattedPartId("@"), fileExtension), 0, 0, null, errors, warnings);
            //Borrar partid.
            partId = string.Empty;
            //Eliminar el atributo.
            if (attHandler == null)
            {
                attHandler = new SwAttributeHandler(swApp, swModel);
            }
            attHandler.deletePartIdAttribute();
        }
        #endregion

    }
}
