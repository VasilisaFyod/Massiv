using System.Windows.Controls;
using Massiv.ViewModels;
using Massiv.Models;

namespace Massiv.Views
{
    /// <summary>
    /// Логика взаимодействия для CUD.xaml
    /// </summary>
    public partial class CUD : Page
    {
        public CUD(int choice, MassivContext context = null, Order orderToEdit = null)
        {
            InitializeComponent();

            var dbContext = context ?? MassivContext.GetContext();

            DataContext = new CUDViewModel(choice, dbContext, orderToEdit);
        }
    }
}
