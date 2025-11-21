using Massiv.Models;
using Massiv.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class CUDViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
        private bool _isRead;
        private int _choice;
        private Order _currentOrder = new Order();
        private ObservableCollection<WorkshopOrder> _workshopOrders = new ObservableCollection<WorkshopOrder>();

        public ObservableCollection<WorkshopOrder> WorkshopOrders
        {
            get => _workshopOrders;
            set
            {
                _workshopOrders = value;
                OnPropertyChanged(nameof(WorkshopOrders));
            }
        }
        public Order CurrentOrder
        {
            get => _currentOrder;
            set
            {
                _currentOrder = value;
                OnPropertyChanged(nameof(CurrentOrder));
            }
        }
       
        public int Choice
        {
            get => _choice;
            set
            {
                _choice = value;
                OnPropertyChanged(nameof(Choice));
                OnPropertyChanged(nameof(IsRead));
                OnPropertyChanged(nameof(ShowReadyCheckboxes));
                OnPropertyChanged(nameof(IsReadVisible));
            }
        }

        public bool IsRead
        {
            get => _choice == 3; 
            set
            {
                if (_isRead != value)
                {
                    _isRead = value;
                    OnPropertyChanged(nameof(IsRead));
                }
            }
        }
        public bool IsReadVisible => _choice != 3;
        public bool ShowReadyCheckboxes => _choice == 3;
        public bool ShowReadyCheckboxesAndHasOrder0 => ShowReadyCheckboxes && WorkshopOrders.Count > 0 && !string.IsNullOrEmpty(WorkshopOrders[0].NumberWorkshopOrder);
        public bool ShowReadyCheckboxesAndHasOrder1 => ShowReadyCheckboxes && WorkshopOrders.Count > 1 && !string.IsNullOrEmpty(WorkshopOrders[1].NumberWorkshopOrder);
        public bool ShowReadyCheckboxesAndHasOrder2 => ShowReadyCheckboxes && WorkshopOrders.Count > 2 && !string.IsNullOrEmpty(WorkshopOrders[2].NumberWorkshopOrder);
        public bool ShowReadyCheckboxesAndHasOrder3 => ShowReadyCheckboxes && WorkshopOrders.Count > 3 && !string.IsNullOrEmpty(WorkshopOrders[3].NumberWorkshopOrder);
        public bool ShowReadyCheckboxesAndHasOrder4 => ShowReadyCheckboxes && WorkshopOrders.Count > 4 && !string.IsNullOrEmpty(WorkshopOrders[4].NumberWorkshopOrder);
        public bool ShowReadyCheckboxesAndHasOrder5 => ShowReadyCheckboxes && WorkshopOrders.Count > 5 && !string.IsNullOrEmpty(WorkshopOrders[5].NumberWorkshopOrder);
        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }


        public CUDViewModel(int choice, MassivContext context, Order orderToEdit = null)
        {
            _context = context;
            if (orderToEdit != null)
            {
                _context.Entry(orderToEdit).State = EntityState.Detached;

                CurrentOrder = _context.Orders
                    .Include(o => o.WorkshopOrders)
                    .AsNoTracking()
                    .FirstOrDefault(o => o.OrderId == orderToEdit.OrderId);
            }
            else
            {
                CurrentOrder = new Order();
            }

            if (CurrentOrder.WorkshopOrders?.Any() == true)
            {
                foreach (var order in CurrentOrder.WorkshopOrders)
                    WorkshopOrders.Add(order);

                while (WorkshopOrders.Count < 6)
                    WorkshopOrders.Add(new WorkshopOrder());
            }
            else
            {
                for (int i = 0; i < 6; i++)
                    WorkshopOrders.Add(new WorkshopOrder());
            }

            _choice = choice;
            OnPropertyChanged(nameof(ShowReadyCheckboxes));
            OnPropertyChanged(nameof(IsReadVisible));
            SaveCommand = new RelayCommand(SaveOrder);
            BackCommand = new RelayCommand(Back);
        }
        private void SaveOrder()
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (!ValidateOrder())
                {
                    return; 
                }
                CurrentOrder.WorkshopOrders = WorkshopOrders.Where(o => !string.IsNullOrWhiteSpace(o.NumberWorkshopOrder)).ToList();

                if (_choice == 1)
                {
                    CurrentOrder.IsCompleted = false;
                    CurrentOrder.IsDeleted = false;
                    _context.Orders.Add(CurrentOrder);
                }
                else if (_choice == 2)
                {
                    _context.Orders.Update(CurrentOrder);
                }
                else if (_choice == 3)
                {
                    foreach (var order in WorkshopOrders.Where(o => !string.IsNullOrEmpty(o.NumberWorkshopOrder)))
                    {
                        if (order.WorkshopOrderId != 0)
                        {
                            var dbOrder = _context.WorkshopOrders.AsNoTracking().FirstOrDefault(o => o.WorkshopOrderId == order.WorkshopOrderId);
                            if (dbOrder != null)
                            {
                                _context.Entry(dbOrder).State = EntityState.Detached;
                                dbOrder.IsReady = order.IsReady;
                                _context.WorkshopOrders.Update(dbOrder);
                            }
                        }
                    }
                }

                _context.SaveChanges();
                MessageBox.Show("Заказ успешно сохранен");
                Back();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }
        private bool ValidateOrder()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(CurrentOrder.NumberOrder))
            {
                errors.Add("Номер заказа обязателен");
            }
            else if (_choice == 1 && _context.Orders.Any(o => o.NumberOrder == CurrentOrder.NumberOrder))
            {
                errors.Add("Заказ с таким номером уже существует");
            }
            else if (_choice == 2)
            {
                var existing = _context.Orders.AsNoTracking()
                    .FirstOrDefault(o => o.NumberOrder == CurrentOrder.NumberOrder && o.OrderId != CurrentOrder.OrderId);
                if (existing != null)
                {
                    errors.Add("Заказ с таким номером уже существует");
                }
            }

            if (CurrentOrder.SquareMeters.HasValue && CurrentOrder.SquareMeters.Value < 0)
            {
                errors.Add("Площадь не может быть отрицательной");
            }

            if (CurrentOrder.List.HasValue)
            {
                if (CurrentOrder.List.Value < 0)
                {
                    errors.Add("Номер листа не может быть отрицательным");
                }
                else if (CurrentOrder.List.Value > int.MaxValue)
                {
                    errors.Add("Номер листа слишком большой");
                }
            }
            if (CurrentOrder.CompletionDate == default)
            {
                errors.Add("Дата сдачи обязательна");
            }
            if (CurrentOrder.ContractDate.HasValue)
            {
                var contractDateTime = CurrentOrder.ContractDate.Value.ToDateTime(TimeOnly.MinValue);

          

                if (CurrentOrder.CompletionDate.HasValue &&
                    CurrentOrder.CompletionDate.Value < CurrentOrder.ContractDate.Value)
                {
                    errors.Add("Дата сдачи не может быть раньше даты договора");
                }
            }
            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "Ошибки валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
        private void Back()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var mainViewModel = mainWindow?.DataContext as MainViewModel;

            mainViewModel?.GoBack();
        }
    }
}
