using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace ASTools.Unit
{
    public class LocalizationProvider : ObservableObject
    {
        private static readonly ResourceManager ResourceManager = new ResourceManager("ASTools.Lang.Resources", typeof(LocalizationProvider).Assembly);
        private static LocalizationProvider _instance;
        public static LocalizationProvider Instance => _instance ??= new LocalizationProvider();

        private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    Thread.CurrentThread.CurrentUICulture = value;
                    OnPropertyChanged(nameof(CurrentCulture));
                    RaiseAllPropertiesChanged();
                }
            }
        }

        public string this[string key] => ResourceManager.GetString(key, _currentCulture) ?? key;

        private void RaiseAllPropertiesChanged()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}
