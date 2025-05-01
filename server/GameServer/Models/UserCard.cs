using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    [Table("user_cards")]
    public class UserCard
    {
        [Key]
        public long Id { get; set; }
        
        public int UserId { get; set; }
        
        public int CardId { get; set; }
        
        public int Count { get; set; } = 1;
        
        public int Level { get; set; } = 1;
        
        public bool IsInDeck { get; set; } = false;
        
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
        
        // 탐색 속성
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("CardId")]
        public virtual Card? Card { get; set; }
    }
} 