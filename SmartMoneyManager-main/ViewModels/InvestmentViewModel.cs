using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace SmartMoneyManager.ViewModels
{
    public class InvestmentViewModel : BaseViewModel
    {
        private double _initial = 1000, _monthly = 200, _rate = 7;
        private int    _years = 10;
        private double _finalVal, _totalInv, _totalGrowth;

        public double InitialAmount  { get => _initial; set { SetProperty(ref _initial, value); Calc(); } }
        public double MonthlyContrib { get => _monthly; set { SetProperty(ref _monthly, value); Calc(); } }
        public double AnnualReturn   { get => _rate;    set { SetProperty(ref _rate, value);    Calc(); } }
        public int    Years          { get => _years;   set { SetProperty(ref _years, value);   Calc(); } }

        public double FinalValue    { get => _finalVal;    set => SetProperty(ref _finalVal, value); }
        public double TotalInvested { get => _totalInv;    set => SetProperty(ref _totalInv, value); }
        public double TotalGrowth   { get => _totalGrowth; set => SetProperty(ref _totalGrowth, value); }

        private ISeries[] _series = Array.Empty<ISeries>();
        private Axis[]    _xAxes  = Array.Empty<Axis>();
        private Axis[]    _yAxes  = Array.Empty<Axis>();

        public ISeries[] ChartSeries { get => _series; private set => SetProperty(ref _series, value); }
        public Axis[]    XAxes       { get => _xAxes;  private set => SetProperty(ref _xAxes, value); }
        public Axis[]    YAxes       { get => _yAxes;  private set => SetProperty(ref _yAxes, value); }

        public ICommand CalcCmd { get; }

        public InvestmentViewModel()
        {
            CalcCmd = new RelayCommand(Calc);
            Calc();
        }

        private void Calc()
        {
            double r = AnnualReturn / 100.0 / 12.0;
            double bal = InitialAmount;
            var inv = new List<double>(); var grw = new List<double>(); var lbl = new List<string>();
            for (int yr = 1; yr <= Math.Max(1, Years); yr++)
            {
                for (int m = 0; m < 12; m++) bal = bal * (1 + r) + MonthlyContrib;
                double i = InitialAmount + MonthlyContrib * 12 * yr;
                inv.Add(i); grw.Add(Math.Max(0, bal - i)); lbl.Add($"Y{yr}");
            }
            TotalInvested = inv.Count > 0 ? inv[^1] : 0;
            TotalGrowth   = grw.Count > 0 ? grw[^1] : 0;
            FinalValue    = TotalInvested + TotalGrowth;

            ChartSeries = new ISeries[]
            {
                new StackedColumnSeries<double>
                {
                    Values = inv, Name = "Invested",
                    Fill = new SolidColorPaint(SKColor.Parse("#0097A7")), Stroke = null,
                    MaxBarWidth = 28, Rx = 2, Ry = 2,
                },
                new StackedColumnSeries<double>
                {
                    Values = grw, Name = "Growth",
                    Fill = new SolidColorPaint(SKColor.Parse("#4CAF50")), Stroke = null,
                    MaxBarWidth = 28, Rx = 2, Ry = 2,
                },
            };
            XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = lbl, TextSize = 11,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#9E9E9E")),
                    SeparatorsPaint = null,
                }
            };
            YAxes = new Axis[]
            {
                new Axis
                {
                    TextSize = 11,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#9E9E9E")),
                    SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#2A3F5F")) { StrokeThickness = 1 },
                    Labeler = v => $"₹{v / 1000:0}k",
                }
            };
        }
    }
}
