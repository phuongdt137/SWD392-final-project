using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.ViewModels.Attendance;

namespace HSMS.MVC.Service;

public interface IAttendanceService
{
    Task<ServiceResult<AttendanceIndex>> IndexAsync(DateTime date, CancellationToken ct = default);
    Task<ServiceResult<AttendanceIndex>> IndexAsync(AttendanceScheduleFilter filter, CancellationToken ct = default);
    Task<ServiceResult<AttendanceSheet>> SheetAsync(long entryId, DateTime date, CancellationToken ct = default);
    Task<ServiceResult<SaveAttendanceResult>> SaveAsync(SaveAttendanceDto input, CancellationToken ct = default);
    Task<ServiceResult<AttendanceHistory>> HistoryAsync(AttendanceFilter filter, CancellationToken ct = default);
    Task<ServiceResult<StudentAttendanceHistory>> StudentHistoryAsync(AttendanceFilter filter, CancellationToken ct = default);
    Task<ServiceResult<AttendanceAudit>> AuditAsync(long recordId, CancellationToken ct = default);
    Task<AttendanceSummary> DashboardAsync(CancellationToken ct = default);
}
