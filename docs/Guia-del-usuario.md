# Guía del usuario del Monitor de conexiones TCP

## Función principal

En pocas palabras, esta herramienta permite **vigilar una dirección IP o un puerto que usted elija**. Puede:

- Registrar automáticamente cuándo aparece una conexión
- Registrar las direcciones IP y los puertos locales y remotos
- Registrar, cuando estén disponibles, el propietario de la conexión informado por Windows, el PID, la ruta del ejecutable, la información del archivo, los procesos superiores o anfitriones y los servicios de Windows relacionados
- Registrar en silencio, mostrar un aviso en la bandeja o abrir una alerta emergente
- Guardar información para revisarla después o compartirla con personal de ciberseguridad
- Ayudar a confirmar si más adelante aparece una nueva conexión al mismo destino

## Cómo funciona

Primero cree una regla para indicar qué dirección IP o qué puerto desea observar. Después active la regla e inicie el monitoreo. La aplicación revisa de forma predeterminada una vez por segundo la lista de conexiones TCP de Windows. En **Inicio** puede cambiar el intervalo de 0,5 a 10 segundos, en pasos de 0,5 segundos. Los intervalos menores tienen más posibilidades de detectar conexiones breves; los mayores usan menos recursos, pero pueden pasarlas por alto. Solo se procesan las conexiones que coinciden con una regla activada. Las demás conexiones normales no generan registros ni avisos.

Cuando una conexión coincide con una regla, la aplicación realiza la acción elegida:

- **Registrar en silencio:** escribe el evento en el registro CSV sin cambiar el icono de la bandeja ni mostrar un contador.
- **Aviso en bandeja y registro:** no abre una ventana. El icono de la bandeja cambia a un estado de aviso, que se borra al abrir la página Registro de eventos.
- **Alerta emergente y registro:** abre una ventana cuando aparece la primera coincidencia. Mientras la ventana esté abierta, las coincidencias posteriores actualizan la misma ventana. Después de cerrarla, el intervalo configurado en la regla determina cuándo puede aparecer otra alerta.

La página Inicio muestra un símbolo compacto para cada acción. **Reglas de monitoreo** combina el símbolo con un nombre corto, mientras que la columna **Acción** del Registro de eventos muestra solo el símbolo para que siga siendo fácil de reconocer en una columna estrecha:

- `1 ●` círculo gris: Registrar en silencio
- `2 ▲` triángulo naranja: Aviso en bandeja y registro
- `3 ◆` rombo rojo: Alerta emergente y registro

El número y la forma también permiten distinguir las acciones cuando resulta difícil ver el color. Coloque el puntero sobre un símbolo de una regla o del registro para ver el nombre completo de la acción.

#### *Importante:*

1. Una coincidencia solo significa que apareció una conexión que usted decidió observar. No demuestra que el equipo tenga malware.
2. Esta herramienta **solo registra conexiones y muestra avisos**. Para decidir otras medidas de seguridad también deben considerarse los resultados del antivirus y el consejo de profesionales cualificados.

## Primer uso

1. Seleccione uno de los siete idiomas compatibles durante la instalación. En una edición portátil, la aplicación pedirá el idioma al abrirse por primera vez.
2. Abra **Reglas de monitoreo**.
3. Seleccione **Nueva regla**.
4. Introduzca las condiciones en los campos del formulario.
5. Revise la vista previa de la regla al final del formulario.
6. Guarde y active la regla.
7. Regrese a **Inicio** y seleccione **Iniciar monitoreo**.

### Ejemplo

Para observar si cualquier puerto local del equipo vuelve a conectarse a `103.1.40.235:1433` —la dirección IP y el puerto del servidor remoto—, cree esta regla:

- Tipo de regla: Conexión TCP
- IP remota: `103.1.40.235`
- Puerto remoto: `1433`
- Puerto local: Cualquiera
- Acción al coincidir: Alerta emergente y registro
- Intervalo para repetir la alerta: 5 minutos

## Registros

Los registros se guardan en:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

Cada conexión nueva que coincide aparece como una sola fila en el **Registro de eventos**. Si permanece abierta durante varias horas, no se registra de nuevo cada segundo. **Estado** indica si está activa o finalizada, y **Duración observada** se actualiza mientras está activa y queda fija cuando termina.

Para facilitar la lectura, la tabla muestra solo los campos principales. La columna **Aplicación** usa la información disponible del producto del archivo y, si no existe, muestra el nombre del proceso. Haga doble clic en una fila para abrir **Detalles del evento** y ver el propietario de la conexión informado por Windows, el PID, la ruta, la información del producto, hasta tres procesos superiores o anfitriones, los servicios de Windows relacionados y los demás campos de la conexión. El estado activo y la duración siguen actualizándose allí, y **Copiar detalles** copia el registro completo.

Este contexto puede ayudar a identificar qué aplicación está relacionada con una conexión, pero no siempre demuestra cuál la provocó finalmente. Por ejemplo, un navegador, proxy, VPN o componente web integrado podría llevar tiempo ejecutándose en segundo plano.

