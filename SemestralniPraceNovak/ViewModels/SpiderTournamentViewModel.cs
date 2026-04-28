using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SemestralniPraceNovak.Database;
using SemestralniPraceNovak.Models;
using SemestralniPraceNovak.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SemestralniPraceNovak.ViewModels
{
    public partial class SpiderTournamentViewModel : ViewModelBase
    {
        private readonly SpiderTournamentService _spiderService;
        private readonly SportService _sportService;
        private readonly PlayerService _playerService;

        [ObservableProperty]
        private ObservableCollection<SpiderTournament> tournaments = new();

        [ObservableProperty]
        private SpiderTournament selectedTournament;

        [ObservableProperty]
        private ObservableCollection<Sport> sports = new();

        [ObservableProperty]
        private Sport selectedSport;

        [ObservableProperty]
        private ObservableCollection<Team> availableTeams = new();

        [ObservableProperty]
        private Team selectedAvailableTeam;

        [ObservableProperty]
        private string newTournamentName;

        [ObservableProperty]
        private string spiderBracketInfo = "Pavoučí turnaj - KO systém";

        [ObservableProperty]
        private ObservableCollection<SpiderParticipant> standings = new();

        public SpiderTournamentViewModel()
        {
            var context = new AppDbContext();

            _spiderService = new SpiderTournamentService(context);
            _sportService = new SportService(context);
            _playerService = new PlayerService(context);

            _ = LoadAsync();
        }
        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                Sports = new ObservableCollection<Sport>(await _sportService.GetAllSportsAsync());
                AvailableTeams = new ObservableCollection<Team>(await _playerService.GetAllTeamsAsync());

                var data = await _spiderService.GetSpiderTournamentsByTournamentIdAsync(0);
                Tournaments = new ObservableCollection<SpiderTournament>(data);
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            finally { IsLoading = false; }
        }
        partial void OnSelectedTournamentChanged(SpiderTournament value)
        {
            if (value != null)
                _ = SelectTournamentAsync(value);
        }

        [RelayCommand]
        public async Task SelectTournamentAsync(SpiderTournament tournament)
        {
            if (tournament == null) return;

            try
            {
                IsLoading = true;

                SelectedTournament = await _spiderService.GetSpiderTournamentByIdAsync(tournament.Id);

                var standingsData = await _spiderService.GetStandingsAsync(tournament.Id);
                Standings = new ObservableCollection<SpiderParticipant>(standingsData);

                UpdateInfo();
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
            finally { IsLoading = false; }
        }
        [RelayCommand]
        public async Task CreateTournamentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTournamentName))
            {
                ErrorMessage = "Zadejte název turnaje.";
                return;
            }

            try
            {
                var tournament = await _spiderService.CreateSpiderTournamentAsync(
                    NewTournamentName,
                    1,
                    8 
                );

                Tournaments.Add(tournament);
                SelectedTournament = tournament;
                NewTournamentName = string.Empty;
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }
        [RelayCommand]
        public async Task InitializeTournamentAsync()
        {
            if (SelectedTournament == null) return;

            try
            {
                await _spiderService.InitializeSpiderAsync(SelectedTournament.Id);
                await SelectTournamentAsync(SelectedTournament);
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }
        [RelayCommand]
        public async Task AddTeamAsync()
        {
            if (SelectedTournament == null || SelectedAvailableTeam == null) return;

            await _spiderService.AddTeamAsync(SelectedTournament.Id, SelectedAvailableTeam.Id);
            await SelectTournamentAsync(SelectedTournament);
        }
        [RelayCommand]
        public async Task DeleteTournamentAsync(SpiderTournament tournament)
        {
            if (tournament == null) return;

            await _spiderService.DeleteAsync(tournament.Id);
            Tournaments.Remove(tournament);
        }
        private void UpdateInfo()
        {
            if (SelectedTournament == null) return;

            int participants = SelectedTournament.Participants?.Count ?? 0;
            int matches = SelectedTournament.Rounds?.Sum(r => r.Matches.Count) ?? 0;

            SpiderBracketInfo = $"Účastníků: {participants} | Zápasů: {matches}";
        }
    }
}