using SmartMoneyManager.Data;
using SmartMoneyManager.Services;
using System;
using System.Windows.Input;

namespace SmartMoneyManager.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IFinanceService _svc = ServiceLocator.Current;

        private string _loginUser = "", _loginErr = "";
        private string _regUser = "", _regName = "", _regEmail = "", _regErr = "";
        private bool   _showReg;

        public string LoginUsername { get => _loginUser; set => SetProperty(ref _loginUser, value); }
        public string LoginError    { get => _loginErr;  set => SetProperty(ref _loginErr,  value); }
        public string RegUsername   { get => _regUser;   set => SetProperty(ref _regUser,   value); }
        public string RegName       { get => _regName;   set => SetProperty(ref _regName,   value); }
        public string RegEmail      { get => _regEmail;  set => SetProperty(ref _regEmail,  value); }
        public string RegError      { get => _regErr;    set => SetProperty(ref _regErr,    value); }
        public bool   ShowRegister  { get => _showReg;   set => SetProperty(ref _showReg,   value); }

        // Filled by code-behind (PasswordBox can't bind directly)
        public string LoginPassword    { get; set; } = "";
        public string RegPassword      { get; set; } = "";
        public string RegConfirmPwd    { get; set; } = "";

        public ICommand LoginCmd       { get; }
        public ICommand RegisterCmd    { get; }
        public ICommand GoRegisterCmd  { get; }
        public ICommand GoLoginCmd     { get; }

        public event Action? LoginSucceeded;

        public LoginViewModel()
        {
            LoginCmd      = new RelayCommand(DoLogin);
            RegisterCmd   = new RelayCommand(DoRegister);
            GoRegisterCmd = new RelayCommand(() => { ShowRegister = true;  RegError = ""; LoginError = ""; });
            GoLoginCmd    = new RelayCommand(() => { ShowRegister = false; RegError = ""; LoginError = ""; });
        }

        private void DoLogin()
        {
            LoginError = "";
            if (string.IsNullOrWhiteSpace(LoginUsername)) { LoginError = "Enter your username."; return; }
            if (string.IsNullOrWhiteSpace(LoginPassword)) { LoginError = "Enter your password."; return; }

            var (ok, err, user) = _svc.Login(LoginUsername.Trim(), LoginPassword);
            if (!ok) { LoginError = err; return; }

            SessionManager.Login(user!);
            LoginSucceeded?.Invoke();
        }

        private void DoRegister()
        {
            RegError = "";
            if (string.IsNullOrWhiteSpace(RegUsername))    { RegError = "Username is required.";         return; }
            if (string.IsNullOrWhiteSpace(RegPassword))    { RegError = "Password is required.";         return; }
            if (RegPassword.Length < 4)                    { RegError = "Password needs 4+ characters."; return; }
            if (RegPassword != RegConfirmPwd)              { RegError = "Passwords do not match.";       return; }

            var (ok, err, user) = _svc.Register(
                RegUsername.Trim(), RegPassword, RegName.Trim(), RegEmail.Trim());
            if (!ok) { RegError = err; return; }

            SessionManager.Login(user!);
            LoginSucceeded?.Invoke();
        }
    }
}
