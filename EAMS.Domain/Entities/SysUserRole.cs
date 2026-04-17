using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 用户角色关联
/// </summary>
[Table("sys_user_roles")]
public class SysUserRole : FullAuditedEntity<long>
{
    [Column("user_id")]
    public long UserId { get; set; }

    [Column("role_id")]
    public long RoleId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual SysUser? User { get; set; }

    [ForeignKey("RoleId")]
    public virtual SysRole? Role { get; set; }
}
