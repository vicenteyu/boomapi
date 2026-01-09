# 🚀 BoomApi

[**简体中文**](#-简体中文) | [**English**](#-english)

---

## <a id="简体中文"></a>🇨🇳 简体中文

**BoomApi** 是一个基于 **.NET 10 Native AOT** 构建的极致轻量级 API Mock 工具。它采用“文件系统即路由”的设计逻辑，通过简单的文件操作或 Web UI 即可快速定义接口并返回原始（Raw）内容。



### ✨ 核心特性
* **⚡ 巅峰性能**：原生编译，零 GC 开销，启动速度低于 10ms。
* **📦 极简部署**：Docker 镜像仅约 40MB，无需安装 .NET 运行时，自包含执行。
* **📂 文件即路由**：`/raw/example.json` 自动对应物理路径 `wwwroot/example.json`，支持所有 HTTP 方法（`GET`, `POST`, `PUT`, `DELETE`, `PATCH`）。
* **🎨 可视化管理**：内建基于 `Tailwind CSS` 的响应式 UI，支持在线创建、预览及删除。
* **🔒 生产就绪**：完美兼容反向代理（X-Forwarded Headers），支持 Docker 数据卷持久化。

### 🚀 快速启动

使用 Docker 一键运行（建议挂载数据卷以持久化数据）：

```bash
docker run -d \
  --name boomapi \
  -p 8080:8080 \
  -v $(pwd)/data:/app/wwwroot \
  ghcr.io/你的用户名/boomapi:latest
```

访问 http://localhost:8080 即可进入管理后台。

---

## <a id="english"></a>🇺🇸 English

**BoomApi** is an ultra-lightweight API Mocking tool built with **.NET 10 Native AOT**. It follows a "File System as Routing" philosophy, allowing you to define endpoints and return raw content through simple file operations or a sleek Web UI.

### ✨ Key Features
* **⚡ Blazing Fast**: Native compilation, zero GC overhead, sub-10ms startup time.
* **📦 Tiny Footprint**: ~40MB Docker image, self-contained, no .NET runtime required.
* **📂 File-based Routing**: `/raw/test.json` automatically maps to `wwwroot/test.json`. Supports all HTTP methods including `GET`, `POST`, `PUT`, `DELETE`, and `PATCH`.
* **🎨 Built-in Dashboard**: Minimalist responsive UI powered by Tailwind CSS for easy management.
* **🔒 Proxy Ready**: Full support for X-Forwarded-Proto and X-Forwarded-Host headers.

### 🚀 Quick Start
Run with Docker:

```bash
docker run -d \
  --name boomapi \
  -p 8080:8080 \
  -v $(pwd)/data:/app/wwwroot \
  ghcr.io/你的用户名/boomapi:latest
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
