using ASTools.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;


namespace ASTools.ModelViews
{
    public partial class FloaderPageViewModel : ObservableObject
    {
        #region Public Properties
        [ObservableProperty]
        public string title;

        [ObservableProperty]
        public FilterModel selectedItem;

        [ObservableProperty]
        public ObservableCollection<FilterModel> items;
        #endregion



        #region Constructor
        public FloaderPageViewModel(string title, ObservableCollection<ComponentModel> tools)
        {
            Title = title;

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
        #endregion
    }
}
