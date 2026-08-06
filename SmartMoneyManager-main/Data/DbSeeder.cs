using SmartMoneyManager.Models;
using System;
using System.Linq;

namespace SmartMoneyManager.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // ── 1. Categories first (everything depends on them) ──────────────
            if (!db.Categories.Any())
            {
                db.Categories.AddRange(
                    new Category { Name = "Food & Dining",  Icon = "🍔", Color = "#FF9800" },
                    new Category { Name = "Transport",      Icon = "🚗", Color = "#2196F3" },
                    new Category { Name = "Entertainment",  Icon = "🎬", Color = "#9C27B0" },
                    new Category { Name = "Shopping",       Icon = "🛍", Color = "#E91E63" },
                    new Category { Name = "Health",         Icon = "💊", Color = "#4CAF50" },
                    new Category { Name = "Utilities",      Icon = "⚡", Color = "#607D8B" },
                    new Category { Name = "Education",      Icon = "📚", Color = "#00BCD4" },
                    new Category { Name = "Travel",         Icon = "✈", Color = "#FF5722" }
                );
                db.SaveChanges();
            }

            // ── 2. Budgets ────────────────────────────────────────────────────
            if (!db.Budgets.Any())
            {
                var now = DateTime.Today;
                var cats = db.Categories.ToDictionary(c => c.Name, c => c.Id);
                db.Budgets.AddRange(
                    new Budget { CategoryId = cats["Food & Dining"],  MonthlyLimit = 400, Month = now.Month, Year = now.Year },
                    new Budget { CategoryId = cats["Transport"],      MonthlyLimit = 200, Month = now.Month, Year = now.Year },
                    new Budget { CategoryId = cats["Entertainment"],  MonthlyLimit = 150, Month = now.Month, Year = now.Year },
                    new Budget { CategoryId = cats["Shopping"],       MonthlyLimit = 300, Month = now.Month, Year = now.Year },
                    new Budget { CategoryId = cats["Health"],         MonthlyLimit = 100, Month = now.Month, Year = now.Year },
                    new Budget { CategoryId = cats["Utilities"],      MonthlyLimit = 250, Month = now.Month, Year = now.Year }
                );
                db.SaveChanges();
            }

            // ── 3. Subscriptions ──────────────────────────────────────────────
            if (!db.Subscriptions.Any())
            {
                var now = DateTime.Today;
                db.Subscriptions.AddRange(
                    new Subscription { Name = "Netflix",         MonthlyCost = 15.99m, NextPaymentDate = now.AddDays(5),  Category = "Entertainment" },
                    new Subscription { Name = "Spotify",         MonthlyCost =  9.99m, NextPaymentDate = now.AddDays(12), Category = "Entertainment" },
                    new Subscription { Name = "Amazon Prime",    MonthlyCost = 14.99m, NextPaymentDate = now.AddDays(20), Category = "Shopping"      },
                    new Subscription { Name = "Gym Membership",  MonthlyCost = 39.99m, NextPaymentDate = now.AddDays(3),  Category = "Health"        },
                    new Subscription { Name = "iCloud Storage",  MonthlyCost =  2.99m, NextPaymentDate = now.AddDays(8),  Category = "Utilities"     },
                    new Subscription { Name = "YouTube Premium", MonthlyCost = 13.99m, NextPaymentDate = now.AddDays(15), Category = "Entertainment" }
                );
                db.SaveChanges();
            }

            // ── 4. Savings Goals ──────────────────────────────────────────────
            if (!db.SavingsGoals.Any())
            {
                var now = DateTime.Today;
                db.SavingsGoals.AddRange(
                    new SavingsGoal { Name = "Emergency Fund",    Icon = "🛡", TargetAmount = 5000,  CurrentAmount = 2150, TargetDate = now.AddMonths(8)  },
                    new SavingsGoal { Name = "Vacation",          Icon = "✈", TargetAmount = 3000,  CurrentAmount =  800, TargetDate = now.AddMonths(10) },
                    new SavingsGoal { Name = "New Laptop",        Icon = "💻", TargetAmount = 1500,  CurrentAmount =  600, TargetDate = now.AddMonths(4)  },
                    new SavingsGoal { Name = "Car Down Payment",  Icon = "🚗", TargetAmount = 10000, CurrentAmount = 3200, TargetDate = now.AddMonths(18) }
                );
                db.SaveChanges();
            }

            // ── 5. Demo user + expenses ───────────────────────────────────────
            if (!db.Users.Any())
            {
                var demo = new User
                {
                    Username     = "demo",
                    PasswordHash = PasswordHelper.Hash("demo1234"),
                    DisplayName  = "Demo User",
                    Email        = "demo@app.com",
                    CreatedAt    = DateTime.UtcNow,
                };
                db.Users.Add(demo);
                db.SaveChanges();
                SeedExpensesForUser(db, demo.Id);
            }
        }

        public static void SeedExpensesForUser(AppDbContext db, int userId)
        {
            if (db.Expenses.Any(e => e.UserId == userId)) return;

            var cats = db.Categories.ToList();
            if (!cats.Any()) return;

            var now = DateTime.Today;
            var rng = new Random(userId * 31 + 7);

            var samples = new[]
            {
                ("Starbucks Coffee",  "Food & Dining",  false),
                ("McDonald's",        "Food & Dining",  false),
                ("Grocery Store",     "Food & Dining",  true),
                ("Pizza Hut",         "Food & Dining",  false),
                ("Uber Ride",         "Transport",      true),
                ("Gas Station",       "Transport",      true),
                ("Parking Fee",       "Transport",      false),
                ("Netflix",           "Entertainment",  false),
                ("Cinema Ticket",     "Entertainment",  false),
                ("Amazon Order",      "Shopping",       false),
                ("Target Run",        "Shopping",       false),
                ("Pharmacy",          "Health",         true),
                ("Gym Membership",    "Health",         true),
                ("Electric Bill",     "Utilities",      true),
                ("Internet Bill",     "Utilities",      true),
                ("Chipotle",          "Food & Dining",  false),
                ("Lyft Ride",         "Transport",      true),
                ("Steam Game",        "Entertainment",  false),
            };

            for (int i = 0; i < 60; i++)
            {
                var (desc, catName, essential) = samples[rng.Next(samples.Length)];
                var cat = cats.FirstOrDefault(c => c.Name == catName) ?? cats[0];
                db.Expenses.Add(new Expense
                {
                    UserId      = userId,
                    CategoryId  = cat.Id,
                    Description = desc,
                    Amount      = Math.Round((decimal)(rng.NextDouble() * 90 + 5), 2),
                    Date        = now.AddDays(-rng.Next(0, 60)),
                    IsEssential = essential,
                });
            }
            db.SaveChanges();
        }
    }
}
