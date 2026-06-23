Add-Type -AssemblyName System.Drawing

function Remove-WhiteBackground {
    param(
        [string]$srcPath,
        [string]$destPath
    )

    if (-not (Test-Path $srcPath)) {
        Write-Warning "Source image not found: $srcPath"
        return
    }

    # Create destination directory if it doesn't exist
    $destDir = [System.IO.Path]::GetDirectoryName($destPath)
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    }

    Write-Host "Loading image from $srcPath..."
    # Load into memory stream to prevent locking issues when saving in-place
    $bytes = [System.IO.File]::ReadAllBytes($srcPath)
    $ms = New-Object System.IO.MemoryStream(,$bytes)
    $bmp = New-Object System.Drawing.Bitmap($ms)
    $newBmp = New-Object System.Drawing.Bitmap($bmp.Width, $bmp.Height)

    Write-Host "Processing pixels (removing white/light-gray background)..."
    for ($x = 0; $x -lt $bmp.Width; $x++) {
        for ($y = 0; $y -lt $bmp.Height; $y++) {
            $pixel = $bmp.GetPixel($x, $y)
            
            # If the pixel is close to white or light gray, make it transparent
            if ($pixel.R -gt 220 -and $pixel.G -gt 220 -and $pixel.B -gt 220) {
                $newBmp.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(0, 0, 0, 0))
            } else {
                $newBmp.SetPixel($x, $y, $pixel)
            }
        }
    }

    Write-Host "Saving transparent image to $destPath..."
    $bmp.Dispose()
    $ms.Dispose()

    # Save as PNG
    $newBmp.Save($destPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $newBmp.Dispose()
    
    Write-Host "Successfully processed background for: $destPath" -ForegroundColor Green
}

# 1. Process main logo icon
Remove-WhiteBackground -srcPath "app-fleet-nexus-net\docs\logo\Fleet_Nexus_Img.png" -destPath "app-fleet-nexus-net\ui\appfleet-nexus-ui\wwwroot\images\logo.png"

# 2. Process logo name text
Remove-WhiteBackground -srcPath "app-fleet-nexus-net\ui\appfleet-nexus-ui\wwwroot\images\logo_name.png" -destPath "app-fleet-nexus-net\ui\appfleet-nexus-ui\wwwroot\images\logo_name.png"
