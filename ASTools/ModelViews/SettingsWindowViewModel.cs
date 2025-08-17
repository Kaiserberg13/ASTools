using ASTools.Lang;
using ASTools.Messenger;
using ASTools.Srttings;
using ASTools.Unit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;

namespace ASTools.ModelViews
{
    public class LangOption
    {
        public string Label { get; }
        public string Value { get; }

        public LangOption(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    public partial class SettingsWindowViewModel : WindowViewModel
    {
        #region private member
        private string _settigsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "App.settings.json");
        private Dictionary<string, string> _changedSettings = new Dictionary<string, string>();
        private General _generalSettings = (General)ConfigurationManager.GetSection("general");
        private bool _isLangChange = false;
        private bool _isDirChange = false;
        #endregion

        #region Public Properties
        public ObservableCollection<LangOption> LangItems { get; } = new();

        private string _selectedLang;
        public string SelectedLang
        {
            get => _selectedLang;
            set 
            {
                SetProperty(ref _selectedLang, value);
                _isLangChange = true;
                IsChanged = true;
                if (_changedSettings == null || !_changedSettings.TryGetValue("language", out var Value))
                {
                    _changedSettings.Add("language", value);
                } else
                {
                    _changedSettings["language"] = value;
                }
            }
        }

        [ObservableProperty]
        private string toolsDir;

        [ObservableProperty]
        private string guidesDir;

        private bool _isChanged = false;
        public bool IsChanged
        {
            get => _isChanged;
            private set
            {
                SetProperty(ref _isChanged, value);
            }
        }

        private bool _isNotSaving = true;
        public bool IsNotSaving
        {
            get => _isNotSaving;
            private set
            {
                SetProperty(ref _isNotSaving, value);
            }
        }
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
                _isDirChange = true;
                var fbd = new OpenFolderDialog
                {
                    Title = folderKey,
                    InitialDirectory = Path.GetFullPath(_generalSettings.GetType().GetProperty(folderKey).ToString()),
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
            IsNotSaving = false;
            IsChanged = false;
            await Task.Run(() =>
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var section = (General)config.GetSection("general");

                foreach (var setting in _changedSettings)
                {
                    switch (setting.Key)
                    {
                        case "language":
                            var newCulture = new CultureInfo(setting.Value);
                            section.Language = newCulture;
                            LocalizationProvider.Instance.CurrentCulture = newCulture;
                            _isLangChange = true;
                            break;
                        case "toolsDir":
                            var oldToolsDir = Path.GetFullPath(section.ToolsDir);
                            var newToolsDir = Path.GetFullPath(setting.Value);
                            if (Directory.Exists(oldToolsDir))
                            {
                                if (!Directory.Exists(newToolsDir))
                                {
                                    Directory.CreateDirectory(newToolsDir);
                                }

                                foreach (var subDir in Directory.GetDirectories(oldToolsDir))
                                {
                                    var destDir = Path.Combine(newToolsDir, Path.GetFileName(subDir));
                                    if (Directory.Exists(destDir))
                                    {
                                        Directory.Delete(destDir, true);
                                    }
                                    Directory.Move(subDir, destDir);
                                }
                            }
                            section.ToolsDir = setting.Value;
                            _isDirChange = true;
                            break;
                        case "guidesDir":
                            var oldGuidesDir = Path.GetFullPath(section.GuidesDir);
                            var newGuidesDir = Path.GetFullPath(setting.Value);
                            if (Directory.Exists(oldGuidesDir))
                            {
                                if (!Directory.Exists(newGuidesDir))
                                {
                                    Directory.CreateDirectory(newGuidesDir);
                                }

                                foreach (var subDir in Directory.GetDirectories(oldGuidesDir))
                                {
                                    var destDir = Path.Combine(newGuidesDir, Path.GetFileName(subDir));
                                    if (Directory.Exists(destDir))
                                    {
                                        Directory.Delete(destDir, true);
                                    }
                                    Directory.Move(subDir, destDir);
                                }
                            }
                            section.GuidesDir = setting.Value;
                            _isDirChange = true;
                            break;
                    }
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("general");
            });

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_isLangChange)
                {
                    WeakReferenceMessenger.Default.Send(new SettingsLangChangeMessage(new CultureInfo(_changedSettings["language"])));
                    _isLangChange = false;
                }
                if(_isDirChange)
                {
                    _isDirChange = false;
                    WeakReferenceMessenger.Default.Send(new SettingsChangedMessage(true));
                }
                IsNotSaving = true;
            });
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
                MessageBoxResult result = MessageBox.Show(Resources.DialogMessageCloseWithoutSaivingText, Resources.DialogMessageCloseWithoutSaivingTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
            ToolsDir = _generalSettings.ToolsDir;
            GuidesDir = _generalSettings.GuidesDir;

            LangItems.Add(new LangOption("Русский", "ru"));
            LangItems.Add(new LangOption("English", "en"));
        }
        #endregion

        #region Functions
        
        #endregion
    }
}
