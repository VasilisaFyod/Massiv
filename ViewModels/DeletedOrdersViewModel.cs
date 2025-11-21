using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace Massiv.ViewModels
{
    public class DeletedOrdersViewModel : ViewModelBase
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
        public ICommand LoadOrdersCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand RestoreOrderCommand { get; }
        public ICommand BackOrderCommand { get; }
        public ICommand ReadOrderCommand { get; }
        public bool IsButtonAddVisible
        {
            get => false;
        }
        public bool IsButtonExportVisible
        {
            get => false;
        }
        public DeletedOrdersViewModel(MassivContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Orders = new ObservableCollection<Order>();

            LoadOrdersCommand = new RelayCommand(async () => await LoadOrdersAsync());
            RestoreOrderCommand = new RelayCommand(RestoreOrder);
            LoadOrdersCommand.Execute(null);
        }
        private void RestoreOrder()
        {
            if (SelectedOrder == null) return;

            try
            {
                var result = MessageBox.Show(
            $"Вы уверены, что хотите восстановить заказ?",
            "Подтверждение восстановления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                var orderToRestore = _context.Orders
            .FirstOrDefault(o => o.OrderId == SelectedOrder.OrderId);

                if (orderToRestore == null)
                {
                    MessageBox.Show("Заказ не найден в базе данных");
                    return;
                }
                orderToRestore.IsDeleted = false;
                _context.SaveChanges();

                LoadOrdersAsync();
                MessageBox.Show("Заказ успешно восстановлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при восстановлении: {ex.Message}");
            }
        }
        private async Task LoadOrdersAsync()
        {
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.IsDeleted.Value)
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
                .Where(o => o.IsDeleted.Value &&
                      (o.NumberOrder.Contains(SearchText) ||
                       o.ClientPhone.Contains(SearchText)))
                .ToList();

            Orders = new ObservableCollection<Order>(filtered);
        }
    }
}
