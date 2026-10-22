

BEGIN TRANSACTION;
GO

CREATE TABLE [AcademicYear] (
    [Id] bigint NOT NULL IDENTITY,
    [YearCode] nvarchar(30) NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    CONSTRAINT [PK_AcademicYear] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AcademicYear_Date] CHECK ([EndDate] >= [StartDate])
);
GO

CREATE TABLE [AssessmentType] (
    [Id] bigint NOT NULL IDENTITY,
    [TypeCode] nvarchar(50) NOT NULL,
    [TypeName] nvarchar(100) NOT NULL,
    [Weight] decimal(5,2) NOT NULL,
    [MinEntries] int NOT NULL DEFAULT 1,
    [MaxEntries] int NULL,
    CONSTRAINT [PK_AssessmentType] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AssessmentType_Entries] CHECK ([MinEntries] >= 0 AND ([MaxEntries] IS NULL OR [MaxEntries] >= [MinEntries])),
    CONSTRAINT [CK_AssessmentType_Weight] CHECK ([Weight] >= 0 AND [Weight] <= 100)
);
GO

CREATE TABLE [Customer] (
    [Id] bigint NOT NULL IDENTITY,
    [Username] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [PhoneNumber] nvarchar(30) NULL,
    [Gender] nvarchar(20) NULL,
    [DateOfBirth] date NULL,
    [Address] nvarchar(500) NULL,
    [AvatarUrl] nvarchar(1000) NULL,
    [StudentCode] nvarchar(50) NULL,
    [AdmissionDate] date NULL,
    [Occupation] nvarchar(150) NULL,
    [EmergencyContact] nvarchar(255) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Employee] (
    [Id] bigint NOT NULL IDENTITY,
    [EmployeeCode] nvarchar(50) NOT NULL,
    [Username] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [PhoneNumber] nvarchar(30) NULL,
    [Gender] nvarchar(20) NULL,
    [DateOfBirth] date NULL,
    [Address] nvarchar(500) NULL,
    [AvatarUrl] nvarchar(1000) NULL,
    [Department] nvarchar(150) NULL,
    [Position] nvarchar(150) NULL,
    [Specialization] nvarchar(255) NULL,
    [MaxPeriodsPerWeek] int NULL,
    [Qualification] nvarchar(255) NULL,
    [HireDate] date NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Employee] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Employee_MaxPeriodsPerWeek] CHECK ([MaxPeriodsPerWeek] IS NULL OR [MaxPeriodsPerWeek] >= 0)
);
GO

CREATE TABLE [GradeLevel] (
    [Id] bigint NOT NULL IDENTITY,
    [LevelCode] nvarchar(30) NOT NULL,
    [LevelName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_GradeLevel] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Role] (
    [Id] bigint NOT NULL IDENTITY,
    [RoleCode] varchar(50) NOT NULL,
    [RoleName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Room] (
    [Id] bigint NOT NULL IDENTITY,
    [RoomCode] nvarchar(50) NOT NULL,
    [RoomName] nvarchar(150) NULL,
    [Building] nvarchar(150) NULL,
    [RoomType] nvarchar(100) NULL,
    [Capacity] int NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
    CONSTRAINT [PK_Room] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Room_Capacity] CHECK ([Capacity] IS NULL OR [Capacity] >= 0)
);
GO

CREATE TABLE [Subject] (
    [Id] bigint NOT NULL IDENTITY,
    [SubjectCode] nvarchar(50) NOT NULL,
    [SubjectName] nvarchar(150) NOT NULL,
    [EvaluationType] nvarchar(50) NULL,
    CONSTRAINT [PK_Subject] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [TimeSlot] (
    [Id] bigint NOT NULL IDENTITY,
    [SlotCode] nvarchar(50) NOT NULL,
    [PeriodNo] int NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Session] nvarchar(30) NULL,
    CONSTRAINT [PK_TimeSlot] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_TimeSlot_PeriodNo] CHECK ([PeriodNo] > 0),
    CONSTRAINT [CK_TimeSlot_Time] CHECK ([EndTime] > [StartTime])
);
GO

CREATE TABLE [Semester] (
    [Id] bigint NOT NULL IDENTITY,
    [SemesterCode] nvarchar(30) NOT NULL,
    [SemesterName] nvarchar(100) NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [AcademicYearId] bigint NOT NULL,
    CONSTRAINT [PK_Semester] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Semester_Date] CHECK ([EndDate] >= [StartDate]),
    CONSTRAINT [FK_Semester_AcademicYear] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYear] ([Id])
);
GO

