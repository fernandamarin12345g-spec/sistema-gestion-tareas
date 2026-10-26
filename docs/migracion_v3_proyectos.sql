-- =====================================================================
-- MIGRACIÓN V3 - Maestro de Proyectos
-- Fecha: 26 de octubre de 2026 (semana 11)
--
-- Qué hace:
--   1. Agrega una descripción opcional a cada proyecto.
--   2. Deja el estado de cada proyecto en uno de los valores permitidos:
--      Planificación, Activo, Pausado o Finalizado.
--
-- Se ejecuta UNA sola vez, después de migracion_v2_tareas.sql. No borra datos.
-- Nota: Tareas.id_proyecto tiene ON DELETE CASCADE. Para no perder tareas,
-- la API (ProyectosController) no permite eliminar un proyecto que tenga tareas.
-- =====================================================================

BEGIN TRANSACTION;

ALTER TABLE Proyectos ADD COLUMN descripcion TEXT;

UPDATE Proyectos
SET estado = 'Activo'
WHERE estado IS NULL OR estado NOT IN ('Planificación', 'Activo', 'Pausado', 'Finalizado');

COMMIT;
