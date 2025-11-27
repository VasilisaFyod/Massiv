using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;
namespace Massiv.ViewModels
{
    public abstract class BaseLogistTablesViewModel : ViewModelBase
    {
        protected readonly MassivContext _context;
        protected readonly string _tableType;
        protected readonly string _currentUserRole;
        private string _searchText;

        public ObservableCollection<LogistTable> LogistTables { get; set; }
        public LogistTable SelectedOrder { get; set; }
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


        public ICommand LoadOrdersCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand RestoreOrderCommand { get; }
        public ICommand BackOrderCommand { get; }
        public ICommand MenuCommand { get; }
        public ICommand ChangeColorCommand { get; }

        public abstract bool IsButtonAddVisible { get; }
        public abstract bool IsButtonExportVisible { get; }

        protected BaseLogistTablesViewModel(MassivContext context, string tableType = null, string userRole = null)
        {
            _context = context;
            _tableType = tableType;
            _currentUserRole = userRole;
            LogistTables = new ObservableCollection<LogistTable>();

            LoadOrdersCommand = new RelayCommand(async () => await LoadDataAsync());
            AddOrderCommand = new RelayCommand(AddOrder);
            ExportToExcelCommand = new RelayCommand(ExportToExcel);
            EditOrderCommand = new RelayCommand(EditOrder);
            DeleteOrderCommand = new RelayCommand(DeleteOrder);
            CompleteOrderCommand = new RelayCommand(CompleteOrder);
            RestoreOrderCommand = new RelayCommand(RestoreOrder);
            BackOrderCommand = new RelayCommand(BackOrder);
            MenuCommand = new RelayCommand(Menu);
            ChangeColorCommand = new RelayCommand<string>(ExecuteChangeColorCommand);

            LoadOrdersCommand.Execute(null);
        }

        protected abstract IQueryable<LogistTable> GetBaseQuery();
        protected abstract int GetExportType();


        protected virtual async Task LoadDataAsync()
        {
            try
            {
                _context.ChangeTracker.Clear();

                var query = GetBaseQuery();

                if (!string.IsNullOrEmpty(_tableType))
                {
                    query = query.Where(lt => lt.TableType == _tableType);
                }

                var tables = await query.ToListAsync();

                ExecuteInUIThread(() =>
                {
                    LogistTables.Clear();
                    foreach (var table in tables)
                    {
                        LogistTables.Add(table);
                    }
                });

                System.Diagnostics.Debug.WriteLine($"Загружено записей: {LogistTables.Count}, TableType: {_tableType}, Role: {_currentUserRole}");
            }
            catch (Exception ex)
            {
                HandleError($"Ошибка загрузки данных: {ex.Message}", "Error loading data");
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
                .Where(lt => lt.OrderNumber.Contains(SearchText) && lt.TableType == _tableType) 
                .ToList();

            ExecuteInUIThread(() =>
            {
                LogistTables.Clear();
                foreach (var table in filtered)
                {
                    LogistTables.Add(table);
                }
            });
        }

        protected virtual void AddOrder()
        {
            var addWindow = new CUDLogistTable(1, null, null, _tableType); 
            NavigateToPage(addWindow, "Добавление логистической таблицы");
        }

        protected virtual void EditOrder()
        {
            if (SelectedOrder == null) return;

            try
            {
                var orderToEdit = _context.LogistTables
                    .FirstOrDefault(lt => lt.LogistTableId == SelectedOrder.LogistTableId);

                if (orderToEdit == null)
                {
                    ShowError("Запись не найдена в базе данных");
                    return;
                }

                var editContext = MassivContext.GetContext();
                var editPage = new CUDLogistTable(2, editContext, orderToEdit);
                NavigateToPage(editPage, "Редактирование логистической таблицы");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при открытии редактирования: {ex.Message}");
            }
        }

        protected virtual void DeleteOrder()
        {
            ExecuteOrderOperation(
                "удалить",
                "Подтверждение удаления",
                table => table.IsDeleted = true,
                "Запись успешно удалена");
        }

        protected virtual void CompleteOrder()
        {
            ExecuteOrderOperation(
                "завершить",
                "Подтверждение завершения",
                table => table.IsCompleted = true,
                "Запись успешно завершена");
        }

        protected virtual void RestoreOrder()
        {
            ExecuteOrderOperation(
                "восстановить",
                "Подтверждение восстановления",
                table => table.IsDeleted = false,
                "Запись успешно восстановлена");
        }

        protected virtual void BackOrder()
        {
            ExecuteOrderOperation(
                "вернуть",
                "Подтверждение возврата",
                table => table.IsCompleted = false,
                "Запись успешно возвращена");
        }

        private void ExecuteOrderOperation(string operationName, string confirmationTitle,
                                         Action<LogistTable> operation, string successMessage)
        {
            if (SelectedOrder == null) return;

            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите {operationName} запись?",
                    confirmationTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                using (var localContext = MassivContext.GetContext())
                {
                    var table = localContext.LogistTables
                        .FirstOrDefault(lt => lt.LogistTableId == SelectedOrder.LogistTableId);

                    if (table == null)
                    {
                        ShowError("Запись не найдена в базе данных");
                        return;
                    }

                    operation(table);
                    localContext.SaveChanges();
                    LoadDataAsync();
                    ShowInfo(successMessage);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при {operationName}: {ex.Message}");
            }
        }

        protected virtual void ExecuteChangeColorCommand(string colorName)
        {
            if (SelectedOrder != null)
            {
                try
                {
                    using (var localContext = MassivContext.GetContext())
                    {
                        var colorHex = colorName switch
                        {
                            "Orange" => "#FFB236",
                            "Red" => "#CB6C6C",
                            "LightBlue" => "#BCF3FF",
                            "Gray" => "#D9D9D9",
                            _ => "#F1BCFF"
                        };

                        var selectedId = SelectedOrder.LogistTableId;

                        var itemToUpdate = localContext.LogistTables
                            .FirstOrDefault(lt => lt.LogistTableId == selectedId);

                        if (itemToUpdate != null)
                        {
                            itemToUpdate.ColorMark = colorHex;
                            localContext.SaveChanges();
                            LoadDataAsync();
                            SelectedOrder = LogistTables.FirstOrDefault(lt => lt.LogistTableId == selectedId);
                            OnPropertyChanged(nameof(SelectedOrder));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка при изменении цвета: {ex.Message}");
                }
            }
        }

        protected virtual void ExportToExcel()
        {
            var exportExcelWindow = new ExportExcel();
            exportExcelWindow.DataContext = new ExportExcelViewModel(exportExcelWindow, _context, GetExportType(), _tableType);
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

        protected void ExecuteInUIThread(Action action)
        {
            Application.Current?.Dispatcher.Invoke(action);
        }

        protected void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected void HandleError(string userMessage, string debugMessage)
        {
            System.Diagnostics.Debug.WriteLine($"{debugMessage}: {userMessage}");
            ShowError(userMessage);
        }
    }
}