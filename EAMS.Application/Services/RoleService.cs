using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using EAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EAMS.Application.Services;

/// <summary>
/// 角色服务实现
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRepository<SysRole, long> _roleRepository;
    private readonly IRepository<SysRolePermission, long> _rolePermissionRepository;
    private readonly IRepository<SysPermission, long> _permissionRepository;

    public RoleService(
        IRepository<SysRole, long> roleRepository,
        IRepository<SysRolePermission, long> rolePermissionRepository,
        IRepository<SysPermission, long> permissionRepository)
    {
        _roleRepository = roleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request)
    {
        // 检查角色代码是否已存在
        if (await _roleRepository.ExistsAsync(r => r.RoleCode == request.RoleCode))
        {
            throw new InvalidOperationException($"角色代码 '{request.RoleCode}' 已存在");
        }

        var role = new SysRole
        {
            RoleName = request.RoleName,
            RoleCode = request.RoleCode,
            Description = request.Description,
            IsEnabled = request.Status == 1,
            SortOrder = request.SortOrder
        };

        await _roleRepository.AddAsync(role);

        // 分配权限
        if (request.PermissionIds?.Any() == true)
        {
            foreach (var permissionId in request.PermissionIds)
            {
                await _rolePermissionRepository.AddAsync(new SysRolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }
        }

        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(long id, UpdateRoleRequestDto request)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new InvalidOperationException("角色不存在");

        role.RoleName = request.RoleName ?? role.RoleName;
        role.Description = request.Description ?? role.Description;
        if (request.Status.HasValue)
            role.IsEnabled = request.Status.Value == 1;
        if (request.SortOrder.HasValue)
            role.SortOrder = request.SortOrder.Value;

        await _roleRepository.UpdateAsync(role);
        return MapToDto(role);
    }

    public async Task DeleteRoleAsync(long id, string deletedBy)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            throw new InvalidOperationException("角色不存在");

        // 检查是否有用户关联此角色
        var hasUsers = await _rolePermissionRepository.ExistsAsync(rp => rp.RoleId == id);
        if (hasUsers)
        {
            // 先删除角色权限关联
            var rolePermissions = await _rolePermissionRepository.FindListAsync(rp => rp.RoleId == id);
            foreach (var rp in rolePermissions)
            {
                await _rolePermissionRepository.DeleteAsync(rp);
            }
        }

        await _roleRepository.DeleteAsync(id);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(long id)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<PagedResult<RoleDto>> GetRolesAsync(RoleQueryRequestDto query)
    {
        var q = _roleRepository.GetQueryable()
            .Where(r => !r.IsDeleted);

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(r => r.RoleName.ToLower().Contains(keyword) ||
                            r.RoleCode.ToLower().Contains(keyword));
        }

        // 状态筛选
        if (query.Status.HasValue)
        {
            q = q.Where(r => r.IsEnabled == (query.Status.Value == 1));
        }

        // 排序
        q = q.OrderBy(r => r.SortOrder).ThenByDescending(r => r.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<RoleDto>
        {
            Items = items.Select(MapToDto),
            Total = total,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task AssignPermissionsAsync(long roleId, IEnumerable<long> permissionIds, string operatorId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new InvalidOperationException("角色不存在");

        // 删除现有权限关联
        var existing = await _rolePermissionRepository.FindListAsync(rp => rp.RoleId == roleId);
        foreach (var rp in existing)
        {
            await _rolePermissionRepository.DeleteAsync(rp);
        }

        // 添加新权限
        foreach (var permissionId in permissionIds)
        {
            await _rolePermissionRepository.AddAsync(new SysRolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });
        }
    }

    public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(long roleId)
    {
        var permissionIds = await _rolePermissionRepository.GetQueryable()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var permissions = await _permissionRepository.GetQueryable()
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync();

        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            PermissionName = p.PermissionName,
            PermissionCode = p.PermissionCode,
            PermissionType = p.PermissionType,
            ParentId = p.ParentId,
            Path = p.Path,
            Component = p.Component,
            Icon = p.Icon,
            SortOrder = p.SortOrder,
            Status = p.IsEnabled ? 1 : 0,
            CreatedAt = p.CreatedAt
        });
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleRepository.GetQueryable()
            .Where(r => !r.IsDeleted && r.IsEnabled)
            .OrderBy(r => r.SortOrder)
            .ToListAsync();

        return roles.Select(MapToDto);
    }

    private static RoleDto MapToDto(SysRole role) => new()
    {
        Id = role.Id,
        RoleName = role.RoleName,
        RoleCode = role.RoleCode,
        Description = role.Description,
        Status = role.IsEnabled ? 1 : 0,
        SortOrder = role.SortOrder,
        CreatedAt = role.CreatedAt
    };
}
