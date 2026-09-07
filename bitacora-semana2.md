Qué aprendí? 

Desarrollé el mapa conceptual sobre los fundamentos de internet y el recorrido de una petición web usando Draw.io. Aprendí cómo interactúan las diferentes partes:

El navegador cliente hace una petición y el servidor DNS se encarga de traducir el dominio a una dirección IP numérica.
La comunicación viaja mediante el protocolo HTTP/HTTPS hacia un servidor ya sea un Hosting tradicional o un VPS.
Entendí la separación entre el frontend lo que ve el usuario y el Backend la lógica del sistema, y cómo se comunican a través de una API que envía y recibe datos, generalmente en formato JSON, consultando la base de datos.
También profundicé en el uso de la terminal de visual studio code con powerShell para navegar y crear archivos sin usar el mouse:
Usé mkdir para crear carpetas, cd para entrar a ellas y cd .. para retroceder.
Con el comando pwd aprendí a ver mi ruta actual y con ls a listar los archivos.
Creé archivos de texto directamente desde consola con ni (New-Item), les inserté texto con echo y leí su contenido con cat.
Hice pruebas reales de red usando el comando ping google.com, donde pude ver la dirección IP de Google y comprobar que mi latencia era de 27ms sin perder paquetes.



Sección 2 del curso visual studio code 
Atajos del teclado: en esta primera parte de la sección dos del curso me brindaron un documento con los atajos del teclado de visual: investigue un poco y encontré en todos lo que vi cuales son los mas importantes:

