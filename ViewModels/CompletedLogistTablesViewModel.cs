using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class CompletedLogistTablesViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
        private readonly string _tableType;
        private readonly string _currentUserRole;
        public ObservableCollection<LogistTable> LogistTables { get; set; }
        public ICommand MenuCommand { get; }

        public CompletedLogistTablesViewModel(MassivContext context, string tableType = null, string userRole = null)
        {
            _context = context;
            _tableType = tableType;
            _currentUserRole = userRole;
            LogistTables = new ObservableCollection<LogistTable>();
            MenuCommand = new RelayCommand(Menu);
            LoadData();
        }

        private void LoadData()
        {
            var query = _context.LogistTables.Where(lt => lt.IsCompleted == true && lt.IsDeleted == false);

            // Для логиста фильтруем по TableType, для менеджера показываем все
            if (!string.IsNullOrEmpty(_tableType))
            {
                query = query.Where(lt => lt.TableType == _tableType);
            }

            var tables = query.ToList();

            LogistTables.Clear();
            foreach (var table in tables)
            {
                LogistTables.Add(table);
            }

            // Для отладки - проверьте это в отладчике
            System.Diagnostics.Debug.WriteLine($"Загружено завершенных записей: {LogistTables.Count}");
            System.Diagnostics.Debug.WriteLine($"TableType: {_tableType}, Role: {_currentUserRole}");
        }

        private void Menu()
        {
            var menuWindow = new MenuView();
            menuWindow.DataContext = new MenuViewModel();
            menuWindow.ShowDialog();
        }
    }
}