# AIWorkspace.Web

> React 18 + TypeScript + Vite 前端项目，构建为静态资源后嵌入 WPF 的 WebView2 中运行。

---

## 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| React | 18.2 | UI 框架 |
| TypeScript | 5.3 | 类型安全 |
| Vite | 5.1 | 构建工具（开发服务器 + 生产打包） |
| TailwindCSS | 3.4 | 原子化 CSS |
| @tailwindcss/typography | ^0.5 | Markdown prose 样式 |
| Zustand | 4.5 | 全局状态管理 |
| react-markdown | 9.0 | Markdown 渲染 |
| remark-gfm | 4.0 | GitHub Flavored Markdown 支持 |
| react-syntax-highlighter | 15.5 | 代码块语法高亮 |
| lucide-react | 0.323 | 图标库 |

---

## 项目配置

### 构建输出

`vite.config.ts` 中配置：

```ts
build: {
  outDir: '../AIWorkspace.WPF/Assets',  // 构建产物直接输出到 WPF 项目
  emptyOutDir: true,
  assetsDir: '.',
}
```

执行 `npm run build` 后，WPF 的 `Assets/` 目录将包含 `index.html`、哈希化的 JS/CSS 文件。

### 虚拟主机映射

WPF 通过 WebView2 的 `SetVirtualHostNameToFolderMapping` 将 `http://aiworkspace.local` 映射到 `Assets/` 目录。前端代码中：

- **不能使用** `crypto.randomUUID()`（非安全上下文）
- **已替换为** `Math.random()` 实现的 `generateUUID()`（`bridge.ts`）
- `navigator.clipboard.writeText` 可用（需用户点击触发）

### 开发模式

```bash
cd src/AIWorkspace.Web
npm install
npm run dev        # 启动 Vite 开发服务器（独立浏览器调试）
```

开发环境下 `window.chrome.webview` 不存在，`bridge.ts` 会 fallback 到 `console.log` + 模拟响应。

---

## npm 脚本

| 脚本 | 说明 |
|------|------|
| `npm run dev` | 启动 Vite 开发服务器 |
| `npm run build` | 编译 TypeScript + Vite 生产打包 |
| `npm run preview` | 预览生产构建 |

---

## Tailwind 主题色

定义于 `tailwind.config.js`：

| Token | 色值 | 用途 |
|-------|------|------|
| `background` | `#1e1e1e` | 页面主背景 |
| `surface` | `#252526` | 卡片/面板背景 |
| `surfaceHover` | `#2a2d2e` | 悬停状态 |
| `border` | `#3e3e42` | 边框/分割线 |
| `text` | `#cccccc` | 主文字 |
| `textSecondary` | `#858585` | 次要文字 |
| `accent` | `#007acc` | 主题色/按钮 |
| `accentHover` | `#0098ff` | 主题色悬停 |
| `codeBg` | `#2a2a2a` | 代码块背景 |
