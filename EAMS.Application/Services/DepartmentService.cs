using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using EAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EAMS.Application.Services;

/// <summary>
/// 部门服务实现
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IRepository<SysDepartment, long> _departmentRepository;
    private readonly IRepository<SysEmployee, long> _employeeRepository;

    public DepartmentService(
        IRepository<SysDepartment, long> departmentRepository,
        IRepository<SysEmployee, long> employeeRepository)
    {
        _departmentRepository = departmentRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequestDto request)
    {
        // 检查部门编码是否已存在
        if (!string.IsNullOrEmpty(request.DeptCode))
        {
            var exists = await _departmentRepository.ExistsAsync(d => d.DeptCode == request.DeptCode && !d.IsDeleted);
            if (exists)
                throw new InvalidOperationException($"部门编码 '{request.DeptCode}' 已存在");
        }

        // 检查父部门是否存在
        if (request.ParentId.HasValue)
        {
            var parent = await _departmentRepository.GetByIdAsync(request.ParentId.Value);
            if (parent == null)
                throw new InvalidOperationException("父部门不存在");
        }

        var department = new SysDepartment
        {
            DeptName = request.DeptName,
            DeptCode = request.DeptCode,
            ParentId = request.ParentId,
            Description = request.Description,
            IsEnabled = request.Status == 1,
            SortOrder = request.SortOrder,
            Leader = request.Leader,
            Phone = request.Phone,
            Email = request.Email
        };

        await _departmentRepository.AddAsync(department);
        return await MapToDtoAsync(department);
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(long id, UpdateDepartmentRequestDto request)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new InvalidOperationException("部门不存在");

        // 检查部门编码唯一性
        if (!string.IsNullOrEmpty(request.DeptCode) && request.DeptCode != department.DeptCode)
        {
            var exists = await _departmentRepository.ExistsAsync(d => d.DeptCode == request.DeptCode && d.Id != id && !d.IsDeleted);
            if (exists)
                throw new InvalidOperationException($"部门编码 '{request.DeptCode}' 已存在");
        }

        // 不能将自己设为父部门
        if (request.ParentId == id)
            throw new InvalidOperationException("不能将部门设为自己的子部门");

        department.DeptName = request.DeptName ?? department.DeptName;
        department.DeptCode = request.DeptCode ?? department.DeptCode;
        department.ParentId = request.ParentId ?? department.ParentId;
        department.Description = request.Description ?? department.Description;
        if (request.Status.HasValue)
            department.IsEnabled = request.Status.Value == 1;
        if (request.SortOrder.HasValue)
            department.SortOrder = request.SortOrder.Value;
        department.Leader = request.Leader ?? department.Leader;
        department.Phone = request.Phone ?? department.Phone;
        department.Email = request.Email ?? department.Email;

        await _departmentRepository.UpdateAsync(department);
        return await MapToDtoAsync(department);
    }

    public async Task DeleteDepartmentAsync(long id, string deletedBy)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            throw new InvalidOperationException("部门不存在");

        // 检查是否有子部门
        var hasChildren = await _departmentRepository.ExistsAsync(d => d.ParentId == id && !d.IsDeleted);
        if (hasChildren)
            throw new InvalidOperationException("该部门下存在子部门，无法删除");

        // 检查是否有员工
        var hasEmployees = await _employeeRepository.ExistsAsync(e => e.DeptId == id && !e.IsDeleted);
        if (hasEmployees)
            throw new InvalidOperationException("该部门下存在员工，无法删除");

        await _departmentRepository.SoftDeleteAsync(id, deletedBy);
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(long id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        return department == null ? null : await MapToDtoAsync(department);
    }

    public async Task<PagedResult<DepartmentDto>> GetDepartmentsAsync(DepartmentQueryRequestDto query)
    {
        var q = _departmentRepository.GetQueryable()
            .Where(d => !d.IsDeleted);

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(d => d.DeptName.ToLower().Contains(keyword) ||
                            (d.DeptCode != null && d.DeptCode.ToLower().Contains(keyword)));
        }

        // 状态筛选
        if (query.Status.HasValue)
        {
            q = q.Where(d => d.IsEnabled == (query.Status.Value == 1));
        }

        // 父部门筛选
        if (query.ParentId.HasValue)
        {
            q = q.Where(d => d.ParentId == query.ParentId.Value);
        }

        // 排序
        q = q.OrderBy(d => d.SortOrder).ThenByDescending(d => d.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = new List<DepartmentDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToDtoAsync(item));
        }

        return new PagedResult<DepartmentDto>
        {
            Items = dtos,
            Total = total,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<IEnumerable<DepartmentTreeDto>> GetDepartmentTreeAsync()
    {
        var departments = await _departmentRepository.GetQueryable()
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

        return BuildTree(departments, null);
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
    {
        var departments = await _departmentRepository.GetQueryable()
            .Where(d => !d.IsDeleted && d.IsEnabled)
            .OrderBy(d => d.SortOrder)
            .ToListAsync();

        var dtos = new List<DepartmentDto>();
        foreach (var dept in departments)
        {
            dtos.Add(await MapToDtoAsync(dept));
        }
        return dtos;
    }

    private async Task<DepartmentDto> MapToDtoAsync(SysDepartment department)
    {
        // 获取父部门名称
        string? parentName = null;
        if (department.ParentId.HasValue)
        {
            var parent = await _departmentRepository.GetByIdAsync(department.ParentId.Value);
            parentName = parent?.DeptName;
        }

        // 获取员工数量
        var employeeCount = await _employeeRepository.GetQueryable()
            .Where(e => e.DeptId == department.Id && !e.IsDeleted)
            .CountAsync();

        return new DepartmentDto
        {
            Id = department.Id,
            DeptName = department.DeptName,
            DeptCode = department.DeptCode,
            ParentId = department.ParentId,
            Description = department.Description,
            Status = department.IsEnabled ? 1 : 0,
            SortOrder = department.SortOrder,
            Leader = department.Leader,
            Phone = department.Phone,
            Email = department.Email,
            CreatedAt = department.CreatedAt,
            ParentName = parentName,
            EmployeeCount = employeeCount
        };
    }

    private IEnumerable<DepartmentTreeDto> BuildTree(List<SysDepartment> all, long? parentId)
    {
        return all
            .Where(d => d.ParentId == parentId)
            .Select(d => new DepartmentTreeDto
            {
                Id = d.Id,
                DeptName = d.DeptName,
                ParentId = d.ParentId,
                SortOrder = d.SortOrder,
                Children = BuildTree(all, d.Id)
            });
    }
}
