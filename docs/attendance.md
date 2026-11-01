# Attendance — điểm danh học sinh

Triển khai theo `attendance-plan.txt` ngày 26/10/2026, trong ứng dụng `HSMS.MVC` hiện có.

## Tận dụng thành phần sẵn có

- Giữ ASP.NET Core MVC/Razor, EF Core 8, SQL Server và luồng Controller → Service → Repository.
- Mở rộng `AttendanceRecord`, `Notification`, `AppDbContext`; tái sử dụng học sinh/phụ huynh trong `Customer`, hồ sơ `Employee`, vai trò, lớp, lịch sử tham gia lớp, phân công, thời khóa biểu và đơn nghỉ. Dùng bảng `MessageLog` hiện có cho trạng thái gửi email và `IEmailService`/SMTP hiện có để gửi thư; không thêm bảng email hay migration cho thay đổi này.
- `AttendanceRepository` kế thừa `GenericRepository<AttendanceRecord>`; `CurrentUserService` dùng `IUserRepository` hiện có.
- Tiếp tục trả `ServiceResult`/`AppError`, dùng dashboard/layout, Bootstrap và jQuery sẵn có.
- Chỉ dùng Session `CurrentUser` như luồng đăng nhập gốc, không phát hành cookie xác thực. `SessionAuthenticationHandler` dựng `HttpContext.User` trong từng request để tiếp tục dùng `[Authorize]`/`IsInRole`; `CurrentUserService` đọc Session rồi kiểm tra trạng thái, vai trò và hồ sơ trong database. Cookie mã phiên ASP.NET Core và anti-forgery vẫn được dùng; TempData chuyển sang lưu trong Session.
- Chỉ thêm bảng nghiệp vụ `AttendanceAuditLog`. Không tạo bảng học sinh, lớp, đơn nghỉ, thông báo hoặc phiên điểm danh thay thế các bảng đã có.

## Chức năng

- `/Attendance`: bộ lọc ngày, khối, lớp, môn, giáo viên, tình trạng và từ khóa; nhóm tiết theo lớp, tiến độ ghi nhận, thống kê theo bộ lọc và phân trang 20 tiết. Truy vấn đếm/lọc/phân trang thực hiện trên SQL Server trước khi tải dữ liệu; Dashboard lấy tổng số tiết chưa đủ kết quả trên toàn phạm vi. Mở bảng rồi quay lại giữ nguyên bộ lọc và trang đang xem.
- `/Attendance/Mark?timetableEntryId=…&date=yyyy-MM-dd`: tự dựng danh sách theo thời khóa biểu và khoảng `JoinedAt <= ngày < LeftAt`; mở bảng không tạo kết quả.
- `POST /Attendance/Save`: lưu đầy đủ cả lớp; chỉnh sửa phải có lý do. Gửi lại nội dung không đổi không tạo thêm kết quả, nhật ký hay thông báo.
- `/Attendance/History`: lọc ngày, học sinh, lớp, môn, trạng thái; phân trang 30 kết quả; thống kê dùng cùng bộ lọc và phạm vi quyền.
- Học sinh và phụ huynh xem bố cục riêng tại `/Attendance/History`: chọn học kỳ, lớp/môn và bảng lịch từng tiết gồm ngày, giờ, phòng, giáo viên, lớp, trạng thái, ghi chú. Phụ huynh chọn từng con được liên kết; đổi con sẽ đặt lại học kỳ/môn. Bảng có cả tiết chưa ghi nhận và tiết sắp tới, phân trang 30 tiết; không quy các tiết này thành vắng.
- `/Attendance/Audit?id=…`: nhân viên có phạm vi phù hợp xem trước/sau của trạng thái, ghi chú, đơn nghỉ, người sửa, lý do và thời điểm.
- `/Notifications`, `POST /Notifications/Read`: hộp thông báo HTML cá nhân, 100 thông báo gần nhất và đánh dấu đã đọc. Chuông lấy số chưa đọc tại server khi mở/tải lại trang; không có polling, pop-up hay kết nối real-time. Đã gỡ endpoint `/Notifications/Feed`, Hub và thư viện SignalR.
- Menu, nút điểm danh, chuông thông báo và các chỉ số chuyên cần trong dashboard dùng dữ liệu thật.

## Quy tắc được bảo vệ tại server

