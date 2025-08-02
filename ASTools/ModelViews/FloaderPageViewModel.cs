using ASTools.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace ASTools.ModelViews
{
    public class FloaderPageViewModel : BaseViewModel
    {
        private string _title;
        private FilterModel _selectedItem;
        private ObservableCollection<FilterModel> _items;

        public FilterModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        public ObservableCollection<FilterModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public FloaderPageViewModel(string title, ObservableCollection<ComponentModel> tools)
        {
            _title = title;

            try
            {
                Items = new ObservableCollection<FilterModel>
                {
                    new FilterModel
                    {
                        Header = "All",
                        Content = tools
                    }
                };

                var filter = tools
                    .SelectMany(tool => tool.Tags, (tool, tag) => new { tool, tag })
                    .GroupBy(tool => tool.tag);

                foreach (var tool in filter)
                {
                    Items.Add(new FilterModel
                    {
                        Header = tool.Key,
                        Content = new ObservableCollection<ComponentModel>(tool.Select(x => x.tool))
                    });
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Error initializing UI: " + ex.Message, "Sturtup critical error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SelectedItem = Items.FirstOrDefault();
        }
    }
}
