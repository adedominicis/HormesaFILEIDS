using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace HormesaFILEIDS.model
{
    internal class SwAttributeHandler
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
        private const int attInstanceInvisible = 1;
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
                swAtt = swAttDef.CreateInstance5(swModel, null, attInstanceName, attInstanceInvisible, (int)swInConfigurationOpts_e.swAllConfiguration);
            }
            //Si existe, se trae del arbol.
            else
            {
                swAtt = getSwAttFromFeatureTree();
            }
        }

        //Eliminar atributo de partid.
        private void deletePartId()
        {
            SolidWorks.Interop.sldworks.Attribute swAtt = getSwAttFromFeatureTree();
            if (swAtt!=null)
            {
                getSwAttFromFeatureTree().Delete(true);
            }
            
        }

        
        private SolidWorks.Interop.sldworks.Attribute getSwAttFromFeatureTree()
        {
            swModelDocExt.SelectByID2(attInstanceName, "ATTRIBUTE", 0, 0, 0, false, 0, null, 0);
            swFeat = (Feature)swSelMgr.GetSelectedObject6(1, -1);
            if (swFeat!=null)
            {
                return (SolidWorks.Interop.sldworks.Attribute)swFeat.GetSpecificFeature2();
            }
            return null;
        }
        #endregion

        #region Métodos públicos
        //Asignar valor al parámetro.
        public void writePartIdOnAttribute(string id)
        {
            try
            {
                //Inicializar inserción de atributos.
                initAttDef();
                addAndRegisterParameter();
                instanceParameter();
                //Reconstruir y guardar.
                swModel.ForceRebuild3(false);
                swModel.Save();
                //Verificar si el atributo existe.
                if (swAtt != null)
                {
                    //Obtener objeto del parametro.
                    swParamPartid = (Parameter)swAtt.GetParameter(attInstanceName);
                    //Asignarle el valor
                    swParamPartid.SetStringValue2(id, (int)swInConfigurationOpts_e.swAllConfiguration, "");
                    //Notificar
                    err.thrower(err.handler(EnumMensajes.registroExitoso,"PARTID: "+getPartIdFromAttribute()));
                }
                else
                {
                    err.thrower(err.handler(EnumMensajes.errorMiscelaneo, "Falló SwAttributeHandler::writePartIdOnAttribute"));
                }

            }
            catch (Exception ex)
            {
                err.thrower(err.handler(EnumMensajes.errorMiscelaneo, ex.Message));
            }

        }

        //Obtener el partid desde el atributo.
        public string getPartIdFromAttribute()
        {
            swAtt = getSwAttFromFeatureTree();
            if (swAtt!=null)
            {
                swParamPartid = (Parameter)swAtt.GetParameter(attInstanceName);
                return swParamPartid.GetStringValue();
            }
            return string.Empty;
        }
        #endregion


    }
}
