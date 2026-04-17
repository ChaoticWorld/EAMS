using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 系统权限
/// </summary>
[Table("sys_permissions")]
public class SysPermission : FullAuditedEntity<long>
{
    [Required]
    [MaxLength(100)]
    [Column("permission_code")]
    public string PermissionCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("permission_name")]
    public string PermissionName { get; set; } = string.Empty;

    /// <summary>
    /// 权限类型：menu-菜单，button-按钮，api-接口
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("permission_type")]
    public string PermissionType { get; set; } = "button";

    [Column("parent_id")]
    public long? ParentId { get; set; }

    [MaxLength(500)]
    [Column("path")]
    public string? Path { get; set; }

    [MaxLength(200)]
    [Column("component")]
    public string? Component { get; set; }

    [MaxLength(100)]
    [Column("icon")]
    public string? Icon { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    // Navigation properties
    [ForeignKey("ParentId")]
    public virtual SysPermission? Parent { get; set; }

    public virtual ICollection<SysPermission> Children { get; set; } = new List<SysPermission>();
    public virtual ICollection<SysRolePermission> RolePermissions { get; set; } = new List<SysRolePermission>();
}
