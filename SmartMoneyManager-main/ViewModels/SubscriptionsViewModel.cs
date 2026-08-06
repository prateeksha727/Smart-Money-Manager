using SmartMoneyManager.Models;
using SmartMoneyManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SmartMoneyManager.ViewModels
{
    public class SubscriptionsViewModel : BaseViewModel
    {
        private readonly IFinanceService _svc = ServiceLocator.Current;

        public ObservableCollection<Subscription> Subscriptions { get; } = new();

        private string        _name = "", _cost = "", _cat = "Entertainment";
        private DateTime      _nextDate = DateTime.Today.AddMonths(1);
        private bool          _editing;
        private Subscription? _sel;
        private double        _total;

        public string    Name         { get => _name;    set => SetProperty(ref _name, value); }
        public string    CostText     { get => _cost;    set => SetProperty(ref _cost, value); }
        public string    Category     { get => _cat;     set => SetProperty(ref _cat, value); }
        public DateTime  NextDate     { get => _nextDate;set => SetProperty(ref _nextDate, value); }
        public bool      IsEditing    { get => _editing; set => SetProperty(ref _editing, value); }
        public double    TotalMonthly { get => _total;   set => SetProperty(ref _total, value); }

        public Subscription? SelSub
        {
            get => _sel;
            set { SetProperty(ref _sel, value); if (value != null) FillForm(value); }
        }

        public string[] Categories { get; } =
            { "Entertainment", "Health", "Shopping", "Utilities", "Education", "Other" };

        public ICommand SaveCmd   { get; }
        public ICommand DeleteCmd { get; }
        public ICommand ClearCmd  { get; }

        public SubscriptionsViewModel()
        {
            SaveCmd   = new RelayCommand(Save);
            DeleteCmd = new RelayCommand(Delete, () => _sel != null);
            ClearCmd  = new RelayCommand(Clear);
            Load();
        }

        public void Load()
        {
            Subscriptions.Clear();
            var list = _svc.GetSubscriptions();
            list.ForEach(s => Subscriptions.Add(s));
            TotalMonthly = list.Sum(s => (double)s.MonthlyCost);
        }

        private void Save()
        {
            if (!decimal.TryParse(CostText, out var cost) || cost <= 0)
            { MessageBox.Show("Enter a valid monthly cost.", "Validation"); return; }
            if (string.IsNullOrWhiteSpace(Name)) return;

            var s = new Subscription
            {
                Id = _editing ? (_sel?.Id ?? 0) : 0,
                Name = Name.Trim(), MonthlyCost = cost,
                NextPaymentDate = NextDate, Category = Category, IsActive = true,
            };
            _svc.SaveSubscription(s, !_editing);
            Clear(); Load();
        }

        private void Delete()
        {
            if (_sel == null) return;
            _svc.DeleteSubscription(_sel.Id);
            Clear(); Load();
        }

        private void FillForm(Subscription s)
        {
            Name = s.Name; CostText = s.MonthlyCost.ToString("0.00");
            NextDate = s.NextPaymentDate; Category = s.Category; IsEditing = true;
        }

        private void Clear()
        {
            Name = ""; CostText = ""; NextDate = DateTime.Today.AddMonths(1);
            Category = "Entertainment"; IsEditing = false; SelSub = null;
        }
    }
}
