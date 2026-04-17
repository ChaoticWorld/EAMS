namespace EAMS.Application.DTOs;

// ========== 角色 CRUD ==========
public class CreateRoleRequestDto
{
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
    public IEnumerable<long>? PermissionIds { get; set; }
}

public class UpdateRoleRequestDto
{
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? SortOrder { get; set; }
}

public class RoleQueryRequestDto : PagedRequestDto
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
}

// ========== 角色响应 ==========
public class RoleDto
{
    public long Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<PermissionDto>? Permissions { get; set; }
}
