using Massiv.Models;
using Massiv.Services;
using Massiv.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Effects;

namespace Massiv.Views
{
    public partial class MainWindow : Window
    {
        private readonly MassivContext _dbContext;
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _dbContext = new MassivContext();

            Func<int, string, Orders> ordersFactory = (choice, tableType) => new Orders(choice, _dbContext);
            Func<int, string, LogistTableView> logistTablesFactory = (choice, tableType) => new LogistTableView(choice, _dbContext, tableType);
            Func<AdminTable> adminTable = () => new AdminTable();
            Func<AdminChartsView> adminChartsView = () => new AdminChartsView();

            _viewModel = new MainViewModel(ordersFactory, logistTablesFactory, adminTable, adminChartsView, _dbContext);
            DataContext = _viewModel;

            Closed += (s, e) => _dbContext.Dispose();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ShowLoginDialog();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        public void ShowLoginDialog()
        {
            this.Effect = new BlurEffect { Radius = 10 };
            var loginWindow = new EntranceView { Owner = this };

            loginWindow.Closed += (s, e) =>
            {
                this.Effect = null;

                if (_viewModel != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _viewModel.ForceRefresh();

                        CommandManager.InvalidateRequerySuggested();
                    }), System.Windows.Threading.DispatcherPriority.Render);
                }
            };

            loginWindow.ShowDialog();
        }
    }
}