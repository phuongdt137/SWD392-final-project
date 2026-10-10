namespace HSMS.Model.Model;

public class TimeSlot
{
    public long Id { get; set; }
    public string SlotCode { get; set; } = null!;
    public int PeriodNo { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Session { get; set; }

    public virtual ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
}
