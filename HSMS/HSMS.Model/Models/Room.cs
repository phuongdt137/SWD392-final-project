namespace HSMS.Model.Model;

public class Room
{
    public long Id { get; set; }
    public string RoomCode { get; set; } = null!;
    public string? RoomName { get; set; }
    public string? Building { get; set; }
    public string? RoomType { get; set; }
    public int? Capacity { get; set; }
    public string Status { get; set; } = "ACTIVE";

    public virtual ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
}
