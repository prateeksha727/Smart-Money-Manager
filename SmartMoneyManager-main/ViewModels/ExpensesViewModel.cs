using SmartMoneyManager.Data;
using SmartMoneyManager.Models;
using SmartMoneyManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SmartMoneyManager.ViewModels
{
    public class ExpensesViewModel : BaseViewModel
    {
        private readonly IFinanceService _svc = ServiceLocator.Current;

        public ObservableCollection<Expense>  Expenses   { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        private string    _desc = "", _amt = "", _filter = "";
        private DateTime  _date = DateTime.Today;
        private Category? _cat;
        private bool      _essential = true, _editing;
        private Expense?  _selected;

        public string   Description { get => _desc;      set { SetProperty(ref _desc, value); AutoCat(); } }
        public string   AmountText  { get => _amt;       set => SetProperty(ref _amt, value); }
        public DateTime Date        { get => _date;      set => SetProperty(ref _date, value); }
        public Category? SelCategory{ get => _cat;       set => SetProperty(ref _cat, value); }
        public bool     IsEssential { get => _essential; set => SetProperty(ref _essential, value); }
        public bool     IsEditing   { get => _editing;   set => SetProperty(ref _editing, value); }
        public string   FilterText  { get => _filter;    set { SetProperty(ref _filter, value); Reload(); } }

        public Expense? SelectedExpense
        {
            get => _selected;
            set { SetProperty(ref _selected, value); if (value != null) FillForm(value); }
        }

        public ICommand SaveCmd   { get; }
        public ICommand DeleteCmd { get; }
        public ICommand ClearCmd  { get; }

        public ExpensesViewModel()
        {
            SaveCmd   = new RelayCommand(Save);
            DeleteCmd = new RelayCommand(Delete, () => _selected != null);
            ClearCmd  = new RelayCommand(Clear);
            Load();
        }

        public void Load()
        {
            Categories.Clear();
            _svc.GetCategories().ForEach(c => Categories.Add(c));
            SelCategory = Categories.FirstOrDefault();
            Reload();
        }

        private void Reload()
        {
            Expenses.Clear();
            _svc.GetExpenses(SessionManager.UserId, FilterText).ForEach(e => Expenses.Add(e));
        }

        private void Save()
        {
            if (!decimal.TryParse(AmountText, out var amt) || amt <= 0)
            { MessageBox.Show("Enter a valid amount.", "Validation"); return; }
            if (string.IsNullOrWhiteSpace(Description))
            { MessageBox.Show("Enter a description.", "Validation"); return; }

            var e = new Expense
            {
                Id          = _editing ? (_selected?.Id ?? 0) : 0,
                UserId      = SessionManager.UserId,
                Description = Description.Trim(),
                Amount      = amt,
                Date        = Date,
                CategoryId  = SelCategory?.Id ?? Categories.First().Id,
                IsEssential = IsEssential,
            };
            _svc.SaveExpense(e, !_editing);
            Clear(); Reload();
        }

        private void Delete()
        {
            if (_selected == null) return;
            if (MessageBox.Show($"Delete '{_selected.Description}'?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            _svc.DeleteExpense(_selected.Id);
            Clear(); Reload();
        }

        private void FillForm(Expense e)
        {
            Description = e.Description;
            AmountText  = e.Amount.ToString("0.00");
            Date        = e.Date;
            SelCategory = Categories.FirstOrDefault(c => c.Id == e.CategoryId);
            IsEssential = e.IsEssential;
            IsEditing   = true;
        }

        private void Clear()
        {
            Description = ""; AmountText = ""; Date = DateTime.Today;
            SelCategory = Categories.FirstOrDefault();
            IsEssential = true; IsEditing = false; SelectedExpense = null;
        }

        private void AutoCat()
        {
            if (string.IsNullOrWhiteSpace(Description) || !Categories.Any()) return;
            var lo = Description.ToLowerInvariant();
            string? match = null;
            if (lo.Contains("uber") || lo.Contains("lyft") || lo.Contains("gas") ||
                lo.Contains("bus")  || lo.Contains("train") || lo.Contains("parking"))
                match = "Transport";
            else if (lo.Contains("netflix") || lo.Contains("spotify") || lo.Contains("cinema") ||
                     lo.Contains("steam")   || lo.Contains("youtube"))
                match = "Entertainment";
            else if (lo.Contains("coffee") || lo.Contains("mcdonald") || lo.Contains("pizza") ||
                     lo.Contains("grocery") || lo.Contains("starbucks") || lo.Contains("restaurant"))
                match = "Food & Dining";
            else if (lo.Contains("amazon") || lo.Contains("target") || lo.Contains("shop"))
                match = "Shopping";
            else if (lo.Contains("gym") || lo.Contains("pharmacy") || lo.Contains("doctor"))
                match = "Health";
            else if (lo.Contains("electric") || lo.Contains("internet") || lo.Contains("bill"))
                match = "Utilities";

            if (match != null)
            {
                var c = Categories.FirstOrDefault(x => x.Name == match);
                if (c != null) SelCategory = c;
            }
        }
    }
}
