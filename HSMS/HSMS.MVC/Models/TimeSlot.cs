using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class TimeSlot
{
    public long Id { get; private set; }
    public string SlotCode { get; private set; } = null!;
    public int PeriodNo { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string? Session { get; private set; }

    public virtual ICollection<TimetableEntry> TimetableEntries { get; private set; } = new List<TimetableEntry>();

    protected TimeSlot() { }

    public TimeSlot(string slotCode, int periodNo, TimeSpan startTime, TimeSpan endTime, string? session = null)
    {
        if (string.IsNullOrWhiteSpace(slotCode))
            throw new AppError("Mã tiết học không được để trống.", "EMPTY_SLOT_CODE");
        if (periodNo <= 0)
            throw new AppError("Số thứ tự tiết học phải lớn hơn 0.", "INVALID_PERIOD_NO");
        if (endTime <= startTime)
            throw new AppError("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.", "INVALID_TIME_RANGE");

        SlotCode = slotCode.Trim();
        PeriodNo = periodNo;
        StartTime = startTime;
        EndTime = endTime;
        Session = session?.Trim();
    }
}
