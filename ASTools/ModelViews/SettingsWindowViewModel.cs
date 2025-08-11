using ASTools.Messenger;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;

namespace ASTools.ModelViews
{
    public partial class SettingsWindowViewModel : WindowViewModel
    {
        #region private member
        private string _settigsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "App.settings.json");
        private Dictionary<string, string> _settings;
        private Dictionary<string, string> _changedSettings = new Dictionary<string, string>();
        #endregion

        #region Public Properties
        [ObservableProperty]
        private string toolsDir;

        [ObservableProperty]
        private string guidesDir;

        [ObservableProperty]
        private bool isChanged = false;

        [ObservableProperty]
        private bool isNotSaving = true;
        #endregion

        #region Changed
        public string NewToolsDir => _changedSettings.TryGetValue("toolsDir", out var value) ? "New path: " + value : "";

        public string NewGuidesDir => _changedSettings.TryGetValue("guidesDir", out var value) ? "New path: " + value : "";
        #endregion

        #region Commands
        [RelayCommand]
        private void OpenSupportBrowser()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://github.com/Kaiserberg13/ASTools",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        [RelayCommand]
        private void SelectFolder(string folderKey)
        {
            if (folderKey != null)
            {
                var fbd = new OpenFolderDialog
                {
                    Title = folderKey,
                    InitialDirectory = Path.GetFullPath(_settings[folderKey]),
                    Multiselect = false
                };
                if (fbd.ShowDialog() == true)
                {
                    if (_changedSettings == null || !_changedSettings.TryGetValue(folderKey, out var Value))
                        _changedSettings.Add(folderKey, fbd.FolderName);

                    var dir = Path.GetFullPath(fbd.FolderName);
                    var baseDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar);

                    if (dir.StartsWith(baseDir))
                    {
                        _changedSettings[folderKey] = dir[baseDir.Length..].TrimStart(Path.DirectorySeparatorChar);
                    }
                    else
                    {
                        _changedSettings[folderKey] = dir;
                    }
                    IsChanged = true;
                    OnPropertyChanged(nameof(NewToolsDir));
                    OnPropertyChanged(nameof(NewGuidesDir));
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_changedSettings.Count > 0)
            {
                IsNotSaving = false;
                IsChanged = false;
                await Task.Run(() =>
                {
                    foreach (var setting in _changedSettings)
                    {
                        if (setting.Key == "toolsDir" || setting.Key == "guidesDir")
                        {
                            var oldPath = Path.GetFullPath(_settings[setting.Key]);
                            var newPath = Path.GetFullPath(setting.Value);

                            if (Directory.Exists(oldPath))
                            {
                                if(!Directory.Exists(newPath))
                                {
                                    Directory.CreateDirectory(newPath);
                                }

                                foreach (var subDir in Directory.GetDirectories(oldPath))
                                {
                                    var destDir = Path.Combine(newPath, Path.GetFileName(subDir));
                                    if (Directory.Exists(destDir))
                                    {
                                        Directory.Delete(destDir, true);
                                    }
                                    Directory.Move(subDir, destDir);
                                }
                            }
                        }
                        _settings[setting.Key] = setting.Value;
                    }
                });

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonContent = JsonSerializer.Serialize(_settings, jsonOptions);
                await File.WriteAllTextAsync(_settigsPath, jsonContent);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    WeakReferenceMessenger.Default.Send(new SettingsChangedMessage(true));
                    IsNotSaving = true;
                }); 
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CurrentWindow.Close();
        }

        [RelayCommand]
        private void CloseWindow()
        {
            if (IsChanged)
            {
                MessageBoxResult result = MessageBox.Show("Изменения не сохранены. Закрыть окно?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                CurrentWindow.Close();
                return;
            }
            CurrentWindow.Close();
        }
        #endregion

        #region Constructors
        public SettingsWindowViewModel(Window window) : base(window)
        {
            _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_settigsPath));

            ToolsDir = _settings["toolsDir"];
            GuidesDir = _settings["guidesDir"];
        }
        #endregion

        #region Functions
        
        #endregion
    }
}
