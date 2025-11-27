using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Massiv.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Func<int, string, Orders> _ordersFactory;
        private readonly Func<int, string, LogistTableView> _logistTablesFactory;
        private readonly MassivContext _dbContext;
        private Stack<MenuItem> _navigationStack = new Stack<MenuItem>();
        public ObservableCollection<MenuItem> MenuItems { get; }

        public ICommand AccountCommand { get; }
        public ICommand OpenGeneralTableCommand { get; }
        public RelayCommand<string> OpenLogistTableCommand { get; }

        private MenuItem _currentMenuItem;
        private Page _currentPage;
        private string _currentLogistTableType;
        private bool _isLogistTableMode;

        public MenuItem CurrentMenuItem
        {
            get => _currentMenuItem;
            set
            {
                _currentMenuItem = value;
                CurrentPage = value?.Page;
                OnPropertyChanged(nameof(CurrentMenuItem));
            }
        }

        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public MainViewModel(Func<int, string, Orders> ordersFactory,
                           Func<int, string, LogistTableView> logistTablesFactory,
                           MassivContext dbContext)
        {
            _ordersFactory = ordersFactory;
            _logistTablesFactory = logistTablesFactory;
            _dbContext = dbContext;

            MenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem("Заказы", () => _ordersFactory(1, null)),
                new MenuItem("Завершенные", () => _ordersFactory(2, null)),
                new MenuItem("Удаленные", () => _ordersFactory(3, null))
            };

            CurrentMenuItem = MenuItems.First();

            AccountCommand = new RelayCommand(OnAccount);
            OpenGeneralTableCommand = new RelayCommand(OpenGeneralTable);
            OpenLogistTableCommand = new RelayCommand<string>(OpenLogistTable);
        }

        public void NavigateToCustomPage(Page page, string title)
        {
            if (CurrentMenuItem != null)
                _navigationStack.Push(CurrentMenuItem);

            var tempMenuItem = new MenuItem(title, () => page);
            CurrentMenuItem = tempMenuItem;
        }

        public void GoBack()
        {
            if (_navigationStack.Count > 0)
            {
                CurrentMenuItem = _navigationStack.Pop();
            }
        }

        public void OpenGeneralTable()
        {
            _currentLogistTableType = null;
            _isLogistTableMode = false;
            UpdateMenuItems();
            CurrentMenuItem = MenuItems.First();
        }

        public void OpenLogistTable(string tableType)
        {
            _currentLogistTableType = tableType;
            _isLogistTableMode = true;
            UpdateMenuItems();
            CurrentMenuItem = MenuItems.First();
        }

        private void UpdateMenuItems()
        {
            MenuItems.Clear();

            if (_isLogistTableMode && !string.IsNullOrEmpty(_currentLogistTableType))
            {
                MenuItems.Add(new MenuItem("Заказы", () => _logistTablesFactory(1, _currentLogistTableType)));
                MenuItems.Add(new MenuItem("Завершенные", () => _logistTablesFactory(2, _currentLogistTableType)));
                MenuItems.Add(new MenuItem("Удаленные", () => _logistTablesFactory(3, _currentLogistTableType)));
            }
            else
            {
                MenuItems.Add(new MenuItem("Заказы", () => _ordersFactory(1, null)));
                MenuItems.Add(new MenuItem("Завершенные", () => _ordersFactory(2, null)));
                MenuItems.Add(new MenuItem("Удаленные", () => _ordersFactory(3, null)));
            }

            OnPropertyChanged(nameof(MenuItems));
        }

        public void OnAccount()
        {
            var accountWindow = new Account();
            var mainWindow = Application.Current.MainWindow as MainWindow;
            accountWindow.DataContext = new AccountViewModel(accountWindow, mainWindow);
            accountWindow.ShowDialog();
        }
    }

    public class MenuItem
    {
        public string Title { get; }
        public Func<Page> PageFactory { get; }
        public Page Page => PageFactory();

        public MenuItem(string title, Func<Page> pageFactory)
        {
            Title = title;
            PageFactory = pageFactory;
        }
    }
}