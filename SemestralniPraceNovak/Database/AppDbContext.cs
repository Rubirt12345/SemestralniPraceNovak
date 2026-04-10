using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Models;

namespace SemestralniPraceNovak.Database
{
	public class AppDbContext : DbContext
	{
		public DbSet<Sport> Sports { get; set; }
		public DbSet<Player> Players { get; set; }
		public DbSet<Team> Teams { get; set; }
		public DbSet<Tournament> Tournaments { get; set; }
		public DbSet<Round> Rounds { get; set; }
		public DbSet<Match> Matches { get; set; }
		public DbSet<TournamentParticipant> TournamentParticipants { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			var dbPath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
				"SemestralniPraceNovak",
				"tournament.db"
			);

			var directory = System.IO.Path.GetDirectoryName(dbPath);
			if (!System.IO.Directory.Exists(directory))
			{
				System.IO.Directory.CreateDirectory(directory);
			}

			optionsBuilder.UseSqlite($"Data Source={dbPath}");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			
			modelBuilder.Entity<Tournament>()
				.HasOne(t => t.Sport)
				.WithMany(s => s.Tournaments)
				.HasForeignKey(t => t.SportId)
				.OnDelete(DeleteBehavior.Restrict);

			
			modelBuilder.Entity<Round>()
				.HasOne(r => r.Tournament)
				.WithMany(t => t.Rounds)
				.HasForeignKey(r => r.TournamentId)
				.OnDelete(DeleteBehavior.Cascade);

			
			modelBuilder.Entity<Match>()
				.HasOne(m => m.Round)
				.WithMany(r => r.Matches)
				.HasForeignKey(m => m.RoundId)
				.OnDelete(DeleteBehavior.Cascade);

			
			modelBuilder.Entity<Match>()
				.HasOne(m => m.Player1)
				.WithMany(p => p.MatchesAsPlayer1)
				.HasForeignKey(m => m.Player1Id)
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<Match>()
				.HasOne(m => m.Player2)
				.WithMany(p => p.MatchesAsPlayer2)
				.HasForeignKey(m => m.Player2Id)
				.OnDelete(DeleteBehavior.SetNull);

			
			modelBuilder.Entity<Match>()
				.HasOne(m => m.Team1)
				.WithMany(t => t.MatchesAsTeam1)
				.HasForeignKey(m => m.Team1Id)
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<Match>()
				.HasOne(m => m.Team2)
				.WithMany(t => t.MatchesAsTeam2)
				.HasForeignKey(m => m.Team2Id)
				.OnDelete(DeleteBehavior.SetNull);

			
			modelBuilder.Entity<TournamentParticipant>()
				.HasOne(tp => tp.Tournament)
				.WithMany(t => t.Participants)
				.HasForeignKey(tp => tp.TournamentId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<TournamentParticipant>()
				.HasOne(tp => tp.Player)
				.WithMany(p => p.Tournaments)
				.HasForeignKey(tp => tp.PlayerId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<TournamentParticipant>()
				.HasOne(tp => tp.Team)
				.WithMany(t => t.Tournaments)
				.HasForeignKey(tp => tp.TeamId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}