# AIWorkspace.Web /src

> React 前端源码目录，包含组件、状态管理、类型定义和通信封装。

---

## 文件夹结构

```
src/
├── main.tsx                       # React 应用入口（ReactDOM.createRoot）
├── App.tsx                        # 根组件（ErrorBoundary + Layout）
├── index.css                      # 全局 Tailwind 样式 + 基础元素样式
├── types/
│   └── index.ts                   # 核心 TypeScript 接口（Provider/Message/Conversation/IpcMessage）
├── services/
│   └── bridge.ts                  # IPC 桥接封装（请求/响应/订阅）
├── stores/                        # Zustand 全局状态
│   ├── useChatStore.ts            # 聊天状态（消息/会话/流式/附件）
│   ├── useProviderStore.ts        # Provider 状态（列表/选中/模型）
│   └── useSettingsStore.ts        # 设置状态（主题/快捷键/文件限制）
└── components/                    # React 组件
    ├── Layout/
    │   ├── Layout.tsx             # 整体布局框架（初始化 + 事件监听 + 弹窗控制）
    │   └── Sidebar.tsx            # 左侧边栏（New Chat + 对话列表 + 折叠按钮）
    ├── Chat/
    │   ├── ChatArea.tsx           # 聊天主区域（顶部菜单栏 + 消息列表 + 输入框）
    │   ├── ChatInput.tsx          # 输入框（文本域/截图/文件/发送按钮）
    │   └── MessageList.tsx        # 消息渲染（用户气泡 + AI Markdown + 滚动管理）
    ├── Markdown/
    │   └── MarkdownRenderer.tsx   # Markdown 渲染器（react-markdown + 代码复制）
    ├── Settings/
    │   ├── ProviderModal.tsx      # Provider 管理弹窗（增删改查/验证）
    │   └── SettingsModal.tsx      # 通用设置弹窗（主题/快捷键/文件限制）
    └── ErrorBoundary/
        └── ErrorBoundary.tsx      # React 错误边界（全局异常捕获）
```

---

## 各文件功能详解

### 入口与样式

| 文件 | 说明 |
|------|------|
| `main.tsx` | 应用启动点。创建 React root，渲染 `<App />`，导入全局 CSS。 |
| `App.tsx` | 根组件。包裹 `<ErrorBoundary>`，渲染 `<Layout />`。 |
| `index.css` | 全局样式。包含 `@tailwind` 指令、滚动条样式、`pre/code/p/ul/ol/a/blockquote` 基础重置、`.btn-primary/.input-field/.card` 组件类。 |

### types/

| 文件 | 说明 |
|------|------|
| `index.ts` | 集中定义领域类型：`Provider`、`ModelInfo`、`Conversation`、`Message`、`Attachment`、`IpcMessage`、`AppSettings`。所有组件和 store 共享这些类型。 |

### services/

| 文件 | 说明 |
|------|------|
| `bridge.ts` | **IPC 通信核心**。`Bridge` 类封装了 WebView2 的 `chrome.webview.postMessage`：支持 Promise 风格的 `send(channel, payload)`（60 秒超时）、pub/sub 风格的 `on(channel, handler)`、`off` 取消订阅。使用自定义 `generateUUID()` 代替 `crypto.randomUUID()`（非安全上下文不可用）。 |

### stores/

| 文件 | 说明 |
|------|------|
| `useChatStore.ts` | **聊天状态中心**。管理 `conversations`、`messages`、`isStreaming`、`streamContent`、`error`、`screenshotData`、`fileAttachment`。关键方法：`sendMessage`（乐观添加用户消息 + 触发 IPC）、`appendStream`（追加流式内容）、`finishStream`（保存 assistant 消息到列表）。 |
| `useProviderStore.ts` | **Provider 状态中心**。管理 `providers`、`models`、`selectedProviderId`、`selectedModelId`。`selectProvider` 自动触发 `loadModels` 拉取模型列表。 |
| `useSettingsStore.ts` | **设置状态**。管理主题、窗口尺寸、快捷键、文件大小限制等。`loadSettings` 从后端读取，`setSetting` 单条持久化。 |

### components/Layout/

| 文件 | 说明 |
|------|------|
| `Layout.tsx` | **布局框架**。`useEffect` 初始化：加载会话/Provider/设置，订阅 `bridge` 事件（`chat:chunk` → `appendStream`、`chat:done` → `finishStream`、`chat:error` → `setError`、`screenshot:ready` → `setScreenshot`）。管理 `ProviderModal` 和 `SettingsModal` 的显示状态。控制 `sidebarCollapsed` 状态。 |
| `Sidebar.tsx` | **左侧边栏**。仅保留 `New Chat` 按钮和 `Recent` 对话列表。每个对话项支持点击切换、悬停删除。右上角有折叠按钮（`PanelLeftClose`）。宽度 `w-56`。 |

