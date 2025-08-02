using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ASTools.Models
{
    public class FilterModel : INotifyPropertyChanged
    {
        private string _header;
        private ObservableCollection<ComponentModel> _content;

        public string Header { get { return _header; } set { _header = value; OnPropertyChanged("Header"); } }
        public ObservableCollection<ComponentModel> Content { get { return _content; } set { _content = value; OnPropertyChanged("Content"); } }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
