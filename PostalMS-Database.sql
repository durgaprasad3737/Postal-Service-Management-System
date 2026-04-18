-- ============================================================
-- PostalMS — Postal Service Management System
-- Middlesex University CST2550
-- Complete SQL Server Script
-- Verified for SQL Server 2022 Compatibility - April 2026
-- ============================================================

USE master;
GO

-- Drop and recreate fresh
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'PostalServiceDB')
BEGIN
    ALTER DATABASE PostalServiceDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PostalServiceDB;
END
GO

CREATE DATABASE PostalServiceDB;
GO
USE PostalServiceDB;
GO

-- ============================================================
-- TABLES
-- ============================================================

-- Users — everyone who registers (User IS the Customer)
-- UserID is used directly as CustomerID in Parcel table
CREATE TABLE Users (
    UserID      VARCHAR(20)  PRIMARY KEY,
    FullName    VARCHAR(100) NOT NULL,
    Email       VARCHAR(100) NOT NULL UNIQUE,
    Phone       VARCHAR(20)  NOT NULL,
    Address     VARCHAR(200) NOT NULL,
    PostCode    VARCHAR(10)  NOT NULL,
    Role        VARCHAR(20)  NOT NULL DEFAULT 'Staff',
    Password    VARCHAR(255) NOT NULL,
    CreatedDate DATETIME     NOT NULL DEFAULT GETDATE(),
    City        VARCHAR(100) NOT NULL DEFAULT 'London'
);
GO

-- Parcel — CustomerID stores UserID directly (no FK — user IS customer)
CREATE TABLE Parcel (
    TrackingID        VARCHAR(20)    PRIMARY KEY,
    CustomerID        VARCHAR(20)    NOT NULL,       -- stores UserID
    ParcelType        VARCHAR(20)    NOT NULL,       -- 'Mail' or 'Package'
    SenderName        VARCHAR(100)   NOT NULL,
    SenderAddress     VARCHAR(200)   NOT NULL,
    ReceiverName      VARCHAR(100)   NOT NULL,
    ReceiverAddress   VARCHAR(200)   NOT NULL,
    Weight            DECIMAL(10,2)  NOT NULL,
    Size              VARCHAR(20)    NOT NULL,
    ServiceType       VARCHAR(30)    NOT NULL,       -- Standard, Express, Next Day
    IsInternational   BIT            NOT NULL DEFAULT 0,
    DestinationCountry VARCHAR(100)  NULL,
    Status            VARCHAR(30)    NOT NULL DEFAULT 'Pending',
    DateSent          DATETIME       NOT NULL DEFAULT GETDATE(),
    Price             DECIMAL(10,2)  NOT NULL,
    EstimatedDelivery DATETIME       NULL,
    RefundRequested   BIT            NOT NULL DEFAULT 0,
    RefundReason      VARCHAR(500)   NULL,
    RefundStatus      VARCHAR(30)    NULL            -- NULL, Pending, Approved, Rejected
);
GO

-- Delivery — linked to Parcel
CREATE TABLE Delivery (
    DeliveryID      VARCHAR(20)  PRIMARY KEY,
    TrackingID      VARCHAR(20)  NOT NULL,
    DriverName      VARCHAR(100) NULL,
    Route           VARCHAR(50)  NULL,
    AssignedDate    DATETIME     NOT NULL DEFAULT GETDATE(),
    DeliveryStatus  VARCHAR(30)  NOT NULL DEFAULT 'Assigned',
    AttemptCount    INT          NOT NULL DEFAULT 0,
    LastAttemptDate DATETIME     NULL,
    Notes           VARCHAR(500) NULL,
    FOREIGN KEY (TrackingID) REFERENCES Parcel(TrackingID) ON DELETE CASCADE
);
GO

-- ============================================================
-- INDEXES
-- ============================================================

