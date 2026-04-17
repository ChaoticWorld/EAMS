using EAMS.Application.DTOs;

namespace EAMS.Application.Interfaces;

/// <summary>
/// 部门服务接口
/// </summary>
public interface IDepartmentService
{
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequestDto request);
    Task<DepartmentDto> UpdateDepartmentAsync(long id, UpdateDepartmentRequestDto request);
    Task DeleteDepartmentAsync(long id, string deletedBy);
    Task<DepartmentDto?> GetDepartmentByIdAsync(long id);
    Task<PagedResult<DepartmentDto>> GetDepartmentsAsync(DepartmentQueryRequestDto query);
    Task<IEnumerable<DepartmentTreeDto>> GetDepartmentTreeAsync();
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
}
