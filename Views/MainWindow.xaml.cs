using DocumentFormat.OpenXml.Bibliography;
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

            Func<int, Orders> ordersFactory = choice => new Orders(choice, _dbContext);

            DataContext = new MainViewModel(ordersFactory);

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

            var loginWindow = new EntranceView
            {
                Owner = this
            };
            loginWindow.ShowDialog();
        }
        
    }
}