CREATE TABLE [CustomerRelationship] (
    [Id] bigint NOT NULL IDENTITY,
    [ParentCustomerId] bigint NOT NULL,
    [StudentCustomerId] bigint NOT NULL,
    [RelationshipType] nvarchar(50) NULL,
    [IsPrimaryGuardian] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_CustomerRelationship] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CustomerRelationship_SameCustomer] CHECK ([ParentCustomerId] <> [StudentCustomerId]),
    CONSTRAINT [FK_CustomerRelationship_Parent] FOREIGN KEY ([ParentCustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_CustomerRelationship_Student] FOREIGN KEY ([StudentCustomerId]) REFERENCES [Customer] ([Id])
);
GO

CREATE TABLE [LeaveRequest] (
    [Id] bigint NOT NULL IDENTITY,
    [StudentCustomerId] bigint NOT NULL,
    [RequestedByCustomerId] bigint NOT NULL,
    [Reason] nvarchar(1000) NOT NULL,
    [Attachment] nvarchar(1000) NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'PENDING',
    [RequestedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    [ReviewNote] nvarchar(1000) NULL,
    [ReviewedByEmployeeId] bigint NULL,
    [ReviewedAt] datetime2 NULL,
    CONSTRAINT [PK_LeaveRequest] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_LeaveRequest_Date] CHECK ([EndDate] >= [StartDate]),
    CONSTRAINT [FK_LeaveRequest_RequestedBy] FOREIGN KEY ([RequestedByCustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_LeaveRequest_ReviewedBy] FOREIGN KEY ([ReviewedByEmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_LeaveRequest_Student] FOREIGN KEY ([StudentCustomerId]) REFERENCES [Customer] ([Id])
);
GO

CREATE TABLE [Notification] (
    [Id] bigint NOT NULL IDENTITY,
    [SenderCustomerId] bigint NULL,
    [SenderEmployeeId] bigint NULL,
    [RecipientCustomerId] bigint NULL,
    [RecipientEmployeeId] bigint NULL,
    [Title] nvarchar(255) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
    [SentAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    [ReadAt] datetime2 NULL,
    CONSTRAINT [PK_Notification] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Notification_Recipient] CHECK (([RecipientCustomerId] IS NOT NULL AND [RecipientEmployeeId] IS NULL) OR ([RecipientCustomerId] IS NULL AND [RecipientEmployeeId] IS NOT NULL)),
    CONSTRAINT [CK_Notification_Sender] CHECK ([SenderCustomerId] IS NULL OR [SenderEmployeeId] IS NULL),
    CONSTRAINT [FK_Notification_RecipientCustomer] FOREIGN KEY ([RecipientCustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_Notification_RecipientEmployee] FOREIGN KEY ([RecipientEmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_Notification_SenderCustomer] FOREIGN KEY ([SenderCustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_Notification_SenderEmployee] FOREIGN KEY ([SenderEmployeeId]) REFERENCES [Employee] ([Id])
);
GO

CREATE TABLE [Class] (
    [Id] bigint NOT NULL IDENTITY,
    [ClassName] nvarchar(100) NOT NULL,
    [HomeroomEmployeeId] bigint NULL,
    [AcademicYearId] bigint NOT NULL,
    [GradeLevelId] bigint NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
    CONSTRAINT [PK_Class] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Class_AcademicYear] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYear] ([Id]),
    CONSTRAINT [FK_Class_GradeLevel] FOREIGN KEY ([GradeLevelId]) REFERENCES [GradeLevel] ([Id]),
    CONSTRAINT [FK_Class_HomeroomEmployee] FOREIGN KEY ([HomeroomEmployeeId]) REFERENCES [Employee] ([Id])
);
GO

CREATE TABLE [CustomerRole] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [RoleId] bigint NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [AssignedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_CustomerRole] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerRole_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CustomerRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id])
);
GO

CREATE TABLE [EmployeeRole] (
    [Id] bigint NOT NULL IDENTITY,
    [EmployeeId] bigint NOT NULL,
    [RoleId] bigint NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [AssignedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_EmployeeRole] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmployeeRole_Employee] FOREIGN KEY ([EmployeeId]) REFERENCES [Employee] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EmployeeRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id])
);
GO

CREATE TABLE [ConductEvaluation] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [SemesterId] bigint NOT NULL,
    [HomeroomEmployeeId] bigint NOT NULL,
    [ConductGrade] nvarchar(50) NOT NULL,
    [TeacherComment] nvarchar(1000) NULL,
    [EvaluatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_ConductEvaluation] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConductEvaluation_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_ConductEvaluation_Employee] FOREIGN KEY ([HomeroomEmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_ConductEvaluation_Semester] FOREIGN KEY ([SemesterId]) REFERENCES [Semester] ([Id])
);
GO

