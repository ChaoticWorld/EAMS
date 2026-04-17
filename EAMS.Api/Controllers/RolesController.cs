using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAMS.Api.Controllers;

/// <summary>
/// 角色管理控制器
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// 分页查询角色
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRoles([FromQuery] RoleQueryRequestDto query)
    {
        var result = await _roleService.GetRolesAsync(query);
        return Success(result);
    }

    /// <summary>
    /// 获取所有角色
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Success(roles);
    }

    /// <summary>
    /// 获取角色详情
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetRole(long id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
            return Error("角色不存在", 404);
        return Success(role);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
    {
        var role = await _roleService.CreateRoleAsync(request);
        return Success(role, "创建成功");
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateRoleRequestDto request)
    {
        var role = await _roleService.UpdateRoleAsync(id, request);
        return Success(role, "更新成功");
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteRole(long id)
    {
        await _roleService.DeleteRoleAsync(id, CurrentUsername);
        return Success<object>(null, "删除成功");
    }

    /// <summary>
    /// 分配权限
    /// </summary>
    [HttpPost("{id:long}/permissions")]
    public async Task<IActionResult> AssignPermissions(long id, [FromBody] IEnumerable<long> permissionIds)
    {
        await _roleService.AssignPermissionsAsync(id, permissionIds, CurrentUsername);
        return Success<object>(null, "权限分配成功");
    }

    /// <summary>
    /// 获取角色权限
    /// </summary>
    [HttpGet("{id:long}/permissions")]
    public async Task<IActionResult> GetRolePermissions(long id)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(id);
        return Success(permissions);
    }
}
