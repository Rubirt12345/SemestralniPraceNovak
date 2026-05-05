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
        partial void OnSelectedTournamentChanged(Tournament value)
        {
            if (value != null)
            {
                _ = SelectTournamentAsync(value);
            }
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var loadedTournaments = await _tournamentService.GetAllTournamentsAsync();
                Tournaments.Clear();
                foreach (var t in loadedTournaments) Tournaments.Add(t);

                var loadedSports = await _sportService.GetAllSportsAsync();
                Sports.Clear();
                foreach (var s in loadedSports) Sports.Add(s);

                var teams = await _playerService.GetAllTeamsAsync();
                AvailableTeams.Clear();
                foreach (var team in teams) AvailableTeams.Add(team);
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
                ErrorMessage = "Vyberte turnaj a tým k přidání.";
                return;
            }

            try
            {
                ErrorMessage = string.Empty;
                var existingParticipants = await _tournamentService.GetTournamentStandingsAsync(SelectedTournament.Id);
                if (existingParticipants.Any(p => p.TeamId == SelectedAvailableTeam.Id))
                {
                    ErrorMessage = $"Tým '{SelectedAvailableTeam.Name}' už v tomto turnaji je!";
                    return;
                }
                await _tournamentService.AddTeamParticipantAsync(SelectedTournament.Id, SelectedAvailableTeam.Id);
                await SelectTournamentAsync(SelectedTournament);
                SelectedAvailableTeam = null;
            }
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ErrorMessage = $"DB Chyba při přidávání týmu: {realError}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba při přidávání týmu: {ex.Message}";
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
                ErrorMessage = string.Empty;
                var tournament = await _tournamentService.CreateTournamentAsync(NewTournamentName, SelectedSport.Id);
                Tournaments.Add(tournament);
                NewTournamentName = string.Empty;
                SelectedSport = null;
            }
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                System.Diagnostics.Debug.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                System.Diagnostics.Debug.WriteLine("SKUTEČNÁ CHYBA DATABÁZE:");
                System.Diagnostics.Debug.WriteLine(realError);
                System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n");

                ErrorMessage = $"DB Chyba: {realError}";
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
                var freshTournament = await _tournamentService.GetTournamentByIdAsync(tournament.Id);
                if (freshTournament == null) return;

                Rounds.Clear();
                if (freshTournament.Rounds != null)
                {
                    foreach (var round in freshTournament.Rounds)
                        Rounds.Add(round);
                }

                var standingsData = await _tournamentService.GetTournamentStandingsAsync(tournament.Id);
                Standings.Clear();
                foreach (var standing in standingsData)
                    Standings.Add(standing);
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
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ErrorMessage = $"DB Chyba pøi mazání: {realError}";
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
                var roundNumber = (SelectedTournament.Rounds?.Count ?? 0) + 1;
                var round = await _tournamentService.CreateRoundAsync(
                    SelectedTournament.Id,
                    roundNumber,
                    $"Kolo {roundNumber}"
                );
                Rounds.Add(round);
            }
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                System.Diagnostics.Debug.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                System.Diagnostics.Debug.WriteLine("SKUTEČNÁ CHYBA DATABÁZE:");
                System.Diagnostics.Debug.WriteLine(realError);
                System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n");

                ErrorMessage = $"DB Chyba: {realError}";
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
                var loadedMatches = await _matchService.GetRoundMatchesAsync(round.Id);
                Matches.Clear();
                foreach (var match in loadedMatches) Matches.Add(match);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task DrawMatchesAsync(Round round)
        {
            var targetRound = round ?? SelectedRound;

            if (targetRound == null)
            {
                ErrorMessage = "Vyberte kolo pro losování.";
                return;
            }

            try
            {
                IsLoading = true;
                SelectedRound = targetRound;

                if (SelectedTournament.Participants != null && SelectedTournament.Participants.Any(p => p.PlayerId.HasValue))
                    await _matchService.DrawPlayersMatchesAsync(targetRound.Id);
                else
                    await _matchService.DrawTeamsMatchesAsync(targetRound.Id);

                await LoadRoundMatchesAsync(targetRound);
            }
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                System.Diagnostics.Debug.WriteLine("\n!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                System.Diagnostics.Debug.WriteLine("SKUTEČNÁ CHYBA DATABÁZE:");
                System.Diagnostics.Debug.WriteLine(realError);
                System.Diagnostics.Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n");

                ErrorMessage = $"DB Chyba: {realError}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi losování: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand]
        public async Task SaveMatchScoreAsync(Match match)
        {
            if (match == null) return;
            if (!match.Score1.HasValue || !match.Score2.HasValue)
            {
                ErrorMessage = "Před uložením prosím zadejte platné skóre pro oba týmy.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                await _matchService.SetMatchResultAsync(match.Id, match.Score1.Value, match.Score2.Value);
                await SelectTournamentAsync(SelectedTournament);

            }
            catch (DbUpdateException ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ErrorMessage = $"DB Chyba při ukládání skóre: {realError}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba při ukládání skóre: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
 }
