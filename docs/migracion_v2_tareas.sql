-- =====================================================================
-- MIGRACIÓN V2 - Sistema de Gestión y Seguimiento de Tareas
-- Fecha: 26 de octubre de 2026 (semana 11)
--
-- Qué hace:
--   1. Estados: agrega "es_final" para saber qué estados cierran una tarea
--      (Terminada y Cancelada). Así la regla de "tarea atrasada" no depende
--      del nombre del estado, que es editable.
--   2. Tareas: agrega descripción, prioridad y fechas (creación, límite,
--      finalización y última actualización).
--   3. Completa los datos de las tareas que ya existían.
--
-- Se ejecuta UNA sola vez sobre la base existente. No borra datos.
-- Para una base nueva desde cero, usar esquema_base_datos.sql (ya incluye estos cambios).
-- =====================================================================

BEGIN TRANSACTION;

-- 1. ESTADOS: marcar los estados que cierran una tarea
ALTER TABLE Estados ADD COLUMN es_final INTEGER NOT NULL DEFAULT 0;

UPDATE Estados SET es_final = 1 WHERE nombre_estado IN ('Terminada', 'Cancelada');

-- 2. TAREAS: campos nuevos
ALTER TABLE Tareas ADD COLUMN descripcion TEXT;
ALTER TABLE Tareas ADD COLUMN prioridad TEXT NOT NULL DEFAULT 'Media'
    CHECK (prioridad IN ('Baja', 'Media', 'Alta', 'Crítica'));
ALTER TABLE Tareas ADD COLUMN fecha_creacion TEXT;        -- 'AAAA-MM-DD HH:MM:SS', la asigna el backend
ALTER TABLE Tareas ADD COLUMN fecha_limite TEXT;          -- 'AAAA-MM-DD', opcional
ALTER TABLE Tareas ADD COLUMN fecha_finalizacion TEXT;    -- se llena al pasar a un estado final
ALTER TABLE Tareas ADD COLUMN fecha_actualizacion TEXT;   -- se actualiza en cada edición

-- 3. DATOS DE LAS TAREAS EXISTENTES
UPDATE Tareas
SET fecha_creacion = datetime('now', 'localtime'),
    fecha_actualizacion = datetime('now', 'localtime')
WHERE fecha_creacion IS NULL;

UPDATE Tareas
SET fecha_finalizacion = datetime('now', 'localtime')
WHERE id_estado IN (SELECT id_estado FROM Estados WHERE es_final = 1);

-- El proyecto 1 es este mismo sistema, y las tareas sin proyecto pertenecen a él
UPDATE Proyectos SET nombre = 'Sistema de Gestión de Tareas' WHERE id_proyecto = 1;
UPDATE Tareas SET id_proyecto = 1 WHERE id_proyecto IS NULL;

COMMIT;
