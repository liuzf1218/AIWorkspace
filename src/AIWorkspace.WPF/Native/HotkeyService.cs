using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace AIWorkspace.WPF.Native;

public class HotkeyService : IDisposable
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const int WM_HOTKEY = 0x0312;

    private readonly IntPtr _hWnd;
    private readonly Dictionary<int, Action> _callbacks = new();
    private int _nextId = 1;
    private bool _disposed;

    public HotkeyService(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hWnd = helper.EnsureHandle();
        HwndSource.FromHwnd(_hWnd)?.AddHook(WndProc);
    }

    public bool Register(string shortcut, Action callback)
    {
        if (!TryParseShortcut(shortcut, out var modifiers, out var key))
            return false;

        var id = _nextId++;
        if (!RegisterHotKey(_hWnd, id, modifiers, key))
            return false;

        _callbacks[id] = callback;
        return true;
    }

    public void UnregisterAll()
    {
        foreach (var id in _callbacks.Keys.ToList())
        {
            UnregisterHotKey(_hWnd, id);
        }
        _callbacks.Clear();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            var id = wParam.ToInt32();
            if (_callbacks.TryGetValue(id, out var callback))
            {
                Dispatcher.CurrentDispatcher.Invoke(() => callback());
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    private static bool TryParseShortcut(string shortcut, out uint modifiers, out uint key)
    {
        modifiers = 0;
        key = 0;

        var parts = shortcut.Split('+', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToList();

        foreach (var part in parts)
        {
            var upper = part.ToUpperInvariant();
            switch (upper)
            {
                case "CTRL":
                case "CONTROL":
                    modifiers |= MOD_CONTROL;
                    break;
                case "ALT":
                    modifiers |= MOD_ALT;
                    break;
                case "SHIFT":
                    modifiers |= MOD_SHIFT;
                    break;
                case "WIN":
                case "WINDOWS":
                    modifiers |= MOD_WIN;
                    break;
                default:
                    if (key != 0) return false;
                    if (upper == "`" || upper == "~")
                    {
                        key = 0xC0; // VK_OEM_3
                    }
                    else if (upper.Length == 1 && char.IsLetterOrDigit(upper[0]))
                    {
                        key = (uint)upper[0];
                    }
                    else if (Enum.TryParse<System.Windows.Input.Key>(part, true, out var wpfKey))
                    {
                        key = (uint)KeyInterop.VirtualKeyFromKey(wpfKey);
                    }
                    else
                    {
                        return false;
                    }
                    break;
            }
        }

        return key != 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        UnregisterAll();
        _disposed = true;
    }
}
