-- Agregar columnas de aranceles a la tabla ajustes
ALTER TABLE ajustes
ADD montoMatricula DECIMAL(18,2) NULL DEFAULT 100000,
    montoInfraestructura DECIMAL(18,2) NULL DEFAULT 15000;
