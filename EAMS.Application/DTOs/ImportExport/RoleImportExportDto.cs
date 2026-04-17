using MiniExcelLibs.Attributes;

namespace EAMS.Application.DTOs;

/// <summary>
/// 角色导出DTO
/// </summary>
public class RoleExportDto
{
    [ExcelColumnName("角色ID")]
    public long Id { get; set; }

    [ExcelColumnName("角色名称")]
    public string RoleName { get; set; } = string.Empty;

    [ExcelColumnName("角色代码")]
    public string RoleCode { get; set; } = string.Empty;

    [ExcelColumnName("描述")]
    public string? Description { get; set; }

    [ExcelColumnName("状态")]
    public string StatusText => Status == 1 ? "启用" : "禁用";

    [ExcelIgnore]
    public int Status { get; set; }

    [ExcelColumnName("排序")]
    public int SortOrder { get; set; }

    [ExcelColumnName("创建时间")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 角色导入DTO
/// </summary>
public class RoleImportDto
{
    [ExcelColumnName("角色名称")]
    public string RoleName { get; set; } = string.Empty;

    [ExcelColumnName("角色代码")]
    public string RoleCode { get; set; } = string.Empty;

    [ExcelColumnName("描述")]
    public string? Description { get; set; }

    [ExcelColumnName("状态")]
    public string? StatusText { get; set; }

    [ExcelColumnName("排序")]
    public int SortOrder { get; set; }
}
