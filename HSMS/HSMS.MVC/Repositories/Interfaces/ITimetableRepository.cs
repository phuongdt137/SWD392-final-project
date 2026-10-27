using HSMS.MVC.DTOs;
using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace HSMS.MVC.Repositories.Interfaces;

public interface ITimetableRepository
{
    // ---- Luồng tạo lịch (khớp diagram) ----
    List<string> Validate(CreateAssignmentRequest request);              // bước 4-6
    TimetableEntry? FindScheduleConflict(CreateAssignmentRequest request); // bước 7-10
    long SaveAssignment(TeachingAssignment assignment);                  // bước 11-14
    long SaveTimetableEntry(TimetableEntry entry);                       // bước 15-18
    IDbContextTransaction BeginTransaction();

    // ---- Luồng xem lịch ----
    List<TimetableEntry> GetEntries(long? employeeId, long? classId, DateTime monday, long? customerId);

    string? GetEmployeeName(long employeeId);
    string? GetStudentName(long customerId);
    string? GetClassName(long classId);

    // ---- Dữ liệu cho dropdown ----
    List<Employee> GetEmployees();
    List<Subject> GetSubjects();
    List<Class> GetClasses();
    List<Semester> GetSemesters();
    List<Room> GetRooms();
    List<TimeSlot> GetTimeSlots();
}


