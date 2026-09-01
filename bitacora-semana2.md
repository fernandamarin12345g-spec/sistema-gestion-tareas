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
AÚN NO ESTÁ TERMINADA ESTA BITÁCORA...