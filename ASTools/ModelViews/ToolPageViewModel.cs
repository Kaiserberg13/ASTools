using ASTools.Messenger;
using ASTools.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ASTools.ModelViews
{
    public partial class ToolPageViewModel : ObservableObject
    {
        public ComponentModel Tool { get; }
        public Visibility isRMHawe { get; }

        public ToolPageViewModel(ComponentModel tool)
        {
            Tool = tool ?? throw new ArgumentNullException(nameof(tool), "Tool cannot be null");
            isRMHawe = tool.MdReadme != null && tool.MdReadme.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        [RelayCommand]
        private void Back()
        {
            WeakReferenceMessenger.Default.Send(new BackToFolderMassege(true));
        }
    }
}
