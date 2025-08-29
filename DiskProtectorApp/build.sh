#!/bin/bash
echo "🔧 Compilando Disk Protector App..."

# Configurar variables de entorno
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export MSBUILDDISABLENODEREUSE=1

# Compilar con todas las propiedades necesarias
dotnet publish -c Release -r win-x64 \
  /p:EnableWindowsTargeting=true \
  /p:UseWindowsForms=true \
  /p:TargetFramework=net8.0-windows \
  /p:RuntimeIdentifier=win-x64 \
  /p:SelfContained=false \
  /p:PublishSingleFile=true

# Verificar resultados
if [ -f "bin/Release/net8.0-windows/win-x64/publish/DiskProtectorApp.exe" ]; then
    echo "✅ ¡Compilación exitosa!"
    echo "📦 Ejecutable: bin/Release/net8.0-windows/win-x64/publish/DiskProtectorApp.exe"
else
    echo "❌ Error en la compilación"
    echo "Buscando archivos:"
    find bin/ -name "*" -type f | head -10
fi
