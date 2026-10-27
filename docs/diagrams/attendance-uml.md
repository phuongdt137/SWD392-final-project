# Attendance — class diagrams and sequence diagrams

Đối chiếu mã nguồn hiện tại ngày 26/10/2026. Nhãn sơ đồ dùng tiếng Anh để thống nhất với swimlane.

[Bản nguồn draw.io gồm 5 trang](attendance-module.drawio). Mỗi trang có bản PNG và SVG tương ứng; SVG giữ độ nét khi phóng lớn.

## Các sơ đồ

| Sơ đồ | Phạm vi | PNG | SVG |
| --- | --- | --- | --- |
| Domain class diagram | Các entity hiện có, thuộc tính/phương thức liên quan, quan hệ và bội số | [PNG](attendance-class-domain.png) | [SVG](attendance-class-domain.svg) |
| Design class diagram | Controller, Service, Repository, helper và thông báo nền | [PNG](attendance-class-design.png) | [SVG](attendance-class-design.svg) |
| Record / update sequence (simplified) | Bản đơn giản theo ảnh tham khảo: 5 thành phần, mở bảng + lưu, gộp trả dữ liệu trung gian | [PNG](attendance-sequence-save.png) | [SVG](attendance-sequence-save.svg) |
| View history sequence | Học sinh/phụ huynh và người dùng nhà trường tra cứu trong phạm vi quyền | [PNG](attendance-sequence-history.png) | [SVG](attendance-sequence-history.svg) |
| Deliver notification sequence | Đọc hàng đợi sau commit, kiểm tra người nhận, SignalR và retry | [PNG](attendance-sequence-notify.png) | [SVG](attendance-sequence-notify.svg) |

## Đọc class diagram

- Đây là các lớp có thật trong dự án, không phải đề xuất tạo lại module. Chỉ AttendanceAuditLog là entity mới của module; AttendanceRecord và Notification được mở rộng. Những lớp còn lại được tái sử dụng.
- Customer biểu diễn cả hồ sơ học sinh và phụ huynh; ParentId là quan hệ tự tham chiếu tới Customer. Không có lớp Student/Parent mới.
- User quản lý tài khoản và vai trò; Employee là hồ sơ giáo viên/nhân viên. UserId, CustomerId, EmployeeId là ba loại ID khác nhau.
- Mỗi AttendanceRecord thuộc một Customer và một TimetableEntry, có người ghi ban đầu, người sửa gần nhất tùy chọn và đơn nghỉ tùy chọn.
- Một AttendanceRecord có nhiều AttendanceAuditLog. Mỗi audit có tối đa một Notification. Notification dùng chung nên AttendanceAuditLogId có thể null; thông báo điểm danh được liên kết với audit.
- RecipientId của Notification trỏ tới User của phụ huynh, không trỏ tới Customer.
- Bội số 1: đúng một; 0..1: không có hoặc một; 0..*: không có hoặc nhiều. Đường association không bắt buộc có mũi tên: đây là quan hệ cấu trúc, không phải thứ tự chạy.
- Nét đứt đầu mũi tên mở: dependency; nét đứt đầu tam giác rỗng: implements; nét liền đầu tam giác rỗng: extends.
- Tên thuộc tính và phương thức lấy từ code. Lược bỏ constructor, một số overload, CancellationToken, kết quả Task/generic dài và các thành phần không liên quan để sơ đồ dễ đọc. Thuộc tính model chủ yếu public get/private set.
- Subject, Semester, Room, AcademicYear, GradeLevel, các lớp vai trò và một số quan hệ requester/reviewer/sender không nằm trong hình domain. Class diagram này không thay thế sơ đồ toàn bộ database.
- AttendanceRepository tái sử dụng GenericRepository<AttendanceRecord>; IAttendanceRepository kế thừa IGenericRepository<AttendanceRecord>. NotificationDispatcher kế thừa BackgroundService, NotificationHub kế thừa Hub.

## Đọc sequence diagram

