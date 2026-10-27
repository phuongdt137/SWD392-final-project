using System.Reflection;
using HSMS.MVC.DTOs;
using HSMS.MVC.Models;
using HSMS.MVC.Service;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS: " + name); }
var repo = DispatchProxy.Create<ITimetableRepository, Stub>();
var stub = (Stub)(object)repo;
var service = new TimetableService(repo);
var request = new CreateAssignmentRequest { EmployeeId=1, SubjectId=2, ClassId=3, SemesterId=4, RoomId=5, TimeSlotId=6, DayOfWeek=1 };
var result = service.CreateAssignmentAndTimetable(request);
Check(result.Success && result.AssignmentId == 10 && result.TimetableId == 20, "Successful creation returns both IDs");
Check(string.Join(",", stub.Calls) == "BeginTransaction,Validate,FindScheduleConflict,SaveAssignment,SaveTimetableEntry,Commit,Dispose", "Sequence and commit");
stub.Calls.Clear(); stub.Invalid = true;
Check(!service.CreateAssignmentAndTimetable(request).Success && !stub.Calls.Contains("SaveAssignment"), "Invalid request does not write");
stub.Invalid = false; stub.Calls.Clear(); stub.FailSave = true;
try { service.CreateAssignmentAndTimetable(request); throw new Exception("Expected save failure"); }
catch (InvalidOperationException) { Check(!stub.Calls.Contains("Commit") && stub.Calls.Contains("Dispose"), "Failed second save disposes uncommitted transaction"); }
stub.FailSave = false;
var conflict = new TimetableEntry(10, 5, 6, 1);
var assignment = new TeachingAssignment(1, 2, 3, 4);
typeof(TimetableEntry).GetProperty("TeachingAssignment")!.SetValue(conflict, assignment);
stub.Conflict = conflict; stub.Calls.Clear();
var clash = service.CreateAssignmentAndTimetable(request);
Check(clash.IsConflict && !clash.Success && !stub.Calls.Contains("SaveAssignment") && !stub.Calls.Contains("Commit"), "Conflict does not write or commit");
stub.Conflict = null;
var week = service.GetTeacherTimetable(42, new DateTime(2026,10,28));
Check(week.WeekStart == new DateTime(2026,10,26) && (long?)stub.ReadArgs![0] == 42, "Teacher scope and Monday normalization");
service.GetStudentTimetable(99, new DateTime(2026,11,1));
Check((long?)stub.ReadArgs![3] == 99 && (DateTime)stub.ReadArgs[2]! == new DateTime(2026,10,26), "Student scope and Sunday normalization");
var method = typeof(HSMS.MVC.Repositories.Implementations.TimetableRepository).GetMethod("HasLessonDate", BindingFlags.NonPublic | BindingFlags.Static)!;
Check(!(bool)method.Invoke(null, new object[]{new DateTime(2026,10,27),new DateTime(2026,10,28),1})!, "Date overlap without requested weekday");
Check((bool)method.Invoke(null, new object[]{new DateTime(2026,10,26),new DateTime(2026,10,26),1})!, "Single-day schedule includes boundary");

await AccountChecks.RunAsync();

public class Stub : DispatchProxy
{
    public List<string> Calls = new();
    public bool Invalid, FailSave;
    public TimetableEntry? Conflict;
    public object?[]? ReadArgs;
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        Calls.Add(method!.Name);
        switch(method.Name)
        {
            case "BeginTransaction": var tx=DispatchProxy.Create<IDbContextTransaction, TransactionStub>(); ((TransactionStub)(object)tx).Calls=Calls; return tx;
            case "Validate": return Invalid ? new List<string>{"Invalid"} : new List<string>();
            case "FindScheduleConflict": return Conflict;
            case "SaveAssignment": return 10L;
            case "SaveTimetableEntry": if(FailSave) throw new InvalidOperationException(); return 20L;
            case "GetEntries": ReadArgs=args; return new List<TimetableEntry>();
            case "GetTimeSlots": return new List<TimeSlot>();
            case "GetEmployeeName": case "GetStudentName": return "Demo";
            default: throw new NotSupportedException(method.Name);
        }
    }
}
public class TransactionStub : DispatchProxy
{
    public List<string> Calls = new();
    protected override object? Invoke(MethodInfo? method, object?[]? args) { Calls.Add(method!.Name); return null; }
}


