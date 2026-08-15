# Monitor de conexiones TCP

## Contexto y finalidad

Al investigar una conexión de red inusual, a menudo necesitamos responder una pregunta sencilla que puede ser difícil de confirmar a tiempo:

> ¿Mi equipo se conectó a una dirección IP o a un puerto específico? Si lo hizo, ¿cuándo ocurrió, con qué proceso lo relacionó Windows y qué contexto de la aplicación puede recuperarse?

El Monitor de recursos de Windows muestra la actividad de red actual, pero el usuario debe abrirlo y observarlo continuamente. Una conexión breve puede desaparecer rápidamente, y no es práctico vigilar la ventana durante mucho tiempo. Tampoco avisa automáticamente sobre un objetivo elegido ni conserva un historial continuo.

El Monitor de conexiones TCP ayuda a resolver este problema. Después de que el usuario elige una dirección IP o un puerto, la aplicación busca en segundo plano las conexiones que coincidan. Cuando encuentra una, registra la hora, las direcciones, los puertos, el propietario de la conexión informado por Windows y, cuando están disponibles, datos del archivo, procesos superiores y servicios de Windows. Después avisa al usuario según la regla configurada.

Esta herramienta no sustituye al Monitor de recursos ni a un antivirus. Sirve para observar objetivos específicos, conservar registros y aportar información para una investigación de seguridad posterior.

## Descripción del proyecto

El Monitor de conexiones TCP es una pequeña **herramienta de Windows para observar conexiones de red mediante reglas**. El usuario puede indicar la dirección IP remota, el puerto remoto o el puerto local que le interesa. Cuando Windows informa de una conexión TCP que coincide con una regla activada, la aplicación la registra o muestra un aviso según esa regla.

En pocas palabras, ayuda a vigilar una dirección IP o un puerto específico. Por ejemplo, puede configurarse para observar `103.1.40.235:1433`. Cuando comienza el monitoreo y el equipo se conecta a ese destino, la aplicación registra la hora, el estado activo o finalizado, la duración observada, el propietario informado por Windows, el PID y el contexto de aplicación disponible. Según la configuración, puede **registrar en silencio, mostrar un aviso en la bandeja o abrir una alerta emergente**.

El intervalo predeterminado es de un segundo. El usuario puede elegir entre 0,5 y 10 segundos, en pasos de 0,5 segundos. Un intervalo menor tiene más posibilidades de detectar conexiones breves; uno mayor usa menos recursos, pero puede pasarlas por alto.

La aplicación solo informa: «Apareció una conexión que usted pidió observar». No clasifica otras conexiones como sospechosas, y una sola conexión no demuestra que el equipo tenga malware. Los datos guardados pueden compartirse con un equipo de ciberseguridad para continuar la investigación.

## Estructura del proyecto

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/
│   └── ConnectionWatcher.App/
├── tests/
│   ├── ConnectionWatcher.Tests/
│   └── ConnectionWatcher.UiSmoke/
├── docs/
├── learning/
├── scripts/
│   └── build-release.ps1
├── packaging/
└── Final-Share/
    ├── TCP-Connection-Watcher-Setup-win-x64.exe
    ├── SHA256SUMS.txt
    └── Docs/
```

- `ConnectionWatcher.sln`: archivo de solución de todo el proyecto.
- `src/ConnectionWatcher.Core`: lógica de configuración, reglas, lectura de conexiones TCP de Windows, seguimiento temporal, contexto de procesos y registros CSV compatibles con versiones anteriores.
- `src/ConnectionWatcher.App`: interfaz de Windows en siete idiomas, incluida la ventana principal, el editor de reglas, los detalles de eventos, el centro de ayuda, la comprobación de actualizaciones, los avisos de la bandeja y la ventana de alerta.
- `tests`: 20 pruebas funcionales y de compatibilidad, además de pruebas de interfaz multilingüe y escalado DPI.
- `docs`: descripción del proyecto y guía del usuario en los siete idiomas compatibles.
- `learning`: tutorial para desarrolladores y material de aprendizaje sobre la arquitectura.
- `scripts/build-release.ps1`: ejecuta la verificación y genera automáticamente `artifacts`, `dist` y `Final-Share`, en ese orden.
- `packaging`: definición del instalador y notas de la edición portátil.
- `Final-Share`: carpeta local, excluida de Git, con un instalador multilingüe, los siete conjuntos de documentos, las notas de la versión y las sumas SHA-256.

## Compilación y verificación

Para compilar el código fuente en Windows se necesita .NET 8 SDK.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

El paquete de distribución incluye `SHA256SUMS.txt`. El destinatario puede comprobar la integridad de los archivos con `Get-FileHash` en PowerShell.

Los responsables del mantenimiento pueden ejecutar `scripts\build-release.ps1` para compilar, probar, publicar, empaquetar, copiar los documentos actuales y generar las sumas en un solo proceso.
