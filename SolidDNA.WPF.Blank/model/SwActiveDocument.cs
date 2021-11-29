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
using Serilog;

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
        //private string descriptorEs;
        private ErrorHandler err;
        private SwAttributeHandler attHandler;
        private SldWorks swApp;

        #endregion

        #region Constructor

        public SwActiveDocument(ModelDoc2 swmodel, SldWorks swapp, DAO daObject)
        {
            //Error handler
            err = new ErrorHandler();
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
            //Leer descriptorES desde el archivo (deshabilitado 1.0.6)
            //descriptorEs = swModel.Extension.CustomPropertyManager[""].Get("DESCRIPTORES");
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

        /// <summary>
        /// Propiedad DescriptorEs
        /// Deshabilitada en 1.0.6
        /// </summary>
        //public string DescriptorEs
        //{
        //    set
        //    {
        //        descriptorEs = value;
        //        writePropertyToFile("DESCRIPTORES", descriptorEs);
        //    }
        //    get
        //    {
        //        return descriptorEs;
        //    }
        //}

        #endregion

        #region Helpers y data readers

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

        //Determinar si el archivo es un plano
        public bool isDrawing()
        {
            return swModel.GetType() == (int)swDocumentTypes_e.swDocDRAWING;
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


        /// <summary>
        /// Obtener nombre de configuracion activa      
        /// </summary>
        /// <returns>Nombre de la configuracion para piezas y ensamblajes, string vacio para planos</returns>
        private string getActiveConfigName()
        {
            if (!isDrawing())
            {
                activeConfig = (Configuration)swModel.GetActiveConfiguration();
                return activeConfig.Name;
            }
            return string.Empty;
        }
        #endregion

        #region Métodos

        /// <summary>
        /// Mostrar el atributo donde se guarda el partid en el featuremanager tree
        /// </summary>
        /// <param name="isVisible">True para mostrar, false para ocultar.</param>
        public void toggleAttVisibility(bool isVisible)
        {
            if (attHandler != null)
            {
                attHandler.toggleAttVisibility(isVisible);
            }

        }

        //Inserta archivo en la DB
        /// <summary>
        /// Registra este archivo en la DB
        /// </summary>
        /// <returns>True si la operación se realizó.</returns>
        public bool insertFileOnDb()
        {
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
                string attPartId = Helpers.formatPartId(attHandler.getPartIdFromAttribute());
                err.thrower(err.handler(EnumMensajes.registroExitoso, "PARTID: " + attPartId));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Asignar un nuevo PARTID a la configuración seleccionada.
        /// </summary>
        /// <param name="configName">Configuración seleccionada.</param>
        /// <returns></returns>
        //Asignar partid a la configuración seleccionada.
        public bool assignPartIdToConfig(string configName)
        {
            if (!string.IsNullOrEmpty(partId))
            {
                //Partid del archivo existe.
                dao.singleReturnQuery(q.assignPartIdToConfigFromFileID(partId, configName));
                //Escribir propiedad personalizada.
                writePartIdToConfig(configName);
                //Logear actividad
                err.logger(string.Format("Asignado PARTID <{0}> a Configuración <{1}> de {2}", getFormattedPartId(configName), configName, getFormattedPartId("@")));
                return true;
            }
            else
            {
                err.logger(string.Format("Ha fallado el asignar el PARTID <{0}> a configuración <{1}> de {2}", getFormattedPartId(configName), configName, getFormattedPartId("@")));
                return false;
            }

        }

        /// <summary>
        /// Escribir PARTID en la propiedad del archivo.
        /// </summary>
        public void writePartIdToFile()
        {
            //Si el archivo no está guardado, no tiene path y no tiene partid.
            if (!string.IsNullOrEmpty(swModel.GetPathName()))
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
                //Log de actividad
                err.logger(string.Format("PARTID <{0}> ha sido reescrito en custom properties", Helpers.formatPartId(partId)));
            }

        }

        /// <summary>
        /// Escribir cualquier propiedad en el archivo
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
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
            //Log de actividad
            err.logger(string.Format("Propiedad <{0}> ha sido escrita con valor: <{1}>", propertyName, propertyValue));
        }

        //Asignar partid al property (configuracion)
        /// <summary>
        /// Escribir propiedad PARTID en la configuracion   
        /// </summary>
        /// <param name="configName">Configuracion donde se escribe el partid.</param>
        public void writePartIdToConfig(string configName)
        {
            string configPartId = getFormattedPartId(configName);
            custPropMgr = swModel.Extension.CustomPropertyManager[configName];
            custPropMgr.Add3("PARTID", (int)swCustomInfoType_e.swCustomInfoText, configPartId, (int)swCustomPropertyAddOption_e.swCustomPropertyDeleteAndAdd);

            //Log de actividad
            err.logger(string.Format("PARTID <{0}> ha sido escrito en la configuración <{1}> de <{2}>", configPartId, configName, getFormattedPartId("@")));
        }


        /// <summary>
        /// Renombrar en la BD una configuración que fue renombrada en la interfaz.
        /// </summary>
        /// <param name="oldName">Nombre actual de la configuración</param>
        /// <param name="newName">Nombre nuevo de la configuración</param>
        internal void renameConfigOnDatabase(string oldName, string newName)
        {
            //Renombrar en la base de datos.
            dao.singleReturnQuery(q.renameConfigFromFilePartId(partId, oldName, newName));
            //Log de actividad
            err.logger(string.Format("La configuración <{0}> de <{1}> ha sido renombrada a <{2}>", oldName, partId, newName));
        }

        /// <summary>
        /// Eliminar en la BD una configuración que fue eliminada en la interfaz
        /// </summary>
        /// <param name="configName">Nombre de la configuracion eliminada.</param>

        internal void deleteConfigurationFromDatabase(string configName)
        {
            //Borrar de la BD
            dao.singleReturnQuery(q.deleteConfigFromFilePartID(partId, configName));
            //Log de actividad
            err.logger(string.Format("La configuración <{0}> de <{1}> ha sido eliminada", configName, partId));
        }

        /// <summary>
        /// Reescribir en todas las configuraciones que tengan PARTID, el partid desde la DB.
        /// </summary>
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
            //Log de actividad
            err.logger(string.Format("Se reescribieron las propiedades PARTID a todas las configuraciones de {0}", partId));
        }

        //Guardar atributos dentro del archivo
        public void setPartIdAsAttribute()
        {

            //Attribute handler
            attHandler = new SwAttributeHandler(swApp, swModel);
            //Escribir partid en los atributos.
            attHandler.writePartIdOnAttribute(partId);
            //Log de actividad
            err.logger(string.Format("Se actualizó atributo en PARTID <{0}>\nValor registrado en el atributo: <{1}>", partId, Helpers.formatPartId(attHandler.getPartIdFromAttribute())));
        }

        private bool isFileRenamed()
        {
            return false;
        }


        /// <summary>
        /// Este método corrige inconsistencias con la data cuando un atributo se corrompe
        /// 
        /// Se consideran varios casos a saber:
        /// 
        /// 1- Al buscar el partid guardado internamente en la pieza en la BD y comparar la ruta
        /// local con la de la base de datos, se infiere que el archivo fue movido o renombrado.
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

            string dbPath, dbName, dbDir;


            //Obtener PATH del archivo y el que existe en la BD para el partid interno
            string modelPath = swModel.GetPathName();
            //El partid almacenado en el atributo
            string attPartId = attHandler.getPartIdFromAttribute();
            //El partid almacenado en la DB.
            string dbPartid = dao.singleReturnQuery(q.getFilePartIdFromPath(modelPath));
            //Nombre del archivo real
            string modelName = Path.GetFileName(modelPath);
            //Directorio del archivo
            string modelDir = Path.GetDirectoryName(modelPath);

            //Caso 1- El atributo tiene PARTID,la BD no. Posible archivo movido.
            if (string.IsNullOrEmpty(dbPartid) && !string.IsNullOrEmpty(attPartId))
            {
                //Ruta de este archivo en la BD
                dbPath = dao.singleReturnQuery(q.getFilePathFromPartId(attPartId));
                //Directorio de este archivo en la BD
                dbDir = Path.GetDirectoryName(dbPath);
                //Nombre en la BD
                dbName = Path.GetFileName(dbPath);

                //Si el nombre del archivo es el mismo en ambas instancias, el archivo fue movido
                if (string.Equals(modelName, dbName, StringComparison.InvariantCultureIgnoreCase))
                {
                    err.logger(string.Format("El archivo <{0}> fue movido desde\n{1}\nHasta\n{2}\nSe actualiza en la BD", modelName, dbPath, modelPath));
                    //Reasignar partid  
                    partId = attPartId;
                    //Actualizar base de datos.
                    dao.singleReturnQuery(q.updatePathFromPartId(partId, modelPath));
                    return true;
                }
                //Si el directorio es el mismo en ambos casos, el archivo fue renombrado
                else if (string.Equals(modelDir, dbDir, StringComparison.InvariantCultureIgnoreCase))
                {
                    err.logger(string.Format("El archivo <{0}> fue renombrado como\n<{1}>\nSe actualiza en la BD", dbName, modelName));
                    //Reasignar partid  
                    partId = attPartId;
                    //Actualizar base de datos.
                    dao.singleReturnQuery(q.updatePathFromPartId(partId, modelPath));
                    return true;
                }
                //El archivo fue renombrado y movido. Es prudente alertar al usuario
                else
                {
                    string msg = string.Format("El archivo <{0}> fue renombrado como\n<{1}>\nAdicionalmente fue movido desde\n{2}\nHasta:{3}", dbName, modelName, dbDir, modelDir);
                    err.logger(msg);
                    if (err.yesNoThrower(string.Format("{0}\nDesea actualizar los registros bajo el PARTID <{1}>", msg, attPartId)))
                    {
                        //Reasignar partid  
                        partId = attPartId;
                        //Actualizar base de datos.
                        dao.singleReturnQuery(q.updatePathFromPartId(partId, modelPath));
                        err.logger(string.Format("Usuario acepta actualizar movido/renombrado PARTID: <{0}>", Helpers.formatPartId(partId)));
                        return true;
                    }
                    err.logger(string.Format("Usuario no acepta actualizar movido/renombrado PARTID: <{0}>", Helpers.formatPartId(partId)));
                }


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
                //Log actividad
                err.logger(string.Format("El archivo: <{0}> ha perdido su atributo interno.\nReescribiendo desde DB con PARTID <{1}>", modelPath, Helpers.formatPartId(dbPartid)));
                return true;
            }

            //Caso 4 - El atributo no tiene partid, la BD tampoco. En este caso hay que comparar paths

            if (string.IsNullOrEmpty(attPartId) && string.IsNullOrEmpty(dbPartid))
            {
                err.logger(string.Format("El archivo: <{0}> está desconectado de la BD por un error desconocido", modelPath));
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

        /// <summary>
        /// Guardar una copia del archivo actual, 
        /// desacoplandolo de la base de datos al eliminar su atributo PARTID y cambiar su nombre.
        /// Es posible registrarlo de nuevo despues.
        /// </summary>
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
            string decoupledPath = string.Format("{0}\\{1} {2}{3}", filePath, "Copia de", getFormattedPartId("@"), fileExtension);
            swModel.Extension.SaveAs(decoupledPath, 0, 0, null, errors, warnings);
            //Borrar partid.
            partId = string.Empty;
            //Eliminar el atributo.
            if (attHandler == null)
            {
                attHandler = new SwAttributeHandler(swApp, swModel);
            }
            attHandler.deletePartIdAttribute();
            //Log actividad
            err.thrower(err.handler(EnumMensajes.archivoDesconectado, decoupledPath));
            #endregion

        }
    }
}
