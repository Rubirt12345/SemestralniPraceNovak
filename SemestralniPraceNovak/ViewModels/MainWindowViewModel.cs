using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SemestralniPraceNovak.Models;

namespace SemestralniPraceNovak.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public TournamentViewModel TournamentViewModel { get; }
        public PlayerViewModel PlayerViewModel { get; }
        public object? OtherViewModelPlaceholder { get; }

        private bool _isDashboardVisible = true;
        public bool IsDashboardVisible { get => _isDashboardVisible; set { _isDashboardVisible = value; OnPropertyChanged(nameof(IsDashboardVisible)); } }

        private bool _isTournamentsVisible;
        public bool IsTournamentsVisible { get => _isTournamentsVisible; set { _isTournamentsVisible = value; OnPropertyChanged(nameof(IsTournamentsVisible)); } }

        private bool _isPlayersVisible;
        public bool IsPlayersVisible { get => _isPlayersVisible; set { _isPlayersVisible = value; OnPropertyChanged(nameof(IsPlayersVisible)); } }

        private bool _isTeamsVisible;
        public bool IsTeamsVisible { get => _isTeamsVisible; set { _isTeamsVisible = value; OnPropertyChanged(nameof(IsTeamsVisible)); } }

        private bool _isBracketVisible;
        public bool IsBracketVisible { get => _isBracketVisible; set { _isBracketVisible = value; OnPropertyChanged(nameof(IsBracketVisible)); } }
        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowTournamentsViewCommand { get; }
        public ICommand ShowPlayersViewCommand { get; }
        public ICommand ShowTeamsViewCommand { get; }
        public ICommand ShowBracketViewCommand { get; }

        private BracketViewModel _bracketViewModel;
        public BracketViewModel BracketViewModel
        {
            get => _bracketViewModel;
            private set { _bracketViewModel = value; OnPropertyChanged(nameof(BracketViewModel)); }
        }

        public MainWindowViewModel()
        {
            TournamentViewModel = new TournamentViewModel();
            PlayerViewModel = new PlayerViewModel();
            _bracketViewModel = new BracketViewModel();
            ShowDashboardCommand = new RelayCommand(_ => ShowDashboard());
            ShowTournamentsViewCommand = new RelayCommand(_ => ShowTournaments());
            ShowPlayersViewCommand = new RelayCommand(_ => ShowPlayers());
            ShowTeamsViewCommand = new RelayCommand(_ => ShowTeams());
            ShowBracketViewCommand = new RelayCommand(_ => ShowBracketView());

            TournamentViewModel.PropertyChanged += TournamentViewModel_PropertyChanged;

            UpdateBracketForSelectedTournament(TournamentViewModel.SelectedTournament);
        }

        private void TournamentViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TournamentViewModel.SelectedTournament))
            {
                UpdateBracketForSelectedTournament(TournamentViewModel.SelectedTournament);
            }
        }

        private void UpdateBracketForSelectedTournament(Models.Tournament? selected)
        {
            if (selected == null)
            {
                BracketViewModel.ClearCommand.Execute(null);
                return;
            }

            var roundsCollection = selected.Rounds ?? new ObservableCollection<Models.Round>();
            BracketViewModel.LoadFromRounds(roundsCollection);
            BracketViewModel.IsPavouk = selected is Models.SpiderTournament;
        }

        private void ShowDashboard() { ResetVisibilities(); IsDashboardVisible = true; }
        private void ShowTournaments() { ResetVisibilities(); IsTournamentsVisible = true; }
        private void ShowPlayers() { ResetVisibilities(); IsPlayersVisible = true; }
        private void ShowTeams() { ResetVisibilities(); IsTeamsVisible = true; }

        private void ShowBracketView()
        {
            BracketViewModel.LoadTeamsFromDatabase();
            ResetVisibilities();
            IsBracketVisible = true;
        }

        private void ResetVisibilities()
        {
            IsDashboardVisible = IsTournamentsVisible = IsPlayersVisible = IsTeamsVisible = IsBracketVisible = false;
        }
    }
}