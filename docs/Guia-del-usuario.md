# Guía del usuario del Monitor de conexiones TCP

## Función principal

En pocas palabras, esta herramienta permite **vigilar una dirección IP o un puerto que usted elija**. Puede:

- Registrar automáticamente cuándo aparece una conexión
- Registrar las direcciones IP y los puertos locales y remotos
- Registrar, cuando estén disponibles, el programa relacionado, el PID y la ruta del ejecutable
- Registrar en silencio, mostrar un aviso en la bandeja o abrir una alerta emergente
- Guardar información para revisarla después o compartirla con personal de ciberseguridad
- Ayudar a confirmar si más adelante aparece una nueva conexión al mismo destino

## Cómo funciona

Primero cree una regla para indicar qué dirección IP o qué puerto desea observar. Después active la regla e inicie el monitoreo. La aplicación revisa una vez por segundo la lista de conexiones TCP de Windows. Solo procesa las conexiones que coinciden con una regla activada. Las demás conexiones normales no generan registros ni avisos.

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

Para facilitar la lectura, la tabla muestra solo los campos principales. Haga doble clic en una fila para abrir **Detalles del evento** y ver las reglas coincidentes, el extremo local, el estado TCP, el PID, la ruta del programa y la acción. El estado activo y la duración siguen actualizándose allí, y **Copiar detalles** copia el registro completo.

La duración observada comienza cuando la aplicación ve la conexión por primera vez, por lo que puede no coincidir con su duración total real. Después de detener el monitoreo, la aplicación no puede saber si la conexión terminó durante ese intervalo. Al iniciar de nuevo el monitoreo se crea, por tanto, una observación nueva. El CSV interno solo escribe información cuando detecta la conexión y cuando termina la observación; la aplicación combina esos datos en una sola fila del Registro de eventos.

También se crea un registro nuevo cuando una conexión desaparece durante dos revisiones y vuelve a aparecer.

El límite total es de 25 MB de forma predeterminada y puede cambiarse a un valor entre 5 y 500 MB en **Configuración**. La aplicación utiliza hasta cinco archivos y elimina automáticamente los registros más antiguos cuando alcanza el límite elegido.

## Centro de ayuda

En **Configuración**, seleccione **Abrir centro de ayuda** para consultar dentro de la aplicación la descripción del proyecto y la guía del usuario. Los documentos cambian al idioma actual de la interfaz.

## Configuración de inicio y sonido de alerta

- **Abrir la aplicación al iniciar sesión en Windows:** abre la aplicación después de iniciar sesión y ayuda a evitar que se olvide el monitoreo. No inicia el monitoreo por sí sola.
- **Iniciar el monitoreo automáticamente al abrir la aplicación:** inicia el monitoreo con las reglas activadas cada vez que se abre la aplicación.
- **Sonido de alerta urgente:** utiliza un aviso breve integrado en la aplicación, por lo que no depende del esquema de sonidos de eventos de Windows. Ajuste el volumen entre el 10% y el 100% (40% de forma predeterminada). **Probar sonido** y las alertas urgentes reales usan el mismo nivel; el volumen de Windows también se mantiene.

## Limitaciones importantes

1. La aplicación revisa las conexiones una vez por segundo, por lo que puede no detectar una conexión que dure menos de un segundo.
2. La versión 1 **solo observa TCP**. No observa UDP.
3. La tabla de conexiones TCP de Windows no ofrece un dato completamente fiable sobre quién inició la conexión, por lo que la aplicación no puede determinar qué lado la inició.
4. Los permisos de Windows pueden impedir que se lea la ruta de algunos procesos del sistema o protegidos. El PID y cualquier nombre de proceso disponible se siguen registrando.
5. No hay monitoreo cuando la aplicación está cerrada, el monitoreo está detenido o el equipo está suspendido.
6. La duración observada comienza cuando la aplicación detecta la conexión por primera vez y tiene una precisión aproximada de un segundo. No es una hora exacta de inicio proporcionada por Windows.
7. La aplicación solo registra y muestra avisos. No cierra programas, no cambia el firewall ni bloquea direcciones IP.

## Privacidad y permisos

1. No se requieren permisos de administrador.
2. No se requiere una cuenta, un nombre de usuario, una contraseña ni una dirección de correo electrónico.
3. La aplicación no se conecta a un servidor del desarrollador ni sube registros.
4. No lee el contenido de los paquetes de red.
5. La configuración se guarda en `%LOCALAPPDATA%\ConnectionWatcher\config.json`.

## Desinstalación

Puede quitar la versión instalada desde **Aplicaciones instaladas** de Windows. La desinstalación elimina el programa, pero conserva de forma predeterminada la configuración y los registros de `%LOCALAPPDATA%\ConnectionWatcher`, para evitar que se pierdan por accidente datos útiles para una investigación. Si está seguro de que ya no los necesita, puede eliminar esa carpeta manualmente.
