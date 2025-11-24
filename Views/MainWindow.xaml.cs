using Massiv.Models;
using Massiv.Services;
using Massiv.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Effects;

namespace Massiv.Views
{
    public partial class MainWindow : Window
    {
        private readonly MassivContext _dbContext;
        public MainWindow()
        {
            InitializeComponent();

            _dbContext = new MassivContext();

            Func<int, string, Orders> ordersFactory = (choice, tableType) => new Orders(choice, _dbContext);

            Func<int, string, LogistTableView> logistTablesFactory = (choice, tableType) => new LogistTableView(choice, _dbContext, tableType);

            DataContext = new MainViewModel(ordersFactory, logistTablesFactory, _dbContext);

            this.Closed += (s, e) => _dbContext.Dispose();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            ShowLoginDialog();
        }

        public void ShowLoginDialog()
        {
            this.Effect = new BlurEffect { Radius = 10 };
            var loginWindow = new EntranceView { Owner = this };
            loginWindow.ShowDialog();
        }
    }
}