using ASTools.Models;
using ASTools.Pages;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ASTools.ModelViews
{
    public class FloaderInfo
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string[] Tools { get; set; }
    }

    public class MainWindowViewModel : WindowViewModel
    {
        public ObservableCollection<NavigationItemModel> NavigationItems { get; set; }
        private NavigationItemModel _selectedItem;
        private object _currentPageViewModel;
        public object CurrentPageViewModel
        {
            get => _currentPageViewModel;
            set
            {
                _currentPageViewModel = value;
                OnPropertyChanged(nameof(CurrentPageViewModel));
            }
        }

        public NavigationItemModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
                OnPageChanged();
            }
        }

        public MainWindowViewModel(Window window) : base(window)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                var allTools = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(baseDir, "Data", "Tools.json")));
                var allFloaders = JsonSerializer.Deserialize<List<FloaderInfo>>(File.ReadAllText(Path.Combine(baseDir, "Data", "FloadersData.json")));

                var toolsModels = allTools
                    .Where(tool => Directory.Exists(Path.Combine(baseDir, tool.Value)) || Directory.Exists(tool.Value))
                    .Select(tool => new ComponentModel(
                        Directory.Exists(tool.Value) ? tool.Value : Path.Combine(baseDir, tool.Value),
                        tool.Key))
                    .ToList();

                NavigationItems = new ObservableCollection<NavigationItemModel>(
                    allFloaders.Select(floader =>
                        new NavigationItemModel(
                            new ObservableCollection<ComponentModel>(
                                toolsModels.Where(tool => floader.Tools.Contains(tool.ToolKey))),
                            floader.Icon,
                            floader.Name))
                );

                if (NavigationItems.Any())
                    SelectedItem = NavigationItems.First();
            } catch (Exception ex)
            {
                MessageBox.Show("Error initializing UI: " + ex.Message, "Sturtup critical error", MessageBoxButton.OK, MessageBoxImage.Error);
                window.Close();
            }
        }

        private void OnPageChanged()
        {
            if (SelectedItem != null)
                CurrentPageViewModel = SelectedItem.PageViewModel;
        }
    }
}
