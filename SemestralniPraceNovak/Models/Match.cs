using CommunityToolkit.Mvvm.ComponentModel;

namespace SemestralniPraceNovak.Models
{
    public partial class Match : ObservableObject
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public Round Round { get; set; }

        public int? Player1Id { get; set; }
        public Player Player1 { get; set; }

        public int? Player2Id { get; set; }
        public Player Player2 { get; set; }

        [ObservableProperty]
        private int? _team1Id;

        [ObservableProperty]
        private Team _team1;

        [ObservableProperty]
        private int? _team2Id;

        [ObservableProperty]
        private Team _team2;

        public int? Participant1Id { get; set; }
        public TournamentParticipant Participant1 { get; set; }

        public int? Participant2Id { get; set; }
        public TournamentParticipant Participant2 { get; set; }

        [ObservableProperty]
        private int? _score1;

        [ObservableProperty]
        private int? _score2;

        [ObservableProperty]
        private int? _winnerId;

        [ObservableProperty]
        private bool _isCompleted;
    }
}