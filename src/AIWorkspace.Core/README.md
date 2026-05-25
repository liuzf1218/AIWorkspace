# AIWorkspace.Core

> .NET 8 类库，承载平台无关的业务逻辑、数据持久化和 AI 接入能力。

---

## 文件夹结构

```
AIWorkspace.Core/
├── AIWorkspace.Core.csproj       # 项目文件（引用 Sqlite/DPAPI/SSE）
├── Models/                        # 数据实体与 DTO
│   ├── ChatModels.cs             # 聊天请求/消息/流式块 DTO
│   ├── Conversation.cs           # 会话与消息领域实体
│   └── Provider.cs               # Provider 配置与模型信息
├── Providers/                     # AI Provider 抽象与实现
│   ├── IProvider.cs              # Provider 接口定义
│   └── OpenAICompatibleProvider.cs # OpenAI 兼容协议实现（SSE 流式）
├── Services/                      # 业务服务层
│   ├── ChatService.cs            # 会话/消息的 CRUD（直接操作 SQLite）
│   ├── DatabaseService.cs        # SQLite 连接管理与建表初始化
│   ├── EncryptionService.cs      # DPAPI 加密（API Key 安全存储）
│   ├── PromptEngine.cs           # 智能 Prompt 增强引擎（技术日志识别）
│   ├── ProviderService.cs        # Provider 配置的增删改查（含加解密桥接）
│   └── SettingsService.cs        # 键值对设置持久化（SQLite settings 表）
└── Utils/                         # 工具类
    ├── FileProcessor.cs          # 文件读取与格式化（JSON/XML 美化）
    └── LogParser.cs              # 日志分析与 Token 估算
```

---

## 各文件功能详解

### Models/

| 文件 | 说明 |
|------|------|
| `ChatModels.cs` | 定义 `ChatRequest`、`ChatMessage`、`ChatChunk`、`ChatImageContent`、`AppSettings` 等 DTO，用于前后端传输和 SSE 流解析。 |
| `Conversation.cs` | 定义数据库实体 `Conversation`（会话元数据）、`Message`（单条消息，含可选 base64 图片）、`Attachment`（文件附件）。 |
| `Provider.cs` | 定义 `Provider`（运行时内存对象，明文 API Key）、`ModelInfo`（模型元数据，含 vision 标记）、`ProviderConfiguration`（数据库持久化对象，加密 API Key）。 |

### Providers/

| 文件 | 说明 |
|------|------|
| `IProvider.cs` | 接口：流式对话 `StreamChatAsync`、获取模型列表 `GetModelsAsync`、验证连通性 `ValidateAsync`。 |
| `OpenAICompatibleProvider.cs` | **核心 AI 接入实现**。支持任意 OpenAI 兼容 API（Claude/Gemini/DeepSeek/Ollama 等）。功能：SSE 流解析、多模态图片消息、模型列表获取、`/v1` URL 去重、内置默认模型列表 fallback。 |

### Services/

| 文件 | 说明 |
|------|------|
| `ChatService.cs` | 会话与消息的数据库操作：创建/删除会话、添加/查询消息、自动更新 `updated_at`。保留最近 20 条作为上下文。 |
| `DatabaseService.cs` | SQLite 数据库引导：创建 `%LocalAppData%\AIWorkspace\aiworkspace.db`，初始化 schema（providers/conversations/messages/attachments/settings）。 |
| `EncryptionService.cs` | 静态工具类，使用 `System.Security.Cryptography.ProtectedData`（Windows DPAPI）对用户级 API Key 进行加密/解密。 |
| `PromptEngine.cs` | **智能 Prompt 增强**。内置 7 种 `IPromptStrategy` 实现，通过正则识别输入类型（Java 异常/Python 追溯/Linux 日志/SQL 错误/Nginx 错误/K8s 错误），自动包装结构化中文 Prompt。当前 `WebBridge` 中临时 bypass（`enhancedContent = userContent`）。 |
| `ProviderService.cs` | Provider CRUD + 加密桥接：加载时自动解密 API Key，保存时自动加密。 |
| `SettingsService.cs` | 基于 SQLite `settings` 表的键值对存储，支持 upsert。 |

### Utils/

| 文件 | 说明 |
|------|------|
| `FileLogger.cs` | 单例文件日志。写入 `%LOCALAPPDATA%\AIWorkspace\logs\app-yyyyMMdd.log`，支持 INFO/WARN/ERROR 级别，线程安全。 |
| `FileProcessor.cs` | 静态文件读取器，支持 `.txt/.log/.json/.xml/.yaml/.sql/.csv/.md` 等。对 JSON/XML 自动格式化输出。 |
| `LogParser.cs` | 日志分析工具：提取关键词高亮片段、重复行去重、中英文 Token 粗略估算。 |

---

## 扩展点

| 扩展需求 | 操作位置 |
|----------|----------|
| 接入非 OpenAI 协议 Provider | 实现 `IProvider` → 在 `WebBridge` 中路由 |
| 新增 Prompt 识别策略 | `PromptEngine.cs` 新增 `IPromptStrategy` → 注册到 `_strategies` |
| 新增支持的文件类型 | `FileProcessor.SupportedExtensions` + `ReadContent` |
| 数据库 schema 变更 | `DatabaseService.InitializeDatabase` + `settings.db_version` 迁移 |
