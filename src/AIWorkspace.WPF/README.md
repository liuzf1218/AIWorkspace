# AIWorkspace.WPF

> .NET 8 WPF 桌面应用，作为 React 前端的原生宿主，提供 WebView2 渲染、系统托盘、全局热键、区域截图、文件对话框等 Windows 专属能力。

---

## 文件夹结构

```
AIWorkspace.WPF/
├── AIWorkspace.WPF.csproj        # 项目文件（WPF + WebView2 + WinForms）
├── App.xaml                       # WPF 应用定义
├── App.xaml.cs                    # 应用入口：系统托盘 NotifyIcon
├── MainWindow.xaml                # 窗口 XAML（仅含 WebView2）
├── MainWindow.xaml.cs             # 主窗口逻辑：WV2 初始化/热键/窗口状态
├── Bridge/
│   └── WebBridge.cs               # IPC 消息总线（前端 ↔ .NET 通信路由）
├── Native/
│   ├── HotkeyService.cs           # Win32 全局热键注册（RegisterHotKey）
│   └── ScreenshotService.cs       # 区域截图：全屏遮罩+选区+压缩编码
└── Assets/                        # React 前端构建产物（由 Vite 输出至此）
    ├── index.html
    ├── index-*.js
    └── index-*.css
```

---

## 各文件功能详解

### 根目录文件

| 文件 | 说明 |
|------|------|
| `App.xaml` | 标准 WPF 应用定义，设置 `StartupUri="MainWindow.xaml"`。 |
| `App.xaml.cs` | **应用生命周期与系统托盘**。`OnStartup` 创建 `NotifyIcon`：右键菜单（Show/Screenshot/Exit）、双击托盘图标唤起窗口、退出时清理图标。 |
| `MainWindow.xaml` | **窗口外壳**。尺寸 1280×800，最小 800×600，深色背景 `#1e1e1e`，内含单个 `WebView2` 控件。 |
| `MainWindow.xaml.cs` | **主协调器**。职责：WebView2 Runtime 检测、环境初始化（自定义 user-data 目录）、虚拟主机映射（`aiworkspace.local` → `Assets/`）、窗口尺寸记忆（SettingsService）、创建 WebBridge 与 HotkeyService、关闭到托盘逻辑。 |

### Bridge/

| 文件 | 说明 |
|------|------|
| `WebBridge.cs` | **核心 IPC 路由器**。订阅 `WebView2.WebMessageReceived`，反序列化 JSON 消息，按 `channel` 路由到对应 Handler。支持：Provider CRUD、会话 CRUD、`chat:send` 流式处理（SSE → 前端 chunk 推送）、文件读取/打开、截图触发、设置读写。关键设计：`_uiDispatcher` 确保跨线程安全调用 `PostWebMessageAsJson`。 |

### Native/

| 文件 | 说明 |
|------|------|
| `HotkeyService.cs` | **全局热键**。封装 `user32.dll` 的 `RegisterHotKey`/`UnregisterHotKey`，通过 WndProc 拦截 `WM_HOTKEY`。支持解析 `Ctrl+Shift+A` 等字符串为 modifier + virtual-key。注册了两个默认热键：截图（`Ctrl+Shift+A`）和窗口唤起/隐藏（`` Ctrl+` ``）。 |
| `ScreenshotService.cs` | **区域截图**。`CaptureArea()` 创建全屏透明遮罩窗口（`ScreenshotWindow`），用户拖拽选区后捕获 Bitmap。压缩流程：等比缩放至最长边 ≤1536px → JPEG 编码（quality=85）→ Base64 字符串。截图前自动隐藏主窗口，完成后恢复。 |

### Assets/

由 `vite.config.ts` (`build.outDir: '../AIWorkspace.WPF/Assets'`) 自动输出前端构建产物。WPF 通过 `SetVirtualHostNameToFolderMapping("aiworkspace.local", assetsPath)` 将其映射为 HTTP 虚拟主机，避免 `file://` 协议下的 ES Module 加载限制。

---

## 关键设计模式

1. **WebView2 虚拟主机映射**：前端通过 `http://aiworkspace.local/index.html` 加载，而非 `file://`，确保 ES Modules 和 `navigator.clipboard` 正常工作。
2. **单文件发布**：`PublishSingleFile=true` + `IncludeNativeLibrariesForSelfExtract=true`，输出单个 `.exe`。
3. **关闭到托盘**：`OnClosing` 中 `e.Cancel = true; Hide();`，仅当托盘选择 Exit 或 `_isClosingToTray` 为 true 时才真正关闭。
4. **Dispatcher 线程安全**：`WebBridge` 在构造函数中保存 `webView.Dispatcher`，所有 `PostWebMessageAsJson` 都通过 `_uiDispatcher.Invoke` 执行，避免后台线程调用崩溃。