- Chỉ bốn trạng thái `PRESENT`, `ABSENT`, `LATE`, `EXCUSED`; chưa ghi nhận không phải có mặt hay vắng.
- `EXCUSED` cần đơn `APPROVED` đúng học sinh, đúng ngày; đổi sang trạng thái khác sẽ bỏ liên kết đơn.
- Giáo viên bộ môn ghi tiết được phân công. Chủ nhiệm được xem lớp chủ nhiệm nhưng không tự có quyền sửa tiết của người khác.
- `STAFF` được ghi/sửa điểm danh toàn trường, kể cả ngày cũ, tương tự `ADMIN` có Employee profile. `PRINCIPAL` xem toàn trường. STAFF thiếu Employee profile được bổ sung hồ sơ tối thiểu khi thực sự lưu kết quả trong cùng transaction để nhật ký có người thực hiện hợp lệ. Không tự cấp quyền quản trị cho tài khoản mới.
- Học sinh xem bản thân; phụ huynh xem con liên kết bằng `Customer.ParentId`.
- Không ghi trước giờ bắt đầu tiết; kiểm tra thứ, hiệu lực, học kỳ và năm học. Giáo viên bị khóa lúc **00:00 ngày kế tiếp theo giờ Việt Nam**. STAFF và ADMIN có hồ sơ Employee được ghi/sửa sau mốc này; quyền được xác định tại server.
- Kiểm tra lại hạn sau `SaveChanges` và trước commit; giáo viên vượt hạn thì rollback cả lớp, nhật ký và thông báo. STAFF và ADMIN không bị khóa bởi thời hạn này, nhưng vẫn chịu các kiểm tra lịch học, danh sách, phiên bản và lý do sửa.
- Transaction SQL Server `Serializable`, token `Guid Version`, đối chiếu toàn bộ danh sách/phiên bản và unique index bảo vệ lưu đồng thời. Xung đột yêu cầu tải lại; không tự ghi đè.
- Kết quả cũ không bị xóa khi danh sách lớp thay đổi. Khoảng tham gia lớp bị chồng trong cùng năm học phải được đối chiếu.
- DbContext chặn đổi ý nghĩa lịch/phân công đã có điểm danh hoặc thu hẹp hiệu lực làm mất lịch sử; tạo lịch mới cho thay đổi tương lai.
- Các POST dùng anti-forgery. Không nhận danh tính nhân viên, người nhận thông báo hoặc quyền từ form.

## Thông báo

`AuthController` được khôi phục theo commit `e401754` trước Attendance: đăng nhập ghi Session, đăng xuất xóa Session; không có `SignInAsync`/`SignOutAsync` và không tự chuyển ADMIN sang Attendance. Không cung cấp tùy chọn ghi nhớ đăng nhập lâu dài. Session hết hạn hoặc bị xóa: trang được bảo vệ chuyển về Login, POST được bảo vệ trả 401; đăng nhập lại mới truy cập được. Thời gian Session idle vẫn là 2 giờ. Không có request thông báo tự động gia hạn phiên trong nền. Khi khởi động lại ứng dụng, kho Session trong bộ nhớ bị mất và người dùng phải đăng nhập lại.

Kết quả, audit, Notification và MessageLog email được ghi trong cùng transaction. Mỗi audit có tối đa một thông báo phụ huynh. Sau commit, dispatcher nền mới dùng `IEmailService.SendAttendanceEmailAsync` gửi SMTP; request điểm danh không chờ SMTP. Thông báo trong ứng dụng tồn tại độc lập, kể cả khi email lỗi.

MessageLog dùng kênh `EMAIL`, template `ATTENDANCE` và các trạng thái `QUEUED` → `SENDING` → `SENT`; lỗi chuyển `FAILED`, có `RetryCount` và hẹn lại bằng các cột gửi nền sẵn có của Notification. `SENT` nghĩa SMTP đã chấp nhận thư, không xác nhận phụ huynh đã nhận hoặc đọc. Hộp thông báo hiển thị trạng thái email riêng với trạng thái đã đọc. Khi ứng dụng khởi động lại, dispatcher tiếp tục quét hàng chờ; nếu dừng sau lúc SMTP nhận nhưng trước khi ghi trạng thái, có thể gửi lặp khi thử lại. Các thông báo cũ không có MessageLog email không bị gửi bù hàng loạt.

Thông báo và email được tạo khi lần đầu vắng không phép/đi muộn, chuyển sang hoặc giữa hai trạng thái này, và khi sửa cảnh báo sang có mặt/có phép. Chỉ sửa ghi chú hoặc lưu cùng nội dung không tạo thêm email. Phụ huynh mở lại hộp thông báo để xem các mục mới. Nếu tài khoản, vai trò hoặc quan hệ phụ huynh–con không còn hợp lệ, dispatcher không gửi dữ liệu cũ. Địa chỉ nhận lấy từ `User.Email` hiện tại của phụ huynh và được cập nhật vào MessageLog trước mỗi lần gửi.

