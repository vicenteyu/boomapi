# 🚀 BoomApi: The 14MB Miracle

<div align="center">
  <img src="https://github.com/vicenteyu/boomapi/raw/master/logo.png" height="200" alt="BoomApi Logo">
</div>

---

[**简体中文**](#-简体中文) | [**English**](#-english)

---

## <a id="简体中文"></a>🇨🇳 简体中文

**BoomApi** 是一个基于 **.NET 10 Native AOT** 构建的极致轻量级 API Mock 工具。它采用“文件系统即路由”的设计逻辑，通过简单的文件操作或 Web UI 即可快速定义接口并返回原始（Raw）内容。

**可能是地球上最轻量的 .NET 10 API Mock 工具。**

### ✨ 核心特性
* **⚡ 巅峰性能**：原生编译，启动速度低于 `10ms`，零运行时依赖，极致节省资源。
* **📦 极简部署**：Docker 镜像仅约 `14 MB`，基于 Ubuntu Chiseled 构建，安全且精简。
* **📂 文件即路由**：`/raw/example.json` 自动对应 `data/example.json`，支持所有 HTTP 方法。
* **⏳ 延迟模拟**：支持文件名约定（如 `api.delay-500ms.json`），自动模拟网络延迟，方便测试前端 `Loading` 状态。
* **🎨 可视化管理**：内建基于 `Tailwind CSS` 的响应式 UI，支持在线创建、预览、管理 Mock 文件及延迟状态。

### 📂 文件即路由 & 延迟模拟
定义一个接口就像创建一个文件一样简单。无需复杂的配置或数据库：

- `data/health.json` --> `http://host:8080/raw/health.json`
- **进阶：模拟延迟**：只需在文件名中包含 `.delay-{ms}ms` 关键字：
  - `data/user.delay-2000ms.json` --> 访问该接口将自动延迟 **`2秒`** 返回。
  - 管理后台会通过琥珀色时钟图标 ⏳ 自动标注这些具有延迟的接口。

### 📊 性能规格 (Performance Stats)

| 指标 | 表现 | 备注 |
| :--- | :--- | :--- |
| **压缩镜像体积** | **14.18 MB** | 基于 Noble Chiseled 极致裁剪 |
| **冷启动时间** | **< 10ms** | Native AOT 原生编译，无 IL 解释 |
| **运行时环境** | **Zero Runtime** | 自包含，镜像内无 .NET 虚拟机 |
| **依赖环境** | **无** | 零依赖，不需安装 .NET Runtime |

### 🚀 快速启动

使用 Docker 一键运行：

```bash
# 创建本地目录
mkdir -p mocks logs

# 赋予权限（必须，因为容器以 UID 1654 运行）
sudo chown -R 1654:1654 mocks logs

# 一键启动
docker run -d \
  -p 8080:8080 \
  -v $(pwd)/mocks:/app/data \
  -v $(pwd)/logs:/app/logs \
  --name boomapi \
  vicenteyu105/boomapi:latest
```

### 🔒 权限与安全说明
本镜像基于 Ubuntu Chiseled 构建，程序以非 root 用户（UID 1654）身份运行。如果挂载了本地数据卷，必须手动修正宿主机目录权限，否则程序将因无法写入数据或日志而崩溃：

```bash
# 在宿主机执行以下命令，将目录所有权授予容器用户
sudo chown -R 1654:1654 ./your-data-dir ./your-logs-dir
```

访问 http://localhost:8080 即可进入管理后台。

---
<div align="center">
  <img src="https://github.com/vicenteyu/boomapi/raw/master/snapshot/snapshot-1.png">
</div>
<div align="center">
  <img src="https://github.com/vicenteyu/boomapi/raw/master/snapshot/snapshot-2.png">
</div>
---

"**Small, but Uncompromising.**" 感谢所有对极致性能有追求的开发者。如果你喜欢这个项目，请为它点亮一颗 Star 🌟。

**GitHub Repository:** https://github.com/vicenteyu/boomapi

---

## <a id="english"></a>🇺🇸 English

**Probably the world's most lightweight API Mocking tool powered by .NET 10.**

**BoomApi** is an ultra-lightweight API Mocking tool built with **.NET 10 Native AOT**. It follows a "File System as Routing" philosophy, allowing you to define endpoints and return raw content through simple file operations or a sleek Web UI.

### ✨ Key Features
* **⚡ Peak Performance**: Native compilation, `<10ms` startup time, zero runtime dependencies.
* **📦 Minimal Footprint**: Docker image is only `~14 MB`, built on Ubuntu Chiseled for maximum security and efficiency.
* **📂 File-system as Routing**: `/raw/example.json` automatically maps to `data/example.json`. Supports all HTTP verbs.
* **⏳ Latency Simulation**: Built-in support for filename conventions (e.g., `api.delay-500ms.json`) to simulate network throttling and test frontend `loading` states.
* **🎨 Visual Dashboard**: Embedded responsive UI powered by `Tailwind CSS` for creating, previewing, and managing mock files.

### 📂 Philosophy: File-system as Routing
Defining an API is as simple as creating a file. No complex JSON schemas or databases required:

- `data/health.json` --> `http://host:8080/raw/health.json`
- **Advanced**: Latency Throttling Simply include the `.delay-{ms}ms` keyword in the filename:
 - `data/api.delay-2000ms.json` --> The API will introduce a **`2s`** delay before responding.
 - The dashboard automatically detects this and displays a dedicated "Hourglass" ⏳ tag.


### 📊 Unrivaled Performance

- **Compressed Image Size**: `14.18 MB` (Smaller than a RAW photo).
- **Startup Latency**: `< 10ms` (Ready before you can blink).
- **Runtime Environment**: `Zero Runtime Dependencies` (Self-contained, no .NET VM inside).
- **Tech Stack**: Built with `.NET 10` + `Native AOT` + `Ubuntu 24.04 Noble Chiseled`.

### 🚀 Quick Start

Run with Docker:

```bash
mkdir -p mocks logs
sudo chown -R 1654:1654 mocks logs
docker run -d \
  -p 8080:8080 \
  -v $(pwd)/mocks:/app/data \
  -v $(pwd)/logs:/app/logs \
  --name boomapi \
  vicenteyu105/boomapi:latest
```

### 🔒 Permissions & Security
This image is built on Ubuntu Chiseled and runs as a non-root user (**UID 1654**). If you are using bind mounts, you must adjust the host directory permissions, or the application will crash due to lack of write access:

```Bash
# Run on your host machine to grant ownership to the container user
sudo chown -R 1654:1654 ./your-data-dir ./your-logs-dir
```

Access the dashboard at http://localhost:8080.

### 🛠️ Build from Source
Ensure you have .NET 10 SDK and C++ Build Tools (Clang & zlib1g-dev for Linux) installed.
```bash
# 1. Restore native assets for Linux
dotnet restore -r linux-x64

# 2. Publish with Native AOT
dotnet publish -c Release -r linux-x64 --no-restore /p:PublishAot=true -o ./publish
```

### 📝 License
Distributed under the MIT License. See LICENSE for more information.

---

Built with ❤️ and .NET 10
