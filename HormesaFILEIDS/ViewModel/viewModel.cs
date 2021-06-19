using SolidWorks.Interop.sldworks;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;


namespace HormesaFILEIDS.ViewModel
{

    public class viewModel : INotifyPropertyChanged
    {

        #region private fields
        private SldWorks swApp;

        #endregion

        #region Inicializar

        public void initVM()
        {
            try
            {
                // Obtener Instancia activa de solidworks
                swApp = (SldWorks)System.Runtime.InteropServices.Marshal.GetActiveObject("SldWorks.Application");

                
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
