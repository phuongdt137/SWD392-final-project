using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Timetable;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

/// <summary>
/// Thực thi Repository cho phân hệ Thời khóa biểu (TimetableRepo).
/// </summary>
public class TimetableRepository : GenericRepository<TimetableEntry>, ITimetableRepository
{
    public TimetableRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<ValidationResultDto> ValidateRequestAsync(CreateTeachingAssignmentRequest request)
    {
        var errors = new List<string>();

        if (request == null)
        {
            errors.Add("Yêu cầu không được để trống.");
            return ValidationResultDto.Invalid(errors);
        }

        // 1. Kiểm tra ngày hiệu lực
        if (request.EffectiveFrom.HasValue && request.EffectiveTo.HasValue && request.EffectiveTo.Value < request.EffectiveFrom.Value)
        {
            errors.Add("Ngày kết thúc hiệu lực phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        // 2. Kiểm tra Thứ trong tuần
        if (request.DayOfWeek < 1 || request.DayOfWeek > 7)
        {
            errors.Add("Thứ trong tuần không hợp lệ (phải từ 1 đến 7).");
        }

        // 3. Kiểm tra giáo viên tồn tại và đang hoạt động
        var teacher = await _context.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.TeacherId);
        if (teacher == null)
        {
            errors.Add($"Không tìm thấy giáo viên với mã ID {request.TeacherId}.");
        }
        else if (teacher.User != null && !teacher.User.IsActive)
        {
            errors.Add($"Giáo viên '{teacher.FullName}' hiện không ở trạng thái hoạt động.");
        }

        // 4. Kiểm tra môn học tồn tại
        var subjectExists = await _context.Subjects
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.SubjectId);
        if (!subjectExists)
        {
            errors.Add($"Không tìm thấy môn học với mã ID {request.SubjectId}.");
        }

        // 5. Kiểm tra lớp học tồn tại
        var classObj = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ClassId);
        if (classObj == null)
        {
            errors.Add($"Không tìm thấy lớp học với mã ID {request.ClassId}.");
        }
        else if (classObj.Status != "ACTIVE")
        {
            errors.Add($"Lớp học '{classObj.ClassName}' không ở trạng thái hoạt động.");
        }

        // 6. Kiểm tra học kỳ tồn tại
        var semesterExists = await _context.Semesters
            .AsNoTracking()
            .AnyAsync(s => s.Id == request.SemesterId);
        if (!semesterExists)
        {
            errors.Add($"Không tìm thấy học kỳ với mã ID {request.SemesterId}.");
        }

