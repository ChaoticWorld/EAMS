using EAMS.Application.Interfaces;
using EAMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EAMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 注册服务
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IMessageService, MessageService>();

        return services;
    }
}
