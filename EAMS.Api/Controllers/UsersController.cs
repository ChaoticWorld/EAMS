using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAMS.Api.Controllers;

/// <summary>
/// 用户管理控制器
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 分页查询用户
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserQueryRequestDto query)
    {
        var result = await _userService.GetUsersAsync(query);
        return Success(result);
    }

    /// <summary>
    /// 获取用户详情
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetUser(long id)
    {
        var user = await _userService.GetCurrentUserAsync(id.ToString());
        if (user == null)
            return Error("用户不存在", 404);
        return Success(user);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        var user = await _userService.CreateUserAsync(request);
        return Success(user, "创建成功");
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserRequestDto request)
    {
        var user = await _userService.UpdateUserAsync(id, request);
        return Success(user, "更新成功");
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        await _userService.DeleteUserAsync(id, CurrentUsername);
        return Success<object>(null, "删除成功");
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    [HttpPost("{id:long}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id, [FromBody] string newPassword)
    {
        await _userService.ResetPasswordAsync(id, newPassword, CurrentUsername);
        return Success<object>(null, "密码重置成功");
    }

    /// <summary>
    /// 分配角色
    /// </summary>
    [HttpPost("{id:long}/roles")]
    public async Task<IActionResult> AssignRoles(long id, [FromBody] IEnumerable<long> roleIds)
    {
        await _userService.AssignRolesAsync(id, roleIds, CurrentUsername);
        return Success<object>(null, "角色分配成功");
    }

    /// <summary>
    /// 获取用户角色
    /// </summary>
    [HttpGet("{id:long}/roles")]
    public async Task<IActionResult> GetUserRoles(long id)
    {
        var roles = await _userService.GetUserRolesAsync(id);
        return Success(roles);
    }
}
