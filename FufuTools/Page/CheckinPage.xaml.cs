using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using MihoyoBBS;

namespace FufuTools
{
    public sealed partial class CheckinPage : Page
    {
        public Grid TitleBar => AppTitleBar;
        
        private ObservableCollection<string> ProgressMessages { get; set; } = new();

        public CheckinPage()
        {
            InitializeComponent();
        }

        private async void StartCheckin_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            ProgressMessages.Clear();
            
            var progressRing = new ProgressRing { IsActive = true, Margin = new Thickness(0, 0, 10, 0) };
            var headerText = new TextBlock { Text = "执行", VerticalAlignment = VerticalAlignment.Center };
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            headerPanel.Children.Add(progressRing);
            headerPanel.Children.Add(headerText);
            
            var progressListView = new ListView
            {
                ItemsSource = ProgressMessages,
                MaxHeight = 200,
                SelectionMode = ListViewSelectionMode.None
            };
            
            var resultTextBlock = new TextBlock 
            { 
                TextWrapping = TextWrapping.Wrap, 
                IsTextSelectionEnabled = true 
            };
            var resultScrollViewer = new ScrollViewer 
            { 
                Content = resultTextBlock, 
                MaxHeight = 300, 
                Visibility = Visibility.Collapsed 
            };

            var dialogContent = new StackPanel();
            dialogContent.Children.Add(headerPanel);
            dialogContent.Children.Add(progressListView);
            dialogContent.Children.Add(resultScrollViewer);

            var dialog = new ContentDialog
            {
                Title = "签到",
                Content = dialogContent,
                XamlRoot = XamlRoot
            };
            
            _ = dialog.ShowAsync();

            try
            {
                var loginData = SecureCookieStorage.Get();
                if (loginData == null || string.IsNullOrEmpty(loginData.CookieString))
                {
                    ProgressMessages.Add("未找到有效的登录信息，请先进行扫码登录");
                    UpdateDialogToResultState(dialog, progressRing, headerText, progressListView, resultScrollViewer, resultTextBlock, "本地登录数据读取失败。");
                    button.IsEnabled = true;
                    return;
                }
                
                var config = new Config();
                config.Account.Cookie = loginData.CookieString;
                config.Device.Id = !string.IsNullOrEmpty(loginData.DeviceId) 
                    ? loginData.DeviceId 
                    : Tools.GetDeviceId(loginData.CookieString);

                var genshinCheckin = new Genshin();
                
                ProgressMessages.Add("正在获取账号信息");
                await genshinCheckin.InitializeAsync(config);

                if (genshinCheckin.AccountList == null || genshinCheckin.AccountList.Count == 0)
                {
                    ProgressMessages.Add("未获取到绑定的原神账号信息");
                    UpdateDialogToResultState(dialog, progressRing, headerText, progressListView, resultScrollViewer, resultTextBlock, "未找到原神账号。");
                }
                else
                {
                    ProgressMessages.Add($"共找到 {genshinCheckin.AccountList.Count} 个账号");
                    
                    string checkinResult = await genshinCheckin.SignAccountAsync(config);
                    
                    ProgressMessages.Add("签到完成");
                    
                    UpdateDialogToResultState(dialog, progressRing, headerText, progressListView, resultScrollViewer, resultTextBlock, checkinResult);
                }
            }
            catch (Exception ex)
            {
                ProgressMessages.Add($"发生异常：{ex.Message}");
                UpdateDialogToResultState(dialog, progressRing, headerText, progressListView, resultScrollViewer, resultTextBlock, $"程序出现错误：\n{ex.Message}");
            }
            finally
            {
                button.IsEnabled = true;
            }
        }
        
        private void UpdateDialogToResultState(
            ContentDialog dialog, 
            ProgressRing ring, 
            TextBlock headerText, 
            ListView listView, 
            ScrollViewer resultView, 
            TextBlock resultText, 
            string finalResult)
        {
            dialog.Title = "签到结果";
            dialog.CloseButtonText = "确定";
            
            ring.IsActive = false;
            ring.Visibility = Visibility.Collapsed;
            headerText.Text = "执行完毕";

            listView.Visibility = Visibility.Collapsed;
            
            resultText.Text = finalResult;
            resultView.Visibility = Visibility.Visible;
        }
    }
}