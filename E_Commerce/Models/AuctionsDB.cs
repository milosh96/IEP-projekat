namespace E_Commerce.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AuctionsDB : DbContext
    {
        public AuctionsDB()
            : base("name=AuctionsDB1")
        {
        }

        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<auction> auction { get; set; }
        public virtual DbSet<bid> bid { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<tokenOrder> tokenOrder { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserClaim> UserClaim { get; set; }
        public virtual DbSet<UserLogin> UserLogin { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<auction>()
                .Property(e => e.title)
                .IsFixedLength();

            modelBuilder.Entity<auction>()
                .Property(e => e.status)
                .IsFixedLength();
      /*      
            modelBuilder.Entity<auction>()
                .HasMany(e => e.bid)
                .WithOptional(e => e.auction)
                .HasForeignKey(e => e.idAuction);
*/
            modelBuilder.Entity<auction>()
                .HasMany(e => e.User)
                .WithMany(e => e.auction)
                .Map(m => m.ToTable("AuctionWon").MapLeftKey("idAuction").MapRightKey("idUser"));

            modelBuilder.Entity<Role>()
                .HasMany(e => e.User)
                .WithMany(e => e.Role1)
                .Map(m => m.ToTable("UserRole").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<tokenOrder>()
                .Property(e => e.status)
                .IsFixedLength();

            modelBuilder.Entity<User>()
                .HasMany(e => e.bid)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.idUser);

            modelBuilder.Entity<User>()
                .HasMany(e => e.tokenOrder)
                .WithOptional(e => e.User)
                .HasForeignKey(e => e.idUser);
        }
    }
}
