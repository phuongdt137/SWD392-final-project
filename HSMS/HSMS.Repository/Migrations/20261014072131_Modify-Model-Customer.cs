using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSMS.Repository.Migrations
{
    /// <inheritdoc />
    public partial class ModifyModelCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerRelationship");

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "Customer",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationshipType",
                table: "Customer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_ParentId",
                table: "Customer",
                column: "ParentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customer_ParentNotSelf",
                table: "Customer",
                sql: "[ParentId] IS NULL OR [ParentId] <> [Id]");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Parent",
                table: "Customer",
                column: "ParentId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Parent",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_ParentId",
                table: "Customer");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customer_ParentNotSelf",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "RelationshipType",
                table: "Customer");

            migrationBuilder.CreateTable(
                name: "CustomerRelationship",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    StudentCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    IsPrimaryGuardian = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRelationship", x => x.Id);
                    table.CheckConstraint("CK_CustomerRelationship_SameCustomer", "[ParentCustomerId] <> [StudentCustomerId]");
                    table.ForeignKey(
                        name: "FK_CustomerRelationship_Parent",
                        column: x => x.ParentCustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerRelationship_Student",
                        column: x => x.StudentCustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRelationship_Parent",
                table: "CustomerRelationship",
                column: "ParentCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRelationship_Student",
                table: "CustomerRelationship",
                column: "StudentCustomerId");

            migrationBuilder.CreateIndex(
                name: "UQ_CustomerRelationship",
                table: "CustomerRelationship",
                columns: new[] { "ParentCustomerId", "StudentCustomerId" },
                unique: true);
        }
    }
}
