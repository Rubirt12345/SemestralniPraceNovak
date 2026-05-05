using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SemestralniPraceNovak.Models;
using SemestralniPraceNovak.Database;

namespace SemestralniPraceNovak.ViewModels
{
    public class BracketViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Round> Rounds { get; } = new ObservableCollection<Round>();
        public ObservableCollection<Team> Teams { get; } = new ObservableCollection<Team>();
        public ObservableCollection<Team> DbTeams { get; } = new ObservableCollection<Team>();

        private Team? _selectedDbTeam;
        public Team? SelectedDbTeam
        {
            get => _selectedDbTeam;
            set
            {
                if (_selectedDbTeam == value) return;
                _selectedDbTeam = value;
                OnPropertyChanged();
                (AddTeamCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private Team? _selectedTeam;
        public Team? SelectedTeam
        {
            get => _selectedTeam;
            set { if (_selectedTeam == value) return; _selectedTeam = value; OnPropertyChanged(); }
        }

        private bool _isPavouk;
        public bool IsPavouk
        {
            get => _isPavouk;
            set { if (_isPavouk == value) return; _isPavouk = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { if (_statusMessage == value) return; _statusMessage = value; OnPropertyChanged(); }
        }
        public ICommand AddTeamCommand { get; }
        public ICommand RemoveTeamCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand DrawBracketCommand { get; }
        public ICommand AdvanceWinnerCommand { get; }

        public BracketViewModel()
        {
            AddTeamCommand = new RelayCommand(_ => AddTeam(), _ => SelectedDbTeam != null);
            RemoveTeamCommand = new RelayCommand(t => RemoveTeam(t as Team), t => t is Team);
            ClearCommand = new RelayCommand(_ => Clear());
            DrawBracketCommand = new RelayCommand(_ => DrawBracket(), _ => Teams.Count >= 2);
            AdvanceWinnerCommand = new RelayCommand(m => AdvanceWinner(m as Match));
            Teams.CollectionChanged += (_, _) =>
                (DrawBracketCommand as RelayCommand)?.RaiseCanExecuteChanged();

            LoadTeamsFromDatabase();
        }

        public void LoadTeamsFromDatabase()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var teams = context.Teams.ToList();
                    DbTeams.Clear();
                    foreach (var team in teams)
                    {
                        DbTeams.Add(team);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Chyba při načítání týmů z databáze: " + ex.Message;
            }
        }

        private void AddTeam()
        {
            if (SelectedDbTeam == null) return;
            if (Teams.Any(t => t.Id == SelectedDbTeam.Id))
            {
                StatusMessage = $"Tým „{SelectedDbTeam.Name}“ už v pavoukovi je.";
                return;
            }

            Teams.Add(SelectedDbTeam);
            StatusMessage = $"Přidán tým „{SelectedDbTeam.Name}“. Celkem: {Teams.Count}.";
        }

        private void RemoveTeam(Team? team)
        {
            if (team == null) return;
            Teams.Remove(team);
            StatusMessage = $"Odebrán tým. Celkem: {Teams.Count}.";
        }

        private void Clear()
        {
            Teams.Clear();
            Rounds.Clear();
            IsPavouk = false;
            StatusMessage = "Vše vyčištěno.";
        }

        public void DrawBracket()
        {
            if (Teams.Count < 2)
            {
                StatusMessage = "Pro losování jsou potřeba alespoň 2 týmy.";
                return;
            }

            int n = 1;
            while (n < Teams.Count) n *= 2;

            var pool = new List<Team>(Teams);
            var rng = new Random();
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            while (pool.Count < n) pool.Add(new Team { Name = "(BYE)" });

            Rounds.Clear();

            var firstRound = new Round { RoundNumber = 1, Name = RoundName(n) };
            for (int i = 0; i < n; i += 2)
            {
                firstRound.Matches.Add(new Match
                {
                    Team1 = pool[i],
                    Team2 = pool[i + 1]
                });
            }
            Rounds.Add(firstRound);

            int remaining = n / 2;
            int roundNum = 2;
            while (remaining >= 2)
            {
                var r = new Round { RoundNumber = roundNum, Name = RoundName(remaining) };
                for (int i = 0; i < remaining; i += 2)
                    r.Matches.Add(new Match());

                Rounds.Add(r);
                remaining /= 2;
                roundNum++;
            }

            IsPavouk = true;
            StatusMessage = $"Vylosováno: {Teams.Count} týmů, {n} pozic, {Rounds.Count} kol.";
        }
        private void AdvanceWinner(Match? currentMatch)
        {
            if (currentMatch == null || currentMatch.Team1 == null || currentMatch.Team2 == null)
            {
                StatusMessage = "Tento zápas nemá definované oba týmy.";
                return;
            }

            if (currentMatch.Score1 == null || currentMatch.Score2 == null)
            {
                StatusMessage = "Před potvrzením musíte zadat skóre u obou týmů.";
                return;
            }
            Team winner;
            if (currentMatch.Score1 > currentMatch.Score2) winner = currentMatch.Team1;
            else if (currentMatch.Score2 > currentMatch.Score1) winner = currentMatch.Team2;
            else
            {
                winner = currentMatch.Team1;
                StatusMessage = "Remíza - pro postup upřednostněn tým 1.";
            }
            currentMatch.WinnerId = winner.Id;
            currentMatch.IsCompleted = true;
            int currentRoundIdx = -1;
            int matchIdx = -1;

            for (int i = 0; i < Rounds.Count; i++)
            {
                matchIdx = Rounds[i].Matches.IndexOf(currentMatch);
                if (matchIdx != -1)
                {
                    currentRoundIdx = i;
                    break;
                }
            }
            if (currentRoundIdx == -1 || currentRoundIdx >= Rounds.Count - 1)
            {
                StatusMessage = $"MÁME VÍTĚZE! Turnaj vyhrál tým: {winner.Name}";
                return;
            }
            var nextRound = Rounds[currentRoundIdx + 1];
            int nextMatchIdx = matchIdx / 2; 
            var nextMatch = nextRound.Matches[nextMatchIdx];
            if (matchIdx % 2 == 0)
            {
                nextMatch.Team1 = winner;
                nextMatch.Team1Id = winner.Id;
            }
            else
            {
                nextMatch.Team2 = winner;
                nextMatch.Team2Id = winner.Id;
            }

            StatusMessage = $"Výsledek uložen. {winner.Name} postupuje!";
        }

        public void LoadFromRounds(IEnumerable<Round>? rounds)
        {
            Rounds.Clear();
            if (rounds == null)
            {
                IsPavouk = false;
                return;
            }

            foreach (var r in rounds.OrderBy(x => x.RoundNumber))
                Rounds.Add(r);

            IsPavouk = Rounds.Count > 0;
        }

        private static string RoundName(int teamsInRound) => teamsInRound switch
        {
            2 => "Finále",
            4 => "Semifinále",
            8 => "Čtvrtfinále",
            16 => "Osmifinále",
            32 => "Šestnáctifinále",
            _ => $"Kolo ({teamsInRound} týmů)"
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}