using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSMS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddModelUserAndRefactorAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_CreatedByEmployee",
                table: "MessageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_RecipientCustomer",
                table: "MessageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_RecipientEmployee",
                table: "MessageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_RecipientCustomer",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_RecipientEmployee",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_SenderCustomer",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_SenderEmployee",
                table: "Notification");

            migrationBuilder.DropTable(
                name: "CustomerRole");

            migrationBuilder.DropTable(
                name: "EmployeeRole");

            migrationBuilder.DropIndex(
                name: "IX_Notification_RecipientCustomer",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_RecipientEmployee",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_SenderCustomerId",
                table: "Notification");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notification_Recipient",
                table: "Notification");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Notification_Sender",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_MessageLog_CreatedByEmployee",
                table: "MessageLog");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MessageLog_Recipient",
                table: "MessageLog");

            migrationBuilder.DropIndex(
                name: "UQ_Employee_Email",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "UQ_Employee_Username",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "UQ_Customer_Email",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "UQ_Customer_Username",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "RecipientCustomerId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "RecipientEmployeeId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "SenderCustomerId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "MessageLog");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "SenderEmployeeId",
                table: "Notification",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_SenderEmployeeId",
                table: "Notification",
                newName: "IX_Notification_SenderId");

            migrationBuilder.RenameColumn(
                name: "RecipientEmployeeId",
                table: "MessageLog",
                newName: "RecipientUserId");

            migrationBuilder.RenameColumn(
                name: "RecipientCustomerId",
                table: "MessageLog",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageLog_RecipientEmployee",
                table: "MessageLog",
                newName: "IX_MessageLog_RecipientUserId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageLog_RecipientCustomer",
                table: "MessageLog",
                newName: "IX_MessageLog_CreatedByUserId");

            migrationBuilder.AddColumn<long>(
                name: "RecipientId",
                table: "Notification",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Employee",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Customer",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Role",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRole_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientId",
                table: "Notification",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "UQ_Employee_UserId",
                table: "Employee",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Customer_UserId",
                table: "Customer",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_UserRole",
                table: "UserRole",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_User",
                table: "Customer",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_User",
                table: "Employee",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_CreatedByUser",
                table: "MessageLog",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_RecipientUser",
                table: "MessageLog",
                column: "RecipientUserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_RecipientUser",
                table: "Notification",
                column: "RecipientId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_SenderUser",
                table: "Notification",
                column: "SenderId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_User",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_User",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_CreatedByUser",
                table: "MessageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageLog_RecipientUser",
                table: "MessageLog");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_RecipientUser",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_SenderUser",
                table: "Notification");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Notification_RecipientId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "UQ_Employee_UserId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "UQ_Customer_UserId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Notification",
                newName: "SenderEmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_SenderId",
                table: "Notification",
                newName: "IX_Notification_SenderEmployeeId");

            migrationBuilder.RenameColumn(
                name: "RecipientUserId",
                table: "MessageLog",
                newName: "RecipientEmployeeId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "MessageLog",
                newName: "RecipientCustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageLog_RecipientUserId",
                table: "MessageLog",
                newName: "IX_MessageLog_RecipientEmployee");

            migrationBuilder.RenameIndex(
                name: "IX_MessageLog_CreatedByUserId",
                table: "MessageLog",
                newName: "IX_MessageLog_RecipientCustomer");

            migrationBuilder.AddColumn<long>(
                name: "RecipientCustomerId",
                table: "Notification",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RecipientEmployeeId",
                table: "Notification",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SenderCustomerId",
                table: "Notification",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedByEmployeeId",
                table: "MessageLog",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Employee",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Employee",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Employee",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Employee",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Employee",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Employee",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Employee",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Employee",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Customer",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Customer",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customer",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Customer",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customer",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Customer",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Customer",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Customer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CustomerRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRole_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerRole_Role",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeRole_Employee",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeRole_Role",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientCustomer",
                table: "Notification",
                column: "RecipientCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RecipientEmployee",
                table: "Notification",
                column: "RecipientEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SenderCustomerId",
                table: "Notification",
                column: "SenderCustomerId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notification_Recipient",
                table: "Notification",
                sql: "([RecipientCustomerId] IS NOT NULL AND [RecipientEmployeeId] IS NULL) OR ([RecipientCustomerId] IS NULL AND [RecipientEmployeeId] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Notification_Sender",
                table: "Notification",
                sql: "[SenderCustomerId] IS NULL OR [SenderEmployeeId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_CreatedByEmployee",
                table: "MessageLog",
                column: "CreatedByEmployeeId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MessageLog_Recipient",
                table: "MessageLog",
                sql: "[RecipientCustomerId] IS NULL OR [RecipientEmployeeId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Employee_Email",
                table: "Employee",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Employee_Username",
                table: "Employee",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Customer_Email",
                table: "Customer",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Customer_Username",
                table: "Customer",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRole_CustomerId",
                table: "CustomerRole",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRole_RoleId",
                table: "CustomerRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ_CustomerRole",
                table: "CustomerRole",
                columns: new[] { "CustomerId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRole_EmployeeId",
                table: "EmployeeRole",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRole_RoleId",
                table: "EmployeeRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ_EmployeeRole",
                table: "EmployeeRole",
                columns: new[] { "EmployeeId", "RoleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_CreatedByEmployee",
                table: "MessageLog",
                column: "CreatedByEmployeeId",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_RecipientCustomer",
                table: "MessageLog",
                column: "RecipientCustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageLog_RecipientEmployee",
                table: "MessageLog",
                column: "RecipientEmployeeId",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_RecipientCustomer",
                table: "Notification",
                column: "RecipientCustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_RecipientEmployee",
                table: "Notification",
                column: "RecipientEmployeeId",
                principalTable: "Employee",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_SenderCustomer",
                table: "Notification",
                column: "SenderCustomerId",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_SenderEmployee",
                table: "Notification",
                column: "SenderEmployeeId",
                principalTable: "Employee",
                principalColumn: "Id");
        }
    }
}
