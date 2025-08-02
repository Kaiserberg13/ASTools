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

        #endregion

        #region Public Propertis

        public int ResizeBorder { get; set; } = 4;
        public Thickness ResizeBorderThickness 
        {
            get { return new Thickness(ResizeBorder);  }
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
                        if(mWindow.WindowState != WindowState.Normal)
                        {
                            mWindow.WindowState = WindowState.Normal;
                        } else
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

            mWindow.StateChanged += (sender, e) =>
            {
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuutherMarginSize));
                OnPropertyChanged(nameof(OuutherMarginSizeThickness));
                OnPropertyChanged(nameof(WindowRadius));
                OnPropertyChanged(nameof(WindowCornerRadius));
            };
        }
        #endregion
    }
}
