using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMoneyManager.Models
{
    public class User
    {
        [Key] public int Id { get; set; }
        [Required] public string Username { get; set; } = "";
        [Required] public string PasswordHash { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }

    public class Category
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; } = "";
        public string Icon { get; set; } = "💰";
        public string Color { get; set; } = "#00BCD4";
    }

    public class Expense
    {
        [Key] public int Id { get; set; }
        [Required] public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        public bool IsEssential { get; set; } = true;
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }

    public class Budget
    {
        [Key] public int Id { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }
        public decimal MonthlyLimit { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class Subscription
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; } = "";
        public decimal MonthlyCost { get; set; }
        public DateTime NextPaymentDate { get; set; } = DateTime.Today.AddMonths(1);
        public string Category { get; set; } = "Entertainment";
        public bool IsActive { get; set; } = true;
    }

    public class SavingsGoal
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; } = "";
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; } = DateTime.Today.AddYears(1);
        public string Icon { get; set; } = "🎯";

        [NotMapped]
        public double ProgressPercent =>
            TargetAmount > 0 ? Math.Min(100.0, (double)(CurrentAmount / TargetAmount) * 100.0) : 0;

        [NotMapped]
        public int MonthsRemaining =>
            Math.Max(1, (int)((TargetDate - DateTime.Today).TotalDays / 30));

        [NotMapped]
        public decimal SuggestedMonthlySaving =>
            MonthsRemaining > 0 ? Math.Max(0m, (TargetAmount - CurrentAmount) / MonthsRemaining) : 0m;
    }
}