CREATE TABLE [SemesterReport] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [SemesterId] bigint NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'DRAFT',
    [FinalizedByEmployeeId] bigint NULL,
    [FinalizedAt] datetime2 NULL,
    [GPA] decimal(5,2) NULL,
    [AcademicRank] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_SemesterReport] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SemesterReport_GPA] CHECK ([GPA] IS NULL OR ([GPA] >= 0 AND [GPA] <= 10)),
    CONSTRAINT [FK_SemesterReport_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_SemesterReport_FinalizedEmployee] FOREIGN KEY ([FinalizedByEmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_SemesterReport_Semester] FOREIGN KEY ([SemesterId]) REFERENCES [Semester] ([Id])
);
GO

CREATE TABLE [StudentClass] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [ClassId] bigint NOT NULL,
    [JoinedAt] date NOT NULL,
    [LeftAt] date NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
    CONSTRAINT [PK_StudentClass] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StudentClass_Date] CHECK ([LeftAt] IS NULL OR [LeftAt] >= [JoinedAt]),
    CONSTRAINT [FK_StudentClass_Class] FOREIGN KEY ([ClassId]) REFERENCES [Class] ([Id]),
    CONSTRAINT [FK_StudentClass_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id])
);
GO

CREATE TABLE [TeachingAssignment] (
    [Id] bigint NOT NULL IDENTITY,
    [EmployeeId] bigint NOT NULL,
    [SubjectId] bigint NOT NULL,
    [ClassId] bigint NOT NULL,
    [SemesterId] bigint NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
    CONSTRAINT [PK_TeachingAssignment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TeachingAssignment_Class] FOREIGN KEY ([ClassId]) REFERENCES [Class] ([Id]),
    CONSTRAINT [FK_TeachingAssignment_Employee] FOREIGN KEY ([EmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_TeachingAssignment_Semester] FOREIGN KEY ([SemesterId]) REFERENCES [Semester] ([Id]),
    CONSTRAINT [FK_TeachingAssignment_Subject] FOREIGN KEY ([SubjectId]) REFERENCES [Subject] ([Id])
);
GO

CREATE TABLE [SemesterSubjectResult] (
    [Id] bigint NOT NULL IDENTITY,
    [SemesterReportId] bigint NOT NULL,
    [SubjectId] bigint NOT NULL,
    [AverageScore] decimal(5,2) NULL,
    [EvaluationResult] nvarchar(100) NULL,
    CONSTRAINT [PK_SemesterSubjectResult] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_SemesterSubjectResult_Score] CHECK ([AverageScore] IS NULL OR ([AverageScore] >= 0 AND [AverageScore] <= 10)),
    CONSTRAINT [FK_SemesterSubjectResult_Report] FOREIGN KEY ([SemesterReportId]) REFERENCES [SemesterReport] ([Id]),
    CONSTRAINT [FK_SemesterSubjectResult_Subject] FOREIGN KEY ([SubjectId]) REFERENCES [Subject] ([Id])
);
GO

CREATE TABLE [Assessment] (
    [Id] bigint NOT NULL IDENTITY,
    [TeachingAssignmentId] bigint NOT NULL,
    [AssessmentTypeId] bigint NOT NULL,
    [AssessmentName] nvarchar(200) NOT NULL,
    [ConductedAt] datetime2 NULL,
    CONSTRAINT [PK_Assessment] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Assessment_AssessmentType] FOREIGN KEY ([AssessmentTypeId]) REFERENCES [AssessmentType] ([Id]),
    CONSTRAINT [FK_Assessment_TeachingAssignment] FOREIGN KEY ([TeachingAssignmentId]) REFERENCES [TeachingAssignment] ([Id])
);
GO

CREATE TABLE [TeacherFeedback] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [TeachingAssignmentId] bigint NOT NULL,
    [FeedbackType] nvarchar(100) NULL,
    [Comment] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_TeacherFeedback] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TeacherFeedback_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_TeacherFeedback_TeachingAssignment] FOREIGN KEY ([TeachingAssignmentId]) REFERENCES [TeachingAssignment] ([Id])
);
GO

CREATE TABLE [TimetableEntry] (
    [Id] bigint NOT NULL IDENTITY,
    [TeachingAssignmentId] bigint NOT NULL,
    [RoomId] bigint NOT NULL,
    [TimeSlotId] bigint NOT NULL,
    [DayOfWeek] tinyint NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'ACTIVE',
    [EffectiveFrom] date NULL,
    [EffectiveTo] date NULL,
    CONSTRAINT [PK_TimetableEntry] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_TimetableEntry_DayOfWeek] CHECK ([DayOfWeek] BETWEEN 1 AND 7),
    CONSTRAINT [CK_TimetableEntry_EffectiveDate] CHECK ([EffectiveFrom] IS NULL OR [EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]),
    CONSTRAINT [FK_TimetableEntry_Room] FOREIGN KEY ([RoomId]) REFERENCES [Room] ([Id]),
    CONSTRAINT [FK_TimetableEntry_TeachingAssignment] FOREIGN KEY ([TeachingAssignmentId]) REFERENCES [TeachingAssignment] ([Id]),
    CONSTRAINT [FK_TimetableEntry_TimeSlot] FOREIGN KEY ([TimeSlotId]) REFERENCES [TimeSlot] ([Id])
);
GO

