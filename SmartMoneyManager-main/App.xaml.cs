using SmartMoneyManager.Data;
using SmartMoneyManager.Views;
using System;
using System.IO;
using System.Windows;

namespace SmartMoneyManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitDb();
            ShowLogin();
        }

        private static void InitDb()
        {
            var dbPath = AppDbContext.DbPath;

            // If DB exists but is missing the Users table (old schema) — delete and rebuild
            if (File.Exists(dbPath))
            {
                try
                {
                    using var ctx = new AppDbContext();
                    ctx.Database.EnsureCreated();
                    _ = ctx.Users.Any(); // will throw if Users table missing
                    DbSeeder.Seed(ctx);
                    return;
                }
                catch
                {
                    try { File.Delete(dbPath); } catch { /* ignore */ }
                }
            }

            try
            {
                using var ctx = new AppDbContext();
                ctx.Database.EnsureCreated();
                DbSeeder.Seed(ctx);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database init failed:\n\n{ex.Message}",
                    "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        public void ShowLogin()
        {
            var win = new LoginWindow();
            win.LoginSucceeded += () =>
            {
                var main = new MainWindow();
                main.LogoutRequested += () => { main.Close(); ShowLogin(); };
                main.Show();
            };
            win.Show();
        }
    }
}
