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
    private readonly IImportExportService _importExportService;

    public RolesController(IRoleService roleService, IImportExportService importExportService)
    {
        _roleService = roleService;
        _importExportService = importExportService;
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

    /// <summary>
    /// 导出角色
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportRoles([FromQuery] RoleQueryRequestDto query)
    {
        var result = await _roleService.GetRolesAsync(query);
        var exportData = result.Items.Select(r => new RoleExportDto
        {
            Id = r.Id,
            RoleName = r.RoleName,
            RoleCode = r.RoleCode,
            Description = r.Description,
            Status = r.Status,
            SortOrder = r.SortOrder,
            CreatedAt = r.CreatedAt
        });

        var bytes = await _importExportService.ExportToExcelAsync(exportData, "角色列表");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"角色列表_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// 导入角色
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportRoles(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Error("请选择要导入的文件");

        using var stream = file.OpenReadStream();
        var rows = await _importExportService.ImportFromExcelAsync<RoleImportDto>(stream);

        var successCount = 0;
        var errors = new List<string>();

        foreach (var (row, index) in rows.Select((r, i) => (r, i + 2)))
        {
            try
            {
                var status = row.StatusText?.Trim() == "启用" ? 1 : 0;
                await _roleService.CreateRoleAsync(new CreateRoleRequestDto
                {
                    RoleName = row.RoleName,
                    RoleCode = row.RoleCode,
                    Description = row.Description,
                    Status = status,
                    SortOrder = row.SortOrder
                });
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"第{index}行: {ex.Message}");
            }
        }

        return Success(new { successCount, errorCount = errors.Count, errors }, $"导入完成，成功{successCount}条");
    }

    /// <summary>
    /// 下载导入模板
    /// </summary>
    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        var template = new List<RoleImportDto>
        {
            new RoleImportDto { RoleName = "示例角色", RoleCode = "example", Description = "这是一个示例", StatusText = "启用", SortOrder = 1 }
        };
        var bytes = _importExportService.ExportToExcelAsync(template, "角色导入模板").Result;
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "角色导入模板.xlsx");
    }
}
