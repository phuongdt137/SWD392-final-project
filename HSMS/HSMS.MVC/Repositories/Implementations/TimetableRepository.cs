using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Data;
using HSMS.MVC.DTOs;
using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HSMS.MVC.Repositories.Implementations;

public class TimetableRepository : ITimetableRepository
{
    private const string Active = "ACTIVE";
    private readonly AppDbContext _db;

    public TimetableRepository(AppDbContext db)
    {
        _db = db;
    }

    // =====================================================================
    // BƯỚC 4-6: validate required fields / references
    // Trả về danh sách lỗi. Danh sách rỗng = hợp lệ.
    // =====================================================================
    public List<string> Validate(CreateAssignmentRequest r)
    {
        var errors = new List<string>();

        // 1) Kiểm tra trường bắt buộc (chưa cần chạm DB)
        if (r.EmployeeId <= 0) errors.Add("Chưa chọn giáo viên.");
        if (r.SubjectId <= 0) errors.Add("Chưa chọn môn học.");
        if (r.ClassId <= 0) errors.Add("Chưa chọn lớp.");
        if (r.SemesterId <= 0) errors.Add("Chưa chọn học kỳ.");
        if (r.RoomId <= 0) errors.Add("Chưa chọn phòng học.");
        if (r.TimeSlotId <= 0) errors.Add("Chưa chọn tiết học.");
        if (r.DayOfWeek < 1 || r.DayOfWeek > 7) errors.Add("Ngày trong tuần không hợp lệ.");
        if (r.EffectiveFrom != null && r.EffectiveTo != null && r.EffectiveFrom > r.EffectiveTo)
            errors.Add("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");

        if (errors.Count > 0) return errors;

        // 2) Kiểm tra các id có thật trong database (references)
        if (!_db.Employees.Any(x => x.Id == r.EmployeeId && x.IsActive && x.EmployeeRoles.Any(ur => ur.IsActive && ur.Role.IsActive && ur.Role.RoleCode == "TEACHER"))) errors.Add("Giáo viên không tồn tại hoặc đã nghỉ.");
        if (!_db.Subjects.Any(x => x.Id == r.SubjectId)) errors.Add("Môn học không tồn tại.");
        if (!_db.Classes.Any(x => x.Id == r.ClassId && x.Status == Active)) errors.Add("Lớp không tồn tại hoặc không hoạt động.");
        if (!_db.Semesters.Any(x => x.Id == r.SemesterId)) errors.Add("Học kỳ không tồn tại.");
        if (!_db.Rooms.Any(x => x.Id == r.RoomId && x.Status == Active)) errors.Add("Phòng học không tồn tại hoặc không hoạt động.");
        if (!_db.TimeSlots.Any(x => x.Id == r.TimeSlotId)) errors.Add("Tiết học không tồn tại.");

        if (errors.Count > 0) return errors;
        if (_db.TeachingAssignments.Any(x => x.EmployeeId == r.EmployeeId && x.SubjectId == r.SubjectId
            && x.ClassId == r.ClassId && x.SemesterId == r.SemesterId && x.Status != Active))
            errors.Add("Phân công này đã ngừng hoạt động. Vui lòng chọn phân công khác.");
        var semester = _db.Semesters.Single(x => x.Id == r.SemesterId);
        var schoolClass = _db.Classes.Single(x => x.Id == r.ClassId);
        if (schoolClass.AcademicYearId != semester.AcademicYearId)
            errors.Add("Lớp và học kỳ phải thuộc cùng năm học.");
        r.EffectiveFrom = (r.EffectiveFrom ?? semester.StartDate).Date;
        r.EffectiveTo = (r.EffectiveTo ?? semester.EndDate).Date;
        if (r.EffectiveFrom < semester.StartDate || r.EffectiveTo > semester.EndDate || r.EffectiveFrom > r.EffectiveTo)
            errors.Add("Khoảng áp dụng phải nằm trong học kỳ và ngày bắt đầu không sau ngày kết thúc.");
        if (errors.Count == 0 && !HasLessonDate(r.EffectiveFrom.Value, r.EffectiveTo.Value, r.DayOfWeek))
            errors.Add("Khoảng ngày đã chọn không có ngày học theo thứ này.");
        var slot = _db.TimeSlots.Single(x => x.Id == r.TimeSlotId);
        if (slot.EndTime <= slot.StartTime) errors.Add("Tiết học có giờ không hợp lệ.");
        return errors;
    }