        // 7. Kiểm tra phòng học tồn tại và hoạt động
        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoomId);
        if (room == null)
        {
            errors.Add($"Không tìm thấy phòng học với mã ID {request.RoomId}.");
        }
        else if (room.Status != "ACTIVE")
        {
            errors.Add($"Phòng học '{room.RoomCode}' không ở trạng thái hoạt động.");
        }

        // 8. Kiểm tra tiết học tồn tại
        var slotExists = await _context.TimeSlots
            .AsNoTracking()
            .AnyAsync(ts => ts.Id == request.TimeSlotId);
        if (!slotExists)
        {
            errors.Add($"Không tìm thấy tiết học với mã ID {request.TimeSlotId}.");
        }

        return errors.Count == 0 ? ValidationResultDto.Valid() : ValidationResultDto.Invalid(errors);
    }

    /// <inheritdoc />
    public async Task<bool> FindScheduleConflictAsync(
        long teacherId,
        long classId,
        long roomId,
        byte dayOfWeek,
        long timeSlotId,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        long? excludeTimetableId = null)
    {
        // Lấy tất cả các mục thời khóa biểu đang hoạt động cùng Thứ và Tiết học
        var query = _context.TimetableEntries
            .AsNoTracking()
            .Include(te => te.TeachingAssignment)
            .Where(te => te.Status == "ACTIVE"
                      && te.DayOfWeek == dayOfWeek
                      && te.TimeSlotId == timeSlotId);

        if (excludeTimetableId.HasValue)
        {
            query = query.Where(te => te.Id != excludeTimetableId.Value);
        }

        var candidates = await query.ToListAsync();

        foreach (var entry in candidates)
        {
            // Kiểm tra giao thoa khoảng thời gian hiệu lực
            bool datesOverlap = (!effectiveFrom.HasValue || !entry.EffectiveTo.HasValue || effectiveFrom.Value <= entry.EffectiveTo.Value)
                             && (!effectiveTo.HasValue || !entry.EffectiveFrom.HasValue || effectiveTo.Value >= entry.EffectiveFrom.Value);

            if (!datesOverlap)
            {
                continue;
            }

            // Xung đột 1: Giáo viên đã có lịch dạy vào thời điểm này
            if (entry.TeachingAssignment != null && entry.TeachingAssignment.EmployeeId == teacherId)
            {
                return true;
            }

            // Xung đột 2: Lớp học đã có tiết học vào thời điểm này
            if (entry.TeachingAssignment != null && entry.TeachingAssignment.ClassId == classId)
            {
                return true;
            }

            // Xung đột 3: Phòng học đã có lớp khác sử dụng vào thời điểm này
            if (entry.RoomId == roomId)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<long> SaveAssignmentAsync(TeachingAssignment assignment)
    {
        // Kiểm tra xem đã có TeachingAssignment tương ứng cho bộ (Employee, Subject, Class, Semester) chưa
        var existing = await _context.TeachingAssignments
            .FirstOrDefaultAsync(ta => ta.EmployeeId == assignment.EmployeeId
                                    && ta.SubjectId == assignment.SubjectId
                                    && ta.ClassId == assignment.ClassId
                                    && ta.SemesterId == assignment.SemesterId);

        if (existing != null)
        {
            if (existing.Status != "ACTIVE")
            {
                existing.Activate();
                await _context.SaveChangesAsync();
            }
            return existing.Id;
        }

        _context.TeachingAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment.Id;
    }

    /// <inheritdoc />
    public async Task<long> SaveTimetableEntryAsync(TimetableEntry entry)
    {
        _context.TimetableEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry.Id;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimetableItemDto>> GetTimetableItemsAsync(TimetableFilterDto filter)
    {
        var query = _context.TimetableEntries
            .AsNoTracking()
            .Include(te => te.TeachingAssignment).ThenInclude(ta => ta.Employee).ThenInclude(e => e.User)
            .Include(te => te.TeachingAssignment).ThenInclude(ta => ta.Subject)
            .Include(te => te.TeachingAssignment).ThenInclude(ta => ta.Class)
            .Include(te => te.TeachingAssignment).ThenInclude(ta => ta.Semester)
            .Include(te => te.Room)
            .Include(te => te.TimeSlot)
            .Where(te => te.Status == "ACTIVE");

        if (filter.SemesterId.HasValue && filter.SemesterId.Value > 0)
        {
            query = query.Where(te => te.TeachingAssignment.SemesterId == filter.SemesterId.Value);
        }

        if (filter.ClassId.HasValue && filter.ClassId.Value > 0)
        {
            query = query.Where(te => te.TeachingAssignment.ClassId == filter.ClassId.Value);
        }

        if (filter.TeacherId.HasValue && filter.TeacherId.Value > 0)
        {
            query = query.Where(te => te.TeachingAssignment.EmployeeId == filter.TeacherId.Value);
        }

        if (filter.RoomId.HasValue && filter.RoomId.Value > 0)
        {
            query = query.Where(te => te.RoomId == filter.RoomId.Value);
        }

        var list = await query
            .OrderBy(te => te.DayOfWeek)
            .ThenBy(te => te.TimeSlot.PeriodNo)
            .ToListAsync();

        return list.Select(MapToItemDto).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimetableItemDto>> GetByTeacherAsync(long employeeId, long? semesterId = null)
    {
        var filter = new TimetableFilterDto
        {
            TeacherId = employeeId,
            SemesterId = semesterId
        };
        return await GetTimetableItemsAsync(filter);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimetableItemDto>> GetByClassAsync(long classId, long? semesterId = null)
    {
        var filter = new TimetableFilterDto
        {
            ClassId = classId,
            SemesterId = semesterId
        };
        return await GetTimetableItemsAsync(filter);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimetableItemDto>> GetByStudentAsync(long customerId, long? semesterId = null)
    {
        var currentClass = await GetCurrentClassOfStudentAsync(customerId);
        if (currentClass == null)
        {
            return Array.Empty<TimetableItemDto>();
        }

        return await GetByClassAsync(currentClass.Id, semesterId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Class>> GetActiveClassesAsync()
    {
        return await _context.Classes
            .AsNoTracking()
            .Where(c => c.Status == "ACTIVE")
            .OrderBy(c => c.ClassName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Employee>> GetActiveTeachersAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .Where(e => e.User == null || e.User.IsActive)
            .OrderBy(e => e.User != null ? e.User.FullName : e.EmployeeCode)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Subject>> GetSubjectsAsync()
    {
        return await _context.Subjects
            .AsNoTracking()
            .OrderBy(s => s.SubjectName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Semester>> GetSemestersAsync()
    {
        return await _context.Semesters
            .AsNoTracking()
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Semester?> GetCurrentSemesterAsync()
    {
        var now = DateTime.UtcNow;
        var semester = await _context.Semesters
            .AsNoTracking()
            .Where(s => s.StartDate <= now && s.EndDate >= now)
            .FirstOrDefaultAsync();

        if (semester != null) return semester;

        return await _context.Semesters
            .AsNoTracking()
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Room>> GetActiveRoomsAsync()
    {
        return await _context.Rooms
            .AsNoTracking()
            .Where(r => r.Status == "ACTIVE")
            .OrderBy(r => r.RoomCode)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TimeSlot>> GetTimeSlotsAsync()
    {
        return await _context.TimeSlots
            .AsNoTracking()
            .OrderBy(ts => ts.PeriodNo)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<TeachingAssignment?> GetAssignmentByIdAsync(long id)
    {
        return await _context.TeachingAssignments
            .Include(ta => ta.Employee).ThenInclude(e => e.User)
            .Include(ta => ta.Subject)
            .Include(ta => ta.Class)
            .Include(ta => ta.Semester)
            .FirstOrDefaultAsync(ta => ta.Id == id);
    }

    /// <inheritdoc />
    public async Task<TimetableEntry?> GetTimetableEntryByIdAsync(long id)
    {
        return await _context.TimetableEntries
            .Include(te => te.TeachingAssignment).ThenInclude(ta => ta.Employee).ThenInclude(e => e.User)
            .Include(te => te.TeachingAssignment).ThenInclude(ta => ta.Subject)
            .Include(te => te.TeachingAssignment).ThenInclude(ta => ta.Class)
            .Include(te => te.Room)
            .Include(te => te.TimeSlot)
            .FirstOrDefaultAsync(te => te.Id == id);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Customer>> GetChildrenByParentCustomerIdAsync(long parentCustomerId)
    {
        return await _context.Customers
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.ParentId == parentCustomerId && (c.User == null || c.User.IsActive))
            .OrderBy(c => c.User.FullName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Class?> GetCurrentClassOfStudentAsync(long customerId)
    {
        var studentClass = await _context.StudentClasses
            .AsNoTracking()
            .Include(sc => sc.Class)
            .Where(sc => sc.CustomerId == customerId && sc.Status == "ACTIVE")
            .OrderByDescending(sc => sc.JoinedAt)
            .FirstOrDefaultAsync();

        return studentClass?.Class;
    }

    private static TimetableItemDto MapToItemDto(TimetableEntry entry)
    {
        return new TimetableItemDto
        {
            Id = entry.Id,
            TeachingAssignmentId = entry.TeachingAssignmentId,
            DayOfWeek = entry.DayOfWeek,
            TimeSlotId = entry.TimeSlotId,
            SlotCode = entry.TimeSlot?.SlotCode ?? string.Empty,
            PeriodNo = entry.TimeSlot?.PeriodNo ?? 0,
            StartTime = entry.TimeSlot?.StartTime ?? TimeSpan.Zero,
            EndTime = entry.TimeSlot?.EndTime ?? TimeSpan.Zero,
            SubjectId = entry.TeachingAssignment?.SubjectId ?? 0,
            SubjectCode = entry.TeachingAssignment?.Subject?.SubjectCode ?? string.Empty,
            SubjectName = entry.TeachingAssignment?.Subject?.SubjectName ?? string.Empty,
            TeacherId = entry.TeachingAssignment?.EmployeeId ?? 0,
            TeacherCode = entry.TeachingAssignment?.Employee?.EmployeeCode ?? string.Empty,
            TeacherName = entry.TeachingAssignment?.Employee?.FullName ?? string.Empty,
            ClassId = entry.TeachingAssignment?.ClassId ?? 0,
            ClassName = entry.TeachingAssignment?.Class?.ClassName ?? string.Empty,
            RoomId = entry.RoomId,
            RoomCode = entry.Room?.RoomCode ?? string.Empty,
            RoomName = entry.Room?.RoomName,
            SemesterId = entry.TeachingAssignment?.SemesterId ?? 0,
            SemesterCode = entry.TeachingAssignment?.Semester?.SemesterCode ?? string.Empty,
            EffectiveFrom = entry.EffectiveFrom,
            EffectiveTo = entry.EffectiveTo,
            Status = entry.Status
        };
    }
}