Không có phụ huynh hoạt động: kết quả vẫn lưu và giao diện báo số cảnh báo chưa có người nhận. Phạm vi là thông báo trong ứng dụng và email, chưa có SMS/push thiết bị. Dispatcher hiện tại dành cho **một máy chủ**; nhiều máy chủ cần điều phối việc nhận và gửi hàng chờ.

### Cấu hình email

Tận dụng `EmailSettings` đang có: `SmtpHost`, `SmtpPort`, `SenderEmail`, `SenderPassword`, `SenderName`, `EnableSsl`. Luồng đặt lại mật khẩu tiếp tục sử dụng cùng dịch vụ gửi. Cấu hình SMTP sai/thiếu hoặc địa chỉ nhận không hợp lệ sẽ được ghi lỗi và thử lại cho Attendance, không bị coi là đã gửi thành công.

`EmailSettings:ApplicationUrl` là URL công khai để tạo liên kết tra cứu trong email; Development mặc định `http://localhost:5105`, khi triển khai đặt URL HTTPS thực tế (có thể dùng biến môi trường `EmailSettings__ApplicationUrl`). Nếu chưa cấu hình URL hợp lệ, email chỉ hướng dẫn đăng nhập, không tạo liên kết từ Host header. Tên, nội dung và đường dẫn được mã hóa HTML trước khi đưa vào thư.

Các tài khoản demo có email đuôi `.invalid` chỉ nhận thông báo trong ứng dụng; email được ghi lỗi/chờ thử lại cho đến khi hồ sơ có địa chỉ thực tế hợp lệ. Để thử gửi thật, cập nhật email đúng cho tài khoản phụ huynh và dùng SMTP đã cấu hình. Không tự gửi thư thử ra ngoài trong quá trình sửa mã.

## Cài đặt và chạy

Migration `20261026045557_AddAttendanceModule` đã được áp dụng lên `hsms-db` ngày 26/10/2026 theo sự cho phép của người dùng. Bản SQL tham khảo: [migrations/Attendance.sql](migrations/Attendance.sql).

Từ thư mục `HSMS`, với SQL Server và connection string hiện có:

```powershell
dotnet restore HSMS.MVC/HSMS.MVC.csproj
dotnet ef database update --project HSMS.MVC
dotnet run --project HSMS.MVC
```

Migration `20261026045557_AddAttendanceModule` bổ sung cột phiên bản/người sửa, audit, thông tin gửi Notification, index và check constraint. Không sửa migration đã áp dụng. Mã seed giữ nguyên `Role.Id = 4` là ADMIN và `Role.Id = 5` là STAFF để khớp lịch sử database; PRINCIPAL tiếp tục được tạo bởi DbSeeder hiện có.

Dữ liệu học vụ mẫu được tạo bằng `DbSeeder.SeedAttendanceDemoAsync`, dùng lại tài khoản `teacher`, `student`, `parent` hiện có và bổ sung các hồ sơ còn thiếu. Chỉ bật tự động trong môi trường Development khi `DemoData:AttendanceEnabled` là `true` trong `appsettings.Development.json`. Chạy lại không tạo trùng, không xóa kết quả điểm danh và không đặt lại mật khẩu các tài khoản `attendance.*` đã tạo. Đặt cờ này về `false` để ngừng bổ sung dữ liệu mẫu.

Thứ trong tuần: 1 = Thứ Hai, …, 7 = Chủ nhật. Phụ huynh phải có role PARENT đang hoạt động, Customer profile và được gắn qua ParentId của học sinh.

Ngày học lưu SQL `date`; các sự kiện do module tạo lưu UTC và hiển thị UTC+7. Tỷ lệ tham dự = `(PRESENT + LATE) / số kết quả đã ghi`; không có kết quả thì hiển thị chưa có dữ liệu. Đơn vị thống kê là lượt học sinh–tiết.

## Tài khoản và luồng thử trực tiếp

Mật khẩu ban đầu của các tài khoản mẫu: `Password@123` (tái sử dụng `DbSeeder.DefaultPassword`).

