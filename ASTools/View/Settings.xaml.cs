using ASTools.ModelViews;
using System.Windows;

namespace ASTools.View
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = new SettingsWindowViewModel(this);
        }
    }
}
