using GameServer.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace GameServer.Utils
{
    // 인증 미들웨어 클래스
    // Spring Security의 Filter 또는 Interceptor와 유사한 역할
    // 모든 요청에 대해 인증 여부를 확인하고 처리
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        
        // 생성자로 다음 미들웨어를 주입 (filter chain과 유사)
        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        // 요청 처리 메서드 - Spring의 Filter.doFilter() 메서드와 유사
        public async Task InvokeAsync(HttpContext context, RedisService redisService)
        {
            // 인증이 필요하지 않은 경로는 통과 - Spring Security의 permitAll()과 유사
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (path.Contains("/api/auth/") || path.Contains("/openapi/")))
            {
                // 다음 미들웨어로 제어 전달 (filter chain의 doFilter 호출과 유사)
                await _next(context);
                return;
            }
            
            // 헤더에서 세션 정보 확인 - Spring의 SecurityContextHolder에서 인증 정보 확인과 유사
            if (!context.Request.Headers.TryGetValue("X-Session-Id", out var sessionIdValues) ||
                !context.Request.Headers.TryGetValue("X-User-Id", out var userIdValues))
            {
                // 인증 정보가 없는 경우 401 응답 - Spring Security의 AuthenticationEntryPoint와 유사
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsJsonAsync(new 
                { 
                    success = false,
                    message = "인증이 필요합니다." 
                });
                return;
            }
            
            var sessionId = sessionIdValues.ToString();
            if (!int.TryParse(userIdValues.ToString(), out int userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    success = false,
                    message = "잘못된 인증 정보입니다." 
                });
                return;
            }
            
            // Redis에서 세션 확인 - Spring Session의 세션 검증과 유사
            var storedSessionId = await redisService.GetUserSessionAsync(userId);
            if (string.IsNullOrEmpty(storedSessionId) || storedSessionId != sessionId)
            {
                // 세션이 만료된 경우 401 응답
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new 
                { 
                    success = false,
                    message = "세션이 만료되었습니다." 
                });
                return;
            }
            
            // 세션 만료 시간 갱신 - Spring Session의 세션 갱신과 유사
            await redisService.AddUserToSessionAsync(userId, sessionId); // TTL 갱신
            
            // 요청 컨텍스트에 사용자 ID 추가 - Spring Security의 SecurityContextHolder.getContext().setAuthentication()과 유사
            context.Items["UserId"] = userId;
            context.Items["SessionId"] = sessionId;
            
            // 다음 미들웨어로 제어 전달
            await _next(context);
        }
    }
    
    // 미들웨어 확장 메소드
    // Spring Security의 configure(HttpSecurity http) 메소드와 유사한 역할
    public static class AuthMiddlewareExtensions
    {
        // IApplicationBuilder에 미들웨어를 추가하는 확장 메소드
        // Spring Security의 addFilterBefore()나 addFilterAfter()와 유사
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
} 