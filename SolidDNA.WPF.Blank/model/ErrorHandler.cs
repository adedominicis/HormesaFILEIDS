using System.Windows;

namespace HormesaFILEIDS.model
{
    public class ErrorHandler
    {

        //public ErrorHandler()
        //{
        //    //Suscribirse al evento de excepciones del DAO
        //    DAO dao = new DAO();
        //    dao.exceptionRaised += Dao_exceptionRaised;
        //}

        //private void Dao_exceptionRaised(object sender, string e)
        //{
        //    DAO dao = (DAO)sender;
        //    //Escribe el mensaje en la base de datos.
        //    dao.logException(e);

        //}

        public void thrower(string msg)
        {
            MessageBox.Show(msg);
        }


        public void sqlThrower(string connection, string query, string method, string exmsg)
        {
            thrower(string.Format("Ha ocurrido un error de base de datos\n" +
                "Conexión: {0}\nConsulta: {1}\nCaller: {2}\nExcepción: {3}", connection, query, method, exmsg));
        }
        public string handler(EnumMensajes err, string tail = "")
        {
            string errorMsg;

            switch (err)
            {
                case EnumMensajes.excepcionInterna:
                    errorMsg =string.Format("Excepcion interna en: {0}",tail);
                    break;
                case EnumMensajes.loginError:
                    errorMsg = "Usuario o contraseña invalidos";
                    break;
                case EnumMensajes.camposRequeridos:
                    errorMsg = "Debe llenar todos los campos";
                    break;
                case EnumMensajes.dataError:
                    // Mostrar mensaje de error 
                    errorMsg = "Datos inválidos";
                    break;
                case EnumMensajes.registroExiste:
                    // Mostrar mensaje de error 
                    errorMsg = "Este registro ya existe";
                    break;
                case EnumMensajes.registroNoExiste:
                    // Mostrar mensaje de error 
                    errorMsg = "Este registro no existe";
                    break;
                case EnumMensajes.errorSQL:
                    // Mostrar mensaje de error 
                    errorMsg = string.Format("Error en consulta SQL \\{0}", tail);
                    break;
                case EnumMensajes.formularioIncompleto:
                    // Mostrar mensaje de error 
                    errorMsg = "Formulario incompleto o con campos erroneos";
                    break;
                case EnumMensajes.registroExitoso:
                    // Mostrar mensaje de error 
                    errorMsg = "Registro añadido a la base de datos";
                    break;
                case EnumMensajes.registroFallido:
                    // Mostrar mensaje de error 
                    errorMsg = "Error al registrar, ya existe";
                    break;
                case EnumMensajes.registroModificado:
                    // Mostrar mensaje de error 
                    errorMsg = "Registro modificado con éxito";
                    break;
                case EnumMensajes.registroEliminado:
                    // Mostrar mensaje de error 
                    errorMsg = "Registro eliminado con éxito";
                    break;
                case EnumMensajes.errorSubirArchivo:
                    // Mostrar mensaje de error 
                    errorMsg = "Error al subir el archivo";
                    break;
                case EnumMensajes.errorBorrarArchivo:
                    // Mostrar mensaje de error 
                    errorMsg = "El campo no pudo ser eliminado";
                    break;
                case EnumMensajes.errorEnConexionDB:
                    // Mostrar mensaje de error 
                    errorMsg = string.Format("No es posible conectar a la base de datos \\{0}", tail);
                    break;
                case EnumMensajes.metodoNoImplementado:
                    // Mostrar mensaje de error 
                    errorMsg = string.Format("El método {0} no ha sido implementado.", tail);
                    break;
                case EnumMensajes.errorMiscelaneo:
                    // Mostrar mensaje de error 
                    errorMsg = string.Format("Errror desconocido: {0}", tail);
                    break;
                case EnumMensajes.notificarUsuario:
                    // Mostrar mensaje de error 
                    errorMsg = string.Format("Mensaje: {0}", tail);
                    break;
                default:
                    errorMsg = "Error desconocido";
                    break;
            }
            return errorMsg;

        }

    }
}