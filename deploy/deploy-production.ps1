# ========================================
# Script de Despliegue Rápido - QuizCraft
# ========================================

param(
    [switch]$SkipBuild,
    # Nota: por defecto NO se aplicarán migraciones desde el script.
    # La aplicación ya ejecuta las migraciones al iniciarse (ver Program.cs -> context.Database.MigrateAsync()).
    # Usa --ApplyMigrations para forzar la ejecución de `dotnet ef database update` antes del despliegue.
    [switch]$ApplyMigrations,
    [switch]$SkipRestart,
    [switch]$Verbose
)

# Configuración
$ResourceGroup = "IAAplicadaGrupo2"
$WebAppName = "quizcraft-webapp"
$ProjectPath = "C:\QuizCraft\src\QuizCraft.Web"
$PublishPath = Join-Path $ProjectPath "publish"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "   DESPLIEGUE A PRODUCCIÓN - QUIZCRAFT" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Azure CLI
Write-Host "🔍 Verificando Azure CLI..." -ForegroundColor Yellow
try {
    $azVersion = az --version 2>&1 | Select-Object -First 1
    Write-Host "✅ Azure CLI disponible: $azVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI no encontrado. Por favor, instálalo primero." -ForegroundColor Red
    exit 1
}

# Verificar sesión de Azure
Write-Host "🔍 Verificando sesión de Azure..." -ForegroundColor Yellow
$account = az account show 2>&1 | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ No has iniciado sesión en Azure. Ejecuta: az login" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Sesión activa: $($account.user.name)" -ForegroundColor Green
Write-Host ""

# Paso 1: Limpiar publicaciones anteriores
Write-Host "📁 Limpiando publicaciones anteriores..." -ForegroundColor Yellow
if (Test-Path $PublishPath) {
    Remove-Item -Recurse -Force $PublishPath
    Write-Host "✅ Carpeta publish eliminada" -ForegroundColor Green
}
Write-Host ""

# Paso 2: Compilar
if (-not $SkipBuild) {
    Write-Host "🔨 Compilando aplicación..." -ForegroundColor Yellow
    Set-Location $ProjectPath
    
    dotnet build -c Release --nologo
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Error durante la compilación" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Compilación exitosa" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "⏭️  Compilación omitida (--SkipBuild)" -ForegroundColor Gray
    Write-Host ""
}

# Paso 3: Publicar
Write-Host "📦 Publicando aplicación..." -ForegroundColor Yellow
Set-Location $ProjectPath

dotnet publish -c Release -o $PublishPath --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error durante la publicación" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Publicación exitosa en: $PublishPath" -ForegroundColor Green
Write-Host ""

# Paso 4: Aplicar migraciones (OPCIONAL)
# Por diseño la aplicación ejecuta las migraciones automáticamente en el arranque
# gracias a: await context.Database.MigrateAsync(); en Program.cs.
# Para entornos controlados o cuando necesites forzar la migración antes de arrancar
# puedes usar --ApplyMigrations al invocar este script.
if ($ApplyMigrations) {
    Write-Host "🗄️  Aplicando migraciones de base de datos (solicitado con --ApplyMigrations)..." -ForegroundColor Yellow
    Set-Location $ProjectPath

    if ($Verbose) {
        dotnet ef database update --configuration Production --no-build --verbose
    } else {
        dotnet ef database update --configuration Production --no-build
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️  Advertencia: Error al aplicar migraciones" -ForegroundColor Yellow
        $continue = Read-Host "¿Deseas continuar con el despliegue? (s/n)"
        if ($continue -ne 's') {
            Write-Host "❌ Despliegue cancelado" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "✅ Migraciones aplicadas correctamente" -ForegroundColor Green
    }
    Write-Host ""
} else {
    Write-Host "⏭️  Migraciones omitidas por defecto. La aplicación aplicará migraciones al iniciarse (Program.cs)." -ForegroundColor Gray
    Write-Host "Si necesitas forzar migraciones antes del despliegue usa: .\deploy-production.ps1 --ApplyMigrations" -ForegroundColor Gray
    Write-Host ""
}

# Paso 5: Desplegar a Azure
Write-Host "☁️  Desplegando a Azure Web App..." -ForegroundColor Yellow
Write-Host "   Resource Group: $ResourceGroup" -ForegroundColor Gray
Write-Host "   Web App: $WebAppName" -ForegroundColor Gray
Write-Host ""

# Comprimir la carpeta publish en un archivo ZIP
$ZipPath = Join-Path $ProjectPath "publish.zip"
Write-Host "📦 Comprimiendo archivos..." -ForegroundColor Yellow
Compress-Archive -Path "$PublishPath\*" -DestinationPath $ZipPath -Force

if (-not (Test-Path $ZipPath)) {
    Write-Host "❌ Error al crear el archivo ZIP" -ForegroundColor Red
    exit 1
}

# Desplegar el archivo ZIP
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --src-path $ZipPath `
    --type zip `
    --async false

# Limpiar archivo ZIP temporal
Remove-Item $ZipPath -Force -ErrorAction SilentlyContinue

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error durante el despliegue" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Despliegue completado" -ForegroundColor Green
Write-Host ""

# Paso 6: Reiniciar Web App
if (-not $SkipRestart) {
    Write-Host "🔄 Reiniciando Web App..." -ForegroundColor Yellow
    
    az webapp restart `
        --resource-group $ResourceGroup `
        --name $WebAppName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️  Advertencia: Error al reiniciar" -ForegroundColor Yellow
    } else {
        Write-Host "✅ Web App reiniciada" -ForegroundColor Green
    }
    
    Write-Host "⏳ Esperando 30 segundos para que la aplicación inicie..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    Write-Host ""
} else {
    Write-Host "⏭️  Reinicio omitido (--SkipRestart)" -ForegroundColor Gray
    Write-Host ""
}

# Paso 7: Verificar despliegue
Write-Host "🔍 Verificando despliegue..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "https://$WebAppName.azurewebsites.net" -UseBasicParsing -TimeoutSec 30
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ La aplicación responde correctamente (HTTP 200)" -ForegroundColor Green
    } else {
        Write-Host "⚠️  La aplicación responde con código: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  No se pudo verificar la aplicación: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "   Esto puede ser normal si la aplicación aún está iniciando." -ForegroundColor Gray
}
Write-Host ""

# Resumen final
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "   ✅ DESPLIEGUE COMPLETADO" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "🌐 URL de la aplicación:" -ForegroundColor Cyan
Write-Host "   https://$WebAppName.azurewebsites.net" -ForegroundColor White
Write-Host ""
Write-Host "📊 Comandos útiles:" -ForegroundColor Cyan
Write-Host "   Ver logs:    az webapp log tail --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor Gray
Write-Host "   Ver estado:  az webapp show --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor Gray
Write-Host "   Reiniciar:   az webapp restart --resource-group $ResourceGroup --name $WebAppName" -ForegroundColor Gray
Write-Host ""
Write-Host "🎉 ¡Despliegue exitoso!" -ForegroundColor Green
Write-Host ""

# Preguntar si desea abrir la aplicación
$open = Read-Host "¿Deseas abrir la aplicación en el navegador? (s/n)"
if ($open -eq 's') {
    Start-Process "https://$WebAppName.azurewebsites.net"
}
