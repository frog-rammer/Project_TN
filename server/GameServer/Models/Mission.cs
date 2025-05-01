using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    [Table("missions")]
    public class Mission
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
        
        public int Type { get; set; } = 0;  // 미션 유형
        
        public int TargetValue { get; set; } = 0;  // 미션 목표 값
        
        public int RewardType { get; set; } = 0;  // 보상 유형
        
        public int RewardId { get; set; } = 0;  // 보상 ID
        
        public int RewardAmount { get; set; } = 0;  // 보상 수량
        
        // 가상 탐색 속성
        public virtual ICollection<UserMission>? UserMissions { get; set; }
    }
} 