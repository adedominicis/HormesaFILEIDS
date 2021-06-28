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
            vm.initVM();
            InitializeComponent();
            //Error handler.
            err = new ErrorHandler();

        }

        #endregion

        #region Metodos accionados desde la interfaz.
        private void btNuevoPartID_Click(object sender, RoutedEventArgs e)
        {
            //Insertar archivo en la BD.
            if (vm.SwActiveDoc.insertFileOnDb())
            {
                err.handler(EnumMensajes.registroExitoso);
            }
            else
            {
                err.handler(EnumMensajes.registroFallido);
            }

        }

        #endregion


    }
}
