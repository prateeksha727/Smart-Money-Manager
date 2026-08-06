namespace SmartMoneyManager.Services
{
    /// <summary>
    /// Swap the backend here.
    /// e.g. Current = new HttpFinanceService("https://your-api.com");
    /// </summary>
    public static class ServiceLocator
    {
        public static IFinanceService Current { get; set; } = new LocalFinanceService();
    }
}
