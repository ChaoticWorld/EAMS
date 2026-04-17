using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 角色权限关联
/// </summary>
[Table("sys_role_permissions")]
public class SysRolePermission : FullAuditedEntity<long>
{
    [Column("role_id")]
    public long RoleId { get; set; }

    [Column("permission_id")]
    public long PermissionId { get; set; }

    // Navigation properties
    [ForeignKey("RoleId")]
    public virtual SysRole? Role { get; set; }

    [ForeignKey("PermissionId")]
    public virtual SysPermission? Permission { get; set; }
}
