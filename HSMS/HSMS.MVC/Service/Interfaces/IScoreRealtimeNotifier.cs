namespace HSMS.MVC.Service;

/// <summary>
/// Đẩy tín hiệu realtime khi điểm của một bài kiểm tra thay đổi.
/// Gọi sau khi đã lưu DB thành công. Lỗi gửi realtime không được làm hỏng thao tác lưu điểm.
/// </summary>
public interface IScoreRealtimeNotifier
{
    Task ScoresUpdatedAsync(long assessmentId, IEnumerable<long> studentCustomerIds);
}
