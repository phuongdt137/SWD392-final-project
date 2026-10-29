# Kiểm tra Attendance

Chạy từ thư mục gốc dự án:

```powershell
dotnet run --project tests/HSMS.Attendance.Checks/HSMS.Attendance.Checks.csproj
```

Bộ kiểm tra dùng tài khoản, dữ liệu và đồng hồ giả lập. Không chạy Program.cs của ứng dụng thật, không kết nối SQL Server, không chạy seed/migration và không gửi email.

- Kiểm tra quyền giáo viên, Staff, Principal, học sinh và phụ huynh.
- Quyền vào action được kiểm tra qua HTTP/Controller; các kiểm tra trực tiếp service tập trung vào phạm vi dữ liệu và nghiệp vụ. Staff/Principal xem được nhật ký, còn giáo viên/học sinh/phụ huynh bị chặn bởi [Authorize].
- Service dùng danh tính đã đăng nhập từ Session, không truy vấn lại tài khoản. Pipeline dùng SessionAuthMiddleware hiện có để kiểm tra người chưa đăng nhập bị chặn và biểu mẫu không thay được danh tính/quyền người ghi.
- Giáo viên không xem được lịch sử/nhật ký; Staff/Principal vẫn xem được. Học sinh dùng bốn bộ lọc học kỳ/môn/ngày/trạng thái; phụ huynh thêm chọn con, không chọn lớp.
- Kiểm tra lựa chọn học kỳ/môn theo từng con, học kỳ mặc định, lựa chọn không còn hợp lệ quay về mặc định, ngày không có dữ liệu trả danh sách rỗng, liên kết thông báo cũ và phân trang giữ bộ lọc.
- Kiểm tra chính xác trước/đúng 00:00 ngày kế tiếp và trước/đúng hạn hết học kỳ.
- Tiết 09:00–09:45 được ghi từ đúng 09:00, vẫn ghi được sau 09:45 đến 23:59:59; khóa ở 00:00 ngày kế tiếp theo giờ Việt Nam.
- Đơn nghỉ đã duyệt đúng học sinh/ngày tự điền EXCUSED và chọn đơn trong biểu mẫu; mở bảng không tự lưu. Khi lưu bắt buộc trạng thái và đơn hợp lệ. Kiểm tra cả hai mốc ngày bắt đầu/kết thúc, đơn chưa duyệt/từ chối/ngoài ngày và lý do sửa trạng thái/ghi chú/đơn.
- Màn hình giáo viên hiển thị các tiết hôm nay theo giờ Việt Nam, không có bộ lọc. Mở bảng không còn kiểm tra chặn ngày khác/phân công; nghiệp vụ giờ bắt đầu và khóa 00:00 vẫn áp dụng khi lưu. Staff/Principal giữ bộ lọc hiện có.
- Chọn thiếu trạng thái hoặc thiếu dòng của học sinh thì báo lỗi trước khi lưu; sửa kết quả chỉ cần học sinh của bảng đang mở, biểu mẫu không gửi RecordId.
- Kiểm tra Staff chỉ sửa kết quả đã có, lý do sửa, đơn nghỉ, nhật ký và rollback khi lưu lỗi/qua hạn.
- Kiểm tra tạo thông báo và hàng chờ email; không gửi đính chính khi Staff sửa sang có mặt/vắng có phép.
- Dùng SQL Server provider để kiểm tra các truy vấn repository có thể tạo SQL; chặn mở kết nối và trả dữ liệu rỗng trong bộ kiểm tra.
- Kiểm tra EF theo dõi đúng liên kết kết quả → nhật ký → thông báo → email và giữ nguyên các ràng buộc database hiện có.
- Chạy pipeline MVC/Razor trong bộ nhớ để kiểm tra bốn view, biểu mẫu, chống giả mạo yêu cầu và quyền truy cập. Cơ chế xác thực giả chỉ tồn tại trong project kiểm tra này.

Các kiểm tra này không thay thế việc kiểm thử với dữ liệu SQL Server và máy chủ SMTP thực tế khi triển khai.

Kiểm tra JavaScript của biểu mẫu với DOM giả lập, dùng Node.js:

```powershell
node --test tests/HSMS.Attendance.Checks/attendance-ui.test.cjs
```

Kiểm tra lý do chỉ bắt buộc khi kết quả cũ thay đổi, không nhận lý do chỉ có khoảng trắng, tự bật yêu cầu lý do khi đơn nghỉ mới làm thay đổi kết quả cũ và khóa nút lưu lúc 00:00.
