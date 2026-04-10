using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SemestralniPraceNovak.Models;
using SemestralniPraceNovak.Services;
using System;
using System.Collections.ObjectModel;
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
        public PlayerViewModel()
        {
            _playerService = new PlayerService(new Database.AppDbContext());
            
            Players = new ObservableCollection<Player>();
            Teams = new ObservableCollection<Team>();

            _ = LoadAsync();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                
                var players = await _playerService.GetAllPlayersAsync();
                Players.Clear();
                foreach (var player in players)
                {
                    Players.Add(player);
                }

                var teams = await _playerService.GetAllTeamsAsync();
                Teams.Clear();
                foreach (var team in teams)
                {
                    Teams.Add(team);
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
        public async Task CreatePlayerAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPlayerName))
            {
                ErrorMessage = "Vyplòte jméno hráèe";
                return;
            }

            try
            {
                var player = await _playerService.CreatePlayerAsync(NewPlayerName);
                Players.Add(player);
                NewPlayerName = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task DeletePlayerAsync(Player player)
        {
            try
            {
                await _playerService.DeletePlayerAsync(player.Id);
                Players.Remove(player);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task CreateTeamAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTeamName))
            {
                ErrorMessage = "Vyplòte jméno týmu";
                return;
            }

            try
            {
                var team = await _playerService.CreateTeamAsync(NewTeamName);
                Teams.Add(team);
                NewTeamName = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task DeleteTeamAsync(Team team)
        {
            try
            {
                await _playerService.DeleteTeamAsync(team.Id);
                Teams.Remove(team);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Chyba: {ex.Message}";
            }
        }
    }
}