- Bản ghi/sửa đã được đơn giản hóa theo ảnh tham khảo: 5 thành phần User → AttendanceController → AttendanceService → AttendanceRepository → Database. [Bản draw.io riêng](attendance-sequence-save.drawio). Gộp AttendancePage vào tương tác người dùng và bỏ leaveRepo riêng; đơn nghỉ được đọc bằng AttendanceRepository.GetLeavesAsync. Bỏ các phản hồi trung gian, helper và vòng lặp chi tiết. Số 1–5 là mở bảng; 6–18 mô tả ghi/sửa với hai kết quả thành công hoặc lỗi. Các nhãn thao tác gộp không phải API mới trong code.
- Mũi tên liền, đầu kín: lời gọi. Mũi tên nét đứt, đầu mở: trả kết quả. Mũi tên liền, đầu mở: sự kiện bất đồng bộ.
- Đường nét đứt dọc là lifeline, không cần mũi tên. Quy ước này là của sequence diagram, khác quy ước minh họa của swimlane trước đó.
- alt: chọn một nhánh theo điều kiện; opt: chỉ chạy khi điều kiện đúng; loop: lặp lại.
- Những nhãn gồm nhiều phương thức là nhóm lời gọi liên tiếp để rút gọn, không phải một API mới.
- Sơ đồ ghi/sửa rút gọn gồm cả mở bảng và lưu. GET Mark/SheetAsync chỉ đọc và dựng form, tạo RosterToken, đưa Version hiện tại vào từng hàng. IndexAsync cung cấp danh sách lịch có lọc/phân trang.
- SaveAsync bắt đầu giao dịch Serializable trước khi lấy người dùng hiện hành. Kiểm tra lại quyền, lịch hiệu lực, giờ, danh sách, đơn nghỉ, phiên bản và lý do sửa.
- Giáo viên sửa tiết được phân công và bị khóa từ 00:00 ngày kế tiếp theo giờ Việt Nam. STAFF sửa toàn trường; ADMIN cần Employee profile. STAFF/ADMIN được miễn hạn 00h nhưng vẫn không được điểm danh trước giờ bắt đầu.
- STAFF thiếu Employee profile được bổ sung trong cùng giao dịch, chỉ khi có thay đổi thực tế.
- Chỉ thay đổi thực tế mới tạo audit và thông báo thích hợp. Không thay đổi: trả Changed=0, không tạo thêm audit/notification và không đánh thức dispatcher.
- Các nhánh lỗi thể hiện kết quả nghiệp vụ: có thể phát sinh sớm trong từng bước; các bước bình thường còn lại bị bỏ qua. Giao dịch chưa commit được tự rollback khi DisposeAsync do await using, rồi DiscardChanges xóa trạng thái EF đang theo dõi.
- Hạn được kiểm tra trước FlushAsync và kiểm tra lại sau SaveChangesAsync, trước CommitAsync. Thất bại hoàn tác cả kết quả, audit và Notification.
- Sau commit mới Wake dispatcher. MVC trả redirect về Mark và nạp lại bảng; giao diện không chờ phụ huynh nhận thông báo.
- Chỉ cảnh báo ABSENT/LATE và cập nhật cảnh báo liên quan được tạo thông báo; chỉ sửa ghi chú không lặp thông báo. Thiếu phụ huynh hoạt động: vẫn lưu, tăng MissingParents và hiện cảnh báo.
- Dispatcher kiểm tra lại tài khoản/role/quan hệ phụ huynh trước khi gửi. Người nhận không còn hợp lệ: đánh dấu đã xử lý, không gửi nội dung cũ. SendAsync hoàn thành không có nghĩa phụ huynh đã đọc.
- Khi gửi lỗi, tăng số lần thử và lùi lịch gửi; thông báo vẫn nằm trong database. Trường hợp gửi lặp, client đối chiếu Notification.Id. Phụ huynh offline đọc feed sau khi đăng nhập.
- Tra cứu dùng phạm vi hiện hành tại server. Học sinh chỉ xem mình, phụ huynh chỉ xem con liên kết. Ngày học được dựng từ lịch + hiệu lực + khoảng tham gia lớp, sau đó hợp nhất lịch sử đã lưu và phân trang 30 tiết.
- Không có AttendanceSession, không ghi Submitted; chưa ghi nhận không mặc định là có mặt hoặc vắng.

## Mã nguồn đối chiếu

- [Entities](../../HSMS/HSMS.MVC/Models/AttendanceRecord.cs), [audit](../../HSMS/HSMS.MVC/Models/AttendanceAuditLog.cs), [notification](../../HSMS/HSMS.MVC/Models/Notification.cs).
- [Controller](../../HSMS/HSMS.MVC/Controllers/AttendanceController.cs), [service](../../HSMS/HSMS.MVC/Service/Implementations/AttendanceService.cs), [repository](../../HSMS/HSMS.MVC/Repositories/Implementations/AttendanceRepository.cs).
- [Quyền](../../HSMS/HSMS.MVC/Service/AttendanceAccess.cs), [thời gian](../../HSMS/HSMS.MVC/Common/SchoolTime.cs), [định danh](../../HSMS/HSMS.MVC/Service/Implementations/CurrentUserService.cs).
- [Dispatcher](../../HSMS/HSMS.MVC/Service/Notifications/NotificationDispatcher.cs), [notification repository](../../HSMS/HSMS.MVC/Repositories/Implementations/NotificationRepository.cs), [SignalR hub](../../HSMS/HSMS.MVC/Hubs/NotificationHub.cs).
- [Ràng buộc attendance/audit/notification](../../HSMS/HSMS.MVC/Data/AppDbContext.Attendance.cs), [cấu hình quan hệ dùng chung](../../HSMS/HSMS.MVC/Data/AppDbContext.cs).

Chỉ bổ sung tài liệu/sơ đồ; không sửa nghiệp vụ hay database, không tạo file kiểm thử.

