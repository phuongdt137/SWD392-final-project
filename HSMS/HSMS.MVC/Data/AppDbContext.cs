using Microsoft.EntityFrameworkCore;
using HSMS.MVC.Models;

namespace HSMS.MVC.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<GradeLevel> GradeLevels => Set<GradeLevel>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeachingAssignment> TeachingAssignments => Set<TeachingAssignment>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AssessmentType> AssessmentTypes => Set<AssessmentType>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<StudentScore> StudentScores => Set<StudentScore>();
    public DbSet<ScoreAuditLog> ScoreAuditLogs => Set<ScoreAuditLog>();
    public DbSet<ConductEvaluation> ConductEvaluations => Set<ConductEvaluation>();
    public DbSet<SemesterReport> SemesterReports => Set<SemesterReport>();
    public DbSet<SemesterSubjectResult> SemesterSubjectResults => Set<SemesterSubjectResult>();
    public DbSet<TeacherFeedback> TeacherFeedbacks => Set<TeacherFeedback>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<MessageLog> MessageLogs => Set<MessageLog>();
    public DbSet<StudentTransfer> StudentTransfers => Set<StudentTransfer>();
    public DbSet<StudentAnnualReport> StudentAnnualReports => Set<StudentAnnualReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================
        // 1. ROLE
        // ============================================================
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(e => e.Id).HasName("PK_Role");
            entity.Property(e => e.RoleCode).HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.RoleName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => e.RoleCode, "UQ_Role_RoleCode").IsUnique();

            // Default roles seed data
            entity.HasData(
                new Role(1, "STUDENT", "Student", "High school student", true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new Role(2, "PARENT", "Parent", "Parent or guardian of student", true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new Role(3, "TEACHER", "Teacher", "School teacher", true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new Role(4, "ADMIN", "Administrator", "System administrator", true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
                new Role(5, "STAFF", "Staff", "School staff", true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc))
            );
        });

        // ============================================================
        // 2. USER
        // ============================================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.Id).HasName("PK_User");
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            entity.Property(e => e.LastLoginAt);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.PasswordResetTokenExpiry);

            entity.HasIndex(e => e.Username, "UQ_User_Username").IsUnique();
            entity.HasIndex(e => e.Email, "UQ_User_Email").IsUnique();
        });

        // ============================================================
        // 3. USER ROLE
        // ============================================================
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRole");
            entity.HasKey(e => e.Id).HasName("PK_UserRole");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.AssignedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UQ_UserRole").IsUnique();
            entity.HasIndex(e => e.UserId, "IX_UserRole_UserId");
            entity.HasIndex(e => e.RoleId, "IX_UserRole_RoleId");

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("FK_UserRole_User")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .HasConstraintName("FK_UserRole_Role")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 4. CUSTOMER
        // ============================================================
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customer", t =>
            {
                t.HasCheckConstraint("CK_Customer_ParentNotSelf", "[ParentId] IS NULL OR [ParentId] <> [Id]");
            });
            entity.HasKey(e => e.Id).HasName("PK_Customer");
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.DateOfBirth).HasColumnType("date");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.StudentCode).HasMaxLength(50);
            entity.Property(e => e.AdmissionDate).HasColumnType("date");
            entity.Property(e => e.Occupation).HasMaxLength(150);
            entity.Property(e => e.EmergencyContact).HasMaxLength(255);
            entity.Property(e => e.RelationshipType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt);

            entity.HasIndex(e => e.UserId, "UQ_Customer_UserId").IsUnique();
            entity.HasIndex(e => e.StudentCode, "UX_Customer_StudentCode").IsUnique().HasFilter("[StudentCode] IS NOT NULL");
            entity.HasIndex(e => e.ParentId, "IX_Customer_ParentId");

            entity.HasOne(e => e.User)
                .WithOne(u => u.CustomerProfile)
                .HasForeignKey<Customer>(e => e.UserId)
                .HasConstraintName("FK_Customer_User")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(e => e.ParentId)
                .HasConstraintName("FK_Customer_Parent")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============================================================
        // 5. EMPLOYEE
        // ============================================================
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee", t =>
            {
                t.HasCheckConstraint("CK_Employee_MaxPeriodsPerWeek", "[MaxPeriodsPerWeek] IS NULL OR [MaxPeriodsPerWeek] >= 0");
            });
            entity.HasKey(e => e.Id).HasName("PK_Employee");
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.DateOfBirth).HasColumnType("date");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Department).HasMaxLength(150);
            entity.Property(e => e.Position).HasMaxLength(150);
            entity.Property(e => e.Specialization).HasMaxLength(255);
            entity.Property(e => e.Qualification).HasMaxLength(255);
            entity.Property(e => e.HireDate).HasColumnType("date");
            entity.Property(e => e.UpdatedAt);

            entity.HasIndex(e => e.UserId, "UQ_Employee_UserId").IsUnique();
            entity.HasIndex(e => e.EmployeeCode, "UQ_Employee_EmployeeCode").IsUnique();

            entity.HasOne(e => e.User)
                .WithOne(u => u.EmployeeProfile)
                .HasForeignKey<Employee>(e => e.UserId)
                .HasConstraintName("FK_Employee_User")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================================
        // 7. ACADEMIC YEAR
        // ============================================================
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.ToTable("AcademicYear", t =>
            {
                t.HasCheckConstraint("CK_AcademicYear_Date", "[EndDate] >= [StartDate]");
            });
            entity.HasKey(e => e.Id).HasName("PK_AcademicYear");
            entity.Property(e => e.YearCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnType("date");

            entity.HasIndex(e => e.YearCode, "UQ_AcademicYear_YearCode").IsUnique();
        });

        // ============================================================
        // 8. SEMESTER
        // ============================================================
        modelBuilder.Entity<Semester>(entity =>
        {
            entity.ToTable("Semester", t =>
            {
                t.HasCheckConstraint("CK_Semester_Date", "[EndDate] >= [StartDate]");
            });
            entity.HasKey(e => e.Id).HasName("PK_Semester");
            entity.Property(e => e.SemesterCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.SemesterName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnType("date");

            entity.HasIndex(e => e.SemesterCode, "UQ_Semester_SemesterCode").IsUnique();
            entity.HasIndex(e => e.AcademicYearId, "IX_Semester_AcademicYear");

            entity.HasOne(e => e.AcademicYear)
                .WithMany(a => a.Semesters)
                .HasForeignKey(e => e.AcademicYearId)
                .HasConstraintName("FK_Semester_AcademicYear")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 9. GRADE LEVEL
        // ============================================================
        modelBuilder.Entity<GradeLevel>(entity =>
        {
            entity.ToTable("GradeLevel");
            entity.HasKey(e => e.Id).HasName("PK_GradeLevel");
            entity.Property(e => e.LevelCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.LevelName).HasMaxLength(100).IsRequired();

            entity.HasIndex(e => e.LevelCode, "UQ_GradeLevel_LevelCode").IsUnique();
        });

        // ============================================================
        // 10. CLASS
        // ============================================================
        modelBuilder.Entity<Class>(entity =>
        {
            entity.ToTable("Class");
            entity.HasKey(e => e.Id).HasName("PK_Class");
            entity.Property(e => e.ClassName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => e.AcademicYearId, "IX_Class_AcademicYear");
            entity.HasIndex(e => e.GradeLevelId, "IX_Class_GradeLevel");
            entity.HasIndex(e => e.HomeroomEmployeeId, "IX_Class_HomeroomEmployee");

            entity.HasOne(e => e.HomeroomEmployee)
                .WithMany(emp => emp.HomeroomClasses)
                .HasForeignKey(e => e.HomeroomEmployeeId)
                .HasConstraintName("FK_Class_HomeroomEmployee")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.AcademicYear)
                .WithMany(a => a.Classes)
                .HasForeignKey(e => e.AcademicYearId)
                .HasConstraintName("FK_Class_AcademicYear")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.GradeLevel)
                .WithMany(g => g.Classes)
                .HasForeignKey(e => e.GradeLevelId)
                .HasConstraintName("FK_Class_GradeLevel")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 11. STUDENT CLASS
        // ============================================================
        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.ToTable("StudentClass", t =>
            {
                t.HasCheckConstraint("CK_StudentClass_Date", "[LeftAt] IS NULL OR [LeftAt] >= [JoinedAt]");
            });
            entity.HasKey(e => e.Id).HasName("PK_StudentClass");
            entity.Property(e => e.JoinedAt).HasColumnType("date");
            entity.Property(e => e.LeftAt).HasColumnType("date");
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => e.CustomerId, "IX_StudentClass_Customer");
            entity.HasIndex(e => e.ClassId, "IX_StudentClass_Class");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.StudentClasses)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("FK_StudentClass_Customer")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Class)
                .WithMany(cls => cls.StudentClasses)
                .HasForeignKey(e => e.ClassId)
                .HasConstraintName("FK_StudentClass_Class")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 12. SUBJECT
        // ============================================================
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subject");
            entity.HasKey(e => e.Id).HasName("PK_Subject");
            entity.Property(e => e.SubjectCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SubjectName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.EvaluationType).HasMaxLength(50);

            entity.HasIndex(e => e.SubjectCode, "UQ_Subject_SubjectCode").IsUnique();
        });

        // ============================================================
        // 13. TEACHING ASSIGNMENT
        // ============================================================
        modelBuilder.Entity<TeachingAssignment>(entity =>
        {
            entity.ToTable("TeachingAssignment");
            entity.HasKey(e => e.Id).HasName("PK_TeachingAssignment");
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => new { e.EmployeeId, e.SubjectId, e.ClassId, e.SemesterId }, "UQ_TeachingAssignment").IsUnique();
            entity.HasIndex(e => e.EmployeeId, "IX_TeachingAssignment_Employee");
            entity.HasIndex(e => e.SubjectId, "IX_TeachingAssignment_Subject");
            entity.HasIndex(e => e.ClassId, "IX_TeachingAssignment_Class");
            entity.HasIndex(e => e.SemesterId, "IX_TeachingAssignment_Semester");

            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.TeachingAssignments)
                .HasForeignKey(e => e.EmployeeId)
                .HasConstraintName("FK_TeachingAssignment_Employee")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Subject)
                .WithMany(s => s.TeachingAssignments)
                .HasForeignKey(e => e.SubjectId)
                .HasConstraintName("FK_TeachingAssignment_Subject")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Class)
                .WithMany(cls => cls.TeachingAssignments)
                .HasForeignKey(e => e.ClassId)
                .HasConstraintName("FK_TeachingAssignment_Class")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Semester)
                .WithMany(sem => sem.TeachingAssignments)
                .HasForeignKey(e => e.SemesterId)
                .HasConstraintName("FK_TeachingAssignment_Semester")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 14. TIME SLOT
        // ============================================================
        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.ToTable("TimeSlot", t =>
            {
                t.HasCheckConstraint("CK_TimeSlot_Time", "[EndTime] > [StartTime]");
                t.HasCheckConstraint("CK_TimeSlot_PeriodNo", "[PeriodNo] > 0");
            });
            entity.HasKey(e => e.Id).HasName("PK_TimeSlot");
            entity.Property(e => e.SlotCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Session).HasMaxLength(30);

            entity.HasIndex(e => e.SlotCode, "UQ_TimeSlot_SlotCode").IsUnique();
        });

        // ============================================================
        // 15. ROOM
        // ============================================================
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Room", t =>
            {
                t.HasCheckConstraint("CK_Room_Capacity", "[Capacity] IS NULL OR [Capacity] >= 0");
            });
            entity.HasKey(e => e.Id).HasName("PK_Room");
            entity.Property(e => e.RoomCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RoomName).HasMaxLength(150);
            entity.Property(e => e.Building).HasMaxLength(150);
            entity.Property(e => e.RoomType).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("ACTIVE");

            entity.HasIndex(e => e.RoomCode, "UQ_Room_RoomCode").IsUnique();
        });

        // ============================================================
        // 16. TIMETABLE ENTRY
        // ============================================================
        modelBuilder.Entity<TimetableEntry>(entity =>
        {
            entity.ToTable("TimetableEntry", t =>
            {
                t.HasCheckConstraint("CK_TimetableEntry_DayOfWeek", "[DayOfWeek] BETWEEN 1 AND 7");
                t.HasCheckConstraint("CK_TimetableEntry_EffectiveDate", "[EffectiveFrom] IS NULL OR [EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]");
            });
            entity.HasKey(e => e.Id).HasName("PK_TimetableEntry");
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("ACTIVE");
            entity.Property(e => e.EffectiveFrom).HasColumnType("date");
            entity.Property(e => e.EffectiveTo).HasColumnType("date");

            entity.HasIndex(e => e.TeachingAssignmentId, "IX_TimetableEntry_Assignment");
            entity.HasIndex(e => e.RoomId, "IX_TimetableEntry_Room");
            entity.HasIndex(e => e.TimeSlotId, "IX_TimetableEntry_TimeSlot");

            entity.HasOne(e => e.TeachingAssignment)
                .WithMany(ta => ta.TimetableEntries)
                .HasForeignKey(e => e.TeachingAssignmentId)
                .HasConstraintName("FK_TimetableEntry_TeachingAssignment")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.TimetableEntries)
                .HasForeignKey(e => e.RoomId)
                .HasConstraintName("FK_TimetableEntry_Room")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.TimeSlot)
                .WithMany(ts => ts.TimetableEntries)
                .HasForeignKey(e => e.TimeSlotId)
                .HasConstraintName("FK_TimetableEntry_TimeSlot")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 17. LEAVE REQUEST
        // ============================================================
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("LeaveRequest", t =>
            {
                t.HasCheckConstraint("CK_LeaveRequest_Date", "[EndDate] >= [StartDate]");
            });
            entity.HasKey(e => e.Id).HasName("PK_LeaveRequest");
            entity.Property(e => e.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Attachment).HasMaxLength(1000);
            entity.Property(e => e.StartDate).HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnType("date");
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("PENDING");
            entity.Property(e => e.RequestedAt).HasDefaultValueSql("SYSDATETIME()");
            entity.Property(e => e.ReviewNote).HasMaxLength(1000);

            entity.HasIndex(e => e.StudentCustomerId, "IX_LeaveRequest_Student");
            entity.HasIndex(e => e.RequestedByCustomerId, "IX_LeaveRequest_RequestedBy");
            entity.HasIndex(e => e.ReviewedByEmployeeId, "IX_LeaveRequest_ReviewedBy");

            entity.HasOne(e => e.StudentCustomer)
                .WithMany(c => c.StudentLeaveRequests)
                .HasForeignKey(e => e.StudentCustomerId)
                .HasConstraintName("FK_LeaveRequest_Student")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.RequestedByCustomer)
                .WithMany(c => c.RequestedLeaveRequests)
                .HasForeignKey(e => e.RequestedByCustomerId)
                .HasConstraintName("FK_LeaveRequest_RequestedBy")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.ReviewedByEmployee)
                .WithMany(emp => emp.ReviewedLeaveRequests)
                .HasForeignKey(e => e.ReviewedByEmployeeId)
                .HasConstraintName("FK_LeaveRequest_ReviewedBy")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 18. ATTENDANCE RECORD
        // ============================================================
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.ToTable("AttendanceRecord");
            entity.HasKey(e => e.Id).HasName("PK_AttendanceRecord");
            entity.Property(e => e.AttendanceDate).HasColumnType("date");
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => new { e.CustomerId, e.TimetableEntryId, e.AttendanceDate }, "UQ_AttendanceRecord").IsUnique();
            entity.HasIndex(e => e.CustomerId, "IX_AttendanceRecord_Customer");
            entity.HasIndex(e => e.RecordedByEmployeeId, "IX_AttendanceRecord_Employee");
            entity.HasIndex(e => e.TimetableEntryId, "IX_AttendanceRecord_Timetable");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.AttendanceRecords)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("FK_AttendanceRecord_Customer")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.RecordedByEmployee)
                .WithMany(emp => emp.RecordedAttendanceRecords)
                .HasForeignKey(e => e.RecordedByEmployeeId)
                .HasConstraintName("FK_AttendanceRecord_Employee")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.LeaveRequest)
                .WithMany(lr => lr.AttendanceRecords)
                .HasForeignKey(e => e.LeaveRequestId)
                .HasConstraintName("FK_AttendanceRecord_LeaveRequest")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.TimetableEntry)
                .WithMany(te => te.AttendanceRecords)
                .HasForeignKey(e => e.TimetableEntryId)
                .HasConstraintName("FK_AttendanceRecord_TimetableEntry")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 19. ASSESSMENT TYPE
        // ============================================================
        modelBuilder.Entity<AssessmentType>(entity =>
        {
            entity.ToTable("AssessmentType", t =>
            {
                t.HasCheckConstraint("CK_AssessmentType_Weight", "[Weight] >= 0 AND [Weight] <= 100");
                t.HasCheckConstraint("CK_AssessmentType_Entries", "[MinEntries] >= 0 AND ([MaxEntries] IS NULL OR [MaxEntries] >= [MinEntries])");
            });
            entity.HasKey(e => e.Id).HasName("PK_AssessmentType");
            entity.Property(e => e.TypeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TypeName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Weight).HasPrecision(5, 2);
            entity.Property(e => e.MinEntries).HasDefaultValue(1);

            entity.HasIndex(e => e.TypeCode, "UQ_AssessmentType_TypeCode").IsUnique();
        });

        // ============================================================
        // 20. ASSESSMENT
        // ============================================================
        modelBuilder.Entity<Assessment>(entity =>
        {
            entity.ToTable("Assessment");
            entity.HasKey(e => e.Id).HasName("PK_Assessment");
            entity.Property(e => e.AssessmentName).HasMaxLength(200).IsRequired();

            entity.HasIndex(e => e.TeachingAssignmentId, "IX_Assessment_Assignment");
            entity.HasIndex(e => e.AssessmentTypeId, "IX_Assessment_Type");

            entity.HasOne(e => e.TeachingAssignment)
                .WithMany(ta => ta.Assessments)
                .HasForeignKey(e => e.TeachingAssignmentId)
                .HasConstraintName("FK_Assessment_TeachingAssignment")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.AssessmentType)
                .WithMany(at => at.Assessments)
                .HasForeignKey(e => e.AssessmentTypeId)
                .HasConstraintName("FK_Assessment_AssessmentType")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 21. STUDENT SCORE
        // ============================================================
        modelBuilder.Entity<StudentScore>(entity =>
        {
            entity.ToTable("StudentScore", t =>
            {
                t.HasCheckConstraint("CK_StudentScore_Score", "[Score] IS NULL OR ([Score] >= 0 AND [Score] <= 10)");
            });
            entity.HasKey(e => e.Id).HasName("PK_StudentScore");
            entity.Property(e => e.Score).HasPrecision(5, 2);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => new { e.CustomerId, e.AssessmentId }, "UQ_StudentScore").IsUnique();
            entity.HasIndex(e => e.CustomerId, "IX_StudentScore_Customer");
            entity.HasIndex(e => e.AssessmentId, "IX_StudentScore_Assessment");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.StudentScores)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("FK_StudentScore_Customer")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Assessment)
                .WithMany(a => a.StudentScores)
                .HasForeignKey(e => e.AssessmentId)
                .HasConstraintName("FK_StudentScore_Assessment")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 22. SCORE AUDIT LOG
        // ============================================================
        modelBuilder.Entity<ScoreAuditLog>(entity =>
        {
            entity.ToTable("ScoreAuditLog");
            entity.HasKey(e => e.Id).HasName("PK_ScoreAuditLog");
            entity.Property(e => e.OldScore).HasPrecision(5, 2);
            entity.Property(e => e.NewScore).HasPrecision(5, 2);
            entity.Property(e => e.ChangeReason).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45).IsUnicode(false);
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => e.StudentScoreId, "IX_ScoreAuditLog_StudentScore");
            entity.HasIndex(e => e.ChangedByEmployeeId, "IX_ScoreAuditLog_Employee");

            entity.HasOne(e => e.StudentScore)
                .WithMany(ss => ss.ScoreAuditLogs)
                .HasForeignKey(e => e.StudentScoreId)
                .HasConstraintName("FK_ScoreAuditLog_StudentScore")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.ChangedByEmployee)
                .WithMany(emp => emp.ScoreAuditLogs)
                .HasForeignKey(e => e.ChangedByEmployeeId)
                .HasConstraintName("FK_ScoreAuditLog_Employee")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 23. CONDUCT EVALUATION
        // ============================================================
        modelBuilder.Entity<ConductEvaluation>(entity =>
        {
            entity.ToTable("ConductEvaluation");
            entity.HasKey(e => e.Id).HasName("PK_ConductEvaluation");
            entity.Property(e => e.ConductGrade).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TeacherComment).HasMaxLength(1000);
            entity.Property(e => e.EvaluatedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => new { e.CustomerId, e.SemesterId }, "UQ_ConductEvaluation").IsUnique();
            entity.HasIndex(e => e.CustomerId, "IX_ConductEvaluation_Customer");
            entity.HasIndex(e => e.SemesterId, "IX_ConductEvaluation_Semester");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.ConductEvaluations)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("FK_ConductEvaluation_Customer")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Semester)
                .WithMany(s => s.ConductEvaluations)
                .HasForeignKey(e => e.SemesterId)
                .HasConstraintName("FK_ConductEvaluation_Semester")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.HomeroomEmployee)
                .WithMany(emp => emp.ConductEvaluations)
                .HasForeignKey(e => e.HomeroomEmployeeId)
                .HasConstraintName("FK_ConductEvaluation_Employee")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 24. SEMESTER REPORT
        // ============================================================
        modelBuilder.Entity<SemesterReport>(entity =>
        {
            entity.ToTable("SemesterReport", t =>
            {
                t.HasCheckConstraint("CK_SemesterReport_GPA", "[GPA] IS NULL OR ([GPA] >= 0 AND [GPA] <= 10)");
            });
            entity.HasKey(e => e.Id).HasName("PK_SemesterReport");
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("DRAFT");
            entity.Property(e => e.GPA).HasPrecision(5, 2);
            entity.Property(e => e.AcademicRank).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => new { e.CustomerId, e.SemesterId }, "UQ_SemesterReport").IsUnique();
            entity.HasIndex(e => e.CustomerId, "IX_SemesterReport_Customer");
            entity.HasIndex(e => e.SemesterId, "IX_SemesterReport_Semester");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.SemesterReports)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("FK_SemesterReport_Customer")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Semester)
                .WithMany(s => s.SemesterReports)
                .HasForeignKey(e => e.SemesterId)
                .HasConstraintName("FK_SemesterReport_Semester")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.FinalizedByEmployee)
                .WithMany(emp => emp.FinalizedSemesterReports)
                .HasForeignKey(e => e.FinalizedByEmployeeId)
                .HasConstraintName("FK_SemesterReport_FinalizedEmployee")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 25. SEMESTER SUBJECT RESULT
        // ============================================================
        modelBuilder.Entity<SemesterSubjectResult>(entity =>
        {
            entity.ToTable("SemesterSubjectResult", t =>
            {
                t.HasCheckConstraint("CK_SemesterSubjectResult_Score", "[AverageScore] IS NULL OR ([AverageScore] >= 0 AND [AverageScore] <= 10)");
            });
            entity.HasKey(e => e.Id).HasName("PK_SemesterSubjectResult");
            entity.Property(e => e.AverageScore).HasPrecision(5, 2);
            entity.Property(e => e.EvaluationResult).HasMaxLength(100);

            entity.HasIndex(e => new { e.SemesterReportId, e.SubjectId }, "UQ_SemesterSubjectResult").IsUnique();
            entity.HasIndex(e => e.SemesterReportId, "IX_SemesterSubjectResult_Report");

            entity.HasOne(e => e.SemesterReport)
                .WithMany(r => r.SemesterSubjectResults)
                .HasForeignKey(e => e.SemesterReportId)
                .HasConstraintName("FK_SemesterSubjectResult_Report")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Subject)
                .WithMany(s => s.SemesterSubjectResults)
                .HasForeignKey(e => e.SubjectId)
                .HasConstraintName("FK_SemesterSubjectResult_Subject")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 26. TEACHER FEEDBACK
        // ============================================================
        modelBuilder.Entity<TeacherFeedback>(entity =>
        {
            entity.ToTable("TeacherFeedback");
            entity.HasKey(e => e.Id).HasName("PK_TeacherFeedback");
            entity.Property(e => e.FeedbackType).HasMaxLength(100);
            entity.Property(e => e.Comment).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => e.CustomerId, "IX_TeacherFeedback_Customer");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.TeacherFeedbacks)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("FK_TeacherFeedback_Customer")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.TeachingAssignment)
                .WithMany(ta => ta.TeacherFeedbacks)
                .HasForeignKey(e => e.TeachingAssignmentId)
                .HasConstraintName("FK_TeacherFeedback_TeachingAssignment")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 27. NOTIFICATION
        // ============================================================
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");
            entity.HasKey(e => e.Id).HasName("PK_Notification");
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SentAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => e.RecipientId, "IX_Notification_RecipientId");
            entity.HasIndex(e => e.SenderId, "IX_Notification_SenderId");
            entity.HasIndex(e => e.IsRead, "IX_Notification_IsRead");

            entity.HasOne(e => e.Recipient)
                .WithMany(u => u.ReceivedNotifications)
                .HasForeignKey(e => e.RecipientId)
                .HasConstraintName("FK_Notification_RecipientUser")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Sender)
                .WithMany(u => u.SentNotifications)
                .HasForeignKey(e => e.SenderId)
                .HasConstraintName("FK_Notification_SenderUser")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ============================================================
        // 28. MESSAGE LOG
        // ============================================================
        modelBuilder.Entity<MessageLog>(entity =>
        {
            entity.ToTable("MessageLog");
            entity.HasKey(e => e.Id).HasName("PK_MessageLog");
            entity.Property(e => e.Channel).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.RecipientAddress).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(255);
            entity.Property(e => e.MessageBody).IsRequired();
            entity.Property(e => e.TemplateCode).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("QUEUED");
            entity.Property(e => e.ExternalMessageId).HasMaxLength(255);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.RetryCount).HasDefaultValue(0);
            entity.Property(e => e.Cost).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");

            entity.HasIndex(e => e.RecipientAddress, "IX_MessageLog_RecipientAddress");
            entity.HasIndex(e => new { e.Channel, e.Status }, "IX_MessageLog_Channel_Status");
            entity.HasIndex(e => e.CreatedAt, "IX_MessageLog_CreatedAt");
            entity.HasIndex(e => e.RecipientUserId, "IX_MessageLog_RecipientUserId");
            entity.HasIndex(e => e.CreatedByUserId, "IX_MessageLog_CreatedByUserId");
            entity.HasIndex(e => e.NotificationId, "IX_MessageLog_Notification");

            entity.HasOne(e => e.RecipientUser)
                .WithMany(u => u.ReceivedMessageLogs)
                .HasForeignKey(e => e.RecipientUserId)
                .HasConstraintName("FK_MessageLog_RecipientUser")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany(u => u.CreatedMessageLogs)
                .HasForeignKey(e => e.CreatedByUserId)
                .HasConstraintName("FK_MessageLog_CreatedByUser")
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Notification)
                .WithMany(n => n.MessageLogs)
                .HasForeignKey(e => e.NotificationId)
                .HasConstraintName("FK_MessageLog_Notification")
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<StudentTransfer>(entity =>
        {
            entity.ToTable("StudentTransfer", t => t.HasCheckConstraint("CK_StudentTransfer_Type", "[TransferType] IN ('CLASS','SCHOOL_IN','SCHOOL_OUT')"));
            entity.HasKey(e => e.Id).HasName("PK_StudentTransfer");
            entity.Property(e => e.TransferType).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.FromSchoolName).HasMaxLength(200);
            entity.Property(e => e.ToSchoolName).HasMaxLength(200);
            entity.Property(e => e.EffectiveDate).HasColumnType("date");
            entity.Property(e => e.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("COMPLETED");
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("SYSDATETIME()");
            entity.HasIndex(e => new { e.CustomerId, e.EffectiveDate }, "IX_StudentTransfer_Student_Date");
            entity.HasIndex(e => e.FromClassId, "IX_StudentTransfer_FromClass");
            entity.HasIndex(e => e.ToClassId, "IX_StudentTransfer_ToClass");
            entity.HasOne(e => e.Customer).WithMany(c => c.StudentTransfers).HasForeignKey(e => e.CustomerId).HasConstraintName("FK_StudentTransfer_Customer").OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.FromClass).WithMany(c => c.TransfersFrom).HasForeignKey(e => e.FromClassId).HasConstraintName("FK_StudentTransfer_FromClass").OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.ToClass).WithMany(c => c.TransfersTo).HasForeignKey(e => e.ToClassId).HasConstraintName("FK_StudentTransfer_ToClass").OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.RecordedByEmployee).WithMany(e => e.RecordedStudentTransfers).HasForeignKey(e => e.RecordedByEmployeeId).HasConstraintName("FK_StudentTransfer_Employee").OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<StudentAnnualReport>(entity =>
        {
            entity.ToTable("StudentAnnualReport", t => t.HasCheckConstraint("CK_StudentAnnualReport_GPA", "[AnnualGpa] IS NULL OR ([AnnualGpa] >= 0 AND [AnnualGpa] <= 10)"));
            entity.HasKey(e => e.Id).HasName("PK_StudentAnnualReport");
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("DRAFT");
            entity.Property(e => e.AnnualGpa).HasPrecision(5, 2);
            entity.Property(e => e.AcademicResult).HasMaxLength(100);
            entity.Property(e => e.ConductGrade).HasMaxLength(50);
            entity.Property(e => e.TeacherComment).HasMaxLength(2000);
            entity.Property(e => e.PrincipalComment).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
            entity.HasIndex(e => new { e.CustomerId, e.AcademicYearId }, "UQ_StudentAnnualReport").IsUnique();
            entity.HasOne(e => e.Customer).WithMany(c => c.StudentAnnualReports).HasForeignKey(e => e.CustomerId).HasConstraintName("FK_StudentAnnualReport_Customer").OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.AcademicYear).WithMany(y => y.StudentAnnualReports).HasForeignKey(e => e.AcademicYearId).HasConstraintName("FK_StudentAnnualReport_AcademicYear").OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(e => e.FinalizedByEmployee).WithMany(e => e.FinalizedStudentAnnualReports).HasForeignKey(e => e.FinalizedByEmployeeId).HasConstraintName("FK_StudentAnnualReport_FinalizedEmployee").OnDelete(DeleteBehavior.ClientSetNull);
        });
    }
}
