using Massiv.Models;
using Massiv.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class LogistTablesViewModel : ViewModelBase
    {
        private readonly MassivContext _context;
        private readonly string _tableType;
        public ObservableCollection<LogistTable> LogistTables { get; set; }

        public ICommand MenuCommand { get; }

        public LogistTablesViewModel(MassivContext context, string tableType = null)
        {
            _context = context;
            _tableType = tableType;
            LogistTables = new ObservableCollection<LogistTable>();
            MenuCommand = new RelayCommand(Menu);
            LoadData();
        }

        private void LoadData()
        {
            var query = _context.LogistTables.Where(lt => lt.IsDeleted != true && lt.IsCompleted != true);

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
        }

        private void Menu()
        {
            var menuWindow = new MenuView();
            menuWindow.DataContext = new MenuViewModel();
            menuWindow.ShowDialog();
        }
    }
}