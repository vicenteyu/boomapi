using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BoomApi;

public partial class Program
{
    static void ConfigureLogger(WebApplicationBuilder builder)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console();

        if (builder.Environment.IsDevelopment())
            loggerConfig.MinimumLevel.Debug();
        else
            loggerConfig.MinimumLevel.Information();

        loggerConfig.WriteTo.Async(p => p.File(
            path: "logs/logs-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7));

        Log.Logger = loggerConfig.CreateLogger();
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog();
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

        var dataPath = Path.Combine(AppContext.BaseDirectory, "data");
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        if (!Directory.EnumerateFileSystemEntries(dataPath).Any())
        {
            File.WriteAllText(Path.Combine(dataPath, "hello-world.json"), Assets.HelloJson, Encoding.UTF8);
            File.WriteAllText(Path.Combine(dataPath, "welcome.html"), Assets.WelcomeHtml, Encoding.UTF8);
            File.WriteAllText(Path.Combine(dataPath, ".initialized"), "");
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseRouting();
        app.UseAntiforgery();

        // --- 首页：英文国际化 + 布局优化 ---
        app.MapGet("/", async (HttpContext context, ILogger<Program> logger) =>
        {
            var list = new DirectoryInfo(dataPath)
                .EnumerateFiles()
                .Where(f => !f.Name.StartsWith('.'))
                .OrderByDescending(p => p.LastWriteTime);

            var scheme = context.Request.Headers.TryGetValue("X-Forwarded-Proto", out StringValues _scheme) ? _scheme.First() : context.Request.Scheme;
            var host = context.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues _host) ? _host.First() : context.Request.Host.ToString();
            var baseUrl = $"{scheme}://{host}";

            var htmlContent = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <title>BoomApi Explorer</title>
                    <link rel="icon" href="{{Assets.FaviconBase64}}">
                    <script src="https://cdn.tailwindcss.com"></script>
                    <link href="https://cdn.jsdelivr.net/npm/font-awesome@4.7.0/css/font-awesome.min.css" rel="stylesheet">
                    <style type="text/tailwindcss">
                        @layer utilities {
                            .btn-transition { transition: all 0.2s ease-in-out; }
                        }
                    </style>
                </head>
                <body class="bg-gray-50 min-h-screen p-4 md:p-8">
                    <div class="max-w-5xl mx-auto bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
                        <div class="px-6 py-5 bg-slate-900 text-white flex items-center justify-between">
                            <div class="flex items-center space-x-3">
                                <div class="w-10 h-10 bg-blue-500 rounded-xl flex items-center justify-center shadow-lg shadow-blue-500/20">
                                    <i class="fa fa-terminal text-white"></i>
                                </div>
                                <div>
                                    <h1 class="text-xl font-bold tracking-tight">BoomApi Hub</h1>
                                    <p class="text-xs text-slate-400 font-mono">16MB Miracle / .NET 10</p>
                                </div>
                            </div>
                            <a href="/create" class="bg-blue-600 hover:bg-blue-500 text-sm font-semibold px-5 py-2.5 rounded-xl transition-all flex items-center shadow-md">
                                <i class="fa fa-plus mr-2"></i> New Endpoint
                            </a>
                        </div>

                        <div class="px-6 py-3 bg-slate-50 border-b border-gray-100">
                            <span class="text-xs font-medium text-slate-500 uppercase tracking-wider">
                                {{list.Count()}} active endpoints found
                            </span>
                        </div>

                        <div class="p-6">
                            {{string.Join("\n", list.Select(p => $@"
                                <div class='group flex items-center justify-between p-4 mb-4 border border-gray-100 rounded-xl hover:border-blue-200 hover:bg-blue-50/30 transition-all'>
                                    <div class='flex-1 min-w-0 mr-6'>
                                        <div class='flex items-center space-x-4'>
                                            <div class='w-12 h-12 rounded-lg bg-gray-100 group-hover:bg-blue-100 flex items-center justify-center text-gray-400 group-hover:text-blue-500 transition-colors'>
                                                <i class='fa fa-code text-lg'></i>
                                            </div>
                                            <div class='min-w-0'>
                                                <div class='text-slate-900 font-semibold truncate text-sm md:text-base'>{$"{baseUrl}/raw/{Path.GetFileName(p.Name)}"}</div>
                                                <div class='flex items-center mt-1 space-x-3 text-xs text-slate-400'>
                                                    <span><i class='fa fa-hdd-o mr-1'></i> {(p.Length > 1024 ? $"{p.Length / 1024} KB" : $"{p.Length} B")}</span>
                                                    <span><i class='fa fa-clock-o mr-1'></i> {p.LastWriteTime:MMM dd, HH:mm}</span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class='flex items-center space-x-2'>
                                        <button onclick='viewFile(`{Uri.EscapeDataString(p.Name)}`)' class='p-2.5 text-slate-600 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-all'><i class='fa fa-external-link text-lg'></i></button>
                                        <button onclick='deleteFile(`{Uri.EscapeDataString(p.Name)}`)' class='p-2.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-all'><i class='fa fa-trash-o text-lg'></i></button>
                                    </div>
                                </div>
                            "))}}
                        </div>

                        <div class="px-6 py-4 bg-gray-50 text-center border-t border-gray-100 flex items-center justify-center space-x-4">
                            <span class="text-xs text-gray-400">© {{DateTime.Now.Year}} BoomApi | Powered by .NET 10 Native AOT</span>
                            <a href="https://github.com/vicenteyu/boomapi" target="_blank" class="text-gray-400 hover:text-slate-900 transition-colors">
                                <i class="fa fa-github text-lg"></i>
                            </a>
                        </div>
                    </div>
                    <script>
                        "use strict";
                        function viewFile(filePath) {
                            window.open(`/raw/${filePath}`);
                        }
                        function deleteFile(filePath) {
                            if(confirm('Are you sure you want to delete this endpoint?')) {
                                fetch(`/delete/${filePath}`, { method: 'DELETE' })
                                    .then(res => { if(res.ok) location.reload(); })
                                    .catch(error => console.error('Delete failed:', error));
                            }
                        }
                    </script>
                </body>
                </html>
                """;

            return TypedResults.Content(htmlContent, "text/html", Encoding.UTF8);
        });

        // --- 创建页：英文国际化 ---
        app.MapGet("/create", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            var htmlContent = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <title>Create Mock - BoomApi</title>
                    <link rel="icon" href="{{Assets.FaviconBase64}}">
                    <script src="https://cdn.tailwindcss.com"></script>
                    <link href="https://cdn.jsdelivr.net/npm/font-awesome@4.7.0/css/font-awesome.min.css" rel="stylesheet">
                </head>
                <body class="bg-gray-100 min-h-screen p-4 md:p-8 flex flex-col">
                    <div class="max-w-5xl w-full mx-auto bg-white rounded-2xl shadow-md overflow-hidden flex-grow flex flex-col">
                        <div class="px-6 py-5 bg-slate-900 text-white flex justify-between items-center">
                            <h1 class="text-xl font-bold flex items-center"><i class="fa fa-plus-circle mr-2 text-blue-400"></i>Create Endpoint</h1>
                            <a href="/" class="text-sm text-slate-400 hover:text-white transition-colors"><i class="fa fa-chevron-left mr-1"></i> Back to List</a>
                        </div>
                        <div class="p-8 flex-grow">
                            <form method="POST" class="space-y-6">
                                <input type='hidden' name='{{tokens.FormFieldName}}' value='{{tokens.RequestToken}}'>
                                <div class="space-y-2">
                                    <label class="block text-sm font-semibold text-slate-700">Endpoint Path</label>
                                    <input type="text" name="path" required class="w-full px-4 py-3 rounded-xl border border-gray-200 focus:ring-2 focus:ring-blue-500 focus:outline-none transition-all" placeholder="e.g. user-profile.json">
                                    <p class="text-[10px] text-slate-400 uppercase tracking-widest font-bold">Use alphanumeric, dots, and dashes only.</p>
                                </div>
                                <div class="space-y-2">
                                    <label class="block text-sm font-semibold text-slate-700">Raw Content (Payload)</label>
                                    <textarea name="raw" rows="12" class="w-full p-4 rounded-xl border border-gray-200 focus:ring-2 focus:ring-blue-500 focus:outline-none font-mono text-sm bg-gray-50" placeholder='{ "status": "success" }'></textarea>
                                </div>
                                <div class="flex justify-end space-x-4 pt-6">
                                    <a href="/" class="px-6 py-2.5 rounded-xl text-slate-500 font-medium hover:bg-gray-100 transition-all">Discard</a>
                                    <button type="submit" class="px-8 py-2.5 bg-blue-600 text-white font-bold rounded-xl hover:bg-blue-500 shadow-lg shadow-blue-500/20 transition-all">Deploy Mock</button>
                                </div>
                            </form>
                        </div>
                        <div class="px-6 py-4 bg-gray-50 text-center border-t border-gray-100 flex items-center justify-center space-x-4">
                            <span class="text-xs text-gray-400">© {{DateTime.Now.Year}} BoomApi | Powered by .NET 10 Native AOT</span>
                            <a href="https://github.com/vicenteyu/boomapi" target="_blank" class="text-gray-400 hover:text-slate-900 transition-colors">
                                <i class="fa fa-github text-lg"></i>
                            </a>
                        </div>
                    </div>
                </body>
                </html>
                """;
            return TypedResults.Content(htmlContent, "text/html", Encoding.UTF8);
        });

        app.MapPost("/create", async Task<Results<RedirectHttpResult, ProblemHttpResult>> ([FromForm] string path, [FromForm] string raw) =>
        {
            if (!FileNameRule().IsMatch(path)) return TypedResults.Problem("Invalid path format.");
            var file_path = Path.Combine(dataPath, path);
            if (File.Exists(file_path)) return TypedResults.Problem("Endpoint already exists.");
            await File.AppendAllTextAsync(file_path, raw, encoding: Encoding.UTF8);
            return TypedResults.Redirect($"~/");
        });

        app.MapDelete("/delete/{path}", async Task<Results<Ok<string>, ProblemHttpResult>> (string path) =>
        {
            var file_path = Path.Combine(dataPath, path);
            if (!File.Exists(file_path)) return TypedResults.Problem("Not found.");
            File.Delete(file_path);
            return TypedResults.Ok("Deleted.");
        });

        app.MapMethods("/raw/{path}", ["GET", "POST", "DELETE", "PUT", "PATCH"], async Task<Results<ContentHttpResult, NotFound>> (string path) =>
        {
            var file_path = Path.Combine(dataPath, path);
            if (!File.Exists(file_path)) return TypedResults.NotFound();
            var content = await File.ReadAllTextAsync(file_path);
            var contentType = Path.GetExtension(path).ToLower() switch
            {
                ".json" => "application/json; charset=utf-8",
                ".xml" => "application/xml; charset=utf-8",
                ".html" => "text/html; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".yaml" or ".yml" => "application/x-yaml; charset=utf-8",
                ".csv" => "text/csv; charset=utf-8",
                _ => "text/plain; charset=utf-8"
            };
            return TypedResults.Text(content, contentType, Encoding.UTF8);
        });

        app.Run();
    }

    [GeneratedRegex("^[a-zA-Z1-9][a-zA-Z1-9.-]*[a-zA-Z1-9]$")]
    private static partial Regex FileNameRule();
}

[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }