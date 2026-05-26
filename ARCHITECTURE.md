# AI Workspace 架构概览

> 本文档描述 AI Workspace 的整体架构、模块划分和代码树结构，便于快速定位和修改代码。

---

## 一、架构总览

AI Workspace 采用多端混合架构：

**桌面端（Windows）：WPF + WebView2 + React**
- **WPF** 提供原生 Windows 能力（窗口管理、系统托盘、全局热键、截图、文件对话框）
- **WebView2** 作为嵌入式浏览器，渲染 React 前端 UI
- **React 18 + TypeScript** 构建现代化界面，通过 IPC 与 .NET 后端通信
- **.NET 8 Core 类库** 承载业务逻辑（Provider 管理、聊天流、数据库、Prompt 增强）

**移动端（Android）：Kotlin + Jetpack Compose**
- **Kotlin** 原生 Android 开发
- **Jetpack Compose** 声明式 UI
- **Room** 本地数据库（复用桌面端 SQLite schema）
- **OkHttp + SSE** 流式 AI 对话
- **Android Keystore** API Key 加密

```
┌─────────────────────────────────────────────────────────────┐
│                      React 18 前端                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Sidebar   │  │  ChatArea   │  │  Settings/Provider  │  │
│  │  (对话列表)  │  │ (消息+输入) │  │      (弹窗)          │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ window.chrome.webview.postMessage
┌──────────────────────────▼────────────────────────────────────┐
│                      WebView2 Runtime                           │
│              http://aiworkspace.local (虚拟主机映射)              │
└──────────────────────────┬────────────────────────────────────┘
                           │ CoreWebView2.WebMessageReceived
┌──────────────────────────▼────────────────────────────────────┐
│                     WPF 宿主层                                 │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │  MainWindow.xaml.cs  ── 窗口/WebView2 初始化             │  │
│  │  App.xaml.cs         ── 系统托盘/生命周期                 │  │
│  │  WebBridge.cs        ── IPC 路由/消息转发                 │  │
│  │  ScreenshotService   ── 区域截图+压缩                     │  │
│  │  HotkeyService       ── 全局热键注册                     │  │
│  └─────────────────────────────────────────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ 类库引用
┌──────────────────────────▼────────────────────────────────────┐
│                   AIWorkspace.Core (.NET 8)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Models    │  │  Providers  │  │      Services       │  │
│  │  数据实体    │  │ LLM 接入层   │  │ 业务逻辑/数据访问    │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐                            │
│  │    Utils    │  │   SQLite    │                            │
│  │ 工具/解析   │  │ 本地数据库   │                            │
│  └─────────────┘  └─────────────┘                            │
└───────────────────────────────────────────────────────────────┘
```

---

## 二、代码树（Code Map）

```
AIWorkspace.sln
├── docs/
│   ├── DEVELOPMENT.md          # 详细开发文档（构建/调试/扩展指南）
│   └── ROADMAP.md              # 产品演进路线图
├── scripts/
│   ├── build.ps1               # 一键构建脚本（前端→WPF→单文件发布）
│   └── setup-db.sql            # 数据库初始化参考脚本
├── src/
│   ├── AIWorkspace.Core/       # .NET 8 类库：业务逻辑与数据层
│   │   ├── README.md           # ← Core 层文件说明
│   │   ├── Models/             # 数据实体与 DTO
│   │   │   ├── ChatModels.cs
│   │   │   ├── Conversation.cs
│   │   │   └── Provider.cs
│   │   ├── Providers/          # AI Provider 抽象与实现
│   │   │   ├── IProvider.cs
│   │   │   └── OpenAICompatibleProvider.cs
│   │   ├── Services/           # 业务服务（数据库/聊天/Prompt）
│   │   │   ├── ChatService.cs
│   │   │   ├── DatabaseService.cs
│   │   │   ├── EncryptionService.cs
│   │   │   ├── PromptEngine.cs
│   │   │   ├── ProviderService.cs
│   │   │   └── SettingsService.cs
│   │   └── Utils/              # 工具类（文件解析/日志分析）
│   │       ├── FileLogger.cs
│   │       ├── FileProcessor.cs
│   │       └── LogParser.cs
│   ├── AIWorkspace.WPF/        # .NET 8 WPF 应用：Windows 原生宿主
│   │   ├── README.md           # ← WPF 层文件说明
│   │   ├── App.xaml            # WPF 应用入口
│   │   ├── MainWindow.xaml     # 主窗口（WebView2 宿主）
│   │   ├── Bridge/             # IPC 通信桥
│   │   ├── Native/             # 原生 Windows API 封装
│   │   └── Assets/             # React 前端构建产物（由 Vite 输出）
│   ├── AIWorkspace.Web/        # React 18 + TypeScript 前端
│   │   ├── README.md           # ← Web 项目配置说明
│   │   ├── package.json        # npm 依赖与脚本
│   │   ├── vite.config.ts      # Vite 构建配置
│   │   ├── tailwind.config.js  # TailwindCSS 主题配置
│   │   └── src/
│   │       ├── README.md       # ← 前端源码结构说明
│   │       ├── main.tsx        # React 应用入口
│   │       ├── App.tsx         # 根组件
│   │       ├── index.css       # 全局样式
│   │       ├── types/          # TypeScript 类型定义
│   │       ├── services/       # IPC 通信封装
│   │       ├── stores/         # Zustand 全局状态
│   │       └── components/     # React 组件
│   │           ├── Layout/     # 布局（Sidebar + 整体框架）
│   │           ├── Chat/       # 聊天（消息列表/输入/区域）
│   │           ├── Markdown/   # Markdown 渲染（代码高亮+复制）
│   │           ├── Settings/   # 设置弹窗（Provider/通用设置）
│   │           └── ErrorBoundary/
│   └── AIWorkspace.Android/    # Kotlin + Jetpack Compose 移动端
│       ├── README.md           # ← Android 项目说明
│       ├── app/build.gradle.kts # 应用构建配置
│       └── app/src/main/java/com/aiworkspace/
│           ├── data/           # Room Entity / DAO / Repository
│           ├── network/        # OkHttp SSE / DTO
│           ├── ui/             # Compose Screens / Components / Theme
│           ├── utils/          # ImageUtils 等工具
│           ├── viewmodel/      # MVVM ViewModel
│           ├── MainActivity.kt
│           └── AIWorkspaceApplication.kt
├── tests/                      # 单元测试（预留）
└── ARCHITECTURE.md             # ← 本文件
```

