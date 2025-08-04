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
        public ObservableCollection<FloaderModel> FloaderItems { get; set; }
        private FloaderModel _selectedItem;
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

        public FloaderModel SelectedItem
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
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(baseDir, "Data", "Config", "App.settings.json")));

            try
            {
                var allTools = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(baseDir, config["toolsJson"])));
                var allFloaders = JsonSerializer.Deserialize<List<FloaderInfo>>(File.ReadAllText(Path.Combine(baseDir, config["foldersJson"])));

                var toolsModels = allTools
                    .Where(tool => Directory.Exists(Path.Combine(baseDir, tool.Value)) || Directory.Exists(tool.Value))
                    .Select(tool => new ComponentModel(
                        Directory.Exists(tool.Value) ? tool.Value : Path.Combine(baseDir, tool.Value),
                        tool.Key))
                    .ToList();

                FloaderItems = new ObservableCollection<FloaderModel>(
                    allFloaders.Select(floader =>
                        new FloaderModel(
                            new ObservableCollection<ComponentModel>(
                                toolsModels.Where(tool => floader.Tools.Contains(tool.ToolKey))),
                            floader.Icon,
                            floader.Name))
                );

                if (FloaderItems.Any())
                    SelectedItem = FloaderItems.First();
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
