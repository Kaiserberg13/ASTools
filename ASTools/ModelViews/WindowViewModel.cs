using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Input;

namespace ASTools.ModelViews
{
    public partial class WindowViewModel : ObservableObject
    {
        #region Private Member
        private Window mWindow;
        private int mOuterMarginSize = 8;
        private int mWindowRadius = 8;
        private Thickness mTaskBarSize;
        #endregion

        #region Public Propertis

        [ObservableProperty]
        public int resizeBorder = 4;

        public Thickness ResizeBorderThickness => new Thickness(ResizeBorder);
        public int OuutherMarginSize
        {
            get => mWindow.WindowState == WindowState.Maximized ? 0 : mOuterMarginSize;
            set => SetProperty(ref mOuterMarginSize, value);
        }
        public Thickness OuutherMarginSizeThickness => new Thickness(OuutherMarginSize);

        public int WindowRadius
        {
            get => mWindow.WindowState == WindowState.Maximized ? 0 : mWindowRadius;
            set => SetProperty(ref mWindowRadius, value);
        }
        public CornerRadius WindowCornerRadius
        {
            get { return new CornerRadius(mWindowRadius); }
        }

        [ObservableProperty]
        public int titleHight = 44;

        public Thickness TaskBarThickness
        {
            get => mWindow.WindowState != WindowState.Maximized ? new Thickness(0) : mTaskBarSize;
            set => SetProperty(ref mTaskBarSize, value); 
        }
        protected Window CurrentWindow => mWindow;


        #endregion

        #region Commands
        [RelayCommand]
        private void MinMaxWinSize()
        {
            mWindow.WindowState = mWindow.WindowState != WindowState.Normal ? WindowState.Normal : WindowState.Maximized;
        }

        [RelayCommand]
        private void Close()
        {
            mWindow.Close();
        }

        [RelayCommand]
        private void Warp()
        {
            mWindow.WindowState = WindowState.Minimized;
        }
        #endregion

        #region Constructor
        public WindowViewModel(Window window)
        {
            mWindow = window;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var waScreen = SystemParameters.WorkArea;

            if(waScreen.Left != 0)
            {
                TaskBarThickness = new Thickness(waScreen.Left, 0, 0, 0);
            } else if (waScreen.Right != screenWidth)
            {
                TaskBarThickness = new Thickness(0, 0, screenWidth - waScreen.Right, 0);
            } else if(waScreen.Top != 0)
            {
                TaskBarThickness = new Thickness(0, waScreen.Top, 0, 0);
            } else if(waScreen.Bottom != screenHeight)
            {
                TaskBarThickness = new Thickness(0, 0, 0, screenHeight - waScreen.Bottom);
            }

            mWindow.StateChanged += (sender, e) =>
            {
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuutherMarginSize));
                OnPropertyChanged(nameof(OuutherMarginSizeThickness));
                OnPropertyChanged(nameof(WindowRadius));
                OnPropertyChanged(nameof(WindowCornerRadius));
                OnPropertyChanged(nameof(TaskBarThickness));
            };
        }
        #endregion
    }
}