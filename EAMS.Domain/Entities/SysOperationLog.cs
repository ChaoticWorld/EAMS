using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 操作日志
/// </summary>
[Table("sys_operation_logs")]
public class SysOperationLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// 操作用户ID
    /// </summary>
    [Column("user_id")]
    public long? UserId { get; set; }

    /// <summary>
    /// 操作用户�?    /// </summary>
    [MaxLength(100)]
    [Column("username")]
    public string? Username { get; set; }

    /// <summary>
    /// 操作模块
    /// </summary>
    [MaxLength(100)]
    [Column("module")]
    public string? Module { get; set; }

    /// <summary>
    /// 操作类型：create-创建，update-更新，delete-删除，query-查询，export-导出，import-导入，login-登录，logout-登出
    /// </summary>
    [Required]
    [MaxLength(20)]
    [Column("operation_type")]
    public string OperationType { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法
    /// </summary>
    [MaxLength(10)]
    [Column("method")]
    public string? Method { get; set; }

    /// <summary>
    /// 请求URL
    /// </summary>
    [MaxLength(500)]
    [Column("request_url")]
    public string? RequestUrl { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    [Column("request_params")]
    public string? RequestParams { get; set; }

    /// <summary>
    /// 响应结果
    /// </summary>
    [Column("response_result")]
    public string? ResponseResult { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [MaxLength(50)]
    [Column("ip_address")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 操作地点
    /// </summary>
    [MaxLength(200)]
    [Column("location")]
    public string? Location { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    [MaxLength(500)]
    [Column("user_agent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 执行时长（毫秒）
    /// </summary>
    [Column("execution_time")]
    public long? ExecutionTime { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [Column("is_success")]
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// 错误信息
    /// </summary>
    [Column("error_message")]
    public string? ErrorMessage { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual SysUser? User { get; set; }
}
