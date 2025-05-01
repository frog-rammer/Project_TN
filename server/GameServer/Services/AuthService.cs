using GameServer.Data;
using GameServer.Models;
using GameServer.Models.Requests;
using GameServer.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    // 인증 서비스 클래스
    // Spring Security의 AuthenticationManager 및 UserDetailsService와 유사한 역할
    public class AuthService
    {
        private readonly GameDbContext _dbContext;
        private readonly RedisService _redisService;
        
        // 생성자 주입 - Spring의 생성자 DI 패턴과 동일
        public AuthService(GameDbContext dbContext, RedisService redisService)
        {
            _dbContext = dbContext;
            _redisService = redisService;
        }
        
        // 로그인 처리 메서드
        // Spring Security의 AuthenticationManager.authenticate()와 유사한 역할
        public async Task<ApiResponse<AuthResponse>> Login(LoginRequest request)
        {
            // 사용자 조회 - Spring Data JPA의 findByUsername()과 유사
            var user = await _dbContext.Users
                .Include(u => u.Profile)  // Eager Loading - JPA의 fetch = FetchType.EAGER와 유사
                .FirstOrDefaultAsync(u => u.Username == request.Username);
                
            if (user == null)
            {
                return ApiResponse<AuthResponse>.CreateError("사용자를 찾을 수 없습니다.");
            }
            
            // 비밀번호 검증 - Spring Security의 PasswordEncoder.matches()와 유사
            if (!VerifyPassword(request.Password, user.Password))
            {
                return ApiResponse<AuthResponse>.CreateError("비밀번호가 일치하지 않습니다.");
            }
            
            // 세션 생성 - Spring Session 또는 JWT 토큰 생성과 유사한 과정
            string sessionId = Guid.NewGuid().ToString();
            await _redisService.AddUserToSessionAsync(user.Id, sessionId);
            
            // 마지막 로그인 시간 업데이트
            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();  // JPA의 entityManager.persist()와 유사
            
            // 응답 객체 생성 - Spring의 ResponseEntity.ok()와 유사
            return ApiResponse<AuthResponse>.CreateSuccess(new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                SessionId = sessionId,
                Nickname = user.Profile?.Nickname ?? string.Empty,
                Level = user.Profile?.Level ?? 1,
                Currency = user.Profile?.Currency ?? 0,
                PremiumCurrency = user.Profile?.PremiumCurrency ?? 0
            });
        }
        
        // 회원 가입 처리 메서드
        // Spring Security의 UserDetailsManager.createUser()와 유사
        public async Task<ApiResponse<AuthResponse>> Register(RegisterRequest request)
        {
            // 사용자명 중복 검사 - Spring Data JPA의 existsByUsername()과 유사
            if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                return ApiResponse<AuthResponse>.CreateError("이미 사용 중인 사용자명입니다.");
            }
            
            // 이메일 중복 검사
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                return ApiResponse<AuthResponse>.CreateError("이미 사용 중인 이메일입니다.");
            }
            
            // 사용자 생성 - JPA에서 새 엔티티 생성과 유사
            var user = new User
            {
                Username = request.Username,
                Password = HashPassword(request.Password),  // Spring Security의 passwordEncoder.encode()와 유사
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            
            // 프로필 생성 - 연관 엔티티 생성
            var profile = new PlayerProfile
            {
                Nickname = request.Nickname,
                Level = 1,
                Experience = 0,
                Currency = 1000,  // 초기 지급 재화
                PremiumCurrency = 100  // 초기 지급 프리미엄 재화
            };
            
            // 연관 관계 설정 - JPA의 객체 그래프 설정과 유사
            user.Profile = profile;
            
            // DB에 사용자 정보 저장 - JPA의 entityManager.persist()와 유사
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            
            // 세션 생성
            string sessionId = Guid.NewGuid().ToString();
            await _redisService.AddUserToSessionAsync(user.Id, sessionId);
            
            // 응답 객체 생성
            return ApiResponse<AuthResponse>.CreateSuccess(new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                SessionId = sessionId,
                Nickname = profile.Nickname,
                Level = profile.Level,
                Currency = profile.Currency,
                PremiumCurrency = profile.PremiumCurrency
            });
        }
        
        // 로그아웃 처리 메서드
        // Spring Security의 SecurityContextLogoutHandler.logout()과 유사
        public async Task<bool> Logout(int userId, string sessionId)
        {
            await _redisService.RemoveUserSessionAsync(userId, sessionId);
            return true;
        }
        
        // 비밀번호 해싱 - Spring Security의 PasswordEncoder.encode()와 유사
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        
        // 비밀번호 검증 - Spring Security의 PasswordEncoder.matches()와 유사
        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return HashPassword(inputPassword) == storedPassword;
        }
    }
} 