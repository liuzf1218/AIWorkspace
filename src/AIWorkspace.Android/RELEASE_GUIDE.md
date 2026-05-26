# AI Workspace Android - APK 发布指南

> 适用于 Linux 服务器环境（无 GUI），通过命令行完成 APK 构建与发布。

---

## 一、环境准备

### 1.1 系统要求

| 组件 | 版本 | 说明 |
|------|------|------|
| OS | Ubuntu 22.04+ / Debian 11+ | 或其他 Linux 发行版 |
| JDK | 17 | `apt install openjdk-17-jdk` |
| Android SDK | 34 | 命令行工具安装 |
| Git | 任意 | 拉取代码 |
| curl/wget | 任意 | 下载工具 |

### 1.2 安装 JDK 17

```bash
sudo apt update
sudo apt install -y openjdk-17-jdk

# 验证
java -version  # 应显示 17.x.x
javac -version # 应显示 17.x.x
```

### 1.3 安装 Android SDK（命令行方式）

```bash
# 创建目录
mkdir -p ~/android-sdk/cmdline-tools
cd ~/android-sdk/cmdline-tools

# 下载命令行工具（Linux 版）
curl -O https://dl.google.com/android/repository/commandlinetools-linux-11076708_latest.zip
unzip commandlinetools-linux-11076708_latest.zip
mv cmdline-tools latest

# 配置环境变量
echo 'export ANDROID_HOME=$HOME/android-sdk' >> ~/.bashrc
echo 'export PATH=$PATH:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools' >> ~/.bashrc
source ~/.bashrc

# 安装 SDK 组件
sdkmanager --licenses  # 全部输入 y 同意
sdkmanager "platform-tools" "platforms;android-34" "build-tools;34.0.0"
```

### 1.4 验证环境

```bash
adb --version        # Android Debug Bridge
sdkmanager --list    # 查看已安装组件
```

---

## 二、获取代码

### 2.1 从远程仓库克隆

```bash
# 在服务器上选择工作目录，例如 /opt/build
cd /opt
mkdir -p build && cd build

# 克隆（密码 YOUR_PASSWORD）
git clone YOUR_SSH_USER@YOUR_SERVER_IP:git-repos/AIWorkspace.git

cd AIWorkspace/src/AIWorkspace.Android
```

### 2.2 确保 gradlew 可执行

```bash
chmod +x gradlew
```

---

## 三、发布前配置

### 3.1 创建签名密钥库（Keystore）

> ⚠️ 密钥库文件极其重要，务必妥善保管，丢失后无法更新已发布的 APK。

```bash
# 进入 Android 项目目录
cd /opt/build/AIWorkspace/src/AIWorkspace.Android

# 创建密钥库（有效期 10000 天 ≈ 27 年）
keytool -genkey -v \
  -keystore aiworkspace-release.keystore \
  -alias aiworkspace \
  -keyalg RSA \
  -keysize 2048 \
  -validity 10000 \
  -storepass YOUR_KEYSTORE_PASSWORD \
  -keypass YOUR_KEYSTORE_PASSWORD \
  -dname "CN=AIWorkspace, OU=Dev, O=AIWorkspace, L=City, ST=State, C=CN"

# 移动密钥库到安全位置（可选）
mkdir -p ~/.android/keys
mv aiworkspace-release.keystore ~/.android/keys/
```

**参数说明：**
- `storepass` / `keypass`: 密钥库密码（生产环境请使用强密码）
- `dname`: 证书发行者信息，按实际情况修改

### 3.2 配置签名（app/build.gradle.kts）

编辑 `app/build.gradle.kts`，在 `android` 块内添加：

```kotlin
android {
    // ... 现有配置 ...

    signingConfigs {
        create("release") {
            storeFile = file(System.getenv("HOME") + "/.android/keys/aiworkspace-release.keystore")
            storePassword = "YOUR_KEYSTORE_PASSWORD"
            keyAlias = "aiworkspace"
            keyPassword = "YOUR_KEYSTORE_PASSWORD"
        }
    }

    buildTypes {
        release {
            isMinifyEnabled = true
            isShrinkResources = true
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
            signingConfig = signingConfigs.getByName("release")
        }
    }
}
```

