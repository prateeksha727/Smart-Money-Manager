using Microsoft.EntityFrameworkCore;
using SmartMoneyManager.Data;
using SmartMoneyManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartMoneyManager.Services
{
    public class LocalFinanceService : IFinanceService
    {
        // ── Auth ──────────────────────────────────────────────────────────────

        public (bool Ok, string Error, User? User) Login(string username, string password)
        {
            try
            {
                using var db = new AppDbContext();
                var clean = username.Trim().ToLowerInvariant();
                // Pull to memory first — avoids string function translation issues in SQLite EF
                var user = db.Users.AsEnumerable()
                    .FirstOrDefault(u => u.Username.ToLowerInvariant() == clean);
                if (user == null || !PasswordHelper.Verify(password, user.PasswordHash))
                    return (false, "Invalid username or password.", null);
                return (true, "", user);
            }
            catch (Exception ex) { return (false, $"Login error: {ex.Message}", null); }
        }

        public (bool Ok, string Error, User? User) Register(
            string username, string password, string displayName, string email)
        {
            try
            {
                using var db = new AppDbContext();
                var clean = username.Trim();
                if (db.Users.AsEnumerable().Any(u =>
                    u.Username.ToLowerInvariant() == clean.ToLowerInvariant()))
                    return (false, "Username already taken.", null);

                var user = new User
                {
                    Username     = clean,
                    PasswordHash = PasswordHelper.Hash(password),
                    DisplayName  = string.IsNullOrWhiteSpace(displayName) ? clean : displayName.Trim(),
                    Email        = email.Trim(),
                    CreatedAt    = DateTime.UtcNow,
                };
                db.Users.Add(user);
                db.SaveChanges();
                DbSeeder.SeedExpensesForUser(db, user.Id);
                return (true, "", user);
            }
            catch (Exception ex) { return (false, $"Registration failed: {ex.Message}", null); }
        }

        // ── Expenses ──────────────────────────────────────────────────────────

        public List<Expense> GetExpenses(int userId, string? filter = null)
        {
            using var db = new AppDbContext();
            var q = db.Expenses.Include(e => e.Category).Where(e => e.UserId == userId);
            if (!string.IsNullOrWhiteSpace(filter))
                q = q.Where(e => e.Description.Contains(filter));
            return q.OrderByDescending(e => e.Date).ToList();
        }

        public void SaveExpense(Expense expense, bool isNew)
        {
            using var db = new AppDbContext();
            if (isNew) { db.Expenses.Add(expense); }
            else
            {
                var e = db.Expenses.Find(expense.Id);
                if (e == null) return;
                e.Description = expense.Description; e.Amount     = expense.Amount;
                e.Date        = expense.Date;         e.CategoryId = expense.CategoryId;
                e.IsEssential = expense.IsEssential;
            }
            db.SaveChanges();
        }

        public void DeleteExpense(int id)
        {
            using var db = new AppDbContext();
            var e = db.Expenses.Find(id);
            if (e != null) { db.Expenses.Remove(e); db.SaveChanges(); }
        }

        // ── Categories ────────────────────────────────────────────────────────

        public List<Category> GetCategories()
        {
            using var db = new AppDbContext();
            return db.Categories.OrderBy(c => c.Name).ToList();
        }

        // ── Budgets ───────────────────────────────────────────────────────────

        public List<BudgetSummary> GetBudgetSummaries(int userId, int month, int year)
        {
            using var db = new AppDbContext();
            var start = new DateTime(year, month, 1);
            var end   = start.AddMonths(1).AddDays(-1);

            var budgets = db.Budgets.Include(b => b.Category)
                .Where(b => b.Month == month && b.Year == year).ToList();

            // Load to memory then aggregate — no SQLite decimal issues since we use REAL column type
            var spentMap = db.Expenses
                .Where(e => e.UserId == userId && e.Date >= start && e.Date <= end)
                .ToList()  // materialize first
                .GroupBy(e => e.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(e => (double)e.Amount));

            return budgets.Select(b =>
            {
                spentMap.TryGetValue(b.CategoryId, out var spent);
                return new BudgetSummary
                {
                    BudgetId     = b.Id,
                    CategoryName = b.Category?.Name ?? "?",
                    CategoryIcon = b.Category?.Icon ?? "💰",
                    MonthlyLimit = (double)b.MonthlyLimit,
                    Spent        = spent,
                };
            }).ToList();
        }

        public void SaveBudget(int categoryId, decimal limit, int month, int year)
        {
            using var db = new AppDbContext();
            var b = db.Budgets.FirstOrDefault(x =>
                x.CategoryId == categoryId && x.Month == month && x.Year == year);
            if (b != null) b.MonthlyLimit = limit;
            else db.Budgets.Add(new Budget
                { CategoryId = categoryId, MonthlyLimit = limit, Month = month, Year = year });
            db.SaveChanges();
        }

        public void DeleteBudget(int budgetId)
        {
            using var db = new AppDbContext();
            var b = db.Budgets.Find(budgetId);
            if (b != null) { db.Budgets.Remove(b); db.SaveChanges(); }
        }

        // ── Subscriptions ─────────────────────────────────────────────────────

        public List<Subscription> GetSubscriptions()
        {
            using var db = new AppDbContext();
            return db.Subscriptions.Where(s => s.IsActive)
                .OrderBy(s => s.NextPaymentDate).ToList();
        }

        public void SaveSubscription(Subscription s, bool isNew)
        {
            using var db = new AppDbContext();
            if (isNew) { db.Subscriptions.Add(s); }
            else
            {
                var e = db.Subscriptions.Find(s.Id);
                if (e == null) return;
                e.Name = s.Name; e.MonthlyCost = s.MonthlyCost;
                e.NextPaymentDate = s.NextPaymentDate; e.Category = s.Category;
            }
            db.SaveChanges();
        }

        public void DeleteSubscription(int id)
        {
            using var db = new AppDbContext();
            var s = db.Subscriptions.Find(id);
            if (s != null) { db.Subscriptions.Remove(s); db.SaveChanges(); }
        }

        // ── Savings Goals ─────────────────────────────────────────────────────

        public List<SavingsGoal> GetSavingsGoals()
        {
            using var db = new AppDbContext();
            return db.SavingsGoals.OrderBy(g => g.TargetDate).ToList();
        }

        public void SaveSavingsGoal(SavingsGoal g, bool isNew)
        {
            using var db = new AppDbContext();
            if (isNew) { db.SavingsGoals.Add(g); }
            else
            {
                var e = db.SavingsGoals.Find(g.Id);
                if (e == null) return;
                e.Name = g.Name; e.TargetAmount = g.TargetAmount;
                e.CurrentAmount = g.CurrentAmount; e.TargetDate = g.TargetDate; e.Icon = g.Icon;
            }
            db.SaveChanges();
        }

        public void DeleteSavingsGoal(int id)
        {
            using var db = new AppDbContext();
            var g = db.SavingsGoals.Find(id);
            if (g != null) { db.SavingsGoals.Remove(g); db.SaveChanges(); }
        }

        // ── Dashboard ─────────────────────────────────────────────────────────

        public DashboardData GetDashboardData(int userId)
        {
            using var db = new AppDbContext();
            var now   = DateTime.Today;
            var start = new DateTime(now.Year, now.Month, 1);

            // Materialize to memory, then use LINQ-to-Objects — zero SQLite translation issues
            var monthExp = db.Expenses.Include(e => e.Category)
                .Where(e => e.UserId == userId && e.Date >= start && e.Date <= now)
                .ToList();

            var budget = db.Budgets
                .Where(b => b.Month == now.Month && b.Year == now.Year)
                .ToList()
                .Sum(b => (double)b.MonthlyLimit);

            var spent   = monthExp.Sum(e => (double)e.Amount);
            var daily   = now.Day > 0 ? spent / now.Day : 0;

            var topCat = monthExp.GroupBy(e => e.Category?.Name ?? "Other")
                .OrderByDescending(g => g.Sum(e => (double)e.Amount))
                .FirstOrDefault()?.Key ?? "—";

            var recent = db.Expenses.Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date).Take(8).ToList();

            var byCategory = monthExp
                .GroupBy(e => e.Category?.Name ?? "Other")
                .Select(g => (g.Key, g.Sum(e => (double)e.Amount)))
                .OrderByDescending(x => x.Item2).Take(6).ToList();

            var last7 = Enumerable.Range(0, 7).Select(d =>
            {
                var day = now.AddDays(-6 + d);
                return (day.ToString("ddd"),
                        monthExp.Where(e => e.Date.Date == day).Sum(e => (double)e.Amount));
            }).ToList();

            return new DashboardData
            {
                TotalSpent    = spent,
                TotalBudget   = budget,
                DailyAverage  = Math.Round(daily, 2),
                TopCategory   = topCat,
                RecentExpenses = recent,
                ByCategory    = byCategory,
                Last7Days     = last7,
            };
        }

        // ── Insights ──────────────────────────────────────────────────────────

        public InsightData GetInsightData(int userId)
        {
            using var db = new AppDbContext();
            var now       = DateTime.Today;
            var thisStart = new DateTime(now.Year, now.Month, 1);
            var lastStart = thisStart.AddMonths(-1);
            var lastEnd   = thisStart.AddDays(-1);

            var all       = db.Expenses.Include(e => e.Category)
                .Where(e => e.UserId == userId).ToList();
            var thisMonth = all.Where(e => e.Date >= thisStart && e.Date <= now).ToList();
            var lastMonth = all.Where(e => e.Date >= lastStart && e.Date <= lastEnd).ToList();

            var items = new List<InsightItem>();

            // Repeated items
            foreach (var g in thisMonth.GroupBy(e => e.Description, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= 3))
            {
                items.Add(new InsightItem
                {
                    Icon = "🔁", Title = $"Repeated: {g.Key}",
                    Detail = $"Purchased {g.Count()} times — total ₹{g.Sum(e => (double)e.Amount):0.00}",
                    Severity = "Warning"
                });
            }

            // MoM
            var thisTot = thisMonth.Sum(e => (double)e.Amount);
            var lastTot = lastMonth.Sum(e => (double)e.Amount);
            if (lastTot > 0)
            {
                var pct = (thisTot - lastTot) / lastTot * 100;
                items.Add(new InsightItem
                {
                    Icon = pct > 0 ? "📈" : "📉",
                    Title = "Month-over-Month",
                    Detail = $"Spending {(pct > 0 ? "up" : "down")} {Math.Abs(pct):0.0}% vs last month.",
                    Severity = pct > 20 ? "Alert" : "Info"
                });
            }

            // Peak day
            var topDow = thisMonth.GroupBy(e => e.Date.DayOfWeek)
                .OrderByDescending(g => g.Sum(e => (double)e.Amount))
                .FirstOrDefault();
            if (topDow != null)
                items.Add(new InsightItem
                {
                    Icon = "📅", Title = $"Peak Day: {topDow.Key}",
                    Detail = $"Most spending on {topDow.Key}s — ₹{topDow.Sum(e => (double)e.Amount):0.00}",
                    Severity = "Info"
                });

            if (!items.Any())
                items.Add(new InsightItem
                {
                    Icon = "✅", Title = "Looking good!",
                    Detail = "No unusual patterns detected.", Severity = "Info"
                });

            var ess    = thisMonth.Where(e => e.IsEssential). Sum(e => (double)e.Amount);
            var nonEss = thisMonth.Where(e => !e.IsEssential).Sum(e => (double)e.Amount);
            var grand  = ess + nonEss;

            var cats = db.Categories.ToList();
            var comp = cats.Select(c => new MonthComparison
            {
                Category  = $"{c.Icon} {c.Name}",
                ThisMonth = thisMonth.Where(e => e.CategoryId == c.Id).Sum(e => (double)e.Amount),
                LastMonth = lastMonth.Where(e => e.CategoryId == c.Id).Sum(e => (double)e.Amount),
            }).Where(c => c.ThisMonth > 0 || c.LastMonth > 0).ToList();

            return new InsightData
            {
                EssentialTotal    = ess,
                NonEssentialTotal = nonEss,
                EssentialPct      = grand > 0 ? ess    / grand * 100 : 50,
                NonEssentialPct   = grand > 0 ? nonEss / grand * 100 : 50,
                Insights          = items,
                Comparison        = comp,
            };
        }
    }
}
