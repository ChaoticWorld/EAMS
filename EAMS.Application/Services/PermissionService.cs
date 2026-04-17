using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using EAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EAMS.Application.Services;

/// <summary>
/// 权限服务实现
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IRepository<SysPermission, long> _permissionRepository;
    private readonly IRepository<SysRolePermission, long> _rolePermissionRepository;
    private readonly IRepository<SysUserRole, long> _userRoleRepository;

    public PermissionService(
        IRepository<SysPermission, long> permissionRepository,
        IRepository<SysRolePermission, long> rolePermissionRepository,
        IRepository<SysUserRole, long> userRoleRepository)
    {
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _userRoleRepository = userRoleRepository;
    }

    public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequestDto request)
    {
        // 检查权限代码是否已存在
        if (await _permissionRepository.ExistsAsync(p => p.PermissionCode == request.PermissionCode))
        {
            throw new InvalidOperationException($"权限代码 '{request.PermissionCode}' 已存在");
        }

        var permission = new SysPermission
        {
            PermissionName = request.PermissionName,
            PermissionCode = request.PermissionCode,
            PermissionType = request.PermissionType,
            ParentId = request.ParentId,
            Path = request.Path,
            Component = request.Component,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            IsEnabled = request.Status == 1
        };

        await _permissionRepository.AddAsync(permission);
        return MapToDto(permission);
    }

    public async Task<PermissionDto> UpdatePermissionAsync(long id, UpdatePermissionRequestDto request)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
            throw new InvalidOperationException("权限不存在");

        permission.PermissionName = request.PermissionName ?? permission.PermissionName;
        permission.Path = request.Path ?? permission.Path;
        permission.Component = request.Component ?? permission.Component;
        permission.Icon = request.Icon ?? permission.Icon;
        if (request.SortOrder.HasValue)
            permission.SortOrder = request.SortOrder.Value;
        if (request.Status.HasValue)
            permission.IsEnabled = request.Status.Value == 1;

        await _permissionRepository.UpdateAsync(permission);
        return MapToDto(permission);
    }

    public async Task DeletePermissionAsync(long id, string deletedBy)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission == null)
            throw new InvalidOperationException("权限不存在");

        // 检查是否有子权限
        if (await _permissionRepository.ExistsAsync(p => p.ParentId == id))
        {
            throw new InvalidOperationException("该权限下存在子权限，无法删除");
        }

        // 删除角色权限关联
        var rolePermissions = await _rolePermissionRepository.FindListAsync(rp => rp.PermissionId == id);
        foreach (var rp in rolePermissions)
        {
            await _rolePermissionRepository.DeleteAsync(rp);
        }

        await _permissionRepository.DeleteAsync(id);
    }

    public async Task<PermissionDto?> GetPermissionByIdAsync(long id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        return permission == null ? null : MapToDto(permission);
    }

    public async Task<IEnumerable<PermissionTreeDto>> GetPermissionTreeAsync()
    {
        var permissions = await _permissionRepository.GetQueryable()
            .Where(p => !p.IsDeleted && p.IsEnabled)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        // 构建树形结构
        var rootPermissions = permissions.Where(p => p.ParentId == null).ToList();
        return BuildPermissionTree(rootPermissions, permissions);
    }

    private List<PermissionTreeDto> BuildPermissionTree(
        IEnumerable<SysPermission> currentLevel,
        List<SysPermission> allPermissions)
    {
        var result = new List<PermissionTreeDto>();
        foreach (var permission in currentLevel)
        {
            var dto = new PermissionTreeDto
            {
                Id = permission.Id,
                PermissionName = permission.PermissionName,
                PermissionCode = permission.PermissionCode,
                PermissionType = permission.PermissionType,
                ParentId = permission.ParentId,
                Path = permission.Path,
                Component = permission.Component,
                Icon = permission.Icon,
                SortOrder = permission.SortOrder,
                Status = permission.IsEnabled ? 1 : 0,
                CreatedAt = permission.CreatedAt,
                Children = BuildPermissionTree(
                    allPermissions.Where(p => p.ParentId == permission.Id),
                    allPermissions)
            };
            result.Add(dto);
        }
        return result;
    }

    public async Task<PagedResult<PermissionDto>> GetPermissionsAsync(PermissionQueryRequestDto query)
    {
        var q = _permissionRepository.GetQueryable()
            .Where(p => !p.IsDeleted);

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(p => p.PermissionName.ToLower().Contains(keyword) ||
                            p.PermissionCode.ToLower().Contains(keyword));
        }

        // 类型筛选
        if (!string.IsNullOrWhiteSpace(query.PermissionType))
        {
            q = q.Where(p => p.PermissionType == query.PermissionType);
        }

        // 状态筛选
        if (query.Status.HasValue)
        {
            q = q.Where(p => p.IsEnabled == (query.Status.Value == 1));
        }

        // 排序
        q = q.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<PermissionDto>
        {
            Items = items.Select(MapToDto),
            Total = total,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId)
    {
        // 获取用户角色
        var roleIds = await _userRoleRepository.GetQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        // 获取角色权限
        var permissionIds = await _rolePermissionRepository.GetQueryable()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync();

        // 获取权限代码
        var permissions = await _permissionRepository.GetQueryable()
            .Where(p => permissionIds.Contains(p.Id) && p.IsEnabled && !p.IsDeleted)
            .Select(p => p.PermissionCode)
            .ToListAsync();

        return permissions;
    }

    public async Task<bool> HasPermissionAsync(long userId, string permissionCode)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permissionCode);
    }

    private static PermissionDto MapToDto(SysPermission permission) => new()
    {
        Id = permission.Id,
        PermissionName = permission.PermissionName,
        PermissionCode = permission.PermissionCode,
        PermissionType = permission.PermissionType,
        ParentId = permission.ParentId,
        Path = permission.Path,
        Component = permission.Component,
        Icon = permission.Icon,
        SortOrder = permission.SortOrder,
        Status = permission.IsEnabled ? 1 : 0,
        CreatedAt = permission.CreatedAt
    };
}