CREATE TABLE [StudentScore] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [AssessmentId] bigint NOT NULL,
    [Score] decimal(5,2) NULL,
    [Comment] nvarchar(1000) NULL,
    [IsLocked] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_StudentScore] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_StudentScore_Score] CHECK ([Score] IS NULL OR ([Score] >= 0 AND [Score] <= 10)),
    CONSTRAINT [FK_StudentScore_Assessment] FOREIGN KEY ([AssessmentId]) REFERENCES [Assessment] ([Id]),
    CONSTRAINT [FK_StudentScore_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id])
);
GO

CREATE TABLE [AttendanceRecord] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [RecordedByEmployeeId] bigint NOT NULL,
    [LeaveRequestId] bigint NULL,
    [TimetableEntryId] bigint NOT NULL,
    [AttendanceDate] date NOT NULL,
    [Status] nvarchar(30) NOT NULL,
    [Comment] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_AttendanceRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AttendanceRecord_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_AttendanceRecord_Employee] FOREIGN KEY ([RecordedByEmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_AttendanceRecord_LeaveRequest] FOREIGN KEY ([LeaveRequestId]) REFERENCES [LeaveRequest] ([Id]),
    CONSTRAINT [FK_AttendanceRecord_TimetableEntry] FOREIGN KEY ([TimetableEntryId]) REFERENCES [TimetableEntry] ([Id])
);
GO

