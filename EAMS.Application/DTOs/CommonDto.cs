namespace EAMS.Application.DTOs;

/// <summary>
/// 分页请求基类
/// </summary>
public class PagedRequestDto
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortField { get; set; }
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// 分页结果
/// </summary>
public class PagedResult<T>
{
    public int Total { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    public IEnumerable<T> Items { get; set; } = new List<T>();
}

/// <summary>
/// API 统一响应
/// </summary>
public class ApiResponse<T>
{
    public int Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static ApiResponse<T> Success(T data, string? message = null)
        => new() { Code = 0, Data = data, Message = message ?? "success" };

    public static ApiResponse<T> Error(string message, int code = -1)
        => new() { Code = code, Message = message };
}
