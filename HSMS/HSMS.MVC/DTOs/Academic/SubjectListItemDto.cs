namespace HSMS.MVC.DTOs.Academic
{
        public class SubjectListItemDto
        {
            public long Id { get; set; }
            public string SubjectCode { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public string? EvaluationType { get; set; }
            public int AssignmentCount { get; set; }
        }
}
