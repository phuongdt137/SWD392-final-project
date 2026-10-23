using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSMS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddmodelMessageLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MessageLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientCustomerId = table.Column<long>(type: "bigint", nullable: true),
                    RecipientEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    RecipientAddress = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MessageBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "QUEUED"),
                    ExternalMessageId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Cost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    NotificationId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageLog", x => x.Id);
                    table.CheckConstraint("CK_MessageLog_Recipient", "[RecipientCustomerId] IS NULL OR [RecipientEmployeeId] IS NULL");
                    table.ForeignKey(
                        name: "FK_MessageLog_CreatedByEmployee",
                        column: x => x.CreatedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessageLog_Notification",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessageLog_RecipientCustomer",
                        column: x => x.RecipientCustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessageLog_RecipientEmployee",
                        column: x => x.RecipientEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_Channel_Status",
                table: "MessageLog",
                columns: new[] { "Channel", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_CreatedAt",
                table: "MessageLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_CreatedByEmployee",
                table: "MessageLog",
                column: "CreatedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_Notification",
                table: "MessageLog",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_RecipientAddress",
                table: "MessageLog",
                column: "RecipientAddress");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_RecipientCustomer",
                table: "MessageLog",
                column: "RecipientCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLog_RecipientEmployee",
                table: "MessageLog",
                column: "RecipientEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageLog");
        }
    }
}
