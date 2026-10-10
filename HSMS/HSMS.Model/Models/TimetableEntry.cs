namespace HSMS.Model.Model;

public class TimetableEntry
{
    public long Id { get; set; }
    public long TeachingAssignmentId { get; set; }
    public long RoomId { get; set; }
    public long TimeSlotId { get; set; }
    public byte DayOfWeek { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public virtual TeachingAssignment TeachingAssignment { get; set; } = null!;
    public virtual Room Room { get; set; } = null!;
    public virtual TimeSlot TimeSlot { get; set; } = null!;
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
