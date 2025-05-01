using GameServer.Data;
using GameServer.Models;
using GameServer.Models.Responses;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameServer.Controllers
{
    // 전투 관련 REST API 컨트롤러
    // Spring Boot의 @RestController와 유사
    [ApiController]
    // API 경로 설정 - Spring의 @RequestMapping("/api/battle")과 유사
    [Route("api/[controller]")]
    public class BattleController : ControllerBase
    {
        private readonly GameDbContext _dbContext;
        private readonly RedisService _redisService;
        
        // 생성자 주입 - Spring의 생성자 DI 패턴과 동일
        public BattleController(GameDbContext dbContext, RedisService redisService)
        {
            _dbContext = dbContext;
            _redisService = redisService;
        }
        
        // 전투 시작 API 엔드포인트
        // Spring의 @PostMapping("/start")과 유사
        [HttpPost("start")]
        public async Task<ActionResult<ApiResponse<BattleStartResponse>>> StartBattle([FromBody] BattleStartRequest request)
        {
            // 미들웨어에서 추가한 사용자 ID 가져오기 - Spring Security의 Authentication 객체와 유사
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || !(userIdObj is int userId))
            {
                return Unauthorized(ApiResponse<BattleStartResponse>.CreateError("인증이 필요합니다."));
            }
            
            // 사용자 덱 카드 가져오기 - JPA Repository의 findAll() 메서드와 유사
            var deckCards = await _dbContext.UserCards
                .Include(uc => uc.Card)  // Eager loading - JPA의 join fetch 또는 @EntityGraph와 유사
                .Where(uc => uc.UserId == userId && uc.IsInDeck)  // 조건절 - JPA Specification 또는 Query Method와 유사
                .ToListAsync();
                
            if (deckCards.Count == 0)
            {
                return BadRequest(ApiResponse<BattleStartResponse>.CreateError("덱에 카드가 없습니다."));
            }
            
            // 상대방 검색 (PVP 또는 PVE)
            if (request.IsPVP)
            {
                // PVP 매칭 로직 구현
                // Redis를 사용한 대기열 구현 - Spring에서 메시징 또는 캐시 서비스를 사용하는 것과 유사
                var waitingUserJson = await _redisService.GetAsync<string>("pvp:waiting");
                
                if (string.IsNullOrEmpty(waitingUserJson) || waitingUserJson.Contains($"\"UserId\":{userId}"))
                {
                    // 매칭 대기열에 추가 - Spring의 캐시 또는 메시지 큐에 데이터 추가와 유사
                    var waitingInfo = new
                    {
                        UserId = userId,
                        Timestamp = DateTime.UtcNow
                    };
                    
                    await _redisService.SetAsync("pvp:waiting", JsonConvert.SerializeObject(waitingInfo), TimeSpan.FromMinutes(5));
                    
                    // 대기 상태 응답 반환
                    return Ok(ApiResponse<BattleStartResponse>.CreateSuccess(new BattleStartResponse
                    {
                        BattleId = 0,
                        Status = "waiting",
                        Message = "상대방을 기다리는 중..."
                    }));
                }
                else
                {
                    // 대기 중인 상대방 있음 - 매칭 성공
                    var waitingUser = JsonConvert.DeserializeObject<dynamic>(waitingUserJson)!;
                    int opponentId = (int)waitingUser.UserId;
                    
                    // 대기열에서 제거
                    await _redisService.RemoveAsync("pvp:waiting");
                    
                    // 전투 생성 - JPA 엔티티 생성 및 저장과 유사
                    var battle = new Battle
                    {
                        Player1Id = userId,
                        Player2Id = opponentId,
                        IsPVP = true,
                        RuleNumber = request.RuleNumber,
                        StartedAt = DateTime.UtcNow
                    };
                    
                    // 데이터베이스에 저장 - JPA repository.save() 메서드와 유사
                    await _dbContext.Battles.AddAsync(battle);
                    await _dbContext.SaveChangesAsync();
                    
                    // 매칭 성공 응답 반환
                    return Ok(ApiResponse<BattleStartResponse>.CreateSuccess(new BattleStartResponse
                    {
                        BattleId = battle.Id,
                        Status = "matched",
                        Message = "상대방을 찾았습니다.",
                        OpponentId = opponentId
                    }));
                }
            }
            else
            {
                // PVE 로직 (AI 상대) - 비동기 처리
                var battle = new Battle
                {
                    Player1Id = userId,
                    Player2Id = null,  // PVE는 상대방 없음
                    IsPVP = false,
                    RuleNumber = request.RuleNumber,
                    StartedAt = DateTime.UtcNow
                };
                
                await _dbContext.Battles.AddAsync(battle);
                await _dbContext.SaveChangesAsync();
                
                return Ok(ApiResponse<BattleStartResponse>.CreateSuccess(new BattleStartResponse
                {
                    BattleId = battle.Id,
                    Status = "started",
                    Message = "AI 상대와 게임이 시작되었습니다."
                }));
            }
        }
        
        // 전투 종료 API 엔드포인트
        // Spring의 @PostMapping("/end")과 유사
        [HttpPost("end")]
        public async Task<ActionResult<ApiResponse<BattleEndResponse>>> EndBattle([FromBody] BattleEndRequest request)
        {
            // 인증된 사용자인지 확인
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || !(userIdObj is int userId))
            {
                return Unauthorized(ApiResponse<BattleEndResponse>.CreateError("인증이 필요합니다."));
            }
            
            // 전투 정보 조회 - JPA repository.findById() 메서드와 유사
            var battle = await _dbContext.Battles.FindAsync(request.BattleId);
            if (battle == null)
            {
                return BadRequest(ApiResponse<BattleEndResponse>.CreateError("존재하지 않는 전투입니다."));
            }
            
            // 현재 사용자가 해당 전투의 참가자인지 확인
            if (battle.Player1Id != userId && (battle.Player2Id != userId && battle.Player2Id != null))
            {
                return BadRequest(ApiResponse<BattleEndResponse>.CreateError("참가하지 않은 전투입니다."));
            }
            
            // 전투 종료 처리 - JPA 엔티티 속성 변경
            battle.EndedAt = DateTime.UtcNow;
            battle.Duration = (int)(battle.EndedAt.Value - battle.StartedAt).TotalSeconds;
            battle.WinnerId = request.WinnerId;
            battle.BattleData = request.BattleData;
            
            // 사용자 전적 업데이트 - 트랜잭션 내 여러 엔티티 수정과 유사
            var player = await _dbContext.Users.FindAsync(userId);
            if (player != null)
            {
                player.TotalBattleCount++;
                
                if (request.WinnerId == userId)
                {
                    player.WinCount++;
                }
                else
                {
                    player.LoseCount++;
                }
            }
            
            // 변경사항 데이터베이스에 저장 - JPA의 transaction commit과 유사
            await _dbContext.SaveChangesAsync();
            
            // 보상 지급 - 서비스 계층에서 처리할 수도 있음
            var reward = new RewardData
            {
                Experience = 100,
                Currency = 50,
                Items = new List<RewardItem>()
            };
            
            // 응답 반환
            return Ok(ApiResponse<BattleEndResponse>.CreateSuccess(new BattleEndResponse
            {
                BattleId = battle.Id,
                Duration = battle.Duration,
                WinnerId = battle.WinnerId,
                Reward = reward
            }));
        }
    }
    
    // DTO 클래스들 - Spring의 DTO(Data Transfer Object) 패턴과 동일
    
    // 전투 시작 요청 DTO
    public class BattleStartRequest
    {
        public bool IsPVP { get; set; } = true;
        public int RuleNumber { get; set; } = 0;
    }
    
    // 전투 시작 응답 DTO
    public class BattleStartResponse
    {
        public long BattleId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? OpponentId { get; set; }
    }
    
    // 전투 종료 요청 DTO
    public class BattleEndRequest
    {
        public long BattleId { get; set; }
        public int WinnerId { get; set; }
        public string BattleData { get; set; } = string.Empty;
    }
    
    // 전투 종료 응답 DTO
    public class BattleEndResponse
    {
        public long BattleId { get; set; }
        public int Duration { get; set; }
        public int WinnerId { get; set; }
        public RewardData Reward { get; set; } = new RewardData();
    }
    
    // 보상 데이터 DTO
    public class RewardData
    {
        public int Experience { get; set; }
        public int Currency { get; set; }
        public List<RewardItem> Items { get; set; } = new List<RewardItem>();
    }
    
    // 보상 아이템 DTO
    public class RewardItem
    {
        public int ItemId { get; set; }
        public int Count { get; set; }
    }
}