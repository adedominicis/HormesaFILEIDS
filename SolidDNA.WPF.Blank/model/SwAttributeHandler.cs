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
        private int attInstanceInvisible = 1;
        #endregion

        #region Constructor.

        public SwAttributeHandler(SldWorks swapp, ModelDoc2 swmodel, bool showAttribute = false)
        {
            //Conectar app de solidworks y modelo activo.
            swApp = swapp;
            swModel = swmodel;
            swModelDocExt = (ModelDocExtension)swModel.Extension;
            swSelMgr = (SelectionMgr)swModel.SelectionManager;
            err = new ErrorHandler();
            attInstanceInvisible = Convert.ToInt32(!showAttribute);

        }

        #endregion

        #region Métodos privados

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

        private SolidWorks.Interop.sldworks.Attribute getSwAttFromFeatureTree()
        {
            swModelDocExt.SelectByID2(attInstanceName, "ATTRIBUTE", 0, 0, 0, false, 0, null, 0);
            swFeat = (Feature)swSelMgr.GetSelectedObject6(1, -1);
            if (swFeat != null)
            {
                return (SolidWorks.Interop.sldworks.Attribute)swFeat.GetSpecificFeature2();
            }
            return null;
        }

        //Formatear PARTID. Esto deberia ser un helper function ya que se pide en dos lugares.
        private string formatPartId(string partId)
        {
            if (!string.IsNullOrEmpty(partId))
            {
                return partId.ToString().PadLeft(6, '0').Insert(3, "-");
            }
            return string.Empty;  
        }

        #endregion

        #region Métodos públicos
        //Cambiar visibilidad del atributo en el property manager.
        public void toggleAttVisibility(bool isVisible)
        {
            try
            {
                swModelDocExt.SelectByID2(attInstanceName, "ATTRIBUTE", 0, 0, 0, false, 0, null, 0);
                swFeat = (Feature)swSelMgr.GetSelectedObject6(1, -1);
                if (swFeat != null)
                {
                    swFeat.SetUIState((int)swUIStates_e.swIsHiddenInFeatureMgr, !isVisible);
                    swModel.EditRebuild3();
                    if (isVisible)
                    {
                        //Mostrar el partid almacenado internamente.
                        err.thrower("El PARTID almacenado es: " + formatPartId(getPartIdFromAttribute()));
                    }

                }
                else if(isVisible)
                {
                    err.thrower("El atributo no existe!");
                    
                }
            }
            catch (Exception ex)
            {
                err.handler(EnumMensajes.excepcionInterna, "Error al mostrar u ocultar atributo", ex);
            }

        }
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
                }
                else
                {
                    err.thrower(err.handler(EnumMensajes.errorMiscelaneo, "Falló SwAttributeHandler::writePartIdOnAttribute"));
                }

            }
            catch (Exception ex)
            {
                err.thrower(err.handler(EnumMensajes.errorMiscelaneo, ex.Message,ex));
            }

        }

        //Obtener el partid desde el atributo.
        public string getPartIdFromAttribute()
        {
            swAtt = getSwAttFromFeatureTree();
            if (swAtt != null)
            {
                swParamPartid = (Parameter)swAtt.GetParameter(attInstanceName);
                return swParamPartid.GetStringValue();
            }
            return string.Empty;
        }

        //Eliminar atributo de partid.
        public void deletePartIdAttribute()
        {
            SolidWorks.Interop.sldworks.Attribute swAtt = getSwAttFromFeatureTree();
            if (swAtt != null)
            {
                getSwAttFromFeatureTree().Delete(true);
            }

        }
        #endregion


    }
}
