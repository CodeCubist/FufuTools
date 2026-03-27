using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace FufuTools
{
    public sealed partial class MainWindow : Window
    {
        private BBSWindow _bbsWindow;
        private Window _loginWindow;
        private Window _checkinWindow;

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            
            SystemBackdrop = new MicaBackdrop();

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(wndId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(1260, 768));
            appWindow.SetIcon("Assets\\WindowIcon.ico");
        }
        
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            BgFadeInStoryboard.Begin();
        }

        private void OpenBBSWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_bbsWindow == null)
            {
                _bbsWindow = new BBSWindow();
                _bbsWindow.Closed += (_, _) => _bbsWindow = null; 
            }
            
            _bbsWindow.Activate();
        }

        private void OpenLoginPage_Click(object sender, RoutedEventArgs e)
        {
            if (_loginWindow == null)
            {
                _loginWindow = new Window();
                _loginWindow.Title = "扫码登录";

                _loginWindow.ExtendsContentIntoTitleBar = true;
                _loginWindow.SystemBackdrop = new MicaBackdrop();

                IntPtr hWnd = WindowNative.GetWindowHandle(_loginWindow);
                WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(wndId);

                appWindow.SetIcon("Assets\\WindowIcon.ico");

                appWindow.Resize(new Windows.Graphics.SizeInt32(600, 600));

                var loginPage = new LoginPage();
                _loginWindow.Content = loginPage;
                
                loginPage.Loaded += (_, _) => 
                {
                    _loginWindow.SetTitleBar(loginPage.AppTitleBar);
                };
                
                _loginWindow.Closed += (_, _) => _loginWindow = null;
            }

            _loginWindow.Activate();
        }

        private void OpenCheckinPage_Click(object sender, RoutedEventArgs e)
        {
            if (_checkinWindow == null)
            {
                _checkinWindow = new Window();
                _checkinWindow.Title = "米游社签到";
                _checkinWindow.ExtendsContentIntoTitleBar = true;
                _checkinWindow.SystemBackdrop = new MicaBackdrop();

                IntPtr hWnd = WindowNative.GetWindowHandle(_checkinWindow);
                WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(wndId);
        
                appWindow.SetIcon("Assets\\WindowIcon.ico");
                appWindow.Resize(new Windows.Graphics.SizeInt32(700, 600));

                var checkinPage = new CheckinPage();
                _checkinWindow.Content = checkinPage;
        
                checkinPage.Loaded += (_, _) => 
                {
                    _checkinWindow.SetTitleBar(checkinPage.AppTitleBar);
                };
        
                _checkinWindow.Closed += (_, _) => _checkinWindow = null;
            }

            _checkinWindow.Activate();
        }
    }
}