using Massiv.Views;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace Massiv.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public ICommand CloseCommand { get; }
        public ICommand OpenGeneralTableCommand { get; }
        public ICommand OpenLogistTableCommand { get; }

        private string _currentTableType = string.Empty;
        private bool _isLogistTableMode = false;

        public string CurrentTableType
        {
            get => _currentTableType;
            set
            {
                _currentTableType = value;
                OnPropertyChanged(nameof(CurrentTableType));
                OnPropertyChanged(nameof(IsGeneralTableSelected));
                OnPropertyChanged(nameof(IsBelarusSelected));
                OnPropertyChanged(nameof(IsInternalSelected));
                OnPropertyChanged(nameof(IsMoscowSelected));
                OnPropertyChanged(nameof(IsCorpusSelected));
                OnPropertyChanged(nameof(IsSpSelected));
                OnPropertyChanged(nameof(IsReclamationsSelected));
            }
        }

        public bool IsLogistTableMode
        {
            get => _isLogistTableMode;
            set
            {
                _isLogistTableMode = value;
                OnPropertyChanged(nameof(IsLogistTableMode));
                OnPropertyChanged(nameof(IsGeneralTableSelected));
            }
        }

        // Свойства для привязки стилей кнопок
        public bool IsGeneralTableSelected => !IsLogistTableMode;
        public bool IsBelarusSelected => IsLogistTableMode && CurrentTableType == "b";
        public bool IsInternalSelected => IsLogistTableMode && CurrentTableType == "i";
        public bool IsMoscowSelected => IsLogistTableMode && CurrentTableType == "m";
        public bool IsCorpusSelected => IsLogistTableMode && CurrentTableType == "c";
        public bool IsSpSelected => IsLogistTableMode && CurrentTableType == "sp";
        public bool IsReclamationsSelected => IsLogistTableMode && CurrentTableType == "r";

        public MenuViewModel()
        {
            CloseCommand = new RelayCommand(CloseMenu);
            OpenGeneralTableCommand = new RelayCommand(OpenGeneralTable);
            OpenLogistTableCommand = new RelayCommand<string>(OpenLogistTable);

            // Подписываемся на событие изменения состояния в MainViewModel
            if (Application.Current.MainWindow?.DataContext is MainViewModel mainViewModel)
            {
                // Обновляем состояние при открытии меню
                UpdateFromMainViewModel(mainViewModel);

                // Подписываемся на изменения
                mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            }
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is MainViewModel mainViewModel)
            {
                UpdateFromMainViewModel(mainViewModel);
            }
        }

        private void UpdateFromMainViewModel(MainViewModel mainViewModel)
        {
            // Получаем текущее состояние через рефлексию или публичные методы
            try
            {
                var type = mainViewModel.GetType();
                var logistTableModeField = type.GetField("_isLogistTableMode",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var currentTableTypeField = type.GetField("_currentLogistTableType",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (logistTableModeField != null && currentTableTypeField != null)
                {
                    IsLogistTableMode = (bool)logistTableModeField.GetValue(mainViewModel);
                    CurrentTableType = (string)currentTableTypeField.GetValue(mainViewModel) ?? string.Empty;
                }
            }
            catch
            {
                // Альтернативный способ - использовать публичные свойства в MainViewModel
                IsLogistTableMode = false;
                CurrentTableType = string.Empty;
            }
        }

        private void CloseMenu()
        {
            // Отписываемся от событий
            if (Application.Current.MainWindow?.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
            }

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