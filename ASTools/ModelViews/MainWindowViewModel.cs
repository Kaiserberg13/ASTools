using ASTools.Messenger;
using ASTools.Models;
using ASTools.Pages;
using ASTools.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    public partial class MainWindowViewModel : WindowViewModel, IRecipient<SettingsChangedMessage>
    {

        #region Private Member
        private FloaderModel _selectedItem;
        private object _currentPageViewModel;
        #endregion

        #region Public Properties
        [ObservableProperty]
        private ObservableCollection<FloaderModel> folderItems;

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
        #endregion

        #region Commands
        [RelayCommand]
        private void OpenSettings()
        {
            var settingsWindow = new Settings();
            settingsWindow.Show();
        }
        #endregion

        #region Constructor
        public MainWindowViewModel(Window window) : base(window)
        {
            WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this);

            var loadingWindow = new LoadScreen();
            loadingWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;  // Центр экрана
            loadingWindow.Topmost = true;
            loadingWindow.Show();

            LoadData().ContinueWith(task =>
            {
                Application.Current.Dispatcher.Invoke(() => loadingWindow.Close());
            });
        }
        #endregion

        #region Messaging
        public async void Receive(SettingsChangedMessage message)
        {
            if (message.Value)
            {
                await LoadData();
                OnPropertyChanged(nameof(FolderItems));
                if(FolderItems.Any())
                    SelectedItem = FolderItems.First();
            }
        }
        #endregion

        #region Functions
        private async Task LoadData()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            Dictionary<string, string> config = null;
            List<FloaderInfo> allFolders = null;
            Dictionary<string, string> allTools = null;

            await Task.Run(() =>
            {
                config = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(baseDir, "Data", "Config", "App.settings.json")));

                allTools = Directory.GetDirectories(Path.GetFullPath(config["toolsDir"]))
                    .Select(dirPath => new DirectoryInfo(dirPath))
                    .Where(dir => dir.Name != "Tamplate")
                    .ToDictionary(
                        dir => dir.Name,
                        dir => dir.FullName
                    );
                allFolders = JsonSerializer.Deserialize<List<FloaderInfo>>(File.ReadAllText(Path.Combine(baseDir, config["foldersJson"])));
            });

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var toolsModels = allTools
                        .Where(tool => Directory.Exists(Path.Combine(baseDir, tool.Value)) || Directory.Exists(tool.Value))
                        .Select(tool => new ComponentModel(
                            Directory.Exists(tool.Value) ? tool.Value : Path.Combine(baseDir, tool.Value),
                            tool.Key))
                        .ToList();

                    FolderItems = new ObservableCollection<FloaderModel>
                    {
                        new FloaderModel
                        (
                            new ObservableCollection<ComponentModel>(toolsModels),
                            "home.png",
                            "Home"
                        )
                    };

                    foreach (var folder in allFolders)
                    {
                        var components = toolsModels
                            .Where(tool => folder.Tools.Contains(tool.ToolKey))
                            .ToList();
                        FolderItems.Add(new FloaderModel(
                            new ObservableCollection<ComponentModel>(components),
                            folder.Icon,
                            folder.Name
                        ));
                    }

                    if (FolderItems.Any())
                        SelectedItem = FolderItems.First();
                } catch (Exception ex)
                {
                    MessageBox.Show("Error initializing UI: " + ex.Message, "Startup critical error", MessageBoxButton.OK, MessageBoxImage.Error);
                    CurrentWindow.Close();
                }
            });
        }

        private void OnPageChanged()
        {
            if (SelectedItem != null)
                CurrentPageViewModel = SelectedItem.PageViewModel;
        }
        #endregion
    }
}
