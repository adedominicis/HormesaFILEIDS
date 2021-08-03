using HormesaFILEIDS.model;
using HormesaFILEIDS.ViewModel;
using System.Windows;
using System.Windows.Controls;


namespace HormesaFILEIDS
{
    /// <summary>
    /// Interaction logic for MyAddinControl.xaml
    /// </summary>
    public partial class MyAddinControl : UserControl
    {
        //DATACONTEXT.
        private viewModel vm;
        private ErrorHandler err;


        #region Constructor


        public MyAddinControl()
        {

            //El datacontext es el viewmodel, el viewmodel se comunica con todas las otras clases 
            vm = new viewModel();
            this.DataContext = vm;
            vm.initializeViewModel(this);
            //Inicializar taskpane en solidworks
            InitializeComponent();
            //Error handler.
            err = new ErrorHandler();
        }

        #endregion

        #region Metodos accionados desde la interfaz.
        private void btNuevoPartIdComponente_Click(object sender, RoutedEventArgs e)
        {
            //Deshabilitar botón.
            btNuevoPartIdComponente.IsEnabled = !vm.asignarPartid();
        }

        private void btRenombrarArchivo_Click(object sender, RoutedEventArgs e)
        {

            vm.renombrarArchivo();
        }

        private void btAsignarPartidAConfig_Click(object sender, RoutedEventArgs e)
        {
            btAsignarPartidAConfig.IsEnabled = !vm.asignarPartIdAConfig();
        }

        private void cbConfiguraciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vm.refreshUI();
            //Si no hay partid en la configuración, botón habilitado.
            btAsignarPartidAConfig.IsEnabled = string.IsNullOrEmpty(txConfigPartid.Text);
        }

        //Probar la conexión al servidor
        private void btProbarConexion_Click(object sender, RoutedEventArgs e)
        {
            btProbarConexion.Content = "Conectando con el servidor...";
            if (vm.AuthHldr.Dao.IsServerConnected())
            {
                err.thrower(err.handler(EnumMensajes.conexionEstablecida, txServerData.Text));
            }
            else
            {
                err.thrower(err.handler(EnumMensajes.conexionFallida, txServerData.Text));
            }
            btProbarConexion.Content = "Probar conexión";


        }

        //Actualizar nombre del servidor
        private void btActualizarServerName_Click(object sender, RoutedEventArgs e)
        {
            if (pwbAppPass.IsVisible)
            {
                //Password visible.
                if (vm.AuthHldr.validateAdmin(pwbAppPass.Password))
                {
                    //Admin admitido
                    if (vm.AuthHldr.updateServerIp(txServerData.Text))
                    {
                        err.thrower(err.handler(EnumMensajes.ipActualizada, txServerData.Text));
                    }
                    else
                    {
                        err.thrower(err.handler(EnumMensajes.errorEscribiendoArchivoServer, txServerData.Text));
                    }
                    vm.refreshUI();
                }
                else
                {
                    err.thrower(err.handler(EnumMensajes.passIncorrecto));
                    
                }
                pwbAppPass.Visibility = Visibility.Hidden;
                btActualizarServerName.Content = "Editar IP del servidor";
            }
            else
            {
                //Password no es visible.
                pwbAppPass.Visibility = Visibility.Visible;
                btActualizarServerName.Content = "Guardar nueva IP del servidor";
            }

            pwbAppPass.Password = string.Empty;
        }

        #endregion

        #region Metodos de estado e inicializacion


        #endregion



    }
}
