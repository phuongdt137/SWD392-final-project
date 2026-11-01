using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class GradeLevel
{
    public long Id { get; private set; }
    public string LevelCode { get; private set; } = null!;
    public string LevelName { get; private set; } = null!;

    public virtual ICollection<Class> Classes { get; private set; } = new List<Class>();

    protected GradeLevel() { }

    public GradeLevel(string levelCode, string levelName)
    {
        if (string.IsNullOrWhiteSpace(levelCode))
            throw new AppError("Mã khối lớp không được để trống.", "LEVEL_CODE_REQUIRED");

        if (string.IsNullOrWhiteSpace(levelName))
            throw new AppError("Tên khối lớp không được để trống.", "LEVEL_NAME_REQUIRED");

        LevelCode = levelCode.Trim();
        LevelName = levelName.Trim();
    }
}
