using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    [Table("battles")]
    public class Battle
    {
        [Key]
        public long Id { get; set; }
        
        public int Player1Id { get; set; }
        
        public int? Player2Id { get; set; }  // PVE의 경우 NULL
        
        public bool IsPVP { get; set; } = true;
        
        public int WinnerId { get; set; }
        
        public int RuleNumber { get; set; } = 0;  // 전투 규칙 번호
        
        [StringLength(255)]
        public string BattleData { get; set; } = string.Empty;  // 전투 상세 데이터 (JSON)
        
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndedAt { get; set; }
        
        public int Duration { get; set; } = 0;  // 전투 시간 (초)
        
        // 탐색 속성
        [ForeignKey("Player1Id")]
        public virtual User? Player1 { get; set; }
        
        [ForeignKey("Player2Id")]
        public virtual User? Player2 { get; set; }
        
        [ForeignKey("WinnerId")]
        public virtual User? Winner { get; set; }
    }
} 