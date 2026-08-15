# Guía del usuario de SocketSight

## Contenido

- [¿Qué es esta herramienta?](#qué-es-esta-herramienta)
- [Instalación e inicio rápido](#instalación-e-inicio-rápido)
- [Intervalo de comprobación](#intervalo-de-comprobación)
- [Qué ocurre después de una coincidencia](#qué-ocurre-después-de-una-coincidencia)
- [Ver eventos](#ver-eventos)
- [Cómo interpretar un registro](#cómo-interpretar-un-registro)
- [Centro de ayuda y actualizaciones](#centro-de-ayuda-y-actualizaciones)
- [Registros, sonido y otros ajustes](#registros-sonido-y-otros-ajustes)
- [Privacidad, permisos y desinstalación](#privacidad-permisos-y-desinstalación)

## ¿Qué es esta herramienta?

SocketSight ayuda a vigilar una IP o un puerto específico.

Cuando una conexión TCP coincide con una regla, la aplicación registra la hora, la IP, el puerto y la información del proceso disponible en Windows, y después aplica el aviso que usted haya elegido.

Solo observa, registra y avisa. No cierra programas, cambia el firewall ni bloquea direcciones IP.

## Instalación e inicio rápido

El idioma elegido durante la instalación también se usa en la aplicación. Al actualizar, elegir otro idioma cambia el idioma de la aplicación una vez; las reglas, los ajustes y los registros se conservan.

Si el inicio tarda más de unos 0,5 segundos, SocketSight muestra una pantalla breve que desaparece cuando la ventana principal está lista.

1. Abra **Reglas de supervisión**.
2. Seleccione **Nueva regla**.
3. Escriba la IP o el puerto que desea vigilar.
4. Guarde y active la regla.
5. Vuelva a **Inicio** y seleccione **Iniciar supervisión**.

Por ejemplo, para vigilar `103.1.40.235:1433`:

- IP remota: `103.1.40.235`
- Puerto remoto: `1433`
- Puerto local: Cualquiera
- Acción: Alerta emergente y registro
- Intervalo de repetición: 5 minutos

## Intervalo de comprobación

El intervalo predeterminado es de un segundo. En **Inicio**, puede elegir entre 0,5 y 10 segundos en pasos de 0,5.

Un intervalo corto detecta mejor las conexiones breves, pero usa más recursos. Incluso con 0,5 segundos, una conexión que aparece y desaparece entre dos comprobaciones puede no detectarse.

Solo las reglas activadas generan registros o avisos.

## Qué ocurre después de una coincidencia

- **Registro silencioso:** escribe en el registro sin avisar.
- **Aviso de bandeja y registro:** cambia el icono de bandeja al estado de advertencia; abrir el registro de eventos elimina el aviso.
- **Alerta emergente y registro:** muestra una ventana para la primera coincidencia; las siguientes actualizan esa misma ventana.

Los números y las formas de Inicio y de la lista de eventos ayudan a distinguir las tres acciones.

## Ver eventos

Una misma conexión aparece como un solo registro, no como una fila nueva cada segundo.

- Una conexión existente muestra **Activa**.
- Una conexión terminada muestra **Finalizada**.
- **Duración observada** se actualiza mientras está activa y se detiene al finalizar.
- **Aplicación** muestra el nombre de producto del archivo cuando está disponible; si no, muestra el nombre del proceso.
- Haga doble clic en un registro para ver proceso, PID, ruta, procesos padre, servicios de Windows y otros detalles. También puede copiar el registro.

Una conexión se marca como finalizada después de estar ausente de la lista de Windows durante dos segundos. Si vuelve en esos dos segundos, sigue siendo el mismo registro; una aparición posterior crea uno nuevo.

La duración empieza cuando la aplicación ve la conexión por primera vez, por lo que puede no ser su duración real. Mientras la supervisión está detenida no se observa la conexión; al volver a iniciar se crea un registro nuevo.

## Cómo interpretar un registro

Una coincidencia solo significa que apareció una conexión que usted eligió vigilar. No demuestra que el equipo tenga malware.

Navegadores, proxies, VPN o componentes web pueden estar ejecutándose en segundo plano. La información del proceso puede ayudar a identificar una aplicación relacionada, pero no garantiza cuál provocó finalmente la conexión.

La lista de conexiones TCP no puede indicar de forma fiable qué lado inició la conexión. Los permisos de Windows también pueden impedir leer algunas rutas, datos de archivo, procesos padre o servicios.

Para decidir si existe un problema de seguridad, combine estos datos con un análisis antivirus o asesoramiento profesional.

## Centro de ayuda y actualizaciones

En **Ajustes**, seleccione **Abrir** junto al Centro de ayuda para leer la descripción del proyecto y la guía del usuario. Los documentos siguen el idioma de la interfaz.

Seleccione **Comprobar ahora** para consultar en GitHub una versión pública más reciente. La aplicación no descarga, instala ni ejecuta actualizaciones automáticamente.

En **Ajustes**, abra **Comentarios** para escribir una sugerencia o un problema. La aplicación abrirá en el navegador un Issue de GitHub rellenado. Revise el texto y envíelo usted mismo; los registros y las conexiones no se adjuntan por defecto.

## Registros, sonido y otros ajustes

Los registros se guardan en:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

El CSV se escribe al detectar una conexión y al terminar su observación, no cada segundo. El registro de eventos reúne la misma conexión en una sola fila.

**Limpiar vista** oculta filas del registro de eventos sin borrar los archivos CSV. Las filas antiguas siguen ocultas después de reiniciar; los eventos nuevos aparecen normalmente.

El límite predeterminado es de 25 MB. Puede cambiarlo a 5–500 MB en **Ajustes**. Se conservan hasta cinco archivos y se elimina el más antiguo al alcanzar el límite.

**Abrir la aplicación al iniciar sesión en Windows** solo abre la aplicación. **Iniciar la supervisión automáticamente al abrir** empieza a vigilar con las reglas activadas.

El sonido de alerta urgente se usa para las alertas emergentes. Puede ajustar su volumen en **Ajustes**; **Probar sonido** usa el mismo volumen y el volumen de Windows también se aplica.

## Privacidad, permisos y desinstalación

- No se necesitan permisos de administrador, cuenta ni contraseña.
- La aplicación no lee el contenido de los paquetes.
- No se suben reglas ni registros.
- GitHub solo se contacta al comprobar actualizaciones manualmente o abrir la página de comentarios.

Al desinstalar, los ajustes y registros se conservan por defecto. Si ya no los necesita, puede borrar manualmente:

```text
%LOCALAPPDATA%\ConnectionWatcher
```