General
Ctrl+Shift+P (Paleta de comandos): Es mi centro de mando absoluto. Si olvido cualquier otro atajo o herramienta, presiono esto, escribo lo que necesito y lo ejecuto desde ahí.
Edición básica
Alt+ ↑ / ↓ (Mover línea): Me permite mover bloques de texto hacia arriba o abajo instantáneamente sin tener que seleccionarlos, cortarlos y pegarlos.
Shift+Alt + ↓ / ↑ (Copiar línea): Duplico líneas enteras en un segundo. Es excelente para crear listas o variables repetitivas rápidamente en mi código.
Ctrl+Enter (Insertar línea abajo): Creo un salto de línea limpio justo debajo de mí, sin importar si mi cursor está atrapado en medio de una palabra.
Navegación
Ctrl+P (Ir al archivo): Es mi buscador maestro. Me permite abrir cualquier archivo del proyecto escribiendo su nombre, ignorando por completo el panel de carpetas.
Ctrl+G (Ir a la línea): Es indispensable para solucionar mis problemas cuando la consola me indica que tengo un error en una línea numérica exacta.
Búsqueda y reemplazo
Ctrl+D (Añadir selección): Selecciono una palabra y presiono este atajo para atrapar la siguiente coincidencia idéntica, permitiéndome editar ambas al mismo tiempo.
Ctrl+H (Reemplazar): Abro la herramienta para buscar una palabra mal escrita y reemplazarla masivamente en todo mi archivo.
Multicursor y selección
Alt+Click (Insertar cursor): Doy clic en distintos lugares de la pantalla y puedo escribir o borrar texto en todos ellos en simultáneo.
Ctrl+L (Seleccionar línea): Sombreo toda la línea donde estoy posicionada con un solo toque, sin tener que arrastrar el cursor.
Edición avanzada de código
Shift+Alt+F (Formatear documento): Ordeno, indento y alineo todo mi código automáticamente para que cumpla con los estándares estéticos profesionales.
F2 (Renombrar símbolo): Cambio el nombre de una variable de forma segura; el editor rastreará y actualizará todas sus referencias para no romper mi programa.
Ctrl+Space (Sugerencias): Fuerzo al sistema a mostrarme las opciones de autocompletado de código si me desaparecen de la pantalla.
Gestión de archivos y editor
Ctrl+\ (Dividir editor): Parto mi pantalla en dos columnas para poder comparar o leer dos archivos a la vez.
Ctrl+Shift+T (Reabrir pestaña): Es mi salvavidas total. Si cierro una pestaña por accidente, con este atajo la revivo instantáneamente.
Interfaz y terminal integrada
Ctrl+B (Ocultar menú): Oculto mi árbol de archivos izquierdo para darme más espacio visual y concentrarme solo en el código.
Ctrl+` (Mostrar terminal): Subo y bajo mi consola instantáneamente. Es vital para intercalar rápidamente entre escribir mis archivos y ejecutar mis comandos de Git.

Ejercicio movimientos de líneas
 En este aprendí dos formas de organizar las líneas sin necesidad del mouse, tanto manual como mas automático.
En la forma manual se puede con Alt+ ↑ / ↓  seleccionó cada linea y las ordeno ya sea de manera ascendente o descendente una por una.
En la forma “automática” seleciono las líneas que quiero ordenar y luego  uso  Ctrl shift p, para abrir la paleta de comandos y luego escribo sort lines, y ahí ya escojo si las quiero ordenar de forma ascendente o descendente.

Comentar código 

en este caso aprendí como comentar bien un bloque de código, pero por lo general es con comando Ctrl +/, pero en mi tipo de teclado es Ctrl +},  así que busque en la paleta de comandos con Ctrl shift p  y escribí  toggle line coment  y ahí aparecía como podía ponerlo.
También esta el comando  shift + alt + a    que sirve para comentar solo una parte exactamente del código no un bloque completo.

Creacióm rápida de archivos 

Al intentar acceder a un archivo inexistente, podemos darle Ctrl+ click  automáticamente vidual me crea un nuevo archivo como tal, y ya luego aplicando el mismo comando me lleva directamente al archivo nuevo que visuaul me creo.

Definiciones 
Con el cursor encima de cada función podemos “ojear” que hace y para que sirve 
Pero con Ctrl + cursor  y así nos da una función mas específica al momento 
Pero si querermos saber donde esta exactamente la definición de la función 
Presionamos Ctrl + clic  y esto nos lleva directamente. Y con Alt + F12  nos deja ver donde esta alojada la función sin salir nos del archivo principal.
Adicionalmente con Ctrl + w  podemos cerrar esos archivos nuevos y si queremos abrirlos de nuevo presionamos Ctrl + shift + t .

Borrar líneas 
 Con  Ctrl + shift + k  borro la línea que desee poniendo el cursor sobre ella.
Pero si queremos borrar de nuestro código todas las variables con ese mismo nombre presionamos Ctrl + shift + l  y esto automatcamente pone un cursor sobre las variables con ese mismo nombre, y así usamos  Ctrl + shift + k  para borralas todas alm mismo tiempo. Y por ultimo presionamos esc  para eliminar los múltiples cursores anteriormente generados.
Deshacer y rehacer 
 Para deshacer usamos ctrl + z  y si queremos rehacer solo presionamos ctrl + shift + +z  e inmediatamente rehacemos lo que acabamos de eliminar   
 
Zen mode 
 Es un modo cero de dsitracciones que se actuva con ctrl + k z  y se desativa d la misma manera 
Terminal integrada 
 En el curso muestran que para abrir la teminal se usa ctrl + ´  pero accediendo desde ctrl+shift+p  y digitando terminal, me di cuenta que ha cambiado y ahora es ctrl + alt + o .

Manejo de tabs 
 Los mas útiles son:
 Objetivo:
      Abrir, reabrir, cerrar tabs, cambiar de tab
    
tips:


Ctrl + W            Cerrar tab
Ctrl + K  Ctrl + W  Cerrar todas
Ctrl + Shift + T    Reabrir anterior
Ctrl + TAB          Cambiar de tab

Tabulaciones 
Utlilizamos el  tab  y shitf+tab  para organizar mas facilemte las líneas de código.

Creación rápida de carpetas y archivos  	
 Nos vamos directo al carpeta le damos en new folder y dentro de esa mima “creación” creamos varios archivos al tiempo, creando una estructura de directorios  ejemplo: 
Seleccionamos new file dentro de este ponemos helpers/js/fetch.js 
En este caso no logre crearlo ya que siempre se me creaba helpers/js y fetch.js 
Intente varias veces que fueras las tres por separado pero no pude.



Qué problemas encontré?

Me costó un poco entender inicialmente cómo encadenar todos los conceptos técnicos DNS, API, JSON, VPS en un solo flujo visual continuo.
Al guardar el mapa conceptual en Draw.io, la plataforma ofrecía subirlo directamente a GitHub, lo cual iba a desincronizar mi repositorio local en visual studio code.
Memorizar la función exacta de los comandos nuevos en la terminal, especialmente las abreviaturas cortas.

Cómo los resolví?

Organicé los actores principales de izquierda a derecha en el diagrama y tracé el paso a paso lógico desde que el cliente ingresa la URL hasta que recibe los datos.
Cancelé el autoguardado en la nube de la aplicación, exporté el diagrama como imagen .png a mi computador y realicé el proceso manual de git add ., git commit y git push para subirlo yo misma de forma controlada.
Repetí los comandos en la consola, prestando atención a cómo cambiaba la ruta en la pantalla negra para guiarme sin perderme. 

Qué conceptos aún no domino?

Hasta el momento el recorrido de la petición web me quedó muy claro, aunque me gustaría profundizar más adelante en las configuraciones de infraestructura exactas que diferencian un Hosting tradicional de un VPS a la hora de desplegar aplicaciones.
      