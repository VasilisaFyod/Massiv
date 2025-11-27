using Massiv.Models;
using Massiv.ViewModels;
using System.Windows.Controls;
namespace Massiv.Views
{
    /// <summary>
    /// Логика взаимодействия для LogistTableView.xaml
    /// </summary>
    public partial class LogistTableView : Page
    {
        public LogistTableView(int choice, MassivContext context, string tableType = null)
        {
            InitializeComponent();

            DataContext = choice switch
            {
                1 => new LogistTablesViewModel(context, tableType),
                2 => new CompletedLogistTablesViewModel(context, tableType),
                3 => new DeletedLogistTablesViewModel(context, tableType),
                _ => throw new ArgumentException("Invalid choice value")
            };
        }
    }
}