---

## 三、模块职责速查

| 模块 | 技术栈 | 核心职责 | 入口文件 |
|------|--------|----------|----------|
| **Core** | .NET 8 | 业务逻辑、数据持久化、AI 流式调用、Prompt 增强 | `AIWorkspace.Core.csproj` |
| **WPF** | .NET 8 + WPF | 窗口管理、WebView2 宿主、系统托盘、热键、截图 | `MainWindow.xaml.cs` |
| **Web** | React 18 + TS + Vite | UI 渲染、用户交互、状态管理 | `main.tsx` |
| **Android** | Kotlin + Compose | 移动端聊天、相机、主题、Provider 管理 | `MainActivity.kt` |

---

## 四、关键数据流

### 发送消息流

```
用户输入 → ChatInput.tsx
    → useChatStore.sendMessage()
    → bridge.send('chat:send')
    → WebBridge.HandleChatSend()
    → OpenAICompatibleProvider.StreamChatAsync() [SSE]
    → bridge.on('chat:chunk') → appendStream()
    → MessageList 渲染流式内容
```

### 截图流

```
点击截图 → bridge.send('screenshot:capture')
    → ScreenshotService.CaptureArea() [区域选择]
    → 压缩/编码 → bridge.on('screenshot:ready')
    → ChatInput 显示缩略图
    → 发送时随消息一起传给 Vision 模型
```

---

## 五、如何定位代码

| 想修改的功能 | 应查看的文件 |
|-------------|-------------|
| **Android** | |
| Android 聊天界面/消息列表 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/screens/ChatScreen.kt` |
| Android 输入框/相机/相册 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/components/ChatInput.kt` |
| Android Provider 管理 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/screens/ProviderManagementScreen.kt` |
| Android 设置/主题 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/ui/screens/SettingsScreen.kt` |
| Android ViewModel/业务逻辑 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/viewmodel/ChatViewModel.kt` |
| Android 数据库/加密 | `src/AIWorkspace.Android/app/src/main/java/com/aiworkspace/data/` |
| **桌面端** | |
| 聊天消息布局/滚动/气泡样式 | `src/AIWorkspace.Web/src/components/Chat/MessageList.tsx` |
| 输入框/截图按钮/发送逻辑 | `src/AIWorkspace.Web/src/components/Chat/ChatInput.tsx` |
| 顶部 Provider/模型下拉菜单 | `src/AIWorkspace.Web/src/components/Chat/ChatArea.tsx` |
| 侧边栏折叠/对话列表 | `src/AIWorkspace.Web/src/components/Layout/Sidebar.tsx` |
| 代码块样式/复制按钮 | `src/AIWorkspace.Web/src/components/Markdown/MarkdownRenderer.tsx` |
| 流式输出状态管理 | `src/AIWorkspace.Web/src/stores/useChatStore.ts` |
| Provider 配置弹窗 | `src/AIWorkspace.Web/src/components/Settings/ProviderModal.tsx` |
| 全局设置弹窗 | `src/AIWorkspace.Web/src/components/Settings/SettingsModal.tsx` |
| 截图热键/窗口唤起 | `src/AIWorkspace.WPF/Native/HotkeyService.cs` |
| 截图区域选择/压缩 | `src/AIWorkspace.WPF/Native/ScreenshotService.cs` |
| IPC 消息路由 | `src/AIWorkspace.WPF/Bridge/WebBridge.cs` |
| AI 流式对话/SSE 解析 | `src/AIWorkspace.Core/Providers/OpenAICompatibleProvider.cs` |
| 聊天业务逻辑/数据持久化 | `src/AIWorkspace.Core/Services/ChatService.cs` |
| 自动 Prompt 增强 | `src/AIWorkspace.Core/Services/PromptEngine.cs` |
| 数据库建表/连接 | `src/AIWorkspace.Core/Services/DatabaseService.cs` |
| Provider/设置数据服务 | `src/AIWorkspace.Core/Services/ProviderService.cs` + `SettingsService.cs` |
| 文件日志 | `src/AIWorkspace.Core/Utils/FileLogger.cs` |
| 文件解析/日志分析 | `src/AIWorkspace.Core/Utils/FileProcessor.cs` + `LogParser.cs` |
| 窗口大小/关闭到托盘 | `src/AIWorkspace.WPF/MainWindow.xaml.cs` |
