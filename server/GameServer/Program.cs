using GameServer.Data;
using GameServer.Services;
using GameServer.Utils;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

// .NET의 엔트리 포인트 (Java의 main 메소드와 비슷)
// ASP.NET Core에서는 Program.cs가 스프링 부트의 Application.java와 유사한 역할
var builder = WebApplication.CreateBuilder(args);

// 데이터베이스 서비스 등록
// Spring Boot의 @Bean 설정과 유사, MariaDB 연결 설정
// EntityFrameworkCore는 JPA/Hibernate와 비슷한 ORM
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Redis 서비스 등록
// 싱글톤 패턴으로 Redis 연결을 등록 (Spring의 @Bean + @Singleton과 유사)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")!)
);
builder.Services.AddSingleton<RedisService>();

// 서비스 등록 - Spring의 @Service 클래스들을 DI 컨테이너에 등록하는 것과 유사
// 스코프드는 HTTP 요청마다 새로운 인스턴스 생성 (Spring의 request scope와 비슷)
builder.Services.AddScoped<AuthService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// MVC 컨트롤러 등록 - Spring Boot의 @EnableWebMvc와 유사
builder.Services.AddControllers();
// Swagger/OpenAPI 문서 생성 설정 - Spring의 SpringFox/Swagger와 유사
builder.Services.AddOpenApi();

// 애플리케이션 빌드 - 여기서부터 미들웨어 파이프라인 구성 (Spring의 Filter 체인과 유사)
var app = builder.Build();

// 개발 환경에서만 활성화 - Spring의 프로필 설정과 유사
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Swagger UI 엔드포인트 추가
    app.UseDeveloperExceptionPage(); // 개발 중 상세 오류 보기
}

// HTTPS 리디렉션 - Spring Security의 requiresChannel().requiresSecure()와 유사
app.UseHttpsRedirection();
// 라우팅 미들웨어 - Spring의 RequestMappingHandlerMapping과 유사
app.UseRouting();

// 사용자 인증 미들웨어 등록 - Spring Security의 Filter 등록과 유사
app.UseAuthMiddleware();

// 컨트롤러 엔드포인트 매핑 - Spring의 @RequestMapping 등록과 유사
app.MapControllers();

//  API 엔드포인트 정의 - 나중에 삭제 예정
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// 애플리케이션 실행 - Spring Boot의 SpringApplication.run()과 유사
app.Run();

// 불변 데이터 객체를 간결하게 정의
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