CREATE TABLE [ScoreAuditLog] (
    [Id] bigint NOT NULL IDENTITY,
    [StudentScoreId] bigint NOT NULL,
    [OldScore] decimal(5,2) NULL,
    [NewScore] decimal(5,2) NULL,
    [ChangeReason] nvarchar(1000) NULL,
    [ChangedByEmployeeId] bigint NOT NULL,
    [IpAddress] varchar(45) NULL,
    [ChangedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_ScoreAuditLog] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ScoreAuditLog_Employee] FOREIGN KEY ([ChangedByEmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_ScoreAuditLog_StudentScore] FOREIGN KEY ([StudentScoreId]) REFERENCES [StudentScore] ([Id])
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsActive', N'RoleCode', N'RoleName') AND [object_id] = OBJECT_ID(N'[Role]'))
    SET IDENTITY_INSERT [Role] ON;
INSERT INTO [Role] ([Id], [CreatedAt], [Description], [IsActive], [RoleCode], [RoleName])
VALUES (CAST(1 AS bigint), '2026-01-01T00:00:00.0000000Z', N'High school student', CAST(1 AS bit), 'STUDENT', N'Student'),
(CAST(2 AS bigint), '2026-01-01T00:00:00.0000000Z', N'Parent or guardian of student', CAST(1 AS bit), 'PARENT', N'Parent'),
(CAST(3 AS bigint), '2026-01-01T00:00:00.0000000Z', N'School teacher', CAST(1 AS bit), 'TEACHER', N'Teacher'),
(CAST(4 AS bigint), '2026-01-01T00:00:00.0000000Z', N'System administrator', CAST(1 AS bit), 'ADMIN', N'Administrator'),
(CAST(5 AS bigint), '2026-01-01T00:00:00.0000000Z', N'School staff', CAST(1 AS bit), 'STAFF', N'Staff');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsActive', N'RoleCode', N'RoleName') AND [object_id] = OBJECT_ID(N'[Role]'))
    SET IDENTITY_INSERT [Role] OFF;
GO

CREATE UNIQUE INDEX [UQ_AcademicYear_YearCode] ON [AcademicYear] ([YearCode]);
GO

CREATE INDEX [IX_Assessment_Assignment] ON [Assessment] ([TeachingAssignmentId]);
GO

CREATE INDEX [IX_Assessment_Type] ON [Assessment] ([AssessmentTypeId]);
GO

CREATE UNIQUE INDEX [UQ_AssessmentType_TypeCode] ON [AssessmentType] ([TypeCode]);
GO

CREATE INDEX [IX_AttendanceRecord_Customer] ON [AttendanceRecord] ([CustomerId]);
GO

CREATE INDEX [IX_AttendanceRecord_Employee] ON [AttendanceRecord] ([RecordedByEmployeeId]);
GO

CREATE INDEX [IX_AttendanceRecord_LeaveRequestId] ON [AttendanceRecord] ([LeaveRequestId]);
GO

CREATE INDEX [IX_AttendanceRecord_Timetable] ON [AttendanceRecord] ([TimetableEntryId]);
GO

CREATE UNIQUE INDEX [UQ_AttendanceRecord] ON [AttendanceRecord] ([CustomerId], [TimetableEntryId], [AttendanceDate]);
GO

CREATE INDEX [IX_Class_AcademicYear] ON [Class] ([AcademicYearId]);
GO

CREATE INDEX [IX_Class_GradeLevel] ON [Class] ([GradeLevelId]);
GO

CREATE INDEX [IX_Class_HomeroomEmployee] ON [Class] ([HomeroomEmployeeId]);
GO

CREATE INDEX [IX_ConductEvaluation_Customer] ON [ConductEvaluation] ([CustomerId]);
GO

CREATE INDEX [IX_ConductEvaluation_HomeroomEmployeeId] ON [ConductEvaluation] ([HomeroomEmployeeId]);
GO

CREATE INDEX [IX_ConductEvaluation_Semester] ON [ConductEvaluation] ([SemesterId]);
GO

CREATE UNIQUE INDEX [UQ_ConductEvaluation] ON [ConductEvaluation] ([CustomerId], [SemesterId]);
GO

CREATE UNIQUE INDEX [UQ_Customer_Email] ON [Customer] ([Email]);
GO

CREATE UNIQUE INDEX [UQ_Customer_Username] ON [Customer] ([Username]);
GO

CREATE UNIQUE INDEX [UX_Customer_StudentCode] ON [Customer] ([StudentCode]) WHERE [StudentCode] IS NOT NULL;
GO

CREATE INDEX [IX_CustomerRelationship_Parent] ON [CustomerRelationship] ([ParentCustomerId]);
GO

CREATE INDEX [IX_CustomerRelationship_Student] ON [CustomerRelationship] ([StudentCustomerId]);
GO

CREATE UNIQUE INDEX [UQ_CustomerRelationship] ON [CustomerRelationship] ([ParentCustomerId], [StudentCustomerId]);
GO

CREATE INDEX [IX_CustomerRole_CustomerId] ON [CustomerRole] ([CustomerId]);
GO

CREATE INDEX [IX_CustomerRole_RoleId] ON [CustomerRole] ([RoleId]);
GO

CREATE UNIQUE INDEX [UQ_CustomerRole] ON [CustomerRole] ([CustomerId], [RoleId]);
GO

CREATE UNIQUE INDEX [UQ_Employee_Email] ON [Employee] ([Email]);
GO

CREATE UNIQUE INDEX [UQ_Employee_EmployeeCode] ON [Employee] ([EmployeeCode]);
GO

CREATE UNIQUE INDEX [UQ_Employee_Username] ON [Employee] ([Username]);
GO

CREATE INDEX [IX_EmployeeRole_EmployeeId] ON [EmployeeRole] ([EmployeeId]);
GO

CREATE INDEX [IX_EmployeeRole_RoleId] ON [EmployeeRole] ([RoleId]);
GO

CREATE UNIQUE INDEX [UQ_EmployeeRole] ON [EmployeeRole] ([EmployeeId], [RoleId]);
GO

CREATE UNIQUE INDEX [UQ_GradeLevel_LevelCode] ON [GradeLevel] ([LevelCode]);
GO

CREATE INDEX [IX_LeaveRequest_RequestedBy] ON [LeaveRequest] ([RequestedByCustomerId]);
GO

CREATE INDEX [IX_LeaveRequest_ReviewedBy] ON [LeaveRequest] ([ReviewedByEmployeeId]);
GO

CREATE INDEX [IX_LeaveRequest_Student] ON [LeaveRequest] ([StudentCustomerId]);
GO

CREATE INDEX [IX_Notification_IsRead] ON [Notification] ([IsRead]);
GO

CREATE INDEX [IX_Notification_RecipientCustomer] ON [Notification] ([RecipientCustomerId]);
GO

CREATE INDEX [IX_Notification_RecipientEmployee] ON [Notification] ([RecipientEmployeeId]);
GO

CREATE INDEX [IX_Notification_SenderCustomerId] ON [Notification] ([SenderCustomerId]);
GO

CREATE INDEX [IX_Notification_SenderEmployeeId] ON [Notification] ([SenderEmployeeId]);
GO

CREATE UNIQUE INDEX [UQ_Role_RoleCode] ON [Role] ([RoleCode]);
GO

CREATE UNIQUE INDEX [UQ_Room_RoomCode] ON [Room] ([RoomCode]);
GO

CREATE INDEX [IX_ScoreAuditLog_Employee] ON [ScoreAuditLog] ([ChangedByEmployeeId]);
GO

CREATE INDEX [IX_ScoreAuditLog_StudentScore] ON [ScoreAuditLog] ([StudentScoreId]);
GO

CREATE INDEX [IX_Semester_AcademicYear] ON [Semester] ([AcademicYearId]);
GO

CREATE UNIQUE INDEX [UQ_Semester_SemesterCode] ON [Semester] ([SemesterCode]);
GO

CREATE INDEX [IX_SemesterReport_Customer] ON [SemesterReport] ([CustomerId]);
GO

CREATE INDEX [IX_SemesterReport_FinalizedByEmployeeId] ON [SemesterReport] ([FinalizedByEmployeeId]);
GO

CREATE INDEX [IX_SemesterReport_Semester] ON [SemesterReport] ([SemesterId]);
GO

CREATE UNIQUE INDEX [UQ_SemesterReport] ON [SemesterReport] ([CustomerId], [SemesterId]);
GO

CREATE INDEX [IX_SemesterSubjectResult_Report] ON [SemesterSubjectResult] ([SemesterReportId]);
GO

CREATE INDEX [IX_SemesterSubjectResult_SubjectId] ON [SemesterSubjectResult] ([SubjectId]);
GO

CREATE UNIQUE INDEX [UQ_SemesterSubjectResult] ON [SemesterSubjectResult] ([SemesterReportId], [SubjectId]);
GO

CREATE INDEX [IX_StudentClass_Class] ON [StudentClass] ([ClassId]);
GO

CREATE INDEX [IX_StudentClass_Customer] ON [StudentClass] ([CustomerId]);
GO

CREATE INDEX [IX_StudentScore_Assessment] ON [StudentScore] ([AssessmentId]);
GO

CREATE INDEX [IX_StudentScore_Customer] ON [StudentScore] ([CustomerId]);
GO

CREATE UNIQUE INDEX [UQ_StudentScore] ON [StudentScore] ([CustomerId], [AssessmentId]);
GO

CREATE UNIQUE INDEX [UQ_Subject_SubjectCode] ON [Subject] ([SubjectCode]);
GO

CREATE INDEX [IX_TeacherFeedback_Customer] ON [TeacherFeedback] ([CustomerId]);
GO

CREATE INDEX [IX_TeacherFeedback_TeachingAssignmentId] ON [TeacherFeedback] ([TeachingAssignmentId]);
GO

CREATE INDEX [IX_TeachingAssignment_Class] ON [TeachingAssignment] ([ClassId]);
GO

CREATE INDEX [IX_TeachingAssignment_Employee] ON [TeachingAssignment] ([EmployeeId]);
GO

CREATE INDEX [IX_TeachingAssignment_Semester] ON [TeachingAssignment] ([SemesterId]);
GO

CREATE INDEX [IX_TeachingAssignment_Subject] ON [TeachingAssignment] ([SubjectId]);
GO

CREATE UNIQUE INDEX [UQ_TeachingAssignment] ON [TeachingAssignment] ([EmployeeId], [SubjectId], [ClassId], [SemesterId]);
GO

CREATE UNIQUE INDEX [UQ_TimeSlot_SlotCode] ON [TimeSlot] ([SlotCode]);
GO

CREATE INDEX [IX_TimetableEntry_Assignment] ON [TimetableEntry] ([TeachingAssignmentId]);
GO

CREATE INDEX [IX_TimetableEntry_Room] ON [TimetableEntry] ([RoomId]);
GO

CREATE INDEX [IX_TimetableEntry_TimeSlot] ON [TimetableEntry] ([TimeSlotId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261010063339_InitDb', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [CustomerRelationship];
GO

ALTER TABLE [Customer] ADD [ParentId] bigint NULL;
GO

ALTER TABLE [Customer] ADD [RelationshipType] nvarchar(50) NULL;
GO

CREATE INDEX [IX_Customer_ParentId] ON [Customer] ([ParentId]);
GO

ALTER TABLE [Customer] ADD CONSTRAINT [CK_Customer_ParentNotSelf] CHECK ([ParentId] IS NULL OR [ParentId] <> [Id]);
GO

ALTER TABLE [Customer] ADD CONSTRAINT [FK_Customer_Parent] FOREIGN KEY ([ParentId]) REFERENCES [Customer] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261014072131_Modify-Model-Customer', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [MessageLog] (
    [Id] bigint NOT NULL IDENTITY,
    [Channel] nvarchar(20) NOT NULL,
    [Provider] nvarchar(50) NULL,
    [RecipientCustomerId] bigint NULL,
    [RecipientEmployeeId] bigint NULL,
    [RecipientAddress] nvarchar(255) NOT NULL,
    [Subject] nvarchar(255) NULL,
    [MessageBody] nvarchar(max) NOT NULL,
    [TemplateCode] nvarchar(50) NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'QUEUED',
    [ExternalMessageId] nvarchar(255) NULL,
    [ErrorMessage] nvarchar(1000) NULL,
    [RetryCount] int NOT NULL DEFAULT 0,
    [Cost] decimal(10,2) NULL,
    [SentAt] datetime2 NULL,
    [DeliveredAt] datetime2 NULL,
    [CreatedByEmployeeId] bigint NULL,
    [NotificationId] bigint NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_MessageLog] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_MessageLog_Recipient] CHECK ([RecipientCustomerId] IS NULL OR [RecipientEmployeeId] IS NULL),
    CONSTRAINT [FK_MessageLog_CreatedByEmployee] FOREIGN KEY ([CreatedByEmployeeId]) REFERENCES [Employee] ([Id]),
    CONSTRAINT [FK_MessageLog_Notification] FOREIGN KEY ([NotificationId]) REFERENCES [Notification] ([Id]),
    CONSTRAINT [FK_MessageLog_RecipientCustomer] FOREIGN KEY ([RecipientCustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_MessageLog_RecipientEmployee] FOREIGN KEY ([RecipientEmployeeId]) REFERENCES [Employee] ([Id])
);
GO

CREATE INDEX [IX_MessageLog_Channel_Status] ON [MessageLog] ([Channel], [Status]);
GO

CREATE INDEX [IX_MessageLog_CreatedAt] ON [MessageLog] ([CreatedAt]);
GO

CREATE INDEX [IX_MessageLog_CreatedByEmployee] ON [MessageLog] ([CreatedByEmployeeId]);
GO

CREATE INDEX [IX_MessageLog_Notification] ON [MessageLog] ([NotificationId]);
GO

CREATE INDEX [IX_MessageLog_RecipientAddress] ON [MessageLog] ([RecipientAddress]);
GO

CREATE INDEX [IX_MessageLog_RecipientCustomer] ON [MessageLog] ([RecipientCustomerId]);
GO

CREATE INDEX [IX_MessageLog_RecipientEmployee] ON [MessageLog] ([RecipientEmployeeId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261021050908_Add-model-MessageLog', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ConductEvaluation] ADD [AbsentDays] int NULL;
GO

ALTER TABLE [ConductEvaluation] ADD [AwardTitle] nvarchar(200) NULL;
GO

ALTER TABLE [ConductEvaluation] ADD [DisciplinaryAction] nvarchar(500) NULL;
GO

ALTER TABLE [ConductEvaluation] ADD [ExcusedAbsentDays] int NULL;
GO

ALTER TABLE [ConductEvaluation] ADD [TotalPoints] int NULL;
GO

CREATE TABLE [AnnualTranscript] (
    [Id] bigint NOT NULL IDENTITY,
    [CustomerId] bigint NOT NULL,
    [AcademicYearId] bigint NOT NULL,
    [AnnualGPA] decimal(5,2) NULL,
    [AcademicRank] nvarchar(50) NULL,
    [ConductRank] nvarchar(50) NULL,
    [PromotionStatus] nvarchar(30) NOT NULL DEFAULT N'PENDING',
    [HomeroomComment] nvarchar(2000) NULL,
    [PrincipalComment] nvarchar(2000) NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'DRAFT',
    [FinalizedByEmployeeId] bigint NULL,
    [FinalizedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_AnnualTranscript] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AnnualTranscript_GPA] CHECK ([AnnualGPA] IS NULL OR ([AnnualGPA] >= 0 AND [AnnualGPA] <= 10)),
    CONSTRAINT [FK_AnnualTranscript_AcademicYear] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYear] ([Id]),
    CONSTRAINT [FK_AnnualTranscript_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]),
    CONSTRAINT [FK_AnnualTranscript_FinalizedEmployee] FOREIGN KEY ([FinalizedByEmployeeId]) REFERENCES [Employee] ([Id])
);
GO