CREATE INDEX idx_users_email      ON Users(Email);
CREATE INDEX idx_parcel_customer  ON Parcel(CustomerID);
CREATE INDEX idx_parcel_status    ON Parcel(Status);
CREATE INDEX idx_parcel_type      ON Parcel(ParcelType);
CREATE INDEX idx_parcel_refund    ON Parcel(RefundRequested);
CREATE INDEX idx_delivery_status  ON Delivery(DeliveryStatus);
CREATE INDEX idx_delivery_track   ON Delivery(TrackingID);
GO

-- ============================================================
-- SAMPLE DATA — only admin user pre-loaded
-- All other users start with empty data (bug fix)
-- ============================================================

INSERT INTO Users VALUES
('USER-001','Admin User', 'admin@gmail.com','07700000001','1 Admin Street London',  'EC1 1AA','Admin',   'admin123',GETDATE(),'London'),
('USER-002','John Smith', 'john@gmail.com', '07700900001','123 Main St London',      'N1 2AB', 'Customer','john123', GETDATE(),'London'),
('USER-003','Alice Brown','alice@gmail.com','07700900002','45 Park Road Manchester', 'M1 1AB', 'Customer','alice123',GETDATE(),'Manchester');
GO

-- Sample parcels for John (USER-002) only
INSERT INTO Parcel VALUES
('PS-2026-00001','USER-002','Package','John Smith','123 Main St London','Jane Doe','45 Park Road Manchester',2.5,'Medium','Express',0,NULL,'In Transit',GETDATE(),12.99,DATEADD(day,2,GETDATE()),0,NULL,NULL),
('PS-2026-00002','USER-002','Mail','John Smith','123 Main St London','Bob Wilson','78 High Street Birmingham',0.1,'Small','Standard',0,NULL,'Delivered',DATEADD(day,-5,GETDATE()),3.99,DATEADD(day,-2,GETDATE()),0,NULL,NULL),
('PS-2026-00003','USER-002','Package','John Smith','123 Main St London','Sarah Lee','12 Oak Avenue Leeds',3.8,'Large','Next Day',0,NULL,'Pending',GETDATE(),19.99,DATEADD(day,1,GETDATE()),0,NULL,NULL),
('PS-2026-00004','USER-002','Package','John Smith','123 Main St London','Chris Evans','56 Rose Lane Bristol',0.8,'Small','Standard',0,NULL,'Failed',DATEADD(day,-3,GETDATE()),4.99,DATEADD(day,-1,GETDATE()),0,NULL,NULL),
('PS-2026-00005','USER-002','Mail','John Smith','123 Main St London','Emma White','Rue de Rivoli, Paris',0.05,'Small','Standard',1,'France','In Transit',GETDATE(),8.99,DATEADD(day,7,GETDATE()),0,NULL,NULL);
GO

-- Sample deliveries for John's parcels
INSERT INTO Delivery VALUES
('DEL-001','PS-2026-00001','Mike Jones','Zone A',DATEADD(day,-1,GETDATE()),'In Progress',1,DATEADD(day,-1,GETDATE()),'Customer not home first attempt'),
('DEL-002','PS-2026-00002','Sarah Lee','Zone B',DATEADD(day,-5,GETDATE()),'Completed',1,DATEADD(day,-2,GETDATE()),'Delivered successfully to front door'),
('DEL-003','PS-2026-00003',NULL,NULL,GETDATE(),'Assigned',0,NULL,NULL),
('DEL-004','PS-2026-00004','Mike Jones','Zone C',DATEADD(day,-3,GETDATE()),'Failed',3,DATEADD(day,-1,GETDATE()),'3 failed attempts - returning to depot'),
('DEL-005','PS-2026-00005','Emma Davis','Zone B',GETDATE(),'In Progress',1,GETDATE(),'International - in transit to France');
GO

-- ============================================================
-- VIEWS
-- ============================================================

