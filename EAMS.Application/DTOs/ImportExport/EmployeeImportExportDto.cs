using MiniExcelLibs.Attributes;

namespace EAMS.Application.DTOs;

/// <summary>
/// 员工导出DTO
/// </summary>
public class EmployeeExportDto
{
    [ExcelColumnName("员工ID")]
    public long Id { get; set; }

    [ExcelColumnName("工号")]
    public string EmployeeNo { get; set; } = string.Empty;

    [ExcelColumnName("姓名")]
    public string Name { get; set; } = string.Empty;

    [ExcelColumnName("性别")]
    public string GenderText => Gender switch
    {
        1 => "男",
        2 => "女",
        _ => "未知"
    };

    [ExcelIgnore]
    public int Gender { get; set; }

    [ExcelColumnName("部门ID")]
    public long? DepartmentId { get; set; }

    [ExcelColumnName("部门名称")]
    public string? DepartmentName { get; set; }

    [ExcelColumnName("职位")]
    public string? Position { get; set; }

    [ExcelColumnName("手机号")]
    public string? Phone { get; set; }

    [ExcelColumnName("邮箱")]
    public string? Email { get; set; }

    [ExcelColumnName("入职日期")]
    public DateTime? HireDate { get; set; }

    [ExcelColumnName("状态")]
    public string StatusText => Status switch
    {
        1 => "在职",
        2 => "离职",
        3 => "休假",
        _ => "未知"
    };

    [ExcelIgnore]
    public int Status { get; set; }

    [ExcelColumnName("创建时间")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 员工导入DTO
/// </summary>
public class EmployeeImportDto
{
    [ExcelColumnName("工号")]
    public string EmployeeNo { get; set; } = string.Empty;

    [ExcelColumnName("姓名")]
    public string Name { get; set; } = string.Empty;

    [ExcelColumnName("性别")]
    public string? GenderText { get; set; }

    [ExcelColumnName("部门ID")]
    public long? DepartmentId { get; set; }

    [ExcelColumnName("职位")]
    public string? Position { get; set; }

    [ExcelColumnName("手机号")]
    public string? Phone { get; set; }

    [ExcelColumnName("邮箱")]
    public string? Email { get; set; }

    [ExcelColumnName("入职日期")]
    public DateTime? HireDate { get; set; }

    [ExcelColumnName("状态")]
    public string? StatusText { get; set; }
}
