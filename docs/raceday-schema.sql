CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- 1. Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 2. Organisers Table
CREATE TABLE Organisers (
    OrganiserId INT PRIMARY KEY FOREIGN KEY REFERENCES Users(UserId),
    OrganisationName NVARCHAR(100) NULL
);
GO

-- 3. Participants Table
CREATE TABLE Participants (
    ParticipantId INT PRIMARY KEY FOREIGN KEY REFERENCES Users(UserId),
    DateOfBirth DATE NULL,
    ContactNumber NVARCHAR(20) NULL
);
GO

-- 4. Events Table
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL FOREIGN KEY REFERENCES Organisers(OrganiserId),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    [Date] DATETIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Upcoming' CHECK (Status IN ('Upcoming', 'Ongoing', 'Completed', 'Cancelled')),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- 5. Categories Table
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL FOREIGN KEY REFERENCES Events(EventId),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    EntryFee DECIMAL(10, 2) NOT NULL
);
GO

-- 6. Enrolments Table
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL FOREIGN KEY REFERENCES Participants(ParticipantId),
    CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId),
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Confirmed' CHECK (Status IN ('Confirmed', 'Cancelled', 'Completed'))
);
GO

-- 7. Results Table
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Enrolments(EnrolmentId),
    FinishTime TIME(3) NULL,
    [Position] INT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Completed', 'Disqualified'))
);
GO

-- Seed Data
INSERT INTO Users (Email, PasswordHash, FullName, Role) VALUES
('organiser1@raceday.com', 'hash1', 'Thabo Mokoena', 'Organiser'),
('organiser2@raceday.com', 'hash2', 'Lindiwe Nkosi', 'Organiser'),
('participant1@raceday.com', 'hash3', 'Sipho Zulu', 'Participant'),
('participant2@raceday.com', 'hash4', 'Zanele Mbeki', 'Participant');
GO

INSERT INTO Organisers (OrganiserId, OrganisationName) VALUES
(1, 'Gauteng Running Club'),
(2, 'Cape Town Cycle Tours');
GO

INSERT INTO Participants (ParticipantId, DateOfBirth, ContactNumber) VALUES
(3, '1990-05-15', '0821234567'),
(4, '1985-11-22', '0839876543');
GO

INSERT INTO Events (OrganiserId, Name, Description, [Date], Location, Status) VALUES
(1, 'Soweto Marathon', 'Iconic race through Soweto.', '2026-11-05 06:00:00', 'Johannesburg', 'Upcoming'),
(1, 'Joburg 10km City Run', 'Fast 10km race in Sandton.', '2026-09-15 07:00:00', 'Sandton', 'Upcoming'),
(2, 'Cape Town Cycle Tour', 'World''s largest timed cycling event.', '2027-03-10 06:30:00', 'Cape Town', 'Upcoming');
GO

INSERT INTO Categories (EventId, Name, Description, EntryFee) VALUES
(1, 'Full Marathon (42.2km)', 'Classic marathon distance.', 350.00),
(1, 'Half Marathon (21.1km)', 'Challenging half marathon.', 250.00),
(1, '10km Fun Run', 'Family fun run.', 150.00),
(2, 'Elite Men', 'Competitive male runners.', 200.00),
(2, 'Elite Women', 'Competitive female runners.', 200.00),
(2, 'Open (All Ages)', 'All other participants.', 150.00),
(3, 'Elite Men', 'Competitive male cyclists.', 500.00),
(3, 'Elite Women', 'Competitive female cyclists.', 500.00),
(3, 'Amateur', 'Recreational cyclists.', 300.00);
GO

INSERT INTO Enrolments (ParticipantId, CategoryId, Status) VALUES
(3, 1, 'Confirmed'),
(3, 4, 'Confirmed'),
(4, 2, 'Confirmed'),
(4, 9, 'Confirmed');
GO
