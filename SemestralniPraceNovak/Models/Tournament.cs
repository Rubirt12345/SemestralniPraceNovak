using System;
using System.Collections.Generic;

namespace SemestralniPraceNovak.Models
{
	public enum TournamentStatus
	{
		Planning,
		GroupPhase,
		EliminationPhase,
		Completed
	}

	public class Tournament
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int SportId { get; set; }
		public Sport Sport { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public TournamentStatus Status { get; set; } = TournamentStatus.Planning;

		public ICollection<Round> Rounds { get; set; } = new List<Round>();
		public ICollection<TournamentParticipant> Participants { get; set; } = new List<TournamentParticipant>();
	}
}