using AngelSix.SolidDna;
using Dna;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static AngelSix.SolidDna.SolidWorksEnvironment;
using HormesaFILEIDS.model;
namespace HormesaFILEIDS
{
    // 
    //  *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-
    //
    //     Welcome to SolidDNA by AngelSix
    //
    //        SolidDNA is a modern framework designed to make developing SolidWorks Add-ins easy.
    //
    //        With this template you have a ready-to-go add-in that will load inside of SolidWorks
    //        and a bunch of useful example projects available here 
    //        https://github.com/angelsix/solidworks-api/tree/develop/Tutorials
    //
    //
    //     Registering Add-in Dll
    //
    //        To get your dll to run inside SolidWorks as an add-in you need to register it.
    //        Inside this project template in the Resources folder is the SolidWorksAddinInstaller.exe.
    //        Compile your project, open up the SolidWorksAddinInstaller.exe, then browse for your
    //        output dll file (for example /bin/Debug/SolidDNA.WPF.Blank.dll) and click Install.
    //
    //        Now when you start SolidWorks your Add-in should load and should appear in the 
    //        Tools > Add-ins menu. 
    //
    //        NOTE: You only need to register your add-in once, or when you move the location or 
    //              change the filename.
    //        
    //
    //     Debugging Code
    //
    //        In order to press F5 to start up SolidWorks and instantly begin debugging your code,
    //        open up Project Properties, go to Debug, select Start External Program, and point
    //        to `C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\SLDWORKS.exe` by default.
    //        If your install is in a different location just change this path.
    //
    //        Also the Project `Properties > Application > Assembly Information` has the
    //        `Make Assembly COM Visible` checked.
    //
    //
    //     Startup Flow
    //
    //        When your SolidDna add-in first loads, SolidWorks will call the ConnectToSW method
    //        inside your AddInIntegration class. 
    //
    //        This method will fire the following methods in this order:
    // 
    //         - ConfigureServices
    //         - PreConnectToSolidWorks
    //         - PreLoadPlugIns
    //         - ApplicationStartup
    //         - ConnectedToSolidWorks
    //        
    //        Once your add-in is unloaded by SolidWorks the DisconnectFromSW method will be called
    //        which will in turn fire the following methods:
    //
    //         - DisconnectedFromSolidWorks
    //
    //  *-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-
    //

    /// <summary>
    /// Register as a SolidWorks Add-In
    /// </summary>

    //Se integra el GUID con el que se registra el assembly en el registro.

    [Guid("19BCBD7C-1518-4180-B118-7DF731D40E5C"), ComVisible(true)]
    public class MyAddinIntegration : AddInIntegration
    {
        #region Constantes
        private const string dbPassword = "HfIdS1000210721";
        private const string dbUser = "FILEIDSUSER";
        private const string dbLogin = "FILEIDS";
        private const string dbName = "HORMESAFILEIDS";
        private const string thisAppPass = "capacillo";
        private const string thisAppUser = "admin";
        private const string serverFileName = "server";
        private const string schemaName = "dbo";
        private const string tableName = "LogEvents";

        #endregion


        #region Helper methods

        public void checkServerConnection()
        {
            DAO dao = new DAO();
            ErrorHandler err = new ErrorHandler();
            if (!dao.IsServerConnected())
            {
                err.thrower(err.handler(EnumMensajes.errorEnConexionDB));
            }
        }
        #endregion

        /// <summary>
        /// Specific application start-up code
        /// </summary>
        public override void ApplicationStartup()
        {
            //Revisar conexion al servidor
            checkServerConnection();

            string serverFilePath = "";
            string installFolder = "";
            try
            {
                //Directorio en donde está instalada la aplicación.
                installFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                serverFilePath = string.Format("{0}\\Resources\\{1}", installFolder, serverFileName);
                //Retornar string de conexion
                string connectionString = string.Format("Server={0};Initial Catalog={1};User Id={2};Password={3};Connect Timeout=5",File.ReadAllText(serverFilePath), dbName, dbLogin, dbPassword);

                //Serilog logger
                Log.Logger = new LoggerConfiguration().WriteTo
                .MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = tableName,
                        SchemaName = schemaName,
                        AutoCreateSqlTable = true
                    },
                    restrictedToMinimumLevel: LogEventLevel.Debug,
                    formatProvider: null,
                    columnOptions: null,
                    logEventFormatter: null)
                .CreateLogger();

                //Prueba en vacio del logger
                Log.Debug("Inicializando FILEIDS "+DateTime.Now);


            }
            catch (Exception ex)
            {
                Log.Error("Error inicializando logger");
            }
        }

        /// <summary>
        /// Called when Dependency Injection is being setup
        /// </summary>
        /// <param name="construction">The framework construction</param>
        public override void ConfigureServices(FrameworkConstruction construction)
        {
            //
            //  Example
            // ---------
            //
            //   Add a service like this (include using Microsoft.Extensions.DependencyInjection):
            //
            //      construction.Services.AddSingleton(new SomeClass());
            //
            //   Retrieve the service anywhere in your application like this
            //
            //      Dna.Framework.Service<SomeClass>();
            //

            // Add file logger (will be in /bin/Debug/SolidDNA.WPF.Blank.log.txt)
            construction.AddFileLogger(Path.ChangeExtension(this.AssemblyFilePath(), "log.txt"));
        }

        /// <summary>
        /// Use this to do early initialization and any configuration of the 
        /// PlugInIntegration class properties such as <see cref="PlugInIntegration.UseDetachedAppDomain"/>
        /// </summary>
        public override void PreConnectToSolidWorks()
        {

        }

        /// <summary>
        /// Steps to take before any plug-in loads
        /// </summary>
        public override void PreLoadPlugIns()
        {

        }
    }

    /// <summary>
    /// Registers as a SolidDna PlugIn to be loaded by our AddIn Integration class 
    /// when the SolidWorks add-in gets loaded.
    /// 
    /// NOTE: We can have multiple plug-ins in a single add-in
    /// </summary>
    public class MySolidDnaPlugIn : SolidPlugIn<MySolidDnaPlugIn>
    {
        #region Private Members

        /// <summary>
        /// The Taskpane UI for our plug-in
        /// </summary>
        private TaskpaneIntegration<MyTaskpaneUI> mTaskpane;

        #endregion

        #region Region Public Properties

        /// <summary>
        /// My Add-in title
        /// </summary>
        public override string AddInTitle => "NUMEROS DE PARTE - HORMESA";

        /// <summary>
        /// My Add-in description
        /// </summary>
        public override string AddInDescription => "Generador de numeros de parte. HORMESA";

        #endregion

        #region Connect To SolidWorks

        public override void ConnectedToSolidWorks()
        {
            // Create our taskpane UI
            mTaskpane = new TaskpaneIntegration<MyTaskpaneUI>()
            {
                // Set taskpane icon
                Icon = Path.Combine(this.AssemblyPath(), "logo-small.png"),
                WpfControl = new MyAddinControl()
            };

            // Add it to taskpane
            mTaskpane.AddToTaskpaneAsync();
        }

        public override void DisconnectedFromSolidWorks()
        {

        }

        #endregion

    }
}
