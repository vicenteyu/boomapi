using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BoomApi;

/// <summary>
/// 纯文本接口应用
/// </summary>
public partial class Program
{
    static void ConfigureLogger(WebApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console();

        // 根据环境设置等级
        if (builder.Environment.IsDevelopment())
        {
            loggerConfig.MinimumLevel.Debug();
        }
        else
        {
            loggerConfig.MinimumLevel.Information();
        }

        // 文件日志：注意 AOT 环境下相对路径的权限问题
        loggerConfig.WriteTo.Async(p => p.File(
            path: "logs/logs-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7));

        Log.Logger = loggerConfig.CreateLogger();

        builder.Logging.ClearProviders();
        builder.Host.UseSerilog(); // .NET 8/9/10 推荐使用此方式集成
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        if (OperatingSystem.IsWindows())
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            builder.Host.UseWindowsService();
        }

        ConfigureLogger(builder);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        builder.Services.AddOpenApi();
        builder.Services.AddAntiforgery();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        // 直接通过 ContentRootPath 构造 wwwroot 路径（无论 WebRootPath 是否为 null）
        var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
            app.Environment.WebRootPath = wwwrootPath; // 手动赋值
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAntiforgery();

        app.MapGet("/", async ([FromServices] IWebHostEnvironment env, HttpContext context, ILogger<Program> logger) =>
        {
            var list = new DirectoryInfo(env.WebRootPath)
                .EnumerateFiles()
                .OrderByDescending(p => p.LastWriteTime);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                // 在MapGet委托中添加
                var headersDebug = string.Join("<br>", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"));
                // 然后在HTML中输出headersDebug查看实际接收到的头信息
                logger.LogDebug("Request Headers: <br>{HeadersDebug}", headersDebug);
            }

            // 获取真实协议（支持反向代理场景）
            var scheme = context.Request.Headers.TryGetValue("X-Forwarded-Proto", out StringValues _scheme)
                ? _scheme.First()
                : context.Request.Scheme;

            // 获取真实域名（支持反向代理场景）
            var host = context.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues _host)
                ? _host.First()
                : context.Request.Host.ToString();

            var baseUrl = $"{scheme}://{host}";

            var htmlContent = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <title>Boom API</title>
                    <script src="https://cdn.tailwindcss.com"></script>
                    <link href="https://cdn.jsdelivr.net/npm/font-awesome@4.7.0/css/font-awesome.min.css" rel="stylesheet">
                            <style type="text/tailwindcss">
                        @layer utilities {
                            .content-auto { content-visibility: auto; }
                            .file-item-shadow { box-shadow: 0 1px 3px rgba(0,0,0,0.05); }
                            .btn-transition { transition: all 0.2s ease-in-out; }
                        }
                    </style>
                </head>
                <body class="bg-gray-50 min-h-screen p-6">
                    <div class="max-w-4xl mx-auto bg-white rounded-lg shadow-md overflow-hidden">
                        <div class="px-6 py-4 bg-gray-800 text-white">
                            <h1 class="text-2xl font-bold flex items-center">
                                <i class="fa fa-folder-open mr-3"></i>WebRoot Files
                            </h1>
                        </div>
                
                        <!-- 操作栏 -->
                        <div class="px-6 py-4 border-b border-gray-100 flex justify-between items-center">
                            <div class="text-sm text-gray-500">
                                共 {{list.Count()}} 个文本
                            </div>
                            <a href="/create" class="px-4 py-2 bg-green-500 text-white rounded-lg hover:bg-green-600 btn-transition flex items-center shadow-sm">
                                <i class="fa fa-plus mr-2"></i> 新增文本
                            </a>
                        </div>

                        <div class="p-4">
                            {{string.Join("\n", list.Select(p => $@"
                                <div class='file-item-shadow rounded-lg flex items-center justify-between p-4 mb-3 bg-white hover:bg-gray-50 transition-colors'>
                                    <!-- 文本信息区域 (占比70%) -->
                                    <div class='flex-1 min-w-0 mr-4'>
                                        <div class='flex items-center space-x-4'>
                                            <div class='w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center text-blue-500 flex-shrink-0'>
                                                <i class='fa fa-file-text-o text-lg'></i>
                                            </div>
                                            <div class='min-w-0'>
                                                <div class='text-gray-800 font-medium truncate'>{$"{baseUrl}/raw/{Path.GetFileName(p.Name)}"}</div>
                                                <div class='text-xs text-gray-500 mt-0.5'>
                                                    {(p.Length > 1024 ? $"{p.Length / 1024} KB" : $"{p.Length} B")}
                                                    ·
                                                    {p.LastWriteTime:yyyy-MM-dd HH:mm}
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                        
                                    <!-- 操作按钮区域 (占比30%，最小宽度确保按钮不换行) -->
                                    <div class='flex-shrink-0 min-w-[220px] flex justify-end space-x-3'>
                                        <button onclick='viewFile(`{Uri.EscapeDataString(p.Name)}`)' 
                                            class='px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 btn-transition flex items-center shadow-sm flex-shrink-0'>
                                            <i class='fa fa-eye mr-2'></i> 查看
                                        </button>
                                        <button onclick='deleteFile(`{Uri.EscapeDataString(p.Name)}`)' 
                                            class='px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 btn-transition flex items-center shadow-sm flex-shrink-0'>
                                            <i class='fa fa-trash mr-2'></i> 删除
                                        </button>
                                    </div>
                                </div>
                            "))}}
                        </div>

                        <div class="px-6 py-3 bg-gray-50 text-center text-xs text-gray-500">
                            © {{DateTime.Now.Year}} 纯文本接口应用 | 共 {{list.Count()}} 个文本
                        </div>

                    </div>
        
                    <script src="/scripts/file-actions.js"></script>
                </body>
                </html>
                """;

            return TypedResults.Content(htmlContent, "text/html", Encoding.UTF8);
        });

        // 添加外部JS文本路由
        app.MapGet("/scripts/file-actions.js", () =>
        {
            var jsContent = @"""use strict"";
                function viewFile(filePath) {
                    fetch(`/raw/${filePath}`)
                        .then(res => res.blob())
                        .then(blob => {
                            const url = URL.createObjectURL(blob);
                            window.open(url);
                        })
                        .catch(error => console.error('查看文本失败:', error));
                }
    
                function deleteFile(filePath) {
                    if(confirm('确定要删除此文本吗？')) {
                        fetch(`/delete/${filePath}`, { method: 'DELETE' })
                            .then(res => res.text())
                            .then(msg => {
                                location.reload();
                            })
                            .catch(error => console.error('删除文本失败:', error));
                    }
                }";

            return TypedResults.Text(jsContent, "application/javascript");
        });

        app.MapDelete("/delete/{path}", async Task<Results<Ok<string>, ProblemHttpResult>> (string path, [FromServices] IWebHostEnvironment env) =>
        {
            var file_path = Path.Combine(env.WebRootPath, path);
            if (!File.Exists(file_path))
                return TypedResults.Problem("Path is not existed.");
            File.Delete(file_path);
            return TypedResults.Ok("Delete success.");
        });

        app.MapGet("/create", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);

            var htmlContent = $$"""
                <!DOCTYPE html>
                <html lang="zh-CN">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>创建新文本 - 纯文本接口应用</title>
                    <script src="https://cdn.tailwindcss.com"></script>
                    <link href="https://cdn.jsdelivr.net/npm/font-awesome@4.7.0/css/font-awesome.min.css" rel="stylesheet">
                    <style type="text/tailwindcss">
                        @layer utilities {        
                            .form - focus { @apply focus:ring-2 focus:ring-blue-500 focus:border-blue-500 focus:outline-none; }
                            .btn-primary { @apply bg-blue-500 hover:bg-blue-600 text-white font-medium py-2 px-4 rounded-lg transition-colors duration-200 flex items-center justify-center; }
                            .btn-secondary { @apply bg-gray-200 hover:bg-gray-300 text-gray-700 font-medium py-2 px-4 rounded-lg transition-colors duration-200 flex items-center justify-center; }
                        }
                    </style>
                </head>
                <body class="bg-gray-100 min-h-screen p-4 md:p-6 flex flex-col">
                    <div class="max-w-2xl w-full mx-auto bg-white rounded-xl shadow-md overflow-hidden flex-grow flex flex-col">
                        <!-- 顶部导航 -->
                        <div class="px-6 py-4 bg-gradient-to-r from-gray-800 to-gray-900 text-white">
                            <div class="flex items-center justify-between">
                                <h1 class="text-xl font-bold flex items-center">
                                    <i class="fa fa-file-text-o mr-2 text-blue-400"></i>创建新文本
                                </h1>
                                <a href="/" class="text-gray-300 hover:text-white transition-colors">
                                    <i class="fa fa-arrow-left"></i> 返回列表
                                </a>
                            </div>
                        </div>
            
                        <!-- 表单区域 -->
                        <div class="p-6 flex-grow">
                            <form method="POST" class="space-y-6">
                                <input type='hidden' name='{{tokens.FormFieldName}}' value='{{tokens.RequestToken}}'>
                    
                                <!-- 文本路径输入 -->
                                <div class="space-y-2">
                                    <label for="path" class="block text-sm font-medium text-gray-700 flex items-center">
                                        <i class="fa fa-folder-open text-gray-400 mr-2"></i>文本路径
                                    </label>
                                    <div class="relative">
                                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                            <i class="fa fa-file-o text-gray-400"></i>
                                        </div>
                                        <input type="text" id="path" name="path" 
                                            class="block w-full pl-10 pr-3 py-3 border border-gray-300 rounded-lg shadow-sm form-focus"
                                            placeholder="例如: document.txt">
                                    </div>
                                    <p class="text-xs text-gray-500">
                                        <i class="fa fa-info-circle mr-1"></i> 只支持 数字、字母、- 和 . 。
                                    </p>
                                </div>
                    
                                <!-- 文本内容输入 -->
                                <div class="space-y-2">
                                    <label for="raw" class="block text-sm font-medium text-gray-700 flex items-center">
                                        <i class="fa fa-edit text-gray-400 mr-2"></i>文本内容
                                    </label>
                                    <div class="relative">
                                        <textarea id="raw" name="raw" rows="12" 
                                            class="block w-full p-3 border border-gray-300 rounded-lg shadow-sm form-focus font-mono text-sm"
                                            placeholder="在此输入文本内容..."></textarea>
                                    </div>
                                </div>
                    
                                <!-- 操作按钮 -->
                                <div class="flex justify-end space-x-3 pt-4 border-t border-gray-100">
                                    <a href="/" class="btn-secondary">
                                        <i class="fa fa-times mr-2"></i>取消
                                    </a>
                                    <button type="submit" class="btn-primary">
                                        <i class="fa fa-save mr-2"></i>提交
                                    </button>
                                </div>
                            </form>
                        </div>
            
                        <!-- 页脚 -->
                        <div class="px-6 py-3 bg-gray-50 text-center text-xs text-gray-500">
                            © {{DateTime.Now.Year}} 纯文本接口应用
                        </div>
                    </div>
                </body>
                </html>
                """;

            return TypedResults.Content(htmlContent, "text/html", Encoding.UTF8);
        });

        app.MapPost("/create", async Task<Results<RedirectHttpResult, ProblemHttpResult>> ([FromForm] string path, [FromForm] string raw, [FromServices] IWebHostEnvironment env) =>
        {
            if (!FileNameRule().IsMatch(path))
                return TypedResults.Problem("Path is not match rules.");

            var file_path = Path.Combine(env.WebRootPath, path);
            if (File.Exists(file_path))
                return TypedResults.Problem("Path is existed.");

            await File.AppendAllTextAsync(file_path, raw, encoding: Encoding.UTF8);

            return TypedResults.Redirect($"~/");
        });

        app.MapMethods("/raw/{path}", ["GET", "POST", "DELETE", "PUT", "PATCH"], async Task<Results<ContentHttpResult, NotFound>> (string path, [FromServices] IWebHostEnvironment env) =>
        {
            var file_path = Path.Combine(env.WebRootPath, path);
            if (!File.Exists(file_path))
                return TypedResults.NotFound();

            var content = await File.ReadAllTextAsync(file_path);

            var contentType = Path.GetExtension(path).ToLower() switch
            {
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".html" => "text/html",
                _ => "text/plain"
            };

            return TypedResults.Text(content, contentType, Encoding.UTF8);
        }).AddOpenApiOperationTransformer((opperation, context, ct) =>
        {
            return Task.CompletedTask;
        });

        app.Run();
    }

    [GeneratedRegex("^[a-zA-Z1-9][a-zA-Z1-9.-]*[a-zA-Z1-9]$")]
    private static partial Regex FileNameRule();
}

[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
