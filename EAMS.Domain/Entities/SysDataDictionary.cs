using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 数据字典
/// </summary>
[Table("sys_data_dictionaries")]
public class SysDataDictionary : FullAuditedEntity<long>
{
    /// <summary>
    /// 字典编码
    /// </summary>
    [Required]
    [MaxLength(50)]
    [Column("dict_code")]
    public string DictCode { get; set; } = string.Empty;

    /// <summary>
    /// 字典名称
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("dict_name")]
    public string DictName { get; set; } = string.Empty;

    /// <summary>
    /// 字典描述
    /// </summary>
    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 是否系统内置（系统内置不可删除）
    /// </summary>
    [Column("is_system")]
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// 是否启用
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    // Navigation properties
    public virtual ICollection<SysDataDictionaryItem> Items { get; set; } = new List<SysDataDictionaryItem>();
}
