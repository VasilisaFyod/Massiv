using System.Windows;
using Massiv.ViewModels;

namespace Massiv.Views
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class MenuView : Window
    {
        public MenuView()
        {
            InitializeComponent();
            DataContext = new MenuViewModel();
        }
    }
}
