using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 部门
/// </summary>
[Table("sys_departments")]
public class SysDepartment : FullAuditedEntity<long>
{
    [Required]
    [MaxLength(100)]
    [Column("dept_name")]
    public string DeptName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("dept_code")]
    public string? DeptCode { get; set; }

    [Column("parent_id")]
    public long? ParentId { get; set; }

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [MaxLength(100)]
    [Column("leader")]
    public string? Leader { get; set; }

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [Column("email")]
    public string? Email { get; set; }

    // Navigation properties
    public virtual SysDepartment? Parent { get; set; }
    public virtual ICollection<SysDepartment> Children { get; set; } = new List<SysDepartment>();
    public virtual ICollection<SysEmployee> Employees { get; set; } = new List<SysEmployee>();
}
