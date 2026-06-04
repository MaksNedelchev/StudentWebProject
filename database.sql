CREATE DATABASE IF NOT EXISTS StudentManagerProjDB;
USE StudentManagerProjDB;

CREATE TABLE IF NOT EXISTS AppUsers (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(120) NOT NULL,
    FullName VARCHAR(120) NOT NULL,
    PasswordHash LONGTEXT NOT NULL,
    Role INT NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    UNIQUE KEY IX_AppUsers_Email (Email)
);

CREATE TABLE IF NOT EXISTS Teachers (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    AppUserId INT NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Department VARCHAR(100) NOT NULL DEFAULT '',
    UNIQUE KEY IX_Teachers_AppUserId (AppUserId),
    CONSTRAINT FK_Teachers_AppUsers_AppUserId FOREIGN KEY (AppUserId) REFERENCES AppUsers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Students (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    AppUserId INT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    EnrollmentDate DATE NOT NULL,
    UNIQUE KEY IX_Students_AppUserId (AppUserId),
    CONSTRAINT FK_Students_AppUsers_AppUserId FOREIGN KEY (AppUserId) REFERENCES AppUsers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Courses (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    TeacherId INT NULL,
    Title VARCHAR(50) NOT NULL,
    Hours INT NOT NULL,
    KEY IX_Courses_TeacherId (TeacherId),
    CONSTRAINT FK_Courses_Teachers_TeacherId FOREIGN KEY (TeacherId) REFERENCES Teachers(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Enrollments (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    Grade DECIMAL(65,30) NULL,
    UNIQUE KEY IX_Enrollments_StudentId_CourseId (StudentId, CourseId),
    KEY IX_Enrollments_CourseId (CourseId),
    CONSTRAINT FK_Enrollments_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Enrollments_Students_StudentId FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Assignments (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    CourseId INT NOT NULL,
    Title VARCHAR(120) NOT NULL,
    Description VARCHAR(1000) NOT NULL,
    DueDate DATETIME(6) NOT NULL,
    KEY IX_Assignments_CourseId (CourseId),
    CONSTRAINT FK_Assignments_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS AssignmentSubmissions (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    AssignmentId INT NOT NULL,
    StudentId INT NOT NULL,
    Content VARCHAR(2000) NOT NULL,
    SubmittedAt DATETIME(6) NOT NULL,
    Grade DECIMAL(65,30) NULL,
    Feedback VARCHAR(1000) NOT NULL,
    UNIQUE KEY IX_AssignmentSubmissions_AssignmentId_StudentId (AssignmentId, StudentId),
    KEY IX_AssignmentSubmissions_StudentId (StudentId),
    CONSTRAINT FK_AssignmentSubmissions_Assignments_AssignmentId FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AssignmentSubmissions_Students_StudentId FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Absences (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    Date DATE NOT NULL,
    Notes VARCHAR(300) NOT NULL,
    KEY IX_Absences_CourseId (CourseId),
    KEY IX_Absences_StudentId (StudentId),
    CONSTRAINT FK_Absences_Courses_CourseId FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Absences_Students_StudentId FOREIGN KEY (StudentId) REFERENCES Students(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Messages (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    SenderUserId INT NOT NULL,
    RecipientUserId INT NOT NULL,
    Subject VARCHAR(120) NOT NULL,
    Body VARCHAR(2000) NOT NULL,
    SentAt DATETIME(6) NOT NULL,
    IsRead TINYINT(1) NOT NULL DEFAULT 0,
    KEY IX_Messages_SenderUserId (SenderUserId),
    KEY IX_Messages_RecipientUserId (RecipientUserId),
    CONSTRAINT FK_Messages_AppUsers_SenderUserId FOREIGN KEY (SenderUserId) REFERENCES AppUsers(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Messages_AppUsers_RecipientUserId FOREIGN KEY (RecipientUserId) REFERENCES AppUsers(Id) ON DELETE RESTRICT
);
