using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SemestralniPraceNovak.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
       
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsDashboardVisible))]
        [NotifyPropertyChangedFor(nameof(IsTournamentsVisible))]
        [NotifyPropertyChangedFor(nameof(IsPlayersVisible))]
        [NotifyPropertyChangedFor(nameof(IsTeamsVisible))]
        private int currentViewIndex = 0;

       
        public bool IsDashboardVisible => CurrentViewIndex == 0;
        public bool IsTournamentsVisible => CurrentViewIndex == 1;
        public bool IsPlayersVisible => CurrentViewIndex == 2;
        public bool IsTeamsVisible => CurrentViewIndex == 3;

        public TournamentViewModel TournamentViewModel { get; }
        public PlayerViewModel PlayerViewModel { get; }

        public IRelayCommand ShowDashboardCommand { get; }
        public IRelayCommand ShowTournamentsViewCommand { get; }
        public IRelayCommand ShowPlayersViewCommand { get; }
        public IRelayCommand ShowTeamsViewCommand { get; }

        public MainWindowViewModel()
        {
            TournamentViewModel = new TournamentViewModel();
            PlayerViewModel = new PlayerViewModel();

            ShowDashboardCommand = new RelayCommand(() => CurrentViewIndex = 0);
            ShowTournamentsViewCommand = new RelayCommand(() => CurrentViewIndex = 1);
            ShowPlayersViewCommand = new RelayCommand(() => CurrentViewIndex = 2);
            ShowTeamsViewCommand = new RelayCommand(() => CurrentViewIndex = 3);
        }
    }
}