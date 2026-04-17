using MiniExcelLibs.Attributes;

namespace EAMS.Application.DTOs;

/// <summary>
/// 权限导出DTO
/// </summary>
public class PermissionExportDto
{
    [ExcelColumnName("权限ID")]
    public long Id { get; set; }

    [ExcelColumnName("权限名称")]
    public string PermissionName { get; set; } = string.Empty;

    [ExcelColumnName("权限代码")]
    public string PermissionCode { get; set; } = string.Empty;

    [ExcelColumnName("类型")]
    public string TypeText => Type switch
    {
        1 => "目录",
        2 => "菜单",
        3 => "按钮",
        _ => "未知"
    };

    [ExcelIgnore]
    public int Type { get; set; }

    [ExcelColumnName("上级ID")]
    public long? ParentId { get; set; }

    [ExcelColumnName("路径")]
    public string? Path { get; set; }

    [ExcelColumnName("图标")]
    public string? Icon { get; set; }

    [ExcelColumnName("排序")]
    public int SortOrder { get; set; }

    [ExcelColumnName("状态")]
    public string StatusText => Status == 1 ? "启用" : "禁用";

    [ExcelIgnore]
    public int Status { get; set; }

    [ExcelColumnName("创建时间")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// 权限导入DTO
/// </summary>
public class PermissionImportDto
{
    [ExcelColumnName("权限名称")]
    public string PermissionName { get; set; } = string.Empty;

    [ExcelColumnName("权限代码")]
    public string PermissionCode { get; set; } = string.Empty;

    [ExcelColumnName("类型")]
    public string? TypeText { get; set; }

    [ExcelColumnName("上级ID")]
    public long? ParentId { get; set; }

    [ExcelColumnName("路径")]
    public string? Path { get; set; }

    [ExcelColumnName("图标")]
    public string? Icon { get; set; }

    [ExcelColumnName("排序")]
    public int SortOrder { get; set; }

    [ExcelColumnName("状态")]
    public string? StatusText { get; set; }
}
