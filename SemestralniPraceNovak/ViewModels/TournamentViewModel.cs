using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SemestralniPraceNovak.Database;
using SemestralniPraceNovak.Models;
using SemestralniPraceNovak.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.ViewModels
{
    public partial class TournamentViewModel : ViewModelBase
    {
        private readonly TournamentService _tournamentService;
        private readonly SportService _sportService;
        private readonly MatchService _matchService;
        private readonly PlayerService _playerService; 

        [ObservableProperty]
        private ObservableCollection<Tournament> tournaments;

        [ObservableProperty]
        private ObservableCollection<Sport> sports;

        [ObservableProperty]
        private Tournament selectedTournament;

        [ObservableProperty]
        private ObservableCollection<Round> rounds;

        [ObservableProperty]
        private ObservableCollection<TournamentParticipant> standings;

        [ObservableProperty]
        private ObservableCollection<Match> matches;

        [ObservableProperty]
        private string newTournamentName;

        [ObservableProperty]
        private Sport selectedSport;

        [ObservableProperty]
        private Round selectedRound;

        
        [ObservableProperty]
        private ObservableCollection<Team> availableTeams;

        [ObservableProperty]
        private Team selectedAvailableTeam;
        

        public TournamentViewModel()
        {
            var context = new AppDbContext();
            _tournamentService = new TournamentService(context);
            _sportService = new SportService(context);
            _matchService = new MatchService(context);
            _playerService = new PlayerService(context); 

            Tournaments = new ObservableCollection<Tournament>();
            Sports = new ObservableCollection<Sport>();
            Rounds = new ObservableCollection<Round>();
            Standings = new ObservableCollection<TournamentParticipant>();
            Matches = new ObservableCollection<Match>();
            AvailableTeams = new ObservableCollection<Team>(); 

            _ = LoadAsync();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                var tournaments = await _tournamentService.GetAllTournamentsAsync();
                Tournaments.Clear();
                foreach (var tournament in tournaments)
                {
                    Tournaments.Add(tournament);
                }

                var sports = await _sportService.GetAllSportsAsync();
                Sports.Clear();
                foreach (var sport in sports)
                {
                    Sports.Add(sport);
                }

                
                var teams = await _playerService.GetAllTeamsAsync();
                AvailableTeams.Clear();
                foreach (var team in teams)
                {
                    AvailableTeams.Add(team);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi naèítání: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        
        [RelayCommand]
        public async Task AddTeamToParticipantAsync()
        {
            if (SelectedTournament == null || SelectedAvailableTeam == null)
            {
                ErrorMessage = "Vyberte turnaj a tým k pøidání.";
                return;
            }

            try
            {
                
                await _tournamentService.AddTeamParticipantAsync(SelectedTournament.Id, SelectedAvailableTeam.Id);

                
                await SelectTournamentAsync(SelectedTournament);

                SelectedAvailableTeam = null; 
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi pøidávání týmu: {ex.Message}";
            }
        }
        

        [RelayCommand]
        public async Task CreateTournamentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTournamentName) || SelectedSport == null)
            {
                ErrorMessage = "Vyplòte prosím všechna pole";
                return;
            }

            try
            {
                var tournament = await _tournamentService.CreateTournamentAsync(NewTournamentName, SelectedSport.Id);
                Tournaments.Add(tournament);
                NewTournamentName = string.Empty;
                SelectedSport = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi vytváøení turnaje: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task SelectTournamentAsync(Tournament tournament)
        {
            if (tournament == null) return;

            try
            {
                IsLoading = true;
                SelectedTournament = await _tournamentService.GetTournamentByIdAsync(tournament.Id);

                Rounds.Clear();
                foreach (var round in SelectedTournament.Rounds)
                {
                    Rounds.Add(round);
                }

                var standings = await _tournamentService.GetTournamentStandingsAsync(tournament.Id);
                Standings.Clear();
                foreach (var standing in standings)
                {
                    Standings.Add(standing);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteTournamentAsync(Tournament tournament)
        {
            try
            {
                await _tournamentService.DeleteTournamentAsync(tournament.Id);
                Tournaments.Remove(tournament);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi mazání: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task CreateRoundAsync()
        {
            if (SelectedTournament == null)
            {
                ErrorMessage = "Vyberte turnaj";
                return;
            }

            try
            {
                var roundNumber = SelectedTournament.Rounds.Count + 1;
                var round = await _tournamentService.CreateRoundAsync(
                    SelectedTournament.Id,
                    roundNumber,
                    $"Kolo {roundNumber}"
                );
                Rounds.Add(round);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task LoadRoundMatchesAsync(Round round)
        {
            try
            {
                SelectedRound = round;
                var matches = await _matchService.GetRoundMatchesAsync(round.Id);
                Matches.Clear();
                foreach (var match in matches)
                {
                    Matches.Add(match);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task DrawMatchesAsync()
        {
            if (SelectedRound == null)
            {
                ErrorMessage = "Vyberte kolo";
                return;
            }

            try
            {
                var context = new AppDbContext();
                var round = await context.Rounds
                    .Include(r => r.Tournament)
                    .FirstOrDefaultAsync(r => r.Id == SelectedRound.Id);

                if (round.Tournament.Participants.Any(p => p.PlayerId.HasValue))
                {
                    await _matchService.DrawPlayersMatchesAsync(SelectedRound.Id);
                }
                else
                {
                    await _matchService.DrawTeamsMatchesAsync(SelectedRound.Id);
                }

                await LoadRoundMatchesAsync(SelectedRound);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi losování: {ex.Message}";
            }
        }
    }
}