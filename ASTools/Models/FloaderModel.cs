using ASTools.ModelViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASTools.Models
{
    public class FloaderModel : INotifyPropertyChanged
    {
        private string _title = "";
        private ImageSource _icon;
        private FloaderPageViewModel _pageViewModel;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        public ImageSource Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged("Icon");
            }
        }
        public FloaderPageViewModel PageViewModel
        {
            get => _pageViewModel;
            set
            {
                _pageViewModel = value;
                OnPropertyChanged("PageViewModel");
            }
        }


        public FloaderModel(ObservableCollection<ComponentModel> floaderTools, string floaderIcon, string floaderTitle)
        {
            try
            {
                Title = floaderTitle;
                Icon = new BitmapImage(new Uri($"pack://application:,,,/Assets/Icons/{floaderIcon}"));
                PageViewModel = new FloaderPageViewModel(_title, floaderTools);
            } catch (Exception ex)
            {
                MessageBox.Show("Error initializing floader: " + ex.Message, "Sturtup critical error", MessageBoxButton.OK, MessageBoxImage.Error);
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
