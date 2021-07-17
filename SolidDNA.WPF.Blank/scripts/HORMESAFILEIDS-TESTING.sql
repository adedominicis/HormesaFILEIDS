/*Hormesa FILEIDS testing*/

/*Inserciones de archivos*/
EXEC insertfilefrompath '\test\test\part.sldprt'

EXEC insertfilefrompath '\test\test\part.sldasm'

EXEC insertfilefrompath '\test\test\part.slddrw'

EXEC insertfilefrompath '\test\test\part.pdf'

/*crear configuraciones*/
EXEC createConfigFromFilePartID 1,
	'Part 1 Config 1'

EXEC createConfigFromFilePartID 1,
	'Part 1 Config 2'

EXEC createConfigFromFilePartID 2,
	'Part 2 config 1'

EXEC createConfigFromFilePartID 3,
	'Drawing, this should fail'

EXEC createConfigFromFilePartID 4,
	'PDF, this should fail'

/*Test Crear configuracion en archivo existente desde el path*/
EXEC createConfigFromFilePath '\test\test\part.sldprt',
	'Tercera Config'

SELECT *
FROM configuraciones

/*Test eliminar configuracion existente en archivo existente desde el path*/
EXEC deleteConfigFromFilePath '\test\test\part.sldprt',
	'Tercera Confi'

SELECT *
FROM configuraciones

EXEC deleteConfigFromFilePath '\test\test\part.sldprt',
	'Tercera Config'

SELECT *
FROM configuraciones

/*Renombrar configuracion existente en archivo existente desde el path*/
EXEC renameConfigFromFilePath '\test\test\part.sldprt',
	'Tercera Config',
	'BUBU'

SELECT *
FROM configuraciones

/*Renombrar configuracion existente en archivo existente desde el path*/
EXEC renameConfigFromFilePartId 1,
	'BUBU',
	'BUBU Y PEPITO'

SELECT *
FROM configuraciones

/*Asignar partids a configuraciones existentes.*/
SELECT *
FROM configuraciones

EXEC assignPartIdToConfigFromFileID 1,
	'Part 1 Config 1'

EXEC assignPartIdToConfigFromFileID 1,
	'Part 1 Config 2'

EXEC assignPartIdToConfigFromFileID 2,
	'Part 2 config 1'

EXEC listConfigsFromFilePartID 1

/*Obtener el partid de una configuracion particular*/
EXEC getConfigPartIdFromConfigName 1,
	'ODIN'

/*Test eliminar configuracion existente en archivo existente desde el id*/
EXEC deleteConfigFromFilePartID 1,
	'Part 1 Config 2'
