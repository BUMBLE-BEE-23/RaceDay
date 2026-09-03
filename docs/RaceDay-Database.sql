/*
    ============================================
    RaceDay System
    Part 1 - SQL Server Database Script
    ============================================

    Entities:
    1. Users
    2. EventTypes
    3. Events
    4. Categories
    5. Enrollments
    6. Results
*/

------------------------------------------------------------
-- 1. CREATE DATABASE
------------------------------------------------------------

IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO


------------------------------------------------------------
-- 2. REMOVE EXISTING TABLES IF THEY EXIST
--    This allows the script to be run again during testing.
------------------------------------------------------------

IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL
    DROP TABLE dbo.Results;
GO

IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrollments;
GO

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;
GO

IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL
    DROP TABLE dbo.Events;
GO

IF OBJECT_ID('dbo.EventTypes', 'U') IS NOT NULL
    DROP TABLE dbo.EventTypes;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO


------------------------------------------------------------
-- 3. USERS TABLE
------------------------------------------------------------

CREATE TABLE dbo.Users
(
    UserID INT IDENTITY(1,1) NOT NULL,

    FirstName NVARCHAR(50) NOT NULL,

    LastName NVARCHAR(50) NOT NULL,

    Email NVARCHAR(150) NOT NULL,

    PasswordHash NVARCHAR(255) NOT NULL,

    Role NVARCHAR(20) NOT NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Users
        PRIMARY KEY (UserID),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organizer', 'Participant'))
);
GO


------------------------------------------------------------
-- 4. EVENT TYPES TABLE
------------------------------------------------------------

CREATE TABLE dbo.EventTypes
(
    EventTypeID INT IDENTITY(1,1) NOT NULL,

    Name NVARCHAR(20) NOT NULL,

    CONSTRAINT PK_EventTypes
        PRIMARY KEY (EventTypeID),

    CONSTRAINT UQ_EventTypes_Name
        UNIQUE (Name)
);
GO


------------------------------------------------------------
-- 5. EVENTS TABLE
------------------------------------------------------------

CREATE TABLE dbo.Events
(
    EventID INT IDENTITY(1,1) NOT NULL,

    OrganizerID INT NOT NULL,

    EventTypeID INT NOT NULL,

    Name NVARCHAR(100) NOT NULL,

    Description NVARCHAR(500) NOT NULL,

    EventDate DATE NOT NULL,

    Location NVARCHAR(200) NOT NULL,

    Distance DECIMAL(6,2) NOT NULL,

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Events_CreatedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Events
        PRIMARY KEY (EventID),

    CONSTRAINT FK_Events_Users
        FOREIGN KEY (OrganizerID)
        REFERENCES dbo.Users(UserID),

    CONSTRAINT FK_Events_EventTypes
        FOREIGN KEY (EventTypeID)
        REFERENCES dbo.EventTypes(EventTypeID),

    CONSTRAINT CK_Events_Distance
        CHECK (Distance > 0)
);
GO


------------------------------------------------------------
-- 6. CATEGORIES TABLE
------------------------------------------------------------

CREATE TABLE dbo.Categories
(
    CategoryID INT IDENTITY(1,1) NOT NULL,

    EventID INT NOT NULL,

    Name NVARCHAR(100) NOT NULL,

    MinimumAge INT NULL,

    MaximumAge INT NULL,

    Distance DECIMAL(6,2) NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryID),

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID)
        REFERENCES dbo.Events(EventID),

    CONSTRAINT CK_Categories_MinimumAge
        CHECK (MinimumAge IS NULL OR MinimumAge >= 0),

    CONSTRAINT CK_Categories_MaximumAge
        CHECK (MaximumAge IS NULL OR MaximumAge >= 0),

    CONSTRAINT CK_Categories_AgeRange
        CHECK
        (
            MinimumAge IS NULL
            OR MaximumAge IS NULL
            OR MinimumAge <= MaximumAge
        ),

    CONSTRAINT CK_Categories_Distance
        CHECK
        (
            Distance IS NULL
            OR Distance > 0
        )
);
GO


------------------------------------------------------------
-- 7. ENROLLMENTS TABLE
------------------------------------------------------------

CREATE TABLE dbo.Enrollments
(
    EnrollmentID INT IDENTITY(1,1) NOT NULL,

    ParticipantID INT NOT NULL,

    EventID INT NOT NULL,

    CategoryID INT NOT NULL,

    EnrolledAt DATETIME2 NOT NULL
        CONSTRAINT DF_Enrollments_EnrolledAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Enrollments
        PRIMARY KEY (EnrollmentID),

    CONSTRAINT FK_Enrollments_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES dbo.Users(UserID),

    CONSTRAINT FK_Enrollments_Event
        FOREIGN KEY (EventID)
        REFERENCES dbo.Events(EventID),

    CONSTRAINT FK_Enrollments_Category
        FOREIGN KEY (CategoryID)
        REFERENCES dbo.Categories(CategoryID),

    CONSTRAINT UQ_Enrollments_Participant_Event
        UNIQUE (ParticipantID, EventID)
);
GO


------------------------------------------------------------
-- 8. RESULTS TABLE
------------------------------------------------------------

