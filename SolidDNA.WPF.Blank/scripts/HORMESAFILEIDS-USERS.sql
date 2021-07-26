/*Hormesa FILEIDS USERS, ROLES and LOGINS*/


/*Crear login de la aplicación*/

USE [master]
GO

/* For security reasons the login is created disabled and with a random password. */
/****** Object:  Login [FILEIDS]    Script Date: 7/21/2021 10:48:08 PM ******/
CREATE LOGIN [FILEIDS] WITH PASSWORD='HfIdS1000210721', DEFAULT_DATABASE=[HORMESAFILEIDS], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO

/*Crear usuario de la aplicación*/
create user FILEIDSUSER for login FILEIDS



