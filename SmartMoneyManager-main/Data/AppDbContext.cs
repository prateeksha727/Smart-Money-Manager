using Microsoft.EntityFrameworkCore;
using SmartMoneyManager.Models;
using System;
using System.IO;

namespace SmartMoneyManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User>        Users         { get; set; } = null!;
        public DbSet<Category>    Categories    { get; set; } = null!;
        public DbSet<Expense>     Expenses      { get; set; } = null!;
        public DbSet<Budget>      Budgets       { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<SavingsGoal> SavingsGoals  { get; set; } = null!;

        public static string DbPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SmartMoneyManager", "smartmoney.db");

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
            o.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder m)
        {
            // Store decimals as REAL (double) in SQLite — avoids ALL Sum/Avg translation errors
            m.Entity<Expense>()     .Property(e => e.Amount)       .HasColumnType("REAL");
            m.Entity<Budget>()      .Property(b => b.MonthlyLimit) .HasColumnType("REAL");
            m.Entity<Subscription>().Property(s => s.MonthlyCost)  .HasColumnType("REAL");
            m.Entity<SavingsGoal>() .Property(g => g.TargetAmount) .HasColumnType("REAL");
            m.Entity<SavingsGoal>() .Property(g => g.CurrentAmount).HasColumnType("REAL");

            m.Entity<User>().HasIndex(u => u.Username).IsUnique();
        }
    }
}
