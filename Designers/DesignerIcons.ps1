# Draws the IsWiX designer icons as tinted tiles: a soft rounded square in the designer's hue with a line glyph in a
# strong shade of it. Each designer gets <Name>.ico (for light backgrounds, and the standalone app) and <Name>.dark.ico
# (for Visual Studio's dark themes), each with 16, 20, 24, 32 and 48 pixel images, 32-bit with alpha; tree icons the
# designers use are written as 16 pixel PNGs the same way (see $treeIcons), and menu icons as the glyphs alone (see
# $menuIcons). Glyphs are drawn
# on a 24-unit grid; 16 and 20 pixels use simpler glyphs where the full ones are too busy.
#
# Run from Windows PowerShell 5.1 (it uses WPF):  powershell -STA -File Designers\DesignerIcons.ps1 [-Preview sheet.png]
param([string]$Preview)
$Repo = Split-Path $PSScriptRoot
Add-Type -AssemblyName PresentationCore, PresentationFramework, WindowsBase, System.Drawing

# Hue stops: tile light, glyph light, tile dark, glyph dark.
$hues = @{
    blue   = '#E6F1FB', '#185FA5', '#0C447C', '#B5D4F4'
    purple = '#EEEDFE', '#534AB7', '#3C3489', '#CECBF6'
    teal   = '#E1F5EE', '#0F6E56', '#085041', '#9FE1CB'
    amber  = '#FAEEDA', '#854F0B', '#633806', '#FAC775'
    pink   = '#FBEAF0', '#993556', '#72243E', '#F4C0D1'
    gray   = '#F1EFE8', '#5F5E5A', '#444441', '#D3D1C7'
    coral  = '#FAECE7', '#993C1D', '#712B13', '#F5C4B3'
    green  = '#EAF3DE', '#3B6D11', '#27500A', '#C0DD97'
    red    = '#FCEBEB', '#A32D2D', '#791F1F', '#F7C1C1'
}

# Gear: a ring of eight teeth around a hub, as one path.
$gear = ''
for ($k = 0; $k -lt 8; $k++) {
    $a = $k * [Math]::PI / 4
    $x1 = 12 + 6.5 * [Math]::Cos($a); $y1 = 12 + 6.5 * [Math]::Sin($a)
    $x2 = 12 + 9.2 * [Math]::Cos($a); $y2 = 12 + 9.2 * [Math]::Sin($a)
    $gear += ' M{0:0.##},{1:0.##} L{2:0.##},{3:0.##}' -f $x1, $y1, $x2, $y2
}
$inv = [Globalization.CultureInfo]::InvariantCulture
$gear = $gear.Replace(',', ' ').Trim()

# The small sizes' gear: six heavier teeth, which stay distinct at 16 px.
$smallGear = ''
for ($k = 0; $k -lt 6; $k++) {
    $a = $k * [Math]::PI / 3 + [Math]::PI / 6
    $smallGear += ' M{0:0.##},{1:0.##} L{2:0.##},{3:0.##}' -f (12 + 5.2 * [Math]::Cos($a)), (12 + 5.2 * [Math]::Sin($a)), (12 + 9 * [Math]::Cos($a)), (12 + 9 * [Math]::Sin($a))
}
$smallGear = $smallGear.Replace(',', ' ').Trim()

