using Massiv.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class CUDAdminTableViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
        private readonly Window _window;
        private int _choice;
        private User _currentUser;
        public Action OnSaved { get; set; } 

        public ObservableCollection<Role> Roles { get; set; }
        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
                if (CurrentUser != null && value != null)
                {
                    CurrentUser.RoleId = value.RoleId;
                    CurrentUser.Role = value;
                }
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                if (_currentUser?.Role != null)
                {
                    SelectedRole = Roles?.FirstOrDefault(r => r.RoleId == _currentUser.Role.RoleId);
                    OnPropertyChanged(nameof(SelectedRole));
                }
            }
        }

        public int Choice
        {
            get => _choice;
            set
            {
                _choice = value;
                OnPropertyChanged(nameof(Choice));
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand SaveCommand { get; }

        public CUDAdminTableViewModel(Window window, MassivContext context, int choice, User userToEdit = null, Action onSaved = null)
        {
            _window = window;
            _context = context;
            _choice = choice;
            OnSaved = onSaved;

            CloseCommand = new RelayCommand(OnClose);
            SaveCommand = new RelayCommand(OnSave);
            LoadAvailableRoles().ContinueWith(_ =>
            {
                InitializeUser(userToEdit);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task LoadAvailableRoles()
        {
            try
            {
                var roles = await _context.Roles.AsNoTracking().ToListAsync();
                Roles = new ObservableCollection<Role>(roles);
                OnPropertyChanged(nameof(Roles));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки ролей: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private void InitializeUser(User userToEdit)
        {
            if (userToEdit != null)
            {
                LoadUserForEdit(userToEdit.UserId);
            }
            else
            {
                CurrentUser = new User
                {
                    Login = "",
                    Role = Roles?.FirstOrDefault()
                };
                SelectedRole = CurrentUser.Role;
                OnPropertyChanged(nameof(SelectedRole));
            }
        }

        private void LoadUserForEdit(int userId)
        {
            try
            {
                using (var localContext = MassivContext.GetContext())
                {
                    var user = localContext.Users
                        .Include(u => u.Role)
                        .AsNoTracking()
                        .FirstOrDefault(u => u.UserId == userId);

                    if (user != null)
                    {
                        CurrentUser = new User
                        {
                            UserId = user.UserId,
                            Login = user.Login,
                            RoleId = user.RoleId,
                            Role = user.Role
                        };

                        SelectedRole = Roles?.FirstOrDefault(r => r.RoleId == user.RoleId);
                        OnPropertyChanged(nameof(SelectedRole));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }

        private void OnSave()
        {
            try
            {
                if (!ValidateUser())
                    return;

                if (SelectedRole != null)
                {
                    CurrentUser.RoleId = SelectedRole.RoleId;
                }

                using (var transactionContext = MassivContext.GetContext())
                {
                    if (_choice == 1)
                    {
                        var newUser = new User
                        {
                            Login = CurrentUser.Login,
                            RoleId = CurrentUser.RoleId
                        };

                        transactionContext.Users.Add(newUser);
                    }
                    else if (_choice == 2)
                    {
                        var existingUser = transactionContext.Users
                            .FirstOrDefault(u => u.UserId == CurrentUser.UserId);

                        if (existingUser != null)
                        {
                            existingUser.Login = CurrentUser.Login;
                            existingUser.RoleId = CurrentUser.RoleId;
                            transactionContext.Users.Update(existingUser);
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден для редактирования");
                            return;
                        }
                    }

                    transactionContext.SaveChanges();
                    MessageBox.Show("Пользователь успешно сохранен");

                    OnSaved?.Invoke();

                    OnClose();
                }
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool ValidateUser()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(CurrentUser?.Login))
            {
                errors.Add("Логин пользователя обязателен");
            }

            if (SelectedRole == null)
            {
                errors.Add("Роль пользователя обязательна");
            }

            if (!string.IsNullOrWhiteSpace(CurrentUser?.Login))
            {
                using (var checkContext = MassivContext.GetContext())
                {
                    bool loginExists;
                    if (_choice == 1)
                    {
                        loginExists = checkContext.Users.Any(u => u.Login == CurrentUser.Login);
                    }
                    else
                    {
                        loginExists = checkContext.Users.Any(u =>
                            u.Login == CurrentUser.Login && u.UserId != CurrentUser.UserId);
                    }

                    if (loginExists)
                    {
                        errors.Add("Пользователь с таким логином уже существует");
                    }
                }
            }

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибки валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void OnClose()
        {
            _window.Close();
        }
    }
}