using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAMS.Api.Controllers;

/// <summary>
/// 权限管理控制器
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : BaseController
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// 分页查询权限
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPermissions([FromQuery] PermissionQueryRequestDto query)
    {
        var result = await _permissionService.GetPermissionsAsync(query);
        return Success(result);
    }

    /// <summary>
    /// 获取权限树
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetPermissionTree()
    {
        var tree = await _permissionService.GetPermissionTreeAsync();
        return Success(tree);
    }

    /// <summary>
    /// 获取权限详情
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPermission(long id)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(id);
        if (permission == null)
            return Error("权限不存在", 404);
        return Success(permission);
    }

    /// <summary>
    /// 创建权限
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequestDto request)
    {
        var permission = await _permissionService.CreatePermissionAsync(request);
        return Success(permission, "创建成功");
    }

    /// <summary>
    /// 更新权限
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePermission(long id, [FromBody] UpdatePermissionRequestDto request)
    {
        var permission = await _permissionService.UpdatePermissionAsync(id, request);
        return Success(permission, "更新成功");
    }

    /// <summary>
    /// 删除权限
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeletePermission(long id)
    {
        await _permissionService.DeletePermissionAsync(id, CurrentUsername);
        return Success<object>(null, "删除成功");
    }

    /// <summary>
    /// 获取当前用户权限
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPermissions()
    {
        var permissions = await _permissionService.GetUserPermissionsAsync(CurrentUserId);
        return Success(permissions);
    }

    /// <summary>
    /// 检查权限
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission([FromQuery] string code)
    {
        var hasPermission = await _permissionService.HasPermissionAsync(CurrentUserId, code);
        return Success(new { hasPermission });
    }
}
