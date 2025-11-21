using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;

namespace Massiv.ViewModels
{
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public ImageSource Icon { get; set; }
        public ICommand Command { get; set; }
        public ObservableCollection<MenuItemViewModel> Children { get; set; } = new ObservableCollection<MenuItemViewModel>();
    }
}