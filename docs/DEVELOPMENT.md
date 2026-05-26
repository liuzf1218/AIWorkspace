# AI Workspace 开发文档

> 版本：V1.2（对应代码版本：Phase 4 功能开发中）  
> 面向开发者的构建、调试、扩展指南。

---

## 目录

1. [项目概述](#一项目概述)
2. [快速开始](#二快速开始)
3. [代码定位指南](#三代码定位指南)
4. [技术架构](#四技术架构)
5. [核心数据流](#五核心数据流)
6. [IPC 通信协议](#六ipc-通信协议)
7. [Provider 开发指南](#七provider-开发指南)
8. [Prompt 策略扩展](#八prompt-策略扩展)
9. [数据库操作](#九数据库操作)
10. [调试指南](#十调试指南)
11. [常见问题](#十一常见问题)
12. [文档地图](#十二文档地图)

---

## 一、项目概述

AI Workspace 是一款面向技术人员的 AI 分析工作台，采用多端混合架构。

- **桌面端（Windows）**：WPF + WebView2 + React
- **移动端（Android）**：Kotlin + Jetpack Compose

核心定位是统一接入多模型，围绕技术日志、截图、文件构建完整分析工作流。

### 关键设计决策

| 决策 | 说明 |
|------|------|
| WPF + WebView2 | 原生能力（截图、DPAPI、文件对话框）+ 现代 Web UI |
| 单文件发布 | `--self-contained true -p:PublishSingleFile=true` |
| SQLite | 零配置本地持久化，路径 `%LOCALAPPDATA%\AIWorkspace\aiworkspace.db` |
| DPAPI | Windows 用户级加密，无需管理密钥 |
| OpenAI Compatible | 统一协议覆盖 90%+ 主流模型 |
| 截图走多模态 | 直接发 Base64 图片给 Claude/GPT-4o，不做 OCR |
| 虚拟主机映射 | `http://aiworkspace.local` 替代 `file://`，避免 ES Module 被拦截 |

---

## 二、快速开始

### 环境要求

| 组件 | 版本 | 说明 |
|------|------|------|
| Windows | 10 1903+ / 11 | 桌面端 WebView2 最低要求 |
| .NET SDK | 8.0+ | 编译桌面端后端 |
| Node.js | 18+ | 编译桌面端前端 |
| WebView2 Runtime | Evergreen | 桌面端运行时依赖 |
| Android Studio | Hedgehog+ | Android 开发 IDE |
| JDK | 17 | Android 编译 |
| Android SDK | 34 | Android 目标版本 |

### 构建步骤

```powershell
# 桌面端构建
# 方式一：使用 PowerShell 脚本（推荐）
.\scripts\build.ps1

# 方式二：手动分步
cd src/AIWorkspace.Web
npm install
npm run build          # 输出到 ../AIWorkspace.WPF/Assets

cd ../../
dotnet restore
dotnet build src/AIWorkspace.WPF/AIWorkspace.WPF.csproj -c Release

# 单文件发布
dotnet publish src/AIWorkspace.WPF/AIWorkspace.WPF.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\publish
```

```bash
# Android 端构建
cd src/AIWorkspace.Android
./gradlew assembleDebug

# 安装到设备
./gradlew installDebug
```

### 开发调试

**前端独立开发：**
```bash
cd src/AIWorkspace.Web
npm run dev    # http://localhost:5173
```
此时 `window.chrome.webview` 不存在，`bridge.ts` 会 fallback 到 console.log。

**WPF 调试：**
- 在 Visual Studio 或 VS Code 中打开 `AIWorkspace.sln`
- 设置启动项目为 `AIWorkspace.WPF`
- F5 调试运行

---

## 三、代码定位指南

### 按功能定位

| 想修改的功能 | 前端文件 | 后端文件 |
|-------------|----------|----------|
| **Android 端** | | |
| Android 聊天界面 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/screens/ChatScreen.kt` | — |
| Android 相机/相册 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/components/ChatInput.kt` | — |
| Android Provider 管理 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/screens/ProviderManagementScreen.kt` | — |
| Android 主题/设置 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/screens/SettingsScreen.kt` | — |
| Android ViewModel | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/viewmodel/ChatViewModel.kt` | — |
| Android 数据库 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/data/db/` | — |
| **桌面端** | | |
| 聊天消息布局/气泡/滚动 | `src/AIWorkspace.Web/src/components/Chat/MessageList.tsx` | — |
| 输入框/截图/文件/发送 | `src/AIWorkspace.Web/src/components/Chat/ChatInput.tsx` | — |
| 顶部 Provider/模型下拉菜单 | `src/AIWorkspace.Web/src/components/Chat/ChatArea.tsx` | — |
| 侧边栏折叠/对话列表 | `src/AIWorkspace.Web/src/components/Layout/Sidebar.tsx` | — |
| 代码块样式/复制按钮 | `src/AIWorkspace.Web/src/components/Markdown/MarkdownRenderer.tsx` | — |
| 流式输出/打字机效果 | `src/AIWorkspace.Web/src/stores/useChatStore.ts` + `MessageList.tsx` | `src/AIWorkspace.WPF/Bridge/WebBridge.cs` |
| Provider 配置弹窗 | `src/AIWorkspace.Web/src/components/Settings/ProviderModal.tsx` | `src/AIWorkspace.Core/Services/ProviderService.cs` |
| 全局设置弹窗 | `src/AIWorkspace.Web/src/components/Settings/SettingsModal.tsx` | `src/AIWorkspace.Core/Services/SettingsService.cs` |
| 截图热键/窗口唤起 | — | `src/AIWorkspace.WPF/Native/HotkeyService.cs` |
| 截图区域选择/压缩 | — | `src/AIWorkspace.WPF/Native/ScreenshotService.cs` |
| IPC 消息路由 | `src/AIWorkspace.Web/src/services/bridge.ts` | `src/AIWorkspace.WPF/Bridge/WebBridge.cs` |
| AI 流式对话/SSE | — | `src/AIWorkspace.Core/Providers/OpenAICompatibleProvider.cs` |
| 聊天业务逻辑/数据持久化 | — | `src/AIWorkspace.Core/Services/ChatService.cs` |
| Prompt 自动增强 | — | `src/AIWorkspace.Core/Services/PromptEngine.cs` |
| 数据库建表/连接 | — | `src/AIWorkspace.Core/Services/DatabaseService.cs` |
| Provider/设置数据服务 | — | `src/AIWorkspace.Core/Services/ProviderService.cs` + `SettingsService.cs` |
| 文件日志 | — | `src/AIWorkspace.Core/Utils/FileLogger.cs` |
| 文件解析/日志分析 | — | `src/AIWorkspace.Core/Utils/FileProcessor.cs` + `LogParser.cs` |
| 窗口/托盘/关闭行为 | — | `src/AIWorkspace.WPF/MainWindow.xaml.cs` + `App.xaml.cs` |

### 按模块阅读

| 模块 | 入口文档 |
|------|----------|
| 整体架构 | `ARCHITECTURE.md`（根目录） |
| Core 业务层 | `src/AIWorkspace.Core/README.md` |
| WPF 宿主层 | `src/AIWorkspace.WPF/README.md` |
| Web 前端 | `src/AIWorkspace.Web/README.md` + `src/AIWorkspace.Web/src/README.md` |


---

## 四、技术架构

```
┌─────────────────────────────────────────────────────────────┐
│                         React UI                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Sidebar   │  │  ChatArea   │  │  Settings/Provider  │  │
│  │  (会话列表)  │  │ (消息+输入) │  │      (弹窗)          │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ window.chrome.webview.postMessage
┌──────────────────────────▼────────────────────────────────────┐
│                      WebView2 Runtime                           │
│              http://aiworkspace.local (虚拟主机映射)              │
└──────────────────────────┬────────────────────────────────────┘
                           │ CoreWebView2.WebMessageReceived
┌──────────────────────────▼────────────────────────────────────┐
│                     WPF MainWindow                              │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                    WebBridge.cs                          │  │
│  │  (消息路由 -> 调用 Core Service -> 返回/流式推送)         │  │
│  └─────────────────────────────────────────────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ Screenshot  │  │   DPAPI     │  │   Win32 File Dialog │  │
│  │   Service   │  │ Encryption  │  │                     │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ 引用 Core 类库
┌──────────────────────────▼────────────────────────────────────┐
│                   AIWorkspace.Core                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │  Provider   │  │   Chat      │  │      Prompt         │  │
│  │   Layer     │  │  Service    │  │     Engine          │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   SQLite    │  │  LogParser  │  │   FileProcessor     │  │
│  │  Database   │  │             │  │                     │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└───────────────────────────────────────────────────────────────┘
```

---

## 五、核心数据流

### 5.1 发送消息（完整流程）

```
用户输入 -> 点击发送
    -> [React] ChatInput.tsx
    -> [Zustand] useChatStore.sendMessage()
    -> 乐观添加用户消息到 messages 列表
    -> 【已知问题】新消息置顶滚动未生效（待修复，见 ROADMAP.md）
    -> [IPC] bridge.send('chat:send', payload)
    -> window.chrome.webview.postMessage()
    -> [WPF] WebBridge.OnWebMessageReceived()
    -> 路由到 HandleChatSend()
    -> [Core] ChatService.AddMessage() -> SQLite
    -> [Core] OpenAICompatibleProvider.StreamChatAsync()
    -> HttpClient POST /v1/chat/completions (SSE)
    -> [API] AI Provider 返回 SSE 流
    -> [Core] 解析 SSE -> yield ChatChunk
    -> [WPF] WebBridge.SendStreamToWeb('chat:chunk', delta)
    -> CoreWebView2.PostWebMessageAsJson()
    -> [React] bridge.on('chat:chunk') -> useChatStore.appendStream()
    -> [React] MessageList 重新渲染 -> 打字机效果
    -> 滚动策略：block='nearest'（仅超视口才滚动）
    -> 收到 finish_reason
    -> [WPF] WebBridge.SendStreamToWeb('chat:done')
    -> [React] useChatStore.finishStream()
    -> 保存 assistant 消息到 SQLite
```

### 5.2 截图多模态流程

```
用户点击截图按钮（或 Ctrl+Shift+A）
    -> [React] bridge.send('screenshot:capture')
    -> [WPF] WebBridge.CaptureScreenshotWithWindowHidden()
    -> 先 Hide() 主窗口，延迟 200ms
    -> [WPF] ScreenshotService.CaptureArea()
    -> 创建全屏遮罩窗口 + 鼠标拖拽选区
    -> 用户框选区域（或按 Escape 取消）
    -> [WPF] Bitmap -> 压缩（最长边 <=1536）-> JPEG
    -> [WPF] Base64 编码
    -> [WPF] 无论成功或取消，都 Show() 恢复主窗口
    -> [WPF] bridge.SendToWeb('screenshot:ready', { base64 })
    -> [React] useChatStore.setScreenshot(base64)
    -> [React] ChatInput 显示缩略图预览
    -> 用户发送消息
    -> [React] 消息组装：text + image_base64
    -> [Core] OpenAICompatibleProvider 按 OpenAI Vision 格式发送
```

---

## 六、IPC 通信协议

### 6.1 消息格式

```typescript
interface IpcMessage {
  id: string;        // UUID，用于请求-响应匹配
  channel: string;   // 消息通道标识
  payload?: any;     // 载荷数据
  error?: string;    // 错误信息（如有）
}
```

### 6.2 通道清单

#### Web -> WPF（请求）

| Channel | Payload | 响应 |
|---------|---------|------|
| `provider:list` | — | `Provider[]` |
| `provider:save` | `Provider` | `Provider`（含 id） |
| `provider:delete` | `{ id: number }` | `{ success: true }` |
| `provider:models` | `{ id: number }` | `ModelInfo[]` |
| `provider:validate` | `{ id: number }` | `{ valid: boolean }` |
| `conversation:list` | — | `Conversation[]` |
| `conversation:create` | `{ title?, modelId?, providerId? }` | `Conversation` |
| `conversation:delete` | `{ id: string }` | `{ success: true }` |
| `conversation:load` | `{ id: string }` | `Message[]` |
| `chat:send` | `{ conversationId, providerId, modelId, content, imageData?, fileContent? }` | 流式推送 |
| `chat:abort` | — | `{ success: true }` |
| `file:read` | `{ path: string }` | `{ name, size, content, tokens }` |
| `file:open` | — | `{ name, size, content, tokens }` |
| `screenshot:capture` | — | 异步推送 `screenshot:ready` |
| `settings:get` | `{ key: string }` | `{ key, value }` |
| `settings:set` | `{ key, value }` | `{ success: true }` |
| `settings:all` | — | `Record<string, string>` |

#### WPF -> Web（推送/响应）

| Channel | Payload | 说明 |
|---------|---------|------|
| `chat:chunk` | `string` | 流式输出增量 |
| `chat:done` | `""` | 流式输出结束 |
| `chat:error` | `string` | 聊天错误 |
| `screenshot:ready` | `{ base64: string }` | 截图完成 |

### 6.3 流式输出机制

`chat:send` 采用服务端推送：React 发送后不等待响应，WPF 多次推送 `chat:chunk`，最后推送 `chat:done`。

---

## 七、Provider 开发指南

当前 `OpenAICompatibleProvider` 已覆盖绝大多数模型。如需接入非标准协议：

1. 实现 `IProvider` 接口（`src/AIWorkspace.Core/Providers/IProvider.cs`）
2. 在 `WebBridge.HandleChatSend` 中根据 Provider 特征路由到新实现类
3. （可选）在 `ProviderModal.tsx` 中添加预设模板

### Provider 配置字段

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `name` | string | Y | 显示名称 |
| `apiBaseUrl` | string | Y | API 基础 URL |
| `apiKey` | string | Y | API Key（存储时自动 DPAPI 加密） |
| `proxyUrl` | string | | HTTP/HTTPS/SOCKS5 代理 |
| `supportsVision` | bool | | 是否支持图片分析 |
| `isEnabled` | bool | | 是否启用 |

---

## 八、Prompt 策略扩展

当前 `PromptEngine` 在 `WebBridge.HandleChatSend` 中被临时 bypass（`enhancedContent = userContent`），如需恢复自动增强，移除 bypass 即可。

添加新策略：在 `PromptEngine.cs` 中实现 `IPromptStrategy`，注册到 `_strategies` 列表（特化策略在前，通用策略兜底在最后）。

---

## 九、数据库操作

### 数据库位置

```
%LOCALAPPDATA%\AIWorkspace\aiworkspace.db
```

### 手动查询

```bash
sqlite3 "$LOCALAPPDATA\\AIWorkspace\\aiworkspace.db"
SELECT id, name, api_base_url FROM providers;
SELECT role, substr(content, 1, 100), created_at FROM messages ORDER BY created_at DESC LIMIT 10;
```

### 迁移策略

当前使用自动建表。如需版本迁移：
1. 在 `settings` 表记录 `db_version`
2. 启动时检查版本，按需执行 ALTER TABLE

---

## 十、调试指南

### 前端独立开发

```bash
cd src/AIWorkspace.Web
npm run dev
```

### 开启 WebView2 DevTools

`MainWindow.xaml.cs` 中已启用，运行时按 **F12**。

### 后端日志

WPF 层使用 `Debug.WriteLine`。在 Visual Studio Output 窗口或 DebugView 工具中查看。

---

## 十一、常见问题

### Q1: WebView2 无法初始化

安装 [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)，检查 `%LOCALAPPDATA%\AIWorkspace\WebView2` 权限。

### Q2: publish 目录 exe 被占用导致构建失败

关闭正在运行的 `AIWorkspace.WPF.exe` 后再 publish，或指定新输出目录 `-o .\publish_vN`。

### Q3: 流式输出卡顿

排查网络/代理、上下文 Token 是否过长、模型本身推理速度。

### Q4: SQLite locked

确保 `DatabaseService` 单例使用，检查是否有外部工具同时打开了数据库文件。

---

## 十二、文档地图

```
项目根目录
├── ARCHITECTURE.md              <- 整体架构 + 代码树 + 模块关系
├── docs/
│   ├── DEVELOPMENT.md           <- 本文件（详细开发指南）
│   └── ROADMAP.md               <- 产品演进路线图
├── src/
│   ├── AIWorkspace.Core/
│   │   └── README.md            <- Core 层文件功能说明
│   ├── AIWorkspace.WPF/
│   │   └── README.md            <- WPF 层文件功能说明
│   └── AIWorkspace.Web/
│       ├── README.md            <- Web 项目配置说明
│       └── src/
│           └── README.md        <- 前端源码组件说明
```
