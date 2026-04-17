using MiniExcelLibs.Attributes;

namespace EAMS.Application.DTOs;

/// <summary>
/// 用户导出DTO
/// </summary>
public class UserExportDto
{
    [ExcelColumnName("用户ID")]
    public long Id { get; set; }

    [ExcelColumnName("用户名")]
    public string Username { get; set; } = string.Empty;

    [ExcelColumnName("真实姓名")]
    public string RealName { get; set; } = string.Empty;

    [ExcelColumnName("邮箱")]
    public string? Email { get; set; }

    [ExcelColumnName("手机号")]
    public string? Phone { get; set; }

    [ExcelColumnName("状态")]
    public string StatusText => Status == 1 ? "启用" : "禁用";

    [ExcelIgnore]
    public int Status { get; set; }

    [ExcelColumnName("创建时间")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 用户导入DTO
/// </summary>
public class UserImportDto
{
    [ExcelColumnName("用户名")]
    public string Username { get; set; } = string.Empty;

    [ExcelColumnName("真实姓名")]
    public string RealName { get; set; } = string.Empty;

    [ExcelColumnName("邮箱")]
    public string? Email { get; set; }

    [ExcelColumnName("手机号")]
    public string? Phone { get; set; }

    [ExcelColumnName("状态")]
    public string? StatusText { get; set; }
}
