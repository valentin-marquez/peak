# GitHub Actions para BagsForEveryone

Este proyecto usa GitHub Actions para empaquetado automático, NO para compilación.

## 🚀 Workflows Configurados

### 1. **Validate Release Files** - `.github/workflows/ci.yml`
Se ejecuta en cada push a `main` o `develop`:
- ✅ Valida el `manifest.json`
- ✅ Verifica que existan los archivos necesarios en `Release/`
- ✅ Verifica el tamaño del DLL
- ✅ Crea un paquete de prueba para verificar estructura
- ✅ Sube artefactos para descarga

### 2. **Package and Release** - `.github/workflows/build-and-release.yml`
Se ejecuta cuando creas un tag de versión:
- 📦 Empaqueta archivos pre-compilados de la carpeta `Release/`
- 🎨 Genera un ícono automáticamente si no existe
- 📋 Crea un GitHub Release con archivos adjuntos
- 🏷️ Actualiza automáticamente la versión en manifest.json
- ✅ Verifica que el DLL exista antes de empaquettar

## 📋 Proceso de Desarrollo y Release

### Paso 1: Desarrollo Local
```bash
# Hacer cambios al código en src/
# Compilar localmente
dotnet build --configuration Release

# Copiar el DLL compilado a Release/
copy "bin\Release\BagsForEveryone.dll" "Release\"
```

### Paso 2: Preparar Release
```bash
# Verificar que Release/ contenga:
# - BagsForEveryone.dll (compilado)
# - manifest.json
# - README.md
# - icon.png (opcional, se genera automáticamente)

# Commitear todo incluyendo el DLL
git add .
git commit -m "Ready for release v1.0.0"
git push origin main
```

### Paso 3: Crear Release
```bash
# Crear y pushear tag
git tag v1.0.0
git push origin v1.0.0
```

### Paso 4: ¡Automático!
- GitHub Actions verifica que el DLL exista en `Release/`
- Empaqueta los archivos pre-compilados
- Crea el ZIP con la estructura correcta de BepInEx
- Genera un GitHub Release listo para Thunderstore

## 📁 Archivos Generados

Cada release automáticamente genera:
- `BagsForEveryone-X.Y.Z.zip` - Paquete para Thunderstore con estructura BepInEx
- `BagsForEveryone.dll` - Archivo DLL para instalación manual

## 🎨 Ícono Automático

Si no tienes un `Release/icon.png`, GitHub Actions crea uno automáticamente:
- 256x256 píxeles (requerimiento de Thunderstore)
- Color verde con texto "B4E"

## ✨ Ventajas del Nuevo Proceso

- ✅ **Sin errores de dependencias**: Compilamos localmente con todas las dependencias
- ✅ **Builds consistentes**: Mismo entorno de desarrollo siempre
- ✅ **Empaquetado automático**: Solo se encarga de crear el ZIP
- ✅ **Verificación previa**: Valida que todo esté listo antes de empaquetar
- ✅ **Proceso simple**: Compilar → Copiar → Tag → Listo

## 🐛 Debugging

Si algo falla:
1. **Error "DLL not found"**: Asegúrate de copiar el DLL compilado a `Release/`
2. **Errores de manifest**: Verifica que `Release/manifest.json` sea válido
3. **Estructura incorrecta**: El workflow crea automáticamente `BepInExPack/plugins/BagsForEveryone/`

## 📋 Checklist antes de Release

- [ ] Código compilado localmente sin errores
- [ ] `Release/BagsForEveryone.dll` existe y funciona
- [ ] `Release/manifest.json` tiene la información correcta
- [ ] `Release/README.md` actualizado
- [ ] Todo commiteado y pusheado a `main`
- [ ] Crear tag de versión