using EAMS.Application.Interfaces;
using EAMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EAMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // 数据库上下文配置 - 由调用方(Api项目)注册具体的DbContext实现
        // 这里只注册通用仓储
        
        // 注册通用仓储
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        return services;
    }
}
