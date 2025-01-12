using System.Net;
using System.Net.Http;
using System.Windows;

namespace LOLDaily
{
    public partial class MainWindow : Window
    {
        private HttpClient _httpClient;
        private List<string> _logMessages = new List<string>();
        private List<string> domains = new List<string>
        {
            "https://qq.com",
            "https://game.qq.com"
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
            this.Closed += OnWindowClosed;
        }

        private async void OnWindowClosed(object sender, EventArgs e)
        {
            Log("窗口关闭，开始清空 Cookies...");

            if (WebView.CoreWebView2 != null)
            {
                var cookieManager = WebView.CoreWebView2.CookieManager;
                 cookieManager.DeleteAllCookies();
                Log("Cookies 已清空。");
            }
        }

        private async void InitializeWebView()
        {
            await WebView.EnsureCoreWebView2Async();
            _httpClient = new HttpClient();

            // 绑定导航完成事件
            WebView.CoreWebView2.NavigationCompleted += async (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    var cookies = await GetAllCookiesFromWebView();

                    if (cookies?.GetAllCookies().Any(c => c.Name == "openid") == true)
                    {
                        Log("Cookie 包含 openid，隐藏 WebView 并开始请求...");
                        WebView.Visibility = Visibility.Collapsed;
                        LogsListView.Visibility = Visibility.Visible;
                        await SendSignRequests(cookies);
                    }
                    else
                    {
                        Log("Cookie 不包含 openid，请等待用户完成登录...");
                    }
                }
                else
                {
                    Log("导航失败，请检查网络连接或目标 URL 是否有效。");
                }
            };

            // 导航到登录页面
            WebView.CoreWebView2.Navigate("https://lol.qq.com");
        }

        private async Task<CookieContainer> GetAllCookiesFromWebView()
        {
            var cookieManager = WebView.CoreWebView2.CookieManager;
            var cookieContainer = new CookieContainer();

            foreach (var domain in domains)
            {
                var cookies = await cookieManager.GetCookiesAsync(domain);
                foreach (var cookie in cookies)
                {
                    cookieContainer.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                }
            }

            return cookieContainer;
        }

        private async Task SendSignRequests(CookieContainer cookies)
        {
            var lolServerSelect = new List<(string t, string v, string status)>
            {
                ("艾欧尼亚   电信", "1", "1"),
                ("比尔吉沃特 网通", "2", "1"),
                ("祖安      电信", "3", "1"),
                ("诺克萨斯   电信", "4", "1"),
                ("班德尔城  电信", "5", "1"),
                ("德玛西亚   网通", "6", "1"),
                ("皮尔特沃夫 电信", "7", "1"),
                ("战争学院   电信", "8", "1"),
                ("弗雷尔卓德 网通", "9", "1"),
                ("巨神峰    电信", "10", "1"),
                ("雷瑟守备   电信", "11", "1"),
                ("无畏先锋   网通", "12", "1"),
                ("裁决之地   电信", "13", "1"),
                ("黑色玫瑰   电信", "14", "1"),
                ("暗影岛     电信", "15", "1"),
                ("恕瑞玛     网通", "16", "1"),
                ("钢铁烈阳   电信", "17", "1"),
                ("水晶之痕   电信", "18", "1"),
                ("均衡教派   网通", "19", "1"),
                ("扭曲丛林   网通", "20", "1"),
                ("教育网专区", "21", "1"),
                ("影流      电信", "22", "1"),
                ("守望之海   电信", "23", "1"),
                ("征服之海   电信", "24", "1"),
                ("卡拉曼达   电信", "25", "1"),
                ("巨龙之巢   网通", "26", "1"),
                ("皮城警备   电信", "27", "1"),
                ("男爵领域   全网络", "30", "1")
            };


            var handler = new HttpClientHandler { CookieContainer = cookies };
            _httpClient = new HttpClient(handler);

            foreach (var server in lolServerSelect)
            {
                var url =
                    $"https://apps.game.qq.com/daoju/igw/main?_service=buy.plug.svr.sysc_ext&paytype=8&iActionId=22565&propid=338943&buyNum=1&_app_id=1006&_plug_id=72007&_biz_code=lol&areaid={server.v}";

                try
                {
                    var response = await _httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    Log($"[{server.t}] 请求成功: {responseContent}");
                }
                catch (HttpRequestException httpEx)
                {
                    Log($"[{server.t}] 请求失败（网络错误）: {httpEx.Message}");
                }
                catch (Exception ex)
                {
                    Log($"[{server.t}] 请求失败: {ex.Message}");
                }

                await Task.Delay(1500); // 等待 1.2 秒
            }
        }

        private void Log(string message)
        {
            _logMessages.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
            LogsListView.ItemsSource = null; // 清空绑定
            LogsListView.ItemsSource = _logMessages; // 重新绑定
        }
    }
}