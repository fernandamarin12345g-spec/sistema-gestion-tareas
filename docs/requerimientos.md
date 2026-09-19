# Documento de Requerimientos y Casos de Uso 
**Proyecto:** Sistema de Gestión y Seguimiento de Tareas

## 1. Descripción General
El sistema permitirá centralizar y gestionar las actividades de un equipo de desarrollo de software. Además de la asignación de responsabilidades y seguimiento de estados, el sistema integrará un modelo de seguridad RBAC normalizado, auditoría de datos en tiempo real y visualización avanzada de inteligencia de negocios para el seguimiento de métricas clave.

## 2. Actores del Sistema (Modelo RBAC Dinámico y Normalizado)
La arquitectura de seguridad estará completamente desacoplada del código base, utilizando una estructura de base de datos en Tercera Forma Normal (3NF) para gestionar roles y permisos de manera granular.

*   **Administrador del Sistema (System Admin):** Acceso global. Gestiona el diccionario de roles, asigna permisos a nivel de base de datos y audita los registros de seguridad del sistema.
*   **Líder de Proyecto (Project Manager):** Supervisor operativo. Coordina al equipo, asigna tareas, y accede a matrices de visualización y dashboards de rentabilidad/avance de proyectos.
*   **Desarrollador (Miembro del equipo):** Ejecutor. Interfaz limpia y enfocada únicamente en su flujo de trabajo (tablero Kanban de sus tareas), con restricciones de borrado o modificación estructural.

## 3. Requerimientos Funcionales Innovadores

**Módulo de Seguridad (RBAC) y Auditoría**
*   **RF-01:** El sistema debe contar con una tabla maestra de Roles dinámicos y una tabla de Permisos granulares (ej. `crear_tarea`, `borrar_proyecto`).
*   **RF-02:** El sistema debe implementar un registro de auditoría (Audit Logging) que capture automáticamente el ID del usuario, la acción realizada, la fecha y la tabla modificada ante cualquier cambio de estado.
*   **RF-03:** El sistema debe restringir las consultas (Queries) basándose en el rol del usuario conectado, previniendo escalada de privilegios.

**Módulo de Gestión de Proyectos y Tareas**
*   **RF-04:** El sistema debe permitir la creación de tareas con relaciones a proyectos, asignando un responsable, fechas de inicio y fechas límite.
*   **RF-05:** **[Innovación]** El sistema debe permitir la clasificación mediante "Etiquetas Inteligentes" (Tags) para agrupar tareas por tecnología o departamento (ej. #Frontend, #BasesDeDatos).
*   **RF-06:** **[Innovación]** Automatización de estados: Si un desarrollador marca un avance del 100%, el sistema debe mover automáticamente el estado de la tarea a "En revisión" o "Terminada".

**Módulo de Business Intelligence (Dashboards)**
*   **RF-07:** El sistema proveerá un dashboard en tiempo real con modelado de datos para el Líder de Proyecto, mostrando métricas de eficiencia, tareas atrasadas y distribución de la carga laboral del equipo.

## 4. Casos de Uso Extendidos
1.  **Caso de Uso 1 - Configuración RBAC:** El Administrador ingresa al módulo de seguridad, crea un nuevo rol llamado "Auditor Externo", le asigna únicamente el permiso de "Solo Lectura" en la tabla de proyectos, y vincula el rol a un nuevo usuario.
2.  **Caso de Uso 2 - Trazabilidad de Errores (Audit Log):** Una tarea crítica desaparece. El Administrador consulta la tabla de auditoría y el sistema le muestra exactamente qué usuario ejecutó la acción "DELETE" y a qué hora lo hizo, manteniendo la integridad del equipo.
3.  **Caso de Uso 3 - Gestión Visual del PM:** El Líder de Proyecto abre su panel interactivo. A través de transformaciones de datos en tiempo real, visualiza una matriz que compara las horas estimadas vs. las horas reales de trabajo de sus desarrolladores, permitiéndole tomar decisiones sobre prioridades.
