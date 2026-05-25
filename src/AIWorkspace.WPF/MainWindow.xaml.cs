using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using AIWorkspace.Core.Services;
using AIWorkspace.WPF.Bridge;
using AIWorkspace.WPF.Native;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace AIWorkspace.WPF;

public partial class MainWindow : Window
{
    private WebBridge? _bridge;
    private HotkeyService? _hotkeyService;
    private SettingsService? _settingsService;
    private bool _isClosingToTray;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
        SetWindowIcon();
    }

    private void SetWindowIcon()
    {
        try
        {
            var bmp = GenerateAppIconBitmap();
            var hIcon = bmp.GetHicon();
            Icon = Imaging.CreateBitmapSourceFromHIcon(hIcon, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            // Store icon handle for cleanup if needed
        }
        catch
        {
            // Fallback to default icon
        }
    }

    private static Bitmap GenerateAppIconBitmap()
    {
        const int size = 256;
        var bmp = new Bitmap(size, size);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.Transparent);

            using var brush = new SolidBrush(System.Drawing.Color.FromArgb(30, 30, 30));
            using var path = new GraphicsPath();
            var r = 48;
            path.AddArc(8, 8, r, r, 180, 90);
            path.AddArc(size - 8 - r, 8, r, r, 270, 90);
            path.AddArc(size - 8 - r, size - 8 - r, r, r, 0, 90);
            path.AddArc(8, size - 8 - r, r, r, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);

            using var font = new System.Drawing.Font("Segoe UI", 72, System.Drawing.FontStyle.Bold);
            var text = "AI";
            var textSize = g.MeasureString(text, font);
            g.DrawString(text, font, System.Drawing.Brushes.White,
                (size - textSize.Width) / 2, (size - textSize.Height) / 2 - 8);
        }
        return bmp;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Check WebView2 runtime availability
        try
        {
            _ = CoreWebView2Environment.GetAvailableBrowserVersionString();
        }
        catch (Exception)
        {
            System.Windows.MessageBox.Show(
                "Microsoft Edge WebView2 Runtime is required but not detected on your system.\n\n" +
                "Please download and install it from:\n" +
                "https://developer.microsoft.com/en-us/microsoft-edge/webview2/\n\n" +
                "After installation, restart AI Workspace.",
                "WebView2 Required",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown();
            return;
        }

        try
        {
            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "AIWorkspace", "WebView2"));

            await WebView.EnsureCoreWebView2Async(env);
        }
        catch (Exception ex)
        {
            AIWorkspace.Core.Utils.FileLogger.Error(ex);
            System.Windows.MessageBox.Show(
                $"Failed to initialize WebView2: {ex.Message}\n\n" +
                "Please ensure WebView2 Runtime is installed and try again.",
                "Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            System.Windows.Application.Current.Shutdown();
            return;
        }

        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        WebView.CoreWebView2.Settings.IsZoomControlEnabled = true;

        // Load window settings
        _settingsService = new SettingsService(new DatabaseService());
        LoadWindowSettings();

        var assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        var indexPath = Path.Combine(assetsPath, "index.html");

        if (File.Exists(indexPath))
        {
            // Use virtual host mapping instead of file:// to support ES modules
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "aiworkspace.local",
                assetsPath,
                CoreWebView2HostResourceAccessKind.Allow);
            WebView.CoreWebView2.Navigate("http://aiworkspace.local/index.html");
        }
        else
        {
            WebView.CoreWebView2.NavigateToString(@"
                <html><body style='background:#1e1e1e;color:#fff;font-family:sans-serif;display:flex;align-items:center;justify-content:center;height:100vh;margin:0;'>
                <div>AI Workspace</div></body></html>");
        }

        _bridge = new WebBridge(WebView);
        _bridge.Register();
        _bridge.ShortcutSettingsChanged += ReloadHotkeys;
        _bridge.ThemeChanged += ApplyMenuTheme;

        // Apply menu theme based on saved setting
        ApplyMenuTheme(_settingsService?.Get("theme") ?? "dark");

        // Register global hotkeys
        _hotkeyService = new HotkeyService(this);
        RegisterHotkeys();
    }

    private void RegisterHotkeys()
    {
        if (_hotkeyService == null) return;

        var screenshotShortcut = _settingsService?.Get("shortcut_screenshot") ?? "Ctrl+Shift+A";
        if (!_hotkeyService.Register(screenshotShortcut, TriggerScreenshot))
        {
            AIWorkspace.Core.Utils.FileLogger.Warning($"Failed to register hotkey: {screenshotShortcut}");
        }

        var toggleShortcut = _settingsService?.Get("shortcut_activate") ?? "Ctrl+`";
        if (!_hotkeyService.Register(toggleShortcut, ToggleWindowVisibility))
        {
            AIWorkspace.Core.Utils.FileLogger.Warning($"Failed to register hotkey: {toggleShortcut}");
        }
    }

    private void ReloadHotkeys()
    {
        _hotkeyService?.UnregisterAll();
        RegisterHotkeys();
        AIWorkspace.Core.Utils.FileLogger.Info("Hotkeys reloaded due to settings change.");
    }

    public void TriggerScreenshot()
    {
        Dispatcher.Invoke(() =>
        {
            _bridge?.SendScreenshotRequest();
        });
    }

    private void ToggleWindowVisibility()
    {
        Dispatcher.Invoke(() =>
        {
            if (Visibility == Visibility.Visible && WindowState != WindowState.Minimized)
            {
                Hide();
            }
            else
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
        });
    }

    public void ForceClose()
    {
        _isClosingToTray = true;
        Dispatcher.Invoke(() => Close());
    }

    private void MenuLogin_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show(
            "登录功能即将推出，敬请期待。",
            "登录",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void MenuSettings_Click(object sender, RoutedEventArgs e)
    {
        var msg = new System.Text.Json.Nodes.JsonObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["channel"] = "app:command",
            ["payload"] = new System.Text.Json.Nodes.JsonObject { ["command"] = "openSettings" }
        };
        WebView.CoreWebView2?.PostWebMessageAsJson(msg.ToJsonString());
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show(
            "AI Workspace\n版本: 1.0.0\n\n一款面向技术人员的 Windows 桌面 AI 分析工作台，\n支持多模型接入、技术日志分析与截图识别。",
            "关于 AI Workspace",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void ApplyMenuTheme(string theme)
    {
        Dispatcher.Invoke(() =>
        {
            if (theme == "light")
            {
                TopMenu.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
                TopMenu.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 41, 55));
                TopMenu.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(209, 213, 219));
                // Sync window background with frontend theme
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            }
            else
            {
                TopMenu.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 48));
                TopMenu.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 204, 204));
                TopMenu.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(75, 75, 79));
                // Sync window background with frontend theme
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30));
            }
        });
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isClosingToTray || Environment.HasShutdownStarted)
        {
            return; // Allow close
        }

        e.Cancel = true;
        Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveWindowSettings();
        _hotkeyService?.Dispose();
        _hotkeyService = null;
        base.OnClosed(e);
    }

    private void LoadWindowSettings()
    {
        if (_settingsService == null) return;

        var widthStr = _settingsService.Get("window_width");
        var heightStr = _settingsService.Get("window_height");

        if (int.TryParse(widthStr, out var width) && width >= 800)
            Width = width;
        if (int.TryParse(heightStr, out var height) && height >= 600)
            Height = height;
    }

    private void SaveWindowSettings()
    {
        if (_settingsService == null) return;

        _settingsService.Set("window_width", ((int)Width).ToString());
        _settingsService.Set("window_height", ((int)Height).ToString());
    }
}
