using GameServer.Models.Requests;
using GameServer.Models.Responses;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GameServer.Controllers
{
    // 인증 관련 REST API 컨트롤러
    // Spring Boot의 @RestController와 유사한 역할
    [ApiController]
    // API 경로 설정 - Spring의 @RequestMapping("/api/auth")와 유사
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        
        // 생성자 주입 - Spring의 생성자 DI 패턴과 동일
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        
        // 로그인 API 엔드포인트
        // Spring의 @PostMapping("/login")과 유사
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            // 서비스 계층 호출 - Spring 서비스 계층 패턴과 동일
            var response = await _authService.Login(request);
            
            // 로그인 실패 시 400 Bad Request 반환
            // Spring의 ResponseEntity.badRequest().body(...)와 유사
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            // 로그인 성공 시 200 OK 반환
            // Spring의 ResponseEntity.ok(...)와 유사
            return Ok(response);
        }
        
        // 회원 가입 API 엔드포인트
        // Spring의 @PostMapping("/register")과 유사
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.Register(request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        
        // 로그아웃 API 엔드포인트
        // Spring의 @PostMapping("/logout")과 유사
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] LogoutRequest request)
        {
            var result = await _authService.Logout(request.UserId, request.SessionId);
            
            return Ok(ApiResponse<bool>.CreateSuccess(result));
        }
    }
    
    // 로그아웃 요청 DTO 클래스
    // Spring의 @RequestBody 클래스와 유사
    public class LogoutRequest
    {
        public int UserId { get; set; }
        public string SessionId { get; set; } = string.Empty;
    }
} 