# Attendance — rà soát và chỉnh sơ đồ swimlane

Đối chiếu với mã nguồn hiện tại ngày 26/10/2026. Sơ đồ gốc mô tả đúng luồng chính của giáo viên nhưng chưa phản ánh đầy đủ quyền, điều kiện lưu và thông báo.

## Bản đã chỉnh

- [Ảnh swimlane](attendance-swimlane.png)
- [Bản vector SVG](attendance-swimlane.svg)
- [Bản nguồn có thể chỉnh sửa](attendance-swimlane.drawio)

Phạm vi sơ đồ: **ghi nhận và chỉnh sửa điểm danh**, gồm người thao tác (TEACHER / STAFF / ADMIN), hệ thống và phụ huynh nhận thông báo. Đăng nhập là điều kiện trước; tra cứu của học sinh/phụ huynh là use case riêng. Không coi hình này là toàn bộ các use case của module.

## Những điểm cần sửa trong hình gốc

| Trong hình gốc | Theo triển khai hiện tại |
| --- | --- |
| Chỉ xác thực vai trò TEACHER | Người ghi/sửa còn có STAFF và ADMIN. Giáo viên sửa tiết được phân công; quyền chủ nhiệm chỉ cho xem các tiết khác. ADMIN phải có hồ sơ Employee; STAFF thiếu hồ sơ được bổ sung trong giao dịch khi thực sự lưu thay đổi. |
| Editing allowed? chưa có điều kiện | Kiểm tra quyền, lịch hiệu lực, danh sách hợp lệ, giờ bắt đầu. Chỉ giáo viên bị khóa sau 00:00 ngày kế tiếp theo giờ Việt Nam; STAFF/ADMIN không chịu hạn này. Không vai trò nào được ghi trước giờ bắt đầu. |
| Save results and audit; set status to Submitted | Không có AttendanceSession hay trạng thái phiên Submitted. Lưu từng AttendanceRecord; tình trạng đã điểm danh được tính từ mức độ đủ kết quả của học sinh. |
| Recheck conditions quá tổng quát | Server kiểm tra lại quyền, thời gian, lịch học, toàn bộ danh sách, bốn trạng thái, đơn nghỉ được duyệt, lý do sửa, roster token và phiên bản kết quả. Xung đột yêu cầu tải lại, không tự ghi đè. |
| Không thể hiện giao dịch/lỗi lưu | Kết quả thay đổi, AttendanceAuditLog và Notification phù hợp cùng một giao dịch. Kiểm tra lại hạn trước commit; thất bại rollback. |
| Dừng ngay khi không được sửa | Vẫn xem bảng chỉ đọc và lý do; có thể chọn tiết khác hoặc kết thúc. |
| Chỉ có thông báo lưu thành công cho người thao tác | Sau commit, dispatcher nền gửi thông báo điểm danh phù hợp tới phụ huynh liên kết. Thành công của lưu không phụ thuộc gửi thành công hay phụ huynh online. |

## Quy tắc tránh hiểu nhầm

- Kết quả học sinh chỉ có PRESENT, ABSENT, LATE, EXCUSED; chưa ghi nhận không mặc định là có mặt hoặc vắng.
- EXCUSED cần đơn APPROVED đúng học sinh và bao phủ ngày học. Có đơn nghỉ không tự tạo kết quả.
- Đổi kết quả/ghi chú/đơn nghỉ đã lưu phải có lý do. Không đổi nội dung: trả thành công với 0 thay đổi; không sinh audit hoặc thông báo mới.
- Mở bảng không tạo kết quả.
- Thông báo được tạo khi lần đầu ABSENT/LATE, chuyển vào/giữa hai trạng thái này, hoặc sửa cảnh báo sang PRESENT/EXCUSED. Chỉ sửa ghi chú không gửi lại cảnh báo.
- Thiếu phụ huynh hoạt động: vẫn lưu điểm danh, báo số cảnh báo chưa có người nhận.
- Dispatcher chỉ xử lý dữ liệu đã commit, kiểm tra lại người nhận, thử lại khi gửi lỗi. Phụ huynh offline có thể đọc thông báo đã lưu khi đăng nhập.
- PRINCIPAL xem toàn trường; học sinh chỉ xem bản thân, phụ huynh chỉ xem con liên kết. Những quyền tra cứu này không cấp quyền sửa.

## Mã nguồn dùng để đối chiếu

- [AttendanceAccess](../../HSMS/HSMS.MVC/Service/AttendanceAccess.cs): phạm vi và quyền ghi/sửa.
- [SchoolTime](../../HSMS/HSMS.MVC/Common/SchoolTime.cs): giờ bắt đầu và khóa giáo viên.
- [AttendanceService](../../HSMS/HSMS.MVC/Service/Implementations/AttendanceService.cs): dựng bảng, kiểm tra và lưu giao dịch.
- [AttendanceController](../../HSMS/HSMS.MVC/Controllers/AttendanceController.cs): lỗi, biểu mẫu và kết quả phản hồi.
- [NotificationDispatcher](../../HSMS/HSMS.MVC/Service/Notifications/NotificationDispatcher.cs): gửi sau commit.
- [Tài liệu module](../attendance.md): chức năng và quy tắc hiện có.

Bản chính là sơ đồ drawio có thể chỉnh sửa; SVG và PNG được xuất trực tiếp từ các nút và đường nối của bản nguồn này để giữ đúng nhánh điều kiện. Đã thử dựng ảnh tự động nhưng không dùng các ảnh thử do công cụ nối sai một số nhánh. Các tệp cuối không phụ thuộc vào những ảnh thử đó.

