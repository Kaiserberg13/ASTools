using System;
using System.Windows;
using System.Windows.Input;

namespace ASTools.ModelViews
{
    public class WindowViewModel : BaseViewModel
    {
        #region Private Member

        private Window mWindow;

        private int mOuterMarginSize = 8;
        private int mWindowRadius = 8;
        private Thickness mTaskBarSize;

        #endregion

        #region Public Propertis

        public int ResizeBorder { get; set; } = 4;
        public Thickness ResizeBorderThickness
        {
            get { return new Thickness(ResizeBorder); }
        }
        public int OuutherMarginSize
        {
            get { return mWindow.WindowState == WindowState.Maximized ? 0 : mOuterMarginSize; }
            set { mOuterMarginSize = value; }
        }
        public Thickness OuutherMarginSizeThickness
        {
            get { return new Thickness(OuutherMarginSize); }
        }
        public int WindowRadius
        {
            get { return mWindow.WindowState == WindowState.Maximized ? 0 : mWindowRadius; }
            set { mWindowRadius = value; }
        }
        public CornerRadius WindowCornerRadius
        {
            get { return new CornerRadius(mWindowRadius); }
        }
        public int TitleHight { get; set; } = 44;
        public Thickness TaskBarThickness
        {
            get { return mWindow.WindowState != WindowState.Maximized ? new Thickness(0) : mTaskBarSize; }
            set { mTaskBarSize = value; }
        }

        #endregion

        #region Commands
        private RelayCommand _minMaxWinSizeCommand;
        public RelayCommand MinMaxWinSizeCommand
        {
            get
            {
                return _minMaxWinSizeCommand ??
                    (_minMaxWinSizeCommand = new RelayCommand(obj =>
                    {
                        if (mWindow.WindowState != WindowState.Normal)
                        {
                            mWindow.WindowState = WindowState.Normal;
                        }
                        else
                        {
                            mWindow.WindowState = WindowState.Maximized;
                        }
                    }));
            }
        }

        private RelayCommand _closeCommand;
        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand ??
                    (_closeCommand = new RelayCommand(obj =>
                    {
                        mWindow.Close();
                    }));
            }
        }

        private RelayCommand _warpCommand;
        public RelayCommand WarpCommand
        {
            get
            {
                return _warpCommand ??
                    (_warpCommand = new RelayCommand(obj =>
                    {
                        mWindow.WindowState = WindowState.Minimized;
                    }));
            }
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