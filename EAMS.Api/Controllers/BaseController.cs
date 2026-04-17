using EAMS.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EAMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Success(data, message));
    }

    protected IActionResult Error(string message, int code = -1)
    {
        return Ok(ApiResponse<object>.Error(message, code));
    }

    protected long CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

    protected string CurrentUsername => User.FindFirst("username")?.Value ?? string.Empty;
}