| Tài khoản | Vai trò | Dữ liệu liên quan |
|---|---|---|
| `teacher` | Giáo viên | Dạy lớp 10A1 — Điểm danh mẫu |
| `student` | Học sinh | Nguyễn Hoàng Long, con của `parent` |
| `attendance.student2` | Học sinh | Nguyễn Ngọc Linh, con của `parent` |
| `attendance.student3` | Học sinh | Trần Minh Khang, con của `attendance.parent2` |
| `attendance.student4` | Học sinh | Lê Gia Hân, con của `attendance.parent2` |
| `parent` | Phụ huynh | Xem Long và Linh |
| `attendance.parent2` | Phụ huynh | Xem Khang và Hân |
| `attendance.admin` | ADMIN | Có hồ sơ Employee, được sửa cả sau 00:00 ngày kế tiếp |
| `staff`, `staff.tech` | STAFF | Ghi/sửa điểm danh toàn trường, kể cả ngày cũ |
| `staff` | Học vụ | Ghi/sửa và tra cứu điểm danh toàn trường, kể cả ngày cũ |

1. Từ thư mục `HSMS`, chạy `dotnet run --project HSMS.MVC --launch-profile http`; mở `http://localhost:5105`.
2. Đăng nhập `teacher`, mở **Điểm danh**, chọn ngày hôm nay. Lớp mẫu có Toán và Ngữ văn, tổng cộng 4 tiết mỗi ngày. Tiết 1 bắt đầu 00:00 là tiết riêng để trải nghiệm bất kỳ giờ nào; các tiết còn lại bắt đầu 07:00, 09:00, 13:00.
3. Mở **Tiết 1**, chọn đủ kết quả cho 4 học sinh. Có thể chọn Long **Vắng không phép**, Linh **Vắng có phép** cùng đơn đã duyệt, Khang **Đi muộn**, Hân **Có mặt**. Không để trống dòng nào.
4. Trước khi lưu, mở trình duyệt khác hoặc cửa sổ ẩn danh, đăng nhập `parent`. `parent` nhận thông báo cho Long; `attendance.parent2` nhận thông báo cho Khang, không nhận dữ liệu của Long/Linh. Muốn nhận email thì hồ sơ phụ huynh cần địa chỉ thực tế hợp lệ.
5. Lưu từ cửa sổ giáo viên, sau đó mở **Tra cứu chuyên cần**, **Nhật ký** hoặc chuông thông báo. Phụ huynh tải lại trang/mở hộp thông báo để xem mục mới và trạng thái gửi email; không có cập nhật real-time.
6. Sửa Long thành **Có mặt**, nhập lý do và lưu để thử thông báo điều chỉnh. Dùng `attendance.admin` và chọn ngày hôm qua để thử quyền sửa sau mốc khóa; cùng ngày đó tài khoản `teacher` chỉ được xem.

Mỗi lần mở ứng dụng trong Development, seeder bổ sung đơn nghỉ đã duyệt cho Linh và đơn đang chờ cho Hân vào ngày hiện tại nếu chưa có. Đơn đang chờ không được gợi ý để áp dụng EXCUSED. Seeder không điền trước AttendanceRecord/Notification, nên bạn tự trải nghiệm toàn bộ luồng lưu. Kết quả do bạn lưu vẫn còn sau khi khởi động lại; chọn tiết khác hoặc ngày mới để thử lượt chưa điểm danh.

Trong `hsms-db` hiện tại đã có thêm một lượt mẫu **Tiết 1 ngày 25/10/2026**, được lưu rồi chỉnh sửa bằng `attendance.admin` qua luồng ứng dụng: 4 kết quả, một học sinh đi muộn, kèm nhật ký và thông báo cho phụ huynh tương ứng. Dùng ngày này để xem lịch sử hoặc thử quyền sửa ngày cũ; các tiết ngày 26/10/2026 vẫn chưa ghi nhận để bạn thử từ đầu.

Project `HSMS.MVC.Tests`, các file mã kiểm thử, kết quả kiểm thử và tham chiếu trong solution đã được xóa theo yêu cầu. Hướng dẫn này dành cho thao tác thử trực tiếp trên ứng dụng.

## Phần ngoài phạm vi

Giao diện tạo/duyệt đơn nghỉ, lịch nghỉ lễ/tiết hủy/dạy thay, xuất Excel/PDF, QR/GPS/nhận diện khuôn mặt và triển khai nhiều máy chủ vẫn cần yêu cầu riêng. Đơn nghỉ được gợi ý trên bảng, không tự sửa kết quả khi duyệt muộn hoặc sau hạn khóa.

Tham khảo kỹ thuật: [EF Core concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency), [transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions), [SMTP SendMailAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient.sendmailasync?view=net-8.0).