    // Có ít nhất một ngày đúng thứ trong khoảng đóng [from, to].
    private static bool HasLessonDate(DateTime from, DateTime to, int weekday) =>
        (to.Date - from.Date).Days >= (weekday - HSMS.MVC.Common.SchoolTime.Weekday(from) + 7) % 7;

    // =====================================================================
    // BƯỚC 7-10: tìm lịch bị trùng
    // Trùng nếu: cùng thứ + giờ chồng nhau + thời gian hiệu lực chồng nhau
    //            + (cùng giáo viên HOẶC cùng lớp HOẶC cùng phòng).
    // Trả về 1 lịch bị trùng, hoặc null nếu không trùng.
    // =====================================================================
    public TimetableEntry? FindScheduleConflict(CreateAssignmentRequest r)
    {
        var slot = _db.TimeSlots.First(x => x.Id == r.TimeSlotId); // giờ bắt đầu/kết thúc của tiết mới
        var dow = (byte)r.DayOfWeek;

        var candidates = _db.TimetableEntries
            .Include(e => e.TeachingAssignment)
            .Where(e => e.Status == Active
                && e.TeachingAssignment.Status == Active
                && e.DayOfWeek == dow
                // giờ chồng nhau: bắt đầu cũ < kết thúc mới VÀ kết thúc cũ > bắt đầu mới
                && e.TimeSlot.StartTime < slot.EndTime
                && e.TimeSlot.EndTime > slot.StartTime
                // khoảng ngày hiệu lực chồng nhau (null = không giới hạn)
                && (e.EffectiveFrom == null || r.EffectiveTo == null || e.EffectiveFrom <= r.EffectiveTo)
                && (e.EffectiveTo == null || r.EffectiveFrom == null || e.EffectiveTo >= r.EffectiveFrom)
                // trùng giáo viên / lớp / phòng
                && (e.TeachingAssignment.EmployeeId == r.EmployeeId
                    || e.TeachingAssignment.ClassId == r.ClassId
                    || e.RoomId == r.RoomId))
            .Include(e => e.TeachingAssignment.Semester)
            .ToList();
        return candidates.FirstOrDefault(e =>
        {
            var semester = e.TeachingAssignment.Semester;
            var from = new[] { r.EffectiveFrom!.Value, e.EffectiveFrom ?? semester.StartDate, semester.StartDate }.Max();
            var to = new[] { r.EffectiveTo!.Value, e.EffectiveTo ?? semester.EndDate, semester.EndDate }.Min();
            return from <= to && HasLessonDate(from, to, r.DayOfWeek);
        });
    }

    // =====================================================================
    // BƯỚC 11-14: INSERT TeachingAssignment, trả về assignmentId
    // DB có unique (Employee, Subject, Class, Semester) nên nếu đã có thì dùng lại id cũ.
    // =====================================================================
    public long SaveAssignment(TeachingAssignment a)
    {
        var existing = _db.TeachingAssignments.FirstOrDefault(x =>
            x.EmployeeId == a.EmployeeId && x.SubjectId == a.SubjectId &&
            x.ClassId == a.ClassId && x.SemesterId == a.SemesterId);

        if (existing != null) return existing.Id;

        _db.TeachingAssignments.Add(a);
        _db.SaveChanges();
        return a.Id;
    }

    // =====================================================================
    // BƯỚC 15-18: INSERT TimetableEntry, trả về timetableId
    // =====================================================================
    public long SaveTimetableEntry(TimetableEntry entry)
    {
        _db.TimetableEntries.Add(entry);
        _db.SaveChanges();
        return entry.Id;
    }

