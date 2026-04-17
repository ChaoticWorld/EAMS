namespace EAMS.Application.DTOs;

// ========== 权限 CRUD ==========
public class CreatePermissionRequestDto
{
    public string PermissionName { get; set; } = string.Empty;
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionType { get; set; } = "menu";
    public long? ParentId { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; } = 0;
    public int Status { get; set; } = 1;
}

public class UpdatePermissionRequestDto
{
    public string? PermissionName { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public int? SortOrder { get; set; }
    public int? Status { get; set; }
}

public class PermissionQueryRequestDto : PagedRequestDto
{
    public string? Keyword { get; set; }
    public string? PermissionType { get; set; }
    public int? Status { get; set; }
}

// ========== 权限响应 ==========
public class PermissionDto
{
    public long Id { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionType { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PermissionTreeDto : PermissionDto
{
    public List<PermissionTreeDto> Children { get; set; } = new();
}