CREATE TABLE [ConductCriteria] (
    [Id] bigint NOT NULL IDENTITY,
    [CriteriaCode] varchar(50) NOT NULL,
    [CriteriaName] nvarchar(200) NOT NULL,
    [MaxPoints] int NOT NULL,
    [Category] nvarchar(100) NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
    CONSTRAINT [PK_ConductCriteria] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConductCriteria_MaxPoints] CHECK ([MaxPoints] > 0),
    CONSTRAINT [CK_ConductCriteria_SortOrder] CHECK ([SortOrder] >= 0)
);
GO

CREATE TABLE [AnnualSubjectResult] (
    [Id] bigint NOT NULL IDENTITY,
    [AnnualTranscriptId] bigint NOT NULL,
    [SubjectId] bigint NOT NULL,
    [Semester1Average] decimal(5,2) NULL,
    [Semester2Average] decimal(5,2) NULL,
    [AnnualAverage] decimal(5,2) NULL,
    [EvaluationResult] nvarchar(100) NULL,
    CONSTRAINT [PK_AnnualSubjectResult] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_AnnualSubjectResult_Annual] CHECK ([AnnualAverage] IS NULL OR ([AnnualAverage] >= 0 AND [AnnualAverage] <= 10)),
    CONSTRAINT [CK_AnnualSubjectResult_S1] CHECK ([Semester1Average] IS NULL OR ([Semester1Average] >= 0 AND [Semester1Average] <= 10)),
    CONSTRAINT [CK_AnnualSubjectResult_S2] CHECK ([Semester2Average] IS NULL OR ([Semester2Average] >= 0 AND [Semester2Average] <= 10)),
    CONSTRAINT [FK_AnnualSubjectResult_Subject] FOREIGN KEY ([SubjectId]) REFERENCES [Subject] ([Id]),
    CONSTRAINT [FK_AnnualSubjectResult_Transcript] FOREIGN KEY ([AnnualTranscriptId]) REFERENCES [AnnualTranscript] ([Id])
);
GO

