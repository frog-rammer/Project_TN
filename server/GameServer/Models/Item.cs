using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    [Table("items")]
    public class Item
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
        
        public int Type { get; set; } = 0;  // 아이템 유형 (소모품, 장비 등)
        
        public int Rarity { get; set; } = 0;  // 아이템 희귀도
        
        public int Effect { get; set; } = 0;  // 효과 ID
        
        public int EffectValue { get; set; } = 0;  // 효과 값
        
        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;
        
        // 가상 탐색 속성
        public virtual ICollection<UserItem>? UserItems { get; set; }
    }
} 