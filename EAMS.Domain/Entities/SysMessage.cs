using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 系统消息
/// </summary>
[Table("sys_messages")]
public class SysMessage : FullAuditedEntity<long>
{
    [Key]
    [Column("id")]
    public new long Id { get; set; }

    /// <summary>
    /// 接收者ID
    /// </summary>
    [Column("receiver_id")]
    public long ReceiverId { get; set; }

    /// <summary>
    /// 发送者ID
    /// </summary>
    [Column("sender_id")]
    public long? SenderId { get; set; }

    /// <summary>
    /// 消息标题
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息类型：system-系统通知，user-用户消息，task-任务通知
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("message_type")]
    public string MessageType { get; set; } = "system";

    /// <summary>
    /// 是否已读
    /// </summary>
    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// 阅读时间
    /// </summary>
    [Column("read_at")]
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// 相关业务ID
    /// </summary>
    [Column("business_id")]
    public long? BusinessId { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [MaxLength(50)]
    [Column("business_type")]
    public string? BusinessType { get; set; }

    // Navigation properties
    [ForeignKey("ReceiverId")]
    public virtual SysUser? Receiver { get; set; }

    [ForeignKey("SenderId")]
    public virtual SysUser? Sender { get; set; }
}
