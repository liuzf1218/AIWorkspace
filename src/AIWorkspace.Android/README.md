# AI Workspace Android

AI Workspace 的 Android 客户端，使用 Kotlin + Jetpack Compose 构建。

## 技术栈

- **语言**: Kotlin 1.9.20
- **UI**: Jetpack Compose (BOM 2024.02.00) + Material3
- **架构**: MVVM + Repository Pattern
- **本地数据库**: Room 2.6.1
- **网络**: OkHttp 4.12.0 + SSE 流式
- **图片加载**: Coil 2.5.0
- **Markdown**: compose-richtext 0.20.0
- **序列化**: kotlinx.serialization 1.6.2
- **加密**: Android Keystore (AES-GCM)

## 项目结构

```
app/src/main/java/com/aiworkspace/
├── data/
│   ├── db/              # Room Database, DAO, Converters
│   ├── entity/          # Room Entity (6 tables)
│   ├── repository/      # Repository layer
│   └── security/        # KeystoreEncryption
├── network/
│   ├── model/           # DTOs (ChatRequest, ChatChunk, etc.)
│   └── SseEventSource.kt # SSE streaming client
├── ui/
│   ├── components/      # Reusable Compose components
│   │   └── markdown/    # MarkdownRenderer
│   ├── navigation/      # AppScreen enum
│   ├── screens/         # MainScreen, ChatScreen, etc.
│   └── theme/           # Color, Theme, Typography
├── utils/               # ImageUtils
├── viewmodel/           # ChatViewModel, ProviderViewModel, SettingsViewModel
├── MainActivity.kt
└── AIWorkspaceApplication.kt
```

## 功能特性

### Phase 1 - 基础骨架
- Room 数据库（conversations, messages, providers, models, attachments, settings）
- Android Keystore API Key 加密
- OkHttp SSE 流式聊天
- 暗色/亮色主题

### Phase 2 - 核心聊天
- 相机拍照 + 相册选择
- 图片压缩（最大 1536px 长边，JPEG 85%）
- 多模态消息（图片 + 文本）
- Markdown 渲染
- 消息列表自动滚动

### Phase 3 - Provider 管理
- Provider 添加/编辑/删除
- API Key 安全存储
- 模型列表获取
- 设置屏幕（主题切换）

### Phase 4 - 完善
- 主题持久化联动
- 对话标题自动生成
- 聊天记录搜索
- 错误重试机制
- 停止生成

## 构建说明

### 环境要求
- Android Studio Hedgehog (2023.1.1) 或更新版本
- JDK 17
- Android SDK 34
- Gradle 8.5

### 构建命令
```bash
./gradlew assembleDebug
```

### 安装到设备
```bash
./gradlew installDebug
```

## 数据模型

Android Room 直接复用桌面端 SQLite schema：

| 表名 | 说明 |
|------|------|
| `conversations` | 对话信息 |
| `messages` | 聊天消息（含图片 base64） |
| `providers` | LLM Provider 配置（加密 API Key） |
| `models` | 每个 Provider 的模型列表 |
| `attachments` | 文件附件 |
| `settings` | 应用设置 |

## 与桌面端的差异

| 功能 | 桌面端 (WPF) | Android |
|------|-------------|---------|
| 截图 | 区域截图 (Win32 API) | 相机拍照 + 相册选择 |
| 文件上传 | 文件对话框 | Storage Access Framework |
| API Key 加密 | Windows DPAPI | Android Keystore AES-GCM |
| 代码高亮 | PrismJS | compose-richtext |
| 导航 | WPF Navigation | ModalNavigationDrawer |

## 待办事项

- [ ] Markdown 代码块复制按钮
- [ ] 通用文件上传（PDF/TXT 等）
- [ ] 消息长按操作（复制、删除）
- [ ] 对话导出/分享
- [ ] 推送通知
- [ ] 生物识别锁
