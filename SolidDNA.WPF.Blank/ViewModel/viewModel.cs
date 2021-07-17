﻿using HormesaFILEIDS.model;
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

    public class viewModel : INotifyPropertyChanged
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
        #endregion

        #region Public properties

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

        //Tabla de configuraciones y partids (GET)
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
                //Inicializar elementos

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en inicializacion de viewModel: " + e.Message);
            }
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
            myView.lblConfiguracion.IsEnabled= notDrawing;
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
        public void updateTextBoxes()
        {
            OnPropertyChanged("PartId");
            OnPropertyChanged("ConfigPartId");
        }
        private void initComboboxes()
        {
            //Seleccionar primero.
            myView.cbConfiguraciones.SelectedIndex = 0;

        }
        private void updateButtons()
        {
            myView.btNuevoPartIdComponente.IsEnabled = string.IsNullOrEmpty(PartId);
        }
        private void refreshUI()
        {
            toggleUIMode();
            updateCombosAndTables();
            updateTextBoxes();
            initComboboxes();
            updateButtons();
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
            if (swModel != null && swActiveDoc != null)
            {
                if (swActiveDoc.insertFileOnDb())
                {
                    OnPropertyChanged("PartId");
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

        public void renombrarArchivo()
        {
            //Renombrar archivo, no implementado.
            swActiveDoc.renameFile();

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
            swApp.FileCloseNotify += SwApp_FileCloseNotify;

            //Suscribir en limpio
            swApp.ActiveDocChangeNotify += SwApp_ActiveDocChangeNotify;
            swApp.FileOpenNotify += SwApp_FileOpenNotify;
            swApp.FileNewNotify2 += SwApp_FileNewNotify2;
            swApp.FileCloseNotify += SwApp_FileCloseNotify;

        }

        //Suscripción a los eventos de parte, pieza o ensamble mediante delegates (swModel)
        public void SwModelEventSubscriber()
        {
            //Crear objetos swDoc
            if (swModel.GetType() == (int)swDocumentTypes_e.swDocPART)
            {
                swPart = (PartDoc)swModel;
            }
            else if (swModel.GetType() == (int)swDocumentTypes_e.swDocASSEMBLY)
            {
                swAssy = (AssemblyDoc)swModel;
            }
            else if (swModel.GetType() == (int)swDocumentTypes_e.swDocDRAWING)
            {
                swDraw = (DrawingDoc)swModel;
            }
            //Eventos de seleccion en partes
            if (swPart != null)
            {
                //Cambio de configuracion activa
                swPart.ActiveConfigChangePostNotify -= SwPart_ActiveConfigChangeNotify;
                swPart.ActiveConfigChangePostNotify += SwPart_ActiveConfigChangeNotify;

                //Agregada configuracion
                swPart.AddItemNotify -= SwPart_AddItemNotify;
                swPart.AddItemNotify += SwPart_AddItemNotify;

                //Antes de eliminar la configuración
                swPart.DeleteItemPreNotify -= SwPart_DeleteItemPreNotify;
                swPart.DeleteItemPreNotify += SwPart_DeleteItemPreNotify;

                //Eliminada configuracion
                swPart.DeleteItemNotify -= SwPart_DeleteItemNotify;
                swPart.DeleteItemNotify += SwPart_DeleteItemNotify;

                //Renombrada configuración
                swPart.RenameItemNotify -= SwPart_RenameItemNotify;
                swPart.RenameItemNotify += SwPart_RenameItemNotify;

                //Eliminada una custom property
                swPart.DeleteCustomPropertyNotify -= SwPart_DeleteCustomPropertyNotify;
                swPart.DeleteCustomPropertyNotify += SwPart_DeleteCustomPropertyNotify;

                //Cambio de un custom property
                swPart.ChangeCustomPropertyNotify -= SwPart_ChangeCustomPropertyNotify;
                swPart.ChangeCustomPropertyNotify += SwPart_ChangeCustomPropertyNotify;
            }
            //Eventos de seleccion en ensamblajes
            if (swAssy != null)
            {
                //Cambio de configuracion activa
                swAssy.ActiveConfigChangePostNotify -= SwPart_ActiveConfigChangeNotify;
                swAssy.ActiveConfigChangePostNotify += SwPart_ActiveConfigChangeNotify;

                //Agregada configuracion
                swAssy.AddItemNotify -= SwPart_AddItemNotify;
                swAssy.AddItemNotify += SwPart_AddItemNotify;

                //Antes de eliminar la configuración
                swAssy.DeleteItemPreNotify -= SwPart_DeleteItemPreNotify;
                swAssy.DeleteItemPreNotify += SwPart_DeleteItemPreNotify;

                //Eliminada configuracion
                swAssy.DeleteItemNotify -= SwPart_DeleteItemNotify;
                swAssy.DeleteItemNotify += SwPart_DeleteItemNotify;

                //Renombrada configuración
                swAssy.RenameItemNotify -= SwPart_RenameItemNotify;
                swAssy.RenameItemNotify += SwPart_RenameItemNotify;

                //Eliminada una custom property
                swAssy.DeleteCustomPropertyNotify -= SwPart_DeleteCustomPropertyNotify;
                swAssy.DeleteCustomPropertyNotify += SwPart_DeleteCustomPropertyNotify;

                //Cambio de un custom property
                swAssy.ChangeCustomPropertyNotify -= SwPart_ChangeCustomPropertyNotify;
                swAssy.ChangeCustomPropertyNotify += SwPart_ChangeCustomPropertyNotify;
            }
            //Eventos de seleccion en dibujos
            if (swDraw != null)
            {


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
            swActiveDoc = new SwActiveDocument(swModel, swApp);
            //Actualizar taskpane
            refreshUI();
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
            swActiveDoc = new SwActiveDocument(swModel, swApp);
            //Revisar integridad de los datos entre el modelo y la BD.
            swActiveDoc.fixPathIntegrity();
            //Actualizar taskpane
            refreshUI();
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
            swActiveDoc = new SwActiveDocument(swModel, swApp);
            //Actualizar taskpane
            refreshUI();
            return 0;
        }
        #endregion

        #region 4- Se añadió una nueva config

        private int SwPart_AddItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                updateCombosAndTables();
            }

            return 0;
        }

        #endregion

        #region 5- Se renombra una configuración
        private int SwPart_RenameItemNotify(int EntityType, string oldName, string NewName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                swActiveDoc.renameConfigOnDatabase(oldName, NewName);
                refreshUI();
            }
            return 0;
        }
        #endregion

        #region 6- Archivo renombrado en SW
        //Se considera el fix cuando se reabre el archivo.

        #endregion

        #region 7- Se eliminó una config
        private int SwPart_DeleteItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                swActiveDoc.deleteConfigurationFromDatabase(itemName);
                updateCombosAndTables();
            }
            return 0;
        }

        private int SwPart_DeleteItemPreNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                //Necesito una manera de bloquear el comando en este pre notify si el usuario dice que no.
            }
            return 0;
        }

        #endregion

        #region 8- Se cambió el partid manualmente. (Esto no está funcionando)
        private int SwPart_DeleteCustomPropertyNotify(string propName, string Configuration, string Value, int valueType)
        {
            if (propName.Equals("partid", StringComparison.OrdinalIgnoreCase))
            {
                //Esto funciona erráticamente. El mensaje sale al abrir un archivo.
            }
            else
            {
                MessageBox.Show("Borró otra propiedad.");
            }
            return 0;
        }

        private int SwPart_ChangeCustomPropertyNotify(string propName, string Configuration, string oldValue, string NewValue, int valueType)
        {
            if (propName.Equals("partid", StringComparison.OrdinalIgnoreCase))
            {
                //Esto funciona erráticamente. El mensaje sale al abrir un archivo.
            }
            else
            {
                MessageBox.Show("Modifió otra propiedad.");
            }
            return 0;
        }


        #endregion

        #region 9- Cambio de configuración activa.

        private int SwPart_ActiveConfigChangeNotify()
        {
            //Esto puede hacerse obsoleto. No tiene un uso hasta ahora.
            return 0;
        }
        #endregion

        #region 10 - Se cierra el archivo.
        private int SwApp_FileCloseNotify(string FileName, int reason)
        {
            //Escribir PartId al archivo
            swActiveDoc.writePartIdToFile();
            //Escribir Partids a todas las configuraciones.
            swActiveDoc.writePartIdToAllConfigs();
            //Limpiar todo
            refreshUI();
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
