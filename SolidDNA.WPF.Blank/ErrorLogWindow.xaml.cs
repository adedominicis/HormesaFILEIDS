using HormesaFILEIDS.model;
using HormesaFILEIDS.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HormesaFILEIDS
{
    /// <summary>
    /// Ventana de errores.
    /// </summary>
    public partial class ErrorLogWindow : Window
    {
        private viewModel vm;
        private ErrorHandler err;

        #region Constructor
        public ErrorLogWindow()
        {
            //El datacontext es el viewmodel, el viewmodel se comunica con todas las otras clases 
            vm = new viewModel();
            this.DataContext = vm;
            vm.loadErrorLogWindowData(this);
            //Inicializar taskpane en solidworks
            InitializeComponent();
            
        }
        #endregion

        #region Métodos públicos
        internal void refreshData()
        {
            vm.loadErrorLogWindowData(this);

        }
        #endregion


    }
}