CREATE TABLE [ConductCriteriaScore] (
    [Id] bigint NOT NULL IDENTITY,
    [ConductEvaluationId] bigint NOT NULL,
    [ConductCriteriaId] bigint NOT NULL,
    [Score] int NOT NULL,
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_ConductCriteriaScore] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ConductCriteriaScore_Score] CHECK ([Score] >= 0),
    CONSTRAINT [FK_ConductCriteriaScore_Criteria] FOREIGN KEY ([ConductCriteriaId]) REFERENCES [ConductCriteria] ([Id]),
    CONSTRAINT [FK_ConductCriteriaScore_Evaluation] FOREIGN KEY ([ConductEvaluationId]) REFERENCES [ConductEvaluation] ([Id]) ON DELETE CASCADE
);
GO

ALTER TABLE [ConductEvaluation] ADD CONSTRAINT [CK_ConductEvaluation_AbsentDays] CHECK ([AbsentDays] IS NULL OR [AbsentDays] >= 0);
GO

ALTER TABLE [ConductEvaluation] ADD CONSTRAINT [CK_ConductEvaluation_ExcusedAbsentDays] CHECK ([ExcusedAbsentDays] IS NULL OR [ExcusedAbsentDays] >= 0);
GO

ALTER TABLE [ConductEvaluation] ADD CONSTRAINT [CK_ConductEvaluation_TotalPoints] CHECK ([TotalPoints] IS NULL OR [TotalPoints] >= 0);
GO

