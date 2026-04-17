namespace EAMS.Application.Interfaces;

/// <summary>
/// JWT Token 服务接口
/// </summary>
public interface IJwtService
{
    string GenerateToken(long userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions);
}
