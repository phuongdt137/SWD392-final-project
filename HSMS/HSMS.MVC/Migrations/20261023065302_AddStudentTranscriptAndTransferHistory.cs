using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSMS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentTranscriptAndTransferHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentAnnualReport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    AcademicYearId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    AnnualGpa = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AcademicResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConductGrade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TeacherComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrincipalComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FinalizedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnnualReport", x => x.Id);
                    table.CheckConstraint("CK_StudentAnnualReport_GPA", "[AnnualGpa] IS NULL OR ([AnnualGpa] >= 0 AND [AnnualGpa] <= 10)");
                    table.ForeignKey(
                        name: "FK_StudentAnnualReport_AcademicYear",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYear",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAnnualReport_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAnnualReport_FinalizedEmployee",
                        column: x => x.FinalizedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentTransfer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    TransferType = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    FromClassId = table.Column<long>(type: "bigint", nullable: true),
                    ToClassId = table.Column<long>(type: "bigint", nullable: true),
                    FromSchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ToSchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "COMPLETED"),
                    RecordedByEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTransfer", x => x.Id);
                    table.CheckConstraint("CK_StudentTransfer_Type", "[TransferType] IN ('CLASS','SCHOOL_IN','SCHOOL_OUT')");
                    table.ForeignKey(
                        name: "FK_StudentTransfer_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentTransfer_Employee",
                        column: x => x.RecordedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentTransfer_FromClass",
                        column: x => x.FromClassId,
                        principalTable: "Class",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentTransfer_ToClass",
                        column: x => x.ToClassId,
                        principalTable: "Class",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnnualReport_AcademicYearId",
                table: "StudentAnnualReport",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnnualReport_FinalizedByEmployeeId",
                table: "StudentAnnualReport",
                column: "FinalizedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "UQ_StudentAnnualReport",
                table: "StudentAnnualReport",
                columns: new[] { "CustomerId", "AcademicYearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransfer_FromClass",
                table: "StudentTransfer",
                column: "FromClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransfer_RecordedByEmployeeId",
                table: "StudentTransfer",
                column: "RecordedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransfer_Student_Date",
                table: "StudentTransfer",
                columns: new[] { "CustomerId", "EffectiveDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransfer_ToClass",
                table: "StudentTransfer",
                column: "ToClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAnnualReport");

            migrationBuilder.DropTable(
                name: "StudentTransfer");
        }
    }
}
