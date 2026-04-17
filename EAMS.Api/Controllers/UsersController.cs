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
    private readonly IImportExportService _importExportService;

    public UsersController(IUserService userService, IImportExportService importExportService)
    {
        _userService = userService;
        _importExportService = importExportService;
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

    /// <summary>
    /// 导出用户
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportUsers([FromQuery] UserQueryRequestDto query)
    {
        var result = await _userService.GetUsersAsync(query);
        var exportData = result.Items.Select(u => new UserExportDto
        {
            Id = u.Id,
            Username = u.Username,
            RealName = u.RealName,
            Email = u.Email,
            Phone = u.Phone,
            Status = u.Status,
            CreatedAt = u.CreatedAt
        });

        var bytes = await _importExportService.ExportToExcelAsync(exportData, "用户列表");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"用户列表_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// 导入用户
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Error("请选择要导入的文件");

        using var stream = file.OpenReadStream();
        var rows = await _importExportService.ImportFromExcelAsync<UserImportDto>(stream);

        var successCount = 0;
        var errors = new List<string>();

        foreach (var (row, index) in rows.Select((r, i) => (r, i + 2)))
        {
            try
            {
                var status = row.StatusText?.Trim() == "启用" ? 1 : 0;
                await _userService.CreateUserAsync(new CreateUserRequestDto
                {
                    Username = row.Username,
                    Password = "123456", // 默认密码
                    RealName = row.RealName,
                    Email = row.Email,
                    Phone = row.Phone
                });
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"第{index}行: {ex.Message}");
            }
        }

        return Success(new { successCount, errorCount = errors.Count, errors }, $"导入完成，成功{successCount}条，默认密码: 123456");
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        var template = new List<UserImportDto>
        {
            new UserImportDto { Username = "zhangsan", RealName = "张三", Email = "zhangsan@example.com", Phone = "13800138000", StatusText = "启用" }
        };
        var bytes = _importExportService.ExportToExcelAsync(template, "用户导入模板").Result;
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "用户导入模板.xlsx");
    }
}
