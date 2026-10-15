-- ============================================
-- RACEDAY DATABASE SCRIPT
-- ============================================

-- Create Database
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ============================================
-- CREATE TABLES
-- ============================================

-- USER Table
-- Stores both Organisers and Participants, distinguished by the role column.
CREATE TABLE [User] (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL CHECK (role IN ('Organiser', 'Participant')),
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- VENUE Table
-- Stores physical locations where events are hosted.
CREATE TABLE Venue (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    address VARCHAR(200),
    city VARCHAR(100) NOT NULL
);
GO

-- EVENT Table
-- A race event created by an Organiser at a specific Venue.
-- Updated for Part 2: event_type (Run, Walk or Cycle) and distance_km were added
-- because the brief requires every event to have a type and a distance.
CREATE TABLE [Event] (
    id INT IDENTITY(1,1) PRIMARY KEY,
    organiser_id INT NOT NULL FOREIGN KEY REFERENCES [User](id),
    venue_id INT NOT NULL FOREIGN KEY REFERENCES Venue(id),
    name VARCHAR(150) NOT NULL,
    event_date DATETIME NOT NULL,
    description VARCHAR(500),
    event_type VARCHAR(20) NOT NULL CHECK (event_type IN ('Run', 'Walk', 'Cycle')),
    distance_km DECIMAL(5,2) NOT NULL
);
GO

-- CATEGORY Table
-- A distance/type category within an Event (e.g. Half Marathon, Fun Ride).
CREATE TABLE Category (
    id INT IDENTITY(1,1) PRIMARY KEY,
    event_id INT NOT NULL FOREIGN KEY REFERENCES [Event](id),
    name VARCHAR(50) NOT NULL,
    distance_km DECIMAL(5,2) NOT NULL
);
GO

-- ENROLMENT Table
-- Links a Participant to a Category they have entered.
CREATE TABLE Enrolment (
    id INT IDENTITY(1,1) PRIMARY KEY,
    participant_id INT NOT NULL FOREIGN KEY REFERENCES [User](id),
    category_id INT NOT NULL FOREIGN KEY REFERENCES Category(id),
    enrolment_date DATETIME DEFAULT GETDATE()
);
GO

-- RESULT Table
-- Stores the finish time and position for a completed Enrolment. One-to-one with Enrolment.
CREATE TABLE [Result] (
    id INT IDENTITY(1,1) PRIMARY KEY,
    enrolment_id INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Enrolment(id),
    finish_time TIME NOT NULL,
    position INT NOT NULL
);
GO

-- ============================================
-- INSERT SAMPLE DATA (SEED DATA)
-- ============================================

-- Insert Users (2 Organisers, 2 Participants)
INSERT INTO [User] (name, email, password_hash, role) VALUES
    ('Thabo Organiser', 'thabo@raceday.co.za', 'hashed_password_123', 'Organiser'),
    ('Zanele Organiser', 'zanele@raceday.co.za', 'hashed_password_456', 'Organiser'),
    ('Sipho Runner', 'sipho@email.com', 'hashed_password_789', 'Participant'),
    ('Lindiwe Walker', 'lindiwe@email.com', 'hashed_password_101', 'Participant');
GO

-- Insert Venues
INSERT INTO Venue (name, address, city) VALUES
    ('Durban ICC', '45 Bram Fischer Road', 'Durban'),
    ('Green Point Stadium', 'Green Point', 'Cape Town'),
    ('FNB Stadium', 'Soccer City, Nasrec', 'Johannesburg');
GO

-- Insert Events
INSERT INTO [Event] (organiser_id, venue_id, name, event_date, description, event_type, distance_km) VALUES
    (1, 1, 'Durban City Marathon', '2026-10-15 06:00:00', 'A scenic marathon along the Durban beachfront.', 'Run', 42.2),
    (2, 2, 'Cape Town Cycle Tour', '2026-11-20 07:00:00', 'The world''s largest timed cycle race around the Cape Peninsula.', 'Cycle', 109.0),
    (1, 3, 'Soweto Marathon', '2026-12-01 05:30:00', 'Iconic marathon through the streets of Soweto.', 'Run', 42.2);
GO

-- Insert Categories
INSERT INTO Category (event_id, name, distance_km) VALUES
    (1, 'Full Marathon (42.2km)', 42.2),
    (1, 'Half Marathon (21.1km)', 21.1),
    (1, '10km Fun Run', 10.0),
    (2, 'Elite (109km)', 109.0),
    (2, 'Amateur (109km)', 109.0),
    (2, 'Fun Ride (42km)', 42.0),
    (3, 'Full Marathon (42.2km)', 42.2),
    (3, 'Half Marathon (21.1km)', 21.1);
GO

-- Insert Enrolments
INSERT INTO Enrolment (participant_id, category_id) VALUES
    (3, 2), -- Sipho in Durban Half Marathon
    (3, 5), -- Sipho in Cape Town Amateur
    (4, 1), -- Lindiwe in Durban Full Marathon
    (4, 8); -- Lindiwe in Soweto Half Marathon
GO

-- Insert Results
INSERT INTO [Result] (enrolment_id, finish_time, position) VALUES
    (1, '01:45:30', 15),
    (3, '04:12:45', 22);
GO

-- View all data to verify
SELECT * FROM [User];
SELECT * FROM Venue;
SELECT * FROM [Event];
SELECT * FROM Category;
SELECT * FROM Enrolment;
SELECT * FROM [Result];
GO
