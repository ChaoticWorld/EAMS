using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 员工
/// </summary>
[Table("sys_employees")]
public class SysEmployee : FullAuditedEntity<long>
{
    [Required]
    [MaxLength(50)]
    [Column("employee_no")]
    public string EmployeeNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("real_name")]
    public string RealName { get; set; } = string.Empty;

    [Column("gender")]
    public int Gender { get; set; } = 0; // 0-未知, 1-男, 2-女

    [MaxLength(20)]
    [Column("id_card")]
    public string? IdCard { get; set; }

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    [Column("dept_id")]
    public long? DeptId { get; set; }

    [Column("hire_date")]
    public DateTime? HireDate { get; set; }

    [Column("leave_date")]
    public DateTime? LeaveDate { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [MaxLength(100)]
    [Column("position")]
    public string? Position { get; set; }

    [MaxLength(200)]
    [Column("avatar")]
    public string? Avatar { get; set; }

    [MaxLength(50)]
    [Column("job_title")]
    public string? JobTitle { get; set; }

    [MaxLength(500)]
    [Column("remark")]
    public string? Remark { get; set; }

    // Navigation properties
    public virtual SysDepartment? Department { get; set; }
    public virtual SysUser? User { get; set; }
}
