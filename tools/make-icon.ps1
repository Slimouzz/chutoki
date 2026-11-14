# Génère l'icône de Chutoki : un ventilateur dont le centre est un bébé qui dort (zZ), au format .ico multi-tailles.
# -Png <fichier> : enregistre aussi une grande version PNG (pour le site / les réseaux).
param([string]$Out = "$PSScriptRoot\..\src\app.ico", [string]$Png = "")
Add-Type -AssemblyName System.Drawing
function C($r, $g, $b, $a = 255) { [System.Drawing.Color]::FromArgb($a, $r, $g, $b) }

function DrawBmp([int]$s) {
    $bmp = New-Object System.Drawing.Bitmap $s, $s
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'; $g.TextRenderingHint = 'AntiAlias'
    $r = New-Object System.Drawing.RectangleF 0, 0, ($s - 1), ($s - 1)
    # fond : nuit bleue
    $br = New-Object System.Drawing.Drawing2D.LinearGradientBrush $r, (C 92 120 255), (C 44 62 170), 60
    $g.FillEllipse($br, $r)
    $c = $s / 2.0
    # pales du ventilateur (blanches, légèrement transparentes)
    $blade = New-Object System.Drawing.SolidBrush (C 255 255 255 235)
    for ($i = 0; $i -lt 4; $i++) {
        $st = $g.Save()
        $g.TranslateTransform($c, $c); $g.RotateTransform(90 * $i + 30)
        $p = New-Object System.Drawing.Drawing2D.GraphicsPath
        $p.AddBezier(0, 0, ($s * 0.10), (-$s * 0.12), ($s * 0.40), (-$s * 0.14), ($s * 0.44), (-$s * 0.02))
        $p.AddBezier(($s * 0.44), (-$s * 0.02), ($s * 0.36), ($s * 0.07), ($s * 0.14), ($s * 0.08), 0, 0)
        $g.FillPath($blade, $p)
        $g.Restore($st)
    }
    # tête du bébé (moyeu)
    $h = $s * 0.46; $hx = $c - $h / 2; $hy = $c - $h / 2
    $g.FillEllipse((New-Object System.Drawing.SolidBrush (C 255 222 196)), $hx, $hy, $h, $h)
    $g.DrawEllipse((New-Object System.Drawing.Pen (C 44 62 170), ([Math]::Max(1, $s * 0.018))), $hx, $hy, $h, $h)
    $ink = C 70 50 60
    $pw = [Math]::Max(1, $s * 0.028)
    $pen = New-Object System.Drawing.Pen $ink, $pw
    $pen.StartCap = 'Round'; $pen.EndCap = 'Round'
    # yeux fermés
    $ew = $h * 0.24; $ey = $c - $h * 0.06
    $g.DrawArc($pen, ($c - $h * 0.20 - $ew / 2), ($ey - $ew * 0.3), $ew, ($ew * 0.6), 20, 140)
    $g.DrawArc($pen, ($c + $h * 0.20 - $ew / 2), ($ey - $ew * 0.3), $ew, ($ew * 0.6), 20, 140)
    if ($s -ge 32) {
        # joues roses
        $pk = New-Object System.Drawing.SolidBrush (C 255 130 150 150)
        $g.FillEllipse($pk, ($c - $h * 0.40), ($c + $h * 0.06), ($h * 0.20), ($h * 0.13))
        $g.FillEllipse($pk, ($c + $h * 0.20), ($c + $h * 0.06), ($h * 0.20), ($h * 0.13))
        # petite bouche
        $mw = $h * 0.20
        $g.DrawArc($pen, ($c - $mw / 2), ($c + $h * 0.08), $mw, ($mw * 0.6), 20, 140)
        # mèche de cheveux
        $g.DrawBezier($pen, ($c - $h * 0.02), ($hy + $h * 0.14), ($c - $h * 0.18), ($hy - $h * 0.06), ($c + $h * 0.18), ($hy - $h * 0.10), ($c + $h * 0.06), ($hy + $h * 0.10))
        # zZ
        $zf = New-Object System.Drawing.Font 'Segoe UI', ([float]($s * 0.17)), ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
        $zb = New-Object System.Drawing.SolidBrush (C 255 236 140)
        $g.DrawString('z', $zf, $zb, ($s * 0.66), ($s * 0.10))
        $zf2 = New-Object System.Drawing.Font 'Segoe UI', ([float]($s * 0.23)), ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
        $g.DrawString('Z', $zf2, $zb, ($s * 0.74), ($s * -0.03))
    }
    $g.Dispose()
    return $bmp
}
function PngBytes([int]$s) {
    $bmp = DrawBmp $s
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
    return , $ms.ToArray()
}

$sizes = 16, 24, 32, 48, 64, 128, 256
$imgs = $sizes | % { , (PngBytes $_) }
$fs = [System.IO.File]::Create((Resolve-Path -LiteralPath (Split-Path $Out) | % Path) + '\' + (Split-Path $Out -Leaf))
$w = New-Object System.IO.BinaryWriter $fs
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $sz = $sizes[$i]; $len = $imgs[$i].Length
    $w.Write([byte]($(if ($sz -ge 256) { 0 } else { $sz }))); $w.Write([byte]($(if ($sz -ge 256) { 0 } else { $sz })))
    $w.Write([byte]0); $w.Write([byte]0); $w.Write([UInt16]1); $w.Write([UInt16]32)
    $w.Write([UInt32]$len); $w.Write([UInt32]$offset); $offset += $len
}
foreach ($img in $imgs) { $w.Write($img) }
$w.Close()
"Icone : $Out"
if ($Png) { $b = DrawBmp 512; $b.Save($Png, [System.Drawing.Imaging.ImageFormat]::Png); $b.Dispose(); "Logo PNG : $Png" }
