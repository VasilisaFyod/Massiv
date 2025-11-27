using Massiv.Models;
using Massiv.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls; 

namespace Massiv.ViewModels
{
    public abstract class BaseOrdersViewModel : ViewModelBase
    {
        protected readonly MassivContext _context;
        private ObservableCollection<Order> _orders;
        private string _searchText;
        private Order _selectedOrder;

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterOrders();
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }

        public abstract bool IsButtonAddVisible { get; }
        public abstract bool IsButtonExportVisible { get; }
        protected abstract IQueryable<Order> GetBaseQuery();
        protected abstract int GetExportType();

        public ICommand LoadOrdersCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand RestoreOrderCommand { get; }
        public ICommand BackOrderCommand { get; }
        public ICommand ReadOrderCommand { get; }
        public ICommand MenuCommand { get; }



        protected BaseOrdersViewModel(MassivContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Orders = new ObservableCollection<Order>();

            LoadOrdersCommand = new RelayCommand(async () => await LoadOrdersAsync());
            AddOrderCommand = new RelayCommand(AddOrder);
            ExportToExcelCommand = new RelayCommand(ExportToExcel);
            EditOrderCommand = new RelayCommand(EditOrder);
            DeleteOrderCommand = new RelayCommand(DeleteOrder);
            CompleteOrderCommand = new RelayCommand(CompleteOrder);
            RestoreOrderCommand = new RelayCommand(RestoreOrder);
            BackOrderCommand = new RelayCommand(BackOrder);
            ReadOrderCommand = new RelayCommand(ReadOrder);
            MenuCommand = new RelayCommand(Menu);

            LoadOrdersCommand.Execute(null);
        }

        protected virtual async Task LoadOrdersAsync()
        {
            try
            {
                var orders = await GetBaseQuery()
                    .Include(o => o.WorkshopOrders)
                    .OrderBy(o => o.CompletionDate)
                    .AsNoTracking()
                    .ToListAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Orders.Clear();
                    foreach (var order in orders)
                    {
                        Orders.Add(order);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        protected virtual void FilterOrders()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadOrdersCommand.Execute(null);
                return;
            }

            var filtered = GetBaseQuery()
                .AsNoTracking()
                .Where(o => o.NumberOrder.Contains(SearchText) ||
                           o.ClientPhone.Contains(SearchText))
                .ToList();

            Orders = new ObservableCollection<Order>(filtered);
        }

        protected void EditOrder()
        {
            if (SelectedOrder == null) return;

            try
            {
                var orderToEdit = _context.Orders
                    .Include(o => o.WorkshopOrders)
                    .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

                if (orderToEdit == null)
                {
                    ShowError("Заказ не найден в базе данных");
                    return;
                }

                var editContext = MassivContext.GetContext();
                var editPage = new CUD(2, editContext, orderToEdit);
                NavigateToPage(editPage, "Редактирование заказа");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при открытии редактирования: {ex.Message}");
            }
        }

        protected void ReadOrder()
        {
            if (SelectedOrder == null) return;

            try
            {
                var orderToRead = _context.Orders
                    .Include(o => o.WorkshopOrders)
                    .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

                if (orderToRead == null)
                {
                    ShowError("Заказ не найден в базе данных");
                    return;
                }

                var readContext = MassivContext.GetContext();
                var editPage = new CUD(3, readContext, orderToRead);
                NavigateToPage(editPage, "Просмотр заказа");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при открытии просмотра: {ex.Message}");
            }
        }

        protected void DeleteOrder()
        {
            ExecuteOrderOperation(
                "удалить",
                "Подтверждение удаления",
                order => order.IsDeleted = true,
                "Заказ успешно удален");
        }

        protected void CompleteOrder()
        {
            ExecuteOrderOperation(
                "завершить",
                "Подтверждение завершения",
                order => order.IsCompleted = true,
                "Заказ успешно завершен");
        }

        protected void RestoreOrder()
        {
            ExecuteOrderOperation(
                "восстановить",
                "Подтверждение восстановления",
                order => order.IsDeleted = false,
                "Заказ успешно восстановлен");
        }

        protected void BackOrder()
        {
            ExecuteOrderOperation(
                "вернуть",
                "Подтверждение возврата",
                order => order.IsCompleted = false,
                "Заказ успешно возвращен");
        }

        private void ExecuteOrderOperation(string operationName, string confirmationTitle, 
                                         Action<Order> operation, string successMessage)
        {
            if (SelectedOrder == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите {operationName} заказ?",
                    confirmationTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                var order = _context.Orders.FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);
                if (order == null)
                {
                    ShowError("Заказ не найден в базе данных");
                    return;
                }

                operation(order);
                _context.SaveChanges();
                LoadOrdersAsync();
                ShowInfo(successMessage);
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при {operationName}: {ex.Message}");
            }
        }

        protected virtual void AddOrder()
        {
            var addOrderWindow = new CUD(1);
            NavigateToPage(addOrderWindow, "Добавление заказа");
        }

        protected virtual void ExportToExcel()
        {
            var exportExcelWindow = new ExportExcel();
            exportExcelWindow.DataContext = new ExportExcelViewModel(exportExcelWindow, _context, GetExportType());
            exportExcelWindow.ShowDialog();
        }

        protected virtual void Menu()
        {
            var menuWindow = new MenuView();
            menuWindow.DataContext = new MenuViewModel();
            menuWindow.ShowDialog();
        }

        protected void NavigateToPage(Page page, string title)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var mainViewModel = mainWindow?.DataContext as MainViewModel;
            mainViewModel?.NavigateToCustomPage(page, title);
        }

        protected void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}