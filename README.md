# AI Workspace

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![Android](https://img.shields.io/badge/Android-API%2034-3DDC84?logo=android)](https://developer.android.com/)

> 面向技术人员的 AI 分析工作台 —— 统一接入多模型，围绕日志、截图、文件构建完整分析工作流。

---

## 📖 项目简介

AI Workspace 是一款为开发者和技术人员设计的 AI 助手客户端，支持在 **Windows 桌面端** 和 **Android 移动端** 上无缝使用。

无论你是需要分析一段报错日志、理解一张系统截图，还是让 AI 帮你梳理代码逻辑，AI Workspace 都能通过统一的多模型接入层，为你提供流式、高效的 AI 对话体验。

### 核心设计理念

- **多模型统一接入**：支持任意 OpenAI 兼容 API（Claude / GPT-4o / DeepSeek / Gemini / Ollama 等）
- **原生能力 + 现代 UI**：桌面端采用 WPF + WebView2 + React，兼顾 Windows 原生能力与现代化界面
- **数据本地优先**：SQLite 本地持久化，API Key 系统级加密（Windows DPAPI / Android Keystore）
- **技术工作流闭环**：截图 → AI 识图、日志 → 结构化分析、文件 → 智能解读

---

## ✨ 功能特性

### 桌面端（Windows）

| 功能 | 说明 |
|------|------|
| 🤖 **多模型对话** | 统一接入 OpenAI 兼容 API，支持流式输出与多轮上下文 |
| 📷 **区域截图** | `Ctrl+Shift+A` 全局热键触发区域截图，直接发送给 Vision 模型分析 |
| 📁 **文件分析** | 拖拽或选择文本/JSON/XML/日志等文件，自动格式化并送入对话 |
| 🔧 **Provider 管理** | 添加、编辑、删除多Provider，自动获取模型列表，API Key 加密存储 |
| 💻 **系统托盘** | 关闭窗口最小化到托盘，快捷键 `Ctrl+`` ` 唤起/隐藏 |
| 🌙 **深色主题** | 整体暗色界面，专为长时间编码场景优化 |

### 移动端（Android）

| 功能 | 说明 |
|------|------|
| 📱 **原生 Android 体验** | Kotlin + Jetpack Compose，Material3 设计 |
| 📷 **相机/相册** | 拍照或选择图片，支持多模态对话 |
| 🔐 **安全存储** | Android Keystore AES-GCM 加密 API Key |
| 🌗 **主题切换** | 支持暗色/亮色模式，持久化记忆 |
| 🔍 **历史搜索** | 聊天记录本地搜索 |

---

## 🏗️ 技术架构

```
┌─────────────────────────────────────────────────────────────┐
│                      React 18 前端                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Sidebar   │  │  ChatArea   │  │  Settings/Provider  │  │
│  │  (对话列表)  │  │ (消息+输入) │  │      (弹窗)          │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ IPC (window.chrome.webview)
┌──────────────────────────▼────────────────────────────────────┐
│                     WPF 宿主层 (Windows)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ WebBridge   │  │ Screenshot  │  │   HotkeyService     │  │
│  │  (IPC路由)   │  │   Service   │  │   (全局热键)         │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ 类库引用
┌──────────────────────────▼────────────────────────────────────┐
│                   AIWorkspace.Core (.NET 8)                    │
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

> 移动端采用独立的 Kotlin + Jetpack Compose 代码库，直接复用桌面端 SQLite 数据 schema。

---

## 🚀 快速开始

### 环境要求

| 组件 | 版本 | 说明 |
|------|------|------|
| Windows | 10 1903+ / 11 | 桌面端运行环境 |
| .NET SDK | 8.0+ | 编译桌面端后端 |
| Node.js | 18+ | 编译桌面端前端 |
| WebView2 Runtime | Evergreen | 桌面端运行时依赖 |
| Android Studio | Hedgehog+ | Android 开发 IDE |
| JDK | 17 | Android 编译 |
| Android SDK | 34 | Android 目标版本 |

### 桌面端构建

```powershell
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

### Android 端构建

```bash
cd src/AIWorkspace.Android
./gradlew assembleDebug

# 安装到设备
./gradlew installDebug
```

### 前端独立开发

```bash
cd src/AIWorkspace.Web
npm run dev    # http://localhost:5173
```

> 独立开发时 `window.chrome.webview` 不存在，截图/文件等原生功能不可用，仅用于 UI 调试。

---

## 📁 项目结构

```
AIWorkspace.sln
├── docs/
│   ├── DEVELOPMENT.md          # 详细开发文档
│   ├── HOME_DEV_GUIDE.md       # 家庭/个人开发环境指南
│   └── ROADMAP.md              # 产品演进路线图
├── scripts/
│   ├── build.ps1               # 一键构建脚本
│   └── setup-db.sql            # 数据库初始化参考
├── src/
│   ├── AIWorkspace.Core/       # .NET 8 类库：业务逻辑与数据层
│   ├── AIWorkspace.WPF/        # .NET 8 WPF 应用：Windows 原生宿主
│   ├── AIWorkspace.Web/        # React 18 + TypeScript 前端
│   └── AIWorkspace.Android/    # Kotlin + Jetpack Compose 移动端
├── tests/                      # 单元测试（预留）
├── ARCHITECTURE.md             # 架构概览与代码地图
└── README.md                   # 本文件
```

---

## 📚 文档

| 文档 | 说明 |
|------|------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | 整体架构、模块职责、代码定位指南 |
| [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) | 构建/调试/扩展指南、IPC 协议、Provider 开发 |
| [docs/ROADMAP.md](docs/ROADMAP.md) | 产品路线图与技术债务 |
| [src/AIWorkspace.Core/README.md](src/AIWorkspace.Core/README.md) | Core 层文件功能说明 |
| [src/AIWorkspace.WPF/README.md](src/AIWorkspace.WPF/README.md) | WPF 层文件功能说明 |
| [src/AIWorkspace.Web/README.md](src/AIWorkspace.Web/README.md) | Web 项目配置说明 |
| [src/AIWorkspace.Android/README.md](src/AIWorkspace.Android/README.md) | Android 项目说明 |

---

## 🛣️ 路线图

- [x] 桌面端 Phase 1-3：核心聊天、Provider 管理、截图、系统托盘
- [x] Android 端 Phase 1-4：基础骨架、核心聊天、Provider 管理、完善优化
- [ ] V1.0 发布：安装包打包、错误处理强化、UI 打磨
- [ ] V1.1：OCR 降级、Prompt Library、消息编辑、导出功能
- [ ] V1.2：PDF/DOCX 解析、Context 压缩器、本地模型深度集成
- [ ] V2.0：MCP 支持、插件系统、RAG 知识库、Agent 工作流

详见 [docs/ROADMAP.md](docs/ROADMAP.md)。

---

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

- 提交 Issue 前请先搜索是否已有相同问题
- PR 请说明改动范围和测试情况
- 遵循现有代码风格

---

## 📄 许可证

本项目基于 [MIT 许可证](LICENSE) 开源。

Copyright (c) 2026 liuzf1218

---

## 🙏 致谢

感谢以下开源项目与社区：

- [.NET](https://dotnet.microsoft.com/) · [React](https://react.dev/) · [Vite](https://vitejs.dev/) · [TailwindCSS](https://tailwindcss.com/)
- [Jetpack Compose](https://developer.android.com/jetpack/compose) · [Room](https://developer.android.com/training/data-storage/room) · [OkHttp](https://square.github.io/okhttp/)
- [WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
