namespace EAMS.Application.DTOs;

// ========== 员工 CRUD ==========
public class CreateEmployeeRequestDto
{
    public string EmployeeNo { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public int Gender { get; set; } = 0;
    public string? IdCard { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public long? DeptId { get; set; }
    public DateTime? HireDate { get; set; }
    public int Status { get; set; } = 1;
    public string? Position { get; set; }
    public string? JobTitle { get; set; }
    public string? Remark { get; set; }
}

public class UpdateEmployeeRequestDto
{
    public string? EmployeeNo { get; set; }
    public string? RealName { get; set; }
    public int? Gender { get; set; }
    public string? IdCard { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public long? DeptId { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public int? Status { get; set; }
    public string? Position { get; set; }
    public string? JobTitle { get; set; }
    public string? Remark { get; set; }
}

public class EmployeeQueryRequestDto : PagedRequestDto
{
    public string? Keyword { get; set; }
    public int? Status { get; set; }
    public long? DeptId { get; set; }
    public int? Gender { get; set; }
}

// ========== 员工响应 ==========
public class EmployeeDto
{
    public long Id { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public int Gender { get; set; }
    public string? IdCard { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public long? DeptId { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public int Status { get; set; }
    public string? Position { get; set; }
    public string? JobTitle { get; set; }
    public string? Avatar { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? DeptName { get; set; }
    public long? UserId { get; set; }
}
