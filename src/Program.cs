using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;
using System.Collections.Frozen;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BoomApi;

public partial class Program
{
    static void ConfigureLogger(WebApplicationBuilder builder)
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "logs-.txt");

        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console();

        if (builder.Environment.IsDevelopment())
            loggerConfig.MinimumLevel.Debug();
        else
            loggerConfig.MinimumLevel.Information();

        loggerConfig.WriteTo.Async(p => p.File(
            path: logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7));

        Log.Logger = loggerConfig.CreateLogger();
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog();
    }

    public static async Task Main(string[] args)
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

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = 100L * 1024 * 1024; // 100MB
        });

        builder.Services.AddOpenApi();

        var dataPath = Path.Combine(AppContext.BaseDirectory, "data");
        var initializedFlag = Path.Combine(dataPath, ".initialized");

        try
        {
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            if (!File.Exists(initializedFlag))
            {
                if (!Directory.EnumerateFileSystemEntries(dataPath).Any())
                {

                    await File.WriteAllTextAsync(Path.Combine(dataPath, "hello-world.json"), Assets.HelloJson, Encoding.UTF8);
                    await File.WriteAllTextAsync(Path.Combine(dataPath, "welcome.html"), Assets.WelcomeHtml, Encoding.UTF8);

                    Log.Information("Initial samples created successfully.");
                }
                else
                {
                    Log.Information("Data directory is not empty, skipping sample creation.");
                }

                await File.WriteAllTextAsync(initializedFlag, DateTime.Now.ToString(), Encoding.UTF8);
                Log.Information("Initialization flag created.");
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Log.Warning("Permission denied on {DataPath}.", dataPath);
            Log.Fatal(ex, "FATAL: Permission Denied! BoomApi (Running as UID 1654/app) cannot write to: {DataPath}. " +
                 "FIX: Run 'sudo chown -R 1654:1654 <host_dir>' on your host machine.", dataPath);

            throw;
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.MapGet("/",
            async (HttpContext context) =>
            {
                var list = new DirectoryInfo(dataPath)
                    .EnumerateFiles("*", SearchOption.AllDirectories)
                    .Where(f => !f.Name.StartsWith('.'))
                    .Select(p => new
                    {
                        p.Length,
                        RelativePath = Path.GetRelativePath(dataPath, p.FullName).Replace(Path.DirectorySeparatorChar, '/'),
                        p.LastWriteTime,
                        p.Name
                    })
                    .OrderByDescending(p => p.LastWriteTime);

                var scheme = context.Request.Headers.TryGetValue("X-Forwarded-Proto", out StringValues _scheme)
                    ? _scheme.First()
                    : context.Request.Scheme;
                var host = context.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues _host)
                    ? _host.First()
                    : context.Request.Host.ToString();
                var baseUrl = $"{scheme}://{host}";

                var listContent = list.Any()
                    ? string.Join("\n", list.Select(p =>
                    {
                        var sb = new StringBuilder(Assets.ListTemplate);
                        sb.Replace("{URL}", $"{baseUrl}/raw/{p.RelativePath}");
                        sb.Replace("{LENGTH}", p.Length > 1024 ? $"{p.Length / 1024} KB" : $"{p.Length} B");
                        sb.Replace("{TIME}", p.LastWriteTime.ToString("MMM dd, HH: mm"));
                        sb.Replace("{PATH}", p.RelativePath);
                        sb.Replace("{DELAY}", DelayMsRegex().Match(p.Name) is Match ma && ma.Success
                            ? $"<span class='text-amber-500 font-medium'><i class='fa fa-hourglass-half mr-1'></i>{ma.Groups[1].Value} ms</span>"
                            : string.Empty);

                        var (icon, colorStyle) = GetFileStyle(p.Name);
                        sb.Replace("{ICON_CLASS}", icon);
                        sb.Replace("{COLOR_STYLE}", colorStyle);

                        return sb.ToString();
                    }))
                    : Assets.EmptyState;

                var htmlContent = new StringBuilder(Assets.IndexHtml);
                htmlContent.Replace("{CURRENT_YEAR}", $"{DateTime.Now.Year}");
                htmlContent.Replace("{FAVICON}", Assets.FaviconBase64);
                htmlContent.Replace("{LIST_COUNT}", $"{list.Count()}");
                htmlContent.Replace("{LIST_CONTENT}", listContent);

                return TypedResults.Content(htmlContent.ToString(), "text/html", Encoding.UTF8);
            });

        app.MapGet("/create",
            async (HttpContext context) =>
            {
                var htmlContent = new StringBuilder(Assets.CreateHtml);
                htmlContent.Replace("{CURRENT_YEAR}", $"{DateTime.Now.Year}");
                htmlContent.Replace("{FAVICON}", Assets.FaviconBase64);

                return TypedResults.Content(htmlContent.ToString(), "text/html", Encoding.UTF8);
            });

        app.MapPost("/create",
            async Task<Results<RedirectHttpResult, ProblemHttpResult>> ([FromForm] string path, [FromForm] string raw) =>
            {
                try
                {
                    if (!IsPathSafe(dataPath, path)) return TypedResults.Problem("Invalid path.");
                    //if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    try { System.Text.Json.JsonDocument.Parse(raw); }
                    //    catch { return TypedResults.Problem("Invalid JSON format."); }
                    //}

                    var filePath = Path.Combine(dataPath, path);
                    var fileName = Path.GetFileName(filePath);
                    if (fileName.StartsWith('.')) return TypedResults.Problem("Invalid path format.");
                    if (File.Exists(filePath)) return TypedResults.Problem("Endpoint already exists.");
                    var folder = Path.GetDirectoryName(filePath);
                    if (folder is not null && !Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    await File.WriteAllTextAsync(filePath, raw, encoding: Encoding.UTF8);
                    return TypedResults.Redirect($"~/");
                }
                catch (UnauthorizedAccessException)
                {
                    return TypedResults.Problem($"Permission denied on {dataPath}. Please check UID 1654 ownership.");
                }
            }).DisableAntiforgery();

        app.MapDelete("/delete/{*path}",
            async Task<Results<Ok<string>, ProblemHttpResult>> (string path) =>
            {
                try
                {
                    if (!IsPathSafe(dataPath, path)) return TypedResults.Problem("Invalid path.");

                    var filePath = Path.Combine(dataPath, path);
                    if (!File.Exists(filePath)) return TypedResults.Problem("Not found.");
                    File.Delete(filePath);
                    return TypedResults.Ok("Deleted.");
                }
                catch (UnauthorizedAccessException)
                {
                    return TypedResults.Problem($"Permission denied on {dataPath}. Please check UID 1654 ownership.");
                }
            });

        app.MapMethods("/raw/{*path}", ["GET", "POST", "DELETE", "PUT", "PATCH"],
            async Task<Results<IResult, NotFound>> (string path) =>
            {
                var match = DelayMsRegex().Match(path);
                if (match.Success)
                {
                    int delay = int.Parse(match.Groups[1].Value);
                    await Task.Delay(delay);
                }

                try
                {
                    if (!IsPathSafe(dataPath, path)) return TypedResults.Problem("Invalid path.");

                    var filePath = Path.Combine(dataPath, path);
                    if (!File.Exists(filePath)) return TypedResults.NotFound();

                    var content = await File.ReadAllTextAsync(filePath);
                    var contentType = GetContentType(filePath);

                    return TypedResults.Text(content, contentType, Encoding.UTF8);
                }
                catch (UnauthorizedAccessException)
                {
                    return TypedResults.Problem($"Permission denied on {dataPath}. Please check UID 1654 ownership.");
                }
            });

        app.Run();
    }

    private static (string icon, string colorClass) GetFileStyle(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".json" => ("fa-file-code-o", "group-hover:text-amber-500 group-hover:bg-amber-100"),
            ".html" => ("fa-html5", "group-hover:text-orange-500 group-hover:bg-orange-100"),
            //".xml" => ("fa-xml", "group-hover:text-pink-500 group-hover:bg-pink-100"),
            ".js" => ("fa-terminal", "group-hover:text-yellow-500 group-hover:bg-yellow-100"),
            ".css" => ("fa-css3", "group-hover:text-indigo-500 group-hover:bg-indigo-100"),
            ".png" or ".jpg" or ".jpeg" or ".svg" => ("fa-file-image-o", "group-hover:text-green-500 group-hover:bg-green-100"),
            _ => ("fa-file-text-o", "group-hover:text-blue-500 group-hover:bg-blue-100")
        };
    }

    private static readonly IReadOnlyDictionary<string, string> MimeMap =
        new Dictionary<string, string> {
            { ".json", "application/json; charset=utf-8" },
            { ".html", "text/html; charset=utf-8" },
            { ".js", "application/javascript; charset=utf-8" },
            { ".css", "text/css; charset=utf-8" },
            { ".xml", "application/xml; charset=utf-8" },
            { ".txt", "text/plain; charset=utf-8" },
            { ".svg", "image/svg+xml" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static string GetContentType(string path)
        => MimeMap.GetValueOrDefault(Path.GetExtension(path), "text/plain; charset=utf-8");

    private static bool IsPathSafe(string basePath, string userInput)
    {
        if (userInput.Any(char.IsControl)) return false;

        string fullBasePath = Path.GetFullPath(basePath);
        if (!fullBasePath.EndsWith(Path.DirectorySeparatorChar))
            fullBasePath += Path.DirectorySeparatorChar;

        string fullPath = Path.GetFullPath(Path.Combine(fullBasePath, userInput));

        return fullPath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"\.delay-(\d+)ms")]
    private static partial Regex DelayMsRegex();
}

[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }