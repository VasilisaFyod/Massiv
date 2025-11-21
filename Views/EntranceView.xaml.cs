using Massiv.ViewModels;
using System.Windows;

namespace Massiv.Views
{
    /// <summary>
    /// Логика взаимодействия для EntranceView.xaml
    /// </summary>
    public partial class EntranceView : Window
    {
        public EntranceView()
        {
            InitializeComponent();
            DataContext = new EntranceViewModel();
        }
    }
}
