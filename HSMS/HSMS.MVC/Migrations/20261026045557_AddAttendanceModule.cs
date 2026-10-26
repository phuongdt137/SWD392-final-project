using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSMS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AttendanceAuditLogId",
                table: "Notification",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryAttempts",
                table: "Notification",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DispatchedAt",
                table: "Notification",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Notification",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextDeliveryAttemptAt",
                table: "Notification",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AttendanceRecord",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedByEmployeeId",
                table: "AttendanceRecord",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "AttendanceRecord",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateTable(
                name: "AttendanceAuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceRecordId = table.Column<long>(type: "bigint", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OldComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NewComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OldLeaveRequestId = table.Column<long>(type: "bigint", nullable: true),
                    NewLeaveRequestId = table.Column<long>(type: "bigint", nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedByEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceAuditLog_AttendanceRecord_AttendanceRecordId",
                        column: x => x.AttendanceRecordId,
                        principalTable: "AttendanceRecord",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AttendanceAuditLog_Employee_ChangedByEmployeeId",
                        column: x => x.ChangedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });



            migrationBuilder.CreateIndex(
                name: "IX_Notification_AttendanceAuditLogId",
                table: "Notification",
                column: "AttendanceAuditLogId",
                unique: true,
                filter: "[AttendanceAuditLogId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_DispatchedAt_NextDeliveryAttemptAt",
                table: "Notification",
                columns: new[] { "DispatchedAt", "NextDeliveryAttemptAt" },
                filter: "[AttendanceAuditLogId] IS NOT NULL AND [DispatchedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientId_IsRead_SentAt",
                table: "Notification",
                columns: new[] { "RecipientId", "IsRead", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecord_TimetableEntryId_AttendanceDate",
                table: "AttendanceRecord",
                columns: new[] { "TimetableEntryId", "AttendanceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecord_UpdatedByEmployeeId",
                table: "AttendanceRecord",
                column: "UpdatedByEmployeeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AttendanceRecord_Status",
                table: "AttendanceRecord",
                sql: "[Status] IN ('PRESENT','ABSENT','LATE','EXCUSED') AND (([Status] = 'EXCUSED' AND [LeaveRequestId] IS NOT NULL) OR ([Status] <> 'EXCUSED' AND [LeaveRequestId] IS NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAuditLog_AttendanceRecordId_ChangedAt",
                table: "AttendanceAuditLog",
                columns: new[] { "AttendanceRecordId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAuditLog_ChangedByEmployeeId",
                table: "AttendanceAuditLog",
                column: "ChangedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecord_Employee_UpdatedByEmployeeId",
                table: "AttendanceRecord",
                column: "UpdatedByEmployeeId",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AttendanceAuditLog_AttendanceAuditLogId",
                table: "Notification",
                column: "AttendanceAuditLogId",
                principalTable: "AttendanceAuditLog",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecord_Employee_UpdatedByEmployeeId",
                table: "AttendanceRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AttendanceAuditLog_AttendanceAuditLogId",
                table: "Notification");


            migrationBuilder.DropIndex(
                name: "IX_Notification_AttendanceAuditLogId",
                table: "Notification");

            migrationBuilder.DropTable(
                name: "AttendanceAuditLog");


            migrationBuilder.DropIndex(
                name: "IX_Notification_DispatchedAt_NextDeliveryAttemptAt",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_RecipientId_IsRead_SentAt",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecord_TimetableEntryId_AttendanceDate",
                table: "AttendanceRecord");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRecord_UpdatedByEmployeeId",
                table: "AttendanceRecord");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AttendanceRecord_Status",
                table: "AttendanceRecord");

            migrationBuilder.DropColumn(
                name: "AttendanceAuditLogId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "DeliveryAttempts",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "DispatchedAt",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "NextDeliveryAttemptAt",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AttendanceRecord");

            migrationBuilder.DropColumn(
                name: "UpdatedByEmployeeId",
                table: "AttendanceRecord");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AttendanceRecord");


        }
    }
}
