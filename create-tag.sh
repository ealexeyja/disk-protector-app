#!/bin/bash

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para mostrar mensajes de error
show_error() {
    echo -e "${RED}❌ Error: $1${NC}"
}

# Función para mostrar mensajes de éxito
show_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

# Función para mostrar información
show_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# Función para mostrar advertencias
show_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Verificar si se proporcionó una versión
if [ -z "$1" ]; then
    echo "Usage: ./create-tag.sh <version> [mensaje]"
    echo "Example: ./create-tag.sh 2.1.0 \"Nuevas características de seguridad\""
    echo ""
    echo "Opciones:"
    echo "  ./create-tag.sh --list      Listar tags existentes"
    echo "  ./create-tag.sh --latest    Mostrar el último tag"
    echo "  ./create-tag.sh --delete <tag> Eliminar un tag (local y remoto)"
    exit 1
fi

# Manejar opciones especiales
case "$1" in
    "--list")
        echo "📋 Tags existentes:"
        git tag -l --sort=-v:refname | head -10
        echo ""
        echo "Tags remotos:"
        git ls-remote --tags origin | awk '{print $2}' | cut -d'/' -f3 | sort -V | tail -10
        exit 0
        ;;
    "--latest")
        LATEST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "No hay tags")
        echo "Último tag: $LATEST_TAG"
        exit 0
        ;;
    "--delete")
        if [ -z "$2" ]; then
            show_error "Debes especificar el tag a eliminar"
            exit 1
        fi
        show_warning "Eliminando tag $2..."
        git tag -d "$2"
        git push origin --delete "$2"
        show_success "Tag $2 eliminado"
        exit 0
        ;;
esac

VERSION="$1"
MESSAGE="${2:-Release version $VERSION}"
TAG_NAME="v$VERSION"

# Validar formato de versión (semántica)
if ! [[ $VERSION =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9]+)?$ ]]; then
    show_error "Formato de versión inválido. Use: MAJOR.MINOR.PATCH (ej: 2.1.0)"
    exit 1
fi

# Verificar si el tag ya existe
if git tag -l | grep -q "^$TAG_NAME$"; then
    show_error "El tag $TAG_NAME ya existe localmente"
    exit 1
fi

# Verificar conexión con el remoto
show_info "Verificando conexión con GitHub..."
if ! git ls-remote --exit-code origin >/dev/null 2>&1; then
    show_error "No se puede conectar al repositorio remoto"
    exit 1
fi

# Verificar si el tag existe remotamente
if git ls-remote --tags origin | grep -q "refs/tags/$TAG_NAME$"; then
    show_error "El tag $TAG_NAME ya existe en el repositorio remoto"
    exit 1
fi

# Mostrar información actual
show_info "Creando tag: $TAG_NAME"
show_info "Mensaje: $MESSAGE"
echo ""

# Verificar estado del repositorio
show_info "Verificando estado del repositorio..."
if [ -n "$(git status --porcelain)" ]; then
    show_warning "Hay cambios no commitados en el working directory:"
    git status --short
    echo ""
    read -p "¿Deseas continuar de todos modos? (y/N): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        show_error "Operación cancelada"
        exit 1
    fi
fi

# Obtener commits desde el último tag
LATEST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "v0.0.0")
show_info "Cambios desde $LATEST_TAG:"
git log --oneline --pretty=format:"  - %s" "$LATEST_TAG..HEAD" 2>/dev/null || git log --oneline --pretty=format:"  - %s" -10

echo ""
read -p "¿Confirmas la creación del tag? (y/N): " -n 1 -r
echo ""
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    show_error "Operación cancelada"
    exit 1
fi

# Crear el tag
show_info "Creando tag local..."
if git tag -a "$TAG_NAME" -m "$MESSAGE"; then
    show_success "Tag $TAG_NAME creado localmente"
else
    show_error "Error al crear el tag"
    exit 1
fi

# Subir el tag al remoto
show_info "Subiendo tag a GitHub..."
if git push origin "$TAG_NAME"; then
    show_success "Tag $TAG_NAME subido exitosamente a GitHub"
else
    show_error "Error al subir el tag"
    show_info "Eliminando tag local..."
    git tag -d "$TAG_NAME"
    exit 1
fi

# Información adicional
show_info "📋 GitHub Actions se ejecutará automáticamente y creará el release"
show_info "⏰ Puede tomar 1-2 minutos para que el release esté disponible"
show_info "🌐 Ve a: https://github.com/ealexeyja/disk-protector-app/actions"
echo ""

# Mostrar comandos útiles
show_info "Comandos útiles:"
echo "  Ver estado del workflow: ./create-tag.sh --list"
echo "  Ver último tag:          ./create-tag.sh --latest"
echo "  Eliminar tag:            ./create-tag.sh --delete $TAG_NAME"
