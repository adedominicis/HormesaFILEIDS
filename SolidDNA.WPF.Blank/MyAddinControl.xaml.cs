using HormesaFILEIDS.model;
using HormesaFILEIDS.ViewModel;
using System;
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
        private bool adminIsLogged = false;
        private DAO dao = new DAO();

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
            err = vm.err;
            //Is admin logged?
            adminIsLogged = false;
            //Llenar info del server.
            vm.refreshUI(true);
        }

        #endregion

        #region Metodos accionados desde la interfaz.

        /// <summary>
        /// Muestra ventana de log de errores.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLogErrores_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorLogWindow logWindow = new ErrorLogWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }
        private void btNuevoPartIdComponente_Click(object sender, RoutedEventArgs e)
        {
            vm.asignarPartid();
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
            if (dao.IsServerConnected())
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
            if (adminIsLogged)
            {
                if (vm.AuthHldr.updateServerIp(txServerData.Text))
                {
                    err.thrower(err.handler(EnumMensajes.ipActualizada, txServerData.Text));
                }
                else
                {
                    err.thrower(err.handler(EnumMensajes.errorEscribiendoArchivoServer, txServerData.Text));
                }
                toggleSettingsPane(1);
            }
            else
            {
                toggleSettingsPane(2);
            }
            vm.refreshUI(true);
            adminIsLogged = false;
        }
        private void btVerificarPass_Click(object sender, RoutedEventArgs e)
        {
            if (vm.AuthHldr.validateAdmin(pwbAppPass.Password))
            {
                //Flag de administrador logeado.
                adminIsLogged = true;
                toggleSettingsPane(3);
            }
            else
            {
                toggleSettingsPane(1);
                adminIsLogged = false;
                err.thrower(err.handler(EnumMensajes.passIncorrecto));
            }
            vm.refreshUI(true);
        }

        #endregion

        #region Metodos de estado e inicializacion
        /// <summary>
        /// Este metodo alterna los estados del tab "Ajustes"
        /// En el estado inicial se ve el control de IP como solo lectura. El botón "btActualizarServerName" está habilitado.
        /// En el segundo estado, se muestran controles para contraseña y se deshabilita el boton "btActualizarServerName"
        /// </summary>
        private void toggleSettingsPane(int estado)
        {
            switch (estado)
            {
                case 1:
                    //Ocultar caja de contraseña, borrar password
                    btLogin.Visibility = Visibility.Hidden;
                    pwbAppPass.Visibility = Visibility.Hidden;
                    lblPassword.Visibility = Visibility.Hidden;
                    pwbAppPass.Password = string.Empty;
                    // txServerData es read only
                    txServerData.IsReadOnly = true;
                    //El boton btActualizarServerName tiene texto por defecto y está habilitado
                    btActualizarServerName.Content = "Editar IP";
                    btActualizarServerName.IsEnabled = true;
                    break;
                case 2:
                    //Mostrar caja de contraseñas
                    pwbAppPass.Password = string.Empty;
                    btLogin.Visibility = Visibility.Visible;
                    pwbAppPass.Visibility = Visibility.Visible;
                    lblPassword.Visibility = Visibility.Visible;

                    //Habilitar boton de contraseña
                    btLogin.Visibility = Visibility.Visible;
                    btLogin.IsEnabled = true;

                    //Desahabilitar boton btActualizarServerName
                    btActualizarServerName.IsEnabled = false;

                    break;
                case 3:
                    //Habilitar escritura en txServerData, modificar texto del boton btActualizarServerName
                    txServerData.IsReadOnly = false;
                    btActualizarServerName.IsEnabled = true;
                    btActualizarServerName.Content = "Edite la IP y haga clic aqui para guardar...";
                    //Ocultar caja de contraseña, borrar password
                    btLogin.Visibility = Visibility.Hidden;
                    pwbAppPass.Visibility = Visibility.Hidden;
                    lblPassword.Visibility = Visibility.Hidden;
                    pwbAppPass.Password = string.Empty;
                    break;

                default:
                    break;
            }
        }


        #endregion


    }
}
