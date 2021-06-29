using SolidWorks.Interop.sldworks;
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using HormesaFILEIDS.model;
using SolidWorks.Interop.swconst;
using System.Collections.ObjectModel;

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
        private SelectionMgr selMgr;
        private Feature swFeat;
        #endregion

        #region Other private fields
        private int selectedConfigId;
        private UIHelper uiHelper;
        private SwActiveDocument swActiveDoc;
        //Instancia de la vista.
        private MyAddinControl myView;
        #endregion


        #region Public properties

        //Identificador de la configuración seleccionada en el combobox.
        public int SelectedConfigId
        {
            set
            {
                //Los combobox de XAML cuentan desde cero, la BD cuenta desde 1 en los ID. Esto es ajustable a posteriori.
                selectedConfigId = value + 1;
                OnPropertyChanged("SelectedConfigId");
            }
        }

        public string PartId
        {
            get 
            {
                if (swActiveDoc != null)
                {
                    return swActiveDoc.getFormattedPartId();
                }
                else
                {
                    return string.Empty;
                }
            }
        }



        //Lista de configuraciones del documento activo.
        public ObservableCollection<string> LsConfigsActiveDoc
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

        //Documento activo.
        public SwActiveDocument SwActiveDoc
        {
            get { return swActiveDoc; }
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
            }
            catch (Exception e)
            {
                MessageBox.Show("Error en inicializacion de viewModel: " + e.Message);
            }
        }

        private void refreshTaskPane()
        {
            //Fire onpropertychanged
            OnPropertyChanged("PartId");
            OnPropertyChanged("LsConfigsActiveDoc");
            //Actualizar vista
            myView.fillComboBoxes();
        }

        #endregion

        #region TaskPane Methods and listeners.
        //Los botones accionados en la vista llaman métodos aqui.
        //Esta suerte de "daisychaining" de métodos puede tener una mejor solución.
        //¿O es una forma de mediador?

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
            //Renombrar archivo;
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
        
        //crea el swDoc correspondiente, bien sea PartDoc, AssemblyDoc o DrawingDoc
        public void updateSwDoc()
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

            //Suscribirse a todos los listeners usando delegados.
            SwSpecificEventSubscriber();

        }

        //Suscripción a los eventos de parte, pieza o ensamble mediante delegates
        public void SwSpecificEventSubscriber()
        {
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
            swModel = (ModelDoc2)NewDoc;
            //Instanciar documento activo.
            swActiveDoc = null;
            swActiveDoc = new SwActiveDocument(swModel);
            //Actualizar taskpane
            refreshTaskPane();
            return 0;
        }
        #endregion

        #region 2- Abrir documento
        private int SwApp_FileOpenNotify(string FileName)
        {
            //Actualizar swModel
            swModel = (ModelDoc2)swApp.ActiveDoc;

            //Instanciar documento activo.
            swActiveDoc = null;
            swActiveDoc = new SwActiveDocument(swModel);
            //Actualizar taskpane
            refreshTaskPane();
            return 0;
        }
        #endregion

        #region 3- Cambiar documento activo
        private int SwApp_ActiveDocChangeNotify()
        {
            //Actualizar swModel
            swModel = (ModelDoc2)swApp.ActiveDoc;
            //Instanciar documento activo.
            swActiveDoc = null;
            swActiveDoc = new SwActiveDocument(swModel);
            //Actualizar taskpane
            refreshTaskPane();
            return 0;
        }
        #endregion

        #region 4- Se añadió una nueva config

        private int SwPart_AddItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                refreshTaskPane();
            }

            return 0;
        }

        #endregion

        #region 5- Se renombra una configuración
        private int SwPart_RenameItemNotify(int EntityType, string oldName, string NewName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                refreshTaskPane();
            }
            return 0;
        }
        #endregion

        #region 6- Archivo renombrado fuera de SW (no implementado)
        #endregion

        #region 7- Se eliminó una config
        private int SwPart_DeleteItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                refreshTaskPane();
            }
            return 0;
        }

        private int SwPart_DeleteItemPreNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                refreshTaskPane();
            }
            return 0;
        }
        #endregion

        #region 8- Se cambió el partid manualmente. (Esto no está funcionando)
        private int SwPart_DeleteCustomPropertyNotify(string propName, string Configuration, string Value, int valueType)
        {
            if (propName.Equals("partid", StringComparison.OrdinalIgnoreCase))
            {
                uiHelper.msgCreator(UIHelper.UserMessages.userDeletedPartid);
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
                uiHelper.msgCreator(UIHelper.UserMessages.userDeletedPartid);
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
            uiHelper.msgCreator(UIHelper.UserMessages.userSwitchedConfig);
            return 0;
        }
        #endregion

        #region 10 - Se cierra el archivo.
        private int SwApp_FileCloseNotify(string FileName, int reason)
        {
            swActiveDoc.writePartIdToFile();
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
