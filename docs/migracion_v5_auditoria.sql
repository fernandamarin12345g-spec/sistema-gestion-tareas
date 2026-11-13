-- =====================================================================
-- MIGRACIÓN V5 - Registro de auditoría
-- Fecha: 13 de noviembre de 2026 (semana 14)
--
-- Qué hace:
--   Completa la tabla Auditoria_Log (creada en la versión 1 y reservada hasta ahora)
--   para guardar quién hizo cada cambio, en qué módulo, sobre qué registro y el detalle.
--     modulo      -> Tareas, Proyectos, Estados, Roles, Usuarios o Auth (inicio de sesión)
--     id_registro -> id del registro afectado (por ejemplo, la tarea 24)
--     detalle     -> texto corto: título, nombre o "Prueba Docker -> En progreso"
--
-- NO hace falta ejecutarla a mano: la API la aplica sola al arrancar
-- (ServicioAuditoria.PrepararTabla revisa qué columnas faltan y las agrega).
-- Se deja como documentación del cambio en el modelo de datos.
-- =====================================================================

BEGIN TRANSACTION;

ALTER TABLE Auditoria_Log ADD COLUMN modulo VARCHAR(50);
ALTER TABLE Auditoria_Log ADD COLUMN id_registro INTEGER;
ALTER TABLE Auditoria_Log ADD COLUMN detalle TEXT;

COMMIT;

-- Consulta de ejemplo: los últimos 20 cambios con el nombre de quien los hizo
-- SELECT a.fecha_hora, u.nombre, a.accion, a.modulo, a.id_registro, a.detalle
-- FROM Auditoria_Log a LEFT JOIN Usuarios u ON a.id_usuario = u.id_usuario
-- ORDER BY a.id_log DESC LIMIT 20;
