using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Forms;
using Serilog;

namespace HormesaFILEIDS.model
{
    internal class ErrorHandler
    {
        /// <summary>
        /// Muestra mensajes de error en pantalla.
        /// </summary>
        /// <param name="msg"></param>
        public void thrower(string msg)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            System.Windows.Forms.MessageBox.Show(msg, "HORMESA FILEIDS v"+version);
            logger(msg);
        }
        /// <summary>
        /// Se usa para logear usando Serilog. No muestra mensajes de error al usuario
        /// </summary>
        /// <param name="msg"></param>
        public void logger(string msg)
        {
            Log.Information(msg);
        }

        /// <summary>
        /// Dialogo si o no
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>true si el usuario marca si, false de lo contrario</returns>
        public bool yesNoThrower(string msg)
        {
            Log.Information(msg);
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
            string sysMsg;

            switch (err)
            {
                case EnumMensajes.excepcionInterna:
                    sysMsg = string.Format("Excepcion interna en: {0}", tail);
                    break;
                case EnumMensajes.errorSQL:
                    // Error en una consulta sql
                    sysMsg = string.Format("Error en consulta SQL \\{0}", tail);
                    break;
                case EnumMensajes.registroExitoso:
                    // Mostrar mensaje de error 
                    sysMsg = string.Format("Registro {0} añadido a la base de datos",tail);
                    break;
                case EnumMensajes.errorEnConexionDB:
                    // Error en la conexión a la DB
                    sysMsg = string.Format("No es posible conectar a la base de datos");
                    break;
                case EnumMensajes.metodoNoImplementado:
                    // metodo por implementar.
                    sysMsg = string.Format("El método {0} no ha sido implementado.", tail);
                    break;
                case EnumMensajes.errorMiscelaneo:
                    // Error miscelaneo
                    sysMsg = string.Format("Errror desconocido: {0}", tail);
                    break;
                case EnumMensajes.notificarUsuario:
                    // Notificar usuario, miscelaneo.
                    sysMsg = string.Format("Mensaje: {0}", tail);
                    break;
                case EnumMensajes.ipActualizada:
                    // IP del servidor de base de datos actualizada en archivo server.
                    sysMsg = string.Format("La IP del servidor de BD ha sido actualizada. {0}", tail);
                    break;
                case EnumMensajes.conexionFallida:
                    // Conexión a base de datos fallida.
                    sysMsg = string.Format("No es posible conectar a la base de datos alojada en la IP: \\{0}\nVerifique que la IP y el puerto SQL sean correctos.\nEl puerto por defecto es el 1433.", tail);
                    break;
                case EnumMensajes.conexionEstablecida:
                    // Conexión a base de datos establecida.
                    sysMsg = string.Format("Conexión establecida con: \\{0}", tail);
                    break;
                case EnumMensajes.passIncorrecto:
                    // Clave de administrador incorrecta.
                    sysMsg = "Clave incorrecta: Admin";
                    break;
                case EnumMensajes.errorEscribiendoArchivoServer:
                    // Error al escribir el archivo
                    sysMsg = string.Format("Error al intentar escribir el archivo {0}\nRevise los privilegios de escritura o inicie SolidWorks como Administrador", tail);
                    break;
                case EnumMensajes.yaTienePartid:
                    // Error al escribir el archivo
                    sysMsg = string.Format("Este archivo ya ha sido registrado bajo el PARTID: {0}\n¿Desea crear una nueva copia limpia sin numero asignado?", tail);
                    break;
                case EnumMensajes.noHayDocumentoActivo:
                    // Error al escribir el archivo
                    sysMsg = string.Format("No hay un documento activo.");
                    break;
                case EnumMensajes.archivoNoGuardado:
                    // El archivo no se ha guardado
                    sysMsg = string.Format("No puede asignar un PARTID a un archivo no guardado.\nGuarde el archivo e intente de nuevo");
                    break;
                case EnumMensajes.atributoVacio:
                    // El archivo no se ha guardado
                    sysMsg = string.Format("El PARTID almacenado se encuentra vacío.\n¿Desea actualizar desde la base de datos?");
                    break;
                case EnumMensajes.logSilencioso:
                    // Notificar usuario, miscelaneo.
                    sysMsg = string.Format("{0}", tail);
                    break;
                case EnumMensajes.errorAlRegistrarArchivo:
                    // Notificar usuario, miscelaneo.
                    sysMsg = string.Format("No se ha podido registrar el archivo en la BD.\nError interno");
                    break;
                case EnumMensajes.archivoDesconectado:
                    // Notificar usuario, miscelaneo.
                    sysMsg = string.Format("Se ha guardado la copia desconectada:\n{0}",tail);
                    break;

                default:
                    sysMsg = "Error desconocido";
                    break;
            }
            
            return sysMsg;

        }

    }
}