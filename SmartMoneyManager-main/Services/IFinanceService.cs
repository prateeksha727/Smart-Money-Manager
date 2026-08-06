using SmartMoneyManager.Models;
using System;
using System.Collections.Generic;

namespace SmartMoneyManager.Services
{
    public interface IFinanceService
    {
        (bool Ok, string Error, User? User) Login(string username, string password);
        (bool Ok, string Error, User? User) Register(string username, string password, string displayName, string email);

        List<Expense>       GetExpenses(int userId, string? filter = null);
        void                SaveExpense(Expense e, bool isNew);
        void                DeleteExpense(int id);
        List<Category>      GetCategories();

        List<BudgetSummary> GetBudgetSummaries(int userId, int month, int year);
        void                SaveBudget(int categoryId, decimal limit, int month, int year);
        void                DeleteBudget(int budgetId);

        List<Subscription>  GetSubscriptions();
        void                SaveSubscription(Subscription s, bool isNew);
        void                DeleteSubscription(int id);

        List<SavingsGoal>   GetSavingsGoals();
        void                SaveSavingsGoal(SavingsGoal g, bool isNew);
        void                DeleteSavingsGoal(int id);

        DashboardData       GetDashboardData(int userId);
        InsightData         GetInsightData(int userId);
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    public class BudgetSummary
    {
        public int     BudgetId     { get; set; }
        public string  CategoryName { get; set; } = "";
        public string  CategoryIcon { get; set; } = "";
        public double  MonthlyLimit { get; set; }
        public double  Spent        { get; set; }
        public double  PercentUsed  => MonthlyLimit > 0 ? Math.Min(100.0, Spent / MonthlyLimit * 100.0) : 0;
        public bool    IsExceeded   => Spent > MonthlyLimit;
        public string  StatusText   => IsExceeded ? "OVER BUDGET" : $"{PercentUsed:0}% used";
    }

    public class DashboardData
    {
        public double                          TotalSpent         { get; set; }
        public double                          TotalBudget        { get; set; }
        public double                          Remaining          => TotalBudget - TotalSpent;
        public double                          DailyAverage       { get; set; }
        public string                          TopCategory        { get; set; } = "—";
        public List<Expense>                   RecentExpenses     { get; set; } = new();
        public List<(string Name, double Amt)> ByCategory         { get; set; } = new();
        public List<(string Day, double Amt)>  Last7Days          { get; set; } = new();
    }

    public class InsightItem
    {
        public string Icon     { get; set; } = "💡";
        public string Title    { get; set; } = "";
        public string Detail   { get; set; } = "";
        public string Severity { get; set; } = "Info";   // Info | Warning | Alert
    }

    public class MonthComparison
    {
        public string Category  { get; set; } = "";
        public double LastMonth { get; set; }
        public double ThisMonth { get; set; }
        public double Change    => ThisMonth - LastMonth;
        public string ChangeText => Change >= 0 ? $"+₹{Change:0.00}" : $"-₹{Math.Abs(Change):0.00}";
    }

    public class InsightData
    {
        public double                  EssentialTotal      { get; set; }
        public double                  NonEssentialTotal   { get; set; }
        public double                  EssentialPct        { get; set; }
        public double                  NonEssentialPct     { get; set; }
        public List<InsightItem>       Insights            { get; set; } = new();
        public List<MonthComparison>   Comparison          { get; set; } = new();
    }
}
