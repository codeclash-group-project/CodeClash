using CodeClash.Application.Common.Interfaces;
using CodeClash.Infrastructure.Hubs;
using CodeClash.Infrastructure.Persistence;
using CodeClash.Infrastructure.Persistence.Repositories;
using CodeClash.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CodeClash.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(
            provider => provider.GetRequiredService<ApplicationDbContext>());

        // ── Repositories ──────────────────────────────────────────────────────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMatchArenaRepository, MatchArenaRepository>();
        services.AddScoped<IMatchmakingQueueRepository, MatchmakingQueueRepository>();

        // ── Services ──────────────────────────────────────────────────────────
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IEloRatingService, EloRatingService>();
        services.AddScoped<IProblemSelectorService, ProblemSelectorService>();
        services.AddScoped<IMatchHubService, MatchHubService>();

        // ── Background Services ───────────────────────────────────────────────
        services.AddHostedService<MatchmakingBackgroundService>();

        return services;
    }
}