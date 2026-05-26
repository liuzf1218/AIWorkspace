# 项目上传 GitHub 完全操作手册

> 本文档记录将已有 Git 项目上传至 GitHub 公开仓库的完整步骤。  
> 以 **AI Workspace** 项目为实例，适用于 Windows + Git Bash 环境。

---

## 目录

1. [前置条件](#一前置条件)
2. [总体流程概览](#二总体流程概览)
3. [第一步：项目自查与准备](#三第一步项目自查与准备)
4. [第二步：敏感信息脱敏](#四第二步敏感信息脱敏)
5. [第三步：撰写 GitHub 标准文档](#五第三步撰写-github-标准文档)
6. [第四步：本地提交](#六第四步本地提交)
7. [第五步：创建 GitHub 仓库](#七第五步创建-github-仓库)
8. [第六步：添加远程并推送](#八第六步添加远程并推送)
9. [第七步：验证与后续更新](#九第七步验证与后续更新)
10. [常见问题](#十常见问题)
11. [命令速查表](#十一命令速查表)

---

## 一、前置条件

### 1.1 必备工具

| 工具 | 验证命令 | 说明 |
|------|----------|------|
| Git | `git --version` | 版本建议 2.30+ |
| GitHub 账号 | — | 已注册，已知用户名（如 `liuzf1218`） |
| 浏览器 | — | 用于在 GitHub 网站创建仓库 |

### 1.2 确认 Git 用户信息（只需配置一次）

```bash
git config --global user.name "你的GitHub用户名"
git config --global user.email "你的GitHub注册邮箱"
```

> 本项目实例：用户名 `liuzf1218`，邮箱 `lzf5027@gmail.com`

---

## 二、总体流程概览

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  项目自查    │ → │  敏感信息脱敏 │ → │  撰写标准文档 │ → │  本地Git提交 │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
                                                              ↓
┌─────────────┐    ┌─────────────┐    ┌─────────────────────────────────┐
│  验证完成    │ ← │  推送代码    │ ← │  创建GitHub仓库 + 添加远程地址   │
└─────────────┘    └─────────────┘    └─────────────────────────────────┘
```

---

## 三、第一步：项目自查与准备

### 3.1 进入项目目录

```bash
cd /g/kimi/AIWorkspace
```

> 建议：后续所有命令均在此目录下执行。

### 3.2 检查当前 Git 状态

```bash
# 查看当前远程仓库地址
git remote -v

# 查看当前分支和未提交变更
git status

# 查看提交历史
git log --oneline -10
```

**预期输出示例：**

```
origin  root@172.19.12.4:git-repos/AIWorkspace.git (fetch)
origin  root@172.19.12.4:git-repos/AIWorkspace.git (push)
```

> **关键认知**：此时只有 `origin` 指向你的私有服务器。GitHub 是新的远程，需要额外添加。

### 3.3 检查现有文档是否齐全

```bash
ls -la *.md 2>/dev/null          # 查看根目录是否有 README.md
ls -la LICENSE 2>/dev/null       # 查看是否有 LICENSE
cat .gitignore                   # 查看忽略规则
```

| 检查项 | 若缺失 | 处理方式 |
|--------|--------|----------|
| README.md | ❌ | 必须新建，见 [第五步](#五第三步撰写-github-标准文档) |
| LICENSE | ❌ | 建议新建，开源项目必备 |
| .gitignore | ❌ | 必须新建，防止提交无关文件 |

---

## 四、第二步：敏感信息脱敏

> **为什么必须做**：公开仓库意味着任何人都能看到代码和历史记录。密码、内网 IP、API Key 等一旦泄露，可能造成严重安全风险。

### 4.1 扫描敏感信息

```bash
# 全局搜索可能的敏感关键词
grep -rni "password\|secret\|apikey\|api_key\|172\.19\.\|192\.168\.\|10\.0\." \
  --include="*.md" --include="*.cs" --include="*.kt" --include="*.ts" \
  --include="*.json" --include="*.sql" .
```

**本项目发现的问题：**

| 文件 | 敏感内容 | 替换为 |
|------|----------|--------|
| `docs/HOME_DEV_GUIDE.md` | `172.19.12.4` | `YOUR_SERVER_IP` |
| `docs/HOME_DEV_GUIDE.md` | `act4` | `YOUR_PASSWORD` |
| `docs/HOME_DEV_GUIDE.md` | `root@172.19.12.4` | `YOUR_SSH_USER@YOUR_SERVER_IP` |
| `src/AIWorkspace.Android/RELEASE_GUIDE.md` | `AIWorkspace2024` | `YOUR_KEYSTORE_PASSWORD` |

### 4.2 执行替换（以 HOME_DEV_GUIDE.md 为例）

**手动方式**（推荐，可控）：

用 VS Code / Notepad++ 打开文件，使用查找替换功能：
- 查找：`172.19.12.4` → 替换为：`YOUR_SERVER_IP`
- 查找：`act4` → 替换为：`YOUR_PASSWORD`
- 查找：`root@` → 替换为：`YOUR_SSH_USER@`

**命令行方式**（适合批量）：

```bash
# 注意：此方法直接修改文件，请确保已备份或已提交当前状态
sed -i 's/172\.19\.12\.4/YOUR_SERVER_IP/g' docs/HOME_DEV_GUIDE.md
sed -i 's/act4/YOUR_PASSWORD/g' docs/HOME_DEV_GUIDE.md
sed -i 's/root@/YOUR_SSH_USER@/g' docs/HOME_DEV_GUIDE.md
```

### 4.3 确认替换结果

```bash
grep -n "YOUR_" docs/HOME_DEV_GUIDE.md
```

若输出包含所有替换后的占位符，且无原始敏感信息残留，则脱敏完成。

---

## 五、第三步：撰写 GitHub 标准文档

### 5.1 撰写 README.md

**作用**：GitHub 仓库的门面，是访客了解项目的第一个入口。

**文件位置**：项目根目录 `/g/kimi/AIWorkspace/README.md`

**必备内容结构**：

```markdown
# 项目名称

[徽章区：License、技术栈版本等]

> 一句话项目简介

## 项目简介
## 功能特性
## 技术架构
## 快速开始
## 项目结构
## 文档索引
## 路线图（可选）
## 贡献指南（可选）
## 许可证
## 致谢（可选）
```

**徽章示例**（直接复制使用）：

```markdown
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
```

**中英双语写法**（参考 AI Workspace 项目）：

在每个章节标题后添加英文：
```markdown
## 📖 项目简介 / Introduction

中文介绍内容...

English introduction content...
```

### 5.2 生成 LICENSE（MIT 协议）

**文件位置**：项目根目录 `/g/kimi/AIWorkspace/LICENSE`

**标准 MIT 协议内容**：

```text
MIT License

Copyright (c) 2026 liuzf1218

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

> 只需修改 `Copyright (c) 2026 liuzf1218` 这一行，署名改为你的 GitHub 用户名。

### 5.3 优化 .gitignore

**作用**：告诉 Git 哪些文件不需要跟踪（如编译产物、临时文件、敏感配置）。

**本项目 .gitignore 最终内容**：

```gitignore
# Dependencies
node_modules/

# Build outputs
publish/
publish-test/
publish_*/

# .NET
bin/
obj/
*.user
*.suo

# Frontend build output (copied to WPF Assets)
src/AIWorkspace.Web/dist/

# Logs
*.log

# IDE
.vs/
.idea/
*.swp

# OS
.DS_Store
Thumbs.db

# Database (local dev)
*.db
*.db-journal

# Temp
*.tmp

# AI workspace internal
.kimi/

# Windows temp files
新建*.txt
```

---

## 六、第四步：本地提交

### 6.1 添加文件到暂存区

```bash
# 添加新增的文件（README.md、LICENSE）
git add README.md LICENSE

# 添加修改的文件（脱敏后的文档）
git add docs/HOME_DEV_GUIDE.md src/AIWorkspace.Android/RELEASE_GUIDE.md

# 或者一次性添加所有变更（慎用，需确认无多余文件）
git add .
```

### 6.2 检查暂存区内容

```bash
git status
```

确认以下文件在 `Changes to be committed` 区域：
- `README.md`（新增）
- `LICENSE`（新增）
- `docs/HOME_DEV_GUIDE.md`（修改）
- `src/AIWorkspace.Android/RELEASE_GUIDE.md`（修改）

### 6.3 提交变更

```bash
git commit -m "docs: 添加 GitHub 标准文档并脱敏内部信息

- 新增 README.md，按 GitHub 规范撰写项目介绍、功能特性、快速开始等
- 新增 LICENSE（MIT），署名 liuzf1218
- 替换 HOME_DEV_GUIDE.md 中的服务器 IP、密码、SSH 用户为占位符
- 替换 RELEASE_GUIDE.md 中的密钥库密码为占位符"
```

> 提交信息规范：
> - 第一行：简短总结（50字以内）
> - 空一行
> - 后续：详细说明，每行一条改动

---

## 七、第五步：创建 GitHub 仓库

### 7.1 打开 GitHub 新建页面

浏览器访问：https://github.com/new

### 7.2 填写仓库信息

| 字段 | 填写内容 | 说明 |
|------|----------|------|
| **Repository name** | `AIWorkspace` | 与项目名保持一致 |
| **Description** | 面向技术人员的 AI 分析工作台... | 一句话描述项目 |
| **Visibility** | `Public` | 公开仓库 |
| **Add a README** | ❌ 不勾选 | 已在本地写好，避免冲突 |
| **Add .gitignore** | ❌ 不勾选 | 已在本地写好 |
| **Choose a license** | ❌ 不勾选 | 已在本地写好 |

### 7.3 点击 Create repository

创建成功后，页面会显示仓库地址：

```
https://github.com/liuzf1218/AIWorkspace
```

记下这个地址，下一步需要用到。

---

## 八、第六步：添加远程并推送

### 8.1 添加 GitHub 远程地址

```bash
# 添加名为 "github" 的远程仓库
git remote add github https://github.com/liuzf1218/AIWorkspace.git

# 验证是否添加成功
git remote -v
```

**预期输出**：

```
origin   root@172.19.12.4:git-repos/AIWorkspace.git (fetch)
origin   root@172.19.12.4:git-repos/AIWorkspace.git (push)
github   https://github.com/liuzf1218/AIWorkspace.git (fetch)
github   https://github.com/liuzf1218/AIWorkspace.git (push)
```

> **双远程说明**：
> - `origin` = 你的私有服务器（原有）
> - `github` = GitHub 公开仓库（新增）

### 8.2 推送代码到 GitHub

```bash
# 将本地 master 分支推送到 github 远程
git push github master
```

**首次推送 HTTPS 方式会要求认证**：

```
Username for 'https://github.com': liuzf1218
Password for 'https://github.com':
```

> **注意**：这里的 Password 不是 GitHub 登录密码，而是 **Personal Access Token (PAT)**。  
> 若尚未配置 PAT，请按 [10.2 节](#102-配置-github-个人访问令牌-pat) 操作。

**推送成功输出**：

```
To https://github.com/liuzf1218/AIWorkspace.git
 * [new branch]      master -> master
```

### 8.3 删除 GitHub 上的文件（如有需要）

若发现某些文件不应出现在 GitHub（如内部文档、临时文件），执行：

```bash
# 从 Git 跟踪中删除文件（本地文件保留）
git rm --cached "文件名"

# 从 Git 跟踪中删除整个目录（本地保留）
git rm -r --cached "目录名"

# 提交删除操作
git commit -m "chore: 移除内部文件和临时文档"

# 推送到 GitHub
git push github master
```

**本项目实际执行**：

```bash
git rm "新建 文本文档.txt"
git rm -r .kimi/
git rm docs/HOME_DEV_GUIDE.md
git commit -m "chore: 移除内部文件和临时文档"
git push github master
```

---

## 九、第七步：验证与后续更新

### 9.1 验证仓库页面

浏览器打开：`https://github.com/liuzf1218/AIWorkspace`

检查清单：
- [ ] README.md 正常渲染，标题、徽章、图片显示正确
- [ ] LICENSE 文件存在，点击后显示 MIT 协议内容
- [ ] 代码文件目录结构正确
- [ ] 提交历史（Commits）完整可见
- [ ] 无敏感信息泄露

### 9.2 后续代码更新推送

日常开发后，同步两个远程仓库：

```bash
# 推送到私有服务器（origin）
git push origin master

# 推送到 GitHub（github）
git push github master

# 或者一次推送所有远程的所有分支
git push --all origin
git push --all github
```

### 9.3 仅从 GitHub 下载（clone）

其他人可通过以下命令获取你的项目：

```bash
git clone https://github.com/liuzf1218/AIWorkspace.git
```

---

## 十、常见问题

### 10.1 push 时提示 "Repository not found"

**原因**：GitHub 仓库尚未创建，或仓库名填写错误。  
**解决**：确认仓库已创建，且 `git remote add github` 的 URL 正确。

### 10.2 配置 GitHub 个人访问令牌 (PAT)

GitHub 已停止支持密码直接登录 Git，必须使用 PAT。

**生成步骤**：

1. 登录 GitHub → 右上角头像 → **Settings**
2. 左侧最下方 → **Developer settings** → **Personal access tokens** → **Tokens (classic)**
3. 点击 **Generate new token (classic)**
4. Note 填写：`AIWorkspace Push`
5. Expiration 选择：`No expiration`（或自定义过期时间）
6. 勾选权限：至少勾选 **`repo`**（完整仓库权限）
7. 点击 **Generate token**
8. **立即复制生成的令牌**（页面关闭后无法再次查看）

**使用方式**：

```bash
# 推送时弹出的 Password 提示中，粘贴 PAT 即可
# 若希望永久保存，配置 Git 凭证管理器：
git config --global credential.helper manager
# 下次推送时输入一次 PAT，后续自动填充
```

### 10.3 push 时提示 "rejected: non-fast-forward"

**原因**：GitHub 仓库已有提交历史，与本地历史不一致。  
**解决**：

```bash
# 若确认 GitHub 上的内容可覆盖，强制推送（慎用）
git push github master --force

# 或者先拉取合并
git pull github master
git push github master
```

### 10.4 如何删除 GitHub 上的某个文件但保留本地文件

```bash
git rm --cached 文件名
git commit -m "chore: 从仓库移除文件，保留本地副本"
git push github master
```

### 10.5 只想推送到 GitHub，不想保留私有服务器远程

```bash
# 删除 origin 远程
git remote remove origin

# 将 github 重命名为 origin（可选，符合习惯）
git remote rename github origin
```

---

## 十一、命令速查表

| 目的 | 命令 |
|------|------|
| 查看远程仓库 | `git remote -v` |
| 添加远程仓库 | `git remote add github https://github.com/用户名/仓库名.git` |
| 推送到指定远程 | `git push github master` |
| 查看当前状态 | `git status` |
| 添加文件到暂存区 | `git add 文件名` 或 `git add .` |
| 提交变更 | `git commit -m "提交说明"` |
| 查看提交历史 | `git log --oneline -10` |
| 从 Git 移除文件（保留本地） | `git rm --cached 文件名` |
| 强制推送（覆盖远程） | `git push github master --force` |
| 拉取远程更新 | `git pull github master` |
| 删除远程仓库地址 | `git remote remove github` |

---

## 附录：本次 AI Workspace 上传的完整时间线

| 时间 | 操作 | 说明 |
|------|------|------|
| T+0 | 项目诊断 | 确认已有 origin 远程，缺少 README/LICENSE |
| T+1 | 敏感信息扫描 | 发现 HOME_DEV_GUIDE.md 和 RELEASE_GUIDE.md 含内网 IP 和密码 |
| T+2 | 脱敏处理 | IP → YOUR_SERVER_IP，密码 → YOUR_PASSWORD 等 |
| T+3 | 撰写 README.md | 中英双语，含徽章、功能特性、快速开始 |
| T+4 | 生成 LICENSE | MIT 协议，署名 liuzf1218 |
| T+5 | 本地提交 | `git add` + `git commit` |
| T+6 | 创建 GitHub 仓库 | 用户手动在 github.com/new 创建 AIWorkspace |
| T+7 | 添加远程并推送 | `git remote add github` + `git push github master` |
| T+8 | 移除多余文件 | 删除 新建文本文档.txt、.kimi/、HOME_DEV_GUIDE.md |
| T+9 | README 升级双语 | 更新为完整中英双语格式 |
| T+10 | 最终验证 | 确认仓库页面渲染正常，无敏感信息 |

---

> 文档版本：V1.0  
> 适用项目：AI Workspace  
> 撰写日期：2026-05-26
