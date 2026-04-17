using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using EAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace EAMS.Application.Services;

/// <summary>
/// 用户服务实现
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<SysUser, long> _userRepository;
    private readonly IRepository<SysRole, long> _roleRepository;
    private readonly IRepository<SysUserRole, long> _userRoleRepository;
    private readonly IRepository<SysRolePermission, long> _rolePermissionRepository;
    private readonly IRepository<SysPermission, long> _permissionRepository;
    private readonly IJwtService _jwtService;

    public UserService(
        IRepository<SysUser, long> userRepository,
        IRepository<SysRole, long> roleRepository,
        IRepository<SysUserRole, long> userRoleRepository,
        IRepository<SysRolePermission, long> rolePermissionRepository,
        IRepository<SysPermission, long> permissionRepository,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.FindAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("用户名或密码错误");
        }

        if (!user.IsEnabled)
        {
            throw new UnauthorizedAccessException("账号已被禁用");
        }

        // 更新最后登录时间
        user.LoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // 获取用户角色
        var roleIds = await _userRoleRepository.GetQueryable()
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var roles = await _roleRepository.GetQueryable()
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.RoleCode)
            .ToListAsync();

        // 获取用户权限
        var permissionIds = await _rolePermissionRepository.GetQueryable()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync();

        var permissions = await _permissionRepository.GetQueryable()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.PermissionCode)
            .ToListAsync();

        // 生成 JWT Token
        var token = _jwtService.GenerateToken(user.Id, user.Username, roles, permissions);

        return new LoginResultDto
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = 86400,
            User = MapToDto(user)
        };
    }

    public Task LogoutAsync(string userId) => Task.CompletedTask;

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        if (!long.TryParse(userId, out var id))
            return null;

        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request)
    {
        // 检查用户名是否已存在
        if (await _userRepository.ExistsAsync(u => u.Username == request.Username))
        {
            throw new InvalidOperationException("用户名已存在");
        }

        var user = new SysUser
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RealName = request.RealName,
            Email = request.Email,
            Phone = request.Phone,
            IsEnabled = request.Status == 1
        };

        await _userRepository.AddAsync(user);

        // 分配角色
        if (request.RoleIds?.Any() == true)
        {
            foreach (var roleId in request.RoleIds)
            {
                await _userRoleRepository.AddAsync(new SysUserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }
        }

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(long id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("用户不存在");

        user.RealName = request.RealName ?? user.RealName;
        user.Email = request.Email ?? user.Email;
        user.Phone = request.Phone ?? user.Phone;
        if (request.Status.HasValue)
            user.IsEnabled = request.Status.Value == 1;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task DeleteUserAsync(long id, string deletedBy)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("用户不存在");

        await _userRepository.DeleteAsync(id);
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(UserQueryRequestDto query)
    {
        var q = _userRepository.GetQueryable()
            .Where(u => !u.IsDeleted);

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(u => u.Username.ToLower().Contains(keyword) ||
                            u.RealName.ToLower().Contains(keyword));
        }

        // 状态筛选
        if (query.Status.HasValue)
        {
            q = q.Where(u => u.IsEnabled == (query.Status.Value == 1));
        }

        // 排序
        q = q.OrderByDescending(u => u.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResult<UserDto>
        {
            Items = items.Select(MapToDto),
            Total = total
        };
    }

    public async Task ChangePasswordAsync(long id, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("用户不存在");

        if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("原密码错误");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);
    }

    public async Task ResetPasswordAsync(long id, string newPassword, string operatorId)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("用户不存在");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);
    }

    public async Task AssignRolesAsync(long userId, IEnumerable<long> roleIds, string operatorId)
    {
        // 删除现有角色关联
        var existing = await _userRoleRepository.FindListAsync(ur => ur.UserId == userId);

        foreach (var ur in existing)
        {
            await _userRoleRepository.DeleteAsync(ur);
        }

        // 添加新角色
        foreach (var roleId in roleIds)
        {
            await _userRoleRepository.AddAsync(new SysUserRole
            {
                UserId = userId,
                RoleId = roleId
            });
        }
    }

    public async Task<IEnumerable<RoleDto>> GetUserRolesAsync(long userId)
    {
        var roleIds = await _userRoleRepository.GetQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var roles = await _roleRepository.GetQueryable()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();

        return roles.Select(r => new RoleDto
        {
            Id = r.Id,
            RoleName = r.RoleName,
            Description = r.Description,
            Status = r.IsEnabled ? 1 : 0
        });
    }

    private static UserDto MapToDto(SysUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        RealName = user.RealName,
        Email = user.Email,
        Phone = user.Phone,
        Status = user.IsEnabled ? 1 : 0,
        CreatedAt = user.CreatedAt
    };
}
