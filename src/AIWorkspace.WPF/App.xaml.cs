using System.Windows;
using System.Windows.Forms;

namespace AIWorkspace.WPF;

public partial class App : System.Windows.Application
{
    private NotifyIcon? _notifyIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "AI Workspace",
            Visible = true,
        };

        // Use dynamically generated icon
        try
        {
            var bmp = GenerateTrayIconBitmap();
            var hIcon = bmp.GetHicon();
            _notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
        }
        catch
        {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show", null, (s, e) => ShowMainWindow());
        contextMenu.Items.Add("Screenshot", null, (s, e) => TriggerScreenshot());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, (s, e) => ShutdownApp());
        _notifyIcon.ContextMenuStrip = contextMenu;

        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        var window = Current.MainWindow;
        if (window == null) return;

        window.Show();
        window.WindowState = WindowState.Normal;
        window.Activate();
    }

    private void TriggerScreenshot()
    {
        var window = Current.MainWindow as MainWindow;
        window?.TriggerScreenshot();
    }

    private void ShutdownApp()
    {
        var window = Current.MainWindow as MainWindow;
        window?.ForceClose();
        _notifyIcon?.Dispose();
        Shutdown();
    }

    private static System.Drawing.Bitmap GenerateTrayIconBitmap()
    {
        const int size = 64;
        var bmp = new System.Drawing.Bitmap(size, size);
        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.Transparent);
            using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(30, 30, 30));
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            var r = 12;
            path.AddArc(2, 2, r, r, 180, 90);
            path.AddArc(size - 2 - r, 2, r, r, 270, 90);
            path.AddArc(size - 2 - r, size - 2 - r, r, r, 0, 90);
            path.AddArc(2, size - 2 - r, r, r, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
            using var font = new System.Drawing.Font("Segoe UI", 22, System.Drawing.FontStyle.Bold);
            var text = "AI";
            var textSize = g.MeasureString(text, font);
            g.DrawString(text, font, System.Drawing.Brushes.White,
                (size - textSize.Width) / 2, (size - textSize.Height) / 2 - 2);
        }
        return bmp;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon?.Dispose();
        base.OnExit(e);
    }
}
