using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 系统用户
/// </summary>
[Table("sys_users")]
public class SysUser : FullAuditedEntity<long>
{
    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("real_name")]
    public string RealName { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [Column("avatar")]
    public string? Avatar { get; set; }

    [Column("gender")]
    public int Gender { get; set; } = 0; // 0-未知, 1-�? 2-�?
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [MaxLength(50)]
    [Column("login_ip")]
    public string? LoginIp { get; set; }

    [Column("login_at")]
    public DateTime? LoginAt { get; set; }

    [MaxLength(500)]
    [Column("remark")]
    public string? Remark { get; set; }

    // Navigation properties
    public virtual ICollection<SysUserRole> UserRoles { get; set; } = new List<SysUserRole>();
    public virtual ICollection<SysMessage> ReceivedMessages { get; set; } = new List<SysMessage>();
    public virtual ICollection<SysMessage> SentMessages { get; set; } = new List<SysMessage>();
    public virtual ICollection<SysOperationLog> OperationLogs { get; set; } = new List<SysOperationLog>();
}
