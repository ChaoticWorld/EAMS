using EAMS.Application.DTOs;

namespace EAMS.Application.Interfaces;

/// <summary>
/// 角色服务接口
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 创建角色
    /// </summary>
    Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request);
    
    /// <summary>
    /// 更新角色
    /// </summary>
    Task<RoleDto> UpdateRoleAsync(long id, UpdateRoleRequestDto request);
    
    /// <summary>
    /// 删除角色
    /// </summary>
    Task DeleteRoleAsync(long id, string deletedBy);
    
    /// <summary>
    /// 获取角色详情
    /// </summary>
    Task<RoleDto?> GetRoleByIdAsync(long id);
    
    /// <summary>
    /// 分页查询角色
    /// </summary>
    Task<PagedResult<RoleDto>> GetRolesAsync(RoleQueryRequestDto query);
    
    /// <summary>
    /// 分配权限
    /// </summary>
    Task AssignPermissionsAsync(long roleId, IEnumerable<long> permissionIds, string operatorId);
    
    /// <summary>
    /// 获取角色权限
    /// </summary>
    Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(long roleId);
    
    /// <summary>
    /// 获取所有角色（不分页）
    /// </summary>
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
}
