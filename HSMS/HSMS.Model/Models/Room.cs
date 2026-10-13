using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class Room
{
    public long Id { get; private set; }
    public string RoomCode { get; private set; } = null!;
    public string? RoomName { get; private set; }
    public string? Building { get; private set; }
    public string? RoomType { get; private set; }
    public int? Capacity { get; private set; }
    public string Status { get; private set; } = "ACTIVE";

    public virtual ICollection<TimetableEntry> TimetableEntries { get; private set; } = new List<TimetableEntry>();

    protected Room() { }

    public Room(string roomCode, string? roomName = null, string? building = null, string? roomType = null, int? capacity = null, string status = "ACTIVE")
    {
        if (string.IsNullOrWhiteSpace(roomCode))
            throw new BusinessException("Mã phòng học không được để trống.", "ROOM_CODE_REQUIRED");

        if (capacity.HasValue && capacity.Value < 0)
            throw new BusinessException("Sức chứa phòng học phải >= 0.", "INVALID_CAPACITY");

        RoomCode = roomCode.Trim().ToUpperInvariant();
        RoomName = roomName;
        Building = building;
        RoomType = roomType;
        Capacity = capacity;
        Status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();
    }

    public bool IsAvailable(long timeSlotId, byte dayOfWeek)
    {
        if (Status != "ACTIVE")
            return false;

        return !TimetableEntries.Any(te => te.Status == "ACTIVE" && te.TimeSlotId == timeSlotId && te.DayOfWeek == dayOfWeek);
    }
}
