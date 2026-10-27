using HSMS.MVC.DTOs;

namespace HSMS.MVC.Service;

public interface ITimetableService
{
    AssignmentResponseDto CreateAssignmentAndTimetable(CreateAssignmentRequest request);

    FormOptionsDto GetFormOptions();
    WeeklyTimetableDto GetStaffTimetable(long? classId, long? employeeId, DateTime week);
    WeeklyTimetableDto GetTeacherTimetable(long employeeId, DateTime week);
    WeeklyTimetableDto GetStudentTimetable(long customerId, DateTime week);
}

