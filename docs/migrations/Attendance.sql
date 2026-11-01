BEGIN TRANSACTION;
GO

ALTER TABLE [Notification] ADD [AttendanceAuditLogId] bigint NULL;
GO

ALTER TABLE [Notification] ADD [DeliveryAttempts] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Notification] ADD [DispatchedAt] datetime2 NULL;
GO

ALTER TABLE [Notification] ADD [Link] nvarchar(500) NULL;
GO

ALTER TABLE [Notification] ADD [NextDeliveryAttemptAt] datetime2 NULL;
GO

ALTER TABLE [AttendanceRecord] ADD [UpdatedAt] datetime2 NULL;
GO

ALTER TABLE [AttendanceRecord] ADD [UpdatedByEmployeeId] bigint NULL;
GO

ALTER TABLE [AttendanceRecord] ADD [Version] uniqueidentifier NOT NULL DEFAULT (NEWID());
GO

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
GO

CREATE UNIQUE INDEX [IX_Notification_AttendanceAuditLogId] ON [Notification] ([AttendanceAuditLogId]) WHERE [AttendanceAuditLogId] IS NOT NULL;
GO

CREATE INDEX [IX_Notification_DispatchedAt_NextDeliveryAttemptAt] ON [Notification] ([DispatchedAt], [NextDeliveryAttemptAt]) WHERE [AttendanceAuditLogId] IS NOT NULL AND [DispatchedAt] IS NULL;
GO

CREATE INDEX [IX_Notification_RecipientId_IsRead_SentAt] ON [Notification] ([RecipientId], [IsRead], [SentAt]);
GO

CREATE INDEX [IX_AttendanceRecord_TimetableEntryId_AttendanceDate] ON [AttendanceRecord] ([TimetableEntryId], [AttendanceDate]);
GO

CREATE INDEX [IX_AttendanceRecord_UpdatedByEmployeeId] ON [AttendanceRecord] ([UpdatedByEmployeeId]);
GO

ALTER TABLE [AttendanceRecord] ADD CONSTRAINT [CK_AttendanceRecord_Status] CHECK ([Status] IN ('PRESENT','ABSENT','LATE','EXCUSED') AND (([Status] = 'EXCUSED' AND [LeaveRequestId] IS NOT NULL) OR ([Status] <> 'EXCUSED' AND [LeaveRequestId] IS NULL)));
GO

CREATE INDEX [IX_AttendanceAuditLog_AttendanceRecordId_ChangedAt] ON [AttendanceAuditLog] ([AttendanceRecordId], [ChangedAt]);
GO

CREATE INDEX [IX_AttendanceAuditLog_ChangedByEmployeeId] ON [AttendanceAuditLog] ([ChangedByEmployeeId]);
GO

ALTER TABLE [AttendanceRecord] ADD CONSTRAINT [FK_AttendanceRecord_Employee_UpdatedByEmployeeId] FOREIGN KEY ([UpdatedByEmployeeId]) REFERENCES [Employee] ([Id]);
GO

ALTER TABLE [Notification] ADD CONSTRAINT [FK_Notification_AttendanceAuditLog_AttendanceAuditLogId] FOREIGN KEY ([AttendanceAuditLogId]) REFERENCES [AttendanceAuditLog] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261026045557_AddAttendanceModule', N'8.0.11');
GO

COMMIT;
GO

