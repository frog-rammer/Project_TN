using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    [Table("user_missions")]
    public class UserMission
    {
        [Key]
        public long Id { get; set; }
        
        public int UserId { get; set; }
        
        public int MissionId { get; set; }
        
        public int Progress { get; set; } = 0;  // 미션 진행도
        
        public bool IsCompleted { get; set; } = false;  // 미션 완료 여부
        
        public bool IsRewardClaimed { get; set; } = false;  // 보상 수령 여부
        
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; } = null;
        
        // 탐색 속성
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("MissionId")]
        public virtual Mission? Mission { get; set; }
    }
} 