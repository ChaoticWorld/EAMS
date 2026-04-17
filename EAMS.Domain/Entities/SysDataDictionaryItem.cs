using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EAMS.Domain.Abstractions;

namespace EAMS.Domain.Entities;

/// <summary>
/// 字典�?/// </summary>
[Table("sys_data_dictionary_items")]
public class SysDataDictionaryItem : FullAuditedEntity<long>
{
    /// <summary>
    /// 字典ID
    /// </summary>
    [Column("dict_id")]
    public long DictId { get; set; }

    /// <summary>
    /// 字典项�?    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("item_value")]
    public string ItemValue { get; set; } = string.Empty;

    /// <summary>
    /// 字典项文�?    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("item_text")]
    public string ItemText { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// 是否默认
    /// </summary>
    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 扩展属性（JSON�?    /// </summary>
    [Column("extra_properties")]
    public string? ExtraProperties { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    // Navigation properties
    [ForeignKey("DictId")]
    public virtual SysDataDictionary? Dictionary { get; set; }
}