$designers = @(
    @{ Dir = 'GeneralInformationDesigner'; Name = 'GeneralInformation'; Hue = 'blue';   Paths = @('M12 3 A9 9 0 1 1 11.99 3', 'M12 8 L12.01 8', 'M11 12 L12 12 L12 16 L13 16'); Small = @('M12 6.5 L12 6.51', 'M12 10.5 L12 17.5') }
    @{ Dir = 'FeaturesDesigner';           Name = 'Features';           Hue = 'purple'; Paths = @('M12 4 L4 8 L12 12 L20 8 Z', 'M4 12 L12 16 L20 12', 'M4 16 L12 20 L20 16'); Small = @('M12 5 L4 9 L12 13 L20 9 Z', 'M4 14 L12 18 L20 14') }
    @{ Dir = 'PropertiesDesigner';         Name = 'Properties';         Hue = 'teal';   Paths = @('M4 6 L12 6', 'M16 6 L20 6', 'M14 4 A2 2 0 1 1 13.99 4', 'M4 12 L6 12', 'M10 12 L20 12', 'M8 10 A2 2 0 1 1 7.99 10', 'M4 18 L15 18', 'M19 18 L20 18', 'M17 16 A2 2 0 1 1 16.99 16') }
    @{ Dir = 'FilesAndFoldersDesigner';    Name = 'FilesAndFolders';    Hue = 'amber';  Paths = @('M5 4 L9 4 L12 7 L19 7 A2 2 0 0 1 21 9 L21 17 A2 2 0 0 1 19 19 L5 19 A2 2 0 0 1 3 17 L3 6 A2 2 0 0 1 5 4 Z') }
    @{ Dir = 'NamespacesDesigner';         Name = 'Namespaces';         Hue = 'pink';   Paths = @('M7 8 L3 12 L7 16', 'M17 8 L21 12 L17 16', 'M14 4 L10 20') }
    @{ Dir = 'ServicesDesigner';           Name = 'Services';           Hue = 'gray';   Paths = @('M12 5.5 A6.5 6.5 0 1 1 11.99 5.5', 'M12 9.5 A2.5 2.5 0 1 1 11.99 9.5', $gear); Small = @('M12 6.8 A5.2 5.2 0 1 1 11.99 6.8', 'M12 10.6 A1.4 1.4 0 1 1 11.99 10.6', $smallGear) }
    @{ Dir = 'ShortCutsDesigner';          Name = 'ShortCuts';          Hue = 'coral';  Paths = @('M12 6 L6 6 A2 2 0 0 0 4 8 L4 18 A2 2 0 0 0 6 20 L16 20 A2 2 0 0 0 18 18 L18 12', 'M11 13 L20 4', 'M15 4 L20 4 L20 9') }
    @{ Dir = 'CustomTablesDesigner';       Name = 'CustomTables';       Hue = 'green';  Paths = @('M5 3 L19 3 A2 2 0 0 1 21 5 L21 19 A2 2 0 0 1 19 21 L5 21 A2 2 0 0 1 3 19 L3 5 A2 2 0 0 1 5 3 Z', 'M3 10 L21 10', 'M10 3 L10 21') }
)
$sizes = 16, 20, 24, 32, 48

function Brush([string]$hex) { $b = New-Object System.Windows.Media.SolidColorBrush ([System.Windows.Media.ColorConverter]::ConvertFromString($hex)); $b.Freeze(); $b }

# One tile at one size, as straight-alpha BGRA rows, top-down.
function Render($d, [int]$size, [bool]$dark) {
    $h = $hues[$d.Hue]
    $tile = Brush $(if ($dark) { $h[2] } else { $h[0] })
    $ink = Brush $(if ($dark) { $h[3] } else { $h[1] })
    $visual = New-Object System.Windows.Media.DrawingVisual
    $dc = $visual.RenderOpen()
    $radius = [Math]::Max(2.5, $size * 0.22)
    # Menu glyphs ($d.NoTile) are drawn alone, larger, as Visual Studio's menu icons are.
    if (-not $d.NoTile) { $dc.DrawRoundedRectangle($tile, $null, (New-Object System.Windows.Rect 0, 0, $size, $size), $radius, $radius) }
    # The glyph fills about 70% of the tile; thinner lines at larger sizes keep it from looking heavy.
    $small = $size -le 20
    $scale = $size * $(if ($d.NoTile) { 0.92 } elseif ($small) { 0.76 } else { 0.70 }) / 24
    $offset = ($size - 24 * $scale) / 2
    $stroke = if ($size -le 20) { 2.5 } elseif ($size -le 24) { 2.3 } else { 2.0 }
    $pen = New-Object System.Windows.Media.Pen $ink, $stroke
    $pen.StartLineCap = 'Round'; $pen.EndLineCap = 'Round'; $pen.LineJoin = 'Round'
    $dc.PushTransform((New-Object System.Windows.Media.MatrixTransform ([System.Windows.Media.Matrix]::new($scale, 0, 0, $scale, $offset, $offset))))
    $paths = if ($small -and $d.Small) { $d.Small } else { $d.Paths }
    foreach ($p in $paths) { $dc.DrawGeometry($null, $pen, [System.Windows.Media.Geometry]::Parse($p)) }
    $dc.Pop()
    $dc.Close()
    $bitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap $size, $size, 96, 96, ([System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $pixels = New-Object byte[] ($size * $size * 4)
    $bitmap.CopyPixels($pixels, $size * 4, 0)
    for ($i = 0; $i -lt $pixels.Length; $i += 4) {
        $a = $pixels[$i + 3]
        if ($a -gt 0 -and $a -lt 255) { for ($c = 0; $c -lt 3; $c++) { $pixels[$i + $c] = [byte][Math]::Min(255, [Math]::Round($pixels[$i + $c] * 255.0 / $a)) } }
    }
    return ,$pixels
}

function Dib([byte[]]$pixels, [int]$size) {
    $ms = New-Object IO.MemoryStream; $bw = New-Object IO.BinaryWriter($ms)
    $bw.Write([uint32]40); $bw.Write([int32]$size); $bw.Write([int32]($size * 2)); $bw.Write([uint16]1); $bw.Write([uint16]32)
    $bw.Write([uint32]0); $bw.Write([uint32]0); $bw.Write([int32]0); $bw.Write([int32]0); $bw.Write([uint32]0); $bw.Write([uint32]0)
    for ($y = $size - 1; $y -ge 0; $y--) { $bw.Write($pixels, $y * $size * 4, $size * 4) }
    $bw.Write((New-Object byte[] ([int]([Math]::Ceiling($size / 32.0) * 4) * $size)))
    $bw.Flush()
    return ,$ms.ToArray()
}

function Write-Icon($d, [bool]$dark, [string]$path) {
    $images = @(foreach ($s in $sizes) { [pscustomobject]@{ Size = $s; Data = [byte[]](Dib (Render $d $s $dark) $s) } })
    $ms = New-Object IO.MemoryStream; $bw = New-Object IO.BinaryWriter($ms)
    $bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]$images.Count)
    $offset = 6 + 16 * $images.Count
    foreach ($i in $images) {
        $bw.Write([byte]$i.Size); $bw.Write([byte]$i.Size); $bw.Write([byte]0); $bw.Write([byte]0)
        $bw.Write([uint16]1); $bw.Write([uint16]32); $bw.Write([uint32]$i.Data.Length); $bw.Write([uint32]$offset)
        $offset += $i.Data.Length
    }
    foreach ($i in $images) { $bw.Write([byte[]]$i.Data) }
    $bw.Flush()
    [IO.File]::WriteAllBytes($path, $ms.ToArray())
}