    public IDbContextTransaction BeginTransaction() => _db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);

    // =====================================================================
    // XEM LỊCH: lấy các tiết đang hoạt động, lọc theo giáo viên và/hoặc lớp
    // =====================================================================
    public List<TimetableEntry> GetEntries(long? employeeId, long? classId, DateTime monday, long? customerId)
    {
        var query = _db.TimetableEntries
            .Include(e => e.TeachingAssignment).ThenInclude(a => a.Employee)
            .Include(e => e.TeachingAssignment).ThenInclude(a => a.Subject)
            .Include(e => e.TeachingAssignment).ThenInclude(a => a.Class)
            .Include(e => e.Room)
            .Include(e => e.TimeSlot)
            .Where(e => e.Status == Active && e.TeachingAssignment.Status == Active && e.TeachingAssignment.Class.Status == Active);

        if (employeeId.HasValue)
            query = query.Where(e => e.TeachingAssignment.EmployeeId == employeeId.Value);
        if (classId.HasValue)
            query = query.Where(e => e.TeachingAssignment.ClassId == classId.Value);

        // Thu hẹp theo tuần ở SQL, sau đó kiểm tra ngày thực tế của từng tiết.
        var sunday = monday.AddDays(6);
        var entries = query.Include(e => e.TeachingAssignment.Semester)
            .Where(e => (e.EffectiveFrom == null || e.EffectiveFrom <= sunday)
                && (e.EffectiveTo == null || e.EffectiveTo >= monday)
                && e.TeachingAssignment.Semester.StartDate <= sunday
                && e.TeachingAssignment.Semester.EndDate >= monday)
            .AsNoTracking().OrderBy(e => e.DayOfWeek).ThenBy(e => e.TimeSlot.PeriodNo).ToList();
        var enrollments = customerId.HasValue ? _db.StudentClasses.AsNoTracking()
            .Where(x => x.CustomerId == customerId && x.JoinedAt <= sunday
                && (x.LeftAt == null || x.LeftAt > monday)
                && (x.Status == Active || x.Status == "TRANSFERRED" || x.Status == "WITHDRAWN"))
            .ToList() : new List<StudentClass>();
        return entries.Where(e =>
        {
            var date = monday.AddDays(e.DayOfWeek - 1);
            var semester = e.TeachingAssignment.Semester;
            return date >= (e.EffectiveFrom ?? semester.StartDate) && date <= (e.EffectiveTo ?? semester.EndDate)
                && date >= semester.StartDate && date <= semester.EndDate
                && (!customerId.HasValue || enrollments.Any(x => x.ClassId == e.TeachingAssignment.ClassId
                    && x.JoinedAt <= date && (x.LeftAt == null || date < x.LeftAt)));
        }).ToList();
    }

    public string? GetEmployeeName(long employeeId) =>
        _db.Employees.Where(x => x.Id == employeeId).Select(x => x.FullName).FirstOrDefault();

    public string? GetStudentName(long customerId) =>
        _db.Customers.Where(x => x.Id == customerId).Select(x => x.FullName).FirstOrDefault();

    public string? GetClassName(long classId) =>
        _db.Classes.Where(x => x.Id == classId).Select(x => x.ClassName).FirstOrDefault();

    // ---- Dropdown ----
    public List<Employee> GetEmployees() => _db.Employees.Where(x => x.IsActive && x.EmployeeRoles.Any(ur => ur.IsActive && ur.Role.IsActive && ur.Role.RoleCode == "TEACHER")).OrderBy(x => x.FullName).ToList();
    public List<Subject> GetSubjects() => _db.Subjects.OrderBy(x => x.SubjectName).ToList();
    public List<Class> GetClasses() => _db.Classes.Where(x => x.Status == Active).OrderBy(x => x.ClassName).ToList();
    public List<Semester> GetSemesters() => _db.Semesters.OrderByDescending(x => x.Id).ToList();
    public List<Room> GetRooms() => _db.Rooms.Where(x => x.Status == Active).OrderBy(x => x.RoomCode).ToList();
    public List<TimeSlot> GetTimeSlots() => _db.TimeSlots.OrderBy(x => x.PeriodNo).ToList();
}




