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
        private readonly Func<int, Orders> _ordersFactory;
        private Stack<MenuItem> _navigationStack = new Stack<MenuItem>();
        public ObservableCollection<MenuItem> MenuItems { get; }

        public ICommand AccountCommand {  get;}

        private MenuItem _currentMenuItem;
        private Page _currentPage;

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
        public MainViewModel(Func<int, Orders> ordersFactory)
        {
            _ordersFactory = ordersFactory;

            MenuItems = new ObservableCollection<MenuItem>
        {
            new MenuItem("Заказы", () => _ordersFactory(1)),
            new MenuItem("Завершенные", () => _ordersFactory(2)),
            new MenuItem("Удаленные", () => _ordersFactory(3))
        };

            CurrentMenuItem = MenuItems.First();
            AccountCommand = new RelayCommand(OnAccount);
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