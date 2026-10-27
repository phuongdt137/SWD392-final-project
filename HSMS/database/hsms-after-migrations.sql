IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE TABLE [AcademicYear] (
        [Id] bigint NOT NULL IDENTITY,
        [YearCode] nvarchar(30) NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        CONSTRAINT [PK_AcademicYear] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AcademicYear_Date] CHECK ([EndDate] >= [StartDate])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE TABLE [GradeLevel] (
        [Id] bigint NOT NULL IDENTITY,
        [LevelCode] nvarchar(30) NOT NULL,
        [LevelName] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_GradeLevel] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE TABLE [Role] (
        [Id] bigint NOT NULL IDENTITY,
        [RoleCode] varchar(50) NOT NULL,
        [RoleName] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE TABLE [Subject] (
        [Id] bigint NOT NULL IDENTITY,
        [SubjectCode] nvarchar(50) NOT NULL,
        [SubjectName] nvarchar(150) NOT NULL,
        [EvaluationType] nvarchar(50) NULL,
        CONSTRAINT [PK_Subject] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsActive', N'RoleCode', N'RoleName') AND [object_id] = OBJECT_ID(N'[Role]'))
        SET IDENTITY_INSERT [Role] ON;
    EXEC(N'INSERT INTO [Role] ([Id], [CreatedAt], [Description], [IsActive], [RoleCode], [RoleName])
    VALUES (CAST(1 AS bigint), ''2026-01-01T00:00:00.0000000Z'', N''High school student'', CAST(1 AS bit), ''STUDENT'', N''Student''),
    (CAST(2 AS bigint), ''2026-01-01T00:00:00.0000000Z'', N''Parent or guardian of student'', CAST(1 AS bit), ''PARENT'', N''Parent''),
    (CAST(3 AS bigint), ''2026-01-01T00:00:00.0000000Z'', N''School teacher'', CAST(1 AS bit), ''TEACHER'', N''Teacher''),
    (CAST(4 AS bigint), ''2026-01-01T00:00:00.0000000Z'', N''System administrator'', CAST(1 AS bit), ''ADMIN'', N''Administrator''),
    (CAST(5 AS bigint), ''2026-01-01T00:00:00.0000000Z'', N''School staff'', CAST(1 AS bit), ''STAFF'', N''Staff'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsActive', N'RoleCode', N'RoleName') AND [object_id] = OBJECT_ID(N'[Role]'))
        SET IDENTITY_INSERT [Role] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_AcademicYear_YearCode] ON [AcademicYear] ([YearCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Assessment_Assignment] ON [Assessment] ([TeachingAssignmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Assessment_Type] ON [Assessment] ([AssessmentTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_AssessmentType_TypeCode] ON [AssessmentType] ([TypeCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_AttendanceRecord_Customer] ON [AttendanceRecord] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_AttendanceRecord_Employee] ON [AttendanceRecord] ([RecordedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_AttendanceRecord_LeaveRequestId] ON [AttendanceRecord] ([LeaveRequestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_AttendanceRecord_Timetable] ON [AttendanceRecord] ([TimetableEntryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_AttendanceRecord] ON [AttendanceRecord] ([CustomerId], [TimetableEntryId], [AttendanceDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Class_AcademicYear] ON [Class] ([AcademicYearId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Class_GradeLevel] ON [Class] ([GradeLevelId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Class_HomeroomEmployee] ON [Class] ([HomeroomEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_ConductEvaluation_Customer] ON [ConductEvaluation] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_ConductEvaluation_HomeroomEmployeeId] ON [ConductEvaluation] ([HomeroomEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_ConductEvaluation_Semester] ON [ConductEvaluation] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_ConductEvaluation] ON [ConductEvaluation] ([CustomerId], [SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Customer_Email] ON [Customer] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Customer_Username] ON [Customer] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UX_Customer_StudentCode] ON [Customer] ([StudentCode]) WHERE [StudentCode] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_CustomerRelationship_Parent] ON [CustomerRelationship] ([ParentCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_CustomerRelationship_Student] ON [CustomerRelationship] ([StudentCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_CustomerRelationship] ON [CustomerRelationship] ([ParentCustomerId], [StudentCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_CustomerRole_CustomerId] ON [CustomerRole] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_CustomerRole_RoleId] ON [CustomerRole] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_CustomerRole] ON [CustomerRole] ([CustomerId], [RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Employee_Email] ON [Employee] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Employee_EmployeeCode] ON [Employee] ([EmployeeCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Employee_Username] ON [Employee] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_EmployeeRole_EmployeeId] ON [EmployeeRole] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_EmployeeRole_RoleId] ON [EmployeeRole] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_EmployeeRole] ON [EmployeeRole] ([EmployeeId], [RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_GradeLevel_LevelCode] ON [GradeLevel] ([LevelCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_LeaveRequest_RequestedBy] ON [LeaveRequest] ([RequestedByCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_LeaveRequest_ReviewedBy] ON [LeaveRequest] ([ReviewedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_LeaveRequest_Student] ON [LeaveRequest] ([StudentCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Notification_IsRead] ON [Notification] ([IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Notification_RecipientCustomer] ON [Notification] ([RecipientCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Notification_RecipientEmployee] ON [Notification] ([RecipientEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Notification_SenderCustomerId] ON [Notification] ([SenderCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Notification_SenderEmployeeId] ON [Notification] ([SenderEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Role_RoleCode] ON [Role] ([RoleCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Room_RoomCode] ON [Room] ([RoomCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_ScoreAuditLog_Employee] ON [ScoreAuditLog] ([ChangedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_ScoreAuditLog_StudentScore] ON [ScoreAuditLog] ([StudentScoreId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_Semester_AcademicYear] ON [Semester] ([AcademicYearId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Semester_SemesterCode] ON [Semester] ([SemesterCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_SemesterReport_Customer] ON [SemesterReport] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_SemesterReport_FinalizedByEmployeeId] ON [SemesterReport] ([FinalizedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_SemesterReport_Semester] ON [SemesterReport] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_SemesterReport] ON [SemesterReport] ([CustomerId], [SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_SemesterSubjectResult_Report] ON [SemesterSubjectResult] ([SemesterReportId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_SemesterSubjectResult_SubjectId] ON [SemesterSubjectResult] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_SemesterSubjectResult] ON [SemesterSubjectResult] ([SemesterReportId], [SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_StudentClass_Class] ON [StudentClass] ([ClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_StudentClass_Customer] ON [StudentClass] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_StudentScore_Assessment] ON [StudentScore] ([AssessmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_StudentScore_Customer] ON [StudentScore] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_StudentScore] ON [StudentScore] ([CustomerId], [AssessmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_Subject_SubjectCode] ON [Subject] ([SubjectCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TeacherFeedback_Customer] ON [TeacherFeedback] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TeacherFeedback_TeachingAssignmentId] ON [TeacherFeedback] ([TeachingAssignmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TeachingAssignment_Class] ON [TeachingAssignment] ([ClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TeachingAssignment_Employee] ON [TeachingAssignment] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TeachingAssignment_Semester] ON [TeachingAssignment] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TeachingAssignment_Subject] ON [TeachingAssignment] ([SubjectId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_TeachingAssignment] ON [TeachingAssignment] ([EmployeeId], [SubjectId], [ClassId], [SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_TimeSlot_SlotCode] ON [TimeSlot] ([SlotCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TimetableEntry_Assignment] ON [TimetableEntry] ([TeachingAssignmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TimetableEntry_Room] ON [TimetableEntry] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    CREATE INDEX [IX_TimetableEntry_TimeSlot] ON [TimetableEntry] ([TimeSlotId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010063339_InitDb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010063339_InitDb', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014072131_Modify-Model-Customer'
)
BEGIN
    DROP TABLE [CustomerRelationship];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014072131_Modify-Model-Customer'
)
BEGIN
    ALTER TABLE [Customer] ADD [ParentId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014072131_Modify-Model-Customer'
)
BEGIN
    ALTER TABLE [Customer] ADD [RelationshipType] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014072131_Modify-Model-Customer'
)
BEGIN
    CREATE INDEX [IX_Customer_ParentId] ON [Customer] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014072131_Modify-Model-Customer'
)
BEGIN
    EXEC(N'ALTER TABLE [Customer] ADD CONSTRAINT [CK_Customer_ParentNotSelf] CHECK ([ParentId] IS NULL OR [ParentId] <> [Id])');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014072131_Modify-Model-Customer'
)
BEGIN
    ALTER TABLE [Customer] ADD CONSTRAINT [FK_Customer_Parent] FOREIGN KEY ([ParentId]) REFERENCES [Customer] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014072131_Modify-Model-Customer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014072131_Modify-Model-Customer', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    CREATE INDEX [IX_MessageLog_Channel_Status] ON [MessageLog] ([Channel], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    CREATE INDEX [IX_MessageLog_CreatedAt] ON [MessageLog] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    CREATE INDEX [IX_MessageLog_CreatedByEmployee] ON [MessageLog] ([CreatedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    CREATE INDEX [IX_MessageLog_Notification] ON [MessageLog] ([NotificationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    CREATE INDEX [IX_MessageLog_RecipientAddress] ON [MessageLog] ([RecipientAddress]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    CREATE INDEX [IX_MessageLog_RecipientCustomer] ON [MessageLog] ([RecipientCustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    CREATE INDEX [IX_MessageLog_RecipientEmployee] ON [MessageLog] ([RecipientEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261021050908_Add-model-MessageLog'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261021050908_Add-model-MessageLog', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE TABLE [StudentAnnualReport] (
        [Id] bigint NOT NULL IDENTITY,
        [CustomerId] bigint NOT NULL,
        [AcademicYearId] bigint NOT NULL,
        [Status] nvarchar(30) NOT NULL DEFAULT N'DRAFT',
        [AnnualGpa] decimal(5,2) NULL,
        [AcademicResult] nvarchar(100) NULL,
        [ConductGrade] nvarchar(50) NULL,
        [TeacherComment] nvarchar(2000) NULL,
        [PrincipalComment] nvarchar(2000) NULL,
        [FinalizedByEmployeeId] bigint NULL,
        [FinalizedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_StudentAnnualReport] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_StudentAnnualReport_GPA] CHECK ([AnnualGpa] IS NULL OR ([AnnualGpa] >= 0 AND [AnnualGpa] <= 10)),
        CONSTRAINT [FK_StudentAnnualReport_AcademicYear] FOREIGN KEY ([AcademicYearId]) REFERENCES [AcademicYear] ([Id]),
        CONSTRAINT [FK_StudentAnnualReport_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]),
        CONSTRAINT [FK_StudentAnnualReport_FinalizedEmployee] FOREIGN KEY ([FinalizedByEmployeeId]) REFERENCES [Employee] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE TABLE [StudentTransfer] (
        [Id] bigint NOT NULL IDENTITY,
        [CustomerId] bigint NOT NULL,
        [TransferType] varchar(20) NOT NULL,
        [FromClassId] bigint NULL,
        [ToClassId] bigint NULL,
        [FromSchoolName] nvarchar(200) NULL,
        [ToSchoolName] nvarchar(200) NULL,
        [EffectiveDate] date NOT NULL,
        [Reason] nvarchar(1000) NOT NULL,
        [Status] nvarchar(30) NOT NULL DEFAULT N'COMPLETED',
        [RecordedByEmployeeId] bigint NOT NULL,
        [RecordedAt] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_StudentTransfer] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_StudentTransfer_Type] CHECK ([TransferType] IN ('CLASS','SCHOOL_IN','SCHOOL_OUT')),
        CONSTRAINT [FK_StudentTransfer_Customer] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]),
        CONSTRAINT [FK_StudentTransfer_Employee] FOREIGN KEY ([RecordedByEmployeeId]) REFERENCES [Employee] ([Id]),
        CONSTRAINT [FK_StudentTransfer_FromClass] FOREIGN KEY ([FromClassId]) REFERENCES [Class] ([Id]),
        CONSTRAINT [FK_StudentTransfer_ToClass] FOREIGN KEY ([ToClassId]) REFERENCES [Class] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE INDEX [IX_StudentAnnualReport_AcademicYearId] ON [StudentAnnualReport] ([AcademicYearId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE INDEX [IX_StudentAnnualReport_FinalizedByEmployeeId] ON [StudentAnnualReport] ([FinalizedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_StudentAnnualReport] ON [StudentAnnualReport] ([CustomerId], [AcademicYearId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE INDEX [IX_StudentTransfer_FromClass] ON [StudentTransfer] ([FromClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE INDEX [IX_StudentTransfer_RecordedByEmployeeId] ON [StudentTransfer] ([RecordedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE INDEX [IX_StudentTransfer_Student_Date] ON [StudentTransfer] ([CustomerId], [EffectiveDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    CREATE INDEX [IX_StudentTransfer_ToClass] ON [StudentTransfer] ([ToClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261023065302_AddStudentTranscriptAndTransferHistory'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261023065302_AddStudentTranscriptAndTransferHistory', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Notification] ADD [AttendanceAuditLogId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Notification] ADD [DeliveryAttempts] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Notification] ADD [DispatchedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Notification] ADD [Link] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Notification] ADD [NextDeliveryAttemptAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Employee] ADD [LastLoginAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Employee] ADD [PasswordResetToken] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Employee] ADD [PasswordResetTokenExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Customer] ADD [LastLoginAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Customer] ADD [PasswordResetToken] nvarchar(256) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Customer] ADD [PasswordResetTokenExpiry] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [AttendanceRecord] ADD [UpdatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [AttendanceRecord] ADD [UpdatedByEmployeeId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [AttendanceRecord] ADD [Version] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    CREATE TABLE [AttendanceAuditLog] (
        [Id] bigint NOT NULL IDENTITY,
        [AttendanceRecordId] bigint NOT NULL,
        [OldStatus] nvarchar(30) NULL,
        [NewStatus] nvarchar(30) NOT NULL,
        [OldComment] nvarchar(1000) NULL,
        [NewComment] nvarchar(1000) NULL,
        [OldLeaveRequestId] bigint NULL,
        [NewLeaveRequestId] bigint NULL,
        [ChangeReason] nvarchar(1000) NULL,
        [ChangedByEmployeeId] bigint NOT NULL,
        [ChangedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AttendanceAuditLog] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AttendanceAuditLog_AttendanceRecord_AttendanceRecordId] FOREIGN KEY ([AttendanceRecordId]) REFERENCES [AttendanceRecord] ([Id]),
        CONSTRAINT [FK_AttendanceAuditLog_Employee_ChangedByEmployeeId] FOREIGN KEY ([ChangedByEmployeeId]) REFERENCES [Employee] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Notification_AttendanceAuditLogId] ON [Notification] ([AttendanceAuditLogId]) WHERE [AttendanceAuditLogId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Notification_DispatchedAt_NextDeliveryAttemptAt] ON [Notification] ([DispatchedAt], [NextDeliveryAttemptAt]) WHERE [AttendanceAuditLogId] IS NOT NULL AND [DispatchedAt] IS NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    CREATE INDEX [IX_Notification_RecipientCustomerId_IsRead_SentAt] ON [Notification] ([RecipientCustomerId], [IsRead], [SentAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    CREATE INDEX [IX_AttendanceRecord_TimetableEntryId_AttendanceDate] ON [AttendanceRecord] ([TimetableEntryId], [AttendanceDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    CREATE INDEX [IX_AttendanceRecord_UpdatedByEmployeeId] ON [AttendanceRecord] ([UpdatedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    EXEC(N'ALTER TABLE [AttendanceRecord] ADD CONSTRAINT [CK_AttendanceRecord_Status] CHECK ([Status] IN (''PRESENT'',''ABSENT'',''LATE'',''EXCUSED'') AND (([Status] = ''EXCUSED'' AND [LeaveRequestId] IS NOT NULL) OR ([Status] <> ''EXCUSED'' AND [LeaveRequestId] IS NULL)))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    CREATE INDEX [IX_AttendanceAuditLog_AttendanceRecordId_ChangedAt] ON [AttendanceAuditLog] ([AttendanceRecordId], [ChangedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    CREATE INDEX [IX_AttendanceAuditLog_ChangedByEmployeeId] ON [AttendanceAuditLog] ([ChangedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [AttendanceRecord] ADD CONSTRAINT [FK_AttendanceRecord_Employee_UpdatedByEmployeeId] FOREIGN KEY ([UpdatedByEmployeeId]) REFERENCES [Employee] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    ALTER TABLE [Notification] ADD CONSTRAINT [FK_Notification_AttendanceAuditLog_AttendanceAuditLogId] FOREIGN KEY ([AttendanceAuditLogId]) REFERENCES [AttendanceAuditLog] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261027033754_AddEmployeeCustomerAccountSupport'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261027033754_AddEmployeeCustomerAccountSupport', N'8.0.11');
END;
GO

COMMIT;
GO

