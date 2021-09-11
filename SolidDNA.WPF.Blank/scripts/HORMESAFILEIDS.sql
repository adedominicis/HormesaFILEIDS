/*Script de destruccion*/
DROP TABLE configuraciones;

DROP TABLE archivos;

DROP TABLE extensiones;

DROP TABLE partids;

drop table errorlogs

/*Script de destruccion*/
create table errorLogs(
id int identity(1,1) primary key,
exceptionType varchar(50),
exceptionMessage varchar(500),
stackTrace varchar(2000),
exceptionDateTime varchar(50)
);

CREATE TABLE EXTENSIONES
(
    id INT identity(1, 1) PRIMARY KEY,
    nombre VARCHAR(255) NOT NULL,
    extension VARCHAR(10) NOT NULL,
);

CREATE TABLE PARTIDS
(
    id INT identity(1, 1) PRIMARY KEY,
    isdeprecated BIT NOT NULL,
);

CREATE TABLE ARCHIVOS
(
    id INT PRIMARY KEY NOT NULL,
    fullpath VARCHAR(255) NOT NULL UNIQUE,
    id_extension INT FOREIGN KEY REFERENCES extensiones(id) NOT NULL,
    id_pdm INT,
);

CREATE TABLE configuraciones
(
    id INT identity(1, 1) PRIMARY KEY,
    id_archivo INT FOREIGN KEY REFERENCES archivos(id) NOT NULL,
    id_partid INT FOREIGN KEY REFERENCES partids(id),
    nombre VARCHAR(255) NOT NULL
)
	/*Procedimientos almacenados*/
	/*Insertar una nueva extensión que no exista.*/
GO

CREATE
	OR

ALTER PROCEDURE addExtensionIfNotExistant
    (@fullpath VARCHAR(255))
AS
BEGIN
    /*extraer la extension*/
    DECLARE @extension VARCHAR(10)
    DECLARE @exists INT

    SET @extension = (
			SELECT reverse(left(reverse(@fullpath), charindex('.', reverse(@fullpath)) - 1))
			)
    SET @exists = (
			SELECT count(*)
    FROM extensiones
    WHERE extension = @extension
			)

    /*Si la extensión no existe, se inserta.*/
    IF @exists = 0
	BEGIN
        INSERT INTO extensiones
        VALUES
            (
                'Custom',
                @extension
			)
    END
END;
GO

/*Crear nuevo partid*/
GO

CREATE
	OR

ALTER PROCEDURE createPartId
AS
BEGIN
    INSERT INTO partids
    OUTPUT
    inserted.id
    VALUES
        (0)
END
GO

/*Insertar nuevo archivo desde el path*/
GO

CREATE
	OR

ALTER PROCEDURE insertFileFromPath
    (@fullpath VARCHAR(255))
AS
BEGIN
    EXEC addExtensionIfNotExistant @fullpath

    DECLARE @partid INT
    DECLARE @extensionid INT
    DECLARE @extension VARCHAR(10)
    DECLARE @alreadyExists INT

    SET @alreadyExists = (
			SELECT count(*)
    FROM archivos
    WHERE fullpath = @fullpath
			)

    /*Verificar que el archivo existe.*/
    IF @alreadyexists = 0
	BEGIN
        SET NOCOUNT ON;

        BEGIN TRANSACTION REGISTRO;

        BEGIN TRY
			/*Obtener el ID de la extensión*/
			SET @extension = (
					SELECT reverse(left(reverse(@fullpath), charindex('.', reverse(@fullpath)) - 1))
					)
			SET @extensionid = (
					SELECT id
        FROM extensiones
        WHERE extensiones.extension = @extension
					)

			/*Registrar nuevo partid*/
			INSERT INTO partids
        VALUES
            (0)

			SET @partid = SCOPE_IDENTITY()

			/*Guardar en la tabla de archivos.*/
			INSERT INTO archivos
        VALUES
            (
                @partid,
                @fullpath,
                @extensionid,
                0
				)

			SELECT @partid

			COMMIT TRANSACTION REGISTRO
		END TRY

		BEGIN CATCH
			ROLLBACK TRANSACTION REGISTRO;
		END CATCH

        SET NOCOUNT OFF
    END
