using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HormesaFILEIDS.model
{
    public class UIHelper
    {
        public enum UserMessages
        {
            userChangedDocument,
            userClosedDocument,
            userAddedConfig,
            userDeletedConfig,
            userRenamedFile,
            userRenamedChildFile,
            userModifiedPartid,
            userRenamedFileOutsidePremises,
            userSwitchedConfig,
            userOpenedDocument,
            UserCreatedNewDocument,
            userAboutToDeleteConfig,
            userRenamedConfiguration,
            userDeletedPartid
        }

        public void msgCreator(Enum msg, string msgTail = "")
        {
            switch (msg)
            {
                case UserMessages.userDeletedPartid:
                    MessageBox.Show("Ha borrado la propiedad PARTID");
                    break;
                case UserMessages.userRenamedConfiguration:
                    MessageBox.Show("Se renombró una configuración");
                    break;
                case UserMessages.userOpenedDocument:
                    MessageBox.Show("Se abrió un archivo");
                    break;
                case UserMessages.userAboutToDeleteConfig:
                    MessageBox.Show("Va a borrar una configuración");
                    break;
                case UserMessages.userChangedDocument:
                    MessageBox.Show("Cambio de archivo");
                    break;
                case UserMessages.userAddedConfig:
                    MessageBox.Show("Se agregó configuración");
                    break;
                case UserMessages.userDeletedConfig:
                    MessageBox.Show("Se eliminó configuración");
                    break;
                case UserMessages.userRenamedFile:
                    MessageBox.Show("Se renombró este archivo");
                    break;
                case UserMessages.userModifiedPartid:
                    MessageBox.Show("Se modificó manualmente el PARTID");
                    break;
                case UserMessages.userRenamedFileOutsidePremises:
                    MessageBox.Show("El archivo ha cambiado su nombre externamente");
                    break;
                case UserMessages.userSwitchedConfig:
                    MessageBox.Show("Ha cambiado la configuración activa");
                    break;
                case UserMessages.UserCreatedNewDocument:
                    MessageBox.Show("Se ha creado un archivo");
                    break;
                default:
                    break;

            }
        }
    }
}
