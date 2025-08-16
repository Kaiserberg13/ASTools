using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASTools.Models
{
    public class ComponentInfo
    {
        public string name { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public string[] tags { get; set; }
    }

    public class ComponentModel : INotifyPropertyChanged
    {
        private ImageSource _icon = new BitmapImage(new Uri("pack://application:,,,/Assets/DefaultIcon.png"));
        private string _name = "Error";
        private string _author = "Error";
        private string _description = "Error";
        private string[] _tags = ["Udefiend"];
        private string _tagsDisplay = "Udefiend";
        private ImageSource _banner = new BitmapImage(new Uri("pack://application:,,,/Assets/DefaultIcon.png"));
        private string _mdReadme = string.Empty;
        private string _codePath;

        public string ToolKey { get; set; }
        public ImageSource Icon { get { return _icon; } set { _icon = value; OnPropertyChanged("Icon"); } }
        public string Name { get { return _name; } set { _name = value; OnPropertyChanged("Name"); } }  
        public string Author { get { return _author; } set { _author = value; OnPropertyChanged("Author"); } } 
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged("Description"); } }
        public string[] Tags { get { return _tags; } set { _tags = value; OnPropertyChanged("Type"); } }
        public string TagsDisplay { get { return _tagsDisplay; } set { _tagsDisplay = value; OnPropertyChanged("TagsDisplay"); } }
        public ImageSource Banner { get { return _banner; } set { _banner = value; OnPropertyChanged("Banner"); } }
        public string CodePath { get { return _codePath; } private set { _codePath = value; OnPropertyChanged("CodePath"); } }
        public string MdReadme
        {
            get { return _mdReadme; }
            set
            {
                _mdReadme = value;
                OnPropertyChanged("MdReadme");
            }
        }

        public ComponentModel(string path, string tool_name)
        {
            try
            {
                ToolKey = tool_name;
                ComponentInfo json = new ComponentInfo {
                    name = "undefiend",
                    author = "unknown",
                    description = "No information",
                    tags = ["undefiend"]
                };

                CodePath = Path.GetFullPath(path);

                if (File.Exists(Path.GetFullPath(Path.Combine(path, "info.json"))))
                    json = JsonSerializer.Deserialize<ComponentInfo>(File.ReadAllText(Path.GetFullPath(Path.Combine(path, "info.json"))));

                if(File.Exists(Path.GetFullPath(Path.Combine(path, "readme.md")))) 
                    MdReadme = File.ReadAllText(Path.GetFullPath(Path.Combine(path, "readme.md")));
                

                Name = json.name ?? "undefiend";
                Author = json.author ?? "unknown";
                Description = json.description ?? "No information";
                Tags = json.tags ?? ["undefiend"];
                TagsDisplay = string.Join(", ", (json.tags ?? ["undefiend"]));

                string iconPath = Path.Combine(path, "icon.png");
                if (!File.Exists(iconPath))
                {
                    iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, iconPath);
                }
                if (File.Exists(iconPath))
                {
                    using (var fs = File.OpenRead(iconPath))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = fs;
                        bmp.EndInit();
                        bmp.Freeze();
                        Icon = bmp;
                    }
                }

                string coverPath = Path.Combine(path, "cover.png");
                if (!File.Exists(iconPath))
                {
                    coverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, coverPath);
                }
                if (File.Exists(coverPath))
                {
                    using (var fs = File.OpenRead(coverPath))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = fs;
                        bmp.EndInit();
                        bmp.Freeze();
                        Banner = bmp;
                    }
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error initializing Tool: " + ex.Message, "Sturtup critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
