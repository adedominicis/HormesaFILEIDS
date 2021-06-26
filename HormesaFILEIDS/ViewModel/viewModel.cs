﻿using SolidWorks.Interop.sldworks;
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
        private SwEventHandler swHandler;

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
                //Instanciar un listener de solidworks
                swHandler = new SwEventHandler(swApp);

            }
            catch (Exception e)
            {
                MessageBox.Show("Error en inicializacion de viewModel: " + e.Message);
            }
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