using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAMS.Api.Controllers;

/// <summary>
/// 部门管理控制器
/// </summary>
[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : BaseController
{
    private readonly IDepartmentService _departmentService;
    private readonly IImportExportService _importExportService;

    public DepartmentsController(IDepartmentService departmentService, IImportExportService importExportService)
    {
        _departmentService = departmentService;
        _importExportService = importExportService;
    }

    /// <summary>
    /// 分页查询部门
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDepartments([FromQuery] DepartmentQueryRequestDto query)
    {
        var result = await _departmentService.GetDepartmentsAsync(query);
        return Success(result);
    }

    /// <summary>
    /// 获取部门树
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetDepartmentTree()
    {
        var tree = await _departmentService.GetDepartmentTreeAsync();
        return Success(tree);
    }

    /// <summary>
    /// 获取所有部门（不分页）
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllDepartments()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        return Success(departments);
    }

    /// <summary>
    /// 获取部门详情
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetDepartment(long id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);
        if (department == null)
            return Error("部门不存在", 404);
        return Success(department);
    }

    /// <summary>
    /// 创建部门
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequestDto request)
    {
        var department = await _departmentService.CreateDepartmentAsync(request);
        return Success(department, "创建成功");
    }

    /// <summary>
    /// 更新部门
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateDepartment(long id, [FromBody] UpdateDepartmentRequestDto request)
    {
        var department = await _departmentService.UpdateDepartmentAsync(id, request);
        return Success(department, "更新成功");
    }

    /// <summary>
    /// 删除部门
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteDepartment(long id)
    {
        await _departmentService.DeleteDepartmentAsync(id, CurrentUsername);
        return Success<object>(null, "删除成功");
    }

    /// <summary>
    /// 导出部门
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportDepartments([FromQuery] DepartmentQueryRequestDto query)
    {
        var result = await _departmentService.GetDepartmentsAsync(query);
        var exportData = result.Items.Select(d => new DepartmentExportDto
        {
            Id = d.Id,
            DepartmentName = d.DeptName,
            DepartmentCode = d.DeptCode,
            ParentId = d.ParentId,
            ManagerName = d.Leader,
            Phone = d.Phone,
            Status = d.Status,
            SortOrder = d.SortOrder,
            CreatedAt = d.CreatedAt
        });

        var bytes = await _importExportService.ExportToExcelAsync(exportData, "部门列表");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"部门列表_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// 导入部门
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportDepartments(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Error("请选择要导入的文件");

        using var stream = file.OpenReadStream();
        var rows = await _importExportService.ImportFromExcelAsync<DepartmentImportDto>(stream);

        var successCount = 0;
        var errors = new List<string>();

        foreach (var (row, index) in rows.Select((r, i) => (r, i + 2)))
        {
            try
            {
                var status = row.StatusText?.Trim() == "启用" ? 1 : 0;
                await _departmentService.CreateDepartmentAsync(new CreateDepartmentRequestDto
                {
                    DeptName = row.DepartmentName,
                    DeptCode = row.DepartmentCode,
                    ParentId = row.ParentId,
                    Leader = row.ManagerName,
                    Phone = row.Phone,
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
        var template = new List<DepartmentImportDto>
        {
            new DepartmentImportDto { DepartmentName = "示例部门", DepartmentCode = "DEPT001", ManagerName = "张三", Phone = "13800138000", StatusText = "启用", SortOrder = 1 }
        };
        var bytes = _importExportService.ExportToExcelAsync(template, "部门导入模板").Result;
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "部门导入模板.xlsx");
    }
}
