using SmartMoneyManager.ViewModels;
using System;
using System.Windows;

namespace SmartMoneyManager
{
    public partial class MainWindow : Window
    {
        public event Action? LogoutRequested;

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainWindowViewModel();
            vm.LogoutRequested += () => LogoutRequested?.Invoke();
            DataContext = vm;
        }

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }
    }
}
