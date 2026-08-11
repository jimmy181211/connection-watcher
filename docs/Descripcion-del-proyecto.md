# Monitor de conexiones TCP

## Contexto y finalidad

Al investigar una conexión de red inusual, a menudo necesitamos responder una pregunta sencilla que puede ser difícil de confirmar a tiempo:

> ¿Mi equipo se conectó a una dirección IP o a un puerto específico? Si lo hizo, ¿cuándo ocurrió y qué programa creó la conexión?

El Monitor de recursos de Windows muestra la actividad de red actual, pero el usuario debe abrirlo y observarlo continuamente. Una conexión breve puede desaparecer rápidamente, y no es práctico vigilar la ventana durante mucho tiempo. Tampoco avisa automáticamente sobre un objetivo elegido ni conserva un historial continuo.

El Monitor de conexiones TCP ayuda a resolver este problema. Después de que el usuario elige una dirección IP o un puerto, la aplicación busca en segundo plano las conexiones que coincidan. Cuando encuentra una, registra la hora, las direcciones, los puertos y, cuando están disponibles, el programa y el PID. Después avisa al usuario según la regla configurada.

Esta herramienta no sustituye al Monitor de recursos ni a un antivirus. Sirve para observar objetivos específicos, conservar registros y aportar información para una investigación de seguridad posterior.

## Descripción del proyecto

El Monitor de conexiones TCP es una pequeña **herramienta de Windows para observar conexiones de red mediante reglas**. El usuario puede indicar la dirección IP remota, el puerto remoto o el puerto local que le interesa. Cuando Windows informa de una conexión TCP que coincide con una regla activada, la aplicación la registra o muestra un aviso según esa regla.

En pocas palabras, ayuda a vigilar una dirección IP o un puerto específico. Por ejemplo, puede configurarse para observar `103.1.40.235:1433`. Cuando comienza el monitoreo y el equipo se conecta a ese destino, la aplicación registra la hora, el estado activo o finalizado, la duración observada, el programa relacionado y su PID. Según la configuración, puede **registrar en silencio, mostrar un aviso en la bandeja o abrir una alerta emergente**.

La aplicación solo informa: «Apareció una conexión que usted pidió observar». No clasifica otras conexiones como sospechosas, y una sola conexión no demuestra que el equipo tenga malware. Los datos guardados pueden compartirse con un equipo de ciberseguridad para continuar la investigación.

## Estructura del proyecto

```text
connection-watcher/
├── ConnectionWatcher.sln
├── src/
│   ├── ConnectionWatcher.Core/
│   └── ConnectionWatcher.App/
├── tests/
│   ├── ConnectionWatcher.Tests/
│   └── ConnectionWatcher.UiSmoke/
├── docs/
├── packaging/
└── Final-Share/
    ├── TCP-Connection-Watcher-Setup-win-x64.exe
    ├── SHA256SUMS.txt
    └── Docs/
```

- `ConnectionWatcher.sln`: archivo de solución de todo el proyecto.
- `src/ConnectionWatcher.Core`: lógica de configuración, reglas, lectura de conexiones TCP de Windows, eliminación de duplicados y registros CSV.
- `src/ConnectionWatcher.App`: interfaz de Windows disponible en siete idiomas, incluida la ventana principal, el editor de reglas, el centro de ayuda, los avisos de la bandeja y la ventana de alerta.
- `tests`: pruebas funcionales y de interfaz; actualmente hay 16 pruebas funcionales.
- `docs`: descripción del proyecto y guía del usuario en los siete idiomas compatibles.
- `packaging`: definición del instalador y notas de la edición portátil.
- `Final-Share`: carpeta final para compartir, con un único instalador multilingüe, los documentos y las sumas SHA-256.

## Compilación y verificación

Para compilar el código fuente en Windows se necesita .NET 8 SDK.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

El paquete de distribución incluye `SHA256SUMS.txt`. El destinatario puede comprobar la integridad de los archivos con `Get-FileHash` en PowerShell.
