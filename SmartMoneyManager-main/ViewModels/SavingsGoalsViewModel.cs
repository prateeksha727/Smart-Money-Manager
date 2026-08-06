using SmartMoneyManager.Models;
using SmartMoneyManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SmartMoneyManager.ViewModels
{
    public class SavingsGoalsViewModel : BaseViewModel
    {
        private readonly IFinanceService _svc = ServiceLocator.Current;

        public ObservableCollection<SavingsGoal> Goals { get; } = new();

        private string      _name = "", _target = "", _current = "", _icon = "🎯";
        private DateTime    _date = DateTime.Today.AddYears(1);
        private bool        _editing;
        private SavingsGoal? _sel;

        public string    Name        { get => _name;    set => SetProperty(ref _name, value); }
        public string    TargetText  { get => _target;  set => SetProperty(ref _target, value); }
        public string    CurrentText { get => _current; set => SetProperty(ref _current, value); }
        public DateTime  TargetDate  { get => _date;    set => SetProperty(ref _date, value); }
        public string    Icon        { get => _icon;    set => SetProperty(ref _icon, value); }
        public bool      IsEditing   { get => _editing; set => SetProperty(ref _editing, value); }

        public SavingsGoal? SelGoal
        {
            get => _sel;
            set { SetProperty(ref _sel, value); if (value != null) FillForm(value); }
        }

        public string[] Icons { get; } = { "🎯", "🏠", "🚗", "✈", "💻", "🎓", "💍", "🛡", "📱", "🎮" };

        public ICommand SaveCmd   { get; }
        public ICommand DeleteCmd { get; }
        public ICommand ClearCmd  { get; }

        public SavingsGoalsViewModel()
        {
            SaveCmd   = new RelayCommand(Save);
            DeleteCmd = new RelayCommand(Delete, () => _sel != null);
            ClearCmd  = new RelayCommand(Clear);
            Load();
        }

        public void Load()
        {
            Goals.Clear();
            _svc.GetSavingsGoals().ForEach(g => Goals.Add(g));
        }

        private void Save()
        {
            if (!decimal.TryParse(TargetText, out var tgt) || tgt <= 0)
            { MessageBox.Show("Enter a valid target amount.", "Validation"); return; }
            if (!decimal.TryParse(CurrentText, out var cur)) cur = 0;
            if (string.IsNullOrWhiteSpace(Name)) { MessageBox.Show("Enter a goal name.", "Validation"); return; }

            var g = new SavingsGoal
            {
                Id = _editing ? (_sel?.Id ?? 0) : 0,
                Name = Name.Trim(), TargetAmount = tgt, CurrentAmount = cur,
                TargetDate = TargetDate, Icon = Icon,
            };
            _svc.SaveSavingsGoal(g, !_editing);
            Clear(); Load();
        }

        private void Delete()
        {
            if (_sel == null) return;
            _svc.DeleteSavingsGoal(_sel.Id);
            Clear(); Load();
        }

        private void FillForm(SavingsGoal g)
        {
            Name = g.Name; TargetText = g.TargetAmount.ToString("0.00");
            CurrentText = g.CurrentAmount.ToString("0.00");
            TargetDate = g.TargetDate; Icon = g.Icon; IsEditing = true;
        }

        private void Clear()
        {
            Name = ""; TargetText = ""; CurrentText = ""; Icon = "🎯";
            TargetDate = DateTime.Today.AddYears(1); IsEditing = false; SelGoal = null;
        }
    }
}
