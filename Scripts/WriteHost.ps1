$content = @'
@page "/"
@namespace SistemIA.Actualizador.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>SistemIA - Actualizador</title>
    <base href="/" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css" rel="stylesheet" />
    <style>
        body { background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%); min-height: 100vh; }
        .update-container { max-width: 800px; margin: 50px auto; }
        .update-card { background: rgba(255,255,255,0.95); border-radius: 15px; box-shadow: 0 10px 40px rgba(0,0,0,0.3); }
        .update-header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 15px 15px 0 0; padding: 25px; }
        .progress { height: 30px; border-radius: 15px; }
        .progress-bar { font-size: 14px; font-weight: bold; }
        .log-container { max-height: 300px; overflow-y: auto; background: #1a1a2e; color: #00ff00; font-family: 'Consolas', monospace; font-size: 12px; padding: 15px; border-radius: 10px; }
        .log-entry { margin: 2px 0; }
        .log-entry.error { color: #ff6b6b; }
        .log-entry.success { color: #51cf66; }
        .log-entry.warning { color: #ffd43b; }
        .status-badge { font-size: 1.1em; padding: 8px 16px; }
    </style>
</head>
<body>
    <component type="typeof(App)" render-mode="ServerPrerendered" />
    <script src="_framework/blazor.server.js"></script>
</body>
</html>
'@

[System.IO.File]::WriteAllText("c:\asis\SistemIA.Actualizador\Pages\_Host.cshtml", $content, [System.Text.Encoding]::UTF8)
Write-Host "_Host.cshtml escrito correctamente"
