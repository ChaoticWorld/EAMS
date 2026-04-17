namespace EAMS.Application.DTOs;

// ========== 部门 CRUD ==========
public class CreateDepartmentRequestDto
{
    public string DeptName { get; set; } = string.Empty;
    public string? DeptCode { get; set; }
    public long? ParentId { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
    public string? Leader { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class UpdateDepartmentRequestDto
{
    public string? DeptName { get; set; }
    public string? DeptCode { get; set; }
    public long? ParentId { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? SortOrder { get; set; }
    public string? Leader { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class DepartmentQueryRequestDto : PagedRequestDto
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
    public long? ParentId { get; set; }
}

// ========== 部门响应 ==========
public class DepartmentDto
{
    public long Id { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public string? DeptCode { get; set; }
    public long? ParentId { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public string? Leader { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ParentName { get; set; }
    public int EmployeeCount { get; set; }
    public IEnumerable<DepartmentDto>? Children { get; set; }
}

/// <summary>
/// 部门树形节点
/// </summary>
public class DepartmentTreeDto
{
    public long Id { get; set; }
    public string DeptName { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int SortOrder { get; set; }
    public IEnumerable<DepartmentTreeDto>? Children { get; set; }
}
