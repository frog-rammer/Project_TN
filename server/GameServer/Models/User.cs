using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameServer.Models
{
    // 사용자 엔티티 클래스
    // JPA/Hibernate의 @Entity 어노테이션과 유사
    [Table("users")] // JPA의 @Table(name = "users")와 유사
    public class User
    {
        // 기본 키 정의 - JPA의 @Id와 유사
        [Key]
        // 자동 생성 값 - JPA의 @GeneratedValue(strategy = GenerationType.IDENTITY)와 유사
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        // 필수 필드 - JPA의 @Column(nullable = false)와 유사
        [Required]
        // 문자열 길이 제한 - JPA의 @Column(length = 50)과 유사
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        // 생성 시간 - JPA의 @CreationTimestamp 또는 @Column(updatable = false)와 유사
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // 마지막 로그인 시간 - JPA의 @UpdateTimestamp와 비슷한 용도
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        
        // 사용자 게임 통계 필드
        public int WinCount { get; set; } = 0;
        public int LoseCount { get; set; } = 0;
        public int TotalBattleCount { get; set; } = 0;
        
        // 탐색 속성들 - JPA의 관계 매핑과 유사
        
        // 아이템과의 1:N 관계 - JPA의 @OneToMany와 유사
        public virtual ICollection<UserItem>? Items { get; set; }
        
        // 카드와의 1:N 관계
        public virtual ICollection<UserCard>? Cards { get; set; }
        
        // 프로필과의 1:1 관계 - JPA의 @OneToOne과 유사
        public virtual PlayerProfile? Profile { get; set; }
    }
} 