-- Dashboard stats per user
CREATE VIEW UserDashboardStats AS
SELECT
    CustomerID,
    COUNT(*)                                                    AS TotalParcels,
    SUM(CASE WHEN Status = 'Pending'          THEN 1 ELSE 0 END) AS Pending,
    SUM(CASE WHEN Status = 'In Transit'       THEN 1 ELSE 0 END) AS InTransit,
    SUM(CASE WHEN Status = 'Out for Delivery' THEN 1 ELSE 0 END) AS OutForDelivery,
    SUM(CASE WHEN Status = 'Delivered'        THEN 1 ELSE 0 END) AS Delivered,
    SUM(CASE WHEN Status = 'Failed'           THEN 1 ELSE 0 END) AS Failed,
    SUM(CASE WHEN RefundRequested = 1         THEN 1 ELSE 0 END) AS RefundsPending,
    SUM(Price)                                                  AS TotalSpent
FROM Parcel
GROUP BY CustomerID;
GO

-- Global dashboard stats
CREATE VIEW DashboardStats AS
SELECT
    COUNT(*)                                                    AS TotalParcels,
    SUM(CASE WHEN Status = 'In Transit'       THEN 1 ELSE 0 END) AS InTransit,
    SUM(CASE WHEN Status = 'Delivered'        THEN 1 ELSE 0 END) AS Delivered,
    SUM(CASE WHEN Status = 'Failed'           THEN 1 ELSE 0 END) AS Failed
FROM Parcel;
GO

-- Full parcel journey view
CREATE VIEW ParcelJourney AS
SELECT
    u.FullName   AS CustomerName,
    u.Email,
    p.TrackingID,
    p.ParcelType,
    p.SenderName,
    p.ReceiverName,
    p.Status     AS ParcelStatus,
    p.ServiceType,
    p.Price,
    p.IsInternational,
    p.DestinationCountry,
    p.RefundRequested,
    p.RefundStatus,
    p.EstimatedDelivery,
    d.DriverName,
    d.Route,
    d.DeliveryStatus,
    d.AttemptCount,
    d.Notes
FROM Users u
JOIN Parcel p   ON u.UserID      = p.CustomerID
LEFT JOIN Delivery d ON p.TrackingID = d.TrackingID;
GO

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

-- Login
CREATE PROCEDURE LoginUser
    @email    VARCHAR(100),
    @password VARCHAR(255)
AS BEGIN
    IF EXISTS (SELECT 1 FROM Users WHERE Email=@email AND Password=@password)
        SELECT UserID, FullName, Email, Role FROM Users WHERE Email=@email AND Password=@password;
    ELSE
        SELECT 'Invalid email or password' AS Message;
END;
GO

-- Register new user
CREATE PROCEDURE RegisterUser
    @userID   VARCHAR(20),
    @fullName VARCHAR(100),
    @email    VARCHAR(100),
    @phone    VARCHAR(20),
    @address  VARCHAR(200),
    @postCode VARCHAR(10),
    @role     VARCHAR(20),
    @password VARCHAR(255),
    @city     VARCHAR(100) = 'London'
AS BEGIN
    IF NOT EXISTS (SELECT 1 FROM Users WHERE Email=@email)
    BEGIN
        INSERT INTO Users VALUES (@userID,@fullName,@email,@phone,@address,@postCode,@role,@password,GETDATE(),@city);
        SELECT 'Account created successfully' AS Message;
    END
    ELSE SELECT 'Email already registered' AS Message;
END;
GO

-- Search parcel by tracking ID
CREATE PROCEDURE SearchParcel
    @trackID VARCHAR(20)
AS BEGIN
    SELECT p.*, d.DriverName, d.DeliveryStatus, d.AttemptCount, d.Notes
    FROM Parcel p
    LEFT JOIN Delivery d ON p.TrackingID = d.TrackingID
    WHERE p.TrackingID = @trackID;
END;
GO

-- Update parcel status
CREATE PROCEDURE UpdateParcelStatus
    @trackID  VARCHAR(20),
    @newStatus VARCHAR(30)
AS BEGIN
    IF EXISTS (SELECT 1 FROM Parcel WHERE TrackingID=@trackID)
    BEGIN
        UPDATE Parcel SET Status=@newStatus WHERE TrackingID=@trackID;
        IF @newStatus='Delivered'
            UPDATE Delivery SET DeliveryStatus='Completed' WHERE TrackingID=@trackID;
        SELECT 'Status updated successfully' AS Message;
    END
    ELSE SELECT 'Parcel not found' AS Message;
