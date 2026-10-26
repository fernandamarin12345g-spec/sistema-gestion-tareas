-- =====================================================================
-- ESQUEMA DE BASE DE DATOS (versión 3) - Sistema de Gestión y Seguimiento de Tareas
-- Motor: SQLite
-- Uso: crea la base completa desde cero, con datos iniciales.
--      Ejecutar sobre un archivo vacío Backend/gestion_tareas.sqlite
-- Para actualizar una base existente: versión 1 -> migracion_v2_tareas.sql -> migracion_v3_proyectos.sql
-- =====================================================================

PRAGMA foreign_keys = ON;

-- ---------------------------------------------------------------------
-- TABLAS MAESTRAS
-- ---------------------------------------------------------------------
CREATE TABLE Roles (
    id_rol INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre_rol VARCHAR(50) NOT NULL,
    is_admin BOOLEAN DEFAULT 0
);

CREATE TABLE Proyectos (
    id_proyecto INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre VARCHAR(100) NOT NULL,
    estado VARCHAR(50) DEFAULT 'Activo',       -- Planificación, Activo, Pausado o Finalizado (lo valida la API)
    descripcion TEXT
);

CREATE TABLE Estados (
    id_estado INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre_estado VARCHAR(50) NOT NULL UNIQUE,
    es_final INTEGER NOT NULL DEFAULT 0          -- 1 = el estado cierra la tarea (Terminada, Cancelada)
);

CREATE TABLE Etiqueta (
    id_etiqueta INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

-- ---------------------------------------------------------------------
-- USUARIOS
-- ---------------------------------------------------------------------
CREATE TABLE Usuarios (
    id_usuario INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre VARCHAR(100) NOT NULL,
    correo VARCHAR(100) UNIQUE NOT NULL,
    id_rol INTEGER REFERENCES Roles(id_rol)
);

-- ---------------------------------------------------------------------
-- TABLA CENTRAL: TAREAS
-- ---------------------------------------------------------------------
CREATE TABLE Tareas (
    id_tarea INTEGER PRIMARY KEY AUTOINCREMENT,
    titulo VARCHAR(200) NOT NULL,
    id_estado INTEGER REFERENCES Estados(id_estado) DEFAULT 1,
    id_proyecto INTEGER REFERENCES Proyectos(id_proyecto) ON DELETE CASCADE,
    id_usuario_responsable INTEGER REFERENCES Usuarios(id_usuario) ON DELETE SET NULL,
    descripcion TEXT,
    prioridad TEXT NOT NULL DEFAULT 'Media'
        CHECK (prioridad IN ('Baja', 'Media', 'Alta', 'Crítica')),
    fecha_creacion TEXT,         -- 'AAAA-MM-DD HH:MM:SS'
    fecha_limite TEXT,           -- 'AAAA-MM-DD'
    fecha_finalizacion TEXT,     -- se llena al pasar a un estado final
    fecha_actualizacion TEXT     -- se actualiza en cada edición
);

-- ---------------------------------------------------------------------
-- TABLAS DE RELACIÓN Y AUDITORÍA (reservadas para versiones futuras)
-- ---------------------------------------------------------------------
CREATE TABLE Tarea_Etiqueta (
    id_tarea INTEGER REFERENCES Tareas(id_tarea) ON DELETE CASCADE,
    id_etiqueta INTEGER REFERENCES Etiqueta(id_etiqueta) ON DELETE CASCADE,
    PRIMARY KEY (id_tarea, id_etiqueta)
);

CREATE TABLE Auditoria_Log (
    id_log INTEGER PRIMARY KEY AUTOINCREMENT,
    accion VARCHAR(50) NOT NULL,
    fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
    id_usuario INTEGER REFERENCES Usuarios(id_usuario) ON DELETE SET NULL
);

-- ---------------------------------------------------------------------
-- DATOS INICIALES
-- ---------------------------------------------------------------------
INSERT INTO Estados (nombre_estado, es_final) VALUES
('Pendiente', 0),
('En progreso', 0),
('Bloqueada', 0),
('En revisión', 0),
('Terminada', 1),
('Cancelada', 1);

INSERT INTO Roles (nombre_rol, is_admin) VALUES
('Administrador', 1),
('Líder de Proyecto', 0),
('Desarrollador', 0);

INSERT INTO Proyectos (nombre, estado, descripcion) VALUES
('Sistema de Gestión de Tareas', 'Activo', 'Proyecto transversal de la práctica: web, API, móvil y escritorio.'),
('Dashboard Comercial Power BI', 'Planificación', NULL);

INSERT INTO Usuarios (nombre, correo, id_rol) VALUES
('Luis Abreu Acevedo', 'luis@strategicsoft.com', 1),
('Fernanda Marin', 'fernandamarin12345g@gmail.com', 2);

INSERT INTO Tareas (titulo, descripcion, id_estado, id_proyecto, id_usuario_responsable, prioridad,
                    fecha_creacion, fecha_limite, fecha_finalizacion, fecha_actualizacion) VALUES
('Diseñar esquema de base de datos', 'Modelo entidad-relación y script SQL del sistema.', 4, 1, 2, 'Alta',
 datetime('now', 'localtime'), NULL, NULL, datetime('now', 'localtime')),
('Diseñar backend', 'API REST CRUD en ASP.NET Core con SQLite.', 5, 1, 1, 'Alta',
 datetime('now', 'localtime'), NULL, datetime('now', 'localtime'), datetime('now', 'localtime'));
