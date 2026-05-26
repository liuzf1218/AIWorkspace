# 家里电脑开发操作指南

> 适用于 Win11 笔记本，从公司/工作室同步后继续开发 AI Workspace。

---

## 一、环境准备（只需一次）

### 1.1 安装 Git

访问 https://git-scm.com/download/win 下载安装，全部默认选项即可。

验证：
```bash
git --version
```

### 1.2 安装 Node.js 18+

访问 https://nodejs.org/ 下载 LTS 版本安装。

验证：
```bash
node --version   # 应显示 v18.x 或更高
npm --version
```

### 1.3 安装 .NET 8 SDK

访问 https://dotnet.microsoft.com/download/dotnet/8.0 下载安装。

验证：
```bash
dotnet --version   # 应显示 8.0.x
```

### 1.4 配置 Git 用户信息

```bash
git config --global user.name "你的名字"
git config --global user.email "你的邮箱"
```

---

## 二、克隆项目到家里电脑

### 2.1 创建项目目录

```bash
mkdir -p ~/Projects
cd ~/Projects
```

### 2.2 克隆仓库

```bash
git clone YOUR_SSH_USER@YOUR_SERVER_IP:git-repos/AIWorkspace.git
```

**输入密码**：`YOUR_PASSWORD`

### 2.3 进入项目

```bash
cd AIWorkspace
```

### 2.4 验证

```bash
git log --oneline -5
```

---

## 三、安装依赖（只需一次）

### 3.1 前端依赖

```bash
cd src/AIWorkspace.Web
npm install
```

### 3.2 后端依赖

```bash
cd ../../
dotnet restore
```

---

## 四、开发流程

### 4.1 启动前端开发服务器（独立调试）

```bash
cd src/AIWorkspace.Web
npm run dev
```

浏览器访问 `http://localhost:5173`

> 注：独立开发时 `window.chrome.webview` 不存在，截图/文件等功能不可用，仅用于 UI 调试。

### 4.2 完整运行（需要 WPF）

```bash
# 先构建前端
cd src/AIWorkspace.Web
npm run build

# 再启动 WPF
cd ../../
dotnet run --project src/AIWorkspace.WPF/AIWorkspace.WPF.csproj
```

### 4.3 修改代码 → 提交 → 推送

```bash
# 1. 随时查看改了什么
git status

# 2. 添加所有改动
git add .

# 3. 提交（写清楚改了什么）
git commit -m "fix: 修复消息置顶滚动问题"

# 4. 推送到服务器（会提示输入密码 YOUR_PASSWORD）
git push

# 输入密码: YOUR_PASSWORD
```

### 4.4 从服务器拉取最新代码

如果公司/工作室的电脑改了代码，家里电脑先拉取：

```bash
git pull

# 输入密码: YOUR_PASSWORD
```

---

## 五、构建发布（生成可执行文件）

```bash
# 一键构建脚本
./scripts/build.ps1

# 或手动分步：
cd src/AIWorkspace.Web
npm run build

cd ../AIWorkspace.WPF
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../../publish
```

输出在 `G:/kimi/AIWorkspace/publish/AIWorkspace.WPF.exe`

---

## 六、减少密码输入（推荐配置 SSH 免密）

如果嫌每次 `git push` / `git pull` 都要输密码，配置一次免密：

### 6.1 生成密钥

```bash
ssh-keygen -t ed25519
# 一路回车，不设密码
```

### 6.2 复制公钥到服务器

```bash
cat ~/.ssh/id_ed25519.pub
# 复制输出的内容（以 ssh-ed25519 开头）
```

SSH 登录服务器，粘贴公钥：

```bash
ssh YOUR_SSH_USER@YOUR_SERVER_IP
# 输入密码: YOUR_PASSWORD

echo '粘贴你复制的公钥内容' >> ~/.ssh/authorized_keys
exit
```

### 6.3 验证免密

```bash
ssh YOUR_SSH_USER@YOUR_SERVER_IP
# 不输密码直接登录 = 成功
```

之后 `git push` / `git pull` 不再需要密码。

---

## 七、常见问题

### Q1: push 时提示冲突

说明两边都改了同一文件：

```bash
git pull                    # 先拉取合并
git add .
git commit -m "合并冲突"
git push
```

### Q2: 提示 "Permission denied"

检查：
1. 服务器 IP 是否正确（`YOUR_SERVER_IP`）
2. 密码是否输入正确（`YOUR_PASSWORD`）
3. 如果配置了免密，检查 `~/.ssh/id_ed25519` 是否存在

### Q3: 前端 `npm run dev` 正常，WPF 里白屏

检查 WebView2 Runtime 是否安装：
https://developer.microsoft.com/en-us/microsoft-edge/webview2/

### Q4: 构建时提示 "exe 被占用"

关闭正在运行的 `AIWorkspace.WPF.exe` 后再 publish。

---

## 八、核心命令速查

| 操作 | 命令 | 说明 |
|------|------|------|
| 克隆 | `git clone YOUR_SSH_USER@YOUR_SERVER_IP:git-repos/AIWorkspace.git` | 首次 |
| 查看改动 | `git status` | 随时 |
| 提交 | `git add . && git commit -m "说明"` | 本地存档 |
| 推送 | `git push` | 上传到服务器 |
| 拉取 | `git pull` | 从服务器下载 |
| 前端开发 | `cd src/AIWorkspace.Web && npm run dev` | 独立调试 |
| 前端构建 | `cd src/AIWorkspace.Web && npm run build` | 输出到 WPF/Assets |
| WPF 运行 | `dotnet run --project src/AIWorkspace.WPF` | 完整运行 |
| 发布 | `./scripts/build.ps1` | 生成单文件 exe |
