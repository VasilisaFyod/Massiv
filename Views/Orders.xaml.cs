using Massiv.Models;
using Massiv.ViewModels;
using System.Windows.Controls;
namespace Massiv.Views
{
    /// <summary>
    /// Логика взаимодействия для Orders.xaml
    /// </summary>
    public partial class Orders : Page
    {
        public Orders(int choice, MassivContext context)
        {
            InitializeComponent();

            DataContext = choice switch
            {
                1 => new OrdersViewModel(context),
                2 => new CompletedOrdersViewModel(context),
                3 => new DeletedOrdersViewModel(context),
                _ => throw new ArgumentException("Invalid choice value")
            };
        }
    }
}
