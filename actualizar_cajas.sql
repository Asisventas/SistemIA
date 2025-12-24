-- Actualizar cajas existentes con nombre e IdSucursal
-- Asignar nombre basado en el id_caja
UPDATE Cajas 
SET Nombre = 'Caja ' + CAST(id_caja AS VARCHAR(10))
WHERE Nombre IS NULL;

-- Asignar IdSucursal = 1 a todas las cajas que no tienen sucursal
-- (esto es temporal, luego se puede cambiar manualmente según corresponda)
UPDATE Cajas 
SET IdSucursal = 1
WHERE IdSucursal IS NULL;

-- Verificar el resultado
SELECT id_caja, Nombre, IdSucursal, CajaActual, Nivel1, Nivel2, Timbrado
FROM Cajas
ORDER BY id_caja;