foreach ($d in $designers) {
    $dir = Join-Path $Repo "Designers\$($d.Dir)"
    Write-Icon $d $false (Join-Path $dir "$($d.Name).ico")
    Write-Icon $d $true (Join-Path $dir "$($d.Name).dark.ico")
    "{0}: {1}.ico and {1}.dark.ico" -f $d.Dir, $d.Name
}

# Tree icons, the same tiles at 16 px as PNGs: <Name>.png and <Name>.dark.png, which DesignerImages.Add loads together.
$computer = @('M4.5 4 L19.5 4 A1.5 1.5 0 0 1 21 5.5 L21 14.5 A1.5 1.5 0 0 1 19.5 16 L4.5 16 A1.5 1.5 0 0 1 3 14.5 L3 5.5 A1.5 1.5 0 0 1 4.5 4 Z', 'M12 16 L12 20', 'M8 20 L16 20')
$folderClosed = $designers[3].Paths
# Open: the folder's back, and its front tipped forward.
$folderOpen = @('M3 17 L3 6 A2 2 0 0 1 5 4 L9 4 L12 7 L17 7 A2 2 0 0 1 19 9 L19 10', 'M3 17 L5.6 11.2 A2 2 0 0 1 7.4 10 L20.5 10 A1 1 0 0 1 21.4 11.4 L18.6 17.8 A2 2 0 0 1 16.8 19 L5 19 A2 2 0 0 1 3 17 Z')
$treeIcons = @(
    @{ Dir = 'FeaturesDesigner\Images'; Name = 'feature';  Hue = 'purple'; Paths = $designers[1].Small }
    @{ Dir = 'FeaturesDesigner\Images'; Name = 'database'; Hue = 'blue';   Paths = @('M5 6.5 A7 2.5 0 1 0 19 6.5 A7 2.5 0 1 0 5 6.5', 'M5 6.5 L5 17.5 A7 2.5 0 0 0 19 17.5 L19 6.5', 'M5 12 A7 2.5 0 0 0 19 12') }
    # The destination trees the Files and Folders, Services and Shortcuts designers share: the computer, the special
    # folders (blue) and the folders below them (amber, as the Files and Folders icon).
    @{ Dir = '..\Shared\FireworksFramework\Images\Designers'; Name = 'computer';           Hue = 'gray';  Paths = $computer }
    @{ Dir = '..\Shared\FireworksFramework\Images\Designers'; Name = 'folder_closed';      Hue = 'amber'; Paths = $folderClosed }
    @{ Dir = '..\Shared\FireworksFramework\Images\Designers'; Name = 'folder_open';        Hue = 'amber'; Paths = $folderOpen }
    @{ Dir = '..\Shared\FireworksFramework\Images\Designers'; Name = 'blue_folder_closed'; Hue = 'blue';  Paths = $folderClosed }
    @{ Dir = '..\Shared\FireworksFramework\Images\Designers'; Name = 'blue_folder_open';   Hue = 'blue';  Paths = $folderOpen }
    # A service, as the Services designer's icon.
    @{ Dir = '..\Shared\FireworksFramework\Images\Designers'; Name = 'service';            Hue = 'gray';  Paths = $designers[5].Paths; Small = $designers[5].Small }
)

