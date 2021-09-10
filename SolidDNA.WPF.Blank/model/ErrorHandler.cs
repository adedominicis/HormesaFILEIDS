using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Forms;

namespace HormesaFILEIDS.model
{
    internal class ErrorHandler
    {
        DAO dao = new DAO();
        //Acceso a datos para logging
        queryDump q = new queryDump();


        /// <summary>
        /// Muestra mensajes de error en pantalla.
        /// </summary>
        /// <param name="msg"></param>
        public void thrower(string msg)
        {
            string version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            System.Windows.Forms.MessageBox.Show(msg, "HORMESA FILEIDS v"+version);
        }

        /// <summary>
        /// Dialogo si o no
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>true si el usuario marca si, false de lo contrario</returns>
        public bool yesNoThrower(string msg)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(msg, "HORMESA FILEIDS", MessageBoxButtons.YesNo);
            return dialogResult == DialogResult.Yes;
        }

        /// <summary>
        /// Genera un mensaje de error desde el enum ErrorHandler
        /// </summary>
        /// <param name="err">EnumMensajes codigos de mensaje de error</param>
        /// <param name="tail">Mensaje adicional o suplementario</param>
        /// <param name="ex">Excepción de origen</param>
        /// <returns></returns>
        public string handler(EnumMensajes err, string tail = "", Exception ex = null)
        {
            string errorMsg;

            switch (err)
            {
                case EnumMensajes.excepcionInterna:
                    errorMsg = string.Format("Excepcion interna en: {0}", tail);
                    break;
                case EnumMensajes.errorSQL:
                    // Error en una consulta sql
                    errorMsg = string.Format("Error en consulta SQL \\{0}", tail);
                    break;
                case EnumMensajes.registroExitoso:
                    // Mostrar mensaje de error 
                    errorMsg = "Registro añadido a la base de datos";
                    break;
                case EnumMensajes.errorEnConexionDB:
                    // Error en la conexión a la DB
                    errorMsg = string.Format("No es posible conectar a la base de datos \\{0}", tail);
                    break;
                case EnumMensajes.metodoNoImplementado:
                    // metodo por implementar.
                    errorMsg = string.Format("El método {0} no ha sido implementado.", tail);
                    break;
                case EnumMensajes.errorMiscelaneo:
                    // Error miscelaneo
                    errorMsg = string.Format("Errror desconocido: {0}", tail);
                    break;
                case EnumMensajes.notificarUsuario:
                    // Notificar usuario, miscelaneo.
                    errorMsg = string.Format("Mensaje: {0}", tail);
                    break;
                case EnumMensajes.ipActualizada:
                    // IP del servidor de base de datos actualizada en archivo server.
                    errorMsg = string.Format("La IP del servidor de BD ha sido actualizada. {0}", tail);
                    break;
                case EnumMensajes.conexionFallida:
                    // Conexión a base de datos fallida.
                    errorMsg = string.Format("No es posible conectar a la base de datos alojada en la IP: \\{0}\nVerifique que la IP y el puerto SQL sean correctos.\nEl puerto por defecto es el 1433.", tail);
                    break;
                case EnumMensajes.conexionEstablecida:
                    // Conexión a base de datos establecida.
                    errorMsg = string.Format("Conexión establecida con: \\{0}", tail);
                    break;
                case EnumMensajes.passIncorrecto:
                    // Clave de administrador incorrecta.
                    errorMsg = "Clave incorrecta: Admin";
                    break;
                case EnumMensajes.errorEscribiendoArchivoServer:
                    // Error al escribir el archivo
                    errorMsg = string.Format("Error al intentar escribir el archivo {0}\nRevise los privilegios de escritura o inicie SolidWorks como Administrador", tail);
                    break;
                case EnumMensajes.yaTienePartid:
                    // Error al escribir el archivo
                    errorMsg = string.Format("Este archivo ya ha sido registrado bajo el PARTID: {0}\n¿Desea crear una nueva copia limpia sin numero asignado?", tail);
                    break;
                case EnumMensajes.noHayDocumentoActivo:
                    // Error al escribir el archivo
                    errorMsg = string.Format("No hay un documento activo.");
                    break;
                case EnumMensajes.archivoNoGuardado:
                    // El archivo no se ha guardado
                    errorMsg = string.Format("No puede asignar un PARTID a un archivo no guardado.\nGuarde el archivo e intente de nuevo");
                    break;
                case EnumMensajes.atributoVacio:
                    // El archivo no se ha guardado
                    errorMsg = string.Format("El PARTID almacenado se encuentra vacío.\n¿Desea actualizar desde la base de datos?");
                    break;

                default:
                    errorMsg = "Error desconocido";
                    break;
            }
            return errorMsg;

        }

    }
}