CREATE TABLE dbo.Results
(
    ResultID INT IDENTITY(1,1) NOT NULL,

    EnrollmentID INT NOT NULL,

    FinishTime TIME NOT NULL,

    FinishingPosition INT NOT NULL,

    RecordedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Results_RecordedAt
        DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultID),

    CONSTRAINT FK_Results_Enrollments
        FOREIGN KEY (EnrollmentID)
        REFERENCES dbo.Enrollments(EnrollmentID),

    CONSTRAINT UQ_Results_Enrollment
        UNIQUE (EnrollmentID),

    CONSTRAINT CK_Results_FinishingPosition
        CHECK (FinishingPosition > 0)
);
GO


------------------------------------------------------------
-- 9. INSERT EVENT TYPES
------------------------------------------------------------

INSERT INTO dbo.EventTypes
(
    Name
)
VALUES
(
    'Run'
),
(
    'Walk'
),
(
    'Cycle'
);
GO


------------------------------------------------------------
-- 10. INSERT 2 ORGANISERS
------------------------------------------------------------

INSERT INTO dbo.Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo@raceday.co.za',
    'DEVELOPMENT_HASH_ORGANISER_1',
    'Organizer'
),
(
    'Lerato',
    'Dlamini',
    'lerato@raceday.co.za',
    'DEVELOPMENT_HASH_ORGANISER_2',
    'Organizer'
);
GO


------------------------------------------------------------
-- 11. INSERT 2 PARTICIPANTS
------------------------------------------------------------

INSERT INTO dbo.Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role
)
VALUES
(
    'Sipho',
    'Nkosi',
    'sipho@example.com',
    'DEVELOPMENT_HASH_PARTICIPANT_1',
    'Participant'
),
(
    'Amahle',
    'Ndlovu',
    'amahle@example.com',
    'DEVELOPMENT_HASH_PARTICIPANT_2',
    'Participant'
);
GO


------------------------------------------------------------
-- 12. INSERT 3 EVENTS
------------------------------------------------------------

INSERT INTO dbo.Events
(
    OrganizerID,
    EventTypeID,
    Name,
    Description,
    EventDate,
    Location,
    Distance
)
VALUES
(
    1,
    1,
    'Johannesburg City Run',
    'A city running event for recreational and competitive runners.',
    '2026-10-10',
    'Johannesburg',
    10.00
),
(
    1,
    2,
    'Pretoria Charity Walk',
    'A community charity walking event.',
    '2026-11-07',
    'Pretoria',
    5.00
),
(
    2,
    3,
    'Cape Town Cycle Challenge',
    'A cycling event along selected Cape Town routes.',
    '2026-12-05',
    'Cape Town',
    40.00
);
GO


------------------------------------------------------------
-- 13. INSERT CATEGORIES
------------------------------------------------------------

INSERT INTO dbo.Categories
(
    EventID,
    Name,
    MinimumAge,
    MaximumAge,
    Distance
)
VALUES
(
    1,
    '10km Open',
    18,
    NULL,
    10.00
),
(
    1,
    '10km Junior',
    13,
    17,
    10.00
),
(
    2,
    '5km Open Walk',
    18,
    NULL,
    5.00
),
(
    2,
    '5km Junior Walk',
    13,
    17,
    5.00
),
(
    3,
    '40km Open Cycle',
    18,
    NULL,
    40.00
),
(
    3,
    '40km Junior Cycle',
    16,
    17,
    40.00
);
GO


------------------------------------------------------------
-- 14. INSERT SAMPLE ENROLLMENTS
------------------------------------------------------------

INSERT INTO dbo.Enrollments
(
    ParticipantID,
    EventID,
    CategoryID
)
VALUES
(
    3,
    1,
    1
),
(
    4,
    2,
    3
),
(
    3,
    3,
    5
);
GO


------------------------------------------------------------
-- 15. INSERT SAMPLE RESULTS
------------------------------------------------------------

INSERT INTO dbo.Results
(
    EnrollmentID,
    FinishTime,
    FinishingPosition
)
VALUES
(
    1,
    '01:02:35',
    14
),
(
    2,
    '00:42:18',
    8
);
GO


------------------------------------------------------------
-- 16. VERIFY USERS
------------------------------------------------------------

SELECT *
FROM dbo.Users;
GO


------------------------------------------------------------
-- 17. VERIFY EVENT TYPES
------------------------------------------------------------

SELECT *
FROM dbo.EventTypes;
GO


------------------------------------------------------------
-- 18. VERIFY EVENTS
------------------------------------------------------------

SELECT *
FROM dbo.Events;
GO


------------------------------------------------------------
-- 19. VERIFY CATEGORIES
------------------------------------------------------------

SELECT *
FROM dbo.Categories;
GO


------------------------------------------------------------
-- 20. VERIFY ENROLLMENTS
------------------------------------------------------------

SELECT *
FROM dbo.Enrollments;
GO


------------------------------------------------------------
-- 21. VERIFY RESULTS
------------------------------------------------------------

SELECT *
FROM dbo.Results;
GO


------------------------------------------------------------
-- END OF RACE DAY DATABASE SCRIPT
------------------------------------------------------------