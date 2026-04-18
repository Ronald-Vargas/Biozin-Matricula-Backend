-- Agregar columna es_virtual a la tabla cursos
ALTER TABLE cursos
ADD es_virtual BIT NOT NULL DEFAULT 0;
