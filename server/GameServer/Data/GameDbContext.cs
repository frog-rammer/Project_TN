using GameServer.Models;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Data
{
    // DbContext는 Hibernate의 SessionFactory와 유사한 역할
    // JPA의 EntityManager를 생성하는 역할도 함께 수행
    public class GameDbContext : DbContext
    {
        // 생성자를 통한 의존성 주입 - Spring에서 생성자 주입 방식과 유사
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
        {
        }
        
        // DbSet<T>는 JPA의 Repository 또는 DAO와 유사한 역할
        // 각 테이블에 대한 CRUD 작업을 수행할 수 있는 인터페이스 제공
        public DbSet<User> Users { get; set; }
        public DbSet<PlayerProfile> PlayerProfiles { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<UserCard> UserCards { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<UserItem> UserItems { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<UserMission> UserMissions { get; set; }
        public DbSet<Battle> Battles { get; set; }
        
        // OnModelCreating은 JPA/Hibernate의 매핑 설정과 유사
        // 엔티티 간의 관계 설정, 키 설정 등 상세 매핑 구성
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User와 PlayerProfile은 1:1 관계 (Hibernate의 @OneToOne과 유사)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<PlayerProfile>(p => p.UserId);
            
            // User와 UserItem은 1:N 관계 (Hibernate의 @OneToMany와 유사)
            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.Items)
                .HasForeignKey(ui => ui.UserId);
            
            // User와 UserCard는 1:N 관계
            modelBuilder.Entity<UserCard>()
                .HasOne(uc => uc.User)
                .WithMany(u => u.Cards)
                .HasForeignKey(uc => uc.UserId);
            
            // Battle 관계 설정 - 복합적인 외래 키 관계
            modelBuilder.Entity<Battle>()
                .HasOne(b => b.Player1)
                .WithMany()
                .HasForeignKey(b => b.Player1Id)
                .OnDelete(DeleteBehavior.Restrict); // 제한 삭제 - Hibernate의 @OnDelete(action = OnDeleteAction.RESTRICT)와 유사
                
            modelBuilder.Entity<Battle>()
                .HasOne(b => b.Player2)
                .WithMany()
                .HasForeignKey(b => b.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Battle>()
                .HasOne(b => b.Winner)
                .WithMany()
                .HasForeignKey(b => b.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 