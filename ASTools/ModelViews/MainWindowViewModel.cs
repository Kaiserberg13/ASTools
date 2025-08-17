using ASTools.Lang;
using ASTools.Messenger;
using ASTools.Models;
using ASTools.Pages;
using ASTools.Unit;
using ASTools.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
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

    public partial class MainWindowViewModel : WindowViewModel, 
        IRecipient<SettingsChangedMessage>, 
        IRecipient<SettingsLangChangeMessage>,
        IRecipient<BackToFolderMassege>,
        IRecipient<OpenToolPageMessage>
    {

        #region Private Member
        private FloaderModel _selectedItem;
        private object _currentPageViewModel;
        Configuration AppConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
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
                CurrentPageViewModel = value.PageViewModel;
                OnPropertyChanged("SelectedItem");
                OnPageChanged();
            }
        }
        #endregion

        #region MainWindow Commands
        [RelayCommand]
        private void OpenSettings()
        {
            var settingsWindow = new Settings();
            settingsWindow.Show();
        }
        #endregion

        #region ToolsPage Commands
        
        #endregion

        #region Constructor
        public MainWindowViewModel(Window window) : base(window)
        {
            WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this);
            WeakReferenceMessenger.Default.Register<SettingsLangChangeMessage>(this);
            WeakReferenceMessenger.Default.Register<BackToFolderMassege>(this);
            WeakReferenceMessenger.Default.Register<OpenToolPageMessage>(this);

            var loadingWindow = new LoadScreen();
            loadingWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
                if (FolderItems.Any())
                    SelectedItem = FolderItems.First();
            }
        }
        public void Receive(SettingsLangChangeMessage message)
        {
            var culture = LocalizationProvider.Instance.CurrentCulture;
            var newTitle = Resources.ResourceManager.GetString("TitleHome", culture)
                           ?? LocalizationProvider.Instance["TitleHome"]
                           ?? "Home";

            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                if (FolderItems != null && FolderItems.Count > 0)
                    FolderItems[0].Title = newTitle;
            }
            OnPropertyChanged(string.Empty);
        }
        public void Receive(BackToFolderMassege message)
        {
            if (SelectedItem != null)
            {
                CurrentPageViewModel = SelectedItem.PageViewModel;
            }
        }
        public void Receive(OpenToolPageMessage message)
        {
            if (message.Value != null)
            {
                CurrentPageViewModel = message.Value;
            }
        }
        #endregion

        #region Functions
        private async Task LoadData()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var general = (ASTools.Srttings.General)ConfigurationManager.GetSection("general");
            List<FloaderInfo> allFolders = null;
            Dictionary<string, string> allTools = null;

            await Task.Run(() =>
            {
                

                allTools = Directory.GetDirectories(Path.GetFullPath(general.ToolsDir))
                    .Select(dirPath => new DirectoryInfo(dirPath))
                    .Where(dir => dir.Name != "Tamplate")
                    .ToDictionary(
                        dir => dir.Name,
                        dir => dir.FullName
                    );

                var assembly = Assembly.GetExecutingAssembly();

                using (Stream stream = assembly.GetManifestResourceStream("ASTools.Settings.Folders.json"))
                {
                    if(stream == null)
                    {
                        throw new FileNotFoundException("Folders.json not found in resources.");
                    }
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        allFolders = JsonSerializer.Deserialize<List<FloaderInfo>>(jsonContent);
                    }
                }

                LocalizationProvider.Instance.CurrentCulture = general.Language;
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

                    var culture = LocalizationProvider.Instance.CurrentCulture;
                    var newTitle = Resources.ResourceManager.GetString("TitleHome", culture)
                                   ?? LocalizationProvider.Instance["TitleHome"]
                                   ?? "Home";

                    FolderItems = new ObservableCollection<FloaderModel>
                    {
                        new FloaderModel
                        (
                            new ObservableCollection<ComponentModel>(toolsModels),
                            "home.png",
                            newTitle,
                            true
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
                    {
                        SelectedItem = FolderItems.First();
                    }
                       
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
