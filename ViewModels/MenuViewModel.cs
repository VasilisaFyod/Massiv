using Massiv.Views;
using System.Windows;
using System.Windows.Input;

namespace Massiv.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public ICommand CloseCommand { get; }
        public ICommand OpenGeneralTableCommand { get; }
        public ICommand OpenLogistTableCommand { get; }

        public MenuViewModel()
        {
            CloseCommand = new RelayCommand(CloseMenu);
            OpenGeneralTableCommand = new RelayCommand(OpenGeneralTable);
            OpenLogistTableCommand = new RelayCommand<string>(OpenLogistTable);
        }

        private void CloseMenu()
        {
            Application.Current.Windows.OfType<MenuView>().FirstOrDefault()?.Close();
        }

        private void OpenGeneralTable()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var mainViewModel = mainWindow?.DataContext as MainViewModel;
            mainViewModel?.OpenGeneralTable();
            CloseMenu();
        }

        private void OpenLogistTable(string tableType)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var mainViewModel = mainWindow?.DataContext as MainViewModel;
            mainViewModel?.OpenLogistTable(tableType);
            CloseMenu();
        }
    }
}
