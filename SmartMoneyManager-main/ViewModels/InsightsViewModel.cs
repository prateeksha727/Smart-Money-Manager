using SmartMoneyManager.Data;
using SmartMoneyManager.Services;
using System.Collections.ObjectModel;

namespace SmartMoneyManager.ViewModels
{
    public class InsightsViewModel : BaseViewModel
    {
        private readonly IFinanceService _svc = ServiceLocator.Current;

        public ObservableCollection<InsightItem>    Insights   { get; } = new();
        public ObservableCollection<MonthComparison> Comparison { get; } = new();

        private double _essTot, _nonEssTot, _essPct, _nonEssPct;
        public double EssentialTotal    { get => _essTot;    set => SetProperty(ref _essTot, value); }
        public double NonEssentialTotal { get => _nonEssTot; set => SetProperty(ref _nonEssTot, value); }
        public double EssentialPct      { get => _essPct;    set => SetProperty(ref _essPct, value); }
        public double NonEssentialPct   { get => _nonEssPct; set => SetProperty(ref _nonEssPct, value); }

        public void Load()
        {
            var d = _svc.GetInsightData(SessionManager.UserId);
            EssentialTotal    = d.EssentialTotal;
            NonEssentialTotal = d.NonEssentialTotal;
            EssentialPct      = d.EssentialPct;
            NonEssentialPct   = d.NonEssentialPct;
            Insights.Clear();   d.Insights.ForEach(i => Insights.Add(i));
            Comparison.Clear(); d.Comparison.ForEach(c => Comparison.Add(c));
        }
    }
}