END;
GO

-- Delete parcel (cascade deletes delivery)
CREATE PROCEDURE DeleteParcel
    @trackID VARCHAR(20)
AS BEGIN
    IF EXISTS (SELECT 1 FROM Parcel WHERE TrackingID=@trackID)
    BEGIN
        DELETE FROM Parcel WHERE TrackingID=@trackID;
        SELECT 'Parcel deleted successfully' AS Message;
    END
    ELSE SELECT 'Parcel not found' AS Message;
END;
GO

-- Auto assign delivery when parcel is created
CREATE PROCEDURE AutoAssignDelivery
    @trackID  VARCHAR(20),
    @newDelID VARCHAR(20)
AS BEGIN
    IF EXISTS (SELECT 1 FROM Parcel WHERE TrackingID=@trackID)
    BEGIN
        INSERT INTO Delivery VALUES (@newDelID,@trackID,NULL,NULL,GETDATE(),'Assigned',0,NULL,NULL);
        SELECT 'Delivery created successfully' AS Message;
    END
    ELSE SELECT 'Parcel not found' AS Message;
END;
GO

-- Update delivery status with attempt count logic
CREATE PROCEDURE UpdateDeliveryStatus
    @delID     VARCHAR(20),
    @newStatus VARCHAR(30)
AS BEGIN
    IF EXISTS (SELECT 1 FROM Delivery WHERE DeliveryID=@delID)
    BEGIN
        IF @newStatus='Failed'
        BEGIN
            UPDATE Delivery
            SET DeliveryStatus=@newStatus, AttemptCount=AttemptCount+1, LastAttemptDate=GETDATE()
            WHERE DeliveryID=@delID;
            IF (SELECT AttemptCount FROM Delivery WHERE DeliveryID=@delID) >= 3
            BEGIN
                UPDATE Delivery SET DeliveryStatus='Return to Depot' WHERE DeliveryID=@delID;
                SELECT 'Max attempts reached — returning to depot' AS Message;
            END
            ELSE SELECT 'Status updated successfully' AS Message;
        END
        ELSE
        BEGIN
            UPDATE Delivery SET DeliveryStatus=@newStatus WHERE DeliveryID=@delID;
            SELECT 'Status updated successfully' AS Message;
        END
    END
    ELSE SELECT 'Delivery not found' AS Message;
END;
GO

-- Refund request
CREATE PROCEDURE RequestRefund
    @trackID VARCHAR(20),
    @reason  VARCHAR(500)
AS BEGIN
    IF EXISTS (SELECT 1 FROM Parcel WHERE TrackingID=@trackID)
    BEGIN
        UPDATE Parcel
        SET RefundRequested=1, RefundReason=@reason, RefundStatus='Pending'
        WHERE TrackingID=@trackID;
        SELECT 'Refund request submitted successfully' AS Message;
    END
    ELSE SELECT 'Parcel not found' AS Message;
END;
GO

-- ============================================================
-- TEST QUERIES — uncomment to verify
-- ============================================================

-- SELECT * FROM Users;
-- SELECT * FROM Parcel;
-- SELECT * FROM Delivery;
-- SELECT * FROM DashboardStats;
-- SELECT * FROM UserDashboardStats WHERE CustomerID = 'USER-002';
-- SELECT * FROM ParcelJourney;

-- EXEC LoginUser 'john@email.com', 'john123';
-- EXEC LoginUser 'admin@postalms.com', 'admin123';
-- EXEC SearchParcel 'PS-2026-00001';
-- EXEC UpdateParcelStatus 'PS-2026-00003', 'In Transit';
-- EXEC UpdateDeliveryStatus 'DEL-003', 'In Progress';
-- EXEC RequestRefund 'PS-2026-00004', 'Parcel arrived damaged';

-- New user test (should show NO parcels):
-- EXEC RegisterUser 'USER-999','Test User','test@test.com','07700000099','1 Test Road','EC1 9ZZ','Staff','test123';
-- SELECT * FROM Parcel WHERE CustomerID = 'USER-999'; -- should return 0 rows
