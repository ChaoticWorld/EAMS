using EAMS.Application.DTOs;

namespace EAMS.Application.Interfaces;

/// <summary>
/// 员工服务接口
/// </summary>
public interface IEmployeeService
{
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequestDto request);
    Task<EmployeeDto> UpdateEmployeeAsync(long id, UpdateEmployeeRequestDto request);
    Task DeleteEmployeeAsync(long id, string deletedBy);
    Task<EmployeeDto?> GetEmployeeByIdAsync(long id);
    Task<PagedResult<EmployeeDto>> GetEmployeesAsync(EmployeeQueryRequestDto query);
    Task<IEnumerable<EmployeeDto>> GetEmployeesByDeptAsync(long deptId);
}
