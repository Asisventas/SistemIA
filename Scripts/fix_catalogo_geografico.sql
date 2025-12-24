SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Correcciones generadas automáticamente por /admin/verificar-catalogo-json
-- Revise antes de ejecutar en producción.

-- Central: SAN ANTONIO
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 141;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 420;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 646;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 838;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 892;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1224;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1622;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1642;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1694;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1698;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1704;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1785;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1905;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 1938;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 2052;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 2115;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 2199;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 2732;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 2789;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 2969;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 3234;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 3724;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 4032;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 4129;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 4348;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 4531;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 4682;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 4955;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 5120;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 5471;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 5579;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 5802;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 6000;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 6038;
UPDATE ciudad SET Departamento = 12, Distrito = 163 WHERE Numero = 6308;

-- Central: SAN LORENZO
UPDATE ciudad SET Departamento = 12, Distrito = 164 WHERE Numero = 142;
UPDATE ciudad SET Departamento = 12, Distrito = 164 WHERE Numero = 1370;
UPDATE ciudad SET Departamento = 12, Distrito = 164 WHERE Numero = 1983;
UPDATE ciudad SET Departamento = 12, Distrito = 164 WHERE Numero = 3840;
UPDATE ciudad SET Departamento = 12, Distrito = 164 WHERE Numero = 5241;

-- Central: YPANE
UPDATE ciudad SET Departamento = 12, Distrito = 168 WHERE Numero = 165;
UPDATE ciudad SET Departamento = 12, Distrito = 168 WHERE Numero = 960;
UPDATE ciudad SET Departamento = 12, Distrito = 168 WHERE Numero = 1255;
UPDATE ciudad SET Departamento = 12, Distrito = 168 WHERE Numero = 4486;

-- Concepción
UPDATE ciudad SET Departamento = 2, Distrito = 2 WHERE Numero = 978;
UPDATE ciudad SET Departamento = 2, Distrito = 2 WHERE Numero = 6265;

-- Cordillera / Caacupé
UPDATE ciudad SET Departamento = 4, Distrito = 26 WHERE Numero = 4703;

-- Caazapá
UPDATE ciudad SET Departamento = 7, Distrito = 77 WHERE Numero = 5129;

-- Central: Ita, Limpio
UPDATE ciudad SET Departamento = 12, Distrito = 156 WHERE Numero = 3082;
UPDATE ciudad SET Departamento = 12, Distrito = 158 WHERE Numero = 3166;

-- Amambay / Pedro Juan Caballero
UPDATE ciudad SET Departamento = 14, Distrito = 185 WHERE Numero = 2079;

-- Boquerón / Filadelfia
UPDATE ciudad SET Departamento = 16, Distrito = 259 WHERE Numero = 5521;

COMMIT;