### components/Chat/

| 文件 | 说明 |
|------|------|
| `ChatArea.tsx` | **聊天主区域**。顶部 Header 扩展为菜单栏：左侧展开 sidebar 按钮 + 对话标题；中间 Provider 下拉（含 Manage Providers 入口）+ Model 下拉（含搜索过滤）；右侧 Settings 按钮。下拉菜单支持点击外部自动关闭（`mousedown` 全局监听）。下方渲染 `<MessageList />` 和 `<ChatInput />`，以及错误横幅。 |
| `ChatInput.tsx` | **输入框组件**。多行文本域（`Shift+Enter` 换行，`Enter` 发送），截图按钮（触发 `screenshot:capture`），文件按钮（触发 `file:open`），发送/中止按钮。底部显示附件预览（截图缩略图 + 文件卡片）。 |
| `MessageList.tsx` | **消息渲染**。构建消息列表（含流式 assistant 消息的虚拟条目）。用户消息右对齐圆角气泡（`bg-surfaceHover rounded-2xl rounded-tr-sm`），AI 消息左对齐 Markdown 渲染。流式输出时自动滚动到底部（`scrollIntoView({ block: 'nearest' })`）。流式消息末尾有闪烁光标（`animate-pulse`）。 |
| `ChatArea.tsx` | **聊天主区域 + 滚动控制**。顶部 Header 扩展为菜单栏：左侧展开 sidebar 按钮 + 对话标题；中间 Provider 下拉（含 Manage Providers 入口）+ Model 下拉（含搜索过滤）；右侧 Settings 按钮。下拉菜单支持点击外部自动关闭。下方渲染 `<MessageList />` 和 `<ChatInput />`，以及错误横幅。**滚动控制**：新用户消息发送后，通过 `querySelectorAll('[data-role="user"]')` 找到最后一条用户消息并执行 `scrollIntoView({ block: 'start' })` 置顶。

### components/Markdown/

| 文件 | 说明 |
|------|------|
| `MarkdownRenderer.tsx` | **富文本渲染**。基于 `react-markdown` + `remark-gfm`。自定义 `code` 组件：识别 `language-xxx` 类名，非行内代码渲染为带头部（语言标签 + Copy 按钮）的语法高亮块（`react-syntax-highlighter` + vscDarkPlus 主题）。Copy 按钮兼容 HTTPS 和 fallback（`document.execCommand('copy')`）。自定义 `table/th/td/pre` 适配暗色主题。 |

### components/Settings/

| 文件 | 说明 |
|------|------|
| `ProviderModal.tsx` | **Provider 管理弹窗**。列表展示所有 Provider（验证/编辑/删除），内联表单添加/编辑 Provider（名称/API Base URL/API Key/代理/Vision 支持）。通过 `bridge` 调用后端验证和保存。 |
| `SettingsModal.tsx` | **通用设置弹窗**。编辑主题、最大文件大小、截图长边限制、快捷键（截图/唤起）。通过 `useSettingsStore` 读写。 |

### components/ErrorBoundary/

| 文件 | 说明 |
|------|------|
| `ErrorBoundary.tsx` | 类组件实现的 React Error Boundary。捕获渲染期错误，显示全屏 fallback UI（错误信息 + Reload 按钮）。 |

---

## 状态流转图

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  useChatStore   │◄───►│  useProviderStore│     │ useSettingsStore│
│  (消息/会话)     │     │  (Provider/模型)  │     │  (应用设置)      │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                           bridge.ts                              │
│                    (send / on / off)                             │
└─────────────────────────────┬───────────────────────────────────┘
                              │ chrome.webview.postMessage
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                         WebBridge.cs (WPF)                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 修改速查

| 想改什么 | 文件 |
|----------|------|
| 消息气泡样式 | `components/Chat/MessageList.tsx` |
| 输入框功能/按钮 | `components/Chat/ChatInput.tsx` |
| 顶部 Provider/模型菜单 | `components/Chat/ChatArea.tsx` |
| 侧边栏宽度/内容 | `components/Layout/Sidebar.tsx` |
| 代码块颜色/复制按钮 | `components/Markdown/MarkdownRenderer.tsx` |
| 全局主题色 | `../../tailwind.config.js` + `index.css` |
| 流式输出行为 | `stores/useChatStore.ts` + `components/Chat/MessageList.tsx` |
| IPC 通信协议 | `services/bridge.ts` + `../../../AIWorkspace.WPF/Bridge/WebBridge.cs` |
