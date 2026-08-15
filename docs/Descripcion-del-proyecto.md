# Descripción del proyecto de SocketSight

## Contenido

- [Antecedentes y propósito](#antecedentes-y-propósito)
- [Descripción del proyecto](#descripción-del-proyecto)
- [Diseño principal](#diseño-principal)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Inicio, idioma y centro de ayuda](#inicio-idioma-y-centro-de-ayuda)
- [Compilación y verificación](#compilación-y-verificación)

## Antecedentes y propósito

El Monitor de recursos de Windows puede mostrar la actividad de red actual, pero el usuario debe mantenerlo abierto y observarlo. Una conexión breve puede desaparecer antes de ser detectada y no es cómodo conservar un registro centrado en un objetivo.

SocketSight permite definir reglas para una IP remota, un puerto remoto o un puerto local. Solo procesa conexiones TCP que coinciden con las reglas y guarda la hora, el estado, la duración observada, el proceso indicado por Windows y el contexto de aplicación disponible.

No sustituye al Monitor de recursos ni al antivirus. Su objetivo es facilitar la observación repetida de una conexión elegida y su revisión posterior, para que el usuario o un profesional de seguridad pueda investigarla.

## Descripción del proyecto

SocketSight es una herramienta local de observación de conexiones TCP para Windows basada en reglas. Cuando se inicia la supervisión, lee la tabla de conexiones TCP de Windows según el intervalo seleccionado y procesa las conexiones que coinciden con reglas activadas.

El intervalo predeterminado es de un segundo. Puede elegirse entre 0,5 y 10 segundos en pasos de 0,5. Un intervalo corto detecta mejor las conexiones breves, pero realiza más comprobaciones; uno largo usa menos recursos, pero puede perder conexiones breves.

La aplicación solo registra o avisa sobre conexiones elegidas mediante reglas. No marca automáticamente como sospechosa la actividad que queda fuera de ellas. Esta versión se centra en TCP; UDP requeriría otro diseño de rastreo de bajo nivel y una atribución de aplicaciones más compleja, por lo que queda fuera de esta versión.

## Diseño principal

- **Primero las reglas:** solo se procesan conexiones que coinciden con reglas activadas.
- **Una observación por conexión:** una conexión continua no se escribe cada segundo.
- **Finalización por tiempo real:** se considera terminada después de dos segundos ausente; si vuelve durante ese tiempo, sigue siendo la misma observación.
- **El contexto de aplicación es una pista:** los datos del proceso, PID, archivo, procesos padre y servicios de Windows ayudan a investigar, pero no prueban la causa definitiva.
- **Vista y datos separados:** **Limpiar vista** oculta filas antiguas sin borrar los registros CSV.
- **Funcionamiento local:** no lee el contenido de los paquetes ni sube reglas o registros. Solo contacta con GitHub cuando el usuario comprueba actualizaciones o abre la página de comentarios.

## Estructura del proyecto

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/       # reglas, supervisión, estado, registros y ajustes
│   └── ConnectionWatcher.App/        # interfaz WinForms, idiomas, bandeja e inicio
├── tests/
│   ├── ConnectionWatcher.Tests/      # pruebas principales y de compatibilidad
│   └── ConnectionWatcher.UiSmoke/    # pruebas de idioma, DPI y diseño
├── docs/                             # descripciones y guías de usuario
├── learning/                         # tutorial y material de aprendizaje
├── scripts/build-release.ps1         # compilación, pruebas, empaquetado y publicación
├── packaging/                        # definición del instalador de Inno Setup
└── Final-Share/                      # archivos finales para los usuarios
```

- `ConnectionWatcher.Core` contiene las reglas, la lectura TCP de Windows, el seguimiento de conexiones, el contexto de procesos, los registros CSV y los ajustes.
- `ConnectionWatcher.App` contiene la interfaz, el editor de reglas, los detalles de eventos, el centro de ayuda, los avisos de bandeja, las alertas, los idiomas y la pantalla de inicio.
- `tests` protege el comportamiento principal y comprueba distintos idiomas y escalas de pantalla.
- `scripts` compila, prueba, publica la aplicación autónoma, crea el instalador, copia los documentos actuales y genera sumas SHA-256.
- `artifacts` es la salida de publicación, `dist` es la salida del instalador y `Final-Share` es el paquete final para usuarios. Los tres pueden regenerarse.

El usuario descarga un único instalador: `SocketSight-Setup-win-x64.exe`. La aplicación instalada es autónoma y de varios archivos; no hace falta instalar .NET por separado.

## Inicio, idioma y centro de ayuda

El instalador admite siete idiomas. El idioma elegido durante la instalación también se convierte en el idioma de la interfaz de SocketSight. Durante una actualización, el nuevo idioma reemplaza una vez al anterior; las reglas, los ajustes y los registros se conservan.

Si el inicio tarda más de unos 0,5 segundos, SocketSight muestra una pantalla breve de inicio. Sus mensajes son solo texto local de estado; no significan que la aplicación se esté conectando a Internet ni ejecutando un análisis adicional. La pantalla se cierra cuando la ventana principal está lista.

El centro de ayuda de Ajustes muestra la descripción del proyecto y la guía del usuario en el idioma actual. La comprobación de actualizaciones es manual; la aplicación no descarga, instala ni ejecuta actualizaciones automáticamente.

## Compilación y verificación

Para compilar en Windows se necesitan .NET 8 SDK e Inno Setup.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Los mantenedores pueden ejecutar:

```powershell
scripts\build-release.ps1
```

El script compila, prueba, publica, crea el instalador, reúne los documentos actuales y genera sumas SHA-256. Los destinatarios pueden usar `Get-FileHash` de PowerShell para comprobar el instalador.
