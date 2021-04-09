CREATE DATABASE `DingoMessages`;

USE `DingoMessages`;

DELIMITER //

CREATE TABLE EncryptionStates(
    `Id` NVARCHAR(450) NOT NULL,
    `EncryptionClientState` LONGTEXT NULL,
    `X509IdentityKey` NVARCHAR(1000) NULL,
    `PrivateIdentityKey` NVARCHAR(1000) NULL,
    `Bundles` LONGTEXT NULL
);

//

DELIMITER ;

DELIMITER //

CREATE TABLE Messages(
	Id nvarchar(450) NOT NULL,
	Messages LONGTEXT NOT NULL DEFAULT ""
);

//

DELIMITER ;

DELIMITER //

CREATE PROCEDURE DingoMessages.CreateUser(IN Id NVARCHAR(450))
BEGIN
    INSERT INTO `Messages`(`Id`, `Messages`)
VALUES(Id, '') ;
END ; //

//

DELIMITER ;

DELIMITER //

CREATE OR REPLACE PROCEDURE DingoMessages.DeleteUser(Id NVARCHAR(450))
BEGIN
    DELETE
FROM
    Messages
WHERE
    `Id` = Id ;
DELETE
FROM
    EncryptionStates
WHERE
    `Id` = Id ;
END ;

//

DELIMITER ;

DELIMITER //

CREATE PROCEDURE DingoMessages.GetBundles(Id NVARCHAR(450))
BEGIN
    SELECT
        Bundles
    FROM
        EncryptionStates
    WHERE
        `Id` = Id ;
END ;

//

DELIMITER ;

DELIMITER //

CREATE PROCEDURE DingoMessages.GetEncryptionState(Id NVARCHAR(450))
BEGIN
    SELECT
        EncryptionClientState
    FROM
        EncryptionStates
    WHERE
        `Id` = Id ;
END ;

//

DELIMITER ;

DELIMITER //

CREATE PROCEDURE DingoMessages.GetIdentityKeys(Id NVARCHAR(450))
BEGIN
    SELECT
        X509IdentityKey, PrivateIdentityKey
    FROM
        EncryptionStates
    WHERE
        `Id` = Id ;
END ;

//

DELIMITER ;

DELIMITER //

CREATE PROCEDURE DingoMessages.GetMessages(Id NVARCHAR(450))
BEGIN
    SELECT
        Messages
    FROM
        Messages
    WHERE
        `Id` = Id ;
END ;

//

DELIMITER ;

DELIMITER //



//

DELIMITER ;

DELIMITER //



//

DELIMITER ;

DELIMITER //



//

DELIMITER ;