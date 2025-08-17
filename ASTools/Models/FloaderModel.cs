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
        private Geometry _icon;
        private FloaderPageViewModel _pageViewModel;
        private bool _isDefault = false;
        private string _iconKey;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }
        public Geometry Icon
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
        public bool IsDefault
        {
            get => _isDefault;
            set
            {
                _isDefault = value;
                OnPropertyChanged("IsDefault");
            }
        }
        public string IconKey
        {
            get => _iconKey;
            set
            {
                _iconKey = value;
                OnPropertyChanged("IconKey");
            }
        }

        public FloaderModel(ObservableCollection<ComponentModel> floaderTools, string floaderIcon, string floaderTitle) 
        {
            try
            {
                Title = floaderTitle;
                IconKey = floaderIcon;
                Icon = (Geometry)Application.Current.FindResource(floaderIcon);
                PageViewModel = new FloaderPageViewModel(_title, floaderTools);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing floader: " + ex.Message, "Sturtup critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public FloaderModel(ObservableCollection<ComponentModel> floaderTools, string floaderIcon, string floaderTitle, bool isItDefault)
        {
            try
            {
                Title = floaderTitle;
                IconKey = floaderIcon;
                Icon = (Geometry)Application.Current.FindResource(floaderIcon);
                PageViewModel = new FloaderPageViewModel(_title, floaderTools, isItDefault);
                IsDefault = isItDefault;
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
