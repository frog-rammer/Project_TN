using GameServer.Data;
using GameServer.Models;
using GameServer.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameServer.Controllers
{
    // 카드 관련 REST API 컨트롤러
    // Spring Boot의 @RestController와 유사
    [ApiController]
    // API 경로 설정 - Spring의 @RequestMapping("/api/card")과 유사
    [Route("api/[controller]")]
    public class CardController : ControllerBase
    {
        private readonly GameDbContext _dbContext;
        
        // 생성자 주입 - Spring의 생성자 DI 패턴과 동일
        public CardController(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        // 모든 카드 목록 조회 API 엔드포인트
        // Spring의 @GetMapping("/list")과 유사
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<List<Card>>>> GetCardList()
        {
            // 모든 카드 조회 - JPA의 findAll() 메서드와 유사
            var cards = await _dbContext.Cards.ToListAsync();
            return Ok(ApiResponse<List<Card>>.CreateSuccess(cards));
        }
        
        // 사용자의 카드 목록 조회 API 엔드포인트
        // Spring의 @GetMapping("/user")과 유사
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse<List<UserCardDto>>>> GetUserCards()
        {
            // 미들웨어에서 추가한 사용자 ID 가져오기 - Spring Security의 Authentication 객체 활용과 유사
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || !(userIdObj is int userId))
            {
                return Unauthorized(ApiResponse<List<UserCardDto>>.CreateError("인증이 필요합니다."));
            }
            
            // 사용자 카드 조회 - JPA의 복잡한 조인 쿼리와 유사
            var userCards = await _dbContext.UserCards
                .Include(uc => uc.Card)  // Eager loading - JPA의 join fetch와 유사
                .Where(uc => uc.UserId == userId)  // 조건절 - JPA Specification 또는 Query Method와 유사
                .Select(uc => new UserCardDto  // 프로젝션 - JPA의 DTO 프로젝션과 유사
                {
                    Id = uc.Id,
                    CardId = uc.CardId,
                    Name = uc.Card!.Name,
                    Description = uc.Card.Description,
                    Rarity = uc.Card.Rarity,
                    Type = uc.Card.Type,
                    Power = uc.Card.Power,
                    Defense = uc.Card.Defense,
                    Count = uc.Count,
                    Level = uc.Level,
                    IsInDeck = uc.IsInDeck,
                    AcquiredAt = uc.AcquiredAt,
                    ImageUrl = uc.Card.ImageUrl
                })
                .ToListAsync();
                
            return Ok(ApiResponse<List<UserCardDto>>.CreateSuccess(userCards));
        }
        
        // 덱 업데이트 API 엔드포인트
        // Spring의 @PostMapping("/deck/update")과 유사
        [HttpPost("deck/update")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateDeck([FromBody] UpdateDeckRequest request)
        {
            // 미들웨어에서 추가한 사용자 ID 가져오기
            if (!HttpContext.Items.TryGetValue("UserId", out var userIdObj) || !(userIdObj is int userId))
            {
                return Unauthorized(ApiResponse<bool>.CreateError("인증이 필요합니다."));
            }
            
            // 모든 카드를 덱에서 제외 - JPA에서는 보통 벌크 업데이트로 처리
            var userCards = await _dbContext.UserCards
                .Where(uc => uc.UserId == userId)
                .ToListAsync();
                
            foreach (var userCard in userCards)
            {
                userCard.IsInDeck = false;
            }
            
            // 선택한 카드를 덱에 추가 - JPA에서 IN 절을 사용한 쿼리와 유사
            var deckCards = await _dbContext.UserCards
                .Where(uc => uc.UserId == userId && request.CardIds.Contains(uc.Id))
                .ToListAsync();
                
            if (deckCards.Count != request.CardIds.Count)
            {
                return BadRequest(ApiResponse<bool>.CreateError("잘못된 카드 ID가 포함되어 있습니다."));
            }
            
            foreach (var deckCard in deckCards)
            {
                deckCard.IsInDeck = true;
            }
            
            // 변경사항 저장 - JPA의 transaction commit과 유사
            await _dbContext.SaveChangesAsync();
            
            return Ok(ApiResponse<bool>.CreateSuccess(true, "덱이 업데이트되었습니다."));
        }
    }
    
    // 사용자 카드 DTO 클래스 - Spring의 DTO(Data Transfer Object) 패턴과 동일
    public class UserCardDto
    {
        public long Id { get; set; }
        public int CardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Rarity { get; set; }
        public int Type { get; set; }
        public int Power { get; set; }
        public int Defense { get; set; }
        public int Count { get; set; }
        public int Level { get; set; }
        public bool IsInDeck { get; set; }
        public System.DateTime AcquiredAt { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
    
    // 덱 업데이트 요청 DTO 클래스
    public class UpdateDeckRequest
    {
        public List<long> CardIds { get; set; } = new List<long>();
    }
} 