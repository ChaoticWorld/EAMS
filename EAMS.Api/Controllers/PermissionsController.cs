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
    private readonly IImportExportService _importExportService;

    public PermissionsController(IPermissionService permissionService, IImportExportService importExportService)
    {
        _permissionService = permissionService;
        _importExportService = importExportService;
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

    /// <summary>
    /// 导出权限
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportPermissions([FromQuery] PermissionQueryRequestDto query)
    {
        var result = await _permissionService.GetPermissionsAsync(query);
        var exportData = result.Items.Select(p => new PermissionExportDto
        {
            Id = p.Id,
            PermissionName = p.PermissionName,
            PermissionCode = p.PermissionCode,
            Type = p.PermissionType?.Trim() switch
            {
                "catalog" => 1,
                "menu" => 2,
                "button" => 3,
                _ => 2
            },
            ParentId = p.ParentId,
            Path = p.Path,
            Icon = p.Icon,
            SortOrder = p.SortOrder,
            Status = p.Status,
            CreatedAt = p.CreatedAt
        });

        var bytes = await _importExportService.ExportToExcelAsync(exportData, "权限列表");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"权限列表_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// 导入权限
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportPermissions(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Error("请选择要导入的文件");

        using var stream = file.OpenReadStream();
        var rows = await _importExportService.ImportFromExcelAsync<PermissionImportDto>(stream);

        var successCount = 0;
        var errors = new List<string>();

        foreach (var (row, index) in rows.Select((r, i) => (r, i + 2)))
        {
            try
            {
                var type = row.TypeText?.Trim() switch
                {
                    "目录" => "catalog",
                    "菜单" => "menu",
                    "按钮" => "button",
                    _ => "menu"
                };
                var status = row.StatusText?.Trim() == "启用" ? 1 : 0;
                await _permissionService.CreatePermissionAsync(new CreatePermissionRequestDto
                {
                    PermissionName = row.PermissionName,
                    PermissionCode = row.PermissionCode,
                    PermissionType = type,
                    ParentId = row.ParentId,
                    Path = row.Path,
                    Icon = row.Icon,
                    SortOrder = row.SortOrder,
                    Status = status
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
        var template = new List<PermissionImportDto>
        {
            new PermissionImportDto { PermissionName = "示例菜单", PermissionCode = "sys:example", TypeText = "菜单", Path = "/example", Icon = "Document", SortOrder = 1, StatusText = "启用" }
        };
        var bytes = _importExportService.ExportToExcelAsync(template, "权限导入模板").Result;
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "权限导入模板.xlsx");
    }
}
