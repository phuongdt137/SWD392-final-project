using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class TimetableEntry
{
    public long Id { get; private set; }
    public long TeachingAssignmentId { get; private set; }
    public long RoomId { get; private set; }
    public long TimeSlotId { get; private set; }
    public byte DayOfWeek { get; private set; }
    public string Status { get; private set; } = "ACTIVE";
    public DateTime? EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    public virtual TeachingAssignment TeachingAssignment { get; private set; } = null!;
    public virtual Room Room { get; private set; } = null!;
    public virtual TimeSlot TimeSlot { get; private set; } = null!;
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; private set; } = new List<AttendanceRecord>();

    protected TimetableEntry() { }

    public TimetableEntry(
        long teachingAssignmentId,
        long roomId,
        long timeSlotId,
        byte dayOfWeek,
        DateTime? effectiveFrom = null,
        DateTime? effectiveTo = null,
        string status = "ACTIVE")
    {
        if (teachingAssignmentId <= 0)
            throw new BusinessException("Phân công giảng dạy không hợp lệ.", "INVALID_TEACHING_ASSIGNMENT_ID");
        if (roomId <= 0)
            throw new BusinessException("Phòng học không hợp lệ.", "INVALID_ROOM_ID");
        if (timeSlotId <= 0)
            throw new BusinessException("Tiết học không hợp lệ.", "INVALID_TIME_SLOT_ID");
        if (dayOfWeek < 1 || dayOfWeek > 7)
            throw new BusinessException("Thứ trong tuần phải nằm trong khoảng từ 1 đến 7.", "INVALID_DAY_OF_WEEK");
        if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveTo.Value < effectiveFrom.Value)
            throw new BusinessException("Ngày kết thúc hiệu lực phải sau ngày bắt đầu hiệu lực.", "INVALID_EFFECTIVE_DATE_RANGE");

        TeachingAssignmentId = teachingAssignmentId;
        RoomId = roomId;
        TimeSlotId = timeSlotId;
        DayOfWeek = dayOfWeek;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();
    }

    public bool CheckConflict(TimetableEntry other)
    {
        if (other == null || other.Id == Id || Status != "ACTIVE" || other.Status != "ACTIVE")
            return false;

        if (DayOfWeek != other.DayOfWeek || TimeSlotId != other.TimeSlotId)
            return false;

        return RoomId == other.RoomId || TeachingAssignmentId == other.TeachingAssignmentId;
    }

    public void Reschedule(Room newRoom, TimeSlot newSlot, byte newDayOfWeek)
    {
        ArgumentNullException.ThrowIfNull(newRoom);
        ArgumentNullException.ThrowIfNull(newSlot);

        if (newDayOfWeek < 1 || newDayOfWeek > 7)
            throw new BusinessException("Thứ trong tuần phải nằm trong khoảng từ 1 đến 7.", "INVALID_DAY_OF_WEEK");

        RoomId = newRoom.Id;
        Room = newRoom;
        TimeSlotId = newSlot.Id;
        TimeSlot = newSlot;
        DayOfWeek = newDayOfWeek;
    }
}
