using System.ComponentModel;
using System.Windows.Controls;
using static AngelSix.SolidDna.SolidWorksEnvironment;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using SharpDX.XInput;
using System;
using HormesaFILEIDS.ViewModel;

namespace HormesaFILEIDS
{
    /// <summary>
    /// Interaction logic for MyAddinControl.xaml
    /// </summary>
    public partial class MyAddinControl : UserControl
    {
        //DATACONTEXT.
        private viewModel vm;


        #region Constructor


        public MyAddinControl()
        {

            //El datacontext es el viewmodel, el viewmodel se comunica con todas las otras clases 
            vm = new viewModel();
            this.DataContext = vm;
            vm.initVM();
            InitializeComponent();
        }

        #endregion


        #region Metodos accionados desde la interfaz.
        private void btNuevoPartID_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ha solicitado generar un partid");
        }

        #endregion


    }
}
