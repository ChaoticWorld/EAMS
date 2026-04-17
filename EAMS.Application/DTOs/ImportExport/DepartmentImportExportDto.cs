using MiniExcelLibs.Attributes;

namespace EAMS.Application.DTOs;

/// <summary>
/// 部门导出DTO
/// </summary>
public class DepartmentExportDto
{
    [ExcelColumnName("部门ID")]
    public long Id { get; set; }

    [ExcelColumnName("部门名称")]
    public string DepartmentName { get; set; } = string.Empty;

    [ExcelColumnName("部门编码")]
    public string DepartmentCode { get; set; } = string.Empty;

    [ExcelColumnName("上级部门ID")]
    public long? ParentId { get; set; }

    [ExcelColumnName("负责人")]
    public string? ManagerName { get; set; }

    [ExcelColumnName("联系电话")]
    public string? Phone { get; set; }

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
/// 部门导入DTO
/// </summary>
public class DepartmentImportDto
{
    [ExcelColumnName("部门名称")]
    public string DepartmentName { get; set; } = string.Empty;

    [ExcelColumnName("部门编码")]
    public string DepartmentCode { get; set; } = string.Empty;

    [ExcelColumnName("上级部门ID")]
    public long? ParentId { get; set; }

    [ExcelColumnName("负责人")]
    public string? ManagerName { get; set; }

    [ExcelColumnName("联系电话")]
    public string? Phone { get; set; }

    [ExcelColumnName("状态")]
    public string? StatusText { get; set; }

    [ExcelColumnName("排序")]
    public int SortOrder { get; set; }
}