END
GO

/*Asignar una configuracion al archivo padre*/
GO

CREATE
	OR

ALTER PROCEDURE createConfigFromFilePartID
    (
    @partid INT,
    @configname VARCHAR(255)
)
AS
BEGIN
    DECLARE @exists INT
    DECLARE @extension VARCHAR(10)
    DECLARE @fullpath VARCHAR(255)

    IF @partid <> 0
	BEGIN
        /*Verificar si la config ya existe*/
        SET @exists = (
				SELECT count(*)
        FROM configuraciones,
            archivos
        WHERE configuraciones.id_archivo = @partid
            AND configuraciones.nombre = @configname
				)
        /*Crear fullpath*/
        SET @fullpath = (
				SELECT fullpath
        FROM archivos
        WHERE id = @partid
				)
        /*Verificar si la extensión es sldprt o sldasm*/
        SET @extension = (
				SELECT reverse(left(reverse(@fullpath), charindex('.', reverse(@fullpath)) - 1))
				)

        /*Si no cuenta ninguna configuracion en este archivo que tenga el mismo nombre, puede agregarla.*/
        IF @exists = 0
            AND (
				@extension = 'sldprt'
            OR @extension = 'sldasm'
				)
		BEGIN
            INSERT INTO configuraciones
            VALUES
                (
                    @partid,
                    NULL,
                    @configname
				)
        END
    END
END
GO

/*Crear una configuracion con el path del archivo padre*/
CREATE
	OR

ALTER PROCEDURE createConfigFromFilePath
    (
    @fullpath VARCHAR(255),
    @configname VARCHAR(255)
)
AS
BEGIN
    DECLARE @filepartid INT
    DECLARE @exists INT
    DECLARE @extension VARCHAR(10)

    /*Obtener el partid del archivo*/
    SET @filepartid = (
			SELECT id
    FROM archivos
    WHERE archivos.fullpath = @fullpath
			)

    IF @filepartid <> 0
	BEGIN
        /*Verificar si la configuracion ya existe*/
        SET @exists = (
				SELECT count(*)
        FROM configuraciones,
            archivos
        WHERE configuraciones.id_archivo = @filepartid
            AND configuraciones.nombre = @configname
				)
        /*Verificar si la extensión es sldprt o sldasm*/
        SET @extension = (
				SELECT reverse(left(reverse(@fullpath), charindex('.', reverse(@fullpath)) - 1))
				)

        /*Si no cuenta ninguna configuracion en este archivo que tenga el mismo nombre, puede agregarla.*/
        IF @exists = 0
            AND (
				@extension = 'sldprt'
            OR @extension = 'sldasm'
				)
		BEGIN
            INSERT INTO configuraciones
            VALUES
                (
                    @filepartid,
                    NULL,
                    @configname
				)
        END
    END
END
GO

/*Obtener todas las configuraciones con sus partids desde el id del archivo*/
GO

CREATE
	OR

ALTER PROCEDURE listConfigsFromFilePartID
    (@partid INT)
AS
BEGIN
    SELECT configuraciones.nombre AS 'CONFIGURACIÓN',
        format(partids.id, '000-000') AS 'PARTID'
    FROM configuraciones
        INNER JOIN archivos ON configuraciones.id_archivo = ARCHIVOS.id
        LEFT JOIN partids ON partids.id = configuraciones.id_partid
    WHERE archivos.id = @partid
END
GO

/*Eliminar una configuracion desde el id del archivo y el nombre de la configuracion*/
GO

CREATE
	OR

ALTER PROCEDURE deleteConfigFromFilePartID
    (
    @filePartId INT,
    @configname VARCHAR(255)
)
AS
BEGIN
    DECLARE @configpartid INT

    /*Si la config tiene un partid asignado, hay que hacerlo deprecated*/
    SET @configpartid = (
			SELECT configuraciones.id_partid
    FROM configuraciones,
        archivos
    WHERE configuraciones.nombre = @configname
        AND archivos.id = configuraciones.id_archivo
        AND archivos.id = @filepartid
			)

    IF @configpartid <> 0
	BEGIN
        UPDATE partids
		SET isdeprecated = 1
		WHERE partids.id = @configpartid
    END

    /*Eliminar*/
    DELETE
	FROM configuraciones
	WHERE id_archivo = @filePartId
        AND nombre = @configname
