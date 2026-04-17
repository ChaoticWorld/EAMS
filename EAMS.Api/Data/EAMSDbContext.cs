using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EAMS.Domain.Entities;

namespace EAMS.Api.Data;

public class EAMSDbContext : DbContext
{
    public EAMSDbContext(DbContextOptions<EAMSDbContext> options) : base(options)
    {
    }

    public DbSet<SysUser> SysUsers { get; set; } = null!;
    public DbSet<SysRole> SysRoles { get; set; } = null!;
    public DbSet<SysPermission> SysPermissions { get; set; } = null!;
    public DbSet<SysUserRole> SysUserRoles { get; set; } = null!;
    public DbSet<SysRolePermission> SysRolePermissions { get; set; } = null!;
    public DbSet<SysMessage> SysMessages { get; set; } = null!;
    public DbSet<SysOperationLog> SysOperationLogs { get; set; } = null!;
    public DbSet<SysDataDictionary> SysDataDictionaries { get; set; } = null!;
    public DbSet<SysDataDictionaryItem> SysDataDictionaryItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply snake_case naming convention to all entities
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table name to snake_case
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));
            
            // Convert column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }

        // SysUser
        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.IsDeleted);
        });

        // SysRole
        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasIndex(e => e.RoleCode).IsUnique();
            entity.HasIndex(e => e.IsDeleted);
        });

        // SysPermission
        modelBuilder.Entity<SysPermission>(entity =>
        {
            entity.HasIndex(e => e.PermissionCode).IsUnique();
            entity.HasIndex(e => e.ParentId);
            entity.HasOne(p => p.Parent)
                  .WithMany(p => p.Children)
                  .HasForeignKey(p => p.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SysUserRole - 复合唯一索引
        modelBuilder.Entity<SysUserRole>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        });

        // SysRolePermission - 复合唯一索引
        modelBuilder.Entity<SysRolePermission>(entity =>
        {
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
        });

        // SysMessage
        modelBuilder.Entity<SysMessage>(entity =>
        {
            entity.HasIndex(e => e.ReceiverId);
            entity.HasIndex(e => e.IsRead);
            
            // 明确配置 Receiver 关系
            entity.HasOne(m => m.Receiver)
                  .WithMany(u => u.ReceivedMessages)
                  .HasForeignKey(m => m.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // 明确配置 Sender 关系
            entity.HasOne(m => m.Sender)
                  .WithMany(u => u.SentMessages)
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SysOperationLog
        modelBuilder.Entity<SysOperationLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.OperationType);
        });

        // SysDataDictionary
        modelBuilder.Entity<SysDataDictionary>(entity =>
        {
            entity.HasIndex(e => e.DictCode).IsUnique();
        });

        // SysDataDictionaryItem
        modelBuilder.Entity<SysDataDictionaryItem>(entity =>
        {
            entity.HasIndex(e => new { e.DictId, e.ItemValue }).IsUnique();
        });
    }
    
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return string.Concat(input.Select((c, i) => 
            i > 0 && char.IsUpper(c) ? "_" + char.ToLower(c) : char.ToLower(c).ToString()));
    }
}
