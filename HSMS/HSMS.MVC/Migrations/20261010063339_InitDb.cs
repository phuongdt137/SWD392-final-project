using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HSMS.MVC.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicYear",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YearCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYear", x => x.Id);
                    table.CheckConstraint("CK_AcademicYear_Date", "[EndDate] >= [StartDate]");
                });

            migrationBuilder.CreateTable(
                name: "AssessmentType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MinEntries = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxEntries = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentType", x => x.Id);
                    table.CheckConstraint("CK_AssessmentType_Entries", "[MinEntries] >= 0 AND ([MaxEntries] IS NULL OR [MaxEntries] >= [MinEntries])");
                    table.CheckConstraint("CK_AssessmentType_Weight", "[Weight] >= 0 AND [Weight] <= 100");
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StudentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "date", nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmergencyContact = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaxPeriodsPerWeek = table.Column<int>(type: "int", nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    HireDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                    table.CheckConstraint("CK_Employee_MaxPeriodsPerWeek", "[MaxPeriodsPerWeek] IS NULL OR [MaxPeriodsPerWeek] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "GradeLevel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LevelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleCode = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Building = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RoomType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.Id);
                    table.CheckConstraint("CK_Room_Capacity", "[Capacity] IS NULL OR [Capacity] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EvaluationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlot",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlotCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeriodNo = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Session = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlot", x => x.Id);
                    table.CheckConstraint("CK_TimeSlot_PeriodNo", "[PeriodNo] > 0");
                    table.CheckConstraint("CK_TimeSlot_Time", "[EndTime] > [StartTime]");
                });

            migrationBuilder.CreateTable(
                name: "Semester",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SemesterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    AcademicYearId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semester", x => x.Id);
                    table.CheckConstraint("CK_Semester_Date", "[EndDate] >= [StartDate]");
                    table.ForeignKey(
                        name: "FK_Semester_AcademicYear",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYear",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerRelationship",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    StudentCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsPrimaryGuardian = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
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

            migrationBuilder.CreateTable(
                name: "LeaveRequest",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedByCustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Attachment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "PENDING"),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    ReviewNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequest", x => x.Id);
                    table.CheckConstraint("CK_LeaveRequest_Date", "[EndDate] >= [StartDate]");
                    table.ForeignKey(
                        name: "FK_LeaveRequest_RequestedBy",
                        column: x => x.RequestedByCustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequest_ReviewedBy",
                        column: x => x.ReviewedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeaveRequest_Student",
                        column: x => x.StudentCustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderCustomerId = table.Column<long>(type: "bigint", nullable: true),
                    SenderEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    RecipientCustomerId = table.Column<long>(type: "bigint", nullable: true),
                    RecipientEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.CheckConstraint("CK_Notification_Recipient", "([RecipientCustomerId] IS NOT NULL AND [RecipientEmployeeId] IS NULL) OR ([RecipientCustomerId] IS NULL AND [RecipientEmployeeId] IS NOT NULL)");
                    table.CheckConstraint("CK_Notification_Sender", "[SenderCustomerId] IS NULL OR [SenderEmployeeId] IS NULL");
                    table.ForeignKey(
                        name: "FK_Notification_RecipientCustomer",
                        column: x => x.RecipientCustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notification_RecipientEmployee",
                        column: x => x.RecipientEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notification_SenderCustomer",
                        column: x => x.SenderCustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notification_SenderEmployee",
                        column: x => x.SenderEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HomeroomEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    AcademicYearId = table.Column<long>(type: "bigint", nullable: false),
                    GradeLevelId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Class_AcademicYear",
                        column: x => x.AcademicYearId,
                        principalTable: "AcademicYear",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Class_GradeLevel",
                        column: x => x.GradeLevelId,
                        principalTable: "GradeLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Class_HomeroomEmployee",
                        column: x => x.HomeroomEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
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

            migrationBuilder.CreateTable(
                name: "ConductEvaluation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    SemesterId = table.Column<long>(type: "bigint", nullable: false),
                    HomeroomEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ConductGrade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TeacherComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConductEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConductEvaluation_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConductEvaluation_Employee",
                        column: x => x.HomeroomEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConductEvaluation_Semester",
                        column: x => x.SemesterId,
                        principalTable: "Semester",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SemesterReport",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    SemesterId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    FinalizedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GPA = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AcademicRank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterReport", x => x.Id);
                    table.CheckConstraint("CK_SemesterReport_GPA", "[GPA] IS NULL OR ([GPA] >= 0 AND [GPA] <= 10)");
                    table.ForeignKey(
                        name: "FK_SemesterReport_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemesterReport_FinalizedEmployee",
                        column: x => x.FinalizedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemesterReport_Semester",
                        column: x => x.SemesterId,
                        principalTable: "Semester",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentClass",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    ClassId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "date", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClass", x => x.Id);
                    table.CheckConstraint("CK_StudentClass_Date", "[LeftAt] IS NULL OR [LeftAt] >= [JoinedAt]");
                    table.ForeignKey(
                        name: "FK_StudentClass_Class",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentClass_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeachingAssignment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    SubjectId = table.Column<long>(type: "bigint", nullable: false),
                    ClassId = table.Column<long>(type: "bigint", nullable: false),
                    SemesterId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingAssignment_Class",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingAssignment_Employee",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingAssignment_Semester",
                        column: x => x.SemesterId,
                        principalTable: "Semester",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeachingAssignment_Subject",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SemesterSubjectResult",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterReportId = table.Column<long>(type: "bigint", nullable: false),
                    SubjectId = table.Column<long>(type: "bigint", nullable: false),
                    AverageScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    EvaluationResult = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterSubjectResult", x => x.Id);
                    table.CheckConstraint("CK_SemesterSubjectResult_Score", "[AverageScore] IS NULL OR ([AverageScore] >= 0 AND [AverageScore] <= 10)");
                    table.ForeignKey(
                        name: "FK_SemesterSubjectResult_Report",
                        column: x => x.SemesterReportId,
                        principalTable: "SemesterReport",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SemesterSubjectResult_Subject",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assessment",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingAssignmentId = table.Column<long>(type: "bigint", nullable: false),
                    AssessmentTypeId = table.Column<long>(type: "bigint", nullable: false),
                    AssessmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConductedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessment_AssessmentType",
                        column: x => x.AssessmentTypeId,
                        principalTable: "AssessmentType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assessment_TeachingAssignment",
                        column: x => x.TeachingAssignmentId,
                        principalTable: "TeachingAssignment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeacherFeedback",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    TeachingAssignmentId = table.Column<long>(type: "bigint", nullable: false),
                    FeedbackType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherFeedback_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeacherFeedback_TeachingAssignment",
                        column: x => x.TeachingAssignmentId,
                        principalTable: "TeachingAssignment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TimetableEntry",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeachingAssignmentId = table.Column<long>(type: "bigint", nullable: false),
                    RoomId = table.Column<long>(type: "bigint", nullable: false),
                    TimeSlotId = table.Column<long>(type: "bigint", nullable: false),
                    DayOfWeek = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVE"),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableEntry", x => x.Id);
                    table.CheckConstraint("CK_TimetableEntry_DayOfWeek", "[DayOfWeek] BETWEEN 1 AND 7");
                    table.CheckConstraint("CK_TimetableEntry_EffectiveDate", "[EffectiveFrom] IS NULL OR [EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]");
                    table.ForeignKey(
                        name: "FK_TimetableEntry_Room",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntry_TeachingAssignment",
                        column: x => x.TeachingAssignmentId,
                        principalTable: "TeachingAssignment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimetableEntry_TimeSlot",
                        column: x => x.TimeSlotId,
                        principalTable: "TimeSlot",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentScore",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    AssessmentId = table.Column<long>(type: "bigint", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentScore", x => x.Id);
                    table.CheckConstraint("CK_StudentScore_Score", "[Score] IS NULL OR ([Score] >= 0 AND [Score] <= 10)");
                    table.ForeignKey(
                        name: "FK_StudentScore_Assessment",
                        column: x => x.AssessmentId,
                        principalTable: "Assessment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentScore_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AttendanceRecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    RecordedByEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    LeaveRequestId = table.Column<long>(type: "bigint", nullable: true),
                    TimetableEntryId = table.Column<long>(type: "bigint", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecord_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AttendanceRecord_Employee",
                        column: x => x.RecordedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AttendanceRecord_LeaveRequest",
                        column: x => x.LeaveRequestId,
                        principalTable: "LeaveRequest",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AttendanceRecord_TimetableEntry",
                        column: x => x.TimetableEntryId,
                        principalTable: "TimetableEntry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScoreAuditLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentScoreId = table.Column<long>(type: "bigint", nullable: false),
                    OldScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NewScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedByEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreAuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreAuditLog_Employee",
                        column: x => x.ChangedByEmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScoreAuditLog_StudentScore",
                        column: x => x.StudentScoreId,
                        principalTable: "StudentScore",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "RoleCode", "RoleName" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "High school student", true, "STUDENT", "Student" },
                    { 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Parent or guardian of student", true, "PARENT", "Parent" },
                    { 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "School teacher", true, "TEACHER", "Teacher" },
                    { 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator", true, "ADMIN", "Administrator" },
                    { 5L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "School staff", true, "STAFF", "Staff" }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_AcademicYear_YearCode",
                table: "AcademicYear",
                column: "YearCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_Assignment",
                table: "Assessment",
                column: "TeachingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessment_Type",
                table: "Assessment",
                column: "AssessmentTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_AssessmentType_TypeCode",
                table: "AssessmentType",
                column: "TypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecord_Customer",
                table: "AttendanceRecord",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecord_Employee",
                table: "AttendanceRecord",
                column: "RecordedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecord_LeaveRequestId",
                table: "AttendanceRecord",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecord_Timetable",
                table: "AttendanceRecord",
                column: "TimetableEntryId");

            migrationBuilder.CreateIndex(
                name: "UQ_AttendanceRecord",
                table: "AttendanceRecord",
                columns: new[] { "CustomerId", "TimetableEntryId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Class_AcademicYear",
                table: "Class",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_GradeLevel",
                table: "Class",
                column: "GradeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_HomeroomEmployee",
                table: "Class",
                column: "HomeroomEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductEvaluation_Customer",
                table: "ConductEvaluation",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductEvaluation_HomeroomEmployeeId",
                table: "ConductEvaluation",
                column: "HomeroomEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ConductEvaluation_Semester",
                table: "ConductEvaluation",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "UQ_ConductEvaluation",
                table: "ConductEvaluation",
                columns: new[] { "CustomerId", "SemesterId" },
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
                name: "UX_Customer_StudentCode",
                table: "Customer",
                column: "StudentCode",
                unique: true,
                filter: "[StudentCode] IS NOT NULL");

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
                name: "UQ_Employee_Email",
                table: "Employee",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Employee_EmployeeCode",
                table: "Employee",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Employee_Username",
                table: "Employee",
                column: "Username",
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

            migrationBuilder.CreateIndex(
                name: "UQ_GradeLevel_LevelCode",
                table: "GradeLevel",
                column: "LevelCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_RequestedBy",
                table: "LeaveRequest",
                column: "RequestedByCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_ReviewedBy",
                table: "LeaveRequest",
                column: "ReviewedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequest_Student",
                table: "LeaveRequest",
                column: "StudentCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_IsRead",
                table: "Notification",
                column: "IsRead");

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

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SenderEmployeeId",
                table: "Notification",
                column: "SenderEmployeeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Role_RoleCode",
                table: "Role",
                column: "RoleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Room_RoomCode",
                table: "Room",
                column: "RoomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreAuditLog_Employee",
                table: "ScoreAuditLog",
                column: "ChangedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreAuditLog_StudentScore",
                table: "ScoreAuditLog",
                column: "StudentScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Semester_AcademicYear",
                table: "Semester",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "UQ_Semester_SemesterCode",
                table: "Semester",
                column: "SemesterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemesterReport_Customer",
                table: "SemesterReport",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterReport_FinalizedByEmployeeId",
                table: "SemesterReport",
                column: "FinalizedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterReport_Semester",
                table: "SemesterReport",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "UQ_SemesterReport",
                table: "SemesterReport",
                columns: new[] { "CustomerId", "SemesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SemesterSubjectResult_Report",
                table: "SemesterSubjectResult",
                column: "SemesterReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterSubjectResult_SubjectId",
                table: "SemesterSubjectResult",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "UQ_SemesterSubjectResult",
                table: "SemesterSubjectResult",
                columns: new[] { "SemesterReportId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentClass_Class",
                table: "StudentClass",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClass_Customer",
                table: "StudentClass",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentScore_Assessment",
                table: "StudentScore",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentScore_Customer",
                table: "StudentScore",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UQ_StudentScore",
                table: "StudentScore",
                columns: new[] { "CustomerId", "AssessmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Subject_SubjectCode",
                table: "Subject",
                column: "SubjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherFeedback_Customer",
                table: "TeacherFeedback",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherFeedback_TeachingAssignmentId",
                table: "TeacherFeedback",
                column: "TeachingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingAssignment_Class",
                table: "TeachingAssignment",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingAssignment_Employee",
                table: "TeachingAssignment",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingAssignment_Semester",
                table: "TeachingAssignment",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingAssignment_Subject",
                table: "TeachingAssignment",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "UQ_TeachingAssignment",
                table: "TeachingAssignment",
                columns: new[] { "EmployeeId", "SubjectId", "ClassId", "SemesterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TimeSlot_SlotCode",
                table: "TimeSlot",
                column: "SlotCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntry_Assignment",
                table: "TimetableEntry",
                column: "TeachingAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntry_Room",
                table: "TimetableEntry",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableEntry_TimeSlot",
                table: "TimetableEntry",
                column: "TimeSlotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecord");

            migrationBuilder.DropTable(
                name: "ConductEvaluation");

            migrationBuilder.DropTable(
                name: "CustomerRelationship");

            migrationBuilder.DropTable(
                name: "CustomerRole");

            migrationBuilder.DropTable(
                name: "EmployeeRole");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "ScoreAuditLog");

            migrationBuilder.DropTable(
                name: "SemesterSubjectResult");

            migrationBuilder.DropTable(
                name: "StudentClass");

            migrationBuilder.DropTable(
                name: "TeacherFeedback");

            migrationBuilder.DropTable(
                name: "LeaveRequest");

            migrationBuilder.DropTable(
                name: "TimetableEntry");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "StudentScore");

            migrationBuilder.DropTable(
                name: "SemesterReport");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "TimeSlot");

            migrationBuilder.DropTable(
                name: "Assessment");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "AssessmentType");

            migrationBuilder.DropTable(
                name: "TeachingAssignment");

            migrationBuilder.DropTable(
                name: "Class");

            migrationBuilder.DropTable(
                name: "Semester");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropTable(
                name: "GradeLevel");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "AcademicYear");
        }
    }
}
