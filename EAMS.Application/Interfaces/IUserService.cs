using EAMS.Application.DTOs;
using EAMS.Domain.Abstractions;

namespace EAMS.Application.Interfaces;

/// <summary>
/// 用户服务接口
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<LoginResultDto> LoginAsync(LoginRequestDto request);
    
    /// <summary>
    /// 用户登出
    /// </summary>
    Task LogoutAsync(string userId);
    
    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    Task<UserDto?> GetCurrentUserAsync(string userId);
    
    /// <summary>
    /// 创建用户
    /// </summary>
    Task<UserDto> CreateUserAsync(CreateUserRequestDto request);
    
    /// <summary>
    /// 更新用户
    /// </summary>
    Task<UserDto> UpdateUserAsync(long id, UpdateUserRequestDto request);
    
    /// <summary>
    /// 删除用户（软删除）
    /// </summary>
    Task DeleteUserAsync(long id, string deletedBy);
    
    /// <summary>
    /// 分页查询用户
    /// </summary>
    Task<PagedResult<UserDto>> GetUsersAsync(UserQueryRequestDto query);
    
    /// <summary>
    /// 修改密码
    /// </summary>
    Task ChangePasswordAsync(long id, ChangePasswordRequestDto request);
    
    /// <summary>
    /// 重置密码
    /// </summary>
    Task ResetPasswordAsync(long id, string newPassword, string operatorId);
    
    /// <summary>
    /// 分配角色
    /// </summary>
    Task AssignRolesAsync(long userId, IEnumerable<long> roleIds, string operatorId);
    
    /// <summary>
    /// 获取用户角色
    /// </summary>
    Task<IEnumerable<RoleDto>> GetUserRolesAsync(long userId);
}
