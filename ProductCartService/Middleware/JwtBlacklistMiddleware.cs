using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public JwtBlacklistMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            var token = authorizationHeader.ToString().Replace("Bearer ", "");

            // Попробуем извлечь токен
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Проверяем токен в Redis
                var isBlacklisted = await _cache.GetStringAsync(token);
                if (!string.IsNullOrEmpty(isBlacklisted))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token is blacklisted.");
                    return;
                }
            }
        }

        // Если токен не найден в черном списке, передаем управление дальше
        await _next(context);
    }
}
