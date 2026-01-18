namespace BoomApi;

public static class Assets
{
    public const string FaviconBase64 = "data:image/x-icon;base64,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";

    public const string HelloJson = """
        {
          "title": "Welcome to BoomApi / 欢迎使用 BoomApi",
          "stats": {
            "size": "16.53 MB",
            "speed": "< 10ms",
            "engine": ".NET 10 Native AOT"
          },
          "features": [
            "Ultra-lightweight / 极致轻量",
            "File-system routing / 文件即路由",
            "Zero runtime / 零运行时依赖"
          ],
          "github": "https://github.com/vicenteyu/boomapi",
          "tip": "You can edit this in /data/hello-world.json / 你可以在 /data 目录中直接编辑此文件"
        }
        """;

    public const string WelcomeHtml = """
        <!DOCTYPE html>
        <html lang="zh-CN">
        <head>
            <meta charset="UTF-8">
            <title>BoomApi Welcome</title>
            <style>
                body { font-family: sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 40px auto; padding: 20px; }
                .card { border: 1px solid #eee; border-radius: 12px; padding: 20px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }
                h1 { color: #2563eb; font-size: 1.5rem; }
                code { background: #f1f5f9; padding: 2px 4px; border-radius: 4px; font-family: monospace; }
            </style>
        </head>
        <body>
            <div class="card">
                <h1>🚀 BoomApi is Running!</h1>
                <p><strong>EN:</strong> This is a raw HTML response served directly from the <code>/data</code> directory.</p>
                <p><strong>ZH:</strong> 这是一个直接从 <code>/data</code> 目录读取并返回的 HTML 原始响应。</p>
                <hr>
                <p>Edit this file: <code>/data/welcome.html</code></p>
                <a href="/">Back to Hub / 返回控制台</a>
            </div>
        </body>
        </html>
        """;

    public const string IndexHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <title>BoomApi Explorer</title>
            <link rel="icon" href="{FAVICON}">
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
                            <p class="text-xs text-slate-400 font-mono">14MB Miracle / .NET 10</p>
                        </div>
                    </div>
                    <a href="/create" class="bg-blue-600 hover:bg-blue-500 text-sm font-semibold px-5 py-2.5 rounded-xl transition-all flex items-center shadow-md">
                        <i class="fa fa-plus mr-2"></i> New Endpoint
                    </a>
                </div>

                <div class="px-6 py-3 bg-slate-50 border-b border-gray-100">
                    <span class="text-xs font-medium text-slate-500 uppercase tracking-wider">
                        {LIST_COUNT} active endpoints found
                    </span>
                </div>

                <div class="p-6">
                    {LIST_CONTENT}
                </div>

                <div class="px-6 py-4 bg-gray-50 text-center border-t border-gray-100 flex items-center justify-center space-x-4">
                    <span class="text-xs text-gray-400">© {CURRENT_YEAR} BoomApi | Powered by .NET 10 Native AOT</span>
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
                    if (confirm('Are you sure you want to delete this endpoint?')) {
                        fetch(`/delete/${filePath}`, { method: 'DELETE' })
                            .then(async res => {
                                if (res.ok) {
                                    location.reload();
                                } else {
                                    const errorData = await res.json().catch(() => ({ detail: "Unknown error" }));
                                    const msg = errorData.detail || `Error: ${res.status}`;
                                    alert(`Delete failed: ${msg}`);
                                    console.error('Server error:', errorData);
                                }
                            })
                            .catch(error => {
                                alert('Network error or server is down.');
                                console.error('Fetch crash:', error);
                            });
                    }
                }
            </script>
        </body>
        </html>
        """;

    public const string ListTemplate = """
        <div class='group flex items-center justify-between p-4 mb-4 border border-gray-100 rounded-xl hover:border-blue-200 hover:bg-blue-50/30 transition-all'>
            <div class='flex-1 min-w-0 mr-6'>
                <div class='flex items-center space-x-4'>
                    <div class="w-12 h-12 rounded-lg bg-gray-100 flex items-center justify-center text-gray-400 transition-colors {COLOR_STYLE}">
                        <i class='fa {ICON_CLASS} text-lg'></i>
                    </div>
                    <div class='min-w-0'>
                        <div class='text-slate-900 font-semibold truncate text-sm md:text-base'>{URL}</div>
                        <div class='flex items-center mt-1 space-x-3 text-xs text-slate-400'>
                            <span><i class='fa fa-hdd-o mr-1'></i>{LENGTH}</span>
                            <span><i class='fa fa-clock-o mr-1'></i>{TIME}</span>
                            {DELAY}
                        </div>
                    </div>
                </div>
            </div>
            <div class='flex items-center space-x-2'>
                <button onclick='viewFile(`{PATH}`)' class='p-2.5 text-slate-600 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-all'><i class='fa fa-external-link text-lg'></i></button>
                <button onclick='deleteFile(`{PATH}`)' class='p-2.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-all'><i class='fa fa-trash-o text-lg'></i></button>
            </div>
        </div>
        """;

    public const string CreateHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8">
            <title>Create Mock - BoomApi</title>
            <link rel="icon" href="{FAVICON}">
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
                        <div class="space-y-2">
                            <label class="block text-sm font-semibold text-slate-700">Endpoint Path</label>
                            <input type="text" name="path" required class="w-full px-4 py-3 rounded-xl border border-gray-200 focus:ring-2 focus:ring-blue-500 focus:outline-none transition-all" placeholder="e.g. user-profile.json">
                            <p class="mt-1 text-xs text-slate-400">Tip: Use folders like <code>v1/users.json</code> to organize.</p>
                        </div>
                        <div class="space-y-2">
                            <label class="block text-sm font-semibold text-slate-700">Raw Content (Payload)</label>
                            <textarea spellcheck="false" name="raw" rows="12" class="w-full p-4 rounded-xl border border-gray-200 focus:ring-2 focus:ring-blue-500 focus:outline-none font-mono text-sm bg-gray-50" placeholder='{ "status": "success" }'></textarea>
                        </div>
                        <div class="flex justify-end space-x-4 pt-6">
                            <a href="/" class="px-6 py-2.5 rounded-xl text-slate-500 font-medium hover:bg-gray-100 transition-all">Discard</a>
                            <button type="submit" class="px-8 py-2.5 bg-blue-600 text-white font-bold rounded-xl hover:bg-blue-500 shadow-lg shadow-blue-500/20 transition-all">Deploy Mock</button>
                        </div>
                    </form>
                </div>
                <div class="px-6 py-4 bg-gray-50 text-center border-t border-gray-100 flex items-center justify-center space-x-4">
                    <span class="text-xs text-gray-400">© {CURRENT_YEAR} BoomApi | Powered by .NET 10 Native AOT</span>
                    <a href="https://github.com/vicenteyu/boomapi" target="_blank" class="text-gray-400 hover:text-slate-900 transition-colors">
                        <i class="fa fa-github text-lg"></i>
                    </a>
                </div>
            </div>
        </body>
        </html>
        """;

    public const string EmptyState = "<div class='text-center py-20 text-gray-400'>No endpoints found.</div>";
}
