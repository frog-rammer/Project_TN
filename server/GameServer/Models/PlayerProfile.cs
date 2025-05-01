using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    [Table("player_profiles")]
    public class PlayerProfile
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        [StringLength(50)]
        public string Nickname { get; set; } = string.Empty;
        
        public int Level { get; set; } = 1;
        
        public int Experience { get; set; } = 0;
        
        public int Rank { get; set; } = 0;
        
        public int Currency { get; set; } = 0;  // 게임 내 기본 통화
        
        public int PremiumCurrency { get; set; } = 0;  // 프리미엄 통화
        
        [StringLength(255)]
        public string AvatarUrl { get; set; } = string.Empty;
        
        // 탐색 속성
        public virtual User? User { get; set; }
    }
} 