using Massiv.Models;
using Massiv.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class CUDLogistTableViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
        private int _choice;
        private string _tableType;
        private LogistTable _currentOrder = new LogistTable();

        public LogistTable CurrentOrder
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
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }


        public CUDLogistTableViewModel(int choice, MassivContext context, LogistTable orderToEdit = null, string tableType = null)
        {
            _context = context;
            _tableType = tableType;
            if (orderToEdit != null)
            {
                _context.Entry(orderToEdit).State = EntityState.Detached;

                CurrentOrder = _context.LogistTables
                    .AsNoTracking()
                    .FirstOrDefault(o => o.LogistTableId == orderToEdit.LogistTableId);
            }
            else
            {
                CurrentOrder = new LogistTable();
                if (!string.IsNullOrEmpty(_tableType) && _choice == 1)
                {
                    CurrentOrder.TableType = _tableType;
                }
            }

            _choice = choice;
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

                if (_choice == 1 && string.IsNullOrEmpty(CurrentOrder.TableType) && !string.IsNullOrEmpty(_tableType))
                {
                    CurrentOrder.TableType = _tableType;
                    CurrentOrder.IsCompleted = false;
                    CurrentOrder.IsDeleted = false;
                    _context.LogistTables.Add(CurrentOrder);
                }
                else if (_choice == 2)
                {
                    _context.LogistTables.Update(CurrentOrder);
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

            if (string.IsNullOrWhiteSpace(CurrentOrder.OrderNumber))
            {
                errors.Add("Номер заказа обязателен");
            }
            else if (_choice == 1 && _context.Orders.Any(o => o.NumberOrder == CurrentOrder.OrderNumber))
            {
                errors.Add("Заказ с таким номером уже существует");
            }
            else if (_choice == 2)
            {
                var existing = _context.LogistTables.AsNoTracking()
                    .FirstOrDefault(o => o.OrderNumber == CurrentOrder.OrderNumber && o.LogistTableId != CurrentOrder.LogistTableId);
                if (existing != null)
                {
                    errors.Add("Заказ с таким номером уже существует");
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
