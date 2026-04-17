using EAMS.Application.DTOs;
using EAMS.Application.Interfaces;
using EAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EAMS.Application.Services;

/// <summary>
/// 员工服务实现
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IRepository<SysEmployee, long> _employeeRepository;
    private readonly IRepository<SysDepartment, long> _departmentRepository;

    public EmployeeService(
        IRepository<SysEmployee, long> employeeRepository,
        IRepository<SysDepartment, long> departmentRepository)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequestDto request)
    {
        // 检查工号是否已存在
        var exists = await _employeeRepository.ExistsAsync(e => e.EmployeeNo == request.EmployeeNo && !e.IsDeleted);
        if (exists)
            throw new InvalidOperationException($"工号 '{request.EmployeeNo}' 已存在");

        // 检查部门是否存在
        if (request.DeptId.HasValue)
        {
            var dept = await _departmentRepository.GetByIdAsync(request.DeptId.Value);
            if (dept == null)
                throw new InvalidOperationException("所属部门不存在");
        }

        var employee = new SysEmployee
        {
            EmployeeNo = request.EmployeeNo,
            RealName = request.RealName,
            Gender = request.Gender,
            IdCard = request.IdCard,
            Phone = request.Phone,
            Email = request.Email,
            DeptId = request.DeptId,
            HireDate = request.HireDate,
            IsEnabled = request.Status == 1,
            Position = request.Position,
            JobTitle = request.JobTitle,
            Remark = request.Remark
        };

        await _employeeRepository.AddAsync(employee);
        return await MapToDtoAsync(employee);
    }

    public async Task<EmployeeDto> UpdateEmployeeAsync(long id, UpdateEmployeeRequestDto request)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            throw new InvalidOperationException("员工不存在");

        // 检查工号唯一性
        if (!string.IsNullOrEmpty(request.EmployeeNo) && request.EmployeeNo != employee.EmployeeNo)
        {
            var exists = await _employeeRepository.ExistsAsync(e => e.EmployeeNo == request.EmployeeNo && e.Id != id && !e.IsDeleted);
            if (exists)
                throw new InvalidOperationException($"工号 '{request.EmployeeNo}' 已存在");
        }

        // 检查部门是否存在
        if (request.DeptId.HasValue)
        {
            var dept = await _departmentRepository.GetByIdAsync(request.DeptId.Value);
            if (dept == null)
                throw new InvalidOperationException("所属部门不存在");
        }

        employee.EmployeeNo = request.EmployeeNo ?? employee.EmployeeNo;
        employee.RealName = request.RealName ?? employee.RealName;
        if (request.Gender.HasValue)
            employee.Gender = request.Gender.Value;
        employee.IdCard = request.IdCard ?? employee.IdCard;
        employee.Phone = request.Phone ?? employee.Phone;
        employee.Email = request.Email ?? employee.Email;
        employee.DeptId = request.DeptId ?? employee.DeptId;
        employee.HireDate = request.HireDate ?? employee.HireDate;
        employee.LeaveDate = request.LeaveDate ?? employee.LeaveDate;
        if (request.Status.HasValue)
            employee.IsEnabled = request.Status.Value == 1;
        employee.Position = request.Position ?? employee.Position;
        employee.JobTitle = request.JobTitle ?? employee.JobTitle;
        employee.Remark = request.Remark ?? employee.Remark;

        await _employeeRepository.UpdateAsync(employee);
        return await MapToDtoAsync(employee);
    }

    public async Task DeleteEmployeeAsync(long id, string deletedBy)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            throw new InvalidOperationException("员工不存在");

        await _employeeRepository.SoftDeleteAsync(id, deletedBy);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(long id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        return employee == null ? null : await MapToDtoAsync(employee);
    }

    public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(EmployeeQueryRequestDto query)
    {
        var q = _employeeRepository.GetQueryable()
            .Where(e => !e.IsDeleted);

        // 关键词搜索
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.ToLower();
            q = q.Where(e => e.RealName.ToLower().Contains(keyword) ||
                            e.EmployeeNo.ToLower().Contains(keyword) ||
                            (e.Phone != null && e.Phone.Contains(keyword)));
        }

        // 状态筛选
        if (query.Status.HasValue)
        {
            q = q.Where(e => e.IsEnabled == (query.Status.Value == 1));
        }

        // 部门筛选
        if (query.DeptId.HasValue)
        {
            q = q.Where(e => e.DeptId == query.DeptId.Value);
        }

        // 性别筛选
        if (query.Gender.HasValue)
        {
            q = q.Where(e => e.Gender == query.Gender.Value);
        }

        // 排序
        q = q.OrderBy(e => e.EmployeeNo).ThenByDescending(e => e.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = new List<EmployeeDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToDtoAsync(item));
        }

        return new PagedResult<EmployeeDto>
        {
            Items = dtos,
            Total = total,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize
        };
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDeptAsync(long deptId)
    {
        var employees = await _employeeRepository.GetQueryable()
            .Where(e => e.DeptId == deptId && !e.IsDeleted && e.IsEnabled)
            .OrderBy(e => e.EmployeeNo)
            .ToListAsync();

        var dtos = new List<EmployeeDto>();
        foreach (var emp in employees)
        {
            dtos.Add(await MapToDtoAsync(emp));
        }
        return dtos;
    }

    private async Task<EmployeeDto> MapToDtoAsync(SysEmployee employee)
    {
        // 获取部门名称
        string? deptName = null;
        if (employee.DeptId.HasValue)
        {
            var dept = await _departmentRepository.GetByIdAsync(employee.DeptId.Value);
            deptName = dept?.DeptName;
        }

        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeNo = employee.EmployeeNo,
            RealName = employee.RealName,
            Gender = employee.Gender,
            IdCard = employee.IdCard,
            Phone = employee.Phone,
            Email = employee.Email,
            DeptId = employee.DeptId,
            HireDate = employee.HireDate,
            LeaveDate = employee.LeaveDate,
            Status = employee.IsEnabled ? 1 : 0,
            Position = employee.Position,
            JobTitle = employee.JobTitle,
            Avatar = employee.Avatar,
            Remark = employee.Remark,
            CreatedAt = employee.CreatedAt,
            ModifiedAt = employee.ModifiedAt,
            DeptName = deptName
        };
    }
}
