using ASTools.Models;
using System;
using System.Windows;
using ASTools.ModelViews;

namespace ASTools.View
{
    /// <summary>
    /// Логика взаимодействия для ToolWindow.xaml
    /// </summary>
    public partial class ToolWindow : Window
    {
        public ToolWindow(ComponentModel tool)
        {
            InitializeComponent();
            DataContext = new ToolWindowViewModel(this, tool);
        }
    }
}