> **安全提示**: 生产环境建议将密码放在环境变量或 `local.properties` 中，不要硬编码。

### 3.3 更新版本号

编辑 `app/build.gradle.kts` 的 `defaultConfig`：

```kotlin
defaultConfig {
    applicationId = "com.aiworkspace"
    minSdk = 26
    targetSdk = 34
    versionCode = 1          # ← 每次发布必须 +1
    versionName = "1.0.0"    # ← 语义化版本号
    // ...
}
```

### 3.4 检查 ProGuard 规则

当前 `app/proguard-rules.pro` 已包含基础规则。发布前确认：

```bash
cat app/proguard-rules.pro
```

确保包含：
- Room Entity 不被混淆
- kotlinx.serialization 序列化类保留
- OkHttp 相关排除警告

---

## 四、构建 APK

### 4.1 清理并构建 Release 版本

```bash
cd /opt/build/AIWorkspace/src/AIWorkspace.Android

# 清理旧构建
./gradlew clean

# 构建 Release APK
./gradlew assembleRelease
```

首次构建会下载 Gradle 和依赖，耗时约 5-15 分钟（取决于网络和服务器性能）。

### 4.2 构建输出位置

```
app/build/outputs/apk/release/app-release.apk
```

### 4.3 验证 APK

```bash
# 检查 APK 基本信息
ls -lh app/build/outputs/apk/release/app-release.apk

# 验证签名（使用 apksigner）
$ANDROID_HOME/build-tools/34.0.0/apksigner verify \
  --verbose \
  app/build/outputs/apk/release/app-release.apk

# 查看 APK 内容
$ANDROID_HOME/build-tools/34.0.0/aapt dump badging \
  app/build/outputs/apk/release/app-release.apk
```

---

## 五、分发与部署

### 5.1 提取 APK

```bash
# 复制到可访问目录
cp app/build/outputs/apk/release/app-release.apk \
   /var/www/html/aiworkspace-v1.0.0.apk

# 或 SCP 下载到本地
scp YOUR_SSH_USER@YOUR_SERVER_IP:/opt/build/AIWorkspace/src/AIWorkspace.Android/app/build/outputs/apk/release/app-release.apk ./
```

### 5.2 安装到设备（测试）

```bash
# 通过 ADB 安装到已连接的设备
adb install -r app/build/outputs/apk/release/app-release.apk
```

---

## 六、快速命令参考

```bash
# 一键构建（环境已配置好）
cd /opt/build/AIWorkspace/src/AIWorkspace.Android
./gradlew clean assembleRelease

# 仅构建 Debug 版（快速验证）
./gradlew assembleDebug

# 查看构建变体
./gradlew tasks --all | grep apk

# 查看依赖树
./gradlew app:dependencies
```

---

## 七、常见问题

### Q1: `./gradlew` 权限不足
```bash
chmod +x gradlew
```

### Q2: `JAVA_HOME` 未设置
```bash
export JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64
echo $JAVA_HOME
```

### Q3: 构建时内存不足
```bash
# 在 gradle.properties 中增加堆内存
org.gradle.jvmargs=-Xmx4096m -Dfile.encoding=UTF-8
```

### Q4: 签名失败 "Keystore file does not exist"
检查 `storeFile` 路径是否正确指向 `~/.android/keys/aiworkspace-release.keystore`。

### Q5: Room 编译错误（KSP）
确保已安装 `build-tools;34.0.0` 和 `platforms;android-34`。

---

## 八、版本迭代流程

下次发布时：

1. `git pull` 拉取最新代码
2. `versionCode` + 1
3. 更新 `versionName`
4. `./gradlew clean assembleRelease`
5. 提取新 APK
