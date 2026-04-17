using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 系统角色
/// </summary>
[Table("sys_roles")]
public class SysRole : FullAuditedEntity<long>
{
    [Required]
    [MaxLength(50)]
    [Column("role_code")]
    public string RoleCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("role_name")]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    // Navigation properties
    public virtual ICollection<SysUserRole> UserRoles { get; set; } = new List<SysUserRole>();
    public virtual ICollection<SysRolePermission> RolePermissions { get; set; } = new List<SysRolePermission>();
}
