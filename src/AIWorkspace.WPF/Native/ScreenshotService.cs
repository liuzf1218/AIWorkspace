using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace AIWorkspace.WPF.Native;

public class ScreenshotService
{
    public event EventHandler<string>? ScreenshotCaptured;

    public void CaptureArea(Action? onComplete = null)
    {
        var window = new ScreenshotWindow();
        bool captured = false;

        window.Captured += (s, bitmap) =>
        {
            captured = true;
            var base64 = CompressAndEncode(bitmap);
            ScreenshotCaptured?.Invoke(this, base64);
            onComplete?.Invoke();
        };

        window.Closed += (s, e) =>
        {
            if (!captured)
            {
                onComplete?.Invoke();
            }
        };

        window.Show();
    }

    public static string CompressAndEncode(Bitmap bitmap, int maxLongEdge = 1536, int quality = 85)
    {
        // Resize if needed
        var width = bitmap.Width;
        var height = bitmap.Height;
        var maxDim = Math.Max(width, height);

        if (maxDim > maxLongEdge)
        {
            var ratio = (double)maxLongEdge / maxDim;
            width = (int)(width * ratio);
            height = (int)(height * ratio);
            var resized = new Bitmap(width, height);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, width, height);
            }
            bitmap.Dispose();
            bitmap = resized;
        }

        // Encode to JPEG
        using var ms = new MemoryStream();
        var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
            .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
        bitmap.Save(ms, encoder, parameters);
        bitmap.Dispose();

        return Convert.ToBase64String(ms.ToArray());
    }
}

public class ScreenshotWindow : Window
{
    public event EventHandler<Bitmap>? Captured;

    private System.Windows.Point _startPoint;
    private Rectangle? _selectionRect;
    private Canvas _canvas = null!;
    private Bitmap? _screenBitmap;

    public ScreenshotWindow()
    {
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        Cursor = System.Windows.Input.Cursors.Cross;
        KeyDown += OnKeyDown;
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _canvas = new Canvas();
        Content = _canvas;

        // Capture full screen
        var screen = SystemParameters.WorkArea;
        _screenBitmap = new Bitmap((int)screen.Width, (int)screen.Height);
        using (var g = Graphics.FromImage(_screenBitmap))
        {
            g.CopyFromScreen((int)screen.Left, (int)screen.Top, 0, 0,
                new System.Drawing.Size((int)screen.Width, (int)screen.Height));
        }

        // Show semi-transparent overlay
        var overlay = new Rectangle
        {
            Width = screen.Width,
            Height = screen.Height,
            Fill = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0))
        };
        _canvas.Children.Add(overlay);
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(this);
        _selectionRect = new Rectangle
        {
            Stroke = Brushes.White,
            StrokeThickness = 2,
            Fill = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255))
        };
        _canvas.Children.Add(_selectionRect);
    }

    private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_selectionRect == null || e.LeftButton != MouseButtonState.Pressed) return;

        var pos = e.GetPosition(this);
        var x = Math.Min(_startPoint.X, pos.X);
        var y = Math.Min(_startPoint.Y, pos.Y);
        var w = Math.Abs(pos.X - _startPoint.X);
        var h = Math.Abs(pos.Y - _startPoint.Y);

        Canvas.SetLeft(_selectionRect, x);
        Canvas.SetTop(_selectionRect, y);
        _selectionRect.Width = w;
        _selectionRect.Height = h;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_selectionRect == null || _screenBitmap == null) return;

        var x = (int)Canvas.GetLeft(_selectionRect);
        var y = (int)Canvas.GetTop(_selectionRect);
        var w = (int)_selectionRect.Width;
        var h = (int)_selectionRect.Height;

        if (w > 10 && h > 10)
        {
            var cropped = _screenBitmap.Clone(
                new System.Drawing.Rectangle(x, y, w, h),
                _screenBitmap.PixelFormat);
            Captured?.Invoke(this, cropped);
        }

        _screenBitmap?.Dispose();
        Close();
    }
}
