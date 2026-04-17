namespace EAMS.Domain.Abstractions;

/// <summary>
/// 软删除实体
/// </summary>
public abstract class SoftDeleteEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// 创建审计 - 泛型版本
/// </summary>
public abstract class CreationAuditedEntity<TKey> : SoftDeleteEntity
{
    public TKey Id { get; set; } = default!;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 创建和修改审计 - 泛型版本
/// </summary>
public abstract class ModificationAuditedEntity<TKey> : CreationAuditedEntity<TKey>
{
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// 完全审计（包含删除用户）- 泛型版本
/// </summary>
public abstract class FullAuditedEntity<TKey> : ModificationAuditedEntity<TKey>
{
}
