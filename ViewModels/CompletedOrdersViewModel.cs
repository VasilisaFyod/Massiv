using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace Massiv.ViewModels
{
    public class CompletedOrdersViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
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
        public bool IsButtonAddVisible
        {
            get => false;
        }
        public bool IsButtonExportVisible
        {
            get => true;
        }
        public ICommand LoadOrdersCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand RestoreOrderCommand { get; }
        public ICommand BackOrderCommand { get; }
        public ICommand ReadOrderCommand { get; }
        public CompletedOrdersViewModel(MassivContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Orders = new ObservableCollection<Order>();

            LoadOrdersCommand = new RelayCommand(async () => await LoadOrdersAsync());
            ExportToExcelCommand = new RelayCommand(ExportToExcel);
            DeleteOrderCommand = new RelayCommand(DeleteOrder);
            BackOrderCommand = new RelayCommand(BackOrder);
            ReadOrderCommand = new RelayCommand(ReadOrder);
            LoadOrdersCommand.Execute(null);
        }
        private void DeleteOrder()
        {
            if (SelectedOrder == null) return;

            try
            {
                var result = MessageBox.Show(
            $"Вы уверены, что хотите удалить заказ?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var orderToDelete = _context.Orders
            .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

                if (orderToDelete == null)
                {
                    MessageBox.Show("Заказ не найден в базе данных");
                    return;
                }
                orderToDelete.IsDeleted = true;
                _context.SaveChanges();

                LoadOrdersAsync();
                MessageBox.Show("Заказ успешно удален");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }
        private void ReadOrder()
        {
            if (SelectedOrder == null) return;

            try
            {
                var orderToRead = _context.Orders
                    .Include(o => o.WorkshopOrders)
                    .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

                if (orderToRead == null)
                {
                    MessageBox.Show("Заказ не найден в базе данных");
                    return;
                }

                var readContext = MassivContext.GetContext();

                var editPage = new CUD(3, readContext, orderToRead);
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var mainViewModel = mainWindow?.DataContext as MainViewModel;
                mainViewModel?.NavigateToCustomPage(editPage, "Просмотр заказа");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии просмотра: {ex.Message}");
            }
        }
        private void BackOrder()
        {
            if (SelectedOrder == null) return;

            try
            {
                var result = MessageBox.Show(
            $"Вы уверены, что хотите вернуть заказ?",
            "Подтверждение возврата",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var orderToBack = _context.Orders
            .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

                if (orderToBack == null)
                {
                    MessageBox.Show("Заказ не найден в базе данных");
                    return;
                }
                orderToBack.IsCompleted = false;
                _context.SaveChanges();

                LoadOrdersAsync();
                MessageBox.Show("Заказ успешно возвращен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при возвращении: {ex.Message}");
            }
        }
        private async Task LoadOrdersAsync()
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => !o.IsDeleted.Value)
                    .Where(o => o.IsCompleted.Value)
                    .OrderByDescending(o => o.OrderDate)
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
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void FilterOrders()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadOrdersCommand.Execute(null);
                return;
            }

            var filtered = _context.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted.Value && o.IsCompleted.Value &&
                      (o.NumberOrder.Contains(SearchText) ||
                       o.ClientPhone.Contains(SearchText)))
                .ToList();

            Orders = new ObservableCollection<Order>(filtered);
        }

        private void ExportToExcel()
        {
            var exportExcelWindow = new ExportExcel();
            exportExcelWindow.DataContext = new ExportExcelViewModel(exportExcelWindow, _context, 2);
            exportExcelWindow.ShowDialog();
        }
    }
}
