using Massiv.Models;
using Massiv.ViewModels;
using Massiv.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace Massiv
{
    public partial class App : Application
    {
        public static User CurrentUser { get; private set; }

        public static void Login(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            using (var scope = _serviceProvider.CreateScope())
            {
                var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
        }
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MassivContext>(options =>
                options.UseSqlServer("Server=DESKTOP-67VCCKN\\SQLEXPRESS;Database=Massiv;Trusted_Connection=True;TrustServerCertificate=True;"),
                ServiceLifetime.Transient);

            services.AddTransient<OrdersViewModel>();
            services.AddTransient<DeletedOrdersViewModel>();
            services.AddTransient<CompletedOrdersViewModel>();

            services.AddTransient<Func<int, Orders>>(provider => choice =>
            {
                var context = provider.GetRequiredService<MassivContext>();
                return new Orders(choice, context);
            });

            services.AddScoped<MainViewModel>();

            services.AddTransient<MainWindow>();
        }
    }
}