using ASTools.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ASTools.ModelViews
{
    public partial class ToolWindowViewModel : WindowViewModel
    {
        private readonly ComponentModel _tool;
        private readonly string _path;
        private readonly Dictionary<string, bool> _boolValues = new();

        [ObservableProperty]
        private ObservableCollection<PanelViewModel> items;

        public ToolWindowViewModel(Window window, ComponentModel tool) : base(window)
        {
            _tool = tool;
            _path = tool.CodePath;
            window.Title = tool.Name;
            window.Icon = tool.Icon;

            BuildUI();
        }

        [RelayCommand]
        private void Run()
        {
            var results = new Dictionary<string, string>();

            foreach (var item in Items)
            {
                if(item is BoolRowVM row)
                {
                    foreach (var boolVm in row.Bools)
                    {
                        results[boolVm.Name] = boolVm.IsActive.ToString();
                    }
                } 
                else if (item is InputVM inputVm)
                {
                    results[inputVm.Name] = inputVm.Text;
                }
            }

            string json = JsonSerializer.Serialize(results, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            string outPath = Path.Combine(_path, "timed.json");
            File.WriteAllText(outPath, json);

            string exePath = Path.GetFileName(_path) + ".exe";
            string exeFullPath = Path.Combine(_path, exePath);

            if (File.Exists(exeFullPath))
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = exeFullPath,
                    WorkingDirectory = _path,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process())
                {
                    process.StartInfo = processInfo;

                    var outputLines = new List<string>();
                    var errorLines = new List<string>();

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputLines.Add(e.Data);
                            Debug.WriteLine($"Output: {e.Data}");
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorLines.Add(e.Data);
                            Debug.WriteLine($"Error: {e.Data}");
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    string output = string.Join("\n", outputLines);
                    string errors = string.Join("\n", errorLines);

                    if (!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(errors))
                    {
                        string message = $"Output:\n{output}\nErrors:\n{errors}";
                        MessageBox.Show(message, "Process Output", MessageBoxButton.OK, MessageBoxImage.Warning);
                    } else if (!string.IsNullOrEmpty(errors))
                    {
                        MessageBox.Show(errors, "Error is running tool", MessageBoxButton.OK, MessageBoxImage.Error);
                    } else if (!string.IsNullOrEmpty(output))
                    {
                        MessageBox.Show(output, "Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void BuildUI()
        {
            if(!File.Exists(Path.Combine(_path, "template.json")))
            {
                MessageBox.Show($"Component code file not found: {Path.Combine(_path, "template.json")}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(_path, "template.json")));
            var templateItems = doc.RootElement.EnumerateArray();

            Items = new ObservableCollection<PanelViewModel>();
            var CurrentBoolRow = new List<BoolVM>();
            const int maxBoolInRow = 3;

            foreach (var item in templateItems)
            {
                if(item.TryGetProperty("category_text", out JsonElement catText))
                {
                    if (CurrentBoolRow.Count > 0)
                    {
                        Items.Add(new BoolRowVM { Bools = new ObservableCollection<BoolVM>(CurrentBoolRow)} );
                        CurrentBoolRow.Clear();
                    }
                    Items.Add(new CategoryVM { Text = catText.GetString() });
                    continue;
                }

                string type = item.TryGetProperty("type", out JsonElement typeElement) ? typeElement.GetString() : null;
                string name = item.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() : null;
                string text = item.TryGetProperty("text", out JsonElement textElement) ? textElement.GetString() : null;
                bool isFilePath = item.TryGetProperty("file_path", out JsonElement filePathElement) && filePathElement.ValueKind == JsonValueKind.True;
                string icon = item.TryGetProperty("icon", out JsonElement iconElement) ? iconElement.GetString() : null;
                string condition = item.TryGetProperty("condition", out JsonElement conditionElement) ? conditionElement.GetString() : null;

                if(type == "bool")
                {
                    var boolVM = new BoolVM
                    {
                        Name = name,
                        Text = text,
                        Icon = icon ?? "🔘",
                        IsActive = false,
                        IsEnabled = true,
                        Condition = condition
                    };
                    boolVM.ToggleCommand = new RelayCommand(() => ToggleBool(boolVM));
                    _boolValues[name] = false;
                    CurrentBoolRow.Add(boolVM);

                    if(CurrentBoolRow.Count >= maxBoolInRow)
                    {
                        Items.Add(new BoolRowVM { Bools = new ObservableCollection<BoolVM>(CurrentBoolRow) });
                        CurrentBoolRow.Clear();
                    }

                } else
                {
                    if (CurrentBoolRow.Count > 0)
                    {
                        Items.Add(new BoolRowVM { Bools = new ObservableCollection<BoolVM>(CurrentBoolRow) });
                        CurrentBoolRow.Clear();
                    }

                    var InputVM = new InputVM
                    {
                        Name = name,
                        Placeholder = text,
                        Text = string.Empty,
                        IsFilePath = isFilePath,
                        Condition = condition,
                        IsEnabled = true
                    };
                    InputVM.PickCommand = new RelayCommand(() => PickFor(InputVM));
                    Items.Add(InputVM);
                }
            }

            if(CurrentBoolRow.Count > 0)
            {
                Items.Add(new BoolRowVM { Bools = new ObservableCollection<BoolVM>(CurrentBoolRow) });
            }

            UpdateConditions(); 
        }

        private void PickFor(InputVM vm)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*|Folders|",
                CheckFileExists = false,
                ValidateNames = false
            };

            if (dlg.ShowDialog() == true)
            {
                string path;

                if (dlg.FilterIndex == 1)
                {
                    path = dlg.FileName;
                }
                else
                {
                    path = Path.GetDirectoryName(dlg.FileName)!;
                }

                vm.Text = path;
            }
        }
        private void ToggleBool(BoolVM vm)
        {
            vm.IsActive = !vm.IsActive;
            _boolValues[vm.Name] = vm.IsActive;
            UpdateConditions();
        }
        private void UpdateConditions()
        {
            foreach (var item in Items)
            {
                if (item is BoolRowVM row)
                {
                    foreach (var boolVm in row.Bools)
                    {
                        boolVm.IsEnabled = string.IsNullOrEmpty(boolVm.Condition) || EvaluateCondition(boolVm.Condition);
                    }
                }
                else if (item is InputVM inputVm)
                {
                    inputVm.IsEnabled = string.IsNullOrEmpty(inputVm.Condition) || EvaluateCondition(inputVm.Condition);
                }
            }
        }
        private bool EvaluateCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return true;

            var parts = condition.Split("==", StringSplitOptions.None);
            if (parts.Length != 2) return true;

            string left = parts[0].Trim();
            string right = parts[1].Trim();

            if (!_boolValues.TryGetValue(left, out bool val)) return true;

            if (string.Equals(right, "True", StringComparison.OrdinalIgnoreCase))
                return val;
            if (string.Equals(right, "False", StringComparison.OrdinalIgnoreCase))
                return !val;

            return true;
        }
    }

    public abstract class PanelViewModel : ObservableObject { }
    public partial class CategoryVM : PanelViewModel
    {
        [ObservableProperty]
        private string text;
    }
    public class BoolRowVM : PanelViewModel
    {
        public ObservableCollection<BoolVM> Bools { get; set; } = new();
    }
    public partial class BoolVM : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string text;

        [ObservableProperty]
        private string icon;

        [ObservableProperty]
        private bool isActive;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private string condition;

        public IRelayCommand ToggleCommand { get; set; }
    }
    public partial class InputVM : PanelViewModel
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string placeholder;

        [ObservableProperty]
        private string text;

        [ObservableProperty]
        private string plaseholder;

        [ObservableProperty]
        private bool isFilePath;

        [ObservableProperty]
        private bool isEnabled;

        [ObservableProperty]
        private string condition;

        public IRelayCommand PickCommand { get; set; }
    }
    public class UIItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate BoolRowTemplate { get; set; }
        public DataTemplate InputTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                CategoryVM => CategoryTemplate,
                BoolRowVM => BoolRowTemplate,
                InputVM => InputTemplate,
                _ => null
            };
        }
    }
}
