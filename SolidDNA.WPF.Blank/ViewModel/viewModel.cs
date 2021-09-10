using HormesaFILEIDS.model;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace HormesaFILEIDS.ViewModel
{

    internal class viewModel : INotifyPropertyChanged
    {

        #region SolidWorks API Private Fields
        private SldWorks swApp;
        private PartDoc swPart;
        private AssemblyDoc swAssy;
        private DrawingDoc swDraw;
        private ModelDoc2 swModel;
        #endregion

        #region Other private fields
        private string selectedConfig;
        private UIHelper uiHelper;
        private SwActiveDocument swActiveDoc;
        //Instancia de la vista.
        private MyAddinControl myView;
        private DAO dao = new DAO();
        private AuthenticationHandler authHdlr;
        private string taskPaneMsg;
        #endregion

        #region Public properties

        public ErrorHandler err;
        //Sw Model (GET and SET)
        private ModelDoc2 SwModel
        {
            get { return swModel; }
            set
            {
                swModel = value;
                SwModelEventSubscriber();
            }
        }


        //Partid de pieza (GET)
        public string PartId
        {
            get
            {
                if (swActiveDoc != null)
                {
                    return swActiveDoc.getFormattedPartId("@");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        //Partid de la configuracion seleccionada (GET)
        public string ConfigPartId
        {
            get
            {
                if (swActiveDoc != null)
                {
                    //Metodo por definir.

                    return swActiveDoc.getFormattedPartId(selectedConfig);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        //Lista de configuraciones del documento activo. (GET)
        public ObservableCollection<string> ListActiveDocumentConfigurations
        {
            get
            {
                if (swActiveDoc != null)
                {
                    return swActiveDoc.getFileConfigList();
                }
                else
                {
                    return new ObservableCollection<string>();
                }
            }
        }

        //Identificador de la configuración seleccionada en el combobox. (SET + OnPropertyChanged)
        public string SelectedConfiguration
        {
            set
            {
                selectedConfig = value;
                OnPropertyChanged("SelectedConfiguration");
                OnPropertyChanged("ConfigPartId");
            }
        }

        //Documento activo. (GET)
        public SwActiveDocument SwActiveDoc
        {
            get { return swActiveDoc; }
        }

        //Tabla de configuraciones y partids (GET) //Esto falla con unhandled exception cuando la base de datos está caida
        public DataView DataViewConfigsAndPartids
        {
            get
            {
                if (swActiveDoc != null)
                {
                    return swActiveDoc.getConfigPartidList().DefaultView;
                }
                return new DataView();
            }

        }

        //Descriptor ES
        public string DescriptorEs
        {
            get
            {
                if (swActiveDoc != null)
                {
                    return swActiveDoc.DescriptorEs;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (swActiveDoc != null)
                {
                    swActiveDoc.DescriptorEs = value;
                    OnPropertyChanged("DescriptorEs");
                }
            }
        }

        //Auth Handler  
        public AuthenticationHandler AuthHldr
        {
            get { return authHdlr; }
        }

        //Mensajes
        public string TaskPaneMsg
        {
            set
            {
                taskPaneMsg = value;
                OnPropertyChanged("TaskPaneMsg");
            }
            get { return taskPaneMsg; }

        }
        //Mostrar atributo
        public bool CbShowAttribute
        {
            set
            {
                swActiveDoc.toggleAttVisibility(value);
                OnPropertyChanged("CbShowAttribute");
            }
        }

        #endregion

        #region Inicializadores

        public void initializeViewModel(MyAddinControl v)
        {
            try
            {
                // Obtener Instancia activa de solidworks
                swApp = (SldWorks)System.Runtime.InteropServices.Marshal.GetActiveObject("SldWorks.Application");
                // Suscribirse a los eventos de la instancia de solidworks (swApp)
                SwAppEventSubscriber();
                //Instanciar algunos singletons importantes.
                uiHelper = new UIHelper();
                //Instancia de la vista
                myView = v;
                //Autenticador
                authHdlr = new AuthenticationHandler();
                //Error handler
                err = new ErrorHandler();
            }
            catch (Exception ex)
            {
                err.thrower(err.handler(EnumMensajes.excepcionInterna, "Error en inicializacion de viewModel: " + ex.Message,ex));
            }
        }
        /// <summary>
        /// Obtener del DAO una tabla con los errores logeados.
        /// </summary>
        /// <param name="errorLogWindow"></param>
        public void loadErrorLogWindowData(ErrorLogWindow errorLogWindow)
        {
            DataTable dtLogErrores = dao.getErrorLogs();
            errorLogWindow.dgridLogErrores.ItemsSource = dtLogErrores.DefaultView;
        }

        //Refrescar componentes dinámicos de la UI 

        private void updateCombosAndTables()
        {
            //Llenar combobox de configuraciones.
            myView.cbConfiguraciones.ItemsSource = ListActiveDocumentConfigurations;
            //Llenar tabla de resumen de configuraciones
            myView.dgridPartids.ItemsSource = DataViewConfigsAndPartids;
        }
        private void toggleUIMode()
        {
            bool notDrawing = !swActiveDoc.isDrawing();
            //Alternar entre el modo planos o modo ensamblajes/piezas. Esto puede hacerse a traves de un binding a una propiedad posiblemente, o de forma directa.
            myView.gridConfiguraciones.IsEnabled = notDrawing;
            myView.lblConfiguracion.IsEnabled = notDrawing;
            myView.lblResumen.IsEnabled = notDrawing;
            myView.dgridPartids.IsEnabled = notDrawing;
            if (!swActiveDoc.isDrawing())
            {
                myView.gridConfiguraciones.Visibility = Visibility.Visible;
                myView.gridConfiguraciones.Visibility = Visibility.Visible;
                myView.lblConfiguracion.Visibility = Visibility.Visible;
                myView.lblResumen.Visibility = Visibility.Visible;
                myView.dgridPartids.Visibility = Visibility.Visible;
            }
            else
            {
                myView.gridConfiguraciones.Visibility = Visibility.Hidden;
                myView.gridConfiguraciones.Visibility = Visibility.Hidden;
                myView.lblConfiguracion.Visibility = Visibility.Hidden;
                myView.lblResumen.Visibility = Visibility.Hidden;
                myView.dgridPartids.Visibility = Visibility.Hidden;
            }

        }
        private void updateTextBoxes()
        {
            OnPropertyChanged("PartId");
            OnPropertyChanged("ConfigPartId");
            OnPropertyChanged("DescriptorEs");
        }
        public void initComboboxes()
        {
            //Seleccionar primero.
            myView.cbConfiguraciones.SelectedIndex = 0;
        }
        private void updateButtons()
        {
            if (string.IsNullOrEmpty(PartId))
            {
                myView.btNuevoPartIdComponente.Content = "ASIGNAR PARTID";
            }
            else
            {
                myView.btNuevoPartIdComponente.Content = "DESCONECTAR Y COPIAR";
            }
        }
        private void initCheckboxes()
        {
            if (swActiveDoc!=null)
            {
                //El atributo se muestra en funcion de que el checkbox esté activo.
                swActiveDoc.toggleAttVisibility((bool)myView.cbMostrarAtributo.IsChecked);
            }
  
        }
        public void refreshUI(bool initCombos = false)
        {

            //Campos sin databinding: ip del servidor se obtiene de un archivo local.
            myView.txServerData.Text = authHdlr.DbServerIp;

            if (swModel != null)
            {
                //Si el archivo está abierto en read only, hay que deshabilitar el panel principal
                myView.swStackPanel.IsEnabled = !swModel.IsOpenedReadOnly();
                //Si el servidor está conectado, se puede actualizar.
                if (dao.IsServerConnected())
                {
                    TaskPaneMsg = string.Empty;
                    toggleUIMode();
                    updateCombosAndTables();
                    updateTextBoxes();
                    updateButtons();
                    initCheckboxes();
                    if (initCombos)
                    {
                        initComboboxes();
                    }
                }
                else
                {
                    TaskPaneMsg = "Problemas de conexión con la base de datos...";
                    myView.swStackPanel.IsEnabled = false;
                }
            }
            else
            {
                myView.swStackPanel.IsEnabled = false;
            }


        }
        #endregion

        #region TaskPane Methods and listeners.
        //Los botones accionados en la vista llaman métodos aqui.
        //Esta suerte de "daisychaining" de métodos puede tener una mejor solución.
        //¿O es una forma de mediador?

        //Asignar nuevo partid a una configuración
        public bool asignarPartIdAConfig()
        {
            if (swModel != null && swActiveDoc != null)
            {
                if (swActiveDoc.assignPartIdToConfig(selectedConfig))
                {
                    //Evento
                    OnPropertyChanged("ConfigPartId");
                    updateCombosAndTables();
                    return true;
                }
                else
                {
                    //Algun mensaje aqui. No pudo asignarse partid.
                    return false;
                }
            }
            else
            {
                //Algun mensaje sobre archivo no disponible.
                return false;
            }


        }

        //Asignar nuevo partid a la pieza, ensamblaje o plano.
        public bool asignarPartid()
        {
            //Existe un archivo abierto y activo.
            if (swModel != null && swActiveDoc != null)
            {
                //El archivo no ha sido guardado por primera vez.
                if (string.IsNullOrEmpty(swModel.GetPathName()))
                {
                    err.thrower(err.handler(EnumMensajes.archivoNoGuardado));
                    return false;
                }
                //De lo contrario, se intenta insertar el archivo en la DB
                if (swActiveDoc.insertFileOnDb())
                {
                    OnPropertyChanged("PartId");
                    return true;
                }
                //Si no se puede insertar el archivo por estar repetido, se ofrece desconexión.
                else if (err.yesNoThrower(err.handler(EnumMensajes.yaTienePartid, SwActiveDoc.getFormattedPartId("@"))))
                {
                    //Usuario quiere resetear el partid
                    swActiveDoc.saveDecoupledFile();
                    //Refrescar interfaz.
                    refreshUI(true);
                    return true;
                }
                return false;
            }
            else
            {
                //No hay un documento activo.
                err.thrower(err.handler(EnumMensajes.noHayDocumentoActivo));
                return false;
            }
        }

        //Renombrar archivo
        internal void renombrarArchivo()
        {
            if (swActiveDoc != null)
            {
                //Esta funcionalidad está rota. Crea copias en lugar de renombrar. Estará desactivada hasta que se repare.
                //swActiveDoc.saveFileCopyAs(DescriptorEs);
            }
        }

        #endregion

        #region Solidworks Event Subscribers

        // Subscripción a los eventos de la instancia de solidworks (swApp)
        private void SwAppEventSubscriber()
        {
            //Desuscribir primero:
            swApp.ActiveDocChangeNotify -= SwApp_ActiveDocChangeNotify;
            swApp.FileOpenNotify -= SwApp_FileOpenNotify;
            swApp.FileNewNotify2 -= SwApp_FileNewNotify2;

            //Suscribir en limpio
            swApp.ActiveDocChangeNotify += SwApp_ActiveDocChangeNotify;
            swApp.FileOpenNotify += SwApp_FileOpenNotify;
            swApp.FileNewNotify2 += SwApp_FileNewNotify2;

        }

        //Suscripción a los eventos de parte, pieza o ensamble mediante delegates (swModel)
        public void SwModelEventSubscriber()
        {
            try
            {
                //Crear objetos swDoc
                if (swModel.GetType() == (int)swDocumentTypes_e.swDocPART)
                {
                    swPart = (PartDoc)swModel;

                    //Cambio de configuracion activa
                    swPart.ActiveConfigChangePostNotify -= SwComponent_ActiveConfigChangeNotify;
                    swPart.ActiveConfigChangePostNotify += SwComponent_ActiveConfigChangeNotify;

                    //Agregada configuracion
                    swPart.AddItemNotify -= SwComponent_AddItemNotify;
                    swPart.AddItemNotify += SwComponent_AddItemNotify;

                    //Antes de eliminar la configuración
                    swPart.DeleteItemPreNotify -= SwComponent_DeleteItemPreNotify;
                    swPart.DeleteItemPreNotify += SwComponent_DeleteItemPreNotify;

                    //Eliminada configuracion
                    swPart.DeleteItemNotify -= SwComponent_DeleteItemNotify;
                    swPart.DeleteItemNotify += SwComponent_DeleteItemNotify;

                    //Renombrada configuración
                    swPart.RenameItemNotify -= SwComponent_RenameItemNotify;
                    swPart.RenameItemNotify += SwComponent_RenameItemNotify;

                    //Se guardó una pieza.
                    swPart.FileSaveNotify -= SwComponent_FileSaveNotify;
                    swPart.FileSaveNotify += SwComponent_FileSaveNotify;
                }
                else if (swModel.GetType() == (int)swDocumentTypes_e.swDocASSEMBLY)
                {
                    swAssy = (AssemblyDoc)swModel;

                    //Cambio de configuracion activa
                    swAssy.ActiveConfigChangePostNotify -= SwComponent_ActiveConfigChangeNotify;
                    swAssy.ActiveConfigChangePostNotify += SwComponent_ActiveConfigChangeNotify;

                    //Agregada configuracion
                    swAssy.AddItemNotify -= SwComponent_AddItemNotify;
                    swAssy.AddItemNotify += SwComponent_AddItemNotify;

                    //Antes de eliminar la configuración
                    swAssy.DeleteItemPreNotify -= SwComponent_DeleteItemPreNotify;
                    swAssy.DeleteItemPreNotify += SwComponent_DeleteItemPreNotify;

                    //Eliminada configuracion
                    swAssy.DeleteItemNotify -= SwComponent_DeleteItemNotify;
                    swAssy.DeleteItemNotify += SwComponent_DeleteItemNotify;

                    //Renombrada configuración
                    swAssy.RenameItemNotify -= SwComponent_RenameItemNotify;
                    swAssy.RenameItemNotify += SwComponent_RenameItemNotify;

                    //Se guardó un ensamblaje
                    swAssy.FileSaveNotify -= SwComponent_FileSaveNotify;
                    swAssy.FileSaveNotify += SwComponent_FileSaveNotify;
                }
                else if (swModel.GetType() == (int)swDocumentTypes_e.swDocDRAWING)
                {
                    swDraw = (DrawingDoc)swModel;
                    //Se guardó un plano
                    swDraw.FileSaveNotify -= SwDraw_FileSaveNotify;
                    swDraw.FileSaveNotify += SwDraw_FileSaveNotify;
                }
            }
            catch (Exception ex)
            {
                err.thrower(err.handler(EnumMensajes.excepcionInterna, ex.Message + "en viewModel::SwModelEventSubscriber",ex));
            }

        }

        #endregion

        // Delegate Methods suscritos a eventos de SolidWorks

        #region 1- Se crea un nuevo documento

        private int SwApp_FileNewNotify2(object NewDoc, int DocType, string TemplateName)
        {
            //Actualizar swModel
            SwModel = (ModelDoc2)NewDoc;
            //Instanciar documento activo.
            swActiveDoc = null;
            swActiveDoc = new SwActiveDocument(swModel, swApp, dao);
            //Actualizar taskpane
            refreshUI(true);
            return 0;
        }
        #endregion

        #region 2- Abrir documento
        private int SwApp_FileOpenNotify(string FileName)
        {
            //Actualizar swModel
            SwModel = (ModelDoc2)swApp.ActiveDoc;
            //Instanciar documento activo.
            swActiveDoc = null;
            swActiveDoc = new SwActiveDocument(swModel, swApp, dao);
            //Revisar integridad de los datos entre el modelo y la BD.
            swActiveDoc.fixPathIntegrity();
            //Actualizar taskpane
            refreshUI(true);
            return 0;
        }
        #endregion

        #region 3- Cambiar documento activo
        private int SwApp_ActiveDocChangeNotify()
        {
            //Actualizar swModel
            SwModel = (ModelDoc2)swApp.ActiveDoc;
            //Instanciar documento activo.
            swActiveDoc = null;
            swActiveDoc = new SwActiveDocument(swModel, swApp, dao);
            //Actualizar taskpane
            refreshUI(true);
            return 0;
        }
        #endregion

        #region 4- Se añadió una nueva config

        private int SwComponent_AddItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                updateCombosAndTables();
            }

            return 0;
        }

        #endregion

        #region 5- Se renombra una configuración
        private int SwComponent_RenameItemNotify(int EntityType, string oldName, string NewName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                swActiveDoc.renameConfigOnDatabase(oldName, NewName);
                refreshUI(true);
            }
            return 0;
        }
        #endregion

        #region 6- Archivo renombrado en SW
        //Se considera el fix cuando se reabre el archivo.

        #endregion

        #region 7- Se eliminó una config
        private int SwComponent_DeleteItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                swActiveDoc.deleteConfigurationFromDatabase(itemName);
                updateCombosAndTables();
            }
            return 0;
        }

        private int SwComponent_DeleteItemPreNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                //Necesito una manera de bloquear el comando en este pre notify si el usuario dice que no.
            }
            return 0;
        }

        #endregion

        #region 8- Cambio de configuración activa.

        private int SwComponent_ActiveConfigChangeNotify()
        {
            //Esto puede hacerse obsoleto. No tiene un uso hasta ahora.
            return 0;
        }
        #endregion

        #region 9 - Se guarda el archivo

        private int SwDraw_FileSaveNotify(string FileName)
        {
            //Escribir PartId al archivo
            swActiveDoc.writePartIdToFile();
            //Limpiar todo
            refreshUI();
            return 0;
        }
        private int SwComponent_FileSaveNotify(string FileName)
        {
            //Escribir PartId al archivo
            swActiveDoc.writePartIdToFile();
            //Escribir Partids a todas las configuraciones.
            swActiveDoc.writePartIdToAllConfigs();
            //Limpiar todo
            refreshUI(true);
            return 0;
        }
        #endregion

        #region Implementar la interfaz InotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
