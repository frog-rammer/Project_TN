using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    // 카드 엔티티 클래스
    // JPA/Hibernate의 @Entity 어노테이션과 유사한 역할
    [Table("cards")] // JPA의 @Table(name = "cards")와 유사
    public class Card
    {
        // 기본 키 정의 - JPA의 @Id와 유사
        [Key]
        public int Id { get; set; }
        
        // 필수 필드 - JPA의 @Column(nullable = false)와 유사
        [Required]
        // 문자열 길이 제한 - JPA의 @Column(length = 50)과 유사
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        // 문자열 길이 제한
        [StringLength(255)]
        public string Description { get; set; } = string.Empty;
        
        // 카드 희귀도 - JPA에서는 단순한 @Column
        public int Rarity { get; set; } = 0;  // 카드 희귀도
        
        public int Type { get; set; } = 0;    // 카드 타입
        
        public int Power { get; set; } = 0;   // 카드 공격력
        
        public int Defense { get; set; } = 0; // 카드 방어력
        
        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;
        
        // 가상 탐색 속성 - JPA의 @OneToMany(mappedBy = "card")와 유사
        // UserCard 엔티티와의 1:N 관계를 정의
        public virtual ICollection<UserCard>? UserCards { get; set; }
    }
} 