CREATE INDEX [IX_AnnualSubjectResult_SubjectId] ON [AnnualSubjectResult] ([SubjectId]);
GO

CREATE INDEX [IX_AnnualSubjectResult_Transcript] ON [AnnualSubjectResult] ([AnnualTranscriptId]);
GO

CREATE UNIQUE INDEX [UQ_AnnualSubjectResult] ON [AnnualSubjectResult] ([AnnualTranscriptId], [SubjectId]);
GO

CREATE INDEX [IX_AnnualTranscript_AcademicYear] ON [AnnualTranscript] ([AcademicYearId]);
GO

CREATE INDEX [IX_AnnualTranscript_Customer] ON [AnnualTranscript] ([CustomerId]);
GO

CREATE INDEX [IX_AnnualTranscript_FinalizedByEmployeeId] ON [AnnualTranscript] ([FinalizedByEmployeeId]);
GO

CREATE UNIQUE INDEX [UQ_AnnualTranscript] ON [AnnualTranscript] ([CustomerId], [AcademicYearId]);
GO

CREATE UNIQUE INDEX [UQ_ConductCriteria_CriteriaCode] ON [ConductCriteria] ([CriteriaCode]);
GO

CREATE INDEX [IX_ConductCriteriaScore_Criteria] ON [ConductCriteriaScore] ([ConductCriteriaId]);
GO

CREATE INDEX [IX_ConductCriteriaScore_Evaluation] ON [ConductCriteriaScore] ([ConductEvaluationId]);
GO

CREATE UNIQUE INDEX [UQ_ConductCriteriaScore] ON [ConductCriteriaScore] ([ConductEvaluationId], [ConductCriteriaId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261022070537_Add_Transcript_And_Conduct_Tables', N'8.0.11');
GO

COMMIT;
GO

