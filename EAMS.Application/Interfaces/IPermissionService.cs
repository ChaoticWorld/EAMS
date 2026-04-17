using EAMS.Application.DTOs;

namespace EAMS.Application.Interfaces;

/// <summary>
/// 权限服务接口
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 创建权限
    /// </summary>
    Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequestDto request);
    
    /// <summary>
    /// 更新权限
    /// </summary>
    Task<PermissionDto> UpdatePermissionAsync(long id, UpdatePermissionRequestDto request);
    
    /// <summary>
    /// 删除权限
    /// </summary>
    Task DeletePermissionAsync(long id, string deletedBy);
    
    /// <summary>
    /// 获取权限详情
    /// </summary>
    Task<PermissionDto?> GetPermissionByIdAsync(long id);
    
    /// <summary>
    /// 获取权限树
    /// </summary>
    Task<IEnumerable<PermissionTreeDto>> GetPermissionTreeAsync();
    
    /// <summary>
    /// 分页查询权限
    /// </summary>
    Task<PagedResult<PermissionDto>> GetPermissionsAsync(PermissionQueryRequestDto query);
    
    /// <summary>
    /// 获取用户的权限列表
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(long userId);
    
    /// <summary>
    /// 检查用户是否有指定权限
    /// </summary>
    Task<bool> HasPermissionAsync(long userId, string permissionCode);
}
