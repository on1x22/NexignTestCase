using Microsoft.EntityFrameworkCore;
using RockPaperScissors.DAL.ContextModels;

namespace RockPaperScissors.DAL.Contexts
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Round> Rounds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                        .HasOne(p => p.PlayerOne)
                        .WithMany(p => p.PlayerOneGames)
                        .HasForeignKey(p => p.PlayerOneId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Game>()
                        .HasOne(p => p.PlayerTwo)
                        .WithMany(p => p.PlayerTwoGames)
                        .HasForeignKey(p => p.PlayerTwoId)
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
