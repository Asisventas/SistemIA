SET QUOTED_IDENTIFIER ON;
GO
UPDATE Productos SET 
    PrecioUnitarioGs = 150000,
    DuracionDiasMembresia = 30,
    ClasesIncluidasMembresia = -1,
    ColorMembresia = '#3498db',
    AccesoTodasAreasMembresia = 1,
    TipoPeriodoMembresia = 'mensual',
    DiasGraciaMembresia = 3
WHERE IdProducto = 20;
GO
