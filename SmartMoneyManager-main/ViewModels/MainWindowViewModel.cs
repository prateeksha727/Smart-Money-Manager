using SmartMoneyManager.Data;
using System;
using System.Windows.Input;

namespace SmartMoneyManager.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private object? _view;
        private string  _nav = "Dashboard", _user = "";

        public object? CurrentView  { get => _view; set => SetProperty(ref _view, value); }
        public string  ActiveNav    { get => _nav;  set => SetProperty(ref _nav, value); }
        public string  UserDisplay  { get => _user; set => SetProperty(ref _user, value); }

        // One instance per ViewModel — loaded on demand
        public DashboardViewModel     Dashboard     { get; } = new();
        public ExpensesViewModel      Expenses      { get; } = new();
        public BudgetViewModel        Budget        { get; } = new();
        public InsightsViewModel      Insights      { get; } = new();
        public SubscriptionsViewModel Subscriptions { get; } = new();
        public SavingsGoalsViewModel  SavingsGoals  { get; } = new();
        public InvestmentViewModel    Investment    { get; } = new();

        public ICommand NavCmd    { get; }
        public ICommand LogoutCmd { get; }

        public event Action? LogoutRequested;

        public MainWindowViewModel()
        {
            NavCmd    = new RelayCommand<string>(Nav);
            LogoutCmd = new RelayCommand(Logout);
            UserDisplay = SessionManager.CurrentUser?.DisplayName.Length > 0
                ? SessionManager.CurrentUser.DisplayName
                : SessionManager.CurrentUser?.Username ?? "";
            Nav("Dashboard");
        }

        private void Nav(string? page)
        {
            ActiveNav = page ?? "Dashboard";
            switch (page)
            {
                case "Dashboard":    Dashboard.Load();    CurrentView = Dashboard;    break;
                case "Expenses":     Expenses.Load();     CurrentView = Expenses;     break;
                case "Budget":       Budget.Load();       CurrentView = Budget;       break;
                case "Insights":     Insights.Load();     CurrentView = Insights;     break;
                case "Subscriptions":Subscriptions.Load();CurrentView = Subscriptions;break;
                case "Savings":      SavingsGoals.Load(); CurrentView = SavingsGoals; break;
                case "Investment":                        CurrentView = Investment;   break;
                default:             Dashboard.Load();    CurrentView = Dashboard;    break;
            }
        }

        private void Logout() { SessionManager.Logout(); LogoutRequested?.Invoke(); }
    }
}
