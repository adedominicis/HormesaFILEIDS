using SolidWorks.Interop.sldworks;
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows;
using HormesaFILEIDS.model;


namespace HormesaFILEIDS.ViewModel
{

    public class viewModel : INotifyPropertyChanged
    {

        #region Private fields
        private SldWorks swApp;
        private string id;
        private int selectedConfigId;
        private DataTable dtResumenPartID;
        private PartDoc swPart;
        private AssemblyDoc swAssy;
        private DrawingDoc swDraw;
        private ModelDoc2 swModel;
        private SelectionMgr selMgr;
        private Feature swFeat;
        private UIHelper uiHelper;

        #endregion


        #region Public properties

        //Identificador de la configuración seleccionada en el combobox.
        public int SelectedConfigId
        {
            set { 
                //Los combobox de XAML cuentan desde cero, la BD cuenta desde 1 en los ID. Esto es ajustable a posteriori.
                selectedConfigId = value + 1;
                OnPropertyChanged("SelectedConfigId");
            }
        }

        public string Id
        {
            get
            {
                return "000-000";
            }
        }

        #endregion


        #region Inicializar

        public void initVM()
        {
            try
            {
                // Obtener Instancia activa de solidworks
                swApp = (SldWorks)System.Runtime.InteropServices.Marshal.GetActiveObject("SldWorks.Application");
                // Suscribirse a los eventos de la instancia de solidworks (swApp)
                SwAppEventSubscriber();
                //Instanciar algunos singletons importantes.
                uiHelper = new UIHelper();

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en inicializacion de viewModel: " + e.Message);
            }
        }

        #endregion

        #region TaskPane Methods and listeners.
        #endregion

        #region Solidworks Event Listeners

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

        #region 1,6 - Nuevo documento o cambio de documento activo
        /// <summary>
        /// Este bloque se encarga de actualizar la referencia a swModel cuando el usuario cambia de documento.
        /// Adicionalmente, crea el swDoc correspondiente, bien sea PartDoc, AssemblyDoc o DrawingDoc
        /// </summary>


        // Handlers (Delegate methods)
        private int SwApp_FileNewNotify2(object NewDoc, int DocType, string TemplateName)
        {
            //Actualizar swModel
            SwModel = (ModelDoc2)NewDoc;
            //Mensaje para debug.
            uiHelper.msgCreator(UIHelper.UserMessages.UserCreatedNewDocument);
            return 0;
        }
        private int SwApp_FileOpenNotify(string FileName)
        {
            //Actualizar swModel
            SwModel = (ModelDoc2)swApp.ActiveDoc;
            //Mensaje para debug.
            uiHelper.msgCreator(UIHelper.UserMessages.userOpenedDocument);
            return 0;
        }
        private int SwApp_ActiveDocChangeNotify()
        {
            //Actualizar swModel
            SwModel = (ModelDoc2)swApp.ActiveDoc;
            //Mensaje para debug.
            uiHelper.msgCreator(UIHelper.UserMessages.userChangedDocument);
            return 0;
        }
        //Actualizar el tipo de documento y castear el tipo al swDoc correcto
        public void updateSwDoc()
        {
            //Crear objetos swDoc
            if (SwModel.GetType() == (int)swDocumentTypes_e.swDocPART)
            {
                swPart = (PartDoc)SwModel;
            }
            else if (SwModel.GetType() == (int)swDocumentTypes_e.swDocASSEMBLY)
            {
                swAssy = (AssemblyDoc)SwModel;
            }
            else if (SwModel.GetType() == (int)swDocumentTypes_e.swDocDRAWING)
            {
                swDraw = (DrawingDoc)SwModel;
            }

            //Suscribirse a todos los listeners usando delegados.
            SwSpecificEventSubscriber();

        }
        #endregion

        #region 2- Se añadió una nueva config

        private int SwPart_AddItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                uiHelper.msgCreator(UIHelper.UserMessages.userAddedConfig);
            }

            return 0;
        }

        #endregion

        #region 3- Se renombra una configuración
        private int SwPart_RenameItemNotify(int EntityType, string oldName, string NewName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                uiHelper.msgCreator(UIHelper.UserMessages.userRenamedConfiguration);
            }
            return 0;
        }
        #endregion

        #region 4- Archivo renombrado fuera de SW (no implementado)
        #endregion

        #region 5- Se eliminó una config
        private int SwPart_DeleteItemNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                uiHelper.msgCreator(UIHelper.UserMessages.userDeletedConfig);
            }
            return 0;
        }

        private int SwPart_DeleteItemPreNotify(int EntityType, string itemName)
        {
            if ((int)swNotifyEntityType_e.swNotifyConfiguration == EntityType)
            {
                uiHelper.msgCreator(UIHelper.UserMessages.userAboutToDeleteConfig);
            }
            return 0;
        }
        #endregion

        #region 7- Se cambió el partid manualmente. (Esto no está funcionando)
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

        #region 8- Cambio de configuración activa.

        private int SwPart_ActiveConfigChangeNotify()
        {
            uiHelper.msgCreator(UIHelper.UserMessages.userSwitchedConfig);
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
