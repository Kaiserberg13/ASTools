using ASTools.Lang;
using ASTools.Messenger;
using ASTools.Models;
using ASTools.Unit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;


namespace ASTools.ModelViews
{
    public partial class FloaderPageViewModel : ObservableObject, IRecipient<SettingsLangChangeMessage>
    {
        #region Public Properties
        [ObservableProperty]
        public string title;

        [ObservableProperty]
        public FilterModel selectedItem;

        [ObservableProperty]
        public ObservableCollection<FilterModel> items;

        private bool _isDefault;
        public Visibility IsDefault
        {
            get => _isDefault? Visibility.Collapsed : Visibility.Visible;
            set
            {
                SetProperty(ref _isDefault, value == Visibility.Collapsed);
            }
        }
        #endregion



        #region Constructor
        public FloaderPageViewModel(string title, ObservableCollection<ComponentModel> tools) : this(title, tools, false) { }
        public FloaderPageViewModel(string title, ObservableCollection<ComponentModel> tools, bool isItDeafault)
        {
            Title = title;
            IsDefault = isItDeafault? Visibility.Collapsed : Visibility.Visible;
            WeakReferenceMessenger.Default.Register<SettingsLangChangeMessage>(this);

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

        public void Receive(SettingsLangChangeMessage message)
        {
            if (_isDefault)
            {
                var culture = LocalizationProvider.Instance.CurrentCulture;
                var newTitle = Resources.ResourceManager.GetString("TitleHome", culture)
                               ?? LocalizationProvider.Instance["TitleHome"]
                               ?? "Home";
                Title = newTitle;
                OnPropertyChanged("Title");
            }
        }
    }
}
