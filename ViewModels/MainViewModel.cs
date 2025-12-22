using Massiv.Models;
using Massiv.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Massiv.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Func<int, string, Orders> _ordersFactory;
        private readonly Func<int, string, LogistTableView> _logistTablesFactory;
        private readonly Func<AdminTable> _adminTableFactory;
        private readonly Func<AdminChartsView> _adminChartsFactory;

        private readonly MassivContext _dbContext;
        private Stack<MenuItem> _navigationStack = new Stack<MenuItem>();
        private ObservableCollection<MenuItem> _menuItems;

        public ObservableCollection<MenuItem> MenuItems
        {
            get => _menuItems;
            set
            {
                _menuItems = value;
                OnPropertyChanged(nameof(MenuItems));
            }
        }

        public ICommand AccountCommand { get; }
        public ICommand OpenGeneralTableCommand { get; }
        public RelayCommand<string> OpenLogistTableCommand { get; }

        private MenuItem _currentMenuItem;
        private Page _currentPage;
        private string _currentLogistTableType = string.Empty;
        private bool _isLogistTableMode = false;

        public string CurrentLogistTableType => _currentLogistTableType;
        public bool IsLogistTableMode => _isLogistTableMode;

        private User _currentUser;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged(nameof(CurrentUser));
                    OnPropertyChanged(nameof(CurrentUserRole));

                    UpdateRoleImmediately();
                }
            }
        }

        public string CurrentUserRole => CurrentUser?.Role?.Name ?? "Не авторизован";

        public MenuItem CurrentMenuItem
        {
            get => _currentMenuItem;
            set
            {
                if (_currentMenuItem != value)
                {
                    _currentMenuItem = value;

                    if (_currentMenuItem != null && _currentMenuItem.PageFactory != null)
                    {
                        var newPage = _currentMenuItem.PageFactory.Invoke();
                        if (newPage != null)
                        {
                            CurrentPage = newPage;
                        }
                    }

                    OnPropertyChanged(nameof(CurrentMenuItem));
                }
            }
        }

        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged(nameof(CurrentPage));

                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        CommandManager.InvalidateRequerySuggested();
                        OnPropertyChanged(nameof(CurrentPage));
                    }, DispatcherPriority.Render);
                }
            }
        }

        public MainViewModel(Func<int, string, Orders> ordersFactory,
                           Func<int, string, LogistTableView> logistTablesFactory,
                           Func<AdminTable> adminTableFactory,
                   Func<AdminChartsView> adminChartsFactory,
                           MassivContext dbContext)
        {
            _ordersFactory = ordersFactory;
            _logistTablesFactory = logistTablesFactory;
            _adminTableFactory = adminTableFactory;
            _adminChartsFactory = adminChartsFactory;
            _dbContext = dbContext;

            MenuItems = new ObservableCollection<MenuItem>();

            AccountCommand = new RelayCommand(OnAccount);
            OpenGeneralTableCommand = new RelayCommand(OpenGeneralTable);
            OpenLogistTableCommand = new RelayCommand<string>(OpenLogistTable);

            App.CurrentUserChanged += OnCurrentUserChanged;

            if (App.CurrentUser != null)
            {
                CurrentUser = App.CurrentUser;
            }
        }

        private void OnCurrentUserChanged(object sender, EventArgs e)
        {
            if (App.CurrentUser != null)
            {
                var user = _dbContext.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.UserId == App.CurrentUser.UserId);

                if (user != null)
                {
                    ResetLogistTableState();
                    CurrentUser = user;
                }
            }
            else
            {
                ResetLogistTableState();
                CurrentUser = null;
            }
        }

        private void ResetLogistTableState()
        {
            _isLogistTableMode = false;
            _currentLogistTableType = string.Empty;
            _navigationStack.Clear(); 
        }


        private void UpdateRoleImmediately()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                try
                {
                    _navigationStack.Clear();

                    if (CurrentUserRole == "Администратор" || CurrentUserRole == "Логист" || CurrentUserRole == "Менеджер")
                    {
                        
                        UpdateMenuItemsImmediately();

                        if (MenuItems.Any())
                        {
                            var firstItem = MenuItems.First();

                            var firstPage = firstItem.PageFactory?.Invoke();
                            if (firstPage != null)
                            {
                                CurrentPage = firstPage;
                            }
                            _currentMenuItem = firstItem;
                            OnPropertyChanged(nameof(CurrentMenuItem));
                        }
                    }
                    else
                    {
                        MenuItems.Clear();
                        CurrentPage = null;
                        _currentMenuItem = null;
                        OnPropertyChanged(nameof(CurrentMenuItem));
                    }

                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        OnPropertyChanged(nameof(MenuItems));
                        CommandManager.InvalidateRequerySuggested();
                    }, DispatcherPriority.Render);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении роли: {ex.Message}");
                }
            }, DispatcherPriority.Normal);
        }

        private void UpdateMenuItemsImmediately()
        {
            var newMenuItems = new ObservableCollection<MenuItem>();

            if(CurrentUserRole == "Администратор")
            {
                newMenuItems.Add(new MenuItem("Пользователи", () => _adminTableFactory()));
                newMenuItems.Add(new MenuItem("Статистика", () => _adminChartsFactory()));
            }
            else
            {
                if (_isLogistTableMode && !string.IsNullOrEmpty(_currentLogistTableType))
                {
                    newMenuItems.Add(new MenuItem("Заказы", () => _logistTablesFactory(1, _currentLogistTableType)));
                    newMenuItems.Add(new MenuItem("Завершенные", () => _logistTablesFactory(2, _currentLogistTableType)));
                    newMenuItems.Add(new MenuItem("Удаленные", () => _logistTablesFactory(3, _currentLogistTableType)));
                }
                else
                {
                    newMenuItems.Add(new MenuItem("Заказы", () => _ordersFactory(1, null)));
                    newMenuItems.Add(new MenuItem("Завершенные", () => _ordersFactory(2, null)));
                    newMenuItems.Add(new MenuItem("Удаленные", () => _ordersFactory(3, null)));
                }
            }

            MenuItems = newMenuItems;
        }

        private void UpdateMenuItems()
        {
            var newMenuItems = new ObservableCollection<MenuItem>();


            if (CurrentUserRole == "Администратор")
            {
                newMenuItems.Add(new MenuItem("Пользователи", () => _adminTableFactory()));
                newMenuItems.Add(new MenuItem("Статистика", () => _adminChartsFactory()));
            }
            else
            {
                if (_isLogistTableMode && !string.IsNullOrEmpty(_currentLogistTableType))
                {
                    newMenuItems.Add(new MenuItem("Заказы", () => _logistTablesFactory(1, _currentLogistTableType)));
                    newMenuItems.Add(new MenuItem("Завершенные", () => _logistTablesFactory(2, _currentLogistTableType)));
                    newMenuItems.Add(new MenuItem("Удаленные", () => _logistTablesFactory(3, _currentLogistTableType)));
                }
                else
                {
                    newMenuItems.Add(new MenuItem("Заказы", () => _ordersFactory(1, null)));
                    newMenuItems.Add(new MenuItem("Завершенные", () => _ordersFactory(2, null)));
                    newMenuItems.Add(new MenuItem("Удаленные", () => _ordersFactory(3, null)));
                }
            }

            MenuItems = newMenuItems;

            if (MenuItems.Any())
            {
                var firstItem = MenuItems.First();
                var firstPage = firstItem.PageFactory?.Invoke();
                if (firstPage != null)
                {
                    CurrentPage = firstPage;
                }
                _currentMenuItem = firstItem;
                OnPropertyChanged(nameof(CurrentMenuItem));
            }
        }

        public void NavigateToCustomPage(Page page, string title)
        {
            if (CurrentMenuItem != null)
                _navigationStack.Push(CurrentMenuItem);

            var tempMenuItem = new MenuItem(title, () => page);
            CurrentPage = page;
            _currentMenuItem = tempMenuItem;
            OnPropertyChanged(nameof(CurrentMenuItem));
        }

        public void GoBack()
        {
            if (_navigationStack.Count > 0)
            {
                var previousItem = _navigationStack.Pop();
                if (previousItem != null)
                {
                    var previousPage = previousItem.PageFactory?.Invoke();
                    if (previousPage != null)
                    {
                        CurrentPage = previousPage;
                    }
                    _currentMenuItem = previousItem;
                    OnPropertyChanged(nameof(CurrentMenuItem));
                }
            }
        }

        public void OpenGeneralTable()
        {
            _currentLogistTableType = string.Empty;
            _isLogistTableMode = false;
            UpdateMenuItems();

            // Уведомляем об изменении
            OnPropertyChanged(nameof(CurrentLogistTableType));
            OnPropertyChanged(nameof(IsLogistTableMode));
        }

        public void OpenLogistTable(string tableType)
        {
            if (string.IsNullOrEmpty(tableType))
                return;

            _currentLogistTableType = tableType;
            _isLogistTableMode = true;
            UpdateMenuItems();

            // Уведомляем об изменении
            OnPropertyChanged(nameof(CurrentLogistTableType));
            OnPropertyChanged(nameof(IsLogistTableMode));
        }
        public void OnAccount()
        {
            var accountWindow = new Account();
            var accountViewModel = new AccountViewModel(accountWindow, Application.Current.MainWindow as MainWindow);
            accountWindow.DataContext = accountViewModel;

            accountWindow.ShowDialog();
        }

        public void ForceRefresh()
        {
            UpdateRoleImmediately();
        }
    }

    public class MenuItem
    {
        public string Title { get; }
        public Func<Page> PageFactory { get; }

        public MenuItem(string title, Func<Page> pageFactory)
        {
            Title = title;
            PageFactory = pageFactory;
        }
    }
}