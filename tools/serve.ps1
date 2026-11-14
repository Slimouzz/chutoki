# Petit serveur local pour previsualiser le site (http://localhost:8765)
param([int]$Port = 8765, [string]$Root = "$PSScriptRoot\..\docs")
$Root = (Resolve-Path $Root).Path
$types = @{ '.html' = 'text/html; charset=utf-8'; '.png' = 'image/png'; '.css' = 'text/css'; '.js' = 'text/javascript'; '.json' = 'application/json'; '.ico' = 'image/x-icon'; '.svg' = 'image/svg+xml' }
$l = New-Object System.Net.HttpListener
$l.Prefixes.Add("http://localhost:$Port/")
$l.Start()
"Site : http://localhost:$Port/"
while ($l.IsListening) {
    $ctx = $l.GetContext()
    $p = [Uri]::UnescapeDataString($ctx.Request.Url.AbsolutePath.TrimStart('/'))
    if ($p -eq '') { $p = 'index.html' }
    $f = [IO.Path]::GetFullPath((Join-Path $Root $p))
    if ($f.StartsWith($Root) -and (Test-Path $f -PathType Leaf)) {
        $b = [IO.File]::ReadAllBytes($f)
        $t = $types[[IO.Path]::GetExtension($f).ToLower()]; if ($t) { $ctx.Response.ContentType = $t }
        $ctx.Response.OutputStream.Write($b, 0, $b.Length)
    } else { $ctx.Response.StatusCode = 404 }
    $ctx.Response.Close()
}
