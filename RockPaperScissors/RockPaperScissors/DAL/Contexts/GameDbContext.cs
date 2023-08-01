using Microsoft.EntityFrameworkCore;
using RockPaperScissors.DAL.ContextModels;
//using System.Data.Entity;
using System.Text.RegularExpressions;

namespace RockPaperScissors.DAL.Contexts
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Round> Rounds { get; set; }


        /*protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // in memory database used for simplicity, change to a real db for production applications
            options.UseInMemoryDatabase("TestGameDb");
        }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                        .HasOne(p => p.PlayerOne)
                        //.HasRequired(m => m.HomeTeam)
                        .WithMany(p => p.PlayerOneGames)
                        .HasForeignKey(p => p.PlayerOneId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Game>()
                        .HasOne(p => p.PlayerTwo)
                        //.HasRequired(m => m.GuestTeam)
                        .WithMany(p => p.PlayerTwoGames)
                        .HasForeignKey(p => p.PlayerTwoId)
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