END
GO

/*Eliminar una configuracion desde el path del archivo y el nombre de la configuracion*/
GO

CREATE
	OR

ALTER PROCEDURE deleteConfigFromFilePath
    (
    @fullpath VARCHAR(255),
    @configname VARCHAR(255)
)
AS
BEGIN
    DECLARE @filepartid INT
    DECLARE @configpartid INT

    /*Obtener PARTID del archivo*/
    SET @filepartid = (
			SELECT id
    FROM archivos
    WHERE archivos.fullpath = @fullpath
			)
    /*Si la config tiene un partid asignado, hay que hacerlo deprecated*/
    SET @configpartid = (
			SELECT configuraciones.id_partid
    FROM configuraciones,
        archivos
    WHERE configuraciones.nombre = @configname
        AND archivos.id = configuraciones.id_archivo
        AND archivos.id = @filepartid
			)

    IF @configpartid <> 0
	BEGIN
        UPDATE partids
		SET isdeprecated = 1
		WHERE partids.id = @configpartid
    END

    DELETE
	FROM configuraciones
	WHERE id_archivo = @filePartId
        AND nombre = @configname
END
GO

/*Renombrar una configuracion desde el path del archivo y el nombre de la configuracion*/
GO

CREATE
	OR

ALTER PROCEDURE renameConfigFromFilePath
    (
    @fullpath VARCHAR(255),
    @oldconfigname VARCHAR(255),
    @newconfigname VARCHAR(255)
)
AS
BEGIN
    DECLARE @filepartid INT

    SET @filepartid = (
			SELECT id
    FROM archivos
    WHERE archivos.fullpath = @fullpath
			)

    UPDATE configuraciones
	SET nombre = @newconfigname
	WHERE configuraciones.id_archivo = @filePartId
        AND nombre = @oldconfigname
END
GO

/*Renombrar una configuracion desde el ID del archivo y el nombre de la configuracion*/
GO

CREATE
	OR

ALTER PROCEDURE renameConfigFromFilePartId
    (
    @filepartid INT,
    @oldconfigname VARCHAR(255),
    @newconfigname VARCHAR(255)
)
AS
BEGIN
    UPDATE configuraciones
	SET nombre = @newconfigname
	WHERE configuraciones.id_archivo = @filePartId
        AND nombre = @oldconfigname
END
GO

/*Determinar si un archivo tiene la configuracion especificada y si esta tiene partid*/
GO

CREATE
	OR

ALTER PROCEDURE checkIfConfigExistsFromFilePartId
    (
    @filepartid INT,
    @configname VARCHAR(255)
)
AS
BEGIN
    DECLARE @exists INT

    SET @exists = (
			SELECT count(*)
    FROM configuraciones,
        archivos
    WHERE configuraciones.nombre = @configname
        AND archivos.id = configuraciones.id_archivo
        AND archivos.id = @filepartid
			)

    SELECT @exists
END
GO

/*Obtener el partid asignado a una configuracion*/
GO

CREATE
	OR

ALTER PROCEDURE getConfigPartIdFromConfigName
    (
    @filepartid INT,
    @configname VARCHAR(255)
)
AS
BEGIN
    SELECT partids.id
    FROM PARTIDS,
        configuraciones
    WHERE configuraciones.id_archivo = @filepartid
        AND PARTIDS.id = configuraciones.id_partid
        AND configuraciones.nombre = @configname
END
GO

/*Asignar un nuevo partid a una configuracion existente.*/
GO

CREATE
	OR

