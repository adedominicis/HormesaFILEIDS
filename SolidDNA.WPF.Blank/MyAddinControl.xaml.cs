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
            //Resetear interfaz
            resetTaskPane();
        }

        #endregion

        #region Metodos accionados desde la interfaz.
        private void btNuevoPartIdComponente_Click(object sender, RoutedEventArgs e)
        {
            //Insertar archivo en la BD.
            if (vm.asignarPartid())
            {
                //Deshabilitar botón.
                btNuevoPartIdComponente.IsEnabled = false;
            }
        }

        private void btRenombrarArchivo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btAsignarPartidAConfig_Click(object sender, RoutedEventArgs e)
        {
            vm.asignarPartIdAConfig();
        }
        #endregion


        #region Metodos de estado e inicializacion


        public void fillComboBoxes()
        {
            cbConfiguraciones.ItemsSource = vm.LsConfigsActiveDoc;
        }

        public void resetTaskPane()
        {
            fillComboBoxes();
        }


        #endregion

    }
}
