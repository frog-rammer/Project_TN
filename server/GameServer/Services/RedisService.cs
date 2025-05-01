using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace GameServer.Services
{
    // Redis 캐시 서비스
    // Spring의 RedisTemplate이나 Spring Data Redis와 유사한 역할
    public class RedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        
        // 생성자 주입 - Spring의 생성자 DI와 유사
        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }
        
        // 데이터를 캐시에 저장
        // Spring Data Redis의 redisTemplate.opsForValue().set()과 유사
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            // Newtonsoft.Json은 Java의 Jackson이나 Gson과 유사
            string serializedValue = JsonConvert.SerializeObject(value);
            await _db.StringSetAsync(key, serializedValue, expiry);
        }
        
        // 캐시에서 데이터 조회
        // Spring Data Redis의 redisTemplate.opsForValue().get()과 유사
        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }
            
            return JsonConvert.DeserializeObject<T>(value!);
        }
        
        // 캐시에서 데이터 삭제
        // Spring Data Redis의 redisTemplate.delete()와 유사
        public async Task<bool> RemoveAsync(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }
        
        // 키 존재 여부 확인
        // Spring Data Redis의 redisTemplate.hasKey()와 유사
        public async Task<bool> ExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }
        
        // 해시 테이블에 데이터 저장
        // Spring Data Redis의 redisTemplate.opsForHash().put()과 유사
        public async Task HashSetAsync(string key, string field, string value)
        {
            await _db.HashSetAsync(key, field, value);
        }
        
        // 해시 테이블에서 데이터 조회
        // Spring Data Redis의 redisTemplate.opsForHash().get()과 유사
        public async Task<string?> HashGetAsync(string key, string field)
        {
            var value = await _db.HashGetAsync(key, field);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        
        // 게임 세션 관리에 사용되는 방법들
        // 사용자 세션 정보 추가 - Redis를 통한 세션 관리
        public async Task AddUserToSessionAsync(int userId, string sessionId)
        {
            // 해시에 사용자 세션 정보 저장
            await _db.HashSetAsync("user:sessions", userId.ToString(), sessionId);
            // 세션 ID에 TTL(만료 시간) 설정 - Spring의 @Cacheable(timeToLive)와 유사
            await _db.StringSetAsync($"session:{sessionId}", userId.ToString(), TimeSpan.FromHours(1));
        }
        
        // 사용자의 현재 세션 조회
        public async Task<string?> GetUserSessionAsync(int userId)
        {
            var sessionId = await _db.HashGetAsync("user:sessions", userId.ToString());
            return sessionId.IsNullOrEmpty ? null : sessionId.ToString();
        }
        
        // 사용자 세션 삭제 (로그아웃)
        public async Task RemoveUserSessionAsync(int userId, string sessionId)
        {
            await _db.HashDeleteAsync("user:sessions", userId.ToString());
            await _db.KeyDeleteAsync($"session:{sessionId}");
        }
    }
} 