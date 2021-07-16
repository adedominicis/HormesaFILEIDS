using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace HormesaFILEIDS.model
{
    public class SwAttributeHandler
    {
        #region Privados.

        private SldWorks swApp;
        private ModelDoc2 swModel;
        private SolidWorks.Interop.sldworks.Attribute swAtt;
        private Parameter swParamPartid;
        private AttributeDef swAttDef;
        private ModelDocExtension swModelDocExt;
        private SelectionMgr swSelMgr;
        private Feature swFeat;
        private ErrorHandler err;
        #endregion

        #region Constantes
        private const string swAttDefUniqueName = "pubHORMESAFILEIDSAttDef";
        private const string addParameterName = "HORMESAPARTID";
        private const string attInstanceName = "HORMESAPARTID";
        #endregion

        #region Constructor.

        public SwAttributeHandler(SldWorks swapp, ModelDoc2 swmodel)
        {
            //Conectar app de solidworks y modelo activo.
            swApp = swapp;
            swModel = swmodel;
            swModelDocExt = (ModelDocExtension)swModel.Extension;
            swSelMgr = (SelectionMgr)swModel.SelectionManager;
            err = new ErrorHandler();

            //Inicialización: Inicializar el objeto AttDef, 
            //agregar y registrar un contenedor, instanciar ese contenedor en el feature manager tree
            initAttDef();
            addAndRegisterParameter();
            instanceParameter();

        }
        #endregion

        #region Métodos

        //Inicializar definición de atributo.
        private void initAttDef()
        {
            swAttDef = (AttributeDef)swApp.DefineAttribute(swAttDefUniqueName);
        }

        //Agregar y registrar parámetro
        private void addAndRegisterParameter()
        {
            swAttDef.AddParameter(addParameterName, (int)swParamType_e.swParamTypeString, 0.0, 0);
            swAttDef.Register();
        }

        //Agregar parámetro al contenedor de datos.
        private void instanceParameter()
        {
            //Verificar si el parametro ya existe en el feature manager.
            swModelDocExt.SelectByID2(attInstanceName, "ATTRIBUTE", 0, 0, 0, false, 0, null, 0);
            swFeat = (Feature)swSelMgr.GetSelectedObject6(1, -1);
            //Si el parámetro no existe en el arbol, hay que crearlo.
            if (swFeat == null)
            {
                swAtt = swAttDef.CreateInstance5(swModel, null, attInstanceName, 0, (int)swInConfigurationOpts_e.swAllConfiguration);
            }
            //Si existe, se trae del arbol.
            else
            {
                swAtt = getSwAttFromFeatureTree();
            }
        }

        //Asignar valor al parámetro.
        public void writePartIdOnAttribute(string id)
        {
            try
            {
                instanceParameter();
                //Obtener objeto del parametro.
                swParamPartid = (Parameter)swAtt.GetParameter(attInstanceName);
                //Asignarle el valor
                swParamPartid.SetStringValue2(id, (int)swInConfigurationOpts_e.swAllConfiguration, "");
            }
            catch (Exception ex)
            {
                err.thrower(err.handler(EnumMensajes.errorMiscelaneo, ex.Message));
            }

        }


        //Eliminar atributo de partid.
        private void deletePartId()
        {
            getSwAttFromFeatureTree().Delete(true);
        }

        private SolidWorks.Interop.sldworks.Attribute getSwAttFromFeatureTree()
        {
            swModelDocExt.SelectByID2(attInstanceName, "ATTRIBUTE", 0, 0, 0, false, 0, null, 0);
            swFeat = (Feature)swSelMgr.GetSelectedObject6(1, -1);
            return (SolidWorks.Interop.sldworks.Attribute)swFeat.GetSpecificFeature2();
        }
        #endregion

        #region Métodos públicos
        //Verificar si el partid existe. Si no existe y se coloca el parametro opcional createIt, puede crearlo y si es exitoso retorna true.
        public bool checkIfPartIdExists(string partId, bool createIt = false)
        {
            //Intentar seleccionar comparar el valor del partid con lo que existe.

            swAtt = getSwAttFromFeatureTree();
            swParamPartid = (Parameter)swAtt.GetParameter(attInstanceName);

            if (swParamPartid != null)
            {
                return string.Equals(swParamPartid.GetStringValue(), partId);
            }
            else
            {
                if (createIt)
                {
                    writePartIdOnAttribute(partId);
                    return checkIfPartIdExists(partId);
                }
                return false;
            }
        }

        //Obtener el partid desde el atributo.
        public string getPartIdFromAttribute()
        {
            swAtt = getSwAttFromFeatureTree();
            swParamPartid = (Parameter)swAtt.GetParameter(attInstanceName);
            return swParamPartid.GetStringValue();
        }
        #endregion


    }
}
