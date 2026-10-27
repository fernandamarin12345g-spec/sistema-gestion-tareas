-- =====================================================================
-- MIGRACIÓN V4 - Inicio de sesión (JWT) y permisos
-- Fecha: 26 de octubre de 2026 (semana 12)
--
-- Qué hace:
--   1. Usuarios: agrega contrasena_hash. La contraseña NUNCA se guarda en texto plano,
--      solo su hash PBKDF2-SHA256 con sal (ver Backend/Seguridad/ServicioContrasenas.cs).
--   2. Roles: agrega gestiona_tareas (1 = puede crear y editar tareas).
--      Administrador y Líder de Proyecto quedan con 1; Desarrollador con 0.
--   3. Asigna una contraseña inicial de DESARROLLO a los 2 usuarios existentes
--      (se deben cambiar desde la sección Usuarios).
--
-- Se ejecuta UNA sola vez, después de migracion_v3_proyectos.sql. No borra datos.
-- =====================================================================

BEGIN TRANSACTION;

ALTER TABLE Usuarios ADD COLUMN contrasena_hash TEXT;

ALTER TABLE Roles ADD COLUMN gestiona_tareas INTEGER NOT NULL DEFAULT 0;
UPDATE Roles SET gestiona_tareas = 1 WHERE is_admin = 1 OR nombre_rol = 'Líder de Proyecto';

UPDATE Usuarios SET contrasena_hash = 'pbkdf2-sha256$100000$cBbispryGIq09Lo2inYYwQ==$FtH6FgLIZYqMB6y7isXkjMRgMlmHB2ks/nbb6DBQlMU='
WHERE correo = 'luis@strategicsoft.com';

UPDATE Usuarios SET contrasena_hash = 'pbkdf2-sha256$100000$aXpq8KQxYBr2d0UXrojCIg==$pJGwnOtJQkIDHXhA47AHNmX6nlEbdqyjZoQ/XwsXYsQ='
WHERE correo = 'fernandamarin12345g@gmail.com';

COMMIT;
