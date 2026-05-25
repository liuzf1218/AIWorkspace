# AI Workspace 后续演进计划

> 当前状态：Phase 1-3 已完成（基础架构 + 核心聊天 + 技术输入系统）  
> 文档版本：V1.0

---

## 目录

1. [Phase 4：完善与发布（V1.0）](#phase-4完善与发布v10)
2. [V1.1 增强](#v11-增强)
3. [V1.2 增强](#v12-增强)
4. [V2.0 大版本](#v20-大版本)
5. [技术债务](#技术债务)
6. [优先级矩阵](#优先级矩阵)

---

## Phase 4：完善与发布（V1.0）

**目标**：打磨体验，打包发布，形成可安装、可运行的桌面应用。

**预估工期**：1.5-2 周

### 4.1 全局快捷键系统

**需求**：
- `Ctrl+Shift+A`：触发区域截图
- `Ctrl+`` `（反引号）：唤起/隐藏窗口
- 支持在设置中自定义快捷键

**实现位置**：
- `AIWorkspace.WPF/Native/HotkeyService.cs`（新增）
- 注册 `RegisterHotKey` / `UnregisterHotKey` Win32 API
- 最小化到系统托盘，快捷键唤醒

**代码框架**：

```csharp
public class HotkeyService : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    public void Register(Window window, int id, ModifierKeys modifiers, Key key, Action callback)
    {
        // 注册 Win32 全局热键
        // 通过 WndProc 接收 WM_HOTKEY 消息
    }
}
```

### 4.2 系统托盘

**需求**：
- 关闭窗口时最小化到托盘
- 托盘图标右键菜单：显示窗口 / 截图 / 退出
- 单击托盘图标唤起窗口

**实现**：
- `NotifyIcon` + `ContextMenuStrip`
- 关联 `HotkeyService` 的唤起功能

### 4.3 安装包打包

**目标**：生成可安装的 `.exe` / `.msi`

**方案对比**：

| 方案 | 优点 | 缺点 | 推荐 |
|------|------|------|------|
| **InnoSetup** | 轻量、脚本化、签名友好 | 需要写 .iss 脚本 | ✅ 首选 |
| MSIX | Windows Store 可分发 | 证书、沙箱限制 | 备选 |
| NSIS | 老牌、功能丰富 | 脚本复杂 | 备选 |
| 单文件直接分发 | 最简单 | 无法卸载、无快捷方式 | 内测用 |

**InnoSetup 脚本框架**（`scripts/setup.iss`）：

```pascal
[Setup]
AppName=AI Workspace
AppVersion=1.0.0
DefaultDirName={autopf}\AI Workspace
OutputDir=..\publish
OutputBaseFilename=AIWorkspace-Setup

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\AI Workspace"; Filename: "{app}\AIWorkspace.WPF.exe"
Name: "{autodesktop}\AI Workspace"; Filename: "{app}\AIWorkspace.WPF.exe"
```

### 4.4 UI 打磨

| 任务 | 说明 | 优先级 |
|------|------|--------|
| 空状态设计 | 无会话时的引导页 | 高 |
| 加载骨架屏 | 模型列表加载、历史加载 | 中 |
| 错误边界 | React ErrorBoundary | 高 |
| 响应式适配 | 窗口缩小时的布局调整 | 中 |
| 动画优化 | 消息进入动画、Toast 提示 | 低 |

### 4.5 错误处理强化

- API 超时友好提示（当前 5 分钟，过长）
- 网络断开检测
- 自动重试机制（最多 3 次）
- 本地日志文件（`%LOCALAPPDATA%\AIWorkspace\logs\app.log`）

### 4.6 会话标题自动生成

**当前**：首次 AI 回复后取前 40 字符

**优化**：
- 调用轻量模型（如 GPT-4o-mini）生成 3-5 字摘要
- 或基于 PromptEngine 识别类型自动生成（如 "Java 异常分析"、"SQL 优化"）

---

## V1.1 增强

**目标**：核心体验打磨 + 小功能补充

**预估工期**：2-3 周

### 1. OCR 降级方案（可选）

**场景**：用户使用的是纯文本模型（如本地 Ollama 小模型），不支持 Vision

**实现**：
- 截图时同时保存原图和 OCR 文本
- 检测到当前模型不支持 Vision 时，提示：
  > "当前模型不支持图片分析。切换到 Claude/GPT-4o 直接识图，或使用 OCR 文本模式（准确率较低）。"
- OCR 引擎：Windows OCR（已内置于系统）

**新增文件**：
- `AIWorkspace.Core/Utils/OcrService.cs`（封装 Windows.Media.Ocr）
- React 端增加"OCR 模式"切换选项

### 2. Prompt Library（手动模板）

**需求**：用户可保存常用的 Prompt 模板，快速插入

**实现**：
- 新增 `prompt_templates` 表
- UI：输入框上方增加 `/` 快捷命令或模板选择下拉

### 3. 消息编辑与重新生成

- 编辑已发送的消息 → 重新发送
- 重新生成某条 AI 回复
- 分支对话（类似 ChatGPT 的编辑后分支）

### 4. 导出功能

| 导出格式 | 内容 | 用途 |
|----------|------|------|
| Markdown | 完整对话 | 存档、分享 |
| PDF | 格式化对话 | 报告 |
| 代码文件 | 提取对话中的所有代码块 | 批量保存 |

### 5. 代码块操作增强

- "插入到剪贴板" → 已存在，优化为直接复制
- "在 VS Code 中打开"（通过 `vscode://` 协议）
- "运行代码"（对 Python/JS 等，通过本地终端）

### 6. 模型对比模式

**场景**：同一问题发送给两个模型，并排对比回答

**UI**：分栏布局，左右各显示一个模型的回复

---

## V1.2 增强

**目标**：文件处理增强 + 性能优化

**预估工期**：2-3 周

### 1. PDF / DOCX / XLSX 解析

| 格式 | 库/方案 | 说明 |
|------|---------|------|
| PDF | `PdfPig` 或 `iTextSharp` | 提取文本 + 表格 |
| DOCX | `DocumentFormat.OpenXml` | Word 文档 |
| XLSX | `EPPlus` 或 `ClosedXML` | Excel 表格 |

**注意**：这些库会增加安装包体积，建议作为可选插件或延迟加载。

### 2. Context 压缩器

**问题**：长对话上下文超过模型窗口限制

**方案**：
- 自动摘要：将早期消息压缩为摘要，保留最近 N 轮完整对话
- Token 预算管理：实时估算，超限时提示用户

```csharp
public class ContextCompressor
{
    public List<ChatMessage> Compress(List<ChatMessage> messages, int maxTokens)
    {
        // 1. 保留最近 6 轮完整对话
        // 2. 更早的对话调用轻量模型生成摘要
        // 3. 替换为 system 消息中的摘要
    }
}
```

### 3. 本地模型深度集成

**当前**：Ollama 通过 OpenAI Compatible API 接入

**增强**：
- 自动检测本地 Ollama 实例
- 显示模型加载状态 / GPU 使用率
- 一键下载推荐模型

### 4. 性能优化

| 优化项 | 方案 | 效果 |
|--------|------|------|
| 虚拟滚动 | React `react-window` | 长会话消息多时不卡顿 |
| 消息懒加载 | 分页加载历史消息 | 启动更快 |
| 图片懒加载 | 滚动到可视区域再渲染 | 内存优化 |
| SQLite WAL 模式 | `PRAGMA journal_mode=WAL` | 并发性能 |

### 5. 多语言支持

- UI 文本提取到 JSON 资源文件
- 支持中文 / English
- Prompt 策略也支持多语言输出

---

## V2.0 大版本

**目标**：从工具进化为平台

### 1. MCP（Model Context Protocol）支持

**意义**：让 AI 直接操作本地工具（文件系统、数据库、命令行）

**场景**：
- "分析这个目录下的所有日志文件" → AI 通过 MCP 读取文件
- "帮我重启这个服务" → AI 通过 MCP 执行 PowerShell

**实现**：
- 集成 `mcp-dotnet-sdk` 或自研 MCP Client
- 内置 MCP Server：文件读取、命令执行、数据库查询

### 2. 插件系统

**架构**：

```
Plugins/
├── MyPlugin/
│   ├── manifest.json          # 插件元数据
│   ├── MyPlugin.dll           # .NET 插件（实现 IPlugin 接口）
│   └── ui/                    # 可选前端组件
│       └── MyComponent.tsx
```

**接口设计**：

```csharp
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
    string Version { get; } 
    void Initialize(IPluginHost host);
    void RegisterCommands(ICommandRegistry registry);
}
```

### 3. RAG / 本地知识库

**场景**：
- 上传技术文档 → 向量化存储 → 问答时检索相关知识
- 团队共享知识库

**技术栈**：
- 向量数据库：SQLite + `sqlite-vec` 扩展，或本地文件
- Embedding：通过 Ollama 本地模型生成向量
- 检索：余弦相似度 Top-K

### 4. Agent 工作流

**场景**：
- "分析这个错误并给出修复方案" → Agent 自动：读取日志 → 分析 → 生成代码 → 写入文件
- "监控这个服务的日志，出现异常时通知我" → 定时任务 + 条件触发

**实现**：
- 状态机驱动的 Agent Loop
- 工具调用（Function Calling）支持
- 人工确认节点（关键操作需用户确认）

### 5. 多模型并行对比

**UI**：

```
┌──────────────────────────────┬──────────────────────────────┐
│  GPT-4o                      │  Claude 3.5 Sonnet           │
│  ─────────                   │  ─────────────────           │
│  回答内容...                  │  回答内容...                  │
├──────────────────────────────┼──────────────────────────────┤
│  DeepSeek V3                 │  Gemini 1.5 Pro              │
│  ───────────                 │  ───────────────             │
│  回答内容...                  │  回答内容...                  │
└──────────────────────────────┴──────────────────────────────┘
```

### 6. 跨平台移植

**方案 A：Avalonia + WebView**
- Avalonia 替代 WPF（支持 Windows/macOS/Linux）
- 继续使用 WebView 加载 React

**方案 B：Tauri**
- Rust 后端 + React 前端
- 天然跨平台，体积更小

**方案 C：MAUI**
- 微软官方跨平台方案
- Blazor Hybrid 替代 React（需重写前端）

---

## 技术债务

### 当前已知债务

| 债务 | 影响 | 解决方案 | 计划版本 |
|------|------|----------|----------|
| `useChatStore` 循环引用 | 编译警告 | 将 `useProviderStore` 提取为参数传入 | V1.0 |
| 前端文件上传未实现真正对话框 | 仅模拟 | WPF 端实现 `file:read` 的真正文件选择 | V1.0 |
| 无单元测试 | 回归风险 | 补充 xUnit + Vitest | V1.1 |
| 硬编码模型列表 | 维护困难 | 从配置文件/远程加载 | V1.1 |
| 未处理 WebView2 未安装场景 | 首次启动崩溃 | 启动前检测并引导安装 | V1.0 |
| 消息无虚拟滚动 | 长会话卡顿 | 集成 `react-window` | V1.2 |
| 无自动更新机制 | 分发困难 | 集成 Squirrel 或自定义更新 | V1.2 |
| 新消息无法置顶滚动 | 用户体验：多轮对话后新消息停留在底部，需手动滚动查看 | 调研 WebView2 与 React scroll 冲突根因；尝试 WPF 端直接控制滚动或改用 CSS scroll-snap | V1.0 |

---

## 优先级矩阵

### 紧急重要（立即做）

- [ ] 全局快捷键（截图 + 唤起）
- [ ] 系统托盘最小化
- [ ] 安装包打包（InnoSetup）
- [ ] 错误边界 + 友好错误提示
- [ ] 修复 `useChatStore` 循环引用

### 重要不紧急（V1.1 做）

- [ ] OCR 降级方案
- [ ] Prompt Library
- [ ] 消息编辑/重新生成
- [ ] 导出 Markdown/PDF
- [ ] 单元测试覆盖
- [ ] 虚拟滚动

### 紧急不重要（视情况）

- [ ] UI 动画优化
- [ ] 暗色/亮色主题切换完善
- [ ] 字体大小调整

### 不紧急不重要（长期）

- [ ] 自定义 CSS 主题
- [ ] 语音输入
- [ ] 云端同步

---

## 附录：开发资源

### 推荐开发工具

| 工具 | 用途 |
|------|------|
| Visual Studio 2022 | WPF + .NET 开发 |
| VS Code | React + TypeScript 开发 |
| DB Browser for SQLite | 数据库调试 |
| Postman / curl | API 测试 |
| DebugView | Win32 Debug 输出查看 |
| Inno Setup Compiler | 安装包制作 |

### 参考文档

- [WebView2 文档](https://docs.microsoft.com/en-us/microsoft-edge/webview2/)
- [OpenAI API 文档](https://platform.openai.com/docs/api-reference)
- [React 文档](https://react.dev/)
- [TailwindCSS 文档](https://tailwindcss.com/)
- [Zustand 文档](https://docs.pmnd.rs/zustand)
