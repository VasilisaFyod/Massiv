using Massiv.Models;
using Massiv.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class AdminTableViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
        public ObservableCollection<User> Users { get; set; }
        public User SelectedUser { get; set; }

        public ICommand LoadUsersCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public AdminTableViewModel()
        {
            _context = MassivContext.GetContext();
            Users = new ObservableCollection<User>();

            AddUserCommand = new RelayCommand(AddUser);
            LoadUsersCommand = new RelayCommand(async () => await LoadUsers());
            EditUserCommand = new RelayCommand(EditUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);

            LoadUsersCommand.Execute(null);
        }

        public async Task LoadUsers()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var users = await _context.Users
                    .Include(o => o.Role)
                    .ToListAsync();

                ExecuteInUIThread(() =>
                {
                    Users.Clear();
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }
                });

                System.Diagnostics.Debug.WriteLine($"Загружено пользователей: {Users.Count}");
            }
            catch (Exception ex)
            {
                HandleError($"Ошибка загрузки пользователей: {ex.Message}", "Error loading users");
            }
        }

        private void AddUser()
        {
            var addWindow = new CUDAdminTable();
            addWindow.DataContext = new CUDAdminTableViewModel(addWindow, _context, 1, null, () =>
            {
                LoadUsersCommand.Execute(null);
            });
            addWindow.ShowDialog();
        }

        private void EditUser()
        {
            if (SelectedUser == null)
            {
                ShowError("Пользователь не выбран");
                return;
            }

            try
            {
                var userToEdit = _context.Users
                    .FirstOrDefault(u => u.UserId == SelectedUser.UserId);

                if (userToEdit == null)
                {
                    ShowError("Пользователь не найден в базе данных");
                    return;
                }

                var editWindow = new CUDAdminTable();
                editWindow.DataContext = new CUDAdminTableViewModel(editWindow, _context, 2, userToEdit, () =>
                {
                    LoadUsersCommand.Execute(null);
                });
                editWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при открытии редактирования: {ex.Message}");
            }
        }

        private void DeleteUser()
        {
            if (SelectedUser == null)
            {
                ShowError("Пользователь не выбран");
                return;
            }

            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя {SelectedUser.Login}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                using (var localContext = MassivContext.GetContext())
                {
                    var user = localContext.Users
                        .FirstOrDefault(u => u.UserId == SelectedUser.UserId);

                    if (user == null)
                    {
                        ShowError("Пользователь не найден в базе данных");
                        return;
                    }
                    localContext.Users.Remove(user);
                    localContext.SaveChanges();

                    LoadUsersCommand.Execute(null);
                    ShowInfo("Пользователь успешно удален");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при удалении: {ex.Message}");
            }
        }

        private void ExecuteInUIThread(Action action)
        {
            Application.Current?.Dispatcher.Invoke(action);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleError(string userMessage, string debugMessage)
        {
            System.Diagnostics.Debug.WriteLine($"{debugMessage}: {userMessage}");
            ShowError(userMessage);
        }
    }
}