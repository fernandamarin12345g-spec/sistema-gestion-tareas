
--  TABLAS MAESTRAS

CREATE TABLE Roles (
    id_rol INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre_rol VARCHAR(50) NOT NULL,
    is_admin BOOLEAN DEFAULT 0
);

CREATE TABLE Proyectos (
    id_proyecto INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre VARCHAR(100) NOT NULL,
    estado VARCHAR(50) DEFAULT 'Activo'
);

CREATE TABLE Etiqueta (
    id_etiqueta INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre VARCHAR(50) NOT NULL UNIQUE
);


--  TABLA DE USUARIOS

CREATE TABLE Usuarios (
    id_usuario INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre VARCHAR(100) NOT NULL,
    correo VARCHAR(100) UNIQUE NOT NULL,
    id_rol INTEGER REFERENCES Roles(id_rol)
);


--  TABLA CENTRAL

CREATE TABLE Tareas (
    id_tarea INTEGER PRIMARY KEY AUTOINCREMENT,
    titulo VARCHAR(200) NOT NULL,
    estado VARCHAR(50) DEFAULT 'Pendiente',
    id_proyecto INTEGER REFERENCES Proyectos(id_proyecto) ON DELETE CASCADE,
    id_usuario_responsable INTEGER REFERENCES Usuarios(id_usuario) ON DELETE SET NULL
);


--  TABLAS DE RELACIÓN Y AUDITORÍA

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

-- 5. INSERCIÓN DE DATOS DE PRUEBA 

INSERT INTO Roles (nombre_rol, is_admin) VALUES
('Administrador', 1),
('Líder de Proyecto', 0),
('Desarrollador', 0);

INSERT INTO Proyectos (nombre, estado) VALUES
('Sistema de Gestión Documental', 'Activo'),
('Dashboard Comercial Power BI', 'Planificación');

INSERT INTO Etiqueta (nombre) VALUES
('Urgente'),
('Backend'),
('Frontend');

INSERT INTO Usuarios (nombre, correo, id_rol) VALUES
('Luis Abreu Acevedo', 'luis@strategicsoft.com', 1),
('Fernanda Marín', 'fernandamarin12345g@gmail.com', 3);

INSERT INTO Tareas (titulo, estado, id_proyecto, id_usuario_responsable) VALUES
('Diseñar esquema de Base de Datos', 'Completado', 1, 2);

INSERT INTO Tarea_Etiqueta (id_tarea, id_etiqueta) VALUES
(1, 2);


-- 6. CONSULTA DE PRUEBA 

SELECT 
    t.titulo AS "Nombre de la Tarea",
    p.nombre AS "Proyecto",
    u.nombre AS "Responsable",
    e.nombre AS "Etiqueta",
    t.estado AS "Estado Actual"
FROM Tareas t
JOIN Proyectos p ON t.id_proyecto = p.id_proyecto
JOIN Usuarios u ON t.id_usuario_responsable = u.id_usuario
JOIN Tarea_Etiqueta te ON t.id_tarea = te.id_tarea
JOIN Etiqueta e ON te.id_etiqueta = e.id_etiqueta;


--  ACTUALIZACIÓN Y BORRADO

UPDATE Tareas 
SET estado = 'En revisión' 
WHERE id_tarea = 1;

DELETE FROM Etiqueta 
WHERE id_etiqueta = 3;