function Write-Png([byte[]]$pixels, [int]$size, [string]$path) {
    $bitmap = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $data = $bitmap.LockBits((New-Object System.Drawing.Rectangle 0, 0, $size, $size), 'WriteOnly', $bitmap.PixelFormat)
    for ($y = 0; $y -lt $size; $y++) { [Runtime.InteropServices.Marshal]::Copy($pixels, $y * $size * 4, [IntPtr]($data.Scan0.ToInt64() + $y * $data.Stride), $size * 4) }
    $bitmap.UnlockBits($data)
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

# Menu icons the WinForms designers share (DesignerImages.MenuImage), the glyphs alone in their hues.
$menuIcons = @(
    @{ Dir = '..\Shared\FireworksFramework\Images\Menus'; Name = 'new_folder';   Hue = 'amber'; NoTile = $true; Paths = @('M3 7 A2 2 0 0 1 5 5 L9 5 L11 7 L19 7 A2 2 0 0 1 21 9 L21 17 A2 2 0 0 1 19 19 L5 19 A2 2 0 0 1 3 17 Z', 'M12 10 L12 16', 'M9 13 L15 13') }
    @{ Dir = '..\Shared\FireworksFramework\Images\Menus'; Name = 'rename';       Hue = 'blue';  NoTile = $true; Paths = @('M4 20 L4.5 16 L15.5 5 L19 8.5 L8 19.5 Z', 'M13.5 7 L17 10.5') }
    @{ Dir = '..\Shared\FireworksFramework\Images\Menus'; Name = 'delete';       Hue = 'red';   NoTile = $true; Paths = @('M6 6 L18 18', 'M18 6 L6 18') }
    @{ Dir = '..\Shared\FireworksFramework\Images\Menus'; Name = 'expand_all';   Hue = 'gray';  NoTile = $true; Paths = @('M6 5 L12 11 L18 5', 'M6 13 L12 19 L18 13') }
    @{ Dir = '..\Shared\FireworksFramework\Images\Menus'; Name = 'collapse_all'; Hue = 'gray';  NoTile = $true; Paths = @('M6 11 L12 5 L18 11', 'M6 19 L12 13 L18 19') }
    @{ Dir = '..\Shared\FireworksFramework\Images\Menus'; Name = 'refresh';      Hue = 'teal';  NoTile = $true; Paths = @('M19 12 A7 7 0 1 1 15.5 5.94', 'M14.6 2.6 L15.7 6.1 L12.2 7.1') }
    @{ Dir = '..\Shared\FireworksFramework\Images\Menus'; Name = 'new_shortcut'; Hue = 'coral'; NoTile = $true; Paths = $designers[6].Paths }
)

foreach ($t in $treeIcons + $menuIcons) {
    $dir = Join-Path $Repo "Designers\$($t.Dir)"
    Write-Png (Render $t 16 $false) 16 (Join-Path $dir "$($t.Name).png")
    Write-Png (Render $t 16 $true) 16 (Join-Path $dir "$($t.Name).dark.png")
    "{0}: {1}.png and {1}.dark.png" -f $t.Dir, $t.Name
}

if (-not $Preview) { return }

# Preview: every icon at 16, 24 and 48 px on Visual Studio's light and dark backgrounds, read back from the .ico files.
$sheet = New-Object System.Drawing.Bitmap (8 * 112 + 16), 220
$g = [System.Drawing.Graphics]::FromImage($sheet)
$g.Clear([System.Drawing.Color]::FromArgb(245, 245, 245))
$g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(31, 31, 31))), 0, 110, $sheet.Width, 110)
$x = 16
foreach ($d in $designers) {
    $dir = Join-Path $Repo "Designers\$($d.Dir)"
    foreach ($variant in @(@("$($d.Name).ico", 14), @("$($d.Name).dark.ico", 124))) {
        $y = $variant[1]; $px = $x
        foreach ($s in 16, 24, 48) {
            $icon = New-Object System.Drawing.Icon((Join-Path $dir $variant[0]), $s, $s)
            $g.DrawImage($icon.ToBitmap(), $px, $y + (48 - $s) / 2, $s, $s)
            $px += $s + 8
        }
    }
    $x += 112
}
$g.Dispose()
$sheet.Save($Preview)