ALTER PROCEDURE assignPartIdToConfigFromFileID
    (
    @filepartid INT,
    @configname VARCHAR(255)
)
AS
BEGIN
    DECLARE @configpartid INT
    DECLARE @exists INT
    DECLARE @alreadyassigned INT

    SET NOCOUNT ON;

    BEGIN TRANSACTION REGISTRO;

    BEGIN TRY
		/*Verificar que la configuración existe*/
		SET @exists = (
				SELECT count(*)
    FROM configuraciones,
        archivos
    WHERE configuraciones.nombre = @configname
        AND archivos.id = configuraciones.id_archivo
        AND archivos.id = @filepartid
				)
		/*Verificar que la configuración no tiene un partid ya asignado*/
		SET @alreadyassigned = (
				SELECT count(PARTIDS.id)
    FROM PARTIDS,
        configuraciones
    WHERE configuraciones.id_archivo = @filepartid
        AND PARTIDS.id = configuraciones.id_partid
        AND configuraciones.nombre = @configname
				)

		/*Si la configuración no existe, hay que crearla.*/
		IF @exists = 0
		BEGIN
        EXEC createConfigFromFilePartID @filepartid,
				@configname

        SET @exists = 1
    END

		/*Si la configuracion existe pero aun no tiene partid, se crea el partid.*/
		IF @exists <> 0
        AND @alreadyassigned = 0
		BEGIN
        /*Registrar nuevo partid*/
        INSERT INTO partids
        VALUES
            (0)

        SET @configpartid = SCOPE_IDENTITY()

        /*Asignar el PARTID a la configuracion, se actualiza la tabla configuraciones.*/
        UPDATE configuraciones
			SET id_partid = @configpartid
			WHERE id_archivo = @filepartid
            AND nombre = @configname
    END

		COMMIT TRANSACTION REGISTRO
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION REGISTRO;
	END CATCH

    SET NOCOUNT OFF
END
GO

/*Obtener path del archivo, desde el PARTID*/
GO

CREATE
	OR

ALTER PROCEDURE getFilePathFromPartId
    (@filePartId INT)
AS
BEGIN
    SELECT fullpath
    FROM archivos
    WHERE id = @filepartid
END
GO

/*Obtener partid del archivo*/
GO

CREATE
	OR

ALTER PROCEDURE getFilePartIdFromPath
    (@fullpath VARCHAR(255))
AS
BEGIN
    SELECT id
    FROM archivos
    WHERE fullpath = @fullpath
END
GO

CREATE
	OR

/*actualizar el path de un archivo*/
ALTER PROCEDURE updatePathFromPartId
    (
    @filePartId INT,
    @newPath VARCHAR(255)
)
AS
BEGIN
    UPDATE archivos
    SET fullpath = @newPath
	WHERE id = @filepartid
END
GO

/*Inserciones básicas*/
INSERT INTO extensiones
VALUES
    (
        'SolidWorks Part',
        'sldprt'
	);

INSERT INTO extensiones
VALUES
    (
        'SolidWorks Assembly',
        'sldasm'
	);

INSERT INTO extensiones
VALUES
    (
        'SolidWorks Drawing',
        'slddrw'
	);

    --Ampliacion V.1.0.5
    -- Insertar nuevo error.

create table errorLogs(
id int identity(1,1) primary key,
exceptionType varchar(50),
exceptionMessage varchar(500),
stackTrace varchar(2000),
exceptionDateTime varchar(50)
);

go
create or alter procedure logErrores(
@exceptionType varchar(50),
@exceptionMessage varchar(500),
@stackTrace varchar(2000)
)
as begin
insert into errorLogs values(@exceptionType,@exceptionMessage,@stackTrace,GETDATE())
end
go

go
create view mostrarLogErrores as
select id as "ID",
exceptionType as "TIPO",
exceptionMessage as "MENSAJE",
stackTrace as "STACKTRACE",
exceptionDateTime as "FECHA"
from errorLogs order by id desc
go

go

-- Tabla para serilog
CREATE TABLE [dbo].[LogEvents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Message] [nvarchar](max) NULL,
	[MessageTemplate] [nvarchar](max) NULL,
	[Level] [nvarchar](max) NULL,
	[TimeStamp] [datetime] NULL,
	[Exception] [nvarchar](max) NULL,
	[Properties] [nvarchar](max) NULL,
 CONSTRAINT [PK_LogEvents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Listado de archivos activos
create view listadoArchivosActivos as
select format(archivos.id,'000-000') as 'PARTID', fullpath as 'RUTA'
from archivos
