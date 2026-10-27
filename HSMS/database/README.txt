CƠ SỞ DỮ LIỆU HSMS — EMPLOYEE / CUSTOMER

Database sử dụng: hsms-db, SQL Server.
Tài khoản lưu trực tiếp ở Employee và Customer.
Phân quyền: EmployeeRole, CustomerRole và Role. Không có bảng User/UserRole.

LỊCH SỬ MIGRATION
Giữ nguyên 4 migration đã áp dụng, từ InitDb đến AddStudentTranscriptAndTransferHistory.
Migration mới: 20261027033754_AddEmployeeCustomerAccountSupport.
Chiều Up chỉ bổ sung LastLoginAt/PasswordResetToken/PasswordResetTokenExpiry vào
Employee và Customer, các cột/index/ràng buộc điểm danh và bảng AttendanceAuditLog.
Không xóa cột dữ liệu cũ, không thay tài khoản bằng bảng User.
Các migration User/Auth và Attendance cũ chưa áp dụng đã được thay bằng migration mới.

CẬP NHẬT DATABASE TỪ THƯ MỤC HSMS
1. Dừng phiên Debug đang chạy trong Visual Studio.
2. Chạy:
   dotnet ef database update --project HSMS.MVC
   Hoặc Package Manager Console (Default project: HSMS.MVC): Update-Database
3. F5 lại và đăng nhập lại để nhận cookie mới có AccountType.

DATABASE MỚI
Tạo database theo connection string rồi chạy lệnh EF bên trên, hoặc chạy toàn bộ
hsms-after-migrations.sql trong SSMS. Script idempotent kiểm tra __EFMigrationsHistory.
Không chạy lại file SQL cũ có CREATE TABLE [User].

ĐÃ XÁC MINH TRÊN DATABASE LOCAL
Migration mới đã áp dụng thành công, database có 5 migration, không có bảng User.
Ứng dụng khởi động, seed Employee/Customer và đăng nhập được staff/teacher/student.
File SQL chứa schema + dữ liệu HasData; tài khoản mẫu được tạo bởi DbSeeder khi chạy app.

TÀI KHOẢN DEMO ĐƯỢC SEED
staff / teacher / student / parent / principal — mật khẩu mẫu Password@123.
Seeder giữ nguyên mật khẩu của tài khoản đã tồn tại.

PHẠM VI TƯƠNG THÍCH
Bộ migration này dành cho database Employee/Customer như hsms-db đã kiểm tra.
Database nơi migration tạo User đã được áp dụng cần kế hoạch chuyển dữ liệu riêng;
không tự đổi bảng lịch sử migration trên database đó.
