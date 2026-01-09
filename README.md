<div align="center">
  <img src="https://github.com/vicenteyu/boomapi/raw/master/logo.png" height="200" alt="BoomApi Logo">
</div>

# 🚀 BoomApi: The 16MB Miracle

[**简体中文**](#-简体中文) | [**English**](#-english)

---

## <a id="简体中文"></a>🇨🇳 简体中文

**BoomApi** 是一个基于 **.NET 10 Native AOT** 构建的极致轻量级 API Mock 工具。它采用“文件系统即路由”的设计逻辑，通过简单的文件操作或 Web UI 即可快速定义接口并返回原始（Raw）内容。

**可能是地球上最轻量的 .NET 10 API Mock工具。**

### ✨ 核心特性
* **⚡ 巅峰性能**：原生编译，零 GC 开销，启动速度低于 10ms。
* **📦 极简部署**：Docker 镜像仅约 `16.53 MB`，无需安装 .NET 运行时，自包含执行。
* **📂 文件即路由**：`/raw/example.json` 自动对应物理路径 `wwwroot/example.json`，支持所有 HTTP 方法（`GET`, `POST`, `PUT`, `DELETE`, `PATCH`）。
* **🎨 可视化管理**：内建基于 `Tailwind CSS` 的响应式 UI，支持在线创建、预览及删除。
* **🔒 生产就绪**：完美兼容反向代理（`X-Forwarded Headers`），支持 `Docker` 数据卷持久化。

### 📊 性能规格 (Performance Stats)

| 指标 | 表现 | 备注 |
| :--- | :--- | :--- |
| **压缩镜像体积** | **16.53 MB** | 基于 Noble Chiseled 极致裁剪 |
| **冷启动时间** | **< 10ms** | Native AOT 原生编译，无 IL 解释 |
| **运行时环境** | **Zero Runtime** | 自包含，镜像内无 .NET 虚拟机 |
| **依赖环境** | **无** | 零依赖，不需安装 .NET Runtime |

### 🚀 快速启动

使用 Docker 一键运行（建议挂载数据卷以持久化数据）：

```bash
docker run -d -p 8080:8080 -v $(pwd)/mocks:/app/data vicenteyu105/boomapi:latest
```


访问 http://localhost:8080 即可进入管理后台。

---

## <a id="english"></a>🇺🇸 English

**Probably the world's most lightweight API Mocking tool powered by .NET 10.**

**BoomApi** is an ultra-lightweight API Mocking tool built with **.NET 10 Native AOT**. It follows a "File System as Routing" philosophy, allowing you to define endpoints and return raw content through simple file operations or a sleek Web UI.

### ✨ Key Features
* **⚡ Blazing Fast**: Native compilation, zero GC overhead, sub-10ms startup time.
* **📦 Tiny Footprint**: ~40MB Docker image, self-contained, no .NET runtime required.
* **📂 File-based Routing**: `/raw/test.json` automatically maps to `wwwroot/test.json`. Supports all HTTP methods including `GET`, `POST`, `PUT`, `DELETE`, and `PATCH`.
* **🎨 Built-in Dashboard**: Minimalist responsive UI powered by `Tailwind CSS` for easy management.
* **🔒 Proxy Ready**: Full support for `X-Forwarded-Proto` and `X-Forwarded-Host` headers.

### 📊 Unrivaled Performance

- **Compressed Image Size**: `16.53 MB` (Smaller than a RAW photo).
- **Startup Latency**: `< 10ms` (Ready before you can blink).
- **Runtime Environment**: `Zero Runtime Dependencies` (Self-contained, no .NET VM inside).
- **Tech Stack**: Built with `.NET 10` + `Native AOT` + `Ubuntu 24.04 Noble Chiseled`.

### 📂 Philosophy: File-system as Routing

Defining an API is as simple as creating a file. No complex JSON schemas or heavy DBs required:

- `wwwroot/api/health.json` --> `GET/POST http://host:8080/raw/api/health.json`

### 🚀 Quick Start

Run with Docker:

```bash
docker run -d -p 8080:8080 -v $(pwd)/mocks:/app/data vicenteyu105/boomapi:latest
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