La duración observada comienza cuando la aplicación ve la conexión por primera vez, por lo que puede no coincidir con su duración total real. Después de detener el monitoreo, la aplicación no puede saber si la conexión terminó durante ese intervalo. Al iniciar de nuevo el monitoreo se crea, por tanto, una observación nueva. El CSV interno solo escribe información cuando detecta la conexión y cuando termina la observación; la aplicación combina esos datos en una sola fila del Registro de eventos.

Una conexión se marca como finalizada solo después de permanecer ausente de la tabla de conexiones de Windows durante dos segundos. Si reaparece dentro de ese tiempo de espera, sigue siendo la misma observación. La hora de finalización corresponde al último momento en que la aplicación vio realmente la conexión. Una aparición posterior al tiempo de espera crea un registro nuevo.

Seleccione **Limpiar pantalla** cuando quiera despejar el Registro de eventos. Esto oculta las filas existentes de la interfaz sin borrar los registros CSV. Los eventos anteriores permanecen ocultos después de reiniciar la aplicación, mientras que los nuevos aparecen normalmente.

El límite total es de 25 MB de forma predeterminada y puede cambiarse a un valor entre 5 y 500 MB en **Configuración**. La aplicación utiliza hasta cinco archivos y elimina automáticamente los registros más antiguos cuando alcanza el límite elegido.

## Centro de ayuda

En **Configuración**, seleccione **Abrir centro de ayuda** para consultar dentro de la aplicación la descripción del proyecto y la guía del usuario. Los documentos cambian al idioma actual de la interfaz.

## Actualizaciones del software

En **Configuración**, seleccione **Comprobar ahora** para consultar en GitHub la versión pública más reciente. La aplicación solo lo hace cuando usted lo solicita. Si existe una versión más nueva, puede abrir su página de GitHub Release, leer las notas y descargarla personalmente. La aplicación no descarga, instala ni ejecuta actualizaciones automáticamente, y no envía reglas ni registros.

## Configuración de inicio y sonido de alerta

- **Abrir la aplicación al iniciar sesión en Windows:** abre la aplicación después de iniciar sesión y ayuda a evitar que se olvide el monitoreo. No inicia el monitoreo por sí sola.
- **Iniciar el monitoreo automáticamente al abrir la aplicación:** inicia el monitoreo con las reglas activadas cada vez que se abre la aplicación.
- **Sonido de alerta urgente:** utiliza un aviso breve integrado en la aplicación, por lo que no depende del esquema de sonidos de eventos de Windows. Ajuste el volumen entre el 10% y el 100% (40% de forma predeterminada). **Probar sonido** aparece junto al control de volumen; tanto la prueba como las alertas urgentes reales usan el mismo nivel, y el volumen de Windows también se mantiene.

## Limitaciones importantes

1. La aplicación revisa las conexiones una vez por segundo de forma predeterminada. Incluso con el ajuste de 0,5 segundos, puede pasar por alto una conexión que aparezca y desaparezca entre dos revisiones.
2. La versión 1 **solo observa TCP**. No observa UDP.
3. La tabla de conexiones TCP de Windows no ofrece un dato completamente fiable sobre quién inició la conexión, por lo que la aplicación no puede determinar qué lado la inició.
4. Los permisos de Windows o la finalización rápida de un proceso pueden impedir que se lea una ruta, información del archivo, un proceso superior o un servicio relacionado. El PID y cualquier nombre de proceso disponible se siguen registrando. El contexto de procesos y servicios es evidencia para investigar, no una conclusión garantizada sobre la causa principal.
5. No hay monitoreo cuando la aplicación está cerrada, el monitoreo está detenido o el equipo está suspendido.
6. La duración observada comienza cuando la aplicación detecta la conexión por primera vez. Su precisión depende del intervalo elegido y no es una hora exacta de inicio proporcionada por Windows.
7. La aplicación solo registra y muestra avisos. No cierra programas, no cambia el firewall ni bloquea direcciones IP.

## Privacidad y permisos

1. No se requieren permisos de administrador.
2. No se requiere una cuenta, un nombre de usuario, una contraseña ni una dirección de correo electrónico.
3. La aplicación solo se conecta a GitHub después de que usted selecciona manualmente **Comprobar ahora**. No se conecta a un servidor del desarrollador ni envía reglas o registros.
4. No lee el contenido de los paquetes de red.
5. La configuración se guarda en `%LOCALAPPDATA%\ConnectionWatcher\config.json`.

## Desinstalación

Puede quitar la versión instalada desde **Aplicaciones instaladas** de Windows. La desinstalación elimina el programa, pero conserva de forma predeterminada la configuración y los registros de `%LOCALAPPDATA%\ConnectionWatcher`, para evitar que se pierdan por accidente datos útiles para una investigación. Si está seguro de que ya no los necesita, puede eliminar esa carpeta manualmente.
