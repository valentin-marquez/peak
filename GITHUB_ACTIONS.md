# GitHub Actions para BagsForEveryone

Este proyecto usa un solo workflow simplificado para empaquetado automático.

## 🚀 Workflow Configurado

### **Release** - `.github/workflows/release.yml`
Se ejecuta cuando creas un tag de versión:
- ✅ Verifica que la carpeta `Release/` esté completa
- 📦 Empaqueta archivos pre-compilados con estructura BepInEx
- 🎨 Genera un ícono automáticamente si no existe
- 📋 Crea un GitHub Release con archivos adjuntos
- 🏷️ Actualiza automáticamente la versión en manifest.json

## 📋 Proceso de Desarrollo y Release

### Paso 1: Desarrollo Local
```bash
# Hacer cambios al código en src/
# Compilar (automáticamente copia el DLL a Release/)
dotnet build --configuration Release
```

### Paso 2: Preparar Release
```bash
# Verificar que Release/ contenga:
# ✅ BagsForEveryone.dll (copiado automáticamente por PostBuild)
# ✅ manifest.json
# ✅ README.md
# ✅ icon.png (opcional, se genera automáticamente)

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
- Empaqueta los archivos con estructura `BepInExPack/plugins/BagsForEveryone/`
- Crea el ZIP listo para Thunderstore
- Genera un GitHub Release

## 📁 Archivos Generados

Cada release automáticamente genera:
- `BagsForEveryone-X.Y.Z.zip` - Paquete completo para Thunderstore
- `BagsForEveryone.dll` - Archivo DLL para instalación manual

## 🎯 Estructura del ZIP Final

```
BagsForEveryone-1.0.0.zip
├── BepInExPack/
│   └── plugins/
│       └── BagsForEveryone/
│           └── BagsForEveryone.dll  ✅
├── manifest.json
├── README.md
└── icon.png
```

## ✨ Ventajas del Workflow Único

- ✅ **Simplicidad**: Un solo archivo de workflow
- ✅ **Sin errores de dependencias**: Solo empaqueta archivos listos
- ✅ **Verificación automática**: Falla si falta algún archivo requerido
- ✅ **Proceso directo**: Compilar → Tag → Listo

## 🔧 PostBuild Automático

El proyecto está configurado para copiar automáticamente el DLL a dos destinos:
```xml
<Exec Command="copy &quot;$(TargetPath)&quot; &quot;D:\SteamLibrary\steamapps\common\PEAK\BepInEx\plugins\&quot;" />
<Exec Command="copy &quot;$(TargetPath)&quot; &quot;$(ProjectDir)Release\&quot;" />
```

## 📋 Checklist antes de Release

- [ ] Código compilado localmente sin errores
- [ ] `Release/BagsForEveryone.dll` existe (automático con PostBuild)
- [ ] `Release/manifest.json` actualizado
- [ ] `Release/README.md` actualizado
- [ ] Todo commiteado y pusheado a `main`
- [ ] Crear tag de versión

¡Listo para el primer release! 🎒