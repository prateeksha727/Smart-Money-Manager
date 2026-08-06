using SmartMoneyManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace SmartMoneyManager.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm;
        public event Action? LoginSucceeded;

        public LoginWindow()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            DataContext = _vm;
            _vm.LoginSucceeded += () => { LoginSucceeded?.Invoke(); Close(); };
        }

        private void SyncPasswords()
        {
            _vm.LoginPassword = PbLogin.Password;
            _vm.RegPassword   = PbReg.Password;
            _vm.RegConfirmPwd = PbConfirm.Password;
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
            SyncPasswords();
            _vm.LoginCmd.Execute(null);
        }

        private void RegisterClick(object sender, RoutedEventArgs e)
        {
            SyncPasswords();
            _vm.RegisterCmd.Execute(null);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { SyncPasswords(); _vm.LoginCmd.Execute(null); }
        }
    }
}
