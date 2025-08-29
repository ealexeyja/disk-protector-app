#!/bin/bash
# Versión simplificada para releases rápidos

if [ -z "$1" ]; then
    echo "Usage: ./tag-release.sh <version>"
    echo "Example: ./tag-release.sh 2.1.0"
    exit 1
fi

VERSION="$1"
TAG_NAME="v$VERSION"

# Validar versión
if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9]+)?$ ]]; then
    echo "Error: Formato de versión inválido. Use: MAJOR.MINOR.PATCH"
    exit 1
fi

# Crear y subir tag
git tag -a "$TAG_NAME" -m "Release version $VERSION"
git push origin "$TAG_NAME"

echo "✅ Tag $TAG_NAME creado y subido"
echo "🚀 GitHub Actions creará el release automáticamente"
