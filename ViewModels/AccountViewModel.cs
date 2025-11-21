using Massiv.Models;
using Massiv.Views;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Effects;

namespace Massiv.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
        private readonly Window _window;
        private readonly MainWindow _mainWindow;
        private string? _login;
        private string? _userName;
        public ICommand CloseCommand { get; }
        public ICommand LogoutCommand { get; }

        public string? Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        public string? UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public AccountViewModel(Window window, MainWindow mainWindow)
        {
            _window = window;
            _mainWindow = mainWindow;
            CloseCommand = new RelayCommand(OnClose);
            LogoutCommand = new RelayCommand(OnLogout);
            LoadCurrentUser();
        }

        private void LoadCurrentUser()
        {
            if (App.CurrentUser != null)
            {
                Login = App.CurrentUser.Login;
            }
        }

        private void OnClose()
        {
            _window.Close();
        }

        private void OnLogout()
        {
            _window.Close();
            App.Logout();
            _mainWindow.ShowLoginDialog();
        }
    }
}