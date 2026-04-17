namespace EAMS.Application.DTOs;

// ========== 登录相关 ==========
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public long ExpiresIn { get; set; }
    public UserDto User { get; set; } = null!;
}

// ========== 用户 CRUD ==========
public class CreateUserRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public int Status { get; set; } = 1;
    public IEnumerable<long>? RoleIds { get; set; }
}

public class UpdateUserRequestDto
{
    public string? RealName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public int? Status { get; set; }
}

public class ChangePasswordRequestDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class UserQueryRequestDto : PagedRequestDto
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
    public long? RoleId { get; set; }
}

// ========== 用户响应 ==========
public class UserDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public IEnumerable<RoleDto>? Roles { get; set; }
}
