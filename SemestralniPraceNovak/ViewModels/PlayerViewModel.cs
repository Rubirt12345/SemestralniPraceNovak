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
    public partial class PlayerViewModel : ViewModelBase
    {
        private readonly PlayerService _playerService;

        [ObservableProperty]
        private ObservableCollection<Player> players;

        [ObservableProperty]
        private ObservableCollection<Team> teams;

        [ObservableProperty]
        private string newPlayerName;

        [ObservableProperty]
        private string newTeamName;

        [ObservableProperty]
        private Player selectedPlayer;

        [ObservableProperty]
        private Team selectedTeam;

        [ObservableProperty]
        private int tabIndex = 0;

        
        [ObservableProperty]
        private string playerTeamName = "Bez týmu";

        [ObservableProperty]
        private int matchesPlayed;

        [ObservableProperty]
        private int totalWins;

        [ObservableProperty]
        private string winRate = "0 %";

        
        [ObservableProperty]
        private ObservableCollection<Player> teamMembers;

        [ObservableProperty]
        private Player? selectedPlayerToAdd;

        public PlayerViewModel()
        {
            _playerService = new PlayerService(new AppDbContext());

            Players = new ObservableCollection<Player>();
            Teams = new ObservableCollection<Team>();
            TeamMembers = new ObservableCollection<Player>();

            _ = LoadAsync();
        }

       
        partial void OnSelectedPlayerChanged(Player? value)
        {
            if (value != null)
            {
                _ = RefreshPlayerStatsAsync(value);
            }
        }

        
        partial void OnSelectedTeamChanged(Team? value)
        {
            if (value != null)
            {
                _ = RefreshTeamMembersAsync(value);
            }
        }

        

        private async Task RefreshPlayerStatsAsync(Player player)
        {
            try
            {
                using var context = new AppDbContext();

                var participant = await context.TournamentParticipants
                    .Include(tp => tp.Team)
                    .FirstOrDefaultAsync(tp => tp.PlayerId == player.Id);

                PlayerTeamName = participant?.Team?.Name ?? "Hráè zatím nemá tým";

                var allMatches = await context.Matches
                    .Where(m => m.IsCompleted && (m.Player1Id == player.Id || m.Player2Id == player.Id))
                    .ToListAsync();

                MatchesPlayed = allMatches.Count;
                TotalWins = allMatches.Count(m => m.WinnerId == player.Id);

                if (MatchesPlayed > 0)
                {
                    double rate = (double)TotalWins / MatchesPlayed * 100;
                    WinRate = $"{Math.Round(rate, 1)} %";
                }
                else { WinRate = "0 %"; }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi naèítání statistik: {ex.Message}";
            }
        }

        private async Task RefreshTeamMembersAsync(Team team)
        {
            try
            {
                using var context = new AppDbContext();
                
                var members = await context.Players
                    .Where(p => p.TeamId == team.Id)
                    .ToListAsync();

                TeamMembers.Clear();
                foreach (var m in members)
                {
                    TeamMembers.Add(m);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi naèítání èlenù týmu: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task AddPlayerToTeamAsync()
        {
            if (SelectedTeam == null || SelectedPlayerToAdd == null)
            {
                ErrorMessage = "Vyberte tým a hráèe k pøidání.";
                return;
            }

            try
            {
                using var context = new AppDbContext();
                
                var player = await context.Players.FindAsync(SelectedPlayerToAdd.Id);
                if (player != null)
                {
                    player.TeamId = SelectedTeam.Id; 
                    await context.SaveChangesAsync();

                    
                    await RefreshTeamMembersAsync(SelectedTeam);
                    SelectedPlayerToAdd = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba pøi pøidávání do týmu: {ex.Message}";
            }
        }

       

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var players = await _playerService.GetAllPlayersAsync();
                Players.Clear();
                foreach (var p in players) Players.Add(p);

                var teams = await _playerService.GetAllTeamsAsync();
                Teams.Clear();
                foreach (var t in teams) Teams.Add(t);
            }
            catch (Exception ex) { ErrorMessage = $"Chyba: {ex.Message}"; }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task CreatePlayerAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPlayerName)) return;
            try
            {
                var player = await _playerService.CreatePlayerAsync(NewPlayerName);
                Players.Add(player);
                NewPlayerName = string.Empty;
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        [RelayCommand]
        public async Task DeletePlayerAsync(Player player)
        {
            try
            {
                await _playerService.DeletePlayerAsync(player.Id);
                Players.Remove(player);
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        [RelayCommand]
        public async Task CreateTeamAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTeamName)) return;
            try
            {
                var team = await _playerService.CreateTeamAsync(NewTeamName);
                Teams.Add(team);
                NewTeamName = string.Empty;
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }

        [RelayCommand]
        public async Task DeleteTeamAsync(Team team)
        {
            try
            {
                await _playerService.DeleteTeamAsync(team.Id);
                Teams.Remove(team);
            }
            catch (Exception ex) { ErrorMessage = ex.Message; }
        }
    }
}