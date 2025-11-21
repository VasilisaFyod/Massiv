using Massiv.Models;
using Massiv.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class EntranceViewModel : ViewModelBase
    {
        private string? _login;
        private readonly string _loginHint = "Введите логин";
        private string? _validationMessage;
        public ICommand LoginCommand { get; }
        public ICommand CloseCommand { get; }

        public string? Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
                OnPropertyChanged(nameof(LoginHint));
                ValidationMessage = string.Empty;
            }
        }

        public string LoginHint => string.IsNullOrWhiteSpace(Login) ? _loginHint : string.Empty;

        public string? ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged(nameof(ValidationMessage));
            }
        }
        public EntranceViewModel()
        {
            LoginCommand = new RelayCommand(async () => await OnLoginAsync());
            CloseCommand = new RelayCommand(OnClose);
        }

        private void OnClose()
        {
            Application.Current.Shutdown();
        }
        private async Task OnLoginAsync()
        {
            try
            {
                ValidationMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(Login))
                {
                    ValidationMessage = "Введите логин";
                    return;
                }

                var user = await Task.Run(async () =>
                {
                    var context = MassivContext.GetContext();
                    return await context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Login == Login);
                });

                if (user == null)
                {
                    ValidationMessage = "Пользователь с таким логином не найден";
                    return;
                }

                App.Login(user);

                var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    mainWindow.Effect = null;
                }

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is EntranceView)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка входа: {ex.Message}";
            }
        }
    }
}
