namespace SemestralniPraceNovak.Models
{
	public class TournamentParticipant
	{
		public int Id { get; set; }
		public int TournamentId { get; set; }
		public Tournament Tournament { get; set; }

		public int? PlayerId { get; set; }
		public Player Player { get; set; }

		public int? TeamId { get; set; }
		public Team Team { get; set; }

		public int Points { get; set; }
		public int Wins { get; set; }
		public int Losses { get; set; }
		public int Draws { get; set; }
	}
}