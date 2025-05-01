using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    [Table("user_items")]
    public class UserItem
    {
        [Key]
        public long Id { get; set; }
        
        public int UserId { get; set; }
        
        public int ItemId { get; set; }
        
        public int Count { get; set; } = 1;
        
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiryAt { get; set; } = null;  // 만료 시간 (없으면 영구)
        
        // 탐색 속성
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }
    }
} 