using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HormesaFILEIDS.model
{
    public class SwActiveDocument
    {
        #region Private fields

        private string partId;
        private ModelDoc2 swModel;
        private ObservableCollection<string> dbConfigList;
        private Configuration activeConfig;
        private DAO dao;
        private queryDump q;
        #endregion

        #region Constructor

        public SwActiveDocument(ModelDoc2 swmodel)
        {
            // Obtener el swModel.
            swModel = swmodel;
            //Modelo de acceso a datos.
            dao = new DAO();
            // Querydump
            q = new queryDump();

            //Si el archivo actual está en la DB, setear el partid interno de la clase para tener identificado el archivo.
            partId = dao.singleReturnQuery(q.getFilePartIdFromPath(swModel.GetPathName()));
        }

        #endregion

        #region Public Properties
        //Partid del componente.
        public string PartId
        {
            get { return partId; }
        }

        //La lista de configuraciones del modelo.
        public ObservableCollection<string> ConfigList
        {
            get { return (ObservableCollection<string>)swModel.GetConfigurationNames(); }
        }

        //La configuración activa.
        public string ActiveConfigName
        {

            get { return getActiveConfigName(); }
        }

        #endregion

        #region Methods

        //Obtener el partid estilizado.
        public string getFormattedPartId()
        {
            if (!string.IsNullOrEmpty(partId))
            {
                return partId.ToString().PadLeft(6, '0').Insert(3, "-");
            }
            return string.Empty;

        }
        //Obtener las configuraciones que hay guardadas en la DB con este partid.
        public DataTable getConfigListFromDB()
        {
            return dao.tableReturnQuery(q.listConfigsFromFilePartID(partId));
        }

        //Inserta archivo en la DB y le asigna partid.
        public bool insertFileOnDb()
        {
            //Insertar el archivo activo en la DB
            dao.singleReturnQuery(q.insertFileFromPath(swModel.GetPathName()));

            //Setear el partid.
            partId = dao.singleReturnQuery(q.getFilePartIdFromPath(swModel.GetPathName()));
            return !string.IsNullOrEmpty(partId);
        }
        private string getActiveConfigName()
        {
            activeConfig = (Configuration)swModel.GetActiveConfiguration();
            return activeConfig.Name;
        }
        #endregion
    }
}
