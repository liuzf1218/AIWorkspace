# AI Workspace

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![Android](https://img.shields.io/badge/Android-API%2034-3DDC84?logo=android)](https://developer.android.com/)

> 面向技术人员的 AI 分析工作台 —— 统一接入多模型，围绕日志、截图、文件构建完整分析工作流。
>
> An AI analysis workbench for technical professionals — unified multi-model access, built around logs, screenshots, and files.

---

## 📖 项目简介 / Introduction

AI Workspace 是一款为开发者和技术人员设计的 AI 助手客户端，支持在 **Windows 桌面端** 和 **Android 移动端** 上无缝使用。

无论你是需要分析一段报错日志、理解一张系统截图，还是让 AI 帮你梳理代码逻辑，AI Workspace 都能通过统一的多模型接入层，为你提供流式、高效的 AI 对话体验。

---

**AI Workspace** is an AI assistant client designed for developers and technical professionals, available seamlessly on **Windows Desktop** and **Android Mobile**.

Whether you need to analyze an error log, understand a system screenshot, or have AI help you sort through code logic, AI Workspace provides a streaming, efficient AI conversation experience through a unified multi-model access layer.

### 核心设计理念 / Core Design Philosophy

| 中文 | English |
|------|---------|
| **多模型统一接入**：支持任意 OpenAI 兼容 API（Claude / GPT-4o / DeepSeek / Gemini / Ollama 等） | **Unified Multi-Model Access**: Supports any OpenAI-compatible API (Claude / GPT-4o / DeepSeek / Gemini / Ollama, etc.) |
| **原生能力 + 现代 UI**：桌面端采用 WPF + WebView2 + React，兼顾 Windows 原生能力与现代化界面 | **Native Capabilities + Modern UI**: Desktop uses WPF + WebView2 + React, balancing native Windows capabilities with a modern interface |
| **数据本地优先**：SQLite 本地持久化，API Key 系统级加密（Windows DPAPI / Android Keystore） | **Local-First Data**: SQLite local persistence, system-level API Key encryption (Windows DPAPI / Android Keystore) |
| **技术工作流闭环**：截图 → AI 识图、日志 → 结构化分析、文件 → 智能解读 | **Technical Workflow Closed Loop**: Screenshot → AI vision analysis, Logs → structured analysis, Files → intelligent interpretation |

---

## ✨ 功能特性 / Features

### 桌面端（Windows）/ Desktop (Windows)

| 中文 | English |
|------|---------|
| 🤖 **多模型对话** | 🤖 **Multi-Model Chat** |
| 统一接入 OpenAI 兼容 API，支持流式输出与多轮上下文 | Unified OpenAI-compatible API access with streaming output and multi-turn context |
| 📷 **区域截图** | 📷 **Area Screenshot** |
| `Ctrl+Shift+A` 全局热键触发区域截图，直接发送给 Vision 模型分析 | `Ctrl+Shift+A` global hotkey triggers area screenshot, sent directly to vision models |
| 📁 **文件分析** | 📁 **File Analysis** |
| 拖拽或选择文本/JSON/XML/日志等文件，自动格式化并送入对话 | Drag or select text/JSON/XML/log files, auto-formatted and sent to conversation |
| 🔧 **Provider 管理** | 🔧 **Provider Management** |
| 添加、编辑、删除多 Provider，自动获取模型列表，API Key 加密存储 | Add, edit, delete multiple providers, auto-fetch model lists, API Key encrypted storage |
| 💻 **系统托盘** | 💻 **System Tray** |
| 关闭窗口最小化到托盘，快捷键 `Ctrl+`` ` 唤起/隐藏 | Close to system tray, `Ctrl+`` ` hotkey to show/hide |
| 🌙 **深色主题** | 🌙 **Dark Theme** |
| 整体暗色界面，专为长时间编码场景优化 | Full dark interface, optimized for long coding sessions |

### 移动端（Android）/ Mobile (Android)

| 中文 | English |
|------|---------|
| 📱 **原生 Android 体验** | 📱 **Native Android Experience** |
| Kotlin + Jetpack Compose，Material3 设计 | Kotlin + Jetpack Compose, Material3 design |
| 📷 **相机/相册** | 📷 **Camera / Gallery** |
| 拍照或选择图片，支持多模态对话 | Take photos or select images, supporting multimodal conversations |
| 🔐 **安全存储** | 🔐 **Secure Storage** |
| Android Keystore AES-GCM 加密 API Key | Android Keystore AES-GCM encryption for API Keys |
| 🌗 **主题切换** | 🌗 **Theme Toggle** |
| 支持暗色/亮色模式，持久化记忆 | Dark / light mode support with persistent memory |
| 🔍 **历史搜索** | 🔍 **History Search** |
| 聊天记录本地搜索 | Local chat history search |

---

## 🏗️ 技术架构 / Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      React 18 Frontend                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │   Sidebar   │  │  ChatArea   │  │  Settings/Provider  │  │
│  │  (Chat List)│  │(Message+Inp)│  │      (Modal)        │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ IPC (window.chrome.webview)
┌──────────────────────────▼────────────────────────────────────┐
│                     WPF Host Layer (Windows)                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐  │
│  │ WebBridge   │  │ Screenshot  │  │   HotkeyService     │  │
│  │  (IPC Router)│  │   Service   │  │   (Global Hotkey)   │  │
│  └─────────────┘  └─────────────┘  └─────────────────────┘  │
└──────────────────────────┬────────────────────────────────────┘
                           │ Class Library Reference
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
>
> The mobile client uses an independent Kotlin + Jetpack Compose codebase, directly reusing the desktop SQLite data schema.

---

## 🚀 快速开始 / Quick Start

### 环境要求 / Requirements

| 组件 / Component | 版本 / Version | 说明 / Note |
|------------------|----------------|-------------|
| Windows | 10 1903+ / 11 | Desktop runtime |
| .NET SDK | 8.0+ | Desktop backend build |
| Node.js | 18+ | Desktop frontend build |
| WebView2 Runtime | Evergreen | Desktop runtime dependency |
| Android Studio | Hedgehog+ | Android IDE |
| JDK | 17 | Android build |
| Android SDK | 34 | Android target version |

### 桌面端构建 / Desktop Build

```powershell
# 方式一：使用 PowerShell 脚本（推荐）/ Method 1: PowerShell script (recommended)
.\scripts\build.ps1

# 方式二：手动分步 / Method 2: Manual steps
cd src/AIWorkspace.Web
npm install
npm run build          # 输出到 ../AIWorkspace.WPF/Assets / Output to ../AIWorkspace.WPF/Assets

cd ../../
dotnet restore
dotnet build src/AIWorkspace.WPF/AIWorkspace.WPF.csproj -c Release

# 单文件发布 / Single-file publish
dotnet publish src/AIWorkspace.WPF/AIWorkspace.WPF.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\publish
```

### Android 端构建 / Android Build

```bash
cd src/AIWorkspace.Android
./gradlew assembleDebug

# 安装到设备 / Install to device
./gradlew installDebug
```

### 前端独立开发 / Frontend Independent Development

```bash
cd src/AIWorkspace.Web
npm run dev    # http://localhost:5173
```

> 独立开发时 `window.chrome.webview` 不存在，截图/文件等原生功能不可用，仅用于 UI 调试。
>
> During independent development, `window.chrome.webview` is unavailable; screenshot/file and other native features are disabled. For UI debugging only.

---

## 📁 项目结构 / Project Structure

```
AIWorkspace.sln
├── docs/
│   ├── DEVELOPMENT.md          # 详细开发文档 / Detailed dev guide
│   └── ROADMAP.md              # 产品演进路线图 / Product roadmap
├── scripts/
│   ├── build.ps1               # 一键构建脚本 / One-click build script
│   └── setup-db.sql            # 数据库初始化参考 / DB init reference
├── src/
│   ├── AIWorkspace.Core/       # .NET 8 类库：业务逻辑与数据层 / Business logic & data layer
│   ├── AIWorkspace.WPF/        # .NET 8 WPF 应用：Windows 原生宿主 / Windows native host
│   ├── AIWorkspace.Web/        # React 18 + TypeScript 前端 / Frontend
│   └── AIWorkspace.Android/    # Kotlin + Jetpack Compose 移动端 / Mobile client
├── tests/                      # 单元测试（预留）/ Unit tests (reserved)
├── ARCHITECTURE.md             # 架构概览与代码地图 / Architecture overview
└── README.md                   # 本文件 / This file
```

---

## 📚 文档 / Documentation

| 文档 / Document | 说明 / Description |
|-----------------|--------------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | 整体架构、模块职责、代码定位指南 / Overall architecture, module responsibilities, code navigation |
| [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) | 构建/调试/扩展指南、IPC 协议、Provider 开发 / Build/debug/extend guide, IPC protocol, provider development |
| [docs/ROADMAP.md](docs/ROADMAP.md) | 产品路线图与技术债务 / Product roadmap & technical debt |
| [src/AIWorkspace.Core/README.md](src/AIWorkspace.Core/README.md) | Core 层文件功能说明 / Core layer file documentation |
| [src/AIWorkspace.WPF/README.md](src/AIWorkspace.WPF/README.md) | WPF 层文件功能说明 / WPF layer file documentation |
| [src/AIWorkspace.Web/README.md](src/AIWorkspace.Web/README.md) | Web 项目配置说明 / Web project configuration |
| [src/AIWorkspace.Android/README.md](src/AIWorkspace.Android/README.md) | Android 项目说明 / Android project documentation |

---

## 🛣️ 路线图 / Roadmap

- [x] 桌面端 Phase 1-3 / Desktop Phase 1-3: Core chat, Provider management, screenshot, system tray
- [x] Android 端 Phase 1-4 / Android Phase 1-4: Skeleton, core chat, provider management, optimization
- [ ] V1.0 Release: Installer packaging, error handling, UI polish
- [ ] V1.1: OCR fallback, Prompt Library, message editing, export
- [ ] V1.2: PDF/DOCX parsing, Context compressor, local model integration
- [ ] V2.0: MCP support, plugin system, RAG knowledge base, Agent workflow

详见 / See [docs/ROADMAP.md](docs/ROADMAP.md).

---

## 🤝 贡献指南 / Contributing

欢迎提交 Issue 和 Pull Request！

欢迎提交 Issue 和 Pull Request！

Issues and Pull Requests are welcome!

- 提交 Issue 前请先搜索是否已有相同问题 / Please search for existing issues before submitting a new one
- PR 请说明改动范围和测试情况 / PRs should describe the scope of changes and testing status
- 遵循现有代码风格 / Follow existing code style

---

## 📄 许可证 / License

本项目基于 [MIT 许可证](LICENSE) 开源。

This project is open-sourced under the [MIT License](LICENSE).

Copyright (c) 2026 liuzf1218

---

## 🙏 致谢 / Acknowledgements

感谢以下开源项目与社区 / Thanks to the following open-source projects and communities:

- [.NET](https://dotnet.microsoft.com/) · [React](https://react.dev/) · [Vite](https://vitejs.dev/) · [TailwindCSS](https://tailwindcss.com/)
- [Jetpack Compose](https://developer.android.com/jetpack/compose) · [Room](https://developer.android.com/training/data-storage/room) · [OkHttp](https://square.github.io/okhttp/)
- [WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)
