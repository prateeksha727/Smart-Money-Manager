using SmartMoneyManager.Data;
using SmartMoneyManager.Models;
using SmartMoneyManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartMoneyManager.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly IFinanceService _svc = ServiceLocator.Current;

        public ObservableCollection<BudgetSummary> Budgets    { get; } = new();
        public ObservableCollection<Category>      Categories { get; } = new();

        private Category?      _selCat;
        private string         _limit = "";
        private BudgetSummary? _selBudget;

        public Category?      SelCategory { get => _selCat;    set => SetProperty(ref _selCat, value); }
        public string         LimitText   { get => _limit;     set => SetProperty(ref _limit, value); }
        public BudgetSummary? SelBudget   { get => _selBudget; set => SetProperty(ref _selBudget, value); }

        public ICommand SaveCmd   { get; }
        public ICommand DeleteCmd { get; }

        public BudgetViewModel()
        {
            SaveCmd   = new RelayCommand(Save);
            DeleteCmd = new RelayCommand(Delete, () => _selBudget != null);
            Load();
        }

        public void Load()
        {
            var now = DateTime.Today;
            Categories.Clear();
            _svc.GetCategories().ForEach(c => Categories.Add(c));
            SelCategory = Categories.Count > 0 ? Categories[0] : null;
            Budgets.Clear();
            _svc.GetBudgetSummaries(SessionManager.UserId, now.Month, now.Year)
                .ForEach(b => Budgets.Add(b));
        }

        private void Save()
        {
            if (!decimal.TryParse(LimitText, out var lim) || lim <= 0)
            { MessageBox.Show("Enter a valid limit.", "Validation"); return; }
            if (SelCategory == null) return;
            var now = DateTime.Today;
            _svc.SaveBudget(SelCategory.Id, lim, now.Month, now.Year);
            LimitText = ""; Load();
        }

        private void Delete()
        {
            if (_selBudget == null) return;
            _svc.DeleteBudget(_selBudget.BudgetId);
            SelBudget = null; Load();
        }
    }
}
