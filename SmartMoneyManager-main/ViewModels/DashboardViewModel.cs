using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SmartMoneyManager.Data;
using SmartMoneyManager.Models;
using SmartMoneyManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SmartMoneyManager.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IFinanceService _svc = ServiceLocator.Current;

        private double _spent, _budget, _remaining, _daily;
        private string _topCat = "—";

        public double TotalSpent    { get => _spent;     set => SetProperty(ref _spent, value); }
        public double TotalBudget   { get => _budget;    set => SetProperty(ref _budget, value); }
        public double Remaining     { get => _remaining; set => SetProperty(ref _remaining, value); }
        public double DailyAverage  { get => _daily;     set => SetProperty(ref _daily, value); }
        public string TopCategory   { get => _topCat;    set => SetProperty(ref _topCat, value); }

        public ObservableCollection<Expense> RecentExpenses { get; } = new();

        private ISeries[] _pie = Array.Empty<ISeries>();
        private ISeries[] _bar = Array.Empty<ISeries>();
        private Axis[]    _bx  = Array.Empty<Axis>();
        private Axis[]    _by  = Array.Empty<Axis>();

        public ISeries[] PieSeries { get => _pie; private set => SetProperty(ref _pie, value); }
        public ISeries[] BarSeries { get => _bar; private set => SetProperty(ref _bar, value); }
        public Axis[]    BarXAxes  { get => _bx;  private set => SetProperty(ref _bx, value); }
        public Axis[]    BarYAxes  { get => _by;  private set => SetProperty(ref _by, value); }

        public void Load()
        {
            var data = _svc.GetDashboardData(SessionManager.UserId);

            TotalSpent   = data.TotalSpent;
            TotalBudget  = data.TotalBudget;
            Remaining    = data.Remaining;
            DailyAverage = data.DailyAverage;
            TopCategory  = data.TopCategory;

            RecentExpenses.Clear();
            data.RecentExpenses.ForEach(e => RecentExpenses.Add(e));

            // Pie chart — guard empty data
            var pieColors = new[]
            {
                "#FF9800","#2196F3","#9C27B0","#E91E63","#4CAF50","#00BCD4"
            };
            if (data.ByCategory.Any())
            {
                PieSeries = data.ByCategory.Select((x, i) => (ISeries)new PieSeries<double>
                {
                    Values      = new[] { Math.Max(0.01, x.Amt) },
                    Name        = x.Name,
                    Fill        = new SolidColorPaint(SKColor.Parse(pieColors[i % pieColors.Length])),
                    Stroke      = null,
                    InnerRadius = 36,
                }).ToArray();
            }
            else
            {
                PieSeries = new ISeries[]
                {
                    new PieSeries<double> { Values = new[] { 1.0 }, Name = "No data",
                        Fill = new SolidColorPaint(SKColor.Parse("#333333")), Stroke = null }
                };
            }

            // Bar chart
            BarSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values      = data.Last7Days.Select(d => Math.Max(0, d.Amt)).ToList(),
                    Name        = "Spending",
                    Fill        = new SolidColorPaint(SKColor.Parse("#00BCD4")),
                    Stroke      = null,
                    MaxBarWidth = 30,
                    Rx = 4, Ry = 4,
                }
            };
            BarXAxes = new Axis[]
            {
                new Axis
                {
                    Labels          = data.Last7Days.Select(d => d.Day).ToList(),
                    TextSize        = 11,
                    LabelsPaint     = new SolidColorPaint(SKColor.Parse("#9E9E9E")),
                    SeparatorsPaint = null,
                    TicksPaint      = null,
                }
            };
            BarYAxes = new Axis[]
            {
                new Axis
                {
                    TextSize        = 11,
                    LabelsPaint     = new SolidColorPaint(SKColor.Parse("#9E9E9E")),
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#2A3F5F")) { StrokeThickness = 1 },
                    Labeler         = v => $"₹{v:0}",
                }
            };
        }
    }
}
