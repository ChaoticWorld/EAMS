using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EAMS.Api.Controllers;

/// <summary>
/// 员工管理控制器
/// </summary>
[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : BaseController
{
    private readonly IEmployeeService _employeeService;
    private readonly IImportExportService _importExportService;

    public EmployeesController(IEmployeeService employeeService, IImportExportService importExportService)
    {
        _employeeService = employeeService;
        _importExportService = importExportService;
    }

    /// <summary>
    /// 分页查询员工
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] EmployeeQueryRequestDto query)
    {
        var result = await _employeeService.GetEmployeesAsync(query);
        return Success(result);
    }

    /// <summary>
    /// 获取部门下的员工
    /// </summary>
    [HttpGet("department/{deptId:long}")]
    public async Task<IActionResult> GetEmployeesByDept(long deptId)
    {
        var employees = await _employeeService.GetEmployeesByDeptAsync(deptId);
        return Success(employees);
    }

    /// <summary>
    /// 获取员工详情
    /// </summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetEmployee(long id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
            return Error("员工不存在", 404);
        return Success(employee);
    }

    /// <summary>
    /// 创建员工
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto request)
    {
        var employee = await _employeeService.CreateEmployeeAsync(request);
        return Success(employee, "创建成功");
    }

    /// <summary>
    /// 更新员工
    /// </summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateEmployee(long id, [FromBody] UpdateEmployeeRequestDto request)
    {
        var employee = await _employeeService.UpdateEmployeeAsync(id, request);
        return Success(employee, "更新成功");
    }

    /// <summary>
    /// 删除员工
    /// </summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteEmployee(long id)
    {
        await _employeeService.DeleteEmployeeAsync(id, CurrentUsername);
        return Success<object>(null, "删除成功");
    }

    /// <summary>
    /// 导出员工
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportEmployees([FromQuery] EmployeeQueryRequestDto query)
    {
        var result = await _employeeService.GetEmployeesAsync(query);
        var exportData = result.Items.Select(e => new EmployeeExportDto
        {
            Id = e.Id,
            EmployeeNo = e.EmployeeNo,
            Name = e.RealName,
            Gender = e.Gender,
            DepartmentId = e.DeptId,
            DepartmentName = e.DeptName,
            Position = e.Position,
            Phone = e.Phone,
            Email = e.Email,
            HireDate = e.HireDate,
            Status = e.Status,
            CreatedAt = e.CreatedAt
        });

        var bytes = await _importExportService.ExportToExcelAsync(exportData, "员工列表");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"员工列表_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
    }

    /// <summary>
    /// 导入员工
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> ImportEmployees(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Error("请选择要导入的文件");

        using var stream = file.OpenReadStream();
        var rows = await _importExportService.ImportFromExcelAsync<EmployeeImportDto>(stream);

        var successCount = 0;
        var errors = new List<string>();

        foreach (var (row, index) in rows.Select((r, i) => (r, i + 2)))
        {
            try
            {
                var gender = row.GenderText?.Trim() switch
                {
                    "男" => 1,
                    "女" => 2,
                    _ => 0
                };
                var status = row.StatusText?.Trim() switch
                {
                    "在职" => 1,
                    "离职" => 2,
                    "休假" => 3,
                    _ => 1
                };
                await _employeeService.CreateEmployeeAsync(new CreateEmployeeRequestDto
                {
                    EmployeeNo = row.EmployeeNo,
                    RealName = row.Name,
                    Gender = gender,
                    DeptId = row.DepartmentId,
                    Position = row.Position,
                    Phone = row.Phone,
                    Email = row.Email,
                    HireDate = row.HireDate,
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
        var template = new List<EmployeeImportDto>
        {
            new EmployeeImportDto { EmployeeNo = "EMP001", Name = "张三", GenderText = "男", DepartmentId = 1, Position = "工程师", Phone = "13800138000", Email = "zhangsan@example.com", StatusText = "在职" }
        };
        var bytes = _importExportService.ExportToExcelAsync(template, "员工导入模板").Result;
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "员工导入模板.xlsx");
    }
}
