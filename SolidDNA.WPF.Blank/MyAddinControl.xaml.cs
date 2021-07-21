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
            vm.updateTextBoxes();
            //Si no hay partid en la configuración, botón habilitado.
            btAsignarPartidAConfig.IsEnabled = string.IsNullOrEmpty(txConfigPartid.Text);
        }
        #endregion


        #region Metodos de estado e inicializacion


        #endregion